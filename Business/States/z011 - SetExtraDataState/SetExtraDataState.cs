using Entities;
using Entities.PaymentService;
//using External_Interface.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;

namespace Business.SetExtraDataState
{
    public class SetExtraDataState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        SetExtraDataStateTableData_Type SetExtraDataStateTableData; //Tabla con datos provenientes del download.
        PropertiesSetExtraDataState prop;
        bool ret = false;
        private bool SetCurrencyEnabled;
        private bool SetChannelEnabled;
        private bool SetTxInfoEnabled;
        private bool SetTxRefEnabled;
        private bool SetShiftsEnabled;
        private bool SetAmountLimitEnabled;
        private bool MoreTimeSubscribed = false;
        private AlephATMAppData alephATMAppData;

        #region "Constructor"
        public SetExtraDataState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "SetExtraDataState";
            this.SetExtraDataStateTableData = (SetExtraDataStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesSetExtraDataState();
            SetExtraDataStateTableExtension1_Type extensionTable1 = null;
            this.prop = this.GetProperties<PropertiesSetExtraDataState>(out ret, this.prop);
            this.alephATMAppData = alephATMAppData;
            if (ret)
            {
                if (this.prop.ExtraDataConfigurations.Count == 0)
                {
                    this.prop.LoadDefaultConfiguration(alephATMAppData);
                    string pathFile = $"{Const.appPath}StatesSets\\Properties{this.ActivityName}.xml";
                    System.IO.File.Delete(pathFile);
                    this.GetProperties<PropertiesSetExtraDataState>(out ret, this.prop);
                }
                if (this.SetExtraDataStateTableData.Item != null)
                    extensionTable1 = (SetExtraDataStateTableExtension1_Type)this.SetExtraDataStateTableData.Item;
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.SetExtraDataStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.SetGoodNextStateNumber))
                    this.prop.SetGoodNextStateNumber = this.SetExtraDataStateTableData.SetGoodNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.SetExtraDataStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.SetExtraDataStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.SetExtraDataStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.SetExtraDataStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetChannelEnabled) && extensionTable1 != null)
                    this.prop.Extension1.SetChannelEnabled = extensionTable1.SetChannelEnabled;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetTransactionInfoEnabled) && extensionTable1 != null)
                    this.prop.Extension1.SetTransactionInfoEnabled = extensionTable1.SetTransactionInfoEnabled;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetTransactionRefEnabled) && extensionTable1 != null)
                    this.prop.Extension1.SetTransactionRefEnabled = extensionTable1.SetTransactionRefEnabled;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetShiftsEnabled) && extensionTable1 != null)
                    this.prop.Extension1.SetShiftsEnabled = extensionTable1.SetShiftsEnabled;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetCurrencyEnabled) && extensionTable1 != null)
                    this.prop.Extension1.SetCurrencyEnabled = extensionTable1.SetCurrencyEnabled;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetAmountLimitEnabled) && extensionTable1 != null)
                    this.prop.Extension1.SetAmountLimitEnabled = extensionTable1.SetAmountLimitEnabled;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetDenominations7384) && extensionTable1 != null)
                    this.prop.Extension1.SetDenominations7384 = extensionTable1.SetDenominations7384;
                if (string.IsNullOrEmpty(this.prop.Extension1.SetDenominations8596) && extensionTable1 != null)
                    this.prop.Extension1.SetDenominations8596 = extensionTable1.SetDenominations8596;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
        }
        #endregion "Constructor"


        /// <summaryExtraInfo.SetExtraDataFields
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
                MainEventsSubscription(true);

                this.EnableJournal = this.prop.Journal.EnableJournal;
                WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                this.SetCurrencyEnabled = this.prop.Extension1.SetCurrencyEnabled.Equals("001") ? true : false;
                this.SetChannelEnabled = this.prop.Extension1.SetChannelEnabled.Equals("001") ? true : false;
                this.SetTxInfoEnabled = this.prop.Extension1.SetTransactionInfoEnabled.Equals("001") ? true : false;
                this.SetTxRefEnabled = this.prop.Extension1.SetTransactionRefEnabled.Equals("001") ? true : false;
                this.SetShiftsEnabled = this.prop.Extension1.SetShiftsEnabled.Equals("001") ? true : false;
                this.SetAmountLimitEnabled = this.prop.Extension1.SetAmountLimitEnabled.Equals("001") ? true : false;

                //especifico depositarios
                if(this.alephATMAppData.Branding == Enums.Branding.Atlas
                    || this.alephATMAppData.Branding == Enums.Branding.GNB
                    || this.alephATMAppData.Branding == Enums.Branding.FIC) //cambiar a FIC o necesarios
                {
                    //if(BankConfiguration.GetMapping(alephATMAppData, out BankConfiguration bankConf))
                    //{

                        if(this.Core.Bo.ExtraInfo.SetExtraDataFields == null)
                        {
                            this.Core.Bo.ExtraInfo.SetExtraDataFields = new List<ExtraDataConf>();
                        }

                        //if(bankConf.Bank == "FIC")
                        //{
                        //    //inhabilitar campos canal
                        //    //this.prop.ExtraDataConfigurations.Where(x => x.extraDataType == Enums.ExtraDataType.channel).ToList().ForEach(x => x.enabled = false);
                           
                        //    var accountNo =  this.Core.Bo.ExtraInfo.HostExtraData.FirstOrDefault(x => x.Key == "holderDocNo");

                        //    if (accountNo.Value == null && !bankConf.WithCtaDefault ) //sin cta titular && string.IsNullOrEmpty(accountNo.TagValue))
                        //    {
                        //        this.prop.ScreenTitle = "language.extraData.tag.enterHolderData";
                        //        //this.prop.ExtraDataConfigurations.Add(new ExtraDataConf { extraDataType = Enums.ExtraDataType.dynamic, enabled = true, name = "holderAccountNo", controlType = "input", controlModel = "numeric", label = "Número de cuenta", maxLength = 16, minLength = 11, editable = true, required = true, index = 1 }) ;
                        //        int holderDocTypeId = 1;
                        //        FieldOption[] holderDocTypeOptions = new FieldOption[]
                        //        {
                        //            new FieldOption { Value = "ci", DisplayName = "CI", HelpText = "language.extraData.tag.placeholderDocType" },
                        //            new FieldOption { Value = "ruc", DisplayName = "RUC" }
                        //        };
                        //        FieldOption[] holderDocNoOptions = new FieldOption[]
                        //        {
                        //            new FieldOption { HelpText = "language.extraData.tag.placeholderDocNo" },
                        //        };
                        //        int holderDocNoId = 2;
                        //        string holderDocTypeOpts = CreateOptionsString(holderDocTypeId, "holderDocType", "language.extraData.tag.docType", holderDocTypeOptions);
                        //        string holderDocNoOpts = CreateOptionsString(holderDocNoId, "holderDocNo", "language.extraData.tag.docNo", holderDocNoOptions);
                        //        this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(
                        //            new ExtraDataConf
                        //            {
                        //                extraDataType = Enums.ExtraDataType.dynamic,
                        //                enabled = true, name = "holderDocType",
                        //                controlType = "radio", 
                        //                controlModel = "",
                        //                label = "language.extraData.tag.docType",
                        //                options = new string[] { holderDocTypeOpts },
                        //                editable = true,
                        //                required = true,
                        //                index = holderDocTypeId
                        //            });
                        //        this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(
                        //            new ExtraDataConf {
                        //                extraDataType = Enums.ExtraDataType.dynamic,
                        //                enabled = true, 
                        //                name = "holderDocNo", 
                        //                controlType = "input", 
                        //                controlModel = "numeric",
                        //                label = "language.extraData.tag.docNo", 
                        //                options = new string[] { holderDocNoOpts }, 
                        //                minLength = 3, 
                        //                maxLength = 15,
                        //                editable = true, 
                        //                required = true, 
                        //                index = holderDocNoId
                        //            });
                        //    }
                        //    else
                        //    {
                        //        var depositorDocNo = this.Core.Bo.ExtraInfo.HostExtraData.FirstOrDefault(x => x.Key == "depositorDocNo");
                        //        if(depositorDocNo.Value == null)
                        //        {
                        //            int depositorDocTypeId = 1;
                        //            FieldOption[] depositorDocTypeOptions = new FieldOption[]
                        //            {
                        //                new FieldOption { Value = "ci", DisplayName = "CI" , HelpText = "language.extraData.tag.placeholderDocType"},
                        //                new FieldOption { Value = "ruc", DisplayName = "RUC" }
                        //            };
                        //            FieldOption[] depositorDocNoOptions = new FieldOption[]
                        //            {
                        //                new FieldOption { HelpText = "language.extraData.tag.placeholderDocNo" },
                        //            };
                        //            int depositorDocNoId = 2;
                        //            this.prop.ScreenTitle = "language.extraData.tag.enterDepositorData";
                        //            string depositorDocTypeOpts = CreateOptionsString(depositorDocTypeId, "depositorDocType", "language.extraData.tag.docType", depositorDocTypeOptions);
                        //            string depositorDocNoOpts = CreateOptionsString(depositorDocNoId, "depositorDocNo", "language.extraData.tag.docNo", depositorDocNoOptions);
                        //            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(
                        //                new ExtraDataConf
                        //                {
                        //                    extraDataType = Enums.ExtraDataType.dynamic,
                        //                    enabled = true,
                        //                    name = "depositorDocType",
                        //                    controlType = "radio",
                        //                    controlModel = "",
                        //                    label = "language.extraData.tag.docType",
                        //                    options = new string[] { depositorDocTypeOpts },
                        //                    editable = true,
                        //                    required = true,
                        //                    index = depositorDocTypeId
                        //                });
                        //            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(
                        //                new ExtraDataConf
                        //                {
                        //                    extraDataType = Enums.ExtraDataType.dynamic,
                        //                    enabled = true,
                        //                    name = "depositorDocNo",
                        //                    controlType = "input",
                        //                    controlModel = "numeric",
                        //                    label = "language.extraData.tag.docNo",
                        //                    options = new string[] { depositorDocNoOpts },
                        //                    maxLength = 15,
                        //                    editable = true,
                        //                    required = true,
                        //                    index = depositorDocNoId
                        //                });

                        //        }

                        //    }
                        //}
                    //}
                }

                this.LoadDynamicFields();

                ConfigurationData extraDataConfs = this.GetConfigurationData();
                var enabledList = extraDataConfs.ExtraDataList.Where(x => x.enabled == true).ToList();
                if (enabledList.Count == 0)//Si todos los campos están deshabilitados no muestro la pantalla
                {
                    Log.Info("No extra data available");
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.SetGoodNextStateNumber);
                }
                else
                {
                    if (this.prop.OnShowSetExtraDataScreen.Action == StateEvent.EventType.ndcScreen)
                        this.prop.OnShowSetExtraDataScreen.HandlerName = this.prop.ScreenNumber;
                    this.CallHandler(this.prop.OnShowSetExtraDataScreen);
                    this.StartTimer();

                    IsUserNotificationPending();
                    if (this.prop.OnConfigurationScreenData.Action == StateEvent.EventType.runScript)
                        this.prop.OnConfigurationScreenData.Parameters = Utilities.Utils.NewtonsoftSerialize(extraDataConfs);
                    this.CallHandler(this.prop.OnConfigurationScreenData);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal bool IsUsingDynamicFields()
        {
            bool dynamicOptions = this.Core.Bo.ExtraInfo?.SetExtraDataFields != null && this.Core.Bo.ExtraInfo?.SetExtraDataFields.Count > 0;
            if (dynamicOptions && this.Core.Bo.ExtraInfo.SetExtraDataFields.Any(a => a.extraDataType == Enums.ExtraDataType.dynamic)) //log options to electronic journal
            {
                var sb = new StringBuilder();
                this.Core.Bo.ExtraInfo.SetExtraDataFields?.Where(a => a.extraDataType == Enums.ExtraDataType.dynamic)?.ToList().ForEach(dt =>
                {
                    sb.Append($"\n\tName: {dt.name} | Control type: {dt.controlType} | Index: {dt.index}");
                });
                this.WriteEJ("Screen of options presented to user: " + sb.ToString());
            }
            return dynamicOptions;
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

        /// <summary>
        /// Load dynamic fields received from previous state.
        /// </summary>
        internal void LoadDynamicFields()
        {
            //remove previous dynamic fields in case they're still present
            this.prop.ExtraDataConfigurations.RemoveAll(p => p.extraDataType == Enums.ExtraDataType.dynamic || p.extraDataType == Enums.ExtraDataType.displayOnly);
            //add new dynamic fields
            if (IsUsingDynamicFields())
                this.prop.ExtraDataConfigurations.AddRange(this.Core.Bo.ExtraInfo.SetExtraDataFields);
        }

        /// <summary>
        /// Create options in stringified json PAE format for dynamic fields.
        /// </summary>
        /// <param name="fieldName">Identifier string, no spaces</param>
        /// <param name="fieldDisplayName">Field name to display in frontend</param>
        /// <param name="optionsNames">Array of options to display, for selects</param>
        /// <returns></returns>
        private static string CreateOptionsString(int id, string fieldName, string fieldDisplayName, FieldOption[] optionsNames = null)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");
            if (optionsNames != null)
            {
                jsonBuilder.Append("\"Options\":[");

                for (int i = 0; i < optionsNames.Length; i++)
                {
                    jsonBuilder.Append("{\"OptionID\": \"" + (i + 1) + "\", \"Fields\": [{\"ID\": \"" + (i + 1) + "\", \"Name\": \"" + fieldName + "\", \"Value\": \"" + optionsNames[i].DisplayName + "\", \"Type\": \"input\"}]}");

                    if (i < optionsNames.Length - 1)
                    {
                        jsonBuilder.Append(", ");
                    }
                }
                jsonBuilder.Append("], ");
            }
            jsonBuilder.Append("\"ID\": \"" + id + "\", \"Name\": \"" + fieldName + "\", \"Type\": \"Select\", \"DisplayText\": \"" + fieldDisplayName + "\", \"HelpText\": \"" + optionsNames[0].HelpText + "\"");
            jsonBuilder.Append("}");

            return jsonBuilder.ToString();
        }

        private static ExtraDataConf CreateDynamicObj(ExtraDataConf edf)
        {
            return edf;
        }

        /// <summary>
        /// Recibe los datos cargados por el cliente
        /// </summary>
        /// <param name="dataInput"></param>
        /// <param name="dataLink"></param>
        private void HandlerInputData(string dataInput, string dataLink)
        {
            string channel = string.Empty;
            //List<ExtraDataInfo> extraDataInfo = new List<ExtraDataInfo>();
            //bool res = false;
            this.ResetTimer();
            try
            {
                Log.Info($"-> Input data: {dataInput}");
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "ExtraData":
                            Dictionary<string, string> extraDataResponse = Utilities.Utils.NewtonsoftDeserialize<Dictionary<string, string>>(out bool res, dataInput);
                            if (!res)
                            {
                                Log.Error("ExtraData empty");
                                this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                                break;
                            }
                            //Recorro y guardo los extraDatas recibidos
                            if(this.Core.Bo.ExtraInfo.ExtraData == null)
                            {
                                this.Core.Bo.ExtraInfo.ExtraData = new List<ExtraData>();
                            }
                            foreach (var edr in extraDataResponse)
                            {
                                if (edr.Key.Equals("currencySelection"))
                                {
                                    GlobalAppData.Instance.SetScratchpad("currency", edr.Value.ToUpper());
                                    if (this.SetCurrencyEnabled)
                                        this.Core.Bo.ExtraInfo.Currency = edr.Value.ToUpper();
                                    else
                                        Log.Info("ExtraData SetCurrency: disable!");
                                }
                                if (edr.Key.Equals("amountLimit"))
                                {
                                    if (!string.IsNullOrEmpty(edr.Value))
                                    {
                                        if (decimal.TryParse(edr.Value, out decimal amountLimit))
                                        {
                                            this.Core.Bo.ExtraInfo.AmountLimit = amountLimit;
                                            GlobalAppData.Instance.SetScratchpad("amountLimit", amountLimit);
                                        }
                                        else
                                        {
                                            Log.Error("Amount limit isn´t numeric");
                                            this.Core.Bo.ExtraInfo.AmountLimit = 0;
                                        }
                                    }
                                    else
                                        Log.Info("Amount limit is empty");
                                }
                                if (edr.Key.Equals("transactionInfo"))
                                {
                                    if (this.SetTxInfoEnabled)
                                        this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData(Enums.ExtraDataType.txInfo, this.prop.Extension1.TransactionInfoTagName, edr.Value));
                                }
                                if (edr.Key.Equals("transactionRef"))
                                {
                                    if (this.SetTxRefEnabled)
                                        this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData(Enums.ExtraDataType.txRef, this.prop.Extension1.TransactionRefTagName, edr.Value));
                                }
                                if (edr.Key.Equals("shiftSelection"))
                                {
                                    if (this.SetShiftsEnabled)
                                        this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData(Enums.ExtraDataType.shifts, this.prop.Extension1.ShiftsTagName, edr.Value));
                                }
                                if (edr.Key.Contains("channel"))
                                {
                                    if (string.IsNullOrEmpty(channel))
                                        channel = $"{edr.Value}";
                                    else
                                        channel = $"{channel}-{edr.Value}";
                                }
                                else if (edr.Key.Contains("dynamic"))
                                {
                                    var dataObj = Utilities.Utils.NewtonsoftDeserialize<FieldDetail>(out bool ret, edr.Value); //properties in dataObj: ID,Value,SelectedOptionsID
                                    if (ret)
                                        this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData(Enums.ExtraDataType.dynamic, dataObj.ID.ToString(), edr.Value)); //ADD dynamics to be received in next state
                                    else
                                        Log.Error("Failed to receive proper object of FieldDetail from Javsacript.");
                                }
                            }

                            if (!string.IsNullOrEmpty(channel))
                                this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData(Enums.ExtraDataType.channel, this.prop.Extension1.ChannelTagName, channel));
                            //Load OK
                            this.Core.AddHostExtraData("extraData", this.Core.Bo.ExtraInfo.ExtraData);

                            //depositarios

                            if(this.alephATMAppData.Branding == Enums.Branding.FIC)
                            {
                                FieldOption[] docTypeOptions = new FieldOption[]
                                {
                                    new FieldOption { Value = "CI", DisplayName = "CI" },
                                    new FieldOption { Value = "RUC", DisplayName = "RUC" }
                                };

                                foreach (ExtraData edr in this.Core.Bo.ExtraInfo.ExtraData) {

                                    var dataObj = Utilities.Utils.NewtonsoftDeserialize<FieldDetail>(out bool ret, edr.TagValue);
                                    switch (dataObj.Name)
                                    {
                                        case "holderDocType":
                                            this.Core.AddHostExtraData("holderDocType", docTypeOptions[dataObj.SelectedOptionsID[0] - 1].Value);
                                            break;
                                        case "holderDocNo":
                                            this.Core.AddHostExtraData("holderDocNo", dataObj.Value);
                                            break;
                                        case "depositorDocType":
                                            this.Core.AddHostExtraData("depositorDocType", docTypeOptions[dataObj.SelectedOptionsID[0] - 1].Value);
                                            break;
                                        case "depositorDocNo":
                                            this.Core.AddHostExtraData("depositorDocNo", dataObj.Value);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                this.Core.Bo.ExtraInfo.SetExtraDataFields = null;
                            }

                            this.SetActivityResult(StateResult.SUCCESS, this.prop.SetGoodNextStateNumber);
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandleOthersKeysReturn(string othersKeys)
        {
            Log.Debug("/--->");
            switch (othersKeys)
            {
                case "CANCEL":
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case "ENTER":
                    {
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.SetGoodNextStateNumber);
                        break;
                    }
                case "MORETIME":
                    this.ResetTimer();
                    break;
            }
        }

        /// <summary>
        /// Aca se recibe el pedido de datos para armar la pantalla de detalle de depósitos
        /// </summary>
        /// <param name="FDKdata"></param>
        /// 
        private void HandlerFDKreturn(string FDKdata)
        {
            try
            {
                Log.Debug("/--->");
                switch (FDKdata)
                {
                    case "B":
                        if (!(GetConfigurationData().ExtraDataList?.Any(x => x.enabled) ?? false))
                        {
                            Log.Info("Notification acknowlaged by user. Must continue to next state.");
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.SetGoodNextStateNumber);
                        }
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private ConfigurationData GetConfigurationData()
        {
            string jsonData = string.Empty;
            ConfigurationData confData = new ConfigurationData();
            List<ExtraDataConf> conf = new List<ExtraDataConf>();
            confData.ExtraDataList = new List<ExtraDataConf>();
            confData.ScreenTitle = this.prop.ScreenTitle;
            try
            {
                //Currency
                if (this.SetCurrencyEnabled)
                    confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.currency));
                else
                    Log.Info("ExtraData SetCurrency: disable!");
                //Channel
                if (this.SetChannelEnabled)
                {
                    if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.ChannelA))
                    {
                        var channels = this.Core.Bo.ExtraInfo.ChannelA.Split(',');
                        if (channels.Length > 2)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                var channelName = $"channel{i + 1}";
                                var extraDataConfig = this.prop.ExtraDataConfigurations.Find(x => x.extraDataType == Enums.ExtraDataType.channel && x.name.Equals(channelName));
                                if (extraDataConfig != null)
                                {
                                    extraDataConfig.value = channels[i];
                                    // set to not editable when preloaded
                                    if (extraDataConfig.editableOnPreloadedValue) extraDataConfig.editable = false;
                                    confData.ExtraDataList.Add(extraDataConfig);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If ChannelA is null or empty, set editable back to true
                        var channelConfigs = this.prop.ExtraDataConfigurations.Where(x => x.extraDataType == Enums.ExtraDataType.channel);
                        foreach (var config in channelConfigs)
                        {
                            config.editable = true;
                            config.value = string.Empty;
                        }
                        confData.ExtraDataList.AddRange(channelConfigs);
                    }

                }
                else
                    Log.Info("ExtraData SetChannel: disable!");
                //Tx info
                if (this.SetTxInfoEnabled)
                    confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.txInfo));
                else
                    Log.Info("ExtraData SetTxInfo: disable!");
                //Tx ref
                if (this.SetTxRefEnabled)
                    confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.txRef));
                else
                    Log.Info("ExtraData SetTxRef: disable!");
                //Shifts
                if (this.SetShiftsEnabled)
                    confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.shifts));
                else
                    Log.Info("ExtraData SetShifts: disable!");
                //Amount limit
                if (this.SetAmountLimitEnabled)
                    confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.amountLimit));
                else
                    Log.Info("ExtraData SetAmountLimit: disable!");

                confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.dynamic));
                confData.ExtraDataList.AddRange(this.prop.ExtraDataConfigurations.FindAll(x => x.extraDataType == Enums.ExtraDataType.displayOnly));

                //extra info

                if (this.Core.Bo.ExtraInfo.SetExtraDataInfo != null && !string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.SetExtraDataInfo.ServiceName))
                {
                    // Injection of extra data for "RECARGA DE SALDO" service
                    if (this.Core.Bo.ExtraInfo.SetExtraDataInfo.ServiceName.ToUpper().Contains("RECARGA DE SALDO"))
                    {
                        this.prop.ExtraDataConfigurations.Find(x =>  x.name.ToUpper() == "CELULAR").value = "09" + this.Core.Bo.ExtraInfo.HostExtraData.First(x => x.Key == "PhoneNumberInfo").Value;
                    }
                    //
                    confData.ExtraInfo = new JObject
                    {
                        ["serviceName"] = (JToken)this.Core.Bo.ExtraInfo.SetExtraDataInfo.ServiceName,
                        ["serviceId"] = (JToken)this.Core.Bo.ExtraInfo.SetExtraDataInfo.ServiceId,
                        ["currentStep"] = (JToken)this.Core.Bo.ExtraInfo.SetExtraDataInfo.CurrentStep,
                        ["stepsLength"] = (JToken)this.Core.Bo.ExtraInfo.SetExtraDataInfo.StepLength
                    }.ToString();
                }

            }
            catch (Exception ex) { Log.Fatal(ex); }
            return confData;
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                if(result != StateResult.SUCCESS)
                {
                    Log.Trace("Removing cart data from hostExtraData.");
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("billId");
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("currentStep");
                }
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.StopTimer();
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.Quit();
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                if (this.prop.ClearDataOnExit)
                {
                    this.Core.Bo.ExtraInfo.ChannelA = string.Empty;
                }
                MainEventsSubscription(false);
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
                        //this.ActivityStart();
                        this.Core.HideScreenModals();
                        StartTimer();
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
            if (!enabled)
            {
                this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);
            }
            else
            {
                if (!this.MoreTimeSubscribed)
                {
                    this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);
                }
                MainEventsSubscription(true);
            }
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

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timeout = true;
            this.StopTimer();
            MainEventsSubscription(false);
            this.moreTime.StartMoreTime();
        }



        bool mainEventsSubscribed = false;
        private void MainEventsSubscription(bool suscribe)
        {
            if (suscribe && !mainEventsSubscribed)
            {
                //this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                mainEventsSubscribed = true;
            }
            else if (!suscribe)
            {
                //this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                mainEventsSubscribed = false;
            }
        }

        #endregion "More time"

    }
}
