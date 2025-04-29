using Entities;
using System;

namespace Business.PINEntryState
{
    public class PINEntryState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        PINEntryStateTableData_Type pINEntryStateTableData; //Tabla con datos provenientes del download.
        PropertiesPINEntryState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;

        #region "Constructor"
        public PINEntryState(StateTable_Type stateTable)
        {
            this.ActivityName = "PINEntryState";
            this.pINEntryStateTableData = (PINEntryStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesPINEntryState();
            this.prop = this.GetProperties<PropertiesPINEntryState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.pINEntryStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.pINEntryStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.pINEntryStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.LocalPINCheckGoodPINNextStateNumber))
                    this.prop.LocalPINCheckGoodPINNextStateNumber = this.pINEntryStateTableData.LocalPINCheckGoodPINNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.LocalPINCheckMaximumBadPINNextStateNumber))
                    this.prop.LocalPINCheckMaximumBadPINNextStateNumber = this.pINEntryStateTableData.LocalPINCheckMaximumBadPINNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.LocalPINCheckErrorScreenNumber))
                    this.prop.LocalPINCheckErrorScreenNumber = this.pINEntryStateTableData.LocalPINCheckErrorScreenNumber;
                if (string.IsNullOrEmpty(this.prop.RemotePINCheckNextStateNumber))
                    this.prop.RemotePINCheckNextStateNumber = this.pINEntryStateTableData.RemotePINCheckNextStateNumber;
                if (this.prop.LocalPINCheckMaximumPINRetries == 0)
                    this.prop.LocalPINCheckMaximumPINRetries = this.pINEntryStateTableData.LocalPINCheckMaximumPINRetries;
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
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                this.Core.Bo.ExtraInfo.NewPinBlock = string.Empty;
                if (this.Core.ScreenConfiguration.KeyboardEntryMode != KeyboardEntryMode_Type.none)
                    this.prop.ActiveFDKs = new KeyMask_Type();
                if (this.Core.ShowPinEntryNDCScreen(this.prop.ScreenNumber, this.prop.ActiveFDKs, this.Core.Bo.ExtraInfo.NewTrack1))
                    this.StartTimer();
                else
                    this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerInputData(string keyCode, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", keyCode);
                this.timerScreen.Stop();
                this.Core.Bo.ExtraInfo.NewPinBlock = keyCode;
                this.timerScreen.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info("/--> Key press: {0}", othersKeys);
            switch (othersKeys)
            {
                case "ENTER": //Confirma TX
                    {
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.LocalPINCheckGoodPINNextStateNumber);
                        break;
                    }
                case "CANCEL":
                    {
                        this.Core.Bo.ExtraInfo.NewPinBlock = string.Empty;
                        this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                        break;
                    }
            }
        }

        private void HandleFDKreturn(string FDKcode)
        {
            try
            {
                Log.Info("-> FDK data: {0}", FDKcode);
                this.SetActivityResult(StateResult.SUCCESS, this.prop.LocalPINCheckGoodPINNextStateNumber); //PIN OK 
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
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
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
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeOutNextStateNumber);
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
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }
}
