using Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Business.MultiCashAcceptState
{
    public class MultiCashAcceptState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        MultiCashAcceptStateTableData_Type multiCashAcceptStateTableData; //Tabla con datos provenientes del download.
        PropertiesMultiCashAcceptState prop;
        Printers.PrintFormat pf;
        bool ret = false;
        private bool AutoDepositEnable = false;
        private bool DepositErrorEnable = false;
        private bool RejectNotesEnable = false;
        private bool MoreTimeSubscribed = false;
        private bool AuthInProgress = false;

        #region "Constructor"
        public MultiCashAcceptState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "MultiCashAcceptState";
            this.multiCashAcceptStateTableData = (MultiCashAcceptStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesMultiCashAcceptState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesMultiCashAcceptState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.multiCashAcceptStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.EnterNextStateNumber))
                    this.prop.EnterNextStateNumber = this.multiCashAcceptStateTableData.EnterNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.multiCashAcceptStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.multiCashAcceptStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.MoreDepositNextStateNumber))
                    this.prop.MoreDepositNextStateNumber = this.multiCashAcceptStateTableData.MoreDepositNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.MaximumNumberOfDeposit))
                    this.prop.MaximumNumberOfDeposit = this.multiCashAcceptStateTableData.MaximumNumberOfDeposit;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.EnablePrintProperties = this.prop.PrintPropertiesEnable;
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
                Log.Info("/--> Activity Name: {0}", this.ActivityName);
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
                this.AutoDepositEnable = false;
                this.RejectNotesEnable = false;
                this.DepositErrorEnable = false;
                this.AuthInProgress = false;
                this.AddEventHandlers();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.pf = new Printers.PrintFormat(this.Core);
                this.ShowMultiCashDepositScreen();
                if (this.prop.AutoDeposit)
                {
                    this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
                    this.Core.Sdo.CIM_AsyncOpen();
                    this.CallHandler(this.prop.OnPleaseWait);
                }
                else
                    this.ShowDepositedNotes();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los retornos tipo Completion
        /// </summary>
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void HandlerCompletionReceive(DeviceMessage dm)
        {
            Completion cr;
            try
            {
                this.StopTimer();//Detengo el timer de More Time
                Log.Info("/--> {0}", dm.Device);
                cr = (Completion)dm.Payload;
                //Logeo resultado
                if (dm.Device == Enums.Devices.CashAcceptor)
                {
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                        Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                    else
                        Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                    switch (dm.Command)
                    {
                        case Enums.Commands.Open: //Retornos del OPEN
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                                this.Core.Sdo.CIM_Status();
                            break;
                        case Enums.Commands.Status: //Respuesta al comando de pedido de STATUS
                            bool ret1 = false;
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                cr = dm.Payload as Completion;
                                StatusCIM statusCIM = Utilities.Utils.JsonDeserialize<StatusCIM>(out ret1, cr.Data);
                                if (ret1)
                                {
                                    if (statusCIM.IntermediateStacker.Equals("0"))//Verifica si hay valores en escrow
                                    {
                                        if (statusCIM.Device.Equals("0"))//Verifico si el dispositivo esta ok
                                        {
                                            if (statusCIM.Pos[1].PositionStatus.Equals("1") && this.prop.RejectDetect)//Billetes presentes en la bandeja de rechazo
                                            {
                                                this.RejectNotesEnable = true;
                                                Log.Warn(string.Format("Reject notes detected.."));
                                            }
                                            if (statusCIM.Pos[0].PositionStatus.Equals("1"))//Billetes presentes en la bandeja de entrada
                                            {
                                                int limit = 0;
                                                if (int.TryParse(this.prop.MaximumNumberOfDeposit, out limit))
                                                    if (this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count < limit)
                                                        this.AutoDepositEnable = true;
                                            }
                                        }
                                        else
                                        {
                                            this.DepositErrorEnable = true;
                                            Log.Warn("Hardware error detected..");
                                        }
                                    }
                                    else
                                    {
                                        this.DepositErrorEnable = true;
                                        Log.Warn("Notes in escrow detected..");
                                    }
                                }
                            }
                            else
                            {
                                this.DepositErrorEnable = true;
                                Log.Warn($"Status CIM error: {cr.CompletionCode.ToString()}");
                            }
                            this.Core.Sdo.CIM_AsyncClose();
                            break;
                        case Enums.Commands.Close:
                            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
                            Log.Info("DepositErrorEnable... {0}", this.DepositErrorEnable);//Hadware error detectado en Multicash state
                            Log.Info("DepositHardwareError... {0}", this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError);//Hadware error detectado en CashAccept state "E8"
                            Log.Info("DepositAtError... {0}", this.prop.DepositAtError);//Bandera de activación de manejo de deposito con error de HW
                            Log.Info("ListPartialDepositCount... {0}", this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count);
                            if (this.DepositErrorEnable &&
                                ((this.prop.DepositAtError && this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count != 0) || this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError))
                            {
                                this.PersistDeposit("hw-error");
                            }
                            else
                            {
                                if (this.prop.AutoDeposit)
                                {
                                    if (this.RejectNotesEnable)
                                    {
                                        this.RejectNotesEnable = false;
                                        this.CallHandler(this.prop.OnReturningCashAdvice);
                                    }
                                    if (this.AutoDepositEnable)
                                    {
                                        Log.Info("AutoDeposit in progress... ");
                                        this.WriteEJ("Auto deposit -MultiCash-");
                                        this.SetActivityResult(StateResult.SUCCESS, this.prop.MoreDepositNextStateNumber);
                                    }
                                    else
                                        this.ShowDepositedNotes();
                                }
                                else
                                    this.ShowDepositedNotes();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ShowDepositedNotes()
        {
            Log.Info("/-->");
            int limit = 0;
            try
            {

                if (this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count != 0)
                {
                    if (this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError)
                    {
                        this.PersistDeposit("hw-error");
                    }
                    else if (this.prop.DepositWithoutEscrowFull && !this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositEscrowFull)
                    {
                        this.PersistDeposit("depositWithoutEscrowFull"); //Persiste automáticamente (sin confirmación de usuario)
                    }
                    else
                    {
                        this.StartTimer();
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        if (int.TryParse(this.prop.MaximumNumberOfDeposit, out limit))
                        {
                            if (this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count < limit)
                                this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.MoreAvailable = true;
                            else
                                this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.MoreAvailable = false;
                        }
                        else
                        {
                            Log.Error("DepositMaxQuantity isnot numeric.");
                            this.PersistDeposit("error");
                            this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                        }
                        if (this.prop.OnMultiCashDepositSummary.Action == StateEvent.EventType.runScript)//Muestro el detalle de lotes ingresados
                            this.prop.OnMultiCashDepositSummary.Parameters = Utilities.Utils.JsonSerialize(this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit);
                        if (!this.CallHandler(this.prop.OnMultiCashDepositSummary))
                        {
                            Log.Error($"Can´t show screen: {this.prop.OnMultiCashDepositSummary.HandlerName}");
                            this.PersistDeposit("error");
                            this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                        }
                    }
                }
                else//Si no hay depósitos para mostrar cierro la transacción
                {
                    Log.Warn("Canceling transacction. Without deposits");
                    this.SetActivityResult(StateResult.CANCEL, this.prop.EnterNextStateNumber);
                }

            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja el retorno de las teclas No FDK presionadas por el usuario
        /// </summary>
        /// <param name="othersKeys"></param>
        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info("/--> Key press: {0}", othersKeys);
            this.WriteEJ(string.Format("Key press: {0}", othersKeys));
            this.StopTimer();
            switch (othersKeys)
            {
                case "ENTER": //Confirma TX
                    {
                        this.PersistDeposit("continue");
                        break;
                    }
                case "ANOTHER_DEPOSIT": //Agregar más depósitos
                    {
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.MoreDepositNextStateNumber);
                        break;
                    }
            }
        }

        private void PersistDeposit(string type)
        {
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            //List<Bills> bills = new List<Bills>();
            List<Detail> lstDetail = new List<Detail>();
            List<Item> lstItems = new List<Item>();
            List<Bills> lstBills = new List<Bills>();
            Contents contents;
            StringBuilder sb = new StringBuilder();
            decimal updValue = 0;
            try
            {
                Log.Debug("Result: {0}", type);
                if (!this.AuthInProgress)
                {
                    this.AuthInProgress = true; //Para prevenir más de un disparo
                    Log.Info("DepositHardwareErrorDetected: {0}", this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError);//Hadware error detectado en CashAccept state "E8"
                    if (this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count != 0)
                    {
                        //A)- Obtengo los datos de los billetes depositados
                        foreach (CashInInfo ci in this.Core.Bo.ExtraInfo.CashInMultiCashData.ListCashInInfo)
                        {
                            foreach (Bills b in ci.Bills)
                            {
                                currencies.Add(b.Currency);//Cargo todos los currencies
                                Bills bills = lstBills.Find(x => x.Value == b.Value && x.Currency.Equals(b.Currency));//Solo trae los Items Acceptor del mismo currency
                                if (bills != null)//TRUE: Ya exixte un Detail de dinero validado con el mismo currency entrante
                                {
                                    bills.Quantity += b.Quantity;
                                }
                                else
                                {
                                    lstBills.Add(b);
                                }
                            }
                        }
                        distinctCurrencies = currencies.Distinct().ToArray();
                        for (int k = 0; k < distinctCurrencies.Length; k++)
                        {
                            lstItems = new List<Item>();
                            sb = new StringBuilder();
                            foreach (Bills b in lstBills)
                            {
                                if (b.Currency.Equals(distinctCurrencies[k]))
                                {
                                    updValue = (decimal)(b.Value * 100);
                                    sb.Append($"{Environment.NewLine}--> ID: {b.Id.ToString().PadLeft(3, ' ')} - CUR: {b.Currency} - QTY: {b.Quantity.ToString().PadLeft(3, ' ')} - VAL: {Utilities.Utils.FormatCurrency(b.Value, b.Currency, 4)} - NDC: {b.NDCNoteID}");
                                    lstItems.Add(new Item(updValue, (decimal)b.Quantity, (decimal)(updValue * b.Quantity), "NOTE", "", ""));
                                }
                            }
                            Log.Info($"Currency detail: {distinctCurrencies[k]}");
                            Log.Info(sb.ToString());
                            Detail detail = new Detail(distinctCurrencies[k], Detail.ContainerIDType.CashAcceptor, "NOTEACCEPTOR", this.Core.GetCollectionId(Enums.TransactionType.DEPOSIT), lstItems);
                            lstDetail.Add(detail);
                        }
                        contents = new Contents(lstDetail);
                        //B)- Send transaction message to host
                        this.CallHandler(this.prop.OnPleaseWait);
                        Thread prtWndThd;
                        prtWndThd = new Thread(new ParameterizedThreadStart(this.AuthorizeTransaction));
                        prtWndThd.Start(contents);
                    }
                    else//Si no hay depósitos para mostrar cierro la transacción
                    {
                        Log.Warn("Canceling transacction. Without deposits");
                        this.SetActivityResult(StateResult.CANCEL, this.prop.EnterNextStateNumber);
                    }
                }
                else
                    Log.Warn($"Transaction authorization already in progress");
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
                string filePath = $"{Const.appPath}Retrieval\\RetrievalTransaction.xml";
                if (this.Core.AlephATMAppData.RetrievalTransactionEnable && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                if (this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError && this.prop.SendDepositStatus) //Extra data E8
                {
                    if (this.Core.Bo.ExtraInfo.ExtraData == null)
                        this.Core.Bo.ExtraInfo.ExtraData = new List<ExtraData>();
                    ExtraData ed = this.Core.Bo.ExtraInfo.ExtraData.FirstOrDefault(x => x.ExtraDataType == Enums.ExtraDataType.txInfo);
                    if (ed != null)
                        ed.TagValue = $"{ed.TagValue} - E8";
                    else
                        this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData(Enums.ExtraDataType.txInfo, "TransactionInfo", "E8"));
                    this.Core.AddHostExtraData("extraData", this.Core.Bo.ExtraInfo.ExtraData);
                }
                //Send to host
                authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.DEPOSIT, contents, this.prop.HostName);
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                {
                    //Print ticket ok
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter1, false);
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter2, false);
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, false);
                    this.ProcessPrinterData(this.prop.OnSendTicketToBD, false);
                }
                else
                {
                    //Print ticket host error
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError1, false);
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError2, false);
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinterError, false);
                    this.ProcessPrinterData(this.prop.OnSendTicketToBDError, false);
                }
                //Aumento el número de transacción
                this.Core.Counters.UpdateTSN();
                this.Core.Bo.ExtraInfo.CashInMultiCashData = new CashInMultiCashData();//Borro datos 
                this.SetActivityResult(StateResult.SUCCESS, this.prop.EnterNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ShowMultiCashDepositScreen()
        {
            if (this.prop.OnMultiCashDeposit.Action == StateEvent.EventType.ndcScreen)
                this.prop.OnMultiCashDeposit.HandlerName = this.prop.ScreenNumber;
            this.CallHandler(this.prop.OnMultiCashDeposit);
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
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "More time"
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
            this.moreTime = new MoreTime(prop.MoreTime.MoreTimeScreenName, prop.MoreTime.MaxTimeOut,
                prop.MoreTime.MaxTimeOutRetries, prop.MoreTime.MoreTimeKeyboardEnabled, this.Core, false, this.ActivityName);
            this.moreTime.EvtMoreTime += new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
        }

        private void AnalyzeMoreTimeResult(MoreTimeResult result)
        {
            this.AddEventHandlers();
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        this.StartTimer();
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.PersistDeposit("cancel");
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        this.PersistDeposit("timeOut");
                        break;
                    }
            }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer()
        {
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.MoreTime.MaxTimeOut * 1000;
            this.SubscribeMoreTime(true);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            timeout = false;
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
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

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timeout = true;
            this.StopTimer();
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
        }

        private void AddEventHandlers()
        {
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            //this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
        }
        private void RemoveEventHandlers()
        {
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            //this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
        }
        #endregion "More time"
    }
}
