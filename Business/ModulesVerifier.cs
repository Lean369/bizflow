using Entities;
using Entities.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Business
{
    internal class ModulesVerifier
    {

        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");

        public delegate void ChangeFitnessDelegate(Enums.Devices device, Const.Fitness fitness, Enums.DeviceStatus internalCode, string detail = null);
        public event ChangeFitnessDelegate ChangeFitnessEvt;
        public delegate void ChangeSuppliessDelegate(Enums.Devices device, Const.Supplies supplies, Enums.DeviceStatus internalCode);
        public event ChangeSuppliessDelegate ChangeSuppliesEvt;
        public delegate void NotifyDependenciesStateDelegate(List<UnitDependency> unitDependencyList);
        public event NotifyDependenciesStateDelegate NotifyDependenciesStateEvt;

        private Core Core { get; set; }
        private CountersManager countersManager;
        private List<UnitDependency> unitDependencyList;
        private Stack<Enums.Devices> transactionStack;
        private Enums.Devices currentDependency;
        private List<ConfParam> confParams;
        private CoinHopperMapping hprMapping;

        public ModulesVerifier(Core core)
        {
            this.Core = core;
            countersManager = CountersManager.Instance; //to get Contents
            var hprConf = new CoinHoppersConf();
            hprMapping = hprConf.Hoppers;
        }

        /// <summary>
        /// Verify the status of a list of dependency devices.
        /// Result is notified by the event NotifyDependenciesStateEvt
        /// NOTE: Remember to SubscribeCompletion() to be able to handle device responses
        /// </summary>
        /// <param name="dependencieDevs"></param>
        public void Verify(List<Enums.Devices> dependencieDevs)
        {
            unitDependencyList = new List<UnitDependency>();
            transactionStack = new Stack<Enums.Devices>();
            foreach (var dv in dependencieDevs) //mark as completed dependencies that are not enabled in the configuration
            {
                UnitDependency.DepStatus depStatus = UnitDependency.DepStatus.InProgress;
                if (dv == Enums.Devices.CashAcceptor && !this.Core.Sdo.DevConf.CIMconfig.Enable)
                    depStatus = UnitDependency.DepStatus.OK;
                else if (dv == Enums.Devices.CashDispenser && !this.Core.Sdo.DevConf.CDMconfig.Enable)
                    depStatus = UnitDependency.DepStatus.OK;
                else if (dv == Enums.Devices.CoinDispenser && !this.Core.Sdo.DevConf.COINconfig.Enable)
                    depStatus = UnitDependency.DepStatus.OK;
                else if (dv == Enums.Devices.Printer && !this.Core.Sdo.DevConf.PRTconfig.Enable)
                    depStatus = UnitDependency.DepStatus.OK;
                else if (dv == Enums.Devices.IOBoard && !this.Core.Sdo.DevConf.IOBoardConfig.Enable)
                    depStatus = UnitDependency.DepStatus.OK;
                else if (dv == Enums.Devices.BarcodeReader && !this.Core.Sdo.DevConf.BCRconfig.Enable)
                    depStatus = UnitDependency.DepStatus.OK;
                else
                    transactionStack.Push(dv); //only add dependencies that are enabled

                //Get device state int code
                var intCode = this.Core.Sdo.SOH.SDO_DevicesState.Find(d => d.Device == dv)?.InternalCode ?? Enums.DeviceStatus.UNK_Undefined;

                unitDependencyList.Add(new UnitDependency
                {
                    Dependency = dv,
                    ExternalCode = this.Core.Sdo.LstErrorCodesTables.Find(e => e.InternalCode == intCode).ExternalCode,
                    Status = depStatus
                });
            }
            DependencyCheck_START();
        }

        public void SubscribeCompletion()
        {
            this.Core.Sdo.EvtCompletionReceive += HandlerCompletionReceive;
        }
        public void UnsubscribeCompletion()
        {
            this.Core.Sdo.EvtCompletionReceive -= HandlerCompletionReceive;
        }

        public void HandlerCompletionReceive(DeviceMessage dm)
        {
            Log.Info($"/--> {dm.Device}");
            try
            {
                Completion cr = (Completion)dm.Payload;
                if (new[] { dm.Device, this.currentDependency }.All(d => d == Enums.Devices.CashAcceptor))
                {
                    switch (dm.Command)//Switcheo respuestas de los comandos
                    {
                        case Enums.Commands.Open:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                                this.Core.Sdo.CIM_Status();
                            else
                                DependencyCheck_DONE(Enums.Devices.CashAcceptor, UnitDependency.DepStatus.Error);
                            break;
                        case Enums.Commands.Status:
                            HandleAcceptorStatusCompletion(cr);
                            break;
                        case Enums.Commands.Close:
                            DependencyCheck_START(); //we call this method here if we could have not done it from EscapeCIM()
                            break;
                    }
                }
                else if (new[] { dm.Device, this.currentDependency }.All(d => d == Enums.Devices.CashDispenser))
                {
                    switch (dm.Command)
                    {
                        case Enums.Commands.Open:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                                this.Core.Sdo.CDM_Status();
                            else
                                DependencyCheck_DONE(Enums.Devices.CashDispenser, UnitDependency.DepStatus.Error);
                            break;
                        case Enums.Commands.Status:
                            HandleDispenserStatusCompletion(cr);
                            break;
                        case Enums.Commands.CashUnitInfo:
                            HandleCashUnitinfoCompletion(cr);
                            break;
                        case Enums.Commands.Close:
                            DependencyCheck_START(); //we call this method here if we could have not done it from EscapeCDM()
                            break;
                    }
                }
                else if (new[] { dm.Device, this.currentDependency }.All(d => d == Enums.Devices.CoinDispenser))
                {
                    switch (dm.Command)
                    {
                        case Enums.Commands.Open:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                                this.Core.Sdo.COIN_GetStatus();
                            else
                                DependencyCheck_DONE(Enums.Devices.CoinDispenser, UnitDependency.DepStatus.Error);
                            break;
                        case Enums.Commands.Status:
                            HandleCoinHopperStatusCompletion(cr);
                            break;
                        case Enums.Commands.Close:
                            DependencyCheck_START(); //we call this method here if we could have not done it from EscapeCOIN()
                            break;
                    }
                }
                else if (new[] { dm.Device, this.currentDependency }.All(d => d == Enums.Devices.Printer))
                {
                    switch (dm.Command)
                    {
                        case Enums.Commands.State:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                switch (cr.Data)
                                {
                                    case "0":
                                        DependencyCheck_DONE(Enums.Devices.Printer, UnitDependency.DepStatus.OK);
                                        break;
                                    default:
                                        DependencyCheck_DONE(Enums.Devices.Printer, UnitDependency.DepStatus.Error, message: $"Bad status received: {cr.Data} ");
                                        break;
                                }
                            }
                            else
                                DependencyCheck_DONE(Enums.Devices.Printer, UnitDependency.DepStatus.Error);
                            break;
                    }
                }
                else if (new[] { dm.Device, this.currentDependency }.All(d => d == Enums.Devices.IOBoard))
                {
                    switch (dm.Command)
                    {
                        case Enums.Commands.State:
                            HandleIOBoardStateCompletion(cr);
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }



        private void HandleAcceptorStatusCompletion(Completion cr)
        {
            if (cr.CompletionCode != CompletionCodeEnum.Success)
            {
                EscapeCIM(false, "CIM status response was not correct.");
                return;
            }
            StatusCIM statusCIM = Utils.JsonDeserialize<StatusCIM>(out bool ret, cr.Data);
            if (!ret)
            {
                EscapeCIM(false, "CIM cashAcceptor status response could not be deserialized.");
                return;
            }
            if (!statusCIM.Device.Equals("0"))
            {
                EscapeCIM(false, "CIM cashAcceptor status not correct.");
                return;
            }
            if (!(new string[] { "0", "5" }).Contains(statusCIM.IntermediateStacker))
            {
                EscapeCIM(false, "Notes in escrow detected", "NOTES IN ESCROW PRESENT -WARNING-");
                return;
            }
            if (!statusCIM.Acceptor.Equals("0") || !statusCIM.BanknoteReader.Equals("0"))
            {
                EscapeCIM(false, "CIM Acceptor or BanknoteReader status not correct.");
                return;
            }
            EscapeCIM(true);
        }

        private void HandleDispenserStatusCompletion(Completion cr)
        {
            if (cr.CompletionCode != CompletionCodeEnum.Success)
            {
                EscapeCDM(false, "CDM request status completion code was not correct.");
                return;
            }
            StatusCDM statusCDM = Utils.JsonDeserialize<StatusCDM>(out bool ret, cr.Data);
            if (!ret)
            {
                EscapeCDM(false, "CDM cashDispenser status response could not be deserialized.");
                return;
            }
            if (!statusCDM.Device.Equals("0"))
            {
                EscapeCDM(false, "CDM status not correct.");
                return;
            }
            if (!(new string[] { "0", "5" }).Contains(statusCDM.IntermediateStacker))
            {
                EscapeCDM(false, "Notes in escrow detected", "NOTES IN ESCROW PRESENT -WARNING-");
                return;
            }
            this.Core.Sdo.CDM_CashUnitInfo();
        }

        private void HandleCashUnitinfoCompletion(Completion cr)
        {
            var cashunitinfo_dt = CashUnitInfo_data(cr);
            if (cashunitinfo_dt == null)
            {
                EscapeCDM(false, "Se recibio cashunitinfo pero data no es valido");
                return;
            }
            //verify supply in cassettes
            var cassettesConfig = countersManager.GetCassetteConfig();
            this.Core.Sdo.CashUnits = cashunitinfo_dt.LstCashUnit;
            foreach (var unit in cashunitinfo_dt.LstCashUnit)
            {
                Const.Supplies SupplyStatus = Const.Supplies.NoNewState;
                if (unit.Values == 0)
                {
                    continue; //ignore reject bin
                }
                //check cassette status
                var validStatus = new string[] {
                    "0", //WFS_CDM_STATCUOK 
                    "1", //WFS_CDM_STATCUFULL
                    "2", //WFS_CDM_STATCUHIGH
                };
                if (!cassettesConfig.UseXFSUnitsMinimumThreshold)
                    validStatus = validStatus.Append("3").ToArray(); //WFS_CDM_STATCULOW | since STATCULOW must be ignored is added as a valid stat
                if (cassettesConfig.AllowEmptyStateXFSUnits)
                    validStatus = validStatus.Append("4").ToArray(); //WFS_CDM_STATCUEMPTY | allow working even if some cassettes are empty

                //supply status from XFS
                var cdmStatusDetail = CDMStatusDetail(unit.Status);
                SupplyStatus = cdmStatusDetail.Supply;
                bool exit = false;
                if (!validStatus.Contains(unit.Status))
                {
                    this.ChangeSuppliesEvt?.Invoke(Enums.Devices.CashDispenser, SupplyStatus, Enums.DeviceStatus.CDM_DeviceError);
                    EscapeCDM(false, $"Cash Unit number {unit.Number} is inoperative due to {cdmStatusDetail.Description}.");
                    return;
                }
                if (cassettesConfig.UseXFSUnitsMinimumThreshold)
                    continue; //ignore min threshold check
                //check cassette supply
                var cassette = cassettesConfig.TypeCassetteList.FirstOrDefault(c => c.Denomination == unit.Values && c.CurrencyIso.Equals(unit.CurrencyID));
                if (cassette == null)
                {
                    EscapeCDM(false, $"Cassette {unit.Values} {unit.CurrencyID} not found in configuration");
                    exit = true;
                }
                else if (unit.Count < cassette.MinItemsRequired && unit.Count > 0)
                {
                    SupplyStatus = Const.Supplies.MediaLow;
                    EscapeCDM(false, $"Cassette {unit.Values} ({cassette.Type}) has not enough items. It has {unit.Count} but it's requiered {cassette.MinItemsRequired}");
                    exit = true;
                }
                else if (unit.Count <= 0 && cassette.MinItemsRequired > 0)
                {
                    SupplyStatus = Const.Supplies.MediaOut;
                    EscapeCDM(false, $"Cassette {unit.Values} ({cassette.Type}) has no items.");
                    exit = true;
                }
                this.ChangeSuppliesEvt?.Invoke(Enums.Devices.CashDispenser, SupplyStatus, TypeSupplyStatusForCassette(cassette, SupplyStatus));
                if (exit) return;
            }
            EscapeCDM(true);
        }
        
        private void HandleCoinHopperStatusCompletion(Completion cr)
        {
            if (cr.CompletionCode != CompletionCodeEnum.Success)
            {
                EscapeCOIN(false, "Error en la solicitud de status al coin dispenser.");
                return;
            }
            StatusListCOIN statusList = JsonConvert.DeserializeObject<StatusListCOIN>(cr.Data); //Utils.JsonDeserialize<StatusListCOIN>(out ret, cr.Data);
            if (statusList == null)
            {
                EscapeCOIN(false, "COIN status response could not be deserialized. DATA: " + cr.Data);
                return;
            }
            var normalStatus = new List<string> { "NORMAL" };
            if(hprMapping.IgnoreEmtpySignal)
                normalStatus.Add("DETECTED_EMPTY_SIGNAL");
            foreach (var status in statusList.Items)
            {
                if (!normalStatus.Contains(status.ErrorCode))
                {
                    EscapeCOIN(false, $"Coin unit {status.Address} has anormal status.");
                    return;
                }
            }
            var coinSupplyCheck = CoinDispenserSupplyCheck();
            if (coinSupplyCheck.success)
                EscapeCOIN(true);
            else
                EscapeCOIN(false, coinSupplyCheck.message);
        }

        private void HandleIOBoardStateCompletion(Completion cr)
        {
            //this.Core.Sdo.SOH.SensorsState

            if (cr.CompletionCode != CompletionCodeEnum.Success)
            {
                EscapeIOBoard(false, "Error en la solicitud de state al IOBoard.");
                return;
            }
            var state = Utils.JsonDeserialize<Entities.SensorsState>(out bool ret, cr.Data);
            if (!ret)
            {
                EscapeIOBoard(false, "IOBoard state response could not be deserialized.");
                return;
            }
            if (state.UpperDoor && state.Door && state.Lock)
            {
                EscapeIOBoard(true, "IOBoard sensor state ok");
            }
            else
            {
                EscapeIOBoard(false, $"IOBoard module state are not correct. INFO -> UPPER DOOR:{state.UpperDoor}, DOOR: {state.Door}, LOCK: {state.Lock}");
            }
        }

        private void EscapeCIM(bool ok, string message = null, string EJmsg = null)
        {
            DependencyCheck_DONE(Enums.Devices.CashAcceptor, ok ? UnitDependency.DepStatus.OK : UnitDependency.DepStatus.Error, message: message, EJmsg: EJmsg, callNextCheck: this.Core.Sdo.DevConf.CIMconfig.KeepConnectionOpen);
            if (!this.Core.Sdo.DevConf.CIMconfig.KeepConnectionOpen)
                this.Core.Sdo.CIM_Close(); //we dont call for the next dependency since we need to close the device first
        }
        private void EscapeCDM(bool ok, string message = null, string EJmsg = null)
        {
            DependencyCheck_DONE(Enums.Devices.CashDispenser, ok ? UnitDependency.DepStatus.OK : UnitDependency.DepStatus.Error, message: message, EJmsg: EJmsg, callNextCheck: this.Core.Sdo.DevConf.CDMconfig.KeepConnectionOpen);
            if (!this.Core.Sdo.DevConf.CDMconfig.KeepConnectionOpen)
                this.Core.Sdo.CDM_Close(); //we dont call for the next dependency since we need to close the device first
        }
        private void EscapeCOIN(bool ok, string message = null, string EJmsg = null)
        {
            DependencyCheck_DONE(Enums.Devices.CoinDispenser, ok ? UnitDependency.DepStatus.OK : UnitDependency.DepStatus.Error, message: message, EJmsg: EJmsg, callNextCheck: false);
            this.Core.Sdo.COIN_Close();
        }
        private void EscapeIOBoard(bool ok, string message = null, string EJmsg = null)
        {
            DependencyCheck_DONE(Enums.Devices.IOBoard, ok ? UnitDependency.DepStatus.OK : UnitDependency.DepStatus.Error, message: message, EJmsg: EJmsg);
        }

        private Enums.DeviceStatus TypeSupplyStatusForCassette(TypeCassetteConf cassette, Const.Supplies supplyStatus)
        {
            var enumName = $"CDM_{cassette.Type}_{supplyStatus}";
            if (Enum.TryParse(enumName, false, out Enums.DeviceStatus devStatus))
                return devStatus;
            else
                return Enums.DeviceStatus.UNK_Undefined;
        }
        private Enums.DeviceStatus TypeSupplyStatusForUnit(Detail.ContainerIDType unit, Const.Supplies supplyStatus)
        {
            var enumName = $"COIN_{unit}_{supplyStatus}";
            if (Enum.TryParse(enumName, false, out Enums.DeviceStatus devStatus))
                return devStatus;
            else
                return Enums.DeviceStatus.UNK_Undefined;
        }
        private (bool success, string message) CoinDispenserSupplyCheck()
        {
            var ret = true; string msg = "";
            var contents = countersManager.GetCountersFromFile().Contents;
            //coin hoppers configuration
            if (this.hprMapping == null)
            {
                msg = "Could not get CoinHopperMapping configuration.";
                return (false, msg);
            }
            //coin hoppers supplay availability
            var coinDetails = contents.LstDetail.Where(d => d.Currency.Equals(this.Core.AlephATMAppData.DefaultCurrency) && d.ContainerType.Equals("COINDISPENSER")).ToList();
            foreach (var hit in this.hprMapping.CoinHopperList)
            {
                Const.Supplies SupplyStatus = Const.Supplies.NoNewState;
                var cit = coinDetails.Where(c => c.LstItems.Any() && Entities.Functions.MixCalculator.ShifftDot(c.LstItems.First().Denomination, c.LstItems.First().Exponent) == Entities.Functions.MixCalculator.ShifftDot(hit.Value, hit.Exponent)).FirstOrDefault();
                if (cit == null)
                {
                    msg = $"Could not get coin dispenser availability for {hit.Value}. (Check values, exponent and compare default currency).";
                    ret = false;
                    continue;
                }
                if (cit.LstItems.First().Num_Items < hit.Min_Items_Required && cit.LstItems.First().Num_Items > 0)
                {
                    SupplyStatus = Const.Supplies.MediaLow;
                    msg = $"Not enough items available for HOPPER {hit.Value}.";
                    ret = false;
                    this.ChangeSuppliesEvt?.Invoke(Enums.Devices.CoinDispenser, SupplyStatus, TypeSupplyStatusForUnit(cit.ContainerId, SupplyStatus));
                    continue;
                }
                else if (cit.LstItems.First().Num_Items == 0 && hit.Min_Items_Required > 0)
                {
                    SupplyStatus = Const.Supplies.MediaOut;
                    msg = $"Not enough items available for HOPPER {hit.Value}.";
                    this.ChangeSuppliesEvt?.Invoke(Enums.Devices.CoinDispenser, SupplyStatus, TypeSupplyStatusForUnit(cit.ContainerId, SupplyStatus));
                    ret = false;
                    continue;
                }
                else
                {
                    SupplyStatus = Const.Supplies.GoodState;
                }
                this.ChangeSuppliesEvt?.Invoke(Enums.Devices.CoinDispenser, SupplyStatus, TypeSupplyStatusForUnit(cit.ContainerId, SupplyStatus));
            }
            return (ret, msg);
        }
        private CashUnitInfo CashUnitInfo_data(object payload)
        {
            var data = payload as Entities.Completion;
            if (data == null)
            {
                Log.Warn("Se recibio cashunitinfo pero sin completion");
                return null;
            }
            var cashunitinfo = Utils.NewtonsoftDeserialize<Entities.CashUnitInfo>(out bool ret, data.Data);
            if (ret == false)
            {
                Log.Warn("Se recibio cashunitinfo pero data no es valido");
                return null;
            }
            return cashunitinfo;
        }


        private void DependencyCheck_START()
        {
            if (!transactionStack.Any())
                return;
            this.currentDependency = transactionStack.Pop();
            switch (this.currentDependency)
            {
                case Enums.Devices.CashAcceptor:
                    Log.Info("Checking CashAcceptor");
                    this.Core.Sdo.CIM_Open();
                    break;
                case Enums.Devices.CashDispenser:
                    Log.Info("Checking CashDispenser");
                    this.Core.Sdo.CDM_Open();
                    break;
                case Enums.Devices.CoinDispenser:
                    Log.Info("Checking CoinDispenser");
                    this.Core.Sdo.COIN_Open();
                    break;
                case Enums.Devices.Printer:
                    Log.Info("Checking Printer");
                    this.Core.Sdo.PTR_GetState();
                    break;
                case Enums.Devices.IOBoard:
                    Log.Info("Checking IOBoard");
                    this.Core.Sdo.IOBoard_GetState();
                    break;
            }
        }

        private void DependencyCheck_DONE(Enums.Devices dependency, UnitDependency.DepStatus status, string message = null, string EJmsg = null, bool callNextCheck = true)
        {
            Log.Info($"Status dependency {dependency} is {status}");
            GlobalAppData.Instance.WriteEJ($"Status dependency {dependency} is {status}");       
            if (!string.IsNullOrEmpty(message))
            {
                GlobalAppData.Instance.WriteEJ("\t\t-->" + message);
                Log.Warn(message);
            }
            var dep = unitDependencyList.Find(d => d.Dependency == dependency);
            dep.Status = status;

            //informar fitness
            Console.WriteLine($"Informando FITNESS de dependency {dependency} is {status}");
            var fitnessData = FitnessData(dependency, status != UnitDependency.DepStatus.OK);
            this.ChangeFitnessEvt?.Invoke(dependency, fitnessData.Fitness, fitnessData.InternalCode, message);

            if (unitDependencyList.All(d => d.Status != UnitDependency.DepStatus.InProgress))
            {
                this.NotifyDependenciesStateEvt?.Invoke(unitDependencyList);
            }
            else if (callNextCheck)
            {
                this.currentDependency = Enums.Devices.Unknown;
                DependencyCheck_START(); //continue with next dependency
            }
        }

        private (Const.Fitness Fitness, Enums.DeviceStatus InternalCode) FitnessData(Enums.Devices device, bool inError)
        {
            var fitness = !inError ? Const.Fitness.NoError : Const.Fitness.Fatal;
            var codeForDev = new Dictionary<Enums.Devices, string>
            {
                { Enums.Devices.CashAcceptor, "CIM" },
                { Enums.Devices.CashDispenser, "CDM" },
                { Enums.Devices.CoinDispenser, "COIN" },
                { Enums.Devices.Printer, "PTR" },
                { Enums.Devices.IOBoard, "AIO" },
                { Enums.Devices.BarcodeReader, "BCR" },
            };
            if (!codeForDev.ContainsKey(device))
                return (fitness, Enums.DeviceStatus.UNK_Undefined);
            var enumName = inError ? $"{codeForDev[device]}_DeviceError" : $"{codeForDev[device]}_DeviceSuccess";
            if (Enum.TryParse(enumName, false, out Enums.DeviceStatus devStatus))
                return (fitness, devStatus);
            else
                return (fitness, Enums.DeviceStatus.UNK_Undefined);
        }

        private (Const.Supplies Supply, string Description) CDMStatusDetail(string status_num)
        {
            var dic = new Dictionary<string, (Const.Supplies, string)>
            {
                { "0", (Const.Supplies.GoodState, "CashUnit is in a good state.") },
                { "1", (Const.Supplies.Overfill, "CashUnit is full") },
                { "2", (Const.Supplies.MediaNearFull, "CashUnit is almost full") },
                { "3", (Const.Supplies.MediaLow, "CashUnit is almost empty") },
                { "4", (Const.Supplies.MediaOut, "CashUnit is empty") },
                { "5", (Const.Supplies.Unknown, "CashUnit is inoperative") },
                { "6", (Const.Supplies.Unknown, "CashUnit is missing") },
                { "7", (Const.Supplies.Unknown, "Values of the specified CashUnit are not available") },
                { "8", (Const.Supplies.Unknown, "No reference value available for the notes in this cash unit") },
                { "9", (Const.Supplies.Unknown, "CashUnit inserted when not in exchange state.") }
            };
            if (dic.ContainsKey(status_num))
                return dic[status_num];
            else
                return (Const.Supplies.Unknown, "Unknown status");
        }


        public class ConfParam
        {
            public enum Type { COIN_IGNORE_FLAG }
            public Type ParamType { get; set; }
            public bool Value { get; set; }
        }

    }
}
