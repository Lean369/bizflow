using Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;
using static Entities.Enums;

namespace Business.TransactionMenuState
{
    public class TransactionMenuState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        TransactionMenuStateTableData_Type transactionMenuStateTableData; //Tabla con datos provenientes del download.
        PropertiesTransactionMenuState prop;
        bool ret = false;
        private ModulesVerifier modulesVerifier;
        private TransactionList transactionList;


        #region "Constructor"
        public TransactionMenuState(StateTable_Type stateTable)
        {
            this.ActivityName = "TransactionMenuState";
            this.transactionMenuStateTableData = (TransactionMenuStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesTransactionMenuState();
            this.prop = this.GetProperties<PropertiesTransactionMenuState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.transactionMenuStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.transactionMenuStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.transactionMenuStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberA))
                    this.prop.NextStateNumberA = this.transactionMenuStateTableData.NextStateNumberA;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberB))
                    this.prop.NextStateNumberB = this.transactionMenuStateTableData.NextStateNumberB;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberC))
                    this.prop.NextStateNumberC = this.transactionMenuStateTableData.NextStateNumberC;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberD))
                    this.prop.NextStateNumberD = this.transactionMenuStateTableData.NextStateNumberD;
                if (string.IsNullOrEmpty(this.prop.DeviceErrorNextStateNumber))
                    this.prop.DeviceErrorNextStateNumber = this.transactionMenuStateTableData.DeviceErrorNextStateNumber;
                if (this.prop.ActiveFDKs == null)
                    this.prop.ActiveFDKs = this.transactionMenuStateTableData.ActiveFDKs;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.transactionMenuStateTableData.Item1;
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

                modulesVerifier = new ModulesVerifier(this.Core);
                modulesVerifier.SubscribeCompletion();
                modulesVerifier.ChangeFitnessEvt += this.ChangeDEV_Fitness;
                modulesVerifier.ChangeSuppliesEvt += this.ChangeDEV_Supplies;
                modulesVerifier.NotifyDependenciesStateEvt += this.SetUserOptions;
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
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.Core.EvtFDKscreenPress += this.HandleFDKreturn;
                this.Core.EvtOthersKeysPress += this.HandleOthersKeysReturn;
                this.Core.Sdo.EvtCompletionReceive += this.HandlerCompletionReceive;
                this.CallHandler(this.prop.OnEightFDKSelection);
                LoadTransactions();
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
                Log.Info($"-> FDK data: {FDKcode}");

                var matchingTransaction = prop.avTransactions.FirstOrDefault(transaction => transaction.fdk.ToString() == FDKcode);

                if (matchingTransaction != null)
                {
                    this.Core.Bo.ExtraInfo.CurrentTxn = Utils.StringToEnum<Enums.AvTxn>(matchingTransaction.transactionTag);
                    GlobalAppData.Instance.SetScratchpad("curTxn", this.Core.Bo.ExtraInfo.CurrentTxn.ToString());
                    Log.Info($"FDK: {this.Core.Bo.ExtraInfo.CurrentTxn}");
                }
                else
                {
                    Log.Warn($"No matching transaction found: {this.Core.Bo.ExtraInfo.CurrentTxn}");
                }


                switch (FDKcode)
                {
                    case "A":
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberA);
                        break;
                    case "B":
                        if(this.Core.AlephATMAppData.Branding == Enums.Branding.RedPagosA) // recarga en redpagos
                            this.Core.Bo.ExtraInfo.IsMobileTopup = true;
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB);
                        break;
                    case "C":
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberC);
                        break;
                    case "D":
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberD);
                       break;
                    default:
                        Log.Error(string.Format("->Input error."));
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los retornos tipo --COMPLETION--
        /// </summary>
        /// <param name="dm"></param>
        private void HandlerCompletionReceive(DeviceMessage dm)
        {
            Log.Info($"/--> {dm.Device}");
            try
            {
                Completion cr = (Completion)dm.Payload;
                if (dm.Device == Enums.Devices.IOBoard || dm.Device == Enums.Devices.CashAcceptor || dm.Device == Enums.Devices.Printer)
                {
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                        Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                    else
                        Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                }
                //modulesVerifier.HandlerCompletionReceive(dm);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void LoadTransactions()
        {
            this.CallHandler(this.prop.OnPleaseWait);            
            if (!TransactionList.GetMapping(out this.transactionList))
            {
                Log.Error("Could not get transaction list mapping.");
                return;
            }
            var dependencieDevs = this.transactionList.Transactions.SelectMany(t => t.Dependencies).Distinct();
            this.modulesVerifier.Verify(dependencieDevs.ToList());
        }

        private void SetUserOptions(List<UnitDependency> unitDependencyList)
        {
            //execute the next line in a new thread to avoid blocking the UI
            new Thread(() =>
            {
                Thread.Sleep(300);
                this.Core.HideScreenModals();
            }).Start();
            if (this.prop.OnShowAvailableTransactions.Action == StateEvent.EventType.runScript)
                this.prop.OnShowAvailableTransactions.Parameters = this.SendAvailableTransactions(this.prop.ScreenNumber, unitDependencyList);
            this.CallHandler(this.prop.OnShowAvailableTransactions);
            this.StartTimer();
        }
        private string SendAvailableTransactions(string currentScreen, List<UnitDependency> unitDependencyList)
        {
            string result = string.Empty;
            this.prop.avTransactions.Clear();
            this.Core.Bo.ExtraInfo.AvailableTxns = new List<AvTxn>();

            this.prop.EnableBagStatusBar = !this.transactionList.DisableBagStatusBar;
            this.prop.EnableExitButton = !this.transactionList.DisableExitButton;

            var unavDevices = unitDependencyList.Where(ud => ud.Status != UnitDependency.DepStatus.OK).Select(udev => udev.ExternalCode).ToList();

            foreach (var tx in transactionList.Transactions)
            {
                if (tx.MenuItem.enabled)
                {
                    var dependenciesOk = tx.Dependencies.All(d => unitDependencyList.Any(ud => ud.Dependency == d && ud.Status == UnitDependency.DepStatus.OK));
                    tx.MenuItem.enabled = dependenciesOk;
                }
                this.prop.avTransactions.Add(tx.MenuItem);
                this.Core.Bo.ExtraInfo.AvailableTxns.Add(Utils.StringToEnum<Enums.AvTxn>(tx.MenuItem.transactionTag));
            }
            try
            {
                JObject jObject = new JObject
                {
                    ["showBagStatusBar"] = (JToken)this.prop.EnableBagStatusBar,
                    ["showExit"] = (JToken)this.prop.EnableExitButton,
                    ["list"] = JArray.FromObject(prop.avTransactions),
                    ["unavailableDevices"] = JArray.FromObject(unavDevices)
                };
                result = jObject.ToString(Formatting.None);
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
            return result;
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
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
                this.CurrentState = ProcessState.FINALIZED;
                modulesVerifier.UnsubscribeCompletion();
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
            bool enableNDCScreen = false;
            if (this.prop.ScreenMode.Equals("000"))
                enableNDCScreen = true;
            this.moreTime = new MoreTime(prop.MoreTime.MoreTimeScreenName, prop.MoreTime.MaxTimeOut,
                prop.MoreTime.MaxTimeOutRetries, prop.MoreTime.MoreTimeKeyboardEnabled, this.Core, enableNDCScreen, this.ActivityName);
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
