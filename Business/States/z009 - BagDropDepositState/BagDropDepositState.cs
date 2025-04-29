using Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Business.BagDropDepositState
{
    public class BagDropDepositState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private string NextState;
        private bool EscrowOpen = false;
        private bool FlagIsCancel = false;
        private bool FlagIsTimeOut = false;
        BagDropDepositStateTableData_Type bagDropDepositStateTableData; //Tabla con datos provenientes del download.
        PropertiesBagDropDepositState prop;
        BagDropDepositStateTableExtension1_Type extensionTable1 = null;
        private Enums.Phases Phase = Enums.Phases.Phase_0;
        private bool VerifyEmptyEscrow = false;
        private bool VerifySensorsFlag = true;
        private bool FirstPrinted = false;
        private bool FlagVerifyWithNotesInEscrow = true;
        private bool MoreTimeSubscribed = false;
        private bool PrinterNotAvailable = false;

        #region "Constructor"
        public BagDropDepositState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            bool ret = false;
            this.ActivityName = "BagDropDepositState";
            this.bagDropDepositStateTableData = (BagDropDepositStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesBagDropDepositState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesBagDropDepositState>(out ret, this.prop);
            if (ret)
            {
                if (this.bagDropDepositStateTableData.Item != null)
                    extensionTable1 = (BagDropDepositStateTableExtension1_Type)this.bagDropDepositStateTableData.Item;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.bagDropDepositStateTableData.Item2;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.bagDropDepositStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.bagDropDepositStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.bagDropDepositStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.bagDropDepositStateTableData.TimeOutNextStateNumber;
                if (this.prop.OperationMode == Enums.CimModel.None)
                    this.prop.OperationMode = this.bagDropDepositStateTableData.OperationMode;
                if (string.IsNullOrEmpty(this.prop.Extension1.ScreenMode) && extensionTable1 != null)
                    this.prop.Extension1.ScreenMode = extensionTable1.ScreenMode;
                if (string.IsNullOrEmpty(this.prop.Extension1.MainScreenNumber) && extensionTable1 != null)
                    this.prop.Extension1.MainScreenNumber = extensionTable1.MainScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.ProcessScreenNumber) && extensionTable1 != null)
                    this.prop.Extension1.ProcessScreenNumber = extensionTable1.ProcessScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.ConfirmationScreenNumber) && extensionTable1 != null)
                    this.prop.Extension1.ConfirmationScreenNumber = extensionTable1.ConfirmationScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.DepositMaxQuantity) && extensionTable1 != null)
                    this.prop.Extension1.DepositMaxQuantity = extensionTable1.DepositMaxQuantity;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language6) && extensionTable1 != null)
                    this.prop.Extension1.Language6 = extensionTable1.Language6;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language7) && extensionTable1 != null)
                    this.prop.Extension1.Language7 = extensionTable1.Language7;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language8) && extensionTable1 != null)
                    this.prop.Extension1.Language8 = extensionTable1.Language8;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
        }
        #endregion "Constructor"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_core"></param>
        /// <returns></returns>
        public override bool InitializeActivity(Core core)
        {
            bool ret = false;
            try
            {
                this.Core = core;
                Log.Info($"/--> Activity Name: {this.ActivityName}");
                this.PrepareMoreTime();
                ret = true;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public override void ActivityStart()
        {
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.GetPerformaceData();
                this.AddEventHandlers();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.FlagVerifyWithNotesInEscrow = true;
                this.PrinterNotAvailable = false;
                this.FlagIsCancel = false;
                this.FlagIsTimeOut = false;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                if (!this.Core.Sdo.StateConnection)
                {
                    Log.Error("AlephDEV disconnected.");
                    this.SetActivityResult2(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                }
                else
                {
                    this.Core.Bo.ExtraInfo.Amount = 0;
                    this.VerifyEmptyEscrow = false;
                    this.FirstPrinted = false;
                    this.VerifySensorsFlag = true;
                    this.Core.Bo.ExtraInfo.BagDropInfo = new BagDropInfo();
                    this.ShowInputDataScreen();
                    //Thread.Sleep(100);//Demora para esperar a que renderice la pantalla
                    this.StartTimer(false);
                    if (this.prop.VerifyLogicalFullBin && this.Core.Counters.LogicalFullBin)
                    {
                        Log.Warn("Logical: Cassette Full");
                        this.WriteEJ("Logical Full Bin");
                        this.ShowModalAdvice(this.prop.OnCashAcceptCassetteFull);
                    }
                    else
                    {
                        if (this.prop.VerifyPrinter)
                        {
                            this.Core.Sdo.PTR_GetState();
                        }
                        else
                        {
                            Log.Warn("VERIFY PRINTER: DISABLED");
                            if (this.Core.Sdo.DevConf.IOBoardConfig.Enable)
                            {
                                this.Core.Sdo.IOBoard_GetState();
                            }
                            else
                            {
                                if (this.prop.OperationMode != Enums.CimModel.MEI && this.Phase == Enums.Phases.Phase_1) //Distinto a MEI
                                    this.Core.Sdo.CIM_AsyncOpen();
                                Log.Warn("VERIFY SENSORS: DISABLED");
                                this.WriteEJ("VERIFY SENSORS: DISABLED");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los retornos de todas las funciones ejecutadas por el SDO
        /// </summary>
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void HandlerCashInDataReceive(DeviceMessage dm)
        {
            Completion cr;
            try
            {
                Log.Info($"/--> Device: {dm.Device}");
                cr = (Completion)dm.Payload;
                //Logeo resultado
                if (dm.Device == Enums.Devices.IOBoard || dm.Device == Enums.Devices.CashAcceptor || dm.Device == Enums.Devices.Printer)
                {
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                        Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                    else
                        Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                }
                if (dm.Device == Enums.Devices.Printer && dm.Command == Enums.Commands.State)
                {
                    this.PrinterNotAvailable = this.GetPrinterState(cr);
                    if (this.Core.Sdo.DevConf.IOBoardConfig.Enable)
                    {
                        this.Core.Sdo.IOBoard_GetState();
                    }
                    else
                    {
                        if (this.prop.OperationMode != Enums.CimModel.MEI)
                            this.Core.Sdo.CIM_AsyncOpen();
                        Log.Warn("VERIFY SENSORS: DISABLED");
                        this.WriteEJ("VERIFY SENSORS: DISABLED");
                    }
                }
                else if (dm.Device == Enums.Devices.IOBoard)
                {
                    if (dm.Command == Enums.Commands.State)
                        if (cr.CompletionCode == CompletionCodeEnum.Success)
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.IOBoard, Const.Fitness.NoError, Enums.DeviceStatus.AIO_DeviceSuccess);
                            if (this.AnalyzeSensorsState(cr.Data) && this.prop.OperationMode != Enums.CimModel.MEI)
                                this.Core.Sdo.CIM_AsyncOpen();
                        }
                        else
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.IOBoard, Const.Fitness.Fatal, Enums.DeviceStatus.AIO_DeviceError);
                            this.ShowModalAdvice(this.prop.OnSensorErrorAdvice);
                        }
                }
                else if (dm.Device == Enums.Devices.CashAcceptor)
                {
                    switch (dm.Command)
                    {
                        //1)- Respuesta al comando de OPEN CIM              
                        case Enums.Commands.Open:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                this.Core.Sdo.CIM_Status();
                            }
                            else
                            {
                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                                this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                            }
                            break;

                        //2)- Respuesta al comando de OPEN ESCROW
                        case Enums.Commands.OpenEscrowShutter:
                            if (cr.CompletionCode != CompletionCodeEnum.Success)
                            {
                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                                this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                            }
                            else
                            {
                                WriteEJ("ESCROW GATE OPEN");
                                this.EscrowOpen = true;
                                //Print 1 ticket
                                if (!this.FirstPrinted)
                                {
                                    this.ProcessPrinterData(this.prop.OnPrintFirstTicket, true);
                                    this.FirstPrinted = true;
                                }
                            }
                            break;

                        //3)- Respuesta al comando de CLOSE ESCROW               
                        case Enums.Commands.CloseEscrowShutter:
                            WriteEJ("ESCROW GATE CLOSE");
                            if (this.prop.OperationMode == Enums.CimModel.SNBC)
                            {
                                this.CallHandler(this.prop.OnPleaseWait);
                                this.EscrowOpen = false;
                                if (this.VerifyEmptyEscrow)
                                {
                                    this.Core.Sdo.CIM_Status();
                                }
                                else if (this.FlagIsCancel)//Cancel
                                    this.Core.Sdo.CIM_AsyncClose();
                            }
                            else if (this.prop.OperationMode == Enums.CimModel.Glory)
                            {
                                if (cr.CompletionCode == CompletionCodeEnum.Success)
                                {
                                    if (this.FlagIsCancel)//Cancel
                                    {
                                        this.CallHandler(this.prop.OnPleaseWait);
                                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                                    }
                                    else
                                    {
                                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.NoError, Enums.DeviceStatus.CIM_DeviceSuccess);//Envía el status a host de device Ok
                                        this.PersistDeposit();
                                    }
                                }
                                else
                                {
                                    this.Phase = Enums.Phases.Phase_3;
                                    this.CallHandler(this.prop.OnEmptyEscrowAdvice);
                                }
                            }
                            break;

                        //4)- Respuesta al comando de pedido de STATUS
                        case Enums.Commands.Status:
                            this.Process_Completion_CIM_Status(cr);
                            break;

                        //5)- Respuesta al comando de RESET
                        case Enums.Commands.Reset:
                            this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                            break;

                        //6)- Respuesta al comando de OPEN RETRACT
                        case Enums.Commands.OpenRetractShutter:
                            Thread.Sleep(1000);
                            WriteEJ("RETRACT GATE OPEN");
                            this.Core.Sdo.CIM_CloseRetractShutter();
                            break;

                        //7)- Respuesta al comando de CLOSE RETRACT
                        case Enums.Commands.CloseRetractShutter:
                            WriteEJ("RETRACT GATE CLOSE");
                            this.Core.Sdo.CIM_Close(); //Cierro CIM y espero la respuesta para hacer el NextState
                            break;

                        //8)- CLOSE CIM
                        case Enums.Commands.Close:
                            if (cr.CompletionCode != CompletionCodeEnum.Success)
                                Log.Error("Close CIM error.");
                            this.Quit();
                            this.Core.SetNextState(this.ActivityResult, this.NextState);
                            break;
                    }
                }
                //Printer return
                if (dm.Device == Enums.Devices.Printer)
                {
                    Log.Info($"Func: {dm.Command} Result: {cr.CompletionCode}");
                    if (cr.CompletionCode != CompletionCodeEnum.Success)
                        Log.Error("Print ticket error.");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_Status(Completion cr)
        {
            bool ret = false;
            Log.Debug("/-->");
            try
            {
                bool notesInEscrow = false;
                Log.Info($"Fase: {this.Phase}");
                if (cr.CompletionCode == CompletionCodeEnum.Success)
                {
                    StatusCIM statusCIM = Utilities.Utils.JsonDeserialize<StatusCIM>(out ret, cr.Data);
                    if (ret)
                    {
                        if (statusCIM.Device.Equals("0"))//Verifico si el dispositivo esta ok
                        {
                            this.Core.HideScreenModals(); //Quito los avisos de pantalla
                            if (this.FlagVerifyWithNotesInEscrow)
                            {
                                notesInEscrow = statusCIM.IntermediateStacker.Equals("1") ? true : false;
                                switch (this.Phase)
                                {
                                    case Enums.Phases.Phase_1:
                                        if (notesInEscrow)
                                        {
                                            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_StatusNotesInEscrow);//Envía el status a host de error de CIM
                                            Log.Warn("Notes in escrow before deposit");
                                            this.WriteEJ("Notes in escrow before deposit");
                                            if (this.prop.ActiveReset)
                                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                            else
                                                this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                                        }
                                        break;
                                    case Enums.Phases.Phase_3:
                                        if (notesInEscrow)
                                        {
                                            if (this.FlagIsCancel)//Cancel
                                            {
                                                this.CallHandler(this.prop.OnNotEmptyEscrowAdvice);
                                                this.StartTimer(true);
                                            }
                                            else//TimeOut (se envía depósito de HOST)
                                            {
                                                this.Phase = Enums.Phases.Phase_4;
                                                this.Core.Sdo.CIM_Status();
                                            }
                                        }
                                        else
                                        {
                                            this.Core.Sdo.CIM_CloseEscrowShutter();
                                            this.CallHandler(this.prop.OnPleaseWait);
                                            this.VerifyEmptyEscrow = false;
                                        }
                                        break;
                                    default:
                                        if (notesInEscrow)
                                        {
                                            //FIN DE DEPÓSITO DE SOBRE EXITOSO
                                            this.CallHandler(this.prop.OnPleaseWait);
                                            if (this.prop.OperationMode == Enums.CimModel.SNBC)
                                            {
                                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.NoError, Enums.DeviceStatus.CIM_DeviceSuccess);//Envía el status a host de device Ok
                                                this.PersistDeposit();
                                            }
                                            else if (this.prop.OperationMode == Enums.CimModel.Glory)
                                            {
                                                this.Core.Sdo.CIM_CloseEscrowShutter();
                                            }
                                        }
                                        else
                                        {
                                            if (this.prop.OperationMode == Enums.CimModel.SNBC)
                                            {
                                                if (this.FlagIsTimeOut)
                                                {
                                                    this.Core.Sdo.CIM_AsyncClose();
                                                }
                                                else
                                                {
                                                    this.StartTimer(true);
                                                    this.Core.Sdo.CIM_OpenEscrowShutter();
                                                    this.Phase = Enums.Phases.Phase_3;
                                                    this.CallHandler(this.prop.OnEmptyEscrowAdvice);
                                                }
                                            }
                                            else if (this.prop.OperationMode == Enums.CimModel.Glory)
                                            {
                                                if (this.FlagIsTimeOut)
                                                {
                                                    this.FlagIsCancel = true;
                                                    this.Core.Sdo.CIM_CloseEscrowShutter();
                                                }
                                                else
                                                {
                                                    this.Phase = Enums.Phases.Phase_3;
                                                    this.CallHandler(this.prop.OnEmptyEscrowAdvice);
                                                }
                                            }

                                        }
                                        break;
                                }
                            }
                            else
                            {
                                this.EscrowOpen = true;
                                this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                            }
                        }
                        else
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                            Log.Error("Device: hardware error");
                            this.WriteEJ("CIM HARDWARE ERROR");
                            if (this.prop.ActiveReset)
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                            else
                                this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                        }
                    }
                    else
                    {
                        Log.Error("JsonDeserialize data error");
                        this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                    }
                }
                else
                {
                    Log.Error($"Completion code error: {cr.CompletionCode}");
                    this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        private void PersistDeposit()
        {
            List<Detail> lstDetail = new List<Detail>();
            List<Item> lstItems;
            Contents contents;
            string extraDataName = string.Empty;
            string extraDataValue = string.Empty;
            int count = 0;
            try
            {
                Log.Debug("/--->");
                foreach (BagDrop b in this.Core.Bo.ExtraInfo.BagDropInfo.baglist)
                {
                    count++;
                    lstItems = new List<Item>();
                    lstItems.Add(new Item(0, 1, Utilities.Utils.GetDecimalAmount(b.amount), "AGGR", b.barcode, b.type));
                    Detail detail = new Detail(b.currency, Detail.ContainerIDType.Depository, "DROPSAFE", this.Core.GetCollectionId(Enums.TransactionType.DEPOSIT_DECLARED), lstItems);
                    lstDetail.Add(detail);
                    //A)- Electronic Journal
                    this.WriteEJ($"{count}) - BagDropDeposit of {b.type.PadLeft(10, ' ')} {b.currency} {b.amount} - {b.barcode}");
                }
                //B)- Update counters
                contents = new Contents(lstDetail);
                if (!this.Core.Counters.UpdateContents(lstDetail))//Actualizo contadores
                    Log.Error("Can not update counters.");
                //C)- Send transaction message to host
                this.CallHandler(this.prop.OnPleaseWait);

                Thread prtWndThd;
                prtWndThd = new Thread(new ParameterizedThreadStart(this.AuthorizeTransaction));
                prtWndThd.Start(contents);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void AuthorizeTransaction(object obj)
        {
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            Contents contents = obj as Contents;
            try
            {
                Log.Debug("/--->");
                authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.DEPOSIT_DECLARED, contents, this.prop.HostName);
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                {
                    //Print second ticket
                    this.ProcessPrinterData(this.prop.OnPrintSecondTicketOnReceiptPrinter1, false);
                    this.ProcessPrinterData(this.prop.OnPrintSecondTicketOnReceiptPrinter2, false);
                    this.ProcessPrinterData(this.prop.OnPrintSecondTicketOnJournalPrinter, false);
                    this.ProcessPrinterData(this.prop.OnSendTicketToBD, false);
                }
                else
                {
                    //Print ticket host error
                    this.ProcessPrinterData(this.prop.OnPrintSecondTicketOnReceiptPrinterError1, false);
                    this.ProcessPrinterData(this.prop.OnPrintSecondTicketOnReceiptPrinterError2, false);
                    this.ProcessPrinterData(this.prop.OnPrintSecondTicketOnJournalPrinterError, false);
                    this.ProcessPrinterData(this.prop.OnSendTicketToBDError, false);
                }
                //Aumento el número de transacción
                this.Core.Counters.UpdateTSN();
                this.SetActivityResult(StateTransition.StateResult.SUCCESS, this.prop.NextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Muestra la pantalla principal 
        /// </summary>
        private void ShowInputDataScreen()
        {
            if (this.CallHandler(this.prop.OnDepositBag))
            {
                this.Phase = Enums.Phases.Phase_1;
            }
            else
            {
                Log.Error($"Can´t show screen: {this.prop.OnDepositBag.HandlerName}");
                this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
            }
        }

        /// <summary>
        /// Maneja los retornos de las teclas FDK presionadas por el usuario
        /// </summary>
        /// <param name="FDKcode"></param>
        private void HandlerFDKreturn(string FDKcode)
        {
            try
            {
                Log.Info($"-> FDK data: {FDKcode}");
                this.ResetTimer();
                switch (FDKcode)
                {
                    case "A": //Fase: 2 (Detalla el contenido de los valores a depositar)                       
                        this.ShowBagDropSummary();
                        this.Phase = Enums.Phases.Phase_2;
                        this.moreTime.countRetries = 0;
                        break;
                    case "B": //Fase: 3 (Verifica sensores y luego Abre escrow)
                        this.moreTime.countRetries = 0;
                        if (this.Core.Bo.ExtraInfo.BagDropInfo.baglist.Count > 0)
                        {
                            if (this.prop.OperationMode == Enums.CimModel.MEI)
                            {
                                this.PersistDeposit();
                                this.Phase = Enums.Phases.Phase_3;
                            }
                            else
                            {
                                if (this.CallHandler(this.prop.OnOpenEscrowAdvice))
                                {
                                    this.Core.Sdo.CIM_OpenEscrowShutter();
                                    this.Phase = Enums.Phases.Phase_3;
                                }
                                else
                                {
                                    Log.Error($"Can´t show screen: {this.prop.OnOpenEscrowAdvice.HandlerName}");
                                    this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                                }
                            }
                        }
                        else
                        {
                            Log.Warn("Sumary bags is empty");
                            this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                        }

                        break;
                    case "C": //Fase: 4 (Cierre de escrow e impresión de segundo ticket)
                        this.moreTime.countRetries = 0;
                        this.VerifyEmptyEscrow = true;
                        this.StopTimer();
                        this.Phase = Enums.Phases.Phase_4;
                        this.CallHandler(this.prop.OnPleaseWait);
                        if (this.Core.Sdo.DevConf.IOBoardConfig.Enable)
                        {
                            if (this.AnalyzeSensors())
                            {
                                if (this.prop.OperationMode == Enums.CimModel.SNBC)
                                {
                                    this.NextState = this.prop.NextStateNumber;
                                    this.Core.Sdo.CIM_CloseEscrowShutter();
                                }
                                else if (this.prop.OperationMode == Enums.CimModel.Glory)
                                {
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    this.Core.Sdo.CIM_Status();
                                }
                            }
                        }
                        else
                        {
                            if (this.prop.OperationMode == Enums.CimModel.SNBC)
                            {
                                this.NextState = this.prop.NextStateNumber;
                                this.Core.Sdo.CIM_CloseEscrowShutter();
                            }
                            else if (this.prop.OperationMode == Enums.CimModel.Glory)
                            {
                                this.CallHandler(this.prop.OnPleaseWait);
                                this.Core.Sdo.CIM_Status();
                            }
                        }
                        break;
                    case "D": //Retorno de boton de "Escrow empty"
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        this.EscrowOpen = true;
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private bool AnalyzeSensors()
        {
            bool ret = false;
            if (this.Core.Sdo.SOH.SensorsState.Door == false)
            {
                Log.Warn("Sensor: Door or lock open");
                this.ShowModalAdvice(this.prop.OnChestDoorOpenAdvice);
                this.WriteEJ("Chest door open");
            }
            else
            {
                if (this.Core.Sdo.SOH.SensorsState.Presence == false)
                {
                    Log.Warn("Sensor: Cassette NO present");
                    this.ShowModalAdvice(this.prop.OnCassetteNotPresentAdvice);
                    this.WriteEJ("Cassette not present");
                }
                else
                {
                    if (this.Core.Sdo.SOH.SensorsState.Cover == true)
                    {
                        Log.Warn("Sensor: Cassette Full");
                        this.ShowModalAdvice(this.prop.OnCashAcceptCassetteFull);
                        this.WriteEJ("Cassette full");
                    }
                    else
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Muestra los datos de los sobres a depositar para amar la tabla de visualización del cliente
        /// </summary>
        private void ShowBagDropSummary()
        {
            int limit = 0;
            Log.Debug("/--->");
            if (int.TryParse(this.prop.Extension1.DepositMaxQuantity, out limit))
            {
                if (this.Core.Bo.ExtraInfo.BagDropInfo.baglist.Count < limit)
                    this.Core.Bo.ExtraInfo.BagDropInfo.MoreAvailable = true;
                else
                    this.Core.Bo.ExtraInfo.BagDropInfo.MoreAvailable = false;
            }
            else
            {
                Log.Error("prop.Extension1.DepositMaxQuantity not numeric.");
                this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
            }
            if (this.prop.OnBagDropSummary.Action == StateEvent.EventType.runScript)
                this.prop.OnBagDropSummary.Parameters = Utilities.Utils.JsonSerialize(this.Core.Bo.ExtraInfo.BagDropInfo);
            if (!this.CallHandler(this.prop.OnBagDropSummary))
            {
                Log.Error($"Can´t show screen: {this.prop.OnBagDropSummary.HandlerName}");
                this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
            }
        }

        /// <summary>
        /// Recibe los datos de las funciones ejecutadas en el HTML
        /// </summary>
        /// <param name="dataInput"></param>
        /// <param name="dataLink"></param>
        private void HandlerInputData(string dataInput, string dataLink)
        {
            try
            {
                Log.Info($"-> Input: {dataInput}");
                this.ResetTimer();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "DeleteBagDrop": //Clickeo de borrado de item a depositar
                            {
                                int index = 0;
                                if (int.TryParse(dataInput, out index))
                                {
                                    this.Core.Bo.ExtraInfo.BagDropInfo.baglist.RemoveAt(index);
                                    this.ShowBagDropSummary();
                                }
                                else
                                    Log.Error("Can´t delete BagDrop data.");
                                break;
                            }
                        case "BagDropData": //Agrega el una lista los datos parciales de items a depositar 
                            {
                                bool ret = false; ;
                                Entities.BagDrop bagDrop = Utilities.Utils.JsonDeserialize<Entities.BagDrop>(out ret, dataInput);
                                if (ret)
                                {
                                    this.Core.Bo.ExtraInfo.BagDropInfo.baglist.Add(new BagDrop(bagDrop.type, bagDrop.currency, bagDrop.amount, bagDrop.barcode));
                                }
                                else
                                {
                                    Log.Error("Error in BagDrop data.");
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los eventos de las teclas presionadas: cancel y more
        /// </summary>
        /// <param name="othersKeys"></param>
        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info($"/--> Key press: {othersKeys}");
            this.ResetTimer();
            this.WriteEJ($"Key press: {othersKeys}");
            switch (othersKeys)
            {
                case "REQUEST":
                    {
                        if (this.prop.OnShowBagdropDepositConfig.Action == StateEvent.EventType.runScript)
                        {
                            this.prop.OnShowBagdropDepositConfig.Parameters = SendBagdropDepositConfig();
                            this.CallHandler(this.prop.OnShowBagdropDepositConfig);
                        }
                        break;
                    }
                case "MORE": //Agrega mas sobres
                    {
                        this.ShowInputDataScreen();
                        break;
                    }
                case "CANCEL":
                    {
                        if (this.prop.OperationMode == Enums.CimModel.SNBC || this.prop.OperationMode == Enums.CimModel.MEI)
                            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        else if (this.prop.OperationMode == Enums.CimModel.Glory)
                        {
                            if (this.Phase == Enums.Phases.Phase_1 || this.Phase == Enums.Phases.Phase_2)
                                this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                            else
                                this.Core.Sdo.CIM_Status();
                        }
                        this.FlagIsCancel = true;
                        break;
                    }
            }
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Info($"/--> Result: {result}");
                this.StopTimer();
                this.ActivityResult = result;
                this.NextState = nextState;
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                Log.Info($"/--> OperationMode: {this.prop.OperationMode}");
                if (this.prop.OperationMode == Enums.CimModel.SNBC)
                {
                    if (this.EscrowOpen)//Verificación de apertura de escrow para los casos de cancelación o Time Out
                    {
                        if (result == StateResult.CANCEL)
                        {
                            this.CallHandler(this.prop.OnCancelDepositAdvice);
                            this.VerifyEmptyEscrow = false;
                            this.Core.Sdo.CIM_Status();
                        }
                        else if (result == StateResult.TIMEOUT)//TimeOut
                        {
                            Log.Info($"/--> ActionOnTimeOut: {this.prop.ActionOnTimeOut}");
                            switch (this.prop.ActionOnTimeOut)
                            {
                                case Const.ActionOnTimeOut.Persist:
                                    ////TimeOut (se envía depósito de HOST)
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    this.Phase = Enums.Phases.Phase_4;
                                    this.VerifyEmptyEscrow = true;
                                    this.Core.Sdo.CIM_CloseEscrowShutter();
                                    break;
                                default:
                                    //TimeOut (No envía depósito a HOST)
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    this.VerifyEmptyEscrow = false;
                                    this.Core.Sdo.CIM_CloseEscrowShutter();
                                    Thread.Sleep(1000);
                                    this.Core.Sdo.CIM_OpenRetractShutter();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (this.Phase == Enums.Phases.Phase_1 || this.Phase == Enums.Phases.Phase_2)
                            this.Core.Sdo.CIM_AsyncClose();
                        else
                            this.Core.Sdo.CIM_OpenRetractShutter();
                    }
                }
                else if (this.prop.OperationMode == Enums.CimModel.Glory)
                {
                    this.Core.Sdo.CIM_AsyncClose(); //Cierro CIM y espero la respuesta para hacer el NextState
                }
                else if (this.prop.OperationMode == Enums.CimModel.MEI)
                {
                    this.SetActivityResult2(result, nextState);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Solo se utiliza para cerrar el estado en caso de que no haya conexión con el DM
        /// </summary>
        /// <param name="result"></param>
        /// <param name="nextState"></param>
        public void SetActivityResult2(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.StopTimer();
                this.Quit();
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private string SendBagdropDepositConfig()
        {
            string ret = string.Empty;
            try
            {
                JObject jObject = new JObject
                {
                    ["selectableCurrencies"] = JArray.FromObject(this.prop.SelectableCurrencies),
                    ["selectableContents"] = JArray.FromObject(this.prop.SelectableContents)
                };
                ret = jObject.ToString(Formatting.None);
            }
            catch (Exception value) { Log.Fatal(value); }
            return ret;
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.RemoveEventHandlers();
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Sensor handler"
        private bool AnalyzeSensorsState(string jsonData)
        {
            bool ret = false;
            SensorsState sensorsState = new SensorsState();
            try
            {
                sensorsState = Utilities.Utils.JsonDeserialize<SensorsState>(out ret, jsonData);
                if (ret)
                {
                    ret = false;
                    if (sensorsState.Door == false)
                    {
                        Log.Warn("Sensor: Door or lock open");
                        this.ShowModalAdvice(this.prop.OnChestDoorOpenAdvice);
                        this.WriteEJ("Chest door open");
                    }
                    else
                    {
                        if (sensorsState.Presence == false)
                        {
                            Log.Warn("Sensor: Cassette NO present");
                            this.ShowModalAdvice(this.prop.OnCassetteNotPresentAdvice);
                            this.WriteEJ("Cassette not present");
                        }
                        else//Caso de operatividad
                        {
                            if (sensorsState.Cover == true)
                            {
                                Log.Warn("Sensor: Cassette Full");
                                this.ShowModalAdvice(this.prop.OnCashAcceptCassetteFull);
                                this.WriteEJ("Cassette full");
                            }
                            else
                            {
                                if (this.VerifySensorsFlag)
                                {
                                    this.VerifySensorsFlag = false; //Fix para doble disparo de sensor
                                    ret = true;
                                }
                                else
                                    Log.Warn("Unexpected state");
                            }
                        }
                    }
                }
                else
                {
                    Log.Error("Invalid sensor data");
                    this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        private void ShowModalAdvice(StateEvent stateEvent)
        {
            Log.Debug("/--->");
            this.Core.HideScreenModals(); //Quito los avisos de pantalla
            //Thread.Sleep(200);
            this.CallHandler(stateEvent);
            this.StartTimer(true);
        }
        #endregion "sensor handler"

        #region "More time"

        private void AddEventHandlers()
        {
            this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCashInDataReceive);
            this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCashInDataReceive);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
        }

        // More time.
        public MoreTime moreTime;

        // Timeout
        public System.Timers.Timer timerScreen;

        // Indicates if time-out occurs
        public bool timeout = false;

        /// <summary>
        /// Instantiates the MoreTime class. Enables MoreTime beep if 
        /// MoreTimeBeepEnabled property of activity is enabled.
        /// </summary>
        private void PrepareMoreTime()
        {
            bool enableNDCScreen = false;
            if (this.prop.Extension1.ScreenMode.Equals("000"))
                enableNDCScreen = true;
            this.moreTime = new MoreTime(prop.MoreTime.MoreTimeScreenName, prop.MoreTime.MaxTimeOut,
                prop.MoreTime.MaxTimeOutRetries, prop.MoreTime.MoreTimeKeyboardEnabled, this.Core, enableNDCScreen, this.ActivityName);
            this.moreTime.EvtMoreTime += new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
        }

        private void AnalyzeMoreTimeResult(MoreTimeResult result)
        {
            this.AddEventHandlers();//Coloco nuevamente los subscriptores de los eventos
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        if (this.Phase == Enums.Phases.Phase_3)
                            this.StartTimer(true);
                        else
                            this.StartTimer(false);
                        if (this.Phase == Enums.Phases.Phase_1)//Si estoy fuera de la pantalla de ingreso de datos, solo cierro el Div de more time.
                        {
                            this.ShowInputDataScreen();//Carga de nuevo el HTML
                        }
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.CallHandler(this.prop.OnPleaseWait);
                        this.FlagVerifyWithNotesInEscrow = false;
                        if (this.Phase == Enums.Phases.Phase_3 || this.Phase == Enums.Phases.Phase_4)
                        {
                            this.VerifyEmptyEscrow = true;
                            this.FlagVerifyWithNotesInEscrow = true;
                            this.NextState = this.prop.CancelNextStateNumber;
                            this.Core.Sdo.CIM_CloseEscrowShutter();
                        }
                        else
                            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        this.FlagIsTimeOut = true;
                        if (this.Phase == Enums.Phases.Phase_3)
                            this.VerifyEmptyEscrow = true;
                        else
                            this.VerifyEmptyEscrow = false;
                        if (this.prop.OperationMode == Enums.CimModel.SNBC)
                        {
                            this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeOutNextStateNumber);
                        }
                        else if (this.prop.OperationMode == Enums.CimModel.Glory && this.Phase == Enums.Phases.Phase_3)
                        {
                            //this.Core.Sdo.CIM_Close(); //Cierro CIM y espero la respuesta para hacer el NextState
                            Log.Info($"/--> ActionOnTimeOut: {this.prop.ActionOnTimeOut}");
                            switch (this.prop.ActionOnTimeOut)
                            {
                                case Const.ActionOnTimeOut.Persist:
                                    ////TimeOut (se envía depósito de HOST)
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    this.Phase = Enums.Phases.Phase_4;
                                    this.VerifyEmptyEscrow = true;
                                    this.Core.Sdo.CIM_Status();
                                    break;
                                default:
                                    this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeOutNextStateNumber);
                                    break;
                            }
                        }
                        else
                            this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeOutNextStateNumber);
                        break;
                    }
            }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer(bool resetMoreTimeRetry)
        {
            Log.Debug("/--->");
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.MoreTime.MaxTimeOut * 1000;
            this.SubscribeMoreTime(true);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            timeout = false;
            if (resetMoreTimeRetry)
                this.moreTime.ResetRetry();
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
            Log.Debug("/--->");
            if (timerScreen != null)
            {
                this.SubscribeMoreTime(false);
                this.timerScreen.Enabled = false;
                this.timerScreen.Stop();
            }
        }

        private void SubscribeMoreTime(bool enabled)
        {
            if (!enabled) this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);
            else if (!MoreTimeSubscribed) this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);

            this.MoreTimeSubscribed = enabled;
        }

        private void ResetTimer()
        {
            if (this.timerScreen != null)
            {
                this.timerScreen.Stop();
                this.timerScreen.Start();
            }
        }

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //this.Core.RaiseInvokeJavascript("MoreTimeLoad", "");
            this.Core.HideScreenModals();
            this.timeout = true;
            this.StopTimer();
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
        }
        #endregion "More time"
    }
}
