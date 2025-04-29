using Entities;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;
using Utilities.Security;
using WebSocket4Net;

namespace Business
{
    public class SDO
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public delegate void DelegateCompletionReceive(DeviceMessage dm);
        public event DelegateCompletionReceive EvtCompletionReceive; //Lanza los mensajes COMPLETION recibidos de AlephDEV
        public delegate void DelegateEventReceive(DeviceMessage dm);
        public event DelegateEventReceive EvtEventReceive; //Lanzalos mensajes EVENT recibidos de AlephDEV
        public delegate void DelegateAckReceive(DeviceMessage dm);
        public event DelegateAckReceive EvtAckReceive; //Lanza los mensajes ACK recibidos de AlephDEV
        private Core Core;
        WebSocket Websocket;
        private Queue<DeviceMessage> QueueDeviceMsg;//Encola los mensajes de dispositivos que se enviaran a AlephDEV
        private Queue<List<Device>> QueueDeviceEvents;//Encola los mensajes de eventos de dispositivos
        private System.Timers.Timer TimerDeviceEvents; //Timer que maneja el encolado de eventos de dispositivos
        private System.Timers.Timer TimerQueueDeviceMsg;//Timer que maneja el encolado de mensajes de dispositivos
        private System.Timers.Timer TimerAlephDEVconnTimeOut;//Timer que maneja el time out de espera de respuesta de AlephDEV
        private System.Timers.Timer TimerReConn; //Timer para la reconexión de AlephDEV
        public System.Timers.Timer TimerAutoStartAlephDEV = null; //Timer para ejecución automática de AlephDEV
        public bool StateConnection = false;
        private int RetrySendCount = 0;
        private int RetrySendLimit = 400; //30ms x 300 = 12 seg
        private int RequestId = 0;
        private DeviceMessage DeviceMessageToSend = null;
        public Health SOH;
        public List<ErrorCodesTable> LstErrorCodesTables = new List<ErrorCodesTable>();
        private Hashtable BagFillLevelSent; //Banderas para enviar nivel de llenado de bolsa una sola vez. Se resetea con el shipout
        public DeviceConfigurations DevConf;
        public UserProfile_Type ShipoutUser = null;
        public List<CashUnit> CashUnits; //Lista de gavetas de efectivo.
        private X509Certificate2 Certificate;

        public SDO(Core _core)
        {
            bool ret = false;
            this.SOH = new Health();
            string fileName = $"{Const.appPath}Config\\DeviceConfigurations.xml";
            this.DevConf = DeviceConfigurations.GetObject(out ret, _core.AlephATMAppData.TerminalModel, _core.AlephATMAppData.DefaultCurrency);
            _core.PrintObjectProperties(this.DevConf);
            if (!Directory.Exists($"{Const.appPath}Terminal"))
                Directory.CreateDirectory($"{Const.appPath}Terminal");
            if (ret)
                this.InitializeDevicesObserver();
            this.Core = _core;
            this.ResetBagFillLevel();
            this.SOH.PropertyChanged += SOH_PropertyChanged;
            if (!ErrorCodesTable.GetErrorData(out this.LstErrorCodesTables))
                Log.Error("Get ErrorCodesTables failed");
            this.InitializeDevicesConnection();

            string certPath = $"{Const.appPath}AlephCERT.pfx";
            this.Certificate = Encryption.GetX509CertFromFile(certPath, Utils.HexToStr(Core.AlephATMAppData.KeyCertificate));
            Log.Info("--Init SDO success--");
        }

        #region Device Events and monitoring
        /// <summary>
        /// Habilita el envío de eventos de llenado de bolsa
        /// </summary>
        private void ResetBagFillLevel()
        {
            if(this.DevConf.BAGconfig.Enable)
            {
                this.BagFillLevelSent = new Hashtable() {
                     { Enums.DeviceStatus.CIM_BagFillLevel_0, true },
                     { Enums.DeviceStatus.CIM_BagFillLevel_50, true },
                     { Enums.DeviceStatus.CIM_BagFillLevel_75, true },
                     { Enums.DeviceStatus.CIM_BagFillLevel_90, true },
                     { Enums.DeviceStatus.CIM_CassetteFull, true } };
            }
        }

        /// <summary>
        /// Handler para manejar los cambios en el estado de los dispositivos y de la terminal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SOH_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                Log.Info($"------SOH_PropertyChangedEvent: {e.PropertyName}");
                if (e.PropertyName.Equals("InUseState"))//Verificador para nivel de llenado de bolsa (ejecuta por InUse y NotInsUse)
                {
                    object obj = this.GetPropValue(sender, e.PropertyName);
                    this.VerifyBagLevelFill(); //Verifico el nivel de llenado de bolsa
                }
                else if (e.PropertyName.Equals("Mode"))//Verificador para cambio de estado de la terminal
                {
                    object obj = this.GetPropValue(sender, e.PropertyName);
                    if (this.SOH.Mode == Const.TerminalMode.InSupervisor)
                    {
                        this.SendDeviceStatus(Enums.DeviceStatus.TER_InSupervisor);
                    }
                    if (this.SOH.Mode == Const.TerminalMode.InService)
                    {
                        this.SendDeviceStatus(Enums.DeviceStatus.TER_InService);
                    }
                }
                this.Update_SOH_File();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        internal void VerifyBagLevelFill()
        {
            try
            {
                if (this.DevConf.BAGconfig.Enable)
                {
                    if (this.Core.Counters != null)
                    {
                        int currentQuantity = this.Core.Counters.TotalDepositedNotes;
                        decimal val90 = Math.Round(Convert.ToDecimal(this.Core.Counters.LogicalFullBinThreshold * 0.90M), 0);
                        decimal val75 = Math.Round(Convert.ToDecimal(this.Core.Counters.LogicalFullBinThreshold * 0.75M), 0);
                        decimal val50 = Math.Round(Convert.ToDecimal(this.Core.Counters.LogicalFullBinThreshold * 0.50M), 0);
                        if (this.Core.Counters.TotalDepositedNotes == 0)
                            this.ChangeSuppliesBAG(Enums.DeviceStatus.CIM_BagFillLevel_0);
                        else if (this.Core.Counters.LogicalFullBin)
                            this.ChangeSuppliesBAG(Enums.DeviceStatus.CIM_CassetteFull);
                        else if (this.Core.Counters.TotalDepositedNotes >= val90)
                            this.ChangeSuppliesBAG(Enums.DeviceStatus.CIM_BagFillLevel_90);
                        else if (this.Core.Counters.TotalDepositedNotes >= val75)
                            this.ChangeSuppliesBAG(Enums.DeviceStatus.CIM_BagFillLevel_75);
                        else if (this.Core.Counters.TotalDepositedNotes >= val50)
                            this.ChangeSuppliesBAG(Enums.DeviceStatus.CIM_BagFillLevel_50);
                    }
                    else
                        Log.Warn("Counters is null");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ChangeSuppliesBAG(Enums.DeviceStatus deviceStatus)
        {
            if (this.BagFillLevelSent.ContainsKey(deviceStatus))
            {
                if ((bool)this.BagFillLevelSent[deviceStatus])//Si ya se envió una vez, no lo vuelvo a enviar
                {
                    SDO_DeviceState CIM_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.CashAcceptor);
                    if (CIM_State != null)
                    {
                        CIM_State.InternalCode = deviceStatus;
                        if (CIM_State.Supplies == Const.Supplies.NewState)
                            CIM_State.Supplies = Const.Supplies.NoNewState;
                        else
                            CIM_State.Supplies = Const.Supplies.NewState;
                    }
                    else
                        Log.Fatal($"Device CIM fitness is null");
                    this.BagFillLevelSent[deviceStatus] = false;
                    this.SOH.BagFillLevel = deviceStatus;
                    this.Update_SOH_File();
                }
            }
            else
                Log.Error($"HashTable Error type not found: {deviceStatus}");
        }

        private void ChangeDEV_Supplies(Enums.Devices device, Const.Supplies supplies, Enums.DeviceStatus internalCode)
        {
            SDO_DeviceState CIM_State = this.Core.Sdo.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == device);
            if (CIM_State != null)
            {
                CIM_State.InternalCode = internalCode;
                CIM_State.Supplies = supplies;
            }
            else
                Log.Fatal($"Device {device} is null");
        }

        private void ChangeDEV_Fitness(Enums.Devices device, Const.Fitness fitness, Enums.DeviceStatus internalCode)
        {
            SDO_DeviceState DEV_State = this.Core.Sdo.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == device);
            if (DEV_State != null)
            {
                DEV_State.InternalCode = internalCode;
                DEV_State.Fitness = fitness;
            }
            else
                Log.Fatal($"Device {device} is null");
        }

        private void Sdo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SDO_DeviceState sDO_DeviceState = (SDO_DeviceState)sender;
            string[] elements = e.PropertyName.Split('.');
            string newValue = string.Empty, internalCodeFitness = string.Empty, internalCodeSupply = string.Empty; 
            if (elements.Length == 2)
            {
                switch (elements[0])
                {
                    case "CashAcceptor":
                    case "CashDispenser":
                    case "CoinDispenser":
                    case "Printer":
                        switch (elements[1])
                        {
                            case "Supplies":
                                newValue = sDO_DeviceState.Supplies.ToString();
                                internalCodeSupply = sDO_DeviceState.InternalCode.ToString();
                                this.SendDeviceStatus(sDO_DeviceState.InternalCode);
                                break;
                            case "Fitness":
                                newValue = sDO_DeviceState.Fitness.ToString();
                                internalCodeFitness = sDO_DeviceState.InternalCode.ToString();
                                this.SendDeviceStatus(sDO_DeviceState.InternalCode);
                                break;
                        }
                        break;
                    case "IOBoard":
                        switch (elements[1])
                        {
                            case "Fitness":
                                newValue = sDO_DeviceState.Fitness.ToString();
                                this.SendDeviceStatus(sDO_DeviceState.InternalCode);
                                break;
                        }
                        break;
                }
            }
            var internalCode = elements[1].Equals("Fitness") ? internalCodeFitness : internalCodeSupply;
            Log.Info($"Device: {elements[0]} Property: {elements[1]} NewValue: {newValue}" + (string.IsNullOrEmpty(internalCode) ? "": $" InternalCode: {internalCode}"));
            this.Update_SOH_File();
        }

        /// <summary>
        /// Create a list of observable objects to handle the "state of heath" of the devices
        /// </summary>
        private void InitializeDevicesObserver()
        {
            this.SOH.SDO_DevicesState = new List<SDO_DeviceState>();
            if (this.DevConf.CIMconfig != null && this.DevConf.CIMconfig.Enable)
            {
                SDO_DeviceState sDO_CIM_State = LoadDeviceState(Enums.Devices.CashAcceptor);
                sDO_CIM_State.PropertyChanged += Sdo_PropertyChanged;
                this.SOH.SDO_DevicesState.Add(sDO_CIM_State);
                Log.Info("CIM subscribed to device monitoring");
            }
            else
                Log.Warn("CIM does not subscribed to device monitoring");
            if (this.DevConf.CDMconfig != null && this.DevConf.CDMconfig.Enable)
            {
                SDO_DeviceState sDO_CDM_State = LoadDeviceState(Enums.Devices.CashDispenser);
                sDO_CDM_State.PropertyChanged += Sdo_PropertyChanged;
                this.SOH.SDO_DevicesState.Add(sDO_CDM_State);
                Log.Info("CDM subscribed to device monitoring");
            }
            else
                Log.Warn("CDM does not subscribed to device monitoring");
            if (this.DevConf.PRTconfig != null && this.DevConf.PRTconfig.Enable)
            {
                SDO_DeviceState sDO_PRT_State = LoadDeviceState(Enums.Devices.Printer);
                sDO_PRT_State.PropertyChanged += Sdo_PropertyChanged;
                this.SOH.SDO_DevicesState.Add(sDO_PRT_State);
                Log.Info("PTR subscribed to device monitoring");
            }
            else
                Log.Warn("PTR does not subscribed to device monitoring");
            if (this.DevConf.IOBoardConfig != null && this.DevConf.IOBoardConfig.Enable)
            {
                SDO_DeviceState sDO_SIU_State = LoadDeviceState(Enums.Devices.IOBoard);
                sDO_SIU_State.PropertyChanged += Sdo_PropertyChanged;
                this.SOH.SDO_DevicesState.Add(sDO_SIU_State);
                Log.Info("IOB subscribed to device monitoring");
            }
            else
                Log.Warn("IOB does not subscribed to device monitoring");
            if (this.DevConf.BAGconfig != null && this.DevConf.BAGconfig.Enable)
            {
                SDO_DeviceState sDO_BAG_State = new SDO_DeviceState(Enums.Devices.Bag, Const.Supplies.GoodState, Const.Fitness.NoError, Enums.DeviceStatus.CIM_DeviceSuccess);
                //sDO_BAG_State.PropertyChanged += Sdo_PropertyChanged; Por ahora no notifica errores
                this.SOH.SDO_DevicesState.Add(sDO_BAG_State);
                Log.Info("BAG subscribed to device monitoring");
            }
            else
                Log.Warn("BAG does not subscribed to device monitoring");
            if (this.DevConf.COINconfig != null && this.DevConf.COINconfig.Enable)
            {
                SDO_DeviceState sDO_COIN_State = LoadDeviceState(Enums.Devices.CoinDispenser);
                sDO_COIN_State.PropertyChanged += Sdo_PropertyChanged;
                this.SOH.SDO_DevicesState.Add(sDO_COIN_State);
                Log.Info("COIN subscribed to device monitoring");
            }
            else
                Log.Warn("COIN does not subscribed to device monitoring");
        }

        private SDO_DeviceState LoadDeviceState(Enums.Devices device)
        {
            var file = new Entities.IniFile($"{Const.appPath}Terminal\\StateOfHealth.txt");
            var devName = new Dictionary<Enums.Devices, string>
            {
                { Enums.Devices.CashAcceptor, "CASH_ACCEPTOR" },
                { Enums.Devices.CashDispenser, "CASH_DISPENSER" },
                { Enums.Devices.CoinDispenser, "COIN_DISPENSER" },
                { Enums.Devices.IOBoard, "IOBOARD" },
                { Enums.Devices.Printer, "RECEIPT_PRINTER" },
            };
            var defaultDeviceStatus = new Dictionary<Enums.Devices, Enums.DeviceStatus>
            {
                { Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_DeviceSuccess },
                { Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_DeviceSuccess },
                { Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_DeviceSuccess },
                { Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_DeviceSuccess },
                { Enums.Devices.Printer, Enums.DeviceStatus.PRT_DeviceSuccess },
                { Enums.Devices.Bag, Enums.DeviceStatus.CIM_DeviceSuccess }
            };
            if (!devName.ContainsKey(device))
            {
                return new SDO_DeviceState(Enums.Devices.CoinDispenser, Const.Supplies.GoodState, Const.Fitness.NoError, defaultDeviceStatus.ContainsKey(device) ? defaultDeviceStatus[device] : Enums.DeviceStatus.UNK_Undefined);
            }
            var section = devName[device];
            var fitnessStr = file.Read("Fitness", section);
            var suppliesStr = file.Read("Supplies", section);
            if (!Enum.TryParse(fitnessStr, out Const.Fitness fitness)) fitness = Const.Fitness.NoError;
            if (!Enum.TryParse(suppliesStr, out Const.Supplies supplies)) supplies = Const.Supplies.GoodState;
            return new SDO_DeviceState(device, supplies, fitness, Enums.DeviceStatus.UNK_Undefined);
        }

        private void Update_SOH_File()
        {
            string result = string.Empty;
            StringBuilder sb = new StringBuilder();
            try
            {
                SDO_DeviceState CIM_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.CashAcceptor);
                SDO_DeviceState CDM_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.CashDispenser);
                SDO_DeviceState COIN_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.CoinDispenser);
                SDO_DeviceState IOB_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.IOBoard);
                SDO_DeviceState PRT_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.Printer);
                SDO_DeviceState BAG_State = this.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.Bag);
                sb.AppendLine($"[TERMINAL_STATE]");
                sb.AppendLine($"Line={this.SOH.Line.ToString()}");
                sb.AppendLine($"Mode={this.SOH.Mode.ToString()}");
                sb.AppendLine($"InUse={this.SOH.InUseState.ToString()}");
                sb.AppendLine($"[DEVICES_STATE]");
                if (CIM_State != null)
                {
                    sb.AppendLine($"[CASH_ACCEPTOR]");
                    sb.AppendLine($"Fitness={CIM_State.Fitness.ToString()}");
                    sb.AppendLine($"Supplies={CIM_State.Supplies.ToString()}");
                    sb.AppendLine($"InternalCode={CIM_State.InternalCode}");
                    if (CIM_State.Details != null && CIM_State.Details.Any())
                        sb.AppendLine($"Details={string.Join(",", CIM_State.Details)}");
                }
                if (BAG_State != null)
                {
                    sb.AppendLine($"[BAG]");
                    sb.AppendLine($"BagFillLevel={this.SOH.BagFillLevel.ToString()}");
                }
                if (CDM_State != null)
                {
                    sb.AppendLine($"[CASH_DISPENSER]");
                    sb.AppendLine($"Fitness={CDM_State.Fitness.ToString()}");
                    sb.AppendLine($"Supplies={CDM_State.Supplies.ToString()}");
                    sb.AppendLine($"InternalCode={CDM_State.InternalCode}");
                    if (CDM_State.Details != null && CDM_State.Details.Any())
                        sb.AppendLine($"Details={string.Join(",", CDM_State.Details)}");
                }
                if(COIN_State != null)
                {
                    sb.AppendLine($"[COIN_DISPENSER]");
                    sb.AppendLine($"Fitness={COIN_State.Fitness.ToString()}");
                    sb.AppendLine($"Supplies={COIN_State.Supplies.ToString()}");
                    sb.AppendLine($"InternalCode={COIN_State.InternalCode}");
                    if (COIN_State.Details != null && COIN_State.Details.Any())
                        sb.AppendLine($"Details={string.Join(",", COIN_State.Details)}");
                }
                if (IOB_State != null && this.SOH.SensorsState != null)
                {
                    sb.AppendLine($"[IOBOARD]");
                    sb.AppendLine($"Fitness={IOB_State.Fitness.ToString()}");
                    sb.AppendLine($"Supplies={IOB_State.Supplies.ToString()}");
                    sb.AppendLine($"CoverSensor={this.SOH.SensorsState.Cover.ToString()}");
                    sb.AppendLine($"DoorSensor={this.SOH.SensorsState.Door.ToString()}");
                    sb.AppendLine($"LockSensor={this.SOH.SensorsState.Lock.ToString()}");
                    sb.AppendLine($"PresenceSensor={this.SOH.SensorsState.Presence.ToString()}");
                    sb.AppendLine($"UpperDoorSensor={this.SOH.SensorsState.UpperDoor.ToString()}");
                    if (IOB_State.Details != null && IOB_State.Details.Any())
                        sb.AppendLine($"Details={string.Join(",", IOB_State.Details)}");
                }
                if (PRT_State != null)
                {
                    sb.AppendLine($"[RECEIPT_PRINTER]");
                    sb.AppendLine($"Fitness={PRT_State.Fitness.ToString()}");
                    sb.AppendLine($"Supplies={PRT_State.Supplies.ToString()}");
                }

                Utils.WriteUTF8FileStream($"{Const.appPath}Terminal\\StateOfHealth.txt", sb.ToString());//Pisa el archivo existente
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion Device Events

        #region "To AlephDEV"

        #region "CIM"
        private void BuildCIM_DeviceFunctionMessage(Enums.Commands function, object payload)
        {
            DeviceMessage dm = new DeviceMessage(Enums.Devices.CashAcceptor, function, 0, payload);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        //Abre el dispositivo CIM
        public void CIM_AsyncOpen()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Open, cd);
        }

        public void CIM_Open()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Open, cd);
        }

        //Cierra el dispositivo CIM
        public void CIM_AsyncClose()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Close, cd);
        }

        public void CIM_Close()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Close, cd);
        }

        //Configura las denominaciones aceptadas por el CIM
        public void CIM_ConfigureNoteTypes(string inputData)
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, inputData);
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.ConfigureNoteTypes, cd);
        }

        //Obtiene las configuración del template de billetes
        public void CIM_GetBankNoteTypes()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.GetBankNoteTypes, cd);
        }

        //Inicia el proceso de recolección de billetes en escrow
        public void CIM_AsyncCashInStart()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CashInStart, cd);
        }

        public void CIM_CashInStart()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CashInStart, cd);
        }

        //Abre la compuerta para ingresar billetes
        public void CIM_AsyncCashIn()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CashIn, cd);
        }

        public void CIM_CashIn()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CashIn, cd);
        }

        //Cancela todas las ejecuciones asincrónicas que esten en curso
        public void CIM_AsyncCancel()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Cancel, cd);
        }

        //Deposita en las gavetas los billetes retenidos en escrow
        public void CIM_AsyncCashInEnd()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CashInEnd, cd);
        }

        public void CIM_CashInEnd()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CashInEnd, cd);
        }

        //Devuleve al cliente los billetes retenidos en escrow
        public void CIM_AsyncRollBack()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.RollBack, cd);
        }

        public void CIM_RollBack()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.RollBack, cd);
        }

        //Retiene los billetes que fueron presentados
        public void CIM_AsyncRetract()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Retract, cd);
        }

        public void CIM_Retract()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Retract, cd);
        }

        /// <summary>
        /// Solicita un Status del CIM y lo carga en una variable global.
        /// 0 - status ok 
        /// 1 - status warning 
        /// 2 - status error 
        /// 3 - status missing
        /// </summary>
        public void CIM_Status()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Status, cd);
        }

        //Abre la compuerta de escrow
        public void CIM_OpenEscrowShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.OpenEscrowShutter, cd);
        }

        //Cierra la compuerta de escrow
        public void CIM_CloseEscrowShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CloseEscrowShutter, cd);
        }

        //Abre la compuerta de bóveda
        public void CIM_OpenRetractShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.OpenRetractShutter, cd);
        }

        //Cierra la compuerta de bóveda
        public void CIM_CloseRetractShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CloseRetractShutter, cd);
        }

        //Abre la compuerta de compartimiento de billetes rechazados
        public void CIM_OpenRejectShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.OpenRejectShutter, cd);
        }

        //Cierra la compuerta de compartimiento de billetes rechazados
        public void CIM_CloseRejectShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CloseRejectShutter, cd);
        }

        //Abre la compuerta de ingreso de billetes
        public void CIM_OpenInputShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.OpenInputShutter, cd);
        }

        //Cierra la compuerta de ingreso de billetes
        public void CIM_CloseInputShutter()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.CloseInputShutter, cd);
        }

        //Perform a reset CIM
        public void CIM_AsyncReset()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Reset, cd);
        }

        //Obtiene los contadores del CIM
        public void CIM_GetCounters()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.GetCounters, cd);
        }

        //Borra los contadores del CIM
        public void CIM_ClearCounters()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.ClearCounters, cd);
        }

        //Inicializa el CIM
        public void CIM_Init()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCIM_DeviceFunctionMessage(Enums.Commands.Init, cd);
        }
        #endregion "CIM"

        #region "CDM"
        private void BuildCDM_DeviceFunctionMessage(Enums.Commands function, object payload)
        {
            DeviceMessage dm = new DeviceMessage(Enums.Devices.CashDispenser, function, 0, payload);
            this.QueueDeviceDataToAlephDEV(dm);
        }


        //Cancela todas las ejecuciones asincrónicas que esten en curso
        public void CDM_AsyncCancel()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Cancel, cd);
        }

        //Abre el dispositivo CIM
        public void CDM_AsyncOpen()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Open, cd);
        }

        public void CDM_Open()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Open, cd);
        }

        public void CDM_AsyncCashUnitInfo()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.CashUnitInfo, cd);
        }
        public void CDM_CashUnitInfo()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.CashUnitInfo, cd);
        }

        public void CDM_AsyncDispense(int cdmAmount, int cdmCount, string currency)
        {
            var valores = new DenominationCDM
            {
                CurrencyID = currency,
                Count = cdmCount,
                Amount = cdmAmount
            };
            string data = Utils.NewtonsoftSerialize(valores);
            CommandData cd = new CommandData(ExecutionEnumType.Async, data, MessageSecurizer.SignMessage(data, Certificate));
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Dispense, cd);
        }

        public void CDM_Dispense(int cdmAmount, int cdmCount, string currency)
        {
            var valores = new Entities.DenominationCDM();
            valores.CurrencyID = currency;
            valores.Count = cdmCount;
            valores.Amount = cdmAmount;

            string data = Utils.NewtonsoftSerialize(valores);
            CommandData cd = new CommandData(ExecutionEnumType.Sync, data, MessageSecurizer.SignMessage(data, Certificate));
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Dispense, cd);
        }

        public void CDM_Status()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Status, cd);
        }

        public void CDM_AsyncPresent()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, string.Empty, MessageSecurizer.SignMessage(string.Empty, Certificate));
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Present, cd);
        }

        public void CDM_Present()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, string.Empty, MessageSecurizer.SignMessage(string.Empty, Certificate));
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Present, cd);
        }

        public void CDM_AsyncReject()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, string.Empty, MessageSecurizer.SignMessage(string.Empty, Certificate));
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Reject, cd);
        }

        public void CDM_Reject()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, string.Empty, MessageSecurizer.SignMessage(string.Empty, Certificate));
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Reject, cd);
        }

        public void CDM_AsyncRetract()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Retract, cd);
        }

        public void CDM_AsyncReset()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Reset, cd);
        }

        public void CDM_Reset()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Reset, cd);
        }

        public void CDM_Retract()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Retract, cd);
        }


        public void CDM_AsyncClose()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Async, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Close, cd);
        }

        public void CDM_Close()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCDM_DeviceFunctionMessage(Enums.Commands.Close, cd);
        }
        #endregion "CDM"

        #region COIN
        private void BuildCOIN_DeviceFunctionMessage(Enums.Commands function, object payload)
        {
            DeviceMessage dm = new DeviceMessage(Enums.Devices.CoinDispenser, function, 0, payload);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        private void COIN_SendCommand(Enums.Commands command, DenominationCOIN denominations, bool includeSignature = false)
        {
            string commandData = Utilities.Utils.NewtonsoftSerialize(denominations);
            CommandData cd = new CommandData(ExecutionEnumType.Sync, commandData);
            if (includeSignature)
            {
                cd = new CommandData(ExecutionEnumType.Sync, commandData, MessageSecurizer.SignMessage(commandData, Certificate));
            }
            BuildCOIN_DeviceFunctionMessage(command, cd);
        }
        public void COIN_Open()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCOIN_DeviceFunctionMessage(Enums.Commands.Open, cd);
        }
        public void COIN_Close()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            this.BuildCOIN_DeviceFunctionMessage(Enums.Commands.Close, cd);
        }
        public void COIN_GetSerial(DenominationCOIN denominations)
        {
            COIN_SendCommand(Enums.Commands.GetSerial, denominations);
        }
        public void COIN_GetStatus()
        {
            COIN_SendCommand(Enums.Commands.Status, new DenominationCOIN());
        }
        public void COIN_GetState(DenominationCOIN denominations)
        {
            COIN_SendCommand(Enums.Commands.State, denominations);
        }
        public void COIN_GetCounter(DenominationCOIN denominations)
        {
            COIN_SendCommand(Enums.Commands.GetCounters, denominations);
        }
        public void COIN_EnableHopper(DenominationCOIN denominations)
        {
            COIN_SendCommand(Enums.Commands.Enable, denominations);
        }
        public void COIN_Dispense(DenominationCOIN denominations)
        {
            COIN_SendCommand(Enums.Commands.Dispense, denominations, true);
        }

        #endregion

        #region "ADM"
        public void ADM_Open()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Open, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        public void ADM_Home()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Home, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        public void ADM_Status()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Status, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void ADM_Reset()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Reset, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void ADM_Capabilities()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Capabilities, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void ADM_Dispense(string position)
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, position);
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Dispense, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void ADM_Load(string position)
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, position);
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Load, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void ADM_Close()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.ADM, Enums.Commands.Close, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        #endregion "ADM"

        #region "PRT"
        // /// /// STM /// /// //
        //public void STM_PrintWindows(string printData)
        //{
        //    CommandData cd = new CommandData(ExecutionEnumType.Sync, printData);
        //    DeviceMessage dm = new DeviceMessage(Enums.Devices.StatementPrinter, Enums.Commands.PrintRawData, 0, cd);
        //    this.QueueDeviceDataToAlephDEV(dm);
        //}

        //public void STM_GetState()
        //{
        //    CommandData cd = new CommandData(ExecutionEnumType.Full, "");
        //    DeviceMessage dm = new DeviceMessage(Enums.Devices.StatementPrinter, Enums.Commands.State, 0, cd);
        //    this.QueueDeviceDataToAlephDEV(dm);
        //}

        // /// /// PRT /// /// //
        public void PTR_PrintRawData(string printData)
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, printData);
            DeviceMessage dm = new DeviceMessage(Enums.Devices.Printer, Enums.Commands.PrintRawData, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void PTR_GetStatus()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Full, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.Printer, Enums.Commands.Status, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void PTR_GetState()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Full, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.Printer, Enums.Commands.State, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        #endregion "PRT"

        #region "BAR"
        public void BAR_StartScanBarcode()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.BarcodeReader, Enums.Commands.StartScan, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void BAR_StopScanBarcode()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.BarcodeReader, Enums.Commands.StopScan, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        #endregion "BAR"

        #region "FPM"
        public void FPM_StartFingerPrintCapture()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.FingerPrintReader, Enums.Commands.StartScan, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void FPM_StopFingerPrintCapture()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.FingerPrintReader, Enums.Commands.StopScan, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }
        #endregion "FPM"

        // /// /// CAM /// /// //
        public void CAM_StartTakePic()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.Camera, Enums.Commands.TakePic, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        // /// /// IDC /// /// //
        public void IDC_StartCardReader()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.CardReader, Enums.Commands.StartScan, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        // /// /// IOBoard /// /// //
        //public void SensorsAndIndicators_GetState()
        //{
        //    CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
        //    DeviceMessage dm = new DeviceMessage(Enums.Devices.SensorsAndIndicators, Enums.Commands.State, 0, cd);
        //    this.QueueDeviceDataToAlephDEV(dm);
        //}

        public void IOBoard_GetState()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.IOBoard, Enums.Commands.State, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        public void AIO_StartShaker()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.IOBoard, Enums.Commands.StartShaker, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        //Cierra el SP del CIM
        public void AD_Close()
        {
            this.KillAlephDEV();
        }

        //Verifica inicialización de los dispositivos
        public void TER_InitVerify()
        {
            CommandData cd = new CommandData(ExecutionEnumType.Sync, "");
            DeviceMessage dm = new DeviceMessage(Enums.Devices.Terminal, Enums.Commands.State, 0, cd);
            this.QueueDeviceDataToAlephDEV(dm);
        }

        #endregion "To AlephDEV"

        #region "From AlephDEV"
        /// <summary>
        /// Procesa los retornos de los dispositivos manejados por AlephDEV
        /// </summary>
        /// <param name="message"></param>
        private void ProcessAlephDEVmessage(string message)
        {
            DeviceMessage dm = null;
            bool ret = false;
            try
            {
                if (message.Length > 3)
                {
                    dm = new DeviceMessage(message, out ret);
                    if (dm != null && ret)
                    {
                        switch (dm.Header.Type)
                        {
                            case Types.Event:
                                this.RaiseEvtEventReceive(dm);
                                if (dm.Device == Enums.Devices.IOBoard && this.DevConf.IOBoardConfig.Enable)
                                {
                                    this.ProcessIOBoardEvent(dm);
                                }
                                break;
                            case Types.Completion:
                                if (this.DeviceMessageToSend != null)
                                {
                                    this.TimerAlephDEVconnTimeOut.Enabled = false;//detengo el timer de respuesta automática
                                    this.RaiseEvtCompletionReceive(dm);//lanzo envento con mensaje recibido
                                }
                                else
                                    Log.Error("TimeOut of Completion message.");
                                //Manejo de errores AIO
                                if (dm.Payload is Completion)
                                {
                                    //Actualizo el estado de sensores
                                    if (dm.Device == Enums.Devices.IOBoard && dm.Command == Enums.Commands.State && this.DevConf.IOBoardConfig.Enable)
                                        this.ProcessIOBoardEvent(dm);
                                }
                                else
                                    Log.Error("Completion error.");
                                break;
                            case Types.Unsolicited:
                                if (dm.Device == Enums.Devices.Terminal)
                                {
                                    switch (dm.Command)
                                    {
                                        case Enums.Commands.ShipOut:
                                            this.ExecuteShipOut(shipOutLogic: true, dm.Payload.ToString());
                                            this.ExecuteShipIn(shipInLogic: true);
                                            break;
                                        case Enums.Commands.ShipIn:
                                            this.ExecuteShipIn(shipInLogic: true);
                                            break;
                                        case Enums.Commands.GoToSupervisor:
                                            this.Core.StartSupervisorMode(logicSupervisorEntry: true);
                                            break;
                                        case Enums.Commands.OutOfSupervisor:
                                            this.RaiseEvtEventReceive(dm);
                                            break;
                                    }
                                }
                                break;
                            case Types.Acknowledge:
                                this.RaiseEvtAckReceive(dm);
                                break;
                        }
                    }
                    else
                        Log.Warn($"Wrong json format: {message}");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public bool SendDeviceStatus(Enums.DeviceStatus errorType)
        {
            bool ret = false;
            List<Device> lstDevices = new List<Device>();
            try
            {
                ErrorCodesTable errorCode = this.LstErrorCodesTables.Find(x => x.InternalCode == errorType);
                if (errorCode != null)
                {
                    lstDevices.Add(new Device(errorCode.DeviceName, errorCode.ExternalCode, errorCode.ExternalMessage));
                    if (errorCode.SendError)
                    {
                        if (this.QueueDeviceEvents.Count < 2000)
                        {
                            this.QueueDeviceEvents.Enqueue(lstDevices);
                            ret = true;
                        }
                    }
                    else
                    {
                        ret = true;
                        Log.Info($"Send error code {errorCode.InternalCode.ToString()}: disabled");
                    }
                }
                else
                    Log.Error($"Error code not found: {errorCode}");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        private void TimerSendDeviceEvent_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            List<Device> lstDevices;
            try
            {
                this.TimerDeviceEvents.Stop();
                this.TimerDeviceEvents.Interval = 1000; //Se cambia la frecuencia de envío a 1 seg
                if (this.QueueDeviceEvents.Count != 0 && this.SOH.InUseState == Const.InUseMode.NotInUse && this.Core.Sdo.SOH.Mode != Const.TerminalMode.InShipout) //this.Core.Sdo.SOH.Mode != Const.TerminalMode.InSupervisor &&
                {
                    lstDevices = this.QueueDeviceEvents.Dequeue();
                    Thread thd;
                    thd = new Thread(new ParameterizedThreadStart(this.Send_StatusMsg));
                    thd.Start(lstDevices);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            finally { this.TimerDeviceEvents.Start(); }
        }

        private void Send_StatusMsg(object param)
        {
            try
            {
                Log.Debug("/--->");
                List<Device> lstDevices = param as List<Device>;
                AuthorizationResult authorizationResult = this.Core.GetHostObject(this.Core.AlephATMAppData.StatusHostName).Host.SendDeviceError(lstDevices);
                Log.Info($"Return: {authorizationResult.authorizationStatus}");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ProcessIOBoardEvent(DeviceMessage dm)
        {
            bool ret = false;
            SensorsState sensorsState = null;
            try
            {
                if (dm.Header.Type == Types.Event)
                {
                    sensorsState = Utils.JsonDeserialize<SensorsState>(out ret, dm.Payload.ToString());
                }
                else if (dm.Header.Type == Types.Completion)
                {
                    Completion cr = (Completion)dm.Payload;
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                        sensorsState = Utils.JsonDeserialize<SensorsState>(out ret, cr.Data);
                }
                if (ret)
                {
                    if (this.SOH.SensorsState == null)
                    {
                        this.SendInitialSensorState(sensorsState);
                    }
                    else
                    {
                        //1)- Presence bag sensor
                        if (this.SOH.SensorsState.Presence && !sensorsState.Presence)
                        {
                            this.Core.WriteEJ($"-PRESENCE SENSOR: SHIPOUT-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_PresenceSensorShipOut);
                            if (this.DevConf.IOBoardConfig.Model == Enums.IOBoardModel.PicBoard)
                            {
                                this.ExecuteShipOut(shipOutLogic: false, string.Empty);
                            }
                            else
                            {
                                if (!sensorsState.Door)
                                {
                                    if (dm.Header.Type == Types.Event)
                                    {
                                        this.ExecuteShipOut(shipOutLogic: false, string.Empty);
                                    }
                                }
                                else { Log.Warn("ShipOut disable by chest door closed."); }
                            }
                        }
                        if (!this.SOH.SensorsState.Presence && sensorsState.Presence)
                        {
                            if (dm.Header.Type == Types.Event)
                            {
                                this.Core.WriteEJ($"-PRESENCE SENSOR: SHIPIN-");
                                this.SendDeviceStatus(Enums.DeviceStatus.AIO_PresenceSensorShipIn);
                                if (this.DevConf.IOBoardConfig.Model == Enums.IOBoardModel.PicBoard)
                                {
                                    this.SOH.ShipOutAvailable = true; //Active shipOut for next change state of presence sensor
                                    this.ExecuteShipIn(shipInLogic: false);
                                }
                                else
                                {
                                    if (!sensorsState.Door)
                                    {
                                        this.ExecuteShipIn(shipInLogic: false);
                                    }
                                    else { Log.Warn("ShipIn disable by chest door closed."); }
                                }
                            }
                        }
                        //2)- Full bag sensor
                        if (this.SOH.SensorsState.Cover && !sensorsState.Cover)
                        {
                            this.Core.WriteEJ("-COVER SENSOR: OPEN-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CoverSensorOpen);
                        }
                        if (!this.SOH.SensorsState.Cover && sensorsState.Cover)
                        {
                            this.Core.WriteEJ("-COVER SENSOR: CLOSE-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CoverSensorClose);
                        }
                        //3)- Chest door sensor
                        if (this.SOH.SensorsState.Door && !sensorsState.Door)
                        {
                            this.Core.WriteEJ("-DOOR SENSOR: OPEN-");
                            this.SOH.ShipOutAvailable = true; //Active shipOut for next change state of presence sensor
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_ChestDoorSensorOpen);
                        }
                        if (!this.SOH.SensorsState.Door && sensorsState.Door)
                        {
                            this.Core.WriteEJ("-DOOR SENSOR: CLOSE-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_ChestDoorSensorClose);
                        }
                        //4)- Lock sensor
                        if (this.SOH.SensorsState.Lock && !sensorsState.Lock)
                        {
                            this.Core.WriteEJ("-LOCK SENSOR: OPEN-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CabinetDoorSensorOpen);//La placa arduino reporta Lock como puerta superior
                        }
                        if (!this.SOH.SensorsState.Lock && sensorsState.Lock)
                        {
                            this.Core.WriteEJ("-LOCK SENSOR: CLOSE-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CabinetDoorSensorClose);
                            //this.Core.ExitSpervisorMode(false); //Este retorno se maneja directamente desde el estado Supervisor
                        }
                        //4)- Upper door sensor
                        if (this.SOH.SensorsState.UpperDoor && !sensorsState.UpperDoor)
                        {
                            this.Core.WriteEJ("-UPPER DOOR SENSOR: OPEN-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CabinetDoorSensorOpen);
                        }
                        if (!this.SOH.SensorsState.UpperDoor && sensorsState.UpperDoor)
                        {
                            this.Core.WriteEJ("-UPPER DOOR: CLOSE-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CabinetDoorSensorClose);
                        }
                        //6)- Pre door sensor
                        if (this.SOH.SensorsState.PreDoor && !sensorsState.PreDoor)
                        {
                            this.Core.WriteEJ("-PRE DOOR SENSOR: OPEN-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_PreDoorSensorOpen);
                        }
                        if (!this.SOH.SensorsState.PreDoor && sensorsState.PreDoor)
                        {
                            this.Core.WriteEJ("-PRE DOOR: CLOSE-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_PreDoorSensorClose);
                        }
                        //7)- Comb sensor
                        if (this.SOH.SensorsState.Comb && !sensorsState.Comb)
                        {
                            this.Core.WriteEJ("-COMB SENSOR: OPEN-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CombSensorOpen);
                        }
                        if (!this.SOH.SensorsState.Comb && sensorsState.Comb)
                        {
                            this.Core.WriteEJ("-COMB: CLOSE-");
                            this.SendDeviceStatus(Enums.DeviceStatus.AIO_CombSensorClose);
                        }
                    }
                    this.SOH.SensorsState = sensorsState;//Update GLOBAL sensors state
                    this.ChangeDEV_Fitness(Enums.Devices.IOBoard, Const.Fitness.NoError, Enums.DeviceStatus.AIO_DeviceSuccess);
                }
                else
                {
                    Log.Error("IO board format error");
                    this.ChangeDEV_Fitness(Enums.Devices.IOBoard, Const.Fitness.Fatal, Enums.DeviceStatus.AIO_DeviceError);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SendInitialSensorState(SensorsState sensorsState)
        {
            //1)- Presence bag sensor
            if (sensorsState.Presence)
            {
                this.Core.WriteEJ($"-PRESENCE SENSOR: SHIPIN-");
                this.SendDeviceStatus(Enums.DeviceStatus.AIO_PresenceSensorShipIn);
            }
            else
            {
                this.Core.WriteEJ($"-PRESENCE SENSOR: SHIPOUT-");
                this.SendDeviceStatus(Enums.DeviceStatus.AIO_PresenceSensorShipOut);
            }
            //2)- Full bag sensor
            if (sensorsState.Cover)
            {
                this.Core.WriteEJ("-COVER SENSOR: CLOSE-");
                this.SendDeviceStatus(Enums.DeviceStatus.AIO_CoverSensorClose);
            }
            else
            {
                this.Core.WriteEJ("-COVER SENSOR: OPEN-");
                this.SendDeviceStatus(Enums.DeviceStatus.AIO_CoverSensorOpen);
            }
            if (this.DevConf.IOBoardConfig.Model == Enums.IOBoardModel.AIO)
            {
                //3)- Chest door sensor
                if (sensorsState.Door)
                {
                    this.Core.WriteEJ("-DOOR SENSOR: CLOSE-");
                    this.SendDeviceStatus(Enums.DeviceStatus.AIO_ChestDoorSensorClose);
                }
                else
                {
                    this.Core.WriteEJ("-DOOR SENSOR: OPEN-");
                    this.SOH.ShipOutAvailable = true; //Active shipOut for next change state of presence sensor
                    this.SendDeviceStatus(Enums.DeviceStatus.AIO_ChestDoorSensorOpen);
                }
                //4)- Cabinet door sensor
                if (sensorsState.Lock)
                {
                    this.Core.WriteEJ("-LOCK SENSOR: CLOSE-");
                    this.SendDeviceStatus(Enums.DeviceStatus.AIO_CabinetDoorSensorClose);
                }
                else
                {
                    this.Core.WriteEJ("-LOCK SENSOR: OPEN-");
                    this.SendDeviceStatus(Enums.DeviceStatus.AIO_CabinetDoorSensorOpen);
                }
            }
        }

        private void ExecuteShipIn(bool shipInLogic)
        {
            string text = "";
            try
            {
                text = (shipInLogic ? "LOGIC" : "MECHANIC");
                this.Core.Counters.UpdateCOLLECTIONID();
                string text2 = $"SHIPIN #: {this.Core.Counters.GetBATCH()} - TYPE: {text}";
                string text3 = $"New COLLECTIONID #: {this.Core.Counters.GetCOLLECTIONID()}";
                this.Core.WriteEJ(text2);
                this.Core.WriteEJ(text3);
                Log.Info(text2);
                Log.Info(text3);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ExecuteShipOut(bool shipOutLogic, string userData)
        {
            string text = "";
            try
            {
                this.DeserializeUserData(userData);
                text = (shipOutLogic ? "LOGIC" : "MECHANIC");
                if (this.Core.AlephATMAppData.ShipOutInBackgroundEnable)
                {
                    if (this.SOH.ShipOutAvailable)
                    {
                        this.SOH.ShipOutAvailable = shipOutLogic;
                        string text2 = $"SHIPOUT #: {this.Core.Counters.GetBATCH()} - TYPE: {text}";
                        string text3 = $"Current COLLECTIONID #: {this.Core.Counters.GetCOLLECTIONID()}";
                        this.Core.WriteEJ(text2);
                        this.Core.WriteEJ(text3);
                        this.Core.StateExecute.StartActivity(this.Core.AlephATMAppData.StateShipout);
                        this.ResetBagFillLevel(); 
                        Log.Info(text2);
                        Log.Info(text3);
                    }
                    else
                        Log.Warn("ShipOut has already been done");
                }
                else
                    this.Core.StartShipoutMode(shipOutLogic);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private bool DeserializeUserData(string userData)
        {
            bool ret = false;
            CommandData commandData;
            try
            {
                this.ShipoutUser = null;
                if (!string.IsNullOrEmpty(userData))
                {
                    commandData = Utils.NewtonsoftDeserialize<CommandData>(out ret, userData);
                    Log.Info($"Command Data: {commandData.Data.ToString()}");
                    var jObject = JObject.Parse(commandData.Data.ToString());
                    string user = jObject["Login"].ToString();
                    string userName = jObject["Name"].ToString();
                    if (ret && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(userName))
                    {
                        this.ShipoutUser = new UserProfile_Type(user, userName);
                        Log.Info($"Shipout User: {this.ShipoutUser.UserName}");
                        ret = true;
                    }
                    else { Log.Warn("User didn't detect"); }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Checkeamos que haya subcriptores al evento que entrega los Completion recibidos
        /// </summary>
        /// <param name="data">.</param>
        protected virtual void RaiseEvtCompletionReceive(DeviceMessage dm)
        {
            DelegateCompletionReceive tmp = EvtCompletionReceive;
            if (tmp != null)
                tmp(dm);
        }

        /// <summary>
        /// Checkeamos que haya subcriptores al evento que entrega los Eventos de dispositivos recibidos
        /// </summary>
        /// <param name="data">.</param>
        public virtual void RaiseEvtEventReceive(DeviceMessage dm)
        {
            DelegateEventReceive tmp = EvtEventReceive;
            if (tmp != null)
                tmp(dm);
        }

        /// <summary>
        /// Checkeamos que haya subcriptores al evento que entrega los ACK de dispositivos recibidos
        /// </summary>
        /// <param name="data">.</param>
        protected virtual void RaiseEvtAckReceive(DeviceMessage dm)
        {
            DelegateAckReceive tmp = EvtAckReceive;
            if (tmp != null)
                tmp(dm);
        }
        #endregion "From AlephDEV"

        #region "WebSocket functions"
        private void InitializeDevicesConnection()
        {
            try
            {
                if (this.Core.AlephATMAppData.AlephDEVEnable)
                {
                    this.QueueDeviceMsg = new Queue<DeviceMessage>();
                    this.QueueDeviceEvents = new Queue<List<Device>>();
                    if (this.Core.AlephATMAppData.AutoStartAlephDEV)
                    {
                        this.KillAlephDEV();
                        this.TimerAutoStartAlephDEV = new System.Timers.Timer();
                        this.TimerAutoStartAlephDEV.Interval = 300;
                        this.TimerAutoStartAlephDEV.Elapsed += new System.Timers.ElapsedEventHandler(AnalyzeProcess);
                        this.TimerAutoStartAlephDEV.Enabled = true;
                    }
                    //Timer para desencolar y envíar los mensajes ade eventos de dispositivos
                    this.TimerDeviceEvents = new System.Timers.Timer(100);
                    this.TimerDeviceEvents.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerSendDeviceEvent_Elapsed);
                    this.TimerDeviceEvents.Enabled = true;
                    //Timer para desencolar y envíar los mensajes a AlephDEV
                    this.TimerQueueDeviceMsg = new System.Timers.Timer(30);
                    this.TimerQueueDeviceMsg.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerSendMessage_Elapsed);
                    this.TimerQueueDeviceMsg.Enabled = true;
                    //Control de timeOut de mensajes hacia AlephDEV
                    this.TimerAlephDEVconnTimeOut = new System.Timers.Timer(this.Core.AlephATMAppData.AlephDEVTimeOut);//Tiempo de espera de respuesta a los mensajes enviados a AlepDEV
                    this.TimerAlephDEVconnTimeOut.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerAlephDEVConnection_Elapsed);
                    this.TimerAlephDEVconnTimeOut.Enabled = false;
                    //Control de reconexión de Websocket
                    this.TimerReConn = new System.Timers.Timer(300);
                    this.TimerReConn.Elapsed += new System.Timers.ElapsedEventHandler(SktClientAtm_OnTimedEVT);
                    this.TimerReConn.Start();
                    //websocket
                    this.Websocket = new WebSocket(this.Core.AlephATMAppData.AlephDEVuri);
                    this.Websocket.Opened += new EventHandler(this.Websocket_Opened);
                    this.Websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(this.Websocket_Error);
                    this.Websocket.Closed += new EventHandler(this.Websocket_Closed);
                    this.Websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.Websocket_MessageReceived);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            this.StateConnection = true;
            Log.Info($"CONNECTED WITH ALEPHDEV: {this.Core.AlephATMAppData.AlephDEVuri}");
        }
        private void Websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Log.Error(e.Exception.Message);
        }
        private void Websocket_Closed(object sender, EventArgs e)
        {
            this.StateConnection = false;
            this.TimerReConn.Start();
            Log.Info("DISCONNECTED WITH AlephDEV");
        }

        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Log.Info($"RCV data from AlephDEV: {e.Message}");
            this.ProcessAlephDEVmessage(e.Message);
        }

        private void SktClientAtm_OnTimedEVT(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.TimerReConn.Stop();
                this.TimerReConn.Interval = 1500;
                if (!this.StateConnection)
                    this.Websocket.Open();
            }
            catch (Exception) { this.TimerReConn.Start(); }
        }

        /// <summary>
        /// Desencola y envía los mensajes a AlephDEV
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerSendMessage_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            DeviceMessage msg;
            try
            {
                this.TimerQueueDeviceMsg.Stop();
                if (this.QueueDeviceMsg.Count != 0)
                {
                    this.DeviceMessageToSend = this.QueueDeviceMsg.Peek();
                    this.DeviceMessageToSend.Header.RequestId = this.RequestId;
                    if (this.SendDataToAlephDEV(this.DeviceMessageToSend.ToJson()))//Send data
                    {
                        this.RetrySendCount = 0;
                        msg = this.QueueDeviceMsg.Dequeue();
                        this.UpdateRequestId();
                        if (msg.Device == Enums.Devices.Terminal && msg.Command == Enums.Commands.State && this.Core.IsBNE_S110M())
                            this.TimerAlephDEVconnTimeOut.Interval = 600000; //FIX: Espera de 10 minutos por si se esta actualizando el template
                        else
                            this.TimerAlephDEVconnTimeOut.Interval = this.Core.AlephATMAppData.AlephDEVTimeOut;
                        this.TimerAlephDEVconnTimeOut.Enabled = true;//Activo el control de time out
                    }
                    else
                    {
                        this.RetrySendCount++;
                        Thread.Sleep(1000);
                        if (this.RetrySendCount > this.RetrySendLimit)
                        {
                            this.RetrySendCount = 0;
                            msg = this.QueueDeviceMsg.Dequeue();
                            this.AutomaticResponse(); //Si no logro transmitir el mensaje realizo una contestación automática
                            Log.Error("The time limit was reached: {0}", this.RetrySendLimit);
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            finally { this.TimerQueueDeviceMsg.Start(); }
        }

        internal int UpdateRequestId()
        {
            try
            {
                Log.Debug("Current request ID: {0}", this.RequestId);
                if (this.RequestId >= 9999)
                    this.RequestId = -1;
                this.RequestId++;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return this.RequestId;
        }

        /// <summary>
        /// Se activa al vencerse el tiempo de espera de la respuesta al mensaje enviado a AlephDEV
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerAlephDEVConnection_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            Log.Warn("Time out of AlephDEV.");
            this.TimerAlephDEVconnTimeOut.Enabled = false;
            this.AutomaticResponse();
        }

        /// <summary>
        /// Se activa ante 2 situaciones:
        /// 1)- Si no se logra transmitir el mensaje a AlephDEV
        /// 2)- Si no se recibe respuesta de AlephDEV
        /// </summary>
        private void AutomaticResponse()
        {
            bool ret = false;
            try
            {
                if (this.DeviceMessageToSend != null)
                {
                    DeviceMessage dm = new DeviceMessage(this.DeviceMessageToSend.ToJson(), 0, out ret);
                    Completion cr = new Completion(CompletionCodeEnum.TimeOut, "TIME OUT", "internal response");
                    dm.Payload = cr;
                    this.RaiseEvtCompletionReceive(dm);
                    this.DeviceMessageToSend = null;
                    Log.Warn($"Automatic response: {dm.ToJson()}");
                }
                else
                    Log.Error($"DeviceMessageToSend is null");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Re lanza el evento a partir del evento generado en la clase Logger
        /// </summary>
        /// <param name="sDataReceived">Mensaje a logear</param>
        private void QueueDeviceDataToAlephDEV(DeviceMessage dm)
        {
            try
            {
                if (this.QueueDeviceMsg.Count < 2000)
                {
                    this.QueueDeviceMsg.Enqueue(dm);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private bool SendDataToAlephDEV(string data)
        {
            bool ret = false;
            try
            {
                if (this.Websocket.State != WebSocketState.Open && !this.Websocket.Handshaked)
                {
                    this.StateConnection = false;
                    this.Websocket.Open();
                    this.StateConnection = true;
                }
                else
                {
                    if (data != null)
                    {
                        Log.Info($"SND data to AlephDEV: {MaskSignature(data.Replace(Const.FS, '?'))}");
                        this.Websocket.Send(data);
                        ret = true;
                    }
                }
            }
            catch (Exception) { }
            return ret;
        }
        #endregion "WebSocket functions"

        #region Miscelaneous
        public static string MaskSignature(string jsonString)
        {
            // Define the regex pattern to find the signature value
            string pattern = "(\"signature\":\")(.*?)(\")";

            // Use the regex to replace the signature value with 6 asterisks
            return Regex.Replace(jsonString, pattern, m => $"{m.Groups[1].Value}******{m.Groups[3].Value}");
        }

        private void KillAlephDEV()
        {
            try
            {
                string fileNameAndPath = string.Format(@"{0}AlephDEV.exe", Entities.Const.appPath);
                if (!string.IsNullOrEmpty(this.Core.AlephATMAppData.AlephDEVPath)) //Si defino una ruta, la tomo como válida.
                    fileNameAndPath = string.Format(@"{0}\AlephDEV.exe", this.Core.AlephATMAppData.AlephDEVPath);
                this.Core.KillExternalApp(fileNameAndPath);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AnalyzeProcess(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.TimerAutoStartAlephDEV.Stop();
                this.Core.LoopObserver();
            }
            catch (Exception ex) { Log.Fatal(ex); }
            finally { this.TimerAutoStartAlephDEV.Start(); }
        }
        #endregion Miscelaneous

    }
}
