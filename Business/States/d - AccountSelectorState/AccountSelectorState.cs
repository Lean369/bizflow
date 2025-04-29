using Entities;
using Entities.PaymentService;
//using External_Interface.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.AccountSelectorState
{
    public class AccountSelectorState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        AccountSelectorStateTableData_Type AccountSelectorStateData; //Tabla con datos provenientes del download.
        PropertiesAccountSelectorState prop;
        //private BankConfiguration bankConfiguration;

        #region "Constructor"
        public AccountSelectorState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            //var retBC = BankConfiguration.GetMapping(alephATMAppData, out this.bankConfiguration);

            bool ret = false;
            this.ActivityName = "AccountSelectorState";
            this.AccountSelectorStateData = (AccountSelectorStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesAccountSelectorState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesAccountSelectorState>(out ret, this.prop);
            if (ret)
            {
                if (this.prop.accountList.Count == 0)
                {
                    this.prop.LoadDefaultConfiguration(alephATMAppData.TerminalModel);
                    string pathFile = $"{Const.appPath}StatesSets\\Properties{this.ActivityName}.xml";
                    System.IO.File.Delete(pathFile);
                    this.GetProperties<PropertiesAccountSelectorState>(out ret, this.prop);
                }
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.AccountSelectorStateData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.AccountSelectorStateData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.AccountSelectorStateData.CancelNextStateNumber;
                if (this.prop.OperationCodeData == GetDataOperationMode_Type.none)
                    this.prop.OperationCodeData = this.AccountSelectorStateData.OperationCodeData;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.AccountSelectorStateData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.BackNextStateNumber))
                    this.prop.BackNextStateNumber = this.AccountSelectorStateData.BackNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Aux2))
                    this.prop.Aux2 = this.AccountSelectorStateData.Aux2;
                if (this.prop.Item == null)
                    this.prop.Item = this.AccountSelectorStateData.Item;
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

        public async override void ActivityStart()
        {
            //KeyMask_Type keyMask;
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                this.Core.Bo.ExtraInfo.Amount = 0;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                if (this.CallHandler(this.prop.OnAccountSelector))
                {
                    this.StartTimer();
                    if(prop.BypassPaymentConfirm) //Bypass payment confirmation
                    {
                        if (this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("billId")) //Search for payment summary
                        {
                            var result = await PaymentSummary((long)this.Core.Bo.ExtraInfo.HostExtraData["billId"]);
                        }
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                    }
                }
                else
                {
                    Log.Error(string.Format("Can´t show screen: {0}", this.prop.OnAccountSelector.HandlerName));
                    this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                }
                //this.ListOfTableItem = Business.TableItem.GetTableItems();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerInputData(string dataInput, string dataLink)
        {
            try
            {
                //bool ret = false;
                Log.Info("-> Input data: {0}", dataInput);
                this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "SelectedAccount":
                            {
                                //ChannelInfo channelInfo = Utilities.Utils.JsonDeserialize<ChannelInfo>(out ret, dataInput);
                                AccountDetail selectedAccount = this.prop.accountList.Find(item => item.id.Equals(Convert.ToInt32(dataInput)));
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                                ////B)- Send transaction message to host

                                //Contents contents = new Contents();
                                //this.CallHandler(this.prop.OnPleaseWait);

                                //Thread prtWndThd;
                                //prtWndThd = new Thread(new ParameterizedThreadStart(this.AuthorizeTransaction));
                                //prtWndThd.Start(contents);
                                break;
                            }
                    }
                }
                this.timerScreen.Start();
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
            switch (othersKeys)
            {
                case "ENTER": //Confirma TX
                    {
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                        break;
                    }
                case "CANCEL":
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
            }
        }

        private async void HandlerFDKreturn(string FDKcode)
        {
            Log.Debug("/--->");
            if (FDKcode == "A")
            {
                switch (this.prop.OperationCodeData)
                {
                    case GetDataOperationMode_Type.none:
                        break;
                    case GetDataOperationMode_Type.fromHost: //especifico depositarios
                        //remove all previous dynamic items to not repeat in case of reload
                        this.prop.accountList.Clear();
                        //this.prop.accountList.RemoveAll(a => a.dynamic == true);
                        var body = this.Core.Bo.ExtraInfo.HostExtraData["HostResultData"] as ResponseBody;
                        //body.Data["NombreTitular"]
                        var json = JsonConvert.SerializeObject(body.Data);
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        //if (this.bankConfiguration.WithCtaDefault) {
                        //    dict.Add("defaultAccount", this.bankConfiguration.WithCtaDefault.ToString());
                        //    dict.Add("defaultAccountType", this.bankConfiguration.TipoCtaDefault);
                        //    dict.Add("defaultAccountNumber", this.bankConfiguration.NroCtoDefault);
                        //    dict.Add("defaultDocumentType", this.bankConfiguration.TipoDocDefault);
                        //    dict.Add("defaultDocumentNumber", this.bankConfiguration.NroDocDefault);
                        //}

                        //change key NombreTitular to holderName
                        if (dict.ContainsKey("NombreTitular"))
                        {
                            this.Core.Bo.ExtraInfo.AccountHolder = dict["NombreTitular"];
                            dict["holderName"] = dict["NombreTitular"];
                            dict.Remove("NombreTitular");
                        }

                        if (dict.ContainsKey("NombreOperador"))
                        {
                            this.Core.Bo.ExtraInfo.DepositorAccount = dict["NombreOperador"];
                            dict["operatorName"] = dict["NombreOperador"];
                            dict.Remove("NombreOperador");
                        }

                        dict.Add("buttonTag", "language.common.btn.continue");

                        AccountDetail acc = new AccountDetail(0, true, "custom");
                        acc.details = JsonConvert.SerializeObject(dict);
                        acc.selectable = true;

                        this.prop.accountList.Add(acc);

                        this.prop.OnShowAccounts.Parameters = JsonConvert.SerializeObject(this.prop.accountList);
                        this.CallHandler(this.prop.OnShowAccounts);
                        break;
                    case GetDataOperationMode_Type.fromList:
                        if (this.prop.OnShowAccounts.Action == StateEvent.EventType.runScript)
                        {
                            //remove all previous dynamic items to not repeat in case of reload
                            this.prop.accountList.RemoveAll(a => a.dynamic == true);
                            //Add "AccountDetail" items generated on runtime
                            var aditionaList = await OnRuntimeAccountDetailList();
                            if (aditionaList.success)
                                this.prop.accountList.AddRange(aditionaList.accountDetailList);

                            this.prop.OnShowAccounts.Parameters = Utilities.Utils.JsonSerialize(this.prop.accountList);

                        }
                        this.CallHandler(this.prop.OnShowAccounts);
                        break;
                    default:
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
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        #region "API CALLS"

        private async Task<(bool success, List<AccountDetail> accountDetailList)> OnRuntimeAccountDetailList()
        {
            (bool succ, List<AccountDetail> acList) res = (false, new List<AccountDetail>());
            if (this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("billId")) //Search for payment summary
            {
                var result = await PaymentSummary((long)this.Core.Bo.ExtraInfo.HostExtraData["billId"]);
                if (!result.success)
                {
                    Log.Error("Failed to get PaymentSummary");
                    return (false, null);
                }
                if (!result.data.ContainsKey(PropKey.Bills) || !result.data.ContainsKey(PropKey.Totals))
                {
                    Log.Error("Bills or Totals were not found on PaymentSummary response.");
                    return (false, null);
                }
                var bills = (List<Bill>)result.data[PropKey.Bills];
                var totals = (List<Entities.PaymentService.Total>)result.data[PropKey.Totals];
                //var currencySymbol = CurrencyFromTotals(totals);
                foreach (var bill in bills)
                {
                    res.acList.Add(new AccountDetail
                    {
                        id = Convert.ToInt32(bill.BillID),
                        enabled = true,
                        dynamic = true,
                        details = Utilities.Utils.NewtonsoftSerialize(new Dictionary<string, string> {
                            //{ "titleLeft", bill.Service.ServiceName },
                            { "titleLeft", Utilities.Utils.NewtonsoftSerialize(new Dictionary<string, string>{ { "text", bill.Service.ServiceName } }) },
                            { "titleRight", Utilities.Utils.NewtonsoftSerialize(new Dictionary<string, string>{ { "amount", $"{string.Format("{0:N}", bill.Amount)}" } })},
                            { "reference", bill.Reference },
                            { "dueDate", bill.Expiration.HasValue ? bill.Expiration.Value.ToShortDateString() : "" },
                        })
                    });
                }
                foreach (var total in totals)
                {
                    res.acList.Add(new AccountDetail
                    {
                        id = 0,
                        enabled = true,
                        dynamic = true,
                        selectable = true,
                        details = Utilities.Utils.NewtonsoftSerialize(new Dictionary<string, string> {
                            //{ "titleLeft", "language.common.tag.total" },
                            { "titleLeft", Utilities.Utils.NewtonsoftSerialize(new Dictionary<string, string>{ { "tag", "common.tag.total" } }) },
                            { "titleRight", Utilities.Utils.NewtonsoftSerialize(new Dictionary<string, string>{ { "amount", $"{string.Format("{0:N}", total.Amount)}" } })},
                            { "buttonTag", "language.shoppingCart.btn.confirmAndAddToCart" },
                        })
                    });
                }
            }
            res.succ = res.acList.Count > 0;
            return res;
        }

        private async Task<(bool success, Dictionary<PropKey, object> data)> PaymentSummary(long billId)
        {
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, new Dictionary<PropKey, object>
            {
                { PropKey.BillID, billId }
            });
            var authResult = await Task.Run(() => this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_SUMMARY, null, this.prop.HostName));

            var respsonse = authResult.Response as ResponseBody;
            var res = authResult.authorizationStatus == AuthorizationStatus.Authorized;
            var data = res ? (Dictionary<PropKey, object>)respsonse.Data : null;
            return (res, data);
        }

        #endregion


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
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"

    }
}
