using Entities;
//using External_Interface.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Business.InformationEntryState
{
    public class InformationEntryState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        InformationEntryStateTableData_Type informationEntryStateTableData; //Tabla con datos provenientes del download.
        PropertiesInformationEntryState prop;
        string amount = "0";
        bool ret = false;

        #region "Constructor"
        public InformationEntryState(StateTable_Type stateTable)
        {
            this.ActivityName = "InformationEntryState";
            this.informationEntryStateTableData = (InformationEntryStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesInformationEntryState();
            this.prop = this.GetProperties<PropertiesInformationEntryState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.informationEntryStateTableData.ScreenNumber;
                if(string.IsNullOrEmpty(this.prop.SetGoodNextStateNumber))
                    this.prop.SetGoodNextStateNumber = this.informationEntryStateTableData.SetGoodNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.informationEntryStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.informationEntryStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKAorINextStateNumber))
                    this.prop.FDKAorINextStateNumber = this.informationEntryStateTableData.FDKANextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKBorHNextStateNumber))
                    this.prop.FDKBorHNextStateNumber = this.informationEntryStateTableData.FDKBNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKCorGNextStateNumber))
                    this.prop.FDKCorGNextStateNumber = this.informationEntryStateTableData.FDKCNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKDorFNextStateNumber))
                    this.prop.FDKDorFNextStateNumber = this.informationEntryStateTableData.FDKDNextStateNumber;
                if(string.IsNullOrEmpty(this.prop.OperationMode))
                    this.prop.OperationMode = this.informationEntryStateTableData.OperationMode;
                if (this.prop.EntryModeAndBufferConfiguration == null)
                    this.prop.EntryModeAndBufferConfiguration = this.informationEntryStateTableData.EntryModeAndBufferConfiguration;
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
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                keyMask = new KeyMask_Type();

                this.prop.LoadDefaultConfiguration(this.Core.AlephATMAppData);

                OperationMode opmode = (OperationMode)Enum.Parse(typeof(OperationMode), this.prop.OperationMode);


                if (opmode == OperationMode.HolderAccountNo || opmode == OperationMode.HolderDocTypeAndNumber)
                {
                    //if (BankConfiguration.GetMapping(this.Core.AlephATMAppData, out BankConfiguration bankConf))
                    //{
                    //    if (bankConf.WithCtaDefault)
                    //    {
                    //        this.SetActivityResult(StateResult.SUCCESS, this.prop.SetGoodNextStateNumber);
                    //        return;
                    //    }
                    //}
                }

                switch (opmode)
                {
                    case OperationMode.PhoneNumberInfo:
                        this.prop.DestinationKey = "PhoneNumberInfo";
                        this.prop.infoEntryDetails = new InfoEntryDetails(true, "language.infoEntry.tag.phoneNumber", "numeric", 7, 7, "09", "customerPhoneNumber");
                        break;
                    case OperationMode.HolderAccountNo:
                        this.prop.DestinationKey = "holderAccountNo";
                        this.prop.infoEntryDetails = new InfoEntryDetails(true, "language.infoEntry.tag.holderAccountNo", "numeric", 16, 16, "", "depositariosAccountNo");
                        break;
                    case OperationMode.HolderDocTypeAndNumber:
                        this.prop.DestinationKey = "holderDocNo";
                        this.prop.SecDestinationKey = "holderDocType";
                        SecondaryField secField = new SecondaryField(
                            new List<SecondaryFieldOptions>
                            {
                                new SecondaryFieldOptions
                                {
                                    LanguageTag = "",
                                    LiteralTag = "CI",
                                    Value = "CI",
                                    Selected = true
                                },
                                new SecondaryFieldOptions
                                {
                                    LanguageTag = "",
                                    LiteralTag = "RUC",
                                    Value = "RUC",
                                    Selected = false
                                }
                            },
                            "language.extraData.tag.docType",
                            "");
                        this.prop.infoEntryDetails = new InfoEntryDetails(true, "language.infoEntry.tag.holderTypeNo", "numeric", 16, 3, "", "holderDocTypeAndNumber", secField);
                        break;
                    case OperationMode.OperatorDocTypeAndNumber:
                        this.prop.DestinationKey = "depositorDocNo";
                        this.prop.SecDestinationKey = "depositorDocType";
                        SecondaryField secField2 = new SecondaryField(
                            new List<SecondaryFieldOptions>
                            {
                                new SecondaryFieldOptions
                                {
                                    LanguageTag = "",
                                    LiteralTag = "CI",
                                    Value = "CI",
                                    Selected = true
                                },
                                new SecondaryFieldOptions
                                {
                                    LanguageTag = "",
                                    LiteralTag = "RUC",
                                    Value = "RUC",
                                    Selected = false
                                }
                            },
                            "language.extraData.tag.docType",
                            "");
                        this.prop.infoEntryDetails = new InfoEntryDetails(true, "language.infoEntry.tag.opTypeNo", "numeric", 16, 3, "", "holderDocTypeAndNumber", secField2);
                        break;
                    default:
                        break;
                }

                

                this.StartTimer();
                this.CallHandler(this.prop.OnShowScreen);
                IsUserNotificationPending();
                this.prop.OnSendScreenData.Parameters = this.GetInfoEntryConfig();
                this.CallHandler(this.prop.OnSendScreenData);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerInputData(string keyCode, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", keyCode);
                this.timerScreen.Stop();
                if(this.prop.EntryModeAndBufferConfiguration != null)
                {
                    switch (this.prop.EntryModeAndBufferConfiguration.DisplayAndBufferParameters)
                    {
                        case DisplayAndBufferParameters_Type.DisplayDataKeyedIn_StoreDataInBufferB:
                        case DisplayAndBufferParameters_Type.DisplayX_StoreDataInBufferB:
                            {
                                this.Core.Bo.ExtraInfo.BufferB = keyCode;
                                break;
                            }
                        case DisplayAndBufferParameters_Type.DisplayDataKeyedIn_StoreDataInBufferC:
                        case DisplayAndBufferParameters_Type.DisplayX_StoreDataInBufferC:
                            {
                                this.Core.Bo.ExtraInfo.BufferC = keyCode;
                                break;
                            }
                        default:
                            {
                                Log.Error(string.Format("-> Error on DisplayAndBufferParameters: {0}", this.prop.EntryModeAndBufferConfiguration.DisplayAndBufferParameters.ToString()));
                                break;
                            }
                    }
                }
                if(!string.IsNullOrEmpty(dataLink))
                {
                    switch (dataLink)
                    {
                        case "EnteredInfo":
                            if(!string.IsNullOrEmpty(keyCode))
                            {
                                if(this.Core.Bo.ExtraInfo.ExtraData == null)
                                {
                                    this.Core.Bo.ExtraInfo.ExtraData = new List<ExtraData>();
                                }

                                JObject json = JObject.Parse(keyCode);
                                json.TryGetValue("data", out JToken fieldData);
                                this.Core.AddHostExtraData(this.prop.DestinationKey, fieldData.ToString());
                                if (this.prop.infoEntryDetails.SecondaryField != null) // hay campo secundario
                                {
                                    json.TryGetValue("secondaryField", out JToken secondaryField);
                                    this.Core.AddHostExtraData(this.prop.SecDestinationKey, secondaryField.ToString());
                                }
                                
                            }
                            Log.Info("-> EnteredInfo: {0}", keyCode);
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.SetGoodNextStateNumber);
                            break;
                    }
                }
                this.timerScreen.Start();
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
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKAorINextStateNumber);
                        break;
                    }
                case "CANCEL":
                    {
                        if (this.prop.EntryModeAndBufferConfiguration != null)
                        {
                            switch (this.prop.EntryModeAndBufferConfiguration.DisplayAndBufferParameters)
                            {
                                case DisplayAndBufferParameters_Type.DisplayDataKeyedIn_StoreDataInBufferB:
                                case DisplayAndBufferParameters_Type.DisplayX_StoreDataInBufferB:
                                    {
                                        this.Core.Bo.ExtraInfo.BufferB = string.Empty;
                                        break;
                                    }
                                case DisplayAndBufferParameters_Type.DisplayDataKeyedIn_StoreDataInBufferC:
                                case DisplayAndBufferParameters_Type.DisplayX_StoreDataInBufferC:
                                    {
                                        this.Core.Bo.ExtraInfo.BufferC = string.Empty;
                                        break;
                                    }
                                default:
                                    {
                                        Log.Error(string.Format("-> Error on DisplayAndBufferParameters: {0}", this.prop.EntryModeAndBufferConfiguration.DisplayAndBufferParameters.ToString()));
                                        break;
                                    }
                            }
                        }
                        this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                        break;
                    }
            }
        }

        private void HandlerFDKreturn(string FDKcode)
        {
            try
            {
                Log.Info("-> FDK data: {0}", FDKcode);
                this.Core.Bo.ExtraInfo.Amount = Utilities.Utils.GetDecimalAmount(this.amount);
                this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKDorFNextStateNumber);
                switch (FDKcode)
                {
                    case "A":
                        {
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKAorINextStateNumber);
                            break;
                        }
                    case "B":
                        {
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKBorHNextStateNumber);
                            break;
                        }
                    case "C":
                        {
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKCorGNextStateNumber);
                            break;
                        }
                    case "D":
                        {
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKDorFNextStateNumber);
                            break;
                        }
                    case "I":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKAorINextStateNumber);
                            break;
                        }
                    case "H":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKBorHNextStateNumber);
                            break;
                        }
                    case "G":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKCorGNextStateNumber);
                            break;
                        }
                    case "F":
                        {
                            if (this.Core.ScreenConfiguration.Digit7aEnable)
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.FDKDorFNextStateNumber);
                            break;
                        }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal bool IsUserNotificationPending()
        {
            if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.PendingUserNotification))
            {
                //call to display notif to user
                this.CallHandler(new StateEvent(StateEvent.EventType.runScript, "UserNotificationModal", this.Core.Bo.ExtraInfo.PendingUserNotification));
                this.Core.Bo.ExtraInfo.PendingUserNotification = null;//clear
                return true;
            }
            return false;
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
        /// Archivo json de configuracion para enviar al front al inicio.
        /// </summary>
        /// <returns></returns>
        private string GetInfoEntryConfig()
        {
            string result = string.Empty;
            try
            {
                JObject secInput = null;
                if (this.prop.infoEntryDetails.SecondaryField != null)
                    secInput = JObject.FromObject(this.prop.infoEntryDetails.SecondaryField);

                JObject jObject = new JObject
                {
                    ["enableScreenKeyboard"] = (JToken)this.prop.infoEntryDetails.EnableScreenKeyboard,// TODO: teclado fisico o en pantalla
                    ["pageTitle"] = (JToken)this.prop.infoEntryDetails.PageTitleTag,
                    ["keyboardMode"] = (JToken)this.prop.infoEntryDetails.KeyboardMode,
                    ["maxLength"] = (JToken)this.prop.infoEntryDetails.MaxLength,
                    ["minLength"] = (JToken)this.prop.infoEntryDetails.MinLength,
                    ["prefix"] = (JToken)this.prop.infoEntryDetails.InputPrefix,
                    ["screenMode"] = (JToken)this.prop.infoEntryDetails.ScreenMode,
                    ["secondaryField"] = secInput
                };

                result = jObject.ToString(Formatting.None);
            }
            catch (Exception value) { Log.Fatal(value); }
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
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }

    public enum OperationMode
    {
        PhoneNumberInfo = 1,
        HolderAccountNo = 2,
        HolderDocTypeAndNumber = 3,
        OperatorDocTypeAndNumber = 4,
    }
}
