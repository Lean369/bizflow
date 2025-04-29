using Entities;
using Entities.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Business.CashDispenseState
{
    public class CashDispenseState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        CashDispenseStateTableData_Type CashDispenseStateTableData; //Tabla con datos provenientes del download.
        PropertiesCashDispenseState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private bool PrinterNotAvailable = false;
        private List<string> ListOfAck = new List<string>();
        private Entities.CashUnitInfo LocalCashUnitInfo;
        private Enums.Commands Fase = Enums.Commands.UNK;

        #region "Constructor"
        public CashDispenseState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "CashDispenseState";
            this.CashDispenseStateTableData = (CashDispenseStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesCashDispenseState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesCashDispenseState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.CashDispenseStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.CashDispenseStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.CashDispenseStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.CashDispenseStateTableData.TimeoutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.CashDispenseStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.CashDispenseStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.CashDispenseStateTableData.Item2;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
        }
        #endregion "Constructor"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public override bool InitializeActivity(Core core)
        {
            bool ret = false;
            try
            {
                this.Core = core;
                Log.Info($"/--> Activity Name: {this.ActivityName}");
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
                this.Fase = Enums.Commands.UNK;
                this.ListOfAck = new List<string>();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");

                if (!DispenseAvailable())
                {
                    Log.Warn("Nothing to dispense.");
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                    return;
                }
                this.AddEventHandlers();
                //this.Core.Bo.ExtraInfo.Amount = 0;
                this.CallHandler(this.prop.OnShowScreen);
                //this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();
                //this.StartTimer(false);
                this.PrinterNotAvailable = false;
                //this.Core.Sdo.CDM_AsyncOpen();
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
                        CDM_OPEN();
                        Log.Warn("VERIFY SENSORS: DISABLED");
                        this.WriteEJ("VERIFY SENSORS: DISABLED");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private bool DispenseAvailable()
        {
            return GetAmount() > 0;
        }

        private void CDM_OPEN()
        {
            this.Core.Sdo.CDM_AsyncOpen();
            this.Fase = Enums.Commands.Open;
        }
        private void CDM_CLOSE()
        {
            this.Core.Sdo.CDM_AsyncClose();
            this.Fase = Enums.Commands.Close;
        }
        private void ExecuteExecuteRetract()
        {
            try
            {
                Log.Debug("/--->");
                this.Fase = Enums.Commands.Retract;
                this.Core.Sdo.CDM_AsyncRetract();
                this.ShowModalAdvice(this.prop.OnRetractCashAdvice);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Returns from SDO"
        /// <summary>
        /// Maneja los retornos tipo --EVENTOS--
        /// </summary>SendContentsToHost
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void HandlerEventReceive(DeviceMessage dm)
        {
            if (dm.Device == Enums.Devices.CashDispenser)
            {
                Log.Info($"/--> Cdm event: {dm?.Payload}");
                if (dm.Payload != null)
                {
                    switch (dm.Payload.ToString())
                    {
                        case "WFS_SRVE_CDM_ITEMSTAKEN":
                            this.StopTimer();//Detengo el timer de More Time | (si se dispara mortime deberiamos accionar un RETRACT por abandono)
                            WriteEJ("Items Taken -OK-");
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber); //dinero tomado, finalizar estado
                            break;
                        case "WFS_ERR_CDM_ITEMSLEFT":
                            WriteEJ("Items Left -WARN-");
                            Log.Trace("Items were left. Retract will be executed..");
                            ExecuteExecuteRetract();
                            break;
                    }
                }          
            }
        }

        /// <summary>
        /// Guarda los RequestID de los ACK recibidos
        /// </summary>
        /// <param name="dm"></param>
        private void HandlerAckReceive(DeviceMessage dm)
        {
            try
            {
                Log.Info($"/-->ACK request ID: {dm.Header.RequestId}");
                this.ListOfAck.Add(dm.Header.RequestId.ToString());
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los retornos tipo --COMPLETION-- asegurando de que se haya recibido previamente un ACK con el mismo RequestID (FIX para desechar mensajes inesperados)
        /// </summary>
        /// <param name="dm"></param>
        private void HandlerCompletionReceive(DeviceMessage dm)
        {

            var match = this.ListOfAck.FirstOrDefault(x => x.Equals(dm.Header.RequestId.ToString()));
            Completion cr = (Completion)dm.Payload;
            if (match != null || cr.CompletionCode == CompletionCodeEnum.TimeOut)
                this.ProcessCompletion(dm);
            else
                Log.Error($"/-->ACK request ID: {dm.Header.RequestId} not found");
        }

        /// <summary>
        /// Procesa los retornos tipo Completion de los dispositivos
        /// </summary>
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void ProcessCompletion(DeviceMessage dm)
        {
            Completion cr;
            try
            {
                Log.Info($"/--> {dm.Device}");
                cr = (Completion)dm.Payload;
                //Logeo resultado
                if (dm.Device == Enums.Devices.IOBoard || dm.Device == Enums.Devices.CashDispenser || dm.Device == Enums.Devices.Printer)
                {
                    Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                }
                if (dm.Device == Enums.Devices.Printer && dm.Command == Enums.Commands.State && cr.CompletionCode == CompletionCodeEnum.Success)
                {
                    CDM_OPEN();
                }
                if (dm.Device == Enums.Devices.CashDispenser)
                {
                    this.StopTimer();//Detengo el timer de More Time
                    switch (dm.Command)//Switcheo respuestas de los comandos
                    {
                        case Enums.Commands.Open:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                this.Core.Sdo.CDM_CashUnitInfo();
                                this.Fase = Enums.Commands.CashUnitInfo;
                                Log.Trace($"CashDispenser open success.");
                            }
                            else
                            {
                                Log.Error($"CashDispenser open failed");
                                this.ChangeDEV_Fitness(Enums.Devices.CashDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.CDM_DeviceError);
                                ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.CDM_OPEN_ERROR);
                            }
                            break;
                        case Enums.Commands.CashUnitInfo:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                LocalCashUnitInfo = Utilities.Utils.JsonDeserialize<CashUnitInfo>(out bool cuiRet, cr.Data);
                                var currency = this.Core.AlephATMAppData.DefaultCurrency;
                                this.Core.Sdo.CDM_AsyncDispense(Convert.ToInt32(GetAmount()), 0, currency);
                                this.Fase = Enums.Commands.Dispense;
                            }
                            else
                            {
                                Log.Error("Failed in {0} execution", dm.Command.ToString());
                                this.ChangeDEV_Fitness(Enums.Devices.CashDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.CDM_DeviceError);
                                ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.CDM_CUI_ERROR);
                            }
                            break;
                        case Enums.Commands.Dispense:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                if (this.Core.Bo.ExtraInfo.CurrentTxn == Enums.AvTxn.cashWithdrawalTx) // is cash withdrawal
                                {
                                    // Authorize cash dispense to host
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    this.NotifyDispense(new List<Detail> { });
                                }
                                else // not cash withdrawal (change dispense, etc)
                                {
                                    WriteEJ("Dispense Notes -OK-");
                                    this.Core.Sdo.CDM_AsyncPresent();
                                    this.Fase = Enums.Commands.Present;
                                }

                                
                            }
                            else
                            {
                                WriteEJ("Dispense Notes -ERROR-");
                                this.ChangeDEV_Fitness(Enums.Devices.CashDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.CDM_DeviceError);
                                CDM_CLOSE();
                                ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.CDM_DISPENSE_ERROR);
                            }
                            break;
                        case Enums.Commands.Present:
                            if (!this.Core.Sdo.DevConf.CDMconfig.KeepConnectionOpen)
                                CDM_CLOSE();
                            if (cr.CompletionCode != CompletionCodeEnum.Success)
                            {
                                WriteEJ("Present Notes -ERROR-");
                                this.ChangeDEV_Fitness(Enums.Devices.CashDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.CDM_DeviceError);
                                ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.CDM_PRESENT_ERROR);
                            }
                            else
                            {
                                WriteEJ("Present Notes -OK-");
                                this.StartTimer();
                                this.prop.OnTakeCash.Parameters = this.Core.Bo.ExtraInfo.AmountToDispenseInNotes;
                                var denomination = Utilities.Utils.JsonDeserialize<DenominationCDM>(out bool retDem, cr.Data);
                                if (retDem)
                                {
                                    var content = UpdateLocalContent(denomination); //update logic counters with the dispensed quantities
                                    if (content is object)
                                        NotifyDispensePRDM(content);
                                    else
                                        Log.Error("Could not update contents or notify movement."); 
                                }
                                else
                                    Log.Error("Could not update contents. A proper DenominationCDM was not received in Dispense response.");
                                this.CallHandler(this.prop.OnTakeCash); //mostrar popup de tomar dinero
                            }
                            break;
                        case Enums.Commands.Cancel:
                            //this.Process_Completion_CDM_Cancel(cr);
                            break;
                        case Enums.Commands.Retract:
                            this.Process_Completion_CDM_Retract(cr);
                            break;
                        case Enums.Commands.Close:
                            this.StartTimer();
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CDM_Retract(Completion cr)
        {
            switch (cr.CompletionCode)
            {
                case CompletionCodeEnum.Success:
                case CompletionCodeEnum.Reject:
                    this.ChangeDEV_Fitness(Enums.Devices.CashDispenser, Const.Fitness.Warning, Enums.DeviceStatus.CDM_RetractSuccess);//Envía el status a host de retract success
                    this.WriteEJ("Retract Notes -OK-");
                    Log.Info("Retract Notes -OK-");
                    //en caso de retract por abandono, no continuar con la transacción, sino cancelar
                    //this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                    RetractExit();
                    break;
                default:
                    this.ChangeDEV_Fitness(Enums.Devices.CashDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.CDM_RetractError);//Envía el status a host de error de retract
                    this.WriteEJ("Retract Notes -ERROR-");
                    Log.Error("Retract Notes -ERROR-");
                    ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.CDM_RETRACT_ERROR);
                    break;
            }
        }

        private void ErrorExit(StateResult errorState, ErrorData.ErrorCodes code, string message = null)
        {
            message = message ?? "Fallo de dispensado";
            this.Core.Bo.ExtraInfo.ErrorCode = new ErrorData { Code = code, Message = message };
            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError, false);
            var stateDic = new Dictionary<StateResult, string> {
                { StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber },
                { StateResult.TIMEOUT, this.prop.TimeoutNextStateNumber },
                { StateResult.CANCEL, this.prop.CancelNextStateNumber },
            };
            this.SetActivityResult(errorState, stateDic[errorState]);
        }

        /*On retract instead of continue to the next state, the operation is canceled and a ticket is sent to Journal */
        private void RetractExit()
        {
            this.prop.OnPrintTicketOnReceiptPrinterError.Action = StateEvent.EventType.printJournal; //we only want to print on journal
            this.Core.Bo.ExtraInfo.ErrorCode = new ErrorData { Code = ErrorData.ErrorCodes.CDM_RETRACT_HAPPENED, Message = "Retract execution" };
            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError, false);
            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
        }

        #endregion "Returns from SDO"


        /// <summary>
        /// Gets amount if it was specified in "DispenseContent" as a Contents object in case it was manually calculated.
        /// Otherwise its retreived from Core.Bo.ExtraInfo.Amount
        /// </summary>
        /// <returns></returns>
        private decimal GetAmount()
        {
            //if (this.Core.Bo.ExtraInfo.AmountToDispenseInNotes != 0)
            //{
            Log.Trace("Amount to dispense in NOTES is: {0}", this.Core.Bo.ExtraInfo.AmountToDispenseInNotes);
            return this.Core.Bo.ExtraInfo.AmountToDispenseInNotes;
            //}
            //return this.Core.Bo.ExtraInfo.Amount;
        }

        private void NotifyDispense(List<Entities.Detail> details)
        {
            Log.Debug("/--->");
            StringBuilder sb = new StringBuilder();
            AuthorizationStatus authRes = AuthorizationStatus.UnavailableService;
            try
            {
                new Thread(new ParameterizedThreadStart((object obj) =>
                {
                    Contents contents = obj as Contents;
                    try
                    {
                        var notif = this.Core.AuthorizeTransaction(Enums.TransactionType.DISPENSE, contents, this.prop.HostName);
                        authRes = notif.authorizationStatus;
                        if (authRes == AuthorizationStatus.Authorized)
                        {
                            WriteEJ("Dispense Notes -OK-");
                            this.Core.HideScreenModals();
                            this.Core.Sdo.CDM_AsyncPresent();
                            this.Fase = Enums.Commands.Present;
                            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter1, false); //Print ticket ok
                            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter2, false);
                            this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, false);
                            this.ProcessPrinterData(this.prop.OnSendTicketToBD, false);
                        }
                        else
                        {
                            Log.Error("Could not get auth from host.");
                            CDM_CLOSE();
                            ErrorExit(StateResult.CANCEL, ErrorData.ErrorCodes.NoError);
                        }
                    }
                    catch (Exception ex) { Log.Fatal(ex); }
                }))
                    .Start(new Contents { LstDetail = details });
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void NotifyDispensePRDM(List<Entities.Detail> details)
        {
            Log.Debug("/--->");
            StringBuilder sb = new StringBuilder();
            try
            {
                new Thread(new ParameterizedThreadStart((object obj) =>
                {
                    Contents contents = obj as Contents;
                    try
                    {
                        var notif = this.Core.AuthorizeTransaction(Enums.TransactionType.SENDCASHOUT, contents, this.prop.HostName);
                        if (notif.authorizationStatus != AuthorizationStatus.Authorized)
                            Log.Warn(notif.authorizationStatus);
                    }
                    catch (Exception ex) { Log.Fatal(ex); }
                }))
                    .Start(new Contents { LstDetail = details });
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void SendContents()
        {
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            try
            {
                Log.Debug("/--->");
                authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.SEND_CONTENTS, this.Core.Counters.Contents, this.prop.HostName);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerFDKreturn(string FDKcode) //Todas las FDK cancelan la operación
        {
            try
            {
                //this.Core.Sdo.BAR_StopScanBarcode(); //Apago el barcode
                this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.StopTimer();
                this.Quit();
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AddEventHandlers()
        {
            this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive += new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive += new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive -= new SDO.DelegateAckReceive(this.HandlerAckReceive);
            if(this.timerScreen != null)
            {
                this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);
            }
        }

        #region "Functions"


        /*
         * Gets counter Detail from Denomination received from dispense command response 
         * to use to update logic counters
         */
        private Detail GetDetailsFromDenomination(DenominationCDM denominationCDM)
        {
            if (LocalCashUnitInfo is null)
            {
                Log.Error("LocalCashUnitInfo was null");
                return null;
            }
            if (LocalCashUnitInfo.LstCashUnit is null)
            {
                Log.Error("LocalCashUnitInfo.LstCashUnit was null");
                return null;
            }
            if (denominationCDM.Values is null)
            {
                Log.Error("denominationCDM.Values was null");
                return null;
            }
            if (LocalCashUnitInfo.LstCashUnit.Count != denominationCDM.Values.Count)
            {
                Log.Error("Received CDM Denomination and LocalCashUnitInfo don't have same quantity of items.");
                return null;
            }
            var detail = new Detail
            {
                LstItems = new List<Item>(),
                ContainerId = Detail.ContainerIDType.CashAcceptor,
                ContainerType = "NOTEACCEPTOR",
                Currency = denominationCDM.CurrencyID
            };
            if (LocalCashUnitInfo.LstCashUnit.Count != denominationCDM.Values.Count)
            {
                Log.Error("The quantity of local cash units (received from cash unit info command) differs from the quantity of units received in CDM dispense command response.");
                return null;
            }

            var currentCashDetail = this.Core.Counters.Contents.LstDetail.FirstOrDefault(c => c.Currency.Equals(this.Core.Bo.ExtraInfo.Currency) && c.ContainerId == Detail.ContainerIDType.CashAcceptor); //obtener el detail CashAcceptor que meneje el currency
            if (currentCashDetail is null)
                Log.Error("Could not find Detail for the provided currency {0}", this.Core.Bo.ExtraInfo.Currency);

            Log.Trace("currentCashDetail es: {0}", Utilities.Utils.JsonSerialize(currentCashDetail));

            for (int i = 0; i < denominationCDM.Values.Count; i++)
            {
                if (denominationCDM.Values[i] == 0) //ignore values that has not been dispensed 
                    continue;

                var cUnit = LocalCashUnitInfo.LstCashUnit[i]; //.FirstOrDefault(c => c.Number == (i+1)); //obtener el cashUnit segun indice de denomination value recibido del dispense command
                if (cUnit is null)
                    Log.Error("Could not find CashUnit for the provided CDMDenomination index {0}", i);

                if (cUnit.Values == 0) //ignore units not used for single note values
                    continue;

                var item = currentCashDetail.LstItems.FirstOrDefault(itm => MixCalculator.ShifftDot(itm.Denomination, itm.Exponent).ToString().Equals(cUnit.Values.ToString())); //obtener el item para esa denominacion
                if (item is null)
                    Log.Error("Could not find Detail.Item for the provided denomination value {0}", cUnit.Values);
                detail.LstItems.Add(new Item
                {
                    Denomination = item.Denomination,
                    Type = item.Type,
                    Exponent = item.Exponent,
                    Num_Items = denominationCDM.Values[i],  //indicar la cantidad dispensada para esa denominacion
                    Total = denominationCDM.Values[i] * item.Denomination
                });
            }
            return detail;
        }

        private bool UpdateDetails(List<Detail> details)
        {
            var sb = new StringBuilder();
            details?.ForEach(dt => {
                sb.Append($"\nCurrency {dt.Currency}");
                dt.LstItems?.ForEach(itm =>
                {
                    sb.Append($"\n\tDenomination {itm.Denomination / 100} | Num of Items: {itm.Num_Items}");
                });
            });
            this.WriteEJ("Dispensed values: \n" + sb.ToString());
            return this.Core.Counters.UpdateContents(details, Counters.TransactionType.DISPENSE);
        }

        private List<Detail> UpdateLocalContent(DenominationCDM denominationCDM)
        {
            var data = new List<Detail> { GetDetailsFromDenomination(denominationCDM) };
            Log.Trace("Executing UpdateContents with DATA: " + Utilities.Utils.JsonSerialize(data));
            return UpdateDetails(data) ? data : null;
        }


        #endregion ""

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
            this.StartTimer();
        }
        #endregion "sensor handler"

        #region "Take cash timeout"

        // Timeout
        public System.Timers.Timer timerScreen;

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer()
        {
            Log.Debug($"/--->StartTimer");
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.TakeCashTimeout * 1000;
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
            Log.Debug("/--->");
            if (timerScreen != null)
            {
                this.timerScreen.Enabled = false;
                this.timerScreen.Stop();
            }
        }

        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.StopTimer();
            this.RemoveEventHandlers();
            switch (this.Fase)
            {
                case Enums.Commands.Present:
                    ExecuteExecuteRetract();
                    break;
                default:
                    this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeoutNextStateNumber);
                    break;
            }
        }
        #endregion "Take cash timeout"
    }
}
