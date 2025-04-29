using Entities;
using Entities.PaymentService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static Entities.Enums;

namespace Business.ShoppingCartState
{
    public class ShoppingCartState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        ShoppingCartStateTableData_Type ShoppingCartStateTableData; //Tabla con datos provenientes del download.
        PropertiesShoppingCartState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();

        const bool DEBUG_BYPASS = false;

        #region "Constructor"
        public ShoppingCartState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "ShoppingCartState";
            this.ShoppingCartStateTableData = (ShoppingCartStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesShoppingCartState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesShoppingCartState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.ShoppingCartStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.ShoppingCartStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.ShoppingCartStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.ShoppingCartStateTableData.TimeoutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.ShoppingCartStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.BackNextStateNumber))
                    this.prop.BackNextStateNumber = this.ShoppingCartStateTableData.BackNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.ShoppingCartStateTableData.Item2;
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
                //this.StartTimer(false);

                //TEMPORAL solo prueba - BORRAR
                //this.Core.Bo.ExtraInfo.AmountToDispenseInNotes = 1200;
                //this.Core.Bo.ExtraInfo.AmountToDispenseInCoins = 68;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private async void HandlerInputData(string inputData, string dataLink)
        {
            this.ResetTimer();
            switch (dataLink)
            {
                case "Continue":
                    this.StopTimer();
                    //continue to next state
                    var cartPrepare = await PrepareShoppingCartForExecution();
                    if (!cartPrepare.success)
                    {
                        Log.Error("Could not prepare CART data for payment authorization. Info: {0} ", cartPrepare.message);
                        this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                    }
                    else
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                    break;
                case "RemoveCartItem":
                    //request item removal and reload cart items from api
                    this.StopTimer();
                    if (long.TryParse(inputData, out long billid))
                    {
                        var remove = await RemoveFromCartBillPayment(billid);
                        if (!remove.success)
                        {
                            Log.Error("Failed to RemoveFromCartBillPayment");
                            WriteEJ("ShoppingCart -ERROR- (Failed to RemoveFromCartBillPayment)");
                            return;
                        }
                        else
                        {
                            Log.Info($"Removed bill ID: {billid}");
                            WriteEJ($"Removed bill ID: {billid}");
                        }
                        //inform to web about remove status
                        var _data = Utilities.Utils.JsonSerialize(remove.data);
                        this.prop.OnRemoveOperation.Parameters = Utilities.Utils.JsonSerialize(new { success = remove.success, data = _data });
                        this.CallHandler(this.prop.OnRemoveOperation);
                        if (await LoadCartInfo())
                            Log.Info("reload cart info success");
                        else
                            Log.Error("reload cart info success");
                    }
                    this.StartTimer(false);
                    break;
                case "AddOtherService":
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.BackNextStateNumber);
                    break;
                default:
                    Log.Info("Evento recibido es {0}", dataLink);
                    break;
            }
        }

        private async void HandlerFDKreturn(string FDKcode) //
        {
            Log.Debug("/--->");
            if (FDKcode == "A")
            {
                if (await CartStartup()) //startup api calls
                    this.StartTimer(false);
                else
                    this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
            }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info("/--> Key press: {0}", othersKeys);
            switch (othersKeys)
            {
                //case "ENTER": //Confirma TX
                //    {
                //        //this.SetActivityResult(0, this.prop.GoodBarcodeReadStateNumber);
                //        break;
                //    }
                case "CANCEL":
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
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
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AddEventHandlers()
        {
            this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
        }


        #region "PAYMENT BUSINESS"

        private async Task<bool> CartStartup()
        {
            var ret = (success: false, data: new ResponseBody());
            ret = await CheckPendingAddtoCart();
            if(!ret.success && this.prop.SingleCartItem)
            {
                // if error on pending item & is single cart, cancel txn
                JObject jObject = new JObject
                {
                    ["message"] = ret.data.Message,
                    ["cancelAfter"] = true
                };
                this.prop.OnShowCartMessage.Parameters = jObject.ToString(Formatting.None);
                this.CallHandler(this.prop.OnShowCartMessage);
                return true;
            } else
            {
                return await LoadCartInfo();
            }
        }

        /*checks if there is a pending item to add to cart*/
        private async Task<(bool, ResponseBody)> CheckPendingAddtoCart()
        {
            var ret = (success: false, data: new ResponseBody());

            if (!this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("billId")
                || (this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("currentStep") && this.Core.Bo.ExtraInfo.HostExtraData["currentStep"] is short currentStep && currentStep < 1))
            {
                return ret;
            }


            ret = await AddtoCartBillPayment((long)this.Core.Bo.ExtraInfo.HostExtraData["billId"]);

            if (!ret.success)
            {
                
                 JObject jObject = new JObject
                {
                    ["message"] = Utilities.Utils.JsonSerialize(ret.data),
                    ["cancelAfter"] = false
                };
                this.prop.OnShowCartMessage.Parameters = jObject.ToString(Formatting.None);
                this.CallHandler(this.prop.OnShowCartMessage);

                //this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                Log.Error("Faild to get AddtoCartBillPayment");
                WriteEJ("ShoppingCart -ERROR- (Faild to get AddtoCartBillPayment)");
                this.Core.Bo.ExtraInfo.HostExtraData.Remove("billId"); //remove processed item whether it was successfull or not
                return ret;
            }
            else
            {
                WriteEJ($"Added bill ID: {this.Core.Bo.ExtraInfo.HostExtraData["billId"]}");
                this.Core.Bo.ExtraInfo.HostExtraData.Remove("billId"); //remove processed item whether it was successfull or not
                return ret;
            }
        }

        /*obtener info del carrito y enviarla al front*/
        private async Task<bool> LoadCartInfo()
        {
            //if (DEBUG_BYPASS)
            //{
            //    DebugBypassLoadCartInfo();
            //    return;
            //}
            bool ret = false;
            var result = await GetCartBillPayment();
            if (!result.success)
            {
                this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                Log.Error("Failed to GetCartBillPayment");
            }
            else
                ret = true;
            //indicates total amount to be deposit in cash accept state | in case of new item added or removed must recall this method
            //since REDPAGOS list "totals" by currency we need to retrieve the total for the currency we can handle
            //Fix para permitir pagos de servicios con distinta divisa al DefaultCurrency
            var containsAll = result.data.Total.All(t => this.Core.Counters.AcceptedCurrencies.Contains(t.Currency));
            if (containsAll == false)
            {
                ret = false;
                Log.Error("Failed to obtain total with local currency.");
                WriteEJ("ShoppingCart -ERROR- (invalid local currency)");
            }

            //    var total = result.data.Total.FirstOrDefault(t => t.Currency == this.Core.Bo.ExtraInfo.Currency);
            //if (total == null)
            //{
            //    ret = false;
            //    Log.Error("Faild to obtain total with local currency.");
            //}

            //is single cart item
            result.data.SingleCartItem = this.prop.SingleCartItem;
            //send cart items to web
            this.prop.OnShowCartItems.Parameters = Utilities.Utils.JsonSerialize(result.data);
            this.CallHandler(this.prop.OnShowCartItems);
            return ret;
        }

        /// <summary>
        /// It requests current shopping cart items and saves it in business object for payment/deposit/withdrawal process in cashAcceptState/cashDispenseState.
        /// Also sets amount to be requested for deposit.
        /// </summary>
        private async Task<(bool success, string message)> PrepareShoppingCartForExecution()
        {
            //if (DEBUG_BYPASS)
            //    return DebugBypassNextState();

            var getcart = await GetCartBillPayment();
            if (!getcart.success || getcart.data.Bills == null || getcart.data.Bills.Count == 0)
                return (false, "Could not get CART items for payment execution.");


            var containsAll = getcart.data.Total.All(t => this.Core.Counters.AcceptedCurrencies.Contains(t.Currency));
            if (containsAll == false)
            {
                WriteEJ("ShoppingCart -ERROR- (invalid currency)");
                return (false, $"Failed to get amount with local currency: {this.Core.Bo.ExtraInfo.Currency}");
            }

            var total = getcart.data.Total.FirstOrDefault(t => this.Core.Counters.AcceptedCurrencies.Contains(t.Currency));

            //var total = getcart.data.Total.FirstOrDefault(t => t.Currency == this.Core.Bo.ExtraInfo.Currency);
            //if (total == null)
            //{
            //    WriteEJ("ShoppingCart -ERROR- (invalid currency)");
            //    return (false, $"Failed to get amount with local currency: {this.Core.Bo.ExtraInfo.Currency}");
            //}
            var payment = new Payment();
            payment.BillsIDs = new List<long>();
            payment.BillList = new List<Bill>(getcart.data.Bills);

            var isPayment = false;

            foreach (var bill in getcart.data.Bills)
            {
                payment.BillsIDs.Add(bill.BillID);
                if (bill.Service.ServiceMode != getcart.data.Bills[0].Service.ServiceMode)
                {
                    Log.Error("Mix of Service Modes in shopping cart items.");
                }
                if (bill.Service.ServiceMode == ServiceMode.Payment)
                {
                    isPayment = true;
                }
            }

            payment.Total = new Total
            {
                Amount = total.Amount,
                Currency = total.Currency
            };
            var payments = new Payments { payment };
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, payments); //add object requiered for payment authorization/execution

            
            this.Core.Bo.ExtraInfo.AmountLimit = (decimal)total.Amount;  //set amount to be required to the user in cash accept state
            this.Core.Bo.ExtraInfo.Amount = (decimal)total.Amount;
            GlobalAppData.Instance.SetScratchpad("amountLimit", this.Core.Bo.ExtraInfo.AmountLimit);
           
            

            

            this.Core.AddHostExtraData("PaymentCartId", getcart.data.CartID);
            WriteEJ("ShoppingCart -OK-");
            WriteEJ($"CART ID: {getcart.data.CartID}");
            payments.ForEach(p => { WriteEJ($"TOTAL: {Utils.FormatCurrency((decimal)p.Total.Amount, p.Total.Currency, 12)}"); });
            return (true, "");
        }
        #endregion

        #region "DEBUG BYPASS"

        /*SOLO PARA TESTING*/
        private void DebugBypassLoadCartInfo()
        {
            Log.Debug("/--->");
            var data = new Cart
            {
                Bills = new List<Bill>
                {
                    new Bill
                    {
                        BillID = 1,
                        Amount = 100,
                        CurrencyID = 1,
                        Service = new Service
                        {
                            ServiceID = 1,
                            ServiceName = "Servicio prueba 1",
                            Enabled = true
                        }
                    },
                },
            };
            this.prop.OnShowCartItems.Parameters = Utilities.Utils.JsonSerialize(data);
            this.CallHandler(this.prop.OnShowCartItems);
        }
        private (bool success, string message) DebugBypassNextState()
        {
            Log.Debug("/--->");
            var payments = new Payments
            {
                new Payment
                {
                    BillsIDs = new List<long> { 1, 2, 3 },
                    Total = new Total
                    {
                        Amount = 1000,
                        Currency = "ARS"
                    }
                }
            };
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, payments);
            this.Core.Bo.ExtraInfo.AmountLimit = 1000;
            this.Core.Bo.ExtraInfo.Amount = 1000;
            GlobalAppData.Instance.SetScratchpad("amountLimit", this.Core.Bo.ExtraInfo.AmountLimit);
            this.Core.AddHostExtraData("PaymentCartId", 101);
            return (true, "");
        }
        #endregion

        #region "API CALLS"

        private async Task<(bool success, ResponseBody data)> AddtoCartBillPayment(long billId)
        {
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, new Dictionary<PropKey, object>
            {
                { PropKey.BillID, billId }
            });
            var authResult = await Task.Run(() => this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_ADDTOCART, null, this.prop.HostName));

            return CastResult<Entities.PaymentService.ResponseBody>(authResult);
        }

        private async Task<(bool success, Cart data)> GetCartBillPayment(long cartId = 0)
        {
            if (cartId != 0)
                this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, new Dictionary<PropKey, object>
                {
                    { PropKey.CartID, cartId }
                });
            var authResult = await Task.Run(() => this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_GETCART, null, this.prop.HostName));
            if (authResult.authorizationStatus == AuthorizationStatus.Declined)
                return (false, null);
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            if (authResult.authorizationStatus == AuthorizationStatus.Declined)
                return (false, null);
            Cart cart = (Cart)rep.rs.Data;
            // Save cart no. of items in sp
            if (rep.sc) SaveCartQtyOnScratchpad(cart); //To be removed, migrating to prop.
            return (rep.sc, rep.sc ? cart : null);
        }

        private async Task<(bool success, Cart data)> RemoveFromCartBillPayment(long billId)
        {
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, new Dictionary<PropKey, object>
            {
                { PropKey.BillID, billId }
            });
            var authResult = await Task.Run(() => this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_REMOVEFROMCART, null, this.prop.HostName));
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            if (authResult.authorizationStatus == AuthorizationStatus.Declined)
                return (false, null);
            Cart cart = (Cart)rep.rs.Data;
            // Save cart no. of items in sp
            if (rep.sc) SaveCartQtyOnScratchpad(cart);
            return (rep.sc, rep.sc ? cart : null);
        }

        private (bool success, Cart data) EmtpyCartPayment()
        {
            var authResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_EMTPYCART, null, this.prop.HostName);
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            return (rep.sc, rep.sc ? (Cart)rep.rs.Data : null);
        }

        private (bool sc, T rs) CastResult<T>(AuthorizationResult authorizationResult)
        {
            //var ok = authorizationResult.authorizationStatus == AuthorizationStatus.Authorized;
            //T data = ok ? (T)authorizationResult.Response : default;
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

        private void SaveCartQtyOnScratchpad(Cart data)
        {
            GlobalAppData.Instance.SetScratchpad("shoppingCartItems", data.Bills.Count); // To be removed
            this.Core.Bo.ExtraInfo.ShoppingCartItems = data.Bills.Count;
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
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.AddEventHandlers();//Coloco nuevamente los subscriptores de los eventos
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        this.StartTimer(false);
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
            this.timeout = true;
            this.StopTimer();
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
        }
        #endregion "More time"
    }
}
