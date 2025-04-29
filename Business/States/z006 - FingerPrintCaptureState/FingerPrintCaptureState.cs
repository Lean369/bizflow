using Entities;
using System;

namespace Business.FingerPrintCaptureState
{
    public class FingerPrintCaptureState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        FingerPrintCaptureStateTableData_Type FingerPrintCaptureStateTableData; //Tabla con datos provenientes del download.
        PropertiesFingerPrintCaptureState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;

        #region "Constructor"
        public FingerPrintCaptureState(StateTable_Type stateTable)
        {
            this.ActivityName = "FingerPrintCaptureState";
            this.FingerPrintCaptureStateTableData = (FingerPrintCaptureStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesFingerPrintCaptureState();
            FingerPrintCaptureStateTableExtension1_Type extensionTable1 = null;
            FingerPrintCaptureStateTableExtension2_Type extensionTable2 = null;
            this.prop = this.GetProperties<PropertiesFingerPrintCaptureState>(out ret, this.prop);
            if (ret)
            {
                if (this.FingerPrintCaptureStateTableData.Item != null)
                    extensionTable1 = (FingerPrintCaptureStateTableExtension1_Type)this.FingerPrintCaptureStateTableData.Item;
                if (this.FingerPrintCaptureStateTableData.Item1 != null)
                    extensionTable2 = (FingerPrintCaptureStateTableExtension2_Type)this.FingerPrintCaptureStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.PlaceFingerScreenNumber))
                    this.prop.PlaceFingerScreenNumber = this.FingerPrintCaptureStateTableData.PlaceFingerScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.FingerPrintCaptureStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.FingerPrintCaptureStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKPressededNextStateNumber))
                    this.prop.FDKPressededNextStateNumber = this.FingerPrintCaptureStateTableData.FDKPressededNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKActiveMask))
                    this.prop.FDKActiveMask = this.FingerPrintCaptureStateTableData.FDKActiveMask;

                if (string.IsNullOrEmpty(this.prop.Extension1.StateNumber) && extensionTable1 != null)
                    this.prop.Extension1.StateNumber = extensionTable1.StateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.MaximunAcceptableWhitePercentage) && extensionTable1 != null)
                    this.prop.Extension1.MaximunAcceptableWhitePercentage = extensionTable1.MaximunAcceptableWhitePercentage;
                if (string.IsNullOrEmpty(this.prop.Extension1.MinimumAcceptableWhitePercentage) && extensionTable1 != null)
                    this.prop.Extension1.MinimumAcceptableWhitePercentage = extensionTable1.MinimumAcceptableWhitePercentage;
                if (string.IsNullOrEmpty(this.prop.Extension1.ImageCapturedNextStateNumber) && extensionTable1 != null)
                    this.prop.Extension1.ImageCapturedNextStateNumber = extensionTable1.ImageCapturedNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.ImageNotCapturedNextStateNumber) && extensionTable1 != null)
                    this.prop.Extension1.ImageNotCapturedNextStateNumber = extensionTable1.ImageNotCapturedNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.HardwareErrorOrDeviceNotPresentNextStateNumber) && extensionTable1 != null)
                    this.prop.Extension1.HardwareErrorOrDeviceNotPresentNextStateNumber = extensionTable1.HardwareErrorOrDeviceNotPresentNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.Reserved1) && extensionTable1 != null)
                    this.prop.Extension1.Reserved1 = extensionTable1.Reserved1;
                if (string.IsNullOrEmpty(this.prop.Extension1.Reserved2) && extensionTable1 != null)
                    this.prop.Extension1.Reserved2 = extensionTable1.Reserved2;
                if (string.IsNullOrEmpty(this.prop.Extension1.Reserved3) && extensionTable1 != null)
                    this.prop.Extension1.Reserved3 = extensionTable1.Reserved3;

                if (string.IsNullOrEmpty(this.prop.Extension2.StateNumber) && extensionTable2 != null)
                    this.prop.Extension2.StateNumber = extensionTable1.StateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.ReadingFingerScreenNumber) && extensionTable2 != null)
                    this.prop.Extension2.ReadingFingerScreenNumber = extensionTable2.ReadingFingerScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.CheckFingerPositionScreenNumber) && extensionTable2 != null)
                    this.prop.Extension2.CheckFingerPositionScreenNumber = extensionTable2.CheckFingerPositionScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.RemoveFingerScreenNumber) && extensionTable2 != null)
                    this.prop.Extension2.RemoveFingerScreenNumber = extensionTable2.RemoveFingerScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.ImageLocationScreenNumber) && extensionTable2 != null)
                    this.prop.Extension2.ImageLocationScreenNumber = extensionTable2.ImageLocationScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.PleaseWaitScreenNumber) && extensionTable2 != null)
                    this.prop.Extension2.PleaseWaitScreenNumber = extensionTable2.PleaseWaitScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.Reserved1) && extensionTable2 != null)
                    this.prop.Extension2.Reserved1 = extensionTable1.Reserved1;
                if (string.IsNullOrEmpty(this.prop.Extension2.Reserved2) && extensionTable2 != null)
                    this.prop.Extension2.Reserved2 = extensionTable1.Reserved2;
                if (string.IsNullOrEmpty(this.prop.Extension2.Reserved3) && extensionTable2 != null)
                    this.prop.Extension2.Reserved3 = extensionTable1.Reserved3;
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
                this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerFingerPrintDataReceive);
                keyMask = Core.GetKeyMaskData(prop.FDKActiveMask); //Activo FDK solo para cancelar
                if (this.Core.ShowGeneralNDCScreen(this.prop.PlaceFingerScreenNumber, keyMask))
                {
                    this.Core.Sdo.FPM_StartFingerPrintCapture();
                    this.StartTimer();
                }
                else
                    this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerFingerPrintDataReceive(DeviceMessage dfr)
        {
            Log.Debug("/--->");
            //if (!string.IsNullOrEmpty(fingerPrintData))
            //{
            //    if (fingerPrintData.Length > 2)
            //    {
            //        if (fingerPrintData.Substring(0, 2).Equals("00"))
            //        {
            //            fingerPrintData = fingerPrintData.Substring(2);
            //            this.Core.bo.ExtraInfo.FingerPrint = fingerPrintData;
            //            Log.Info(tring.Format("Lenght: {0} - Data: {1}", fingerPrintData.Length, fingerPrintData));
            //            this.SetActivityResult(StateResult.SUCCESSFULLY, this.prop.Extension1.ImageCapturedNextStateNumber);
            //        }
            //        else
            //        {
            //            Log.Error(string.Format("Device Barcode error: {0}", fingerPrintData.Substring(2)));
            //            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension1.HardwareErrorOrDeviceNotPresentNextStateNumber);
            //        }
            //    }
            //    else
            //    {
            //        Log.Error(string.Format("Barcode data lenght is less to expected."));
            //        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension1.HardwareErrorOrDeviceNotPresentNextStateNumber);
            //    }
            //}
            //else
            //{
            //    Log.Error(string.Format("Barcode data is null or empty."));
            //    this.SetActivityResult(StateResult.HWERROR, this.prop.Extension1.HardwareErrorOrDeviceNotPresentNextStateNumber);
            //}
        }
        private void HandleOthersKeysReturn(string othersKeys)
        {
            Log.Debug("/--->");
            ////TODO: No se activa el enter y cancel
            //switch (othersKeys)
            //{
            //    case "ENTER": //Confirma TX
            //        {
            //            //this.Core.bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
            //            //this.SetActivityResult(0, this.prop.FDKAorINextStateNumber);
            //            break;
            //        }
            //    case "CANCEL":
            //        {
            //            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
            //            break;
            //        }
            //}
        }

        private void HandleFDKreturn(string FDKcode) //Todas las FDK cancelan la operación
        {
            try
            {
                Log.Info("-> FDK data: {0}", FDKcode);
                this.Core.Sdo.FPM_StopFingerPrintCapture();
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
                this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerFingerPrintDataReceive);
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
            this.Core.Sdo.FPM_StopFingerPrintCapture();
            this.StopTimer();
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerFingerPrintDataReceive);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }
}
