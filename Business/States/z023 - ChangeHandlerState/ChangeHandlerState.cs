using Entities;
using Entities.PaymentService;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Business.ChangeHandlerState
{
    public class ChangeHandlerState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        ChangeHandlerStateTableData_Type ChangeHandlerStateTableData; //Tabla con datos provenientes del download.
        PropertiesChangeHandlerState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();

        #region "Constructor"
        public ChangeHandlerState(StateTable_Type stateTable)
        {
            this.ActivityName = "ChangeHandlerState";
            this.ChangeHandlerStateTableData = (ChangeHandlerStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesChangeHandlerState();
            this.prop = this.GetProperties<PropertiesChangeHandlerState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.ChangeHandlerStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.ChangeHandlerStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.ChangeHandlerStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.ChangeHandlerStateTableData.TimeoutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.ChangeHandlerStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.ChangeHandlerStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.ChangeHandlerStateTableData.Item2;
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

                ChangeProcessStart();  //start service payment execution process

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
                //this.Core.Sdo.BAR_StopScanBarcode(); //Apago el barcode
                //this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
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

        #region "PAYMENT BUSINESS"

        /// <summary>
        /// Calculate change to be dispensed.
        /// </summary>
        private void ChangeProcessStart()
        {
            //get deposited amount
            var depositedAmount = GetDepositedAmount();
            decimal dispenseAmount = 0;
            Log.Info("MONTO QUE DEBE PAGAR ES : " + this.Core.Bo.ExtraInfo.Amount);
            Log.Info("MONTO DEPOSITADO ES : " + depositedAmount);
            WriteEJ($"TRANSACTION AMOUNT: {Utils.FormatCurrency(this.Core.Bo.ExtraInfo.Amount, this.Core.Bo.ExtraInfo.Currency, 12)}");
            WriteEJ($"DEPOSITED   AMOUNT: {Utils.FormatCurrency(depositedAmount, this.Core.Bo.ExtraInfo.Currency, 12)}");
            //some verifications, if its payment/deposit change
            if (this.Core.Bo.ExtraInfo.CurrentTxn != Enums.AvTxn.cashWithdrawalTx)
            {            
                if (depositedAmount == 0) //cambiar por el objeto de bussiness obj que utilice cashAcceptState para almacenar el monto depositado
                {
                    Log.Error("Deposit amount was not received.");
                    this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                    return;
                }
                if (this.Core.Bo.ExtraInfo.Amount == 0)
                {
                    Log.Error("Required Deposit Amount was not received.");
                    this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                    return;
                }
                //calculate change to dispense
                dispenseAmount = depositedAmount - this.Core.Bo.ExtraInfo.Amount;
            } else //if its a cash withdrawal we directly dispense the amount
            {
                dispenseAmount = this.Core.Bo.ExtraInfo.Amount;
            }

            if (dispenseAmount < 0)
            {
                Log.Error("Deposited amount does not meet requested amount {0}.", this.Core.Bo.ExtraInfo.Amount);
                this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                return;
            }
            if(dispenseAmount == 0)
            {
                // SetReceiptData();
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber); //jump to next state | no dispense requiered
                return;
            }
            if(!TypeCassetteMapping.GetMapping(out var typeCassetteMapping, this.Core.AlephATMAppData.DefaultCurrency))
            {
                Log.Error("Could not get TYPE MAPPING.");
                return;
            }
            //calculate quantity of notes and coins to dispense for each denomination
            var toDispense = new Entities.Functions.MixCalculator()
                .WithContents(this.Core.Counters.Contents)
                .SetCurrency(this.Core.Bo.ExtraInfo.Currency)
                .SetTypeMapping(typeCassetteMapping)
                .CalculateCoinAndNoteAmount(dispenseAmount);
            Log.Info("CAMBIO A DISPENSAR: {2} | Para notas: {0} | Para coins: {1}", toDispense[0], toDispense[1], dispenseAmount);
            this.WriteEJ($"CHANGE TO DISPENSE -> \n\t In Notes: {Utils.FormatCurrency(toDispense[0], this.Core.Bo.ExtraInfo.Currency, 12)} | In Coins: {Utils.FormatCurrency(toDispense[1], this.Core.Bo.ExtraInfo.Currency, 12)}");
            this.Core.Bo.ExtraInfo.AmountToDispenseInNotes = toDispense[0];
            this.Core.Bo.ExtraInfo.AmountToDispenseInCoins = toDispense[1];
            //SetReceiptData();
            //go to next state
            this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
        }

        /*Set data to be printed*/
        private void SetReceiptData()
        {
            //this.Core.Bo.ExtraInfo.ErrorCode = null;
            this.Core.Bo.ExtraInfo.ExtraData.Clear();
            //DATA FROM API
            var apiReceiptData = GetReceiptDataFromApi();
            if(apiReceiptData.success)
            {
                if (!string.IsNullOrEmpty(apiReceiptData.receipt.ElectronicInvoice))
                    this.Core.Bo.ExtraInfo.QRdata = apiReceiptData.receipt.ElectronicInvoice;
                AddReceiptExtraField(apiReceiptData.receipt.Summary);
                //AddReceiptExtraField(string.Empty, "messageToUser1");
                if(apiReceiptData.receipt.Details != null)
                    foreach (var detail in apiReceiptData.receipt.Details)
                    {
                        AddReceiptPaymentDetails(detail);
                    }
            }
            //DATA FROM LOCAL 
            //amount to pay
            var payAmo = Utilities.Utils.FormatCurrency(this.Core.Bo.ExtraInfo.Amount, this.Core.Bo.ExtraInfo.Currency, 2);
            AddReceiptAmount(string.Format("{0}", payAmo), "noteTotal");
            //deposited amount
            var depositedAmount = GetDepositedAmount();
            var depoAmo = Utilities.Utils.FormatCurrency(depositedAmount, this.Core.Bo.ExtraInfo.Currency, 2);
            AddReceiptAmount(string.Format("{0}", depoAmo), "paymentPay");
            //change to be dispensed
            decimal dispenseAmount = depositedAmount - this.Core.Bo.ExtraInfo.Amount;
            var dispenseAmo = Utilities.Utils.FormatCurrency(dispenseAmount, this.Core.Bo.ExtraInfo.Currency, 2);
            AddReceiptAmount(string.Format("{0}", dispenseAmo), "paymentChange");
            
            this.Core.Bo.ExtraInfo.ReceiptRequired = true;
        }
        private void AddReceiptExtraField(string value, string tagname = null)
        {
            this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData
            {
                ExtraDataType = Enums.ExtraDataType.dynamic,
                TagName = tagname ?? "keyless",
                TagValue = value
            });
        }
        private void AddReceiptAmount(string value, string tagname = null)
        {
            if(this.Core.Bo.ExtraInfo.PaymentAmounts == null)
                this.Core.Bo.ExtraInfo.PaymentAmounts = new List<ExtraData>();
            this.Core.Bo.ExtraInfo.PaymentAmounts.Add(new ExtraData
            {
                ExtraDataType = Enums.ExtraDataType.dynamic,
                TagName = tagname ?? "keyless",
                TagValue = value
            });
        }
        private void AddReceiptPaymentDetails(string value, string tagname = null)
        {
            if (this.Core.Bo.ExtraInfo.PaymentData == null)
                this.Core.Bo.ExtraInfo.PaymentData = new List<ExtraData>();
            this.Core.Bo.ExtraInfo.PaymentData.Add(new ExtraData
            {
                ExtraDataType = Enums.ExtraDataType.dynamic,
                TagName = tagname ?? "keyless",
                TagValue = value
            });
        }

        private (bool success, PaymentReceipt receipt, string message) GetReceiptDataFromApi()
        {
            if(!this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("PaymentCartId") || this.Core.Bo.ExtraInfo.HostExtraData["PaymentCartId"] is null)
            {
                Log.Error("Unable to obtain PaymentCartId");
                return (false, null, "");
            }
            var requestData = new Dictionary<PropKey, object>
            {
                { PropKey.CartID, this.Core.Bo.ExtraInfo.HostExtraData["PaymentCartId"] },
            };
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, requestData);
            var authResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_RECEIPT, null, this.prop.HostName);
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            return (rep.sc, rep.sc ? (PaymentReceipt)rep.rs.Data : null, rep.rs.Message);
        }
        private (bool sc, T rs) CastResult<T>(AuthorizationResult authorizationResult)
        {
            //var ok = authorizationResult.authorizationStatus == AuthorizationStatus.Authorized;
            //T data = (T)authorizationResult.Response;
            //return (ok, data);
            var ok = authorizationResult.authorizationStatus == AuthorizationStatus.Authorized;
            try
            {
                T data = (T)authorizationResult.Response;
                return (ok, data);
            }
            catch (InvalidCastException)
            {
                return (false, default);
            }
        }
        #endregion

        #region "Functions"

        private decimal GetDepositedAmount()
        {
            var itemsDep = new List<Bills>();
            this.Core.Bo.ExtraInfo.CashInMultiCashData.ListCashInInfo.ForEach(c => {
                itemsDep.AddRange(c.Bills.Where(b => b.Currency.Equals(this.Core.Bo.ExtraInfo.Currency)).ToList());
            });
            decimal depositedAmount = 0;
            itemsDep.ForEach(item => {
                depositedAmount += item.Quantity * item.Value;
            });
            return depositedAmount;
        }

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
