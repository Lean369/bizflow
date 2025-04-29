using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.SettlementOperationState
{
    public class SettlementOperationState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        SettlementOperationStateTableData_Type SettlementOperationStateTableData; //Tabla con datos provenientes del download.
        PropertiesSettlementOperationState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();

        #region "Constructor"
        public SettlementOperationState(StateTable_Type stateTable)
        {
            this.ActivityName = "SettlementOperationState";
            this.SettlementOperationStateTableData = (SettlementOperationStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesSettlementOperationState();
            this.prop = this.GetProperties<PropertiesSettlementOperationState>(out ret, this.prop);
            if (ret)
            {
                //if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                //    this.prop.ScreenNumber = this.SettlementOperationStateTableData.ScreenNumber;
                //if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                //    this.prop.NextStateNumber = this.SettlementOperationStateTableData.NextStateNumber;
                //if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                //    this.prop.HardwareErrorNextStateNumber = this.SettlementOperationStateTableData.HardwareErrorNextStateNumber;
                //if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                //    this.prop.TimeoutNextStateNumber = this.SettlementOperationStateTableData.TimeoutNextStateNumber;
                //if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                //    this.prop.CancelNextStateNumber = this.SettlementOperationStateTableData.CancelNextStateNumber;
                //if (string.IsNullOrEmpty(this.prop.Item1))
                //    this.prop.Item1 = this.SettlementOperationStateTableData.Item1;
                //if (string.IsNullOrEmpty(this.prop.Item2))
                //    this.prop.Item2 = this.SettlementOperationStateTableData.Item2;
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
                this.ListOfAck = new List<string>();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.AddEventHandlers();
                this.Core.Bo.ExtraInfo.Amount = 0;
                this.CallHandler(this.prop.OnShowScreen);
                //this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();
                this.Core.Sdo.BAR_StartScanBarcode();
                this.StartTimer(false);
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
            if (dm.Device == Enums.Devices.BarcodeReader)
            {
                this.StopTimer();//Detengo el timer de More Time
                Log.Info("/--> Cim event: {0}", dm.Payload.ToString());
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
                Log.Info("/-->ACK request ID: {0}", dm.Header.RequestId);
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
                Log.Error("/-->ACK request ID: {0} not found", dm.Header.RequestId);
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
                Log.Info("/--> {0}", dm.Device);
                cr = (Completion)dm.Payload;
                //Logeo resultado
                if (dm.Device == Enums.Devices.BarcodeReader)
                {
                    //if (!string.IsNullOrEmpty(barcodeData))
                    //{
                    //    if (barcodeData.Length > 2)
                    //    {
                    //        if (barcodeData.Substring(0, 2).Equals("00"))
                    //        {
                    //            barcodeData = barcodeData.Substring(2);
                    //            this.Core.bo.ExtraInfo.Barcode = barcodeData;
                    //            this.Core.bo.ExtraInfo.Barcode = Utilities.Utils.HexToStr(barcodeData);
                    //            Log.Info(tring.Format("Lenght: {0} - Data: {1}", barcodeData.Length, barcodeData));
                    //            this.SetActivityResult(StateResult.SUCCESSFULLY, this.prop.GoodBarcodeReadStateNumber);
                    //        }
                    //        else
                    //        {
                    //            Log.Error(string.Format("Device Barcode error: {0}", barcodeData.Substring(2)));
                    //            this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Log.Error(string.Format("Barcode data lenght is less to expected."));
                    //        this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                    //    }            
                    //}
                    //else
                    //{
                    //    Log.Error(string.Format("Barcode data is null or empty."));
                    //    this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                    //}
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #endregion "Returns from SDO"

        private void HandlerFDKreturn(string FDKcode) //Todas las FDK cancelan la operación
        {
            try
            {
                this.Core.Sdo.BAR_StopScanBarcode(); //Apago el barcode
                this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            ////TODO: No se activa el enter y cancel
            Log.Info("/--> Key press: {0}", othersKeys);
            //switch(othersKeys)
            //{
            //    case "ENTER": //Confirma TX
            //        {
            //            //this.SetActivityResult(0, this.prop.GoodBarcodeReadStateNumber);
            //            break;
            //        }
            //    case "CANCEL":
            //        {
            //            this.Core.sdo.StopScanBarcode(); //Apago el barcode
            //            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
            //            break;
            //        }
            //}
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
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive += new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive += new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive -= new SDO.DelegateAckReceive(this.HandlerAckReceive);
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
            this.AddEventHandlers();//Coloco nuevamente los subscriptores de los eventos
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
        private void StartTimer(bool resetMoreTimeRetry)
        {
            Log.Debug($"/--->ResetRetry: {resetMoreTimeRetry}");
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.MoreTime.MaxTimeOut * 1000;
            this.SubscribeMoreTime(true);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            this.timeout = false;
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
            else if (!this.MoreTimeSubscribed) this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);

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

        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Core.Sdo.BAR_StopScanBarcode();//Apago el barcode
            this.timeout = true;
            this.StopTimer();
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
        }
        #endregion "More time"
    }
}
