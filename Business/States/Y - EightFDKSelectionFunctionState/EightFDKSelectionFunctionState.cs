using Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Business.EightFDKSelectionFunctionState
{
    /// <summary>
    /// Set the OperationalCode buffer
    /// </summary>
    public class EightFDKSelectionFunctionState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        EightFDKSelectionFunctionStateTableData_Type eightFDKSelectionFunctionStateTableData; //Tabla con datos provenientes del download.
        PropertiesEightFDKSelectionFunctionState prop;
        bool ret = false;

        #region "Constructor"
        public EightFDKSelectionFunctionState(StateTable_Type stateTable)
        {
            this.ActivityName = "EightFDKSelectionFunctionState";
            this.eightFDKSelectionFunctionStateTableData = (EightFDKSelectionFunctionStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesEightFDKSelectionFunctionState();
            EightFDKSelectionFunctionStateTableDataExtension_Type extensionTable = null;
            this.prop = this.GetProperties<PropertiesEightFDKSelectionFunctionState>(out ret, this.prop);
            if (ret)
            {
                if (this.eightFDKSelectionFunctionStateTableData.Item != null)
                    extensionTable = (EightFDKSelectionFunctionStateTableDataExtension_Type)this.eightFDKSelectionFunctionStateTableData.Item;
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.eightFDKSelectionFunctionStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.eightFDKSelectionFunctionStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.eightFDKSelectionFunctionStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKNextStateNumber))
                    this.prop.FDKNextStateNumber = this.eightFDKSelectionFunctionStateTableData.FDKNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.OperationCodeBufferPositions))
                    this.prop.OperationCodeBufferPositions = this.eightFDKSelectionFunctionStateTableData.OperationCodeBufferPositions;
                if (this.prop.ActiveFDKs == null)
                    this.prop.ActiveFDKs = this.eightFDKSelectionFunctionStateTableData.ActiveFDKs;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKA) && extensionTable != null)
                    this.prop.Extension.CodeFDKA = extensionTable.CodeFDKA;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKB) && extensionTable != null)
                    this.prop.Extension.CodeFDKB = extensionTable.CodeFDKB;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKC) && extensionTable != null)
                    this.prop.Extension.CodeFDKC = extensionTable.CodeFDKC;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKD) && extensionTable != null)
                    this.prop.Extension.CodeFDKD = extensionTable.CodeFDKD;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKF) && extensionTable != null)
                    this.prop.Extension.CodeFDKF = extensionTable.CodeFDKF;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKG) && extensionTable != null)
                    this.prop.Extension.CodeFDKG = extensionTable.CodeFDKG;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKH) && extensionTable != null)
                    this.prop.Extension.CodeFDKH = extensionTable.CodeFDKH;
                if (string.IsNullOrEmpty(this.prop.Extension.CodeFDKI) && extensionTable != null)
                    this.prop.Extension.CodeFDKI = extensionTable.CodeFDKI;
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
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                //Armado de pantalla
                this.CallHandler(this.prop.OnEightFDKSelection);
                if (this.prop.OnShowAvailableTransactions.Action == StateEvent.EventType.runScript)
                    this.prop.OnShowAvailableTransactions.Parameters = SendAvailableTransactions(this.prop.ScreenNumber);
                this.CallHandler(this.prop.OnShowAvailableTransactions);
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
                        //if (this.prop.OnShowAvailableTransactions.Action == StateEvent.EventType.runScript)
                        //{
                        //    this.prop.OnShowAvailableTransactions.Parameters = SendAvailableTransactions(this.prop.ScreenNumber);
                        //}

                        //this.CallHandler(this.prop.OnShowAvailableTransactions);
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
            bool flag;
            try
            {
                Log.Info($"-> FDK data: {FDKcode}");
                if (this.WithExtension()) //Extensión para lenguajes (not in use)
                {
                    flag = this.SetExtensionOpCode();
                }
                else
                {
                    flag = this.EightFDK_SetOPCode();
                }
                if (flag)
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKNextStateNumber);
                else
                    Log.Error(string.Format("->Input error."));
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
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Functions"
        private bool EightFDK_SetOPCode()
        {
            int position;
            try
            {
                Log.Debug("/--->");
                char[] array = this.Core.Bo.ExtraInfo.OperationCodeData.ToCharArray();
                if (int.TryParse(this.prop.OperationCodeBufferPositions, out position))
                {
                    if (position < 8)
                    {
                        array[position] = Convert.ToChar(this.Core.Bo.LastFDKPressed);//Setea la posición para la FDK presionada
                        string operationCodeData = new string(array);
                        this.Core.Bo.ExtraInfo.OperationCodeData = operationCodeData;
                    }
                    else
                        Log.Error($"->Index out of range: {position}");
                }
                else
                    Log.Error($"->Index not numeric: {position}");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }
            return true;
        }

        private bool SetExtensionOpCode()
        {
            string lastFDKPressed;
            Log.Debug("/--->");
            switch (lastFDKPressed = this.Core.Bo.LastFDKPressed)
            {
                case "A":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKA);
                case "B":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKB);
                case "C":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKC);
                case "D":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKD);
                case "F":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKF);
                case "G":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKG);
                case "H":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKH);
                case "I":
                    return this.EightFDK_ExtSetOpCode(this.prop.Extension.CodeFDKI);
            }
            return false;
        }

        /// <summary>
        /// Set OperationalCodeBuffer
        /// </summary>
        /// <param name="OpCodeChar"></param>
        /// <returns></returns>
        private bool EightFDK_ExtSetOpCode(string OpCodeChar)
        {
            try
            {
                Log.Debug("/--->");
                char[] array = this.Core.Bo.ExtraInfo.OperationCodeData.ToCharArray();
                for (int i = 0; i < 3; i++)
                {
                    int num = int.Parse(this.prop.OperationCodeBufferPositions.Substring(i, 1));
                    string opCodeCharacter = this.GetOpCodeCharacter(OpCodeChar.Substring(i, 1));
                    if (opCodeCharacter != "@")
                    {
                        array[num] = Convert.ToChar(opCodeCharacter);
                    }
                }
                string operationCodeData = new string(array);
                this.Core.Bo.ExtraInfo.OperationCodeData = operationCodeData;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return true;
        }

        private string GetOpCodeCharacter(string op)
        {
            string result;
            switch (op)
            {
                case "0":
                    result = "@";
                    return result;
                case "1":
                    result = "A";
                    return result;
                case "2":
                    result = "B";
                    return result;
                case "3":
                    result = "C";
                    return result;
                case "4":
                    result = "D";
                    return result;
                case "6":
                    result = "F";
                    return result;
                case "7":
                    result = "G";
                    return result;
                case "8":
                    result = "H";
                    return result;
                case "9":
                    result = "I";
                    return result;
            }
            result = "@";
            return result;
        }

        private bool WithExtension()
        {
            return this.prop.Extension.CodeFDKA != null && this.prop.Extension.CodeFDKB != null && this.prop.Extension.CodeFDKC != null && this.prop.Extension.CodeFDKD != null && this.prop.Extension.CodeFDKF != null && this.prop.Extension.CodeFDKG != null && this.prop.Extension.CodeFDKH != null && this.prop.Extension.CodeFDKI != null;
        }

        private string SendAvailableTransactions(string currentScreen)
        {
            string result = string.Empty;
            this.prop.avTransactions.Clear();

            switch (currentScreen)
            {
                case "002": //RTLA - With Barcode
                    if (this.prop.ActiveFDKs.FDKB)
                        this.prop.avTransactions.Add(new TransactionMenuItem("withoutBarcode", Const.Colors.Primary, Const.Icons.Keyboard, 'B'));
                    if (this.prop.ActiveFDKs.FDKA)
                        this.prop.avTransactions.Add(new TransactionMenuItem("withBarcode", Const.Colors.Primary, Const.Icons.Barcode, 'A'));
                    break;
                case "003": //RTLA - Without Barcode
                    if (this.prop.ActiveFDKs.FDKB)
                        this.prop.avTransactions.Add(new TransactionMenuItem("depositEnvelope", Const.Colors.Primary, Const.Icons.Envelope, 'B'));
                    if (this.prop.ActiveFDKs.FDKA)
                        this.prop.avTransactions.Add(new TransactionMenuItem("depositCash", Const.Colors.Primary, Const.Icons.Money, 'A'));
                    break;
                case "004": //RTLB
                    if (this.prop.ActiveFDKs.FDKA)
                        this.prop.avTransactions.Add(new TransactionMenuItem("verifyNotes", Const.Colors.Primary, Const.Icons.Search, 'A'));
                    if (this.prop.ActiveFDKs.FDKB)
                        this.prop.avTransactions.Add(new TransactionMenuItem("login", Const.Colors.Primary, Const.Icons.Login, 'B'));
                    break;
                case "005": //PAYA
                    if (this.prop.ActiveFDKs.FDKA)
                        this.prop.avTransactions.Add(new TransactionMenuItem("cashDepositTx", Const.Colors.Primary, Const.Icons.Money, 'A', false));
                    if (this.prop.ActiveFDKs.FDKB)
                        this.prop.avTransactions.Add(new TransactionMenuItem("cashDispenseTx", Const.Colors.Primary, Const.Icons.Login, 'B', false));
                    break;
                default:
                    Log.Warn($"->Current screen number not declared to offer options: {currentScreen}");
                    break;
            }

            try
            {
                JObject jObject = new JObject
                {
                    ["showBagStatusBar"] = (JToken)this.prop.EnableBagStatusBar,
                    ["showExit"] = (JToken)this.prop.EnableExitButton,
                    ["list"] = JArray.FromObject(prop.avTransactions)
                };

                result = jObject.ToString(Formatting.None);
            }
            catch (Exception value)
            {

                Log.Fatal(value);
            }
            return result;
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
