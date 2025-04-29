using Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.AmountEntryState
{
    public class AmountEntryState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        AmountEntryStateTableData_Type amountEntryStateTableData; //Tabla con datos provenientes del download.
        PropertiesAmountEntryState prop;
        string amount = "0";
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        Dictionary<string, int> availableNotes = new Dictionary<string, int>();


        #region "Constructor"
        public AmountEntryState(StateTable_Type stateTable)
        {
            this.ActivityName = "AmountEntryState";
            this.amountEntryStateTableData = (AmountEntryStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesAmountEntryState();
            this.prop = this.GetProperties<PropertiesAmountEntryState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.amountEntryStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.amountEntryStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.amountEntryStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKAorINextStateNumber))
                    this.prop.FDKAorINextStateNumber = this.amountEntryStateTableData.FDKAorINextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKBorHNextStateNumber))
                    this.prop.FDKBorHNextStateNumber = this.amountEntryStateTableData.FDKBorHNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKCorGNextStateNumber))
                    this.prop.FDKCorGNextStateNumber = this.amountEntryStateTableData.FDKCorGNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKDorFNextStateNumber))
                    this.prop.FDKDorFNextStateNumber = this.amountEntryStateTableData.FDKDorFNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.AmountDisplayScreenNumber))
                    this.prop.AmountDisplayScreenNumber = this.amountEntryStateTableData.AmountDisplayScreenNumber;
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
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                this.Core.Bo.ExtraInfo.Amount = 0;
                this.availableNotes = this.Core.Sdo.CashUnits.ToDictionary(cashUnit => cashUnit.Values.ToString(), cashUnit => cashUnit.Count);
                keyMask = new KeyMask_Type();
                if (this.Core.ScreenConfiguration.KeyboardEntryMode == KeyboardEntryMode_Type.none) //Solo activo las FDK cuando NO hay teclado en pantalla
                {
                    keyMask.FDKA = this.prop.FDKAorINextStateNumber.Equals("255") ? false : true;
                    keyMask.FDKB = this.prop.FDKBorHNextStateNumber.Equals("255") ? false : true;
                    keyMask.FDKC = this.prop.FDKCorGNextStateNumber.Equals("255") ? false : true;
                    keyMask.FDKD = this.prop.FDKDorFNextStateNumber.Equals("255") ? false : true;
                }
                this.CallHandler(this.prop.OnShowAmountEntryScreen);
                this.StartTimer();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerInputData(string dataInput, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", dataInput);
                this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataLink))
                {
                    switch (dataLink)
                    {
                        case "EnteredAmount":
                            {
                                if (CanDispenseAmount(Convert.ToDecimal(dataInput), availableNotes))
                                {
                                    this.Core.Bo.ExtraInfo.Amount = Convert.ToDecimal(dataInput);
                                    this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKAorINextStateNumber);
                                }
                                else
                                {
                                    this.prop.amountEntryDetails.PreviousAttempts++;
                                    this.prop.amountEntryDetails.RetryReason = "invalidAmount";
                                    //this.prop.OnShowAmountDetails.Parameters = this.GetAmountEntryConfig();
                                    this.ActivityStart();
                                }
                                this.amount = dataInput;
                                break;
                            }
                    }
                }
                this.timerScreen.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info($"/--> Key press: {othersKeys}");
            switch (othersKeys)
            {
                //case "ENTER":
                //    {
                //        Log.Error("Undefined");
                //        this.Core.Bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
                //        this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKAorINextStateNumber);
                //        break;
                //    }
                case "CANCEL":
                    {
                        this.Core.Bo.ExtraInfo.Amount = 0;
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case "REQUEST":
                    this.prop.OnShowAmountDetails.Parameters = this.GetAmountEntryConfig(); // Utilities.Utils.JsonSerialize(this.prop.amountEntryDetails);
                    this.CallHandler(this.prop.OnShowAmountDetails);
                    break;
            }
        }

        private void HandlerFDKreturn(string FDKcode)
        {
            string NextStateNumber = string.Empty;
            try
            {
                Log.Info($"-> FDK data: {FDKcode}");
                this.Core.Bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
                this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKDorFNextStateNumber);
                switch (FDKcode)
                {
                    case "A":
                        {
                            NextStateNumber = this.prop.FDKAorINextStateNumber;
                            break;
                        }
                    case "B":
                        {
                            NextStateNumber = this.prop.FDKBorHNextStateNumber;
                            break;
                        }
                    case "C":
                        {
                            NextStateNumber = this.prop.FDKCorGNextStateNumber;
                            break;
                        }
                    case "D":
                        {
                            NextStateNumber = this.prop.FDKDorFNextStateNumber;
                            break;
                        }
                    case "I":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                NextStateNumber = this.prop.FDKAorINextStateNumber;
                            break;
                        }
                    case "H":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                NextStateNumber = this.prop.FDKBorHNextStateNumber;
                            break;
                        }
                    case "G":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                NextStateNumber = this.prop.FDKCorGNextStateNumber;
                            break;
                        }
                    case "F":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                NextStateNumber = this.prop.FDKDorFNextStateNumber;
                            break;
                        }
                }
                if (!string.IsNullOrEmpty(NextStateNumber))
                    this.SetActivityResult(StateResult.SUCCESS, NextStateNumber);
                else
                    this.SetActivityResult(StateResult.SWERROR, NextStateNumber);
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
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Functions"

        /// <summary>
        /// Archivo json de configuracion para enviar al front al principio.
        /// Falta definir lo retry reason para los errores de host y de monto.
        /// </summary>
        /// <returns></returns>
        private string GetAmountEntryConfig()
        {
            string result = string.Empty;
            try
            {
                JObject jObject = new JObject
                {
                    ["previousAttempts"] = (JToken)prop.amountEntryDetails.PreviousAttempts,
                    ["retryReason"] = (JToken)prop.amountEntryDetails.RetryReason,
                    ["manualEntry"] = new JObject
                    {
                        ["enabled"] = (JToken)this.prop.amountEntryDetails.EnableKeyboard,
                        ["allowDecimals"] = (JToken)this.prop.amountEntryDetails.AllowDecimals,
                        ["maxLength"] = (JToken)this.prop.amountEntryDetails.MaxLength,
                        ["virtualPinpad"] = (JToken)this.prop.amountEntryDetails.VirtualPinpad
                        // TODO: teclado fisico o en pantalla
                    },
                    ["fastCash"] = new JObject
                    {
                        ["enabled"] = (JToken)this.prop.amountEntryDetails.EnableFastCash,
                        ["values"] = JArray.FromObject(CreateFastCashButtons(prop.amountEntryDetails.FastCashButtonsCount, availableNotes))
                    }
                };

                result = jObject.ToString(Formatting.None);
            }
            catch (Exception value) { Log.Fatal(value); }
            return result;
        }


        /// <summary>
        /// Detemina si es posible o no dispensar un monto.
        /// </summary>
        /// <param name="amount">Monto solicitado.</param>
        /// <param name="availableNotes">Diccionario de denominaciones/cantidad disponibles.</param>
        /// <returns></returns>
        public bool CanDispenseAmount(decimal amount, Dictionary<string, int> availableNotes)
        {
            // Convertir monto al menor entero (ej. centavos para ARS).
            int amountInCents = (int)(amount * 100);


            // Definir un diccionario con las denominaciones y sus valores en centavos.
            var denominations = new Dictionary<string, int>();
            foreach (var denom in availableNotes)
            {
                denominations[denom.Key] = Convert.ToInt32(denom.Key) * 100;
            }

            // Iterar por las denominaciones en orden descendente.
            foreach (var denomination in denominations.OrderByDescending(x => x.Value))
            {
                // Chequear si la denominación está disponible y si hay suficiente cantidad para dispensar el monto.
                if (availableNotes.ContainsKey(denomination.Key) && availableNotes[denomination.Key] > 0)
                {
                    int numNotes = Math.Min(amountInCents / denomination.Value, availableNotes[denomination.Key]);
                    amountInCents -= numNotes * denomination.Value;
                    availableNotes[denomination.Key] -= numNotes;
                }

                // Si el monto fue satisfecho por completo ✔️
                if (amountInCents == 0)
                {
                    return true;
                }
            }

            // Si no se pudo ❌
            return false;
        }

        /// <summary>
        /// Genera botones FastCash para enviar al frontend, dependiendo de los bileltes/denominaciones disponibles.
        /// </summary>
        /// <param name="numButtons">Numero de botones a crear.</param>
        /// <param name="availableNotes">Diccionario de denominaciones/cantidad disponibles.</param>
        /// <returns></returns>
        private static List<decimal> CreateFastCashButtons(int numButtons, Dictionary<string, int> availableNotes)
        {

            var fastCashButtons = new List<decimal>();
            int multiplier = 1;

            // Ponemos las denominaciones en un array.
            string[] denominations = availableNotes.Keys.ToArray();

            // Seteamos numerador a uno y vamos aumentando.
            while (fastCashButtons.Count < numButtons)
            {
                // Recorremos el array de denoms.
                foreach (var denomination in denominations)
                {
                    // Chequeamos si existe y si hay suficiente segun multiplicador.
                    if (availableNotes.ContainsKey(denomination) && availableNotes[denomination] >= multiplier)
                    {
                        decimal buttonAmount = Convert.ToInt32(denomination) * multiplier;
                        // Chequeamos que no este duplicado.
                        if (!fastCashButtons.Contains(buttonAmount))
                        {
                            fastCashButtons.Add(buttonAmount);
                            // Chequeamos si no se cumple el maximo de botones.
                            if (fastCashButtons.Count >= numButtons)
                            {
                                fastCashButtons.Sort();
                                return fastCashButtons;
                            }
                        }
                    }
                }
                multiplier++;
            }
            fastCashButtons.Sort();
            return fastCashButtons;
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
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }
}
