using System;
using System.Threading;
using System.Threading.Tasks;
using Entities;

namespace Business.EnvelopeDispenserState
{
    public class EnvelopeDispenserState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        EnvelopeDispenserStateTableData_Type envelopeDispenserStateTableData; //Tabla con datos provenientes del download.
        PropertiesEnvelopeDispenserState prop;
        string amount = "0";
        bool ret = true;
        bool flagHome = false;
        private bool MoreTimeSubscribed = false;

        #region "Constructor"
        public EnvelopeDispenserState(StateTable_Type stateTable)
        {
            this.ActivityName = "EnvelopeDispenserState";
            this.envelopeDispenserStateTableData = (EnvelopeDispenserStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesEnvelopeDispenserState();
            this.prop = this.GetProperties<PropertiesEnvelopeDispenserState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.envelopeDispenserStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.envelopeDispenserStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.envelopeDispenserStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.ErrorNextStateNumber))
                    this.prop.ErrorNextStateNumber = this.envelopeDispenserStateTableData.ErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.envelopeDispenserStateTableData.TimeoutNextStateNumber;
                if (this.prop.EnvelopeOperationMode == EnvelopeOperationMode_Type.none)
                    this.prop.EnvelopeOperationMode = this.envelopeDispenserStateTableData.EnvelopeOperationMode;
                if (this.prop.Item == null)
                    this.prop.Item = this.envelopeDispenserStateTableData.Item;
                if (this.prop.Item1 == null)
                    this.prop.Item1 = this.envelopeDispenserStateTableData.Item1;
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
                this.flagHome = true;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
                this.Core.Sdo.EvtEventReceive += new SDO.DelegateEventReceive(this.HandlerEventReceive);
                if (this.CallHandler(this.prop.OnShowEnvelopeScreen))
                {
                    this.StartTimer();
                }
                else
                    this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        private void HandlerInputData(string dataInput, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", dataInput);
                this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "DispenseEnvelope":
                            {
                                var task = Task.Run(async delegate
                                {
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    await Task.Delay(5000);
                                    this.Core.HideScreenModals();
                                    await Task.Delay(1000);
                                    this.CallHandler(this.prop.OnTakeEnvelopeAdvice);
                                    await Task.Delay(10000);
                                    this.CallHandler(this.prop.OnClearModals);
                                });
                                //this.Core.Sdo.ADM_Open();
                                break;
                            }
                        case "LoadEnvelope":
                            {
                                var task = Task.Run(async delegate
                                {
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    await Task.Delay(5000);
                                    this.Core.HideScreenModals();
                                    await Task.Delay(1000);
                                    this.CallHandler(this.prop.OnInsertEnvelopeAdvice);
                                    await Task.Delay(10000);
                                    this.CallHandler(this.prop.OnClearModals);
                                });
                                //this.Core.Sdo.ADM_Open();
                                break;
                            }
                    }
                }
                this.timerScreen.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Returns from SDO"
            //DFS_EVT_ADM_TAKEENVELOPE = 1,
            //DFS_EVT_ADM_FRONTALDOOROPEN = 2,
            //DFS_EVT_ADM_FRONTALDOORCLOSE = 3,
            //DFS_EVT_ADM_PRESENTERDOOROPEN = 4,
            //DFS_EVT_ADM_PRESENTERDOORCLOS = 5,
            //DFS_EVT_ADM_REARDOOROPEN = 6,
            //DFS_EVT_ADM_REARDOORCLOSE = 7,
            //DFS_EVT_ADM_FRAUDATTEMPT = 8,
            //DFS_EVT_ADM_DEVOFFLINE = 9,
            //DFS_EVT_ADM_STARTRETRACT = 10,
            //DFS_EVT_ADM_ENDTRETRACT = 11
        /// <summary>
        /// Maneja los retornos tipo Eventos
        /// </summary>SendContentsToHost
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void HandlerEventReceive(DeviceMessage dm)
        {
            if (dm.Device == Enums.Devices.ADM)
            {
                this.StopTimer();//Detengo el timer de More Time
                Log.Info("/--> ADM event: {0}", dm.Payload.ToString());
                if(dm.Payload.ToString().Equals("DFS_EVT_ADM_TAKEENVELOPE"))
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                else
                    this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
            }
        }

        /// <summary>
        /// Maneja los retornos tipo Completion de los dispositivos
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
                if (dm.Device == Enums.Devices.IOBoard || dm.Device == Enums.Devices.ADM || dm.Device == Enums.Devices.Printer)
                {
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                        Log.Info("Dev: {0} Func: {1} Result: {2}", dm.Device, dm.Command, cr.CompletionCode);
                    else
                        Log.Warn(string.Format("Dev: {0} Func: {1} Result: {2}", dm.Device, dm.Command, cr.CompletionCode));
                }
                if (dm.Device == Enums.Devices.ADM)
                {
                    switch(dm.Command)
                    {
                        case Enums.Commands.Open:
                            {
                                if (cr.CompletionCode == CompletionCodeEnum.Success)
                                {
                                    this.WriteEJ(string.Format("Dispense envelope pos: {0}", this.Core.Bo.ExtraInfo.Currency));
                                    this.Core.Sdo.ADM_Dispense(this.Core.Bo.ExtraInfo.Currency);
                                }
                                else
                                    this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                                break;
                            }
                        case Enums.Commands.Dispense:
                            {
                                if (cr.CompletionCode != CompletionCodeEnum.Success)
                                {
                                    if(this.flagHome)
                                        this.Core.Sdo.ADM_Home();
                                    else
                                        this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                                }
                                break;
                            }
                        case Enums.Commands.Home:
                            {
                                if (cr.CompletionCode == CompletionCodeEnum.Success)
                                {
                                    this.flagHome = false;
                                    this.Core.Sdo.ADM_Dispense(this.Core.Bo.ExtraInfo.Currency);
                                }
                                else
                                    this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ProcessADMCompletion(Completion cr)
        {
            try
            {

            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #endregion "Returns from SDO"

        public void AuthorizeTransaction(object obj)
        {
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            Contents contents = obj as Contents;
            try
            {
                Log.Debug("/--->");
                authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.GET_ACCOUNTS, contents, this.prop.HostName);
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                {
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                }
                else
                {
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.CancelNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info("/--> Key press: {0}", othersKeys);
            switch(othersKeys)
            {
                case "ENTER": //Confirma TX
                    {
                        //this.Core.Bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                        break;
                    }
                case "CANCEL":
                    {
                        this.Core.Bo.ExtraInfo.Amount = 0;
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
            }
        }

        private void HandlerFDKreturn(string FDKcode)
        {
            try
            {
                Log.Info("-> FDK data: {0}", FDKcode);
                this.Core.Bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
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
                this.WriteEJ(string.Format("State result of {0}: {1}", this.ActivityName, result.ToString()));
                this.Core.SetNextState(result, nextState);
                this.Core.Sdo.ADM_Close();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
                this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Functions"



        #endregion ""

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
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.ActivityStart();
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeoutNextStateNumber);
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
            else if (!this.MoreTimeSubscribed) this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);

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
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }
}
