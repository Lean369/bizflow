using Entities;
using Entities.PaymentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Business.BarcodeReadState
{
    public class BarcodeReadState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        BarcodeReadStateTableData_Type barcodeReadStateTableData; //Tabla con datos provenientes del download.
        PropertiesBarcodeReadState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();
        private List<string> ReadBarcodes = new List<string> { "","","" };
         
        #region "Constructor"
        public BarcodeReadState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "BarcodeReadState";
            this.barcodeReadStateTableData = (BarcodeReadStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesBarcodeReadState(alephATMAppData);
            BarcodeReadStateTableDataExtension1_Type extensionTable = null;
            this.prop = this.GetProperties<PropertiesBarcodeReadState>(out ret, this.prop);
            if (ret)
            {
                if (this.barcodeReadStateTableData.Item != null)
                    extensionTable = (BarcodeReadStateTableDataExtension1_Type)this.barcodeReadStateTableData.Item;
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.barcodeReadStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.GoodBarcodeReadStateNumber))
                    this.prop.GoodBarcodeReadStateNumber = this.barcodeReadStateTableData.GoodBarcodeReadStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.barcodeReadStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.ErrorNextStateNumber))
                    this.prop.ErrorNextStateNumber = this.barcodeReadStateTableData.ErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.barcodeReadStateTableData.TimeoutNextStateNumber;
                if (this.prop.BarcodeDataDestination == BarcodeDataDestination_Type.none)
                    this.prop.BarcodeDataDestination = this.barcodeReadStateTableData.BarcodeDataDestination;
                if (string.IsNullOrEmpty(this.prop.ActiveCancelFDKKeyMask))
                    this.prop.ActiveCancelFDKKeyMask = this.barcodeReadStateTableData.ActiveCancelFDKKeyMask;
                if (this.prop.Extension != null && extensionTable != null)
                {
                    if (string.IsNullOrEmpty(this.prop.Extension.StateNumber))
                        this.prop.Extension.StateNumber = extensionTable.StateNumber;
                    if (this.prop.Extension.Item != null)
                        this.prop.Extension.Item = extensionTable.Item;
                    if (this.prop.Extension.Item1 != null)
                        this.prop.Extension.Item1 = extensionTable.Item1;
                    if (this.prop.Extension.Item2 != null)
                        this.prop.Extension.Item2 = extensionTable.Item2;
                    if (this.prop.Extension.Item3 != null)
                        this.prop.Extension.Item3 = extensionTable.Item3;
                    if (this.prop.Extension.Item4 != null)
                        this.prop.Extension.Item4 = extensionTable.Item4;
                    if (this.prop.Extension.Item5 != null)
                        this.prop.Extension.Item5 = extensionTable.Item5;
                    if (this.prop.Extension.Item6 != null)
                        this.prop.Extension.Item6 = extensionTable.Item6;
                }
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
                if (this.prop.EnableState)
                {
                    this.CurrentState = ProcessState.INPROGRESS;
                    this.ListOfAck = new List<string>();
                    this.ReadBarcodes = new List<string> { "", "", "" };
                    this.EnableJournal = this.prop.Journal.EnableJournal;
                    this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                    this.AddEventHandlers();
                    this.Core.Bo.ExtraInfo.Amount = 0;
                    if (this.CallHandler(this.prop.OnShowScreen))
                    {
                        this.StartTimer(false);
                    }
                    this.prop.OnBarcodeReadConfig.Parameters = this.prop.BarcodeDataDestination.ToString();
                    this.CallHandler(this.prop.OnBarcodeReadConfig);
                    this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();

                    IsUserNotificationPending();

                    this.Core.Sdo.BAR_StartScanBarcode();

                    //after 3 seconds trigger DepositBarcodeCheck
                    //Task.Delay(3000).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankDeposit, "310528436543"));
                    //Task.Delay(4500).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankDeposit, "1DB038675100"));
                    //Task.Delay(6000).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankDeposit, "200004999002"));

                    //Task.Delay(3000).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankDeposit, "1DB038675100"));
                    //Task.Delay(4500).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankDeposit, "200004982002"));
                    //Task.Delay(6000).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankDeposit, "310528436549"));

                    //Task.Delay(3000).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankWithdrawal, "330000000002"));
                    //Task.Delay(4500).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankWithdrawal, "1CB044552380"));
                    //Task.Delay(6000).ContinueWith(t => BankBarcodeCheck(BarcodeDataDestination_Type.BankWithdrawal, "200003500003"));
                }
                else
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.GoodBarcodeReadStateNumber);
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
                //this.StopTimer();//Detengo el timer de More Time
                Log.Info("/--> BCR event: {0}", dm.Payload.ToString());
                ProcessEventReceived(dm.Payload.ToString());
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
            {
                this.ProcessCompletion(dm);
            }
            else
            {
                Log.Error($"/-->ACK request ID: {dm.Header.RequestId} not found");
                this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
            }
        }

        private void ProcessCompletion(DeviceMessage dm)
        {
            Completion cr;
            try
            {
                Log.Info($"/--> {dm.Device}");
                //this.StopTimer();//Detengo el timer de More Time
                cr = (Completion)dm.Payload;
                if (dm.Device == Enums.Devices.BarcodeReader)
                {
                    switch (dm.Command)
                    {
                        case Enums.Commands.StartScan:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                if(!this.Core.Sdo.DevConf.BCRconfig.MultiRead) //only on single read
                                    this.ProcessEventReceived(cr.Data);
                            }
                            else
                                this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                            break;
                        case Enums.Commands.StopScan:
                            // this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Procesa los eventos del dispositivo
        /// </summary>
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void ProcessEventReceived(string data) //ProcessCompletion()
        {
            try
            {
                Log.Info("/-->");
                this.ResetTimer();
                var input = Utilities.Utils.HexToStr(data);
                Log.Info($"RECEIVED CODE FORM BARCODE -> Lenght: {input.Length} - Data: {input}");
                string barcodeData = new string(input.Where(c => !char.IsControl(c)).ToArray());
                if (string.IsNullOrEmpty(barcodeData))
                {
                    Log.Error("Barcode data is null or empty.");
                    this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                }
                if (barcodeData.Length <= 2)
                {
                    Log.Error("Barcode data lenght is less to expected.");
                    this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                }
                //guardar en memoria codigos leidos
                switch (this.prop.BarcodeDataDestination)
                {
                    case BarcodeDataDestination_Type.ChannelsA:
                        if (barcodeData.Length == 16)
                        {
                            this.Core.Bo.ExtraInfo.ChannelA = $"{barcodeData.Substring(this.prop.CoordChannel_1.iniPos, this.prop.CoordChannel_1.length)},{barcodeData.Substring(this.prop.CoordChannel_3.iniPos, this.prop.CoordChannel_3.length)},{barcodeData.Substring(this.prop.CoordChannel_2.iniPos, this.prop.CoordChannel_2.length)}";//barcodeData;
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.GoodBarcodeReadStateNumber);
                        }
                        else
                        {
                            Log.Error($"Barcode data lenght must be 16 - current lenght: {barcodeData.Length}");
                            this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                        }
                        break;
                    case BarcodeDataDestination_Type.Scratchpad:
                        this.BarcodeCheck(barcodeData);
                        break;
                    case BarcodeDataDestination_Type.BankDeposit:
                    case BarcodeDataDestination_Type.BankWithdrawal:
                        this.BankBarcodeCheck(this.prop.BarcodeDataDestination, barcodeData);
                        break;
                    default:
                        Log.Error("Invalid barcode destination");
                        this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                        break;
                }
            }
            catch (Exception ex)
            {
                this.SetActivityResult(StateResult.HWERROR, this.prop.ErrorNextStateNumber);
                Log.Fatal(ex);
            }
        }

        //check if new barcode already exists, if it doesnt add it to the list
        private void BarcodeCheck(string newBarcode)
        {
            try
            {
                //if (!IsDigitsOnly(newBarcode)) //ignore if its not only numbers
                //    return;
                if (string.IsNullOrWhiteSpace(newBarcode))
                    return;
                GlobalAppData.Instance.GetScratchpad("all-barcodes", out object allBarcodesObj);
                if (!string.IsNullOrWhiteSpace(allBarcodesObj as string))
                {
                    var barcodes = allBarcodesObj.ToString().Split(','); //check all comma separeted barcodes
                    if (!barcodes.Any(x => x.Equals(newBarcode)))
                    {
                        GlobalAppData.Instance.SetScratchpad("all-barcodes", $"{allBarcodesObj},{newBarcode}");
                        this.Core.RaiseEvtScreenData(this.prop.OnBarcodeChecks);
                    }
                }
                else
                {
                    GlobalAppData.Instance.SetScratchpad("all-barcodes", newBarcode);
                    if (prop.ShowConfirmBarcodes) // show confirm enables confirmation w list; no confirm passes through
                    {
                        this.Core.RaiseEvtScreenData(this.prop.OnBarcodeChecks);
                    }
                    else
                    {
                        EndBarcodeRead();
                    }
                }
            }
            catch (Exception) { }
        }

        private void BankBarcodeCheck(BarcodeDataDestination_Type btype, string brcd)
        {
            string[] prefixes = new string[] { };

            switch (btype)
            {
                case BarcodeDataDestination_Type.BankDeposit:
                    prefixes = this.prop.DepositPrefixes;
                    break;
                case BarcodeDataDestination_Type.BankWithdrawal:
                    prefixes = this.prop.WithdrawalPrefixes;
                    break;
                default:
                    break;
            }

            try
            {
                if (brcd.Length == 12)
                {
                    switch (brcd[0])
                    {
                        case '1':
                            if (!ReadBarcodes.Contains(brcd))
                            {
                                if (prefixes.Contains(brcd.Substring(0, 3)))
                                {
                                    this.ReadBarcodes[0] = brcd;
                                } else
                                {
                                    // Wrong barcode prefix
                                    this.CallHandler(this.prop.OnWrongBarcode);
                                }
                            }
                            break;
                        case '2':
                            this.ReadBarcodes[1] = brcd;
                            break;
                        case '3':
                            this.ReadBarcodes[2] = brcd;
                            break;
                    }
                } else
                {   // Wrong barcode length
                    this.CallHandler(this.prop.OnWrongBarcode);
                }
                // if the array have the three barcodes, then send the data to the server
                if (this.ReadBarcodes.Count == 3 && this.ReadBarcodes.All(x => !string.IsNullOrEmpty(x)))
                {
                    this.CallHandler(this.prop.OnPleaseWait);
                    this.Core.Bo.ExtraInfo.Barcode = string.Join(this.prop.BankCodesSeparator, this.ReadBarcodes) + this.prop.BankCodesSeparator;
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.GoodBarcodeReadStateNumber);
                }
                this.prop.OnBarcodeChecks.Parameters = Utils.JsonSerialize(this.ReadBarcodes.ToArray());
                this.CallHandler(this.prop.OnBarcodeChecks);
                
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
        #endregion "Returns from SDO"
        private void HandlerOthersKeysReturn(string othersKeys)
        {
            ////TODO: No se activa el enter y cancel
            Log.Info("/--> Key press: {0}", othersKeys);
            switch (othersKeys)
            {
                case "CANCEL":
                    {
                        this.Core.Sdo.BAR_StopScanBarcode(); //Apago el barcode
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case "CONTINUE":
                    {
                        EndBarcodeRead();
                        break;
                    }
            }
        }

        private void EndBarcodeRead()
        {
            GlobalAppData.Instance.GetScratchpad("all-barcodes", out object allBarcodesObj);
            if (!string.IsNullOrWhiteSpace(allBarcodesObj as string))
            {
                this.Core.Bo.ExtraInfo.Barcode = allBarcodesObj as string;
                if (!(this.prop.MultipleCodesSeparator is null))
                {
                    var codes = this.Core.Bo.ExtraInfo.Barcode.Split(',');
                    this.Core.Bo.ExtraInfo.Barcode = string.Join(this.prop.MultipleCodesSeparator, codes); //separetor is only used to separete in result codes (to be sent to server)
                }
                this.Core.Sdo.BAR_StopScanBarcode(); //Apago el barcode
                GlobalAppData.Instance.SetScratchpad("all-barcodes", string.Empty); //clear barcodes in scratchpad
                this.SetActivityResult(StateResult.SUCCESS, this.prop.GoodBarcodeReadStateNumber);
            }
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
                GlobalAppData.Instance.DeleteScratchpad("all-barcodes");
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AddEventHandlers()
        {
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
            this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive += new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive += new SDO.DelegateAckReceive(this.HandlerAckReceive);

        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive -= new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        private void HandlerInputData(string dataInput, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", dataInput);
                //this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "StartBarcode":
                            {
                                this.Core.Sdo.BAR_StartScanBarcode();
                                break;
                            }
                        case "StopBarcode":
                            {
                                //this.CallHandler(this.prop.OnPleaseWait);
                                this.Core.Sdo.BAR_StopScanBarcode();
                                break;
                            }
                    }
                }
                //this.timerScreen.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void IsUserNotificationPending()
        {
            if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.PendingUserNotification))
            {
                //call to display notif to user
                this.CallHandler(new StateEvent(StateEvent.EventType.runScript, "UserNotificationModal", this.Core.Bo.ExtraInfo.PendingUserNotification));
                this.Core.Bo.ExtraInfo.PendingUserNotification = null;//clear
            }
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
