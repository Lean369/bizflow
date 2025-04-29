using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Diagnostics;
using Entities;

namespace Business.FourFDKSelectionFunctionState
{
    public class FourFDKSelectionFunctionState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        FourFDKSelectionFunctionStateTableData_Type fourFDKSelectionFunctionStateTableData; //Tabla con datos provenientes del download.
        PropertiesFourFDKSelectionFunctionState prop;
        bool ret = false;

        #region "Constructor"
        public FourFDKSelectionFunctionState(StateTable_Type stateTable)
        {
            this.ActivityName = "FourFDKSelectionFunctionState";
            this.fourFDKSelectionFunctionStateTableData = (FourFDKSelectionFunctionStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesFourFDKSelectionFunctionState();
            this.prop = this.GetProperties<PropertiesFourFDKSelectionFunctionState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.fourFDKSelectionFunctionStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.fourFDKSelectionFunctionStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.fourFDKSelectionFunctionStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKAorINextStateNumber))
                    this.prop.FDKAorINextStateNumber = this.fourFDKSelectionFunctionStateTableData.FDKAorINextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKBorHNextStateNumber))
                    this.prop.FDKBorHNextStateNumber = this.fourFDKSelectionFunctionStateTableData.FDKBorHNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKCorGNextStateNumber))
                    this.prop.FDKCorGNextStateNumber = this.fourFDKSelectionFunctionStateTableData.FDKCorGNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKDorFNextStateNumber))
                    this.prop.FDKDorFNextStateNumber = this.fourFDKSelectionFunctionStateTableData.FDKDorFNextStateNumber;
                if (this.prop.OperationCodeBufferEntryNumber == OperationCodeBufferEntryNumberChar3_Type.Item0)
                    this.prop.OperationCodeBufferEntryNumber = this.fourFDKSelectionFunctionStateTableData.OperationCodeBufferEntryNumber;
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
            KeyMask_Type keyMask;
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                keyMask = new KeyMask_Type();
                keyMask.FDKA = this.prop.FDKAorINextStateNumber.Equals("255") ? false : true;
                keyMask.FDKB = this.prop.FDKBorHNextStateNumber.Equals("255") ? false : true;
                keyMask.FDKC = this.prop.FDKCorGNextStateNumber.Equals("255") ? false : true;
                keyMask.FDKD = this.prop.FDKDorFNextStateNumber.Equals("255") ? false : true;
                this.CallHandler(this.prop.OnShowScreen);
                this.StartTimer();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandleOthersKeysReturn(string othersKeys)
        {
            Log.Debug("/--->");
            //TODO: No se activa el enter y cancel
            switch (othersKeys)
            {
                case "ENTER": //Confirma TX
                    {
                        //this.Core.bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
                        //this.SetActivityResult(0, this.prop.FDKAorINextStateNumber);
                        break;
                    }
                case "CANCEL":
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
            }
        }

        private void HandleFDKreturn(string FDKcode)
        {
            try
            {
                Log.Info("-> FDK data: {0}", FDKcode);
                this.CheckFdkAndSetNextState();
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
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Functions"

        private void CheckFdkAndSetNextState()
        {
            string lastFDKPressed, nextState = string.Empty;
            Log.Debug("/--->");
            switch (lastFDKPressed = this.Core.Bo.LastFDKPressed)
            {
                case "A":
                    nextState = this.prop.FDKAorINextStateNumber;
                    break;
                case "B":
                    nextState = this.prop.FDKBorHNextStateNumber;
                    break;
                case "C":
                    nextState = this.prop.FDKCorGNextStateNumber;
                    break;
                case "D":
                    nextState = this.prop.FDKDorFNextStateNumber;
                    break;
            }
            if (this.FourFDK_SetOPCode(lastFDKPressed) == 0)
            {
                this.SetActivityResult(StateResult.SUCCESS, nextState);
            }
            else
            {
                this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
            }
        }

        private int FourFDK_SetOPCode(string fdkPressed)
        {
            try
            {
                Log.Debug("/--->");
                char[] array = this.Core.Bo.ExtraInfo.OperationCodeData.ToCharArray();
                int num = (int)this.prop.OperationCodeBufferEntryNumber - 1;
                array[num] = Convert.ToChar(fdkPressed);
                string operationCodeData = new string(array);
                this.Core.Bo.ExtraInfo.OperationCodeData = operationCodeData;
                Log.Info("Set Key: {0} - Pos: {1}", fdkPressed, num + 1);
                //this.Journal(this.prop.Journal.);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return -1;
            }
            return 0;
        }

        #endregion "Functions"

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
            this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
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
                this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
                this.timerScreen.Enabled = false;
                this.timerScreen.Stop();
            }
        }

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void timerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timeout = true;
            this.StopTimer();
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }
}
