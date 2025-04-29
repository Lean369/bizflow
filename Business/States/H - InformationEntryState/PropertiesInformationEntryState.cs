//using System.Xml.Serialization;
using Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Business.InformationEntryState
{
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesInformationEntryState/1.0.0.0/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.AlephATM.com/states/PropertiesInformationEntryState/1.0.0.0/", IsNullable = false)]
    public partial class PropertiesInformationEntryState
    {
        private string screenNumberField;
        private string setGoodNextStateNumberField;

        private string timeOutNextStateNumberField;

        private string cancelNextStateNumberField;

        private string fDKAorINextStateNumberField;

        private string fDKBorHNextStateNumberField;

        private string fDKCorGNextStateNumberField;

        private string fDKDorFNextStateNumberField;

        private string destinationKeyField;

        private string secDestinationKeyField;

        private string operationModeField;

        public StateEvent OnShowScreen;
        public StateEvent OnSendScreenData;
        public InfoEntryDetails infoEntryDetails;
        private EntryModeAndBufferConfiguration_Type entryModeAndBufferConfigurationField;

        private PropertiesKeyboard keyboardField;

        private PropertiesJournal journalField;

        private PropertiesMoreTime moreTimeField;

        public PropertiesInformationEntryState()
        {
            this.screenNumberField = "";
            this.setGoodNextStateNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.fDKAorINextStateNumberField = "";
            this.fDKBorHNextStateNumberField = "";
            this.fDKCorGNextStateNumberField = "";
            this.fDKDorFNextStateNumberField = "";
            this.operationModeField = "";
            this.entryModeAndBufferConfigurationField = null;
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "informationEntry.htm", "");
            this.OnSendScreenData = new StateEvent(StateEvent.EventType.runScript, "screenData", "");
            this.keyboardField = new PropertiesKeyboard();
            this.journalField = new PropertiesJournal();
            this.moreTimeField = new PropertiesMoreTime();
        }

        internal void LoadDefaultConfiguration(AlephATMAppData alephATMAppData)
        {
            switch (alephATMAppData.Branding)
            {
                case Enums.Branding.RedPagosA:
                    this.destinationKeyField = "PhoneNumberInfo";
                    this.infoEntryDetails = new InfoEntryDetails(true, "language.infoEntry.tag.phoneNumber", "numeric", 7, 7, "09","customerPhoneNumber");
                    break;
                case Enums.Branding.Atlas:
                case Enums.Branding.GNB:
                case Enums.Branding.FIC:
                    this.destinationKeyField = "holderAccountNo";
                    this.infoEntryDetails = new InfoEntryDetails(true, "language.infoEntry.tag.holderAccountNo", "numeric", 16, 16, "", "depositariosAccountNo");
                    break;
                default:
                    this.infoEntryDetails = new InfoEntryDetails();
                    break;
            }
        }

        /// <comentarios/>
        public string ScreenNumber
        {
            get
            {
                return this.screenNumberField;
            }
            set
            {
                this.screenNumberField = value;
            }
        }

        /// <comentarios/>
        public string SetGoodNextStateNumber
        {
            get
            {
                return this.setGoodNextStateNumberField;
            }
            set
            {
                this.setGoodNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string TimeOutNextStateNumber
        {
            get
            {
                return this.timeOutNextStateNumberField;
            }
            set
            {
                this.timeOutNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string CancelNextStateNumber
        {
            get
            {
                return this.cancelNextStateNumberField;
            }
            set
            {
                this.cancelNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string FDKAorINextStateNumber
        {
            get
            {
                return this.fDKAorINextStateNumberField;
            }
            set
            {
                this.fDKAorINextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string FDKBorHNextStateNumber
        {
            get
            {
                return this.fDKBorHNextStateNumberField;
            }
            set
            {
                this.fDKBorHNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string FDKCorGNextStateNumber
        {
            get
            {
                return this.fDKCorGNextStateNumberField;
            }
            set
            {
                this.fDKCorGNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string FDKDorFNextStateNumber
        {
            get
            {
                return this.fDKDorFNextStateNumberField;
            }
            set
            {
                this.fDKDorFNextStateNumberField = value;
            }
        }

        public string OperationMode
        {
            get
            {
                return this.operationModeField;
            }
            set
            {
                this.operationModeField = value;
            }
        }

        public string DestinationKey { get => destinationKeyField; set => destinationKeyField = value; }

        public string SecDestinationKey { get => secDestinationKeyField; set => secDestinationKeyField = value; }

        /// <comentarios/>
        public EntryModeAndBufferConfiguration_Type EntryModeAndBufferConfiguration
        {
            get
            {
                return this.entryModeAndBufferConfigurationField;
            }
            set
            {
                this.entryModeAndBufferConfigurationField = value;
            }
        }

        /// <comentarios/>
        public PropertiesKeyboard keyboard
        {
            get
            {
                return this.keyboardField;
            }
            set
            {
                this.keyboardField = value;
            }
        }

        /// <comentarios/>
        public PropertiesJournal Journal
        {
            get
            {
                return this.journalField;
            }
            set
            {
                this.journalField = value;
            }
        }

        /// <comentarios/>
        public PropertiesMoreTime MoreTime
        {
            get
            {
                return this.moreTimeField;
            }
            set
            {
                this.moreTimeField = value;
            }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/GetCustomerInput/1.0.0.0/")]
    public partial class PropertiesJournal
    {

        private bool enableJournalField;

        private string inputCancelFileNameField;

        private string inputTimeoutFileNameField;

        public PropertiesJournal()
        {
            this.enableJournalField = true;
            this.inputCancelFileNameField = "InputCancel.txt";
            this.inputTimeoutFileNameField = "InputTimeout.txt";
        }

        /// <comentarios/>
        public bool EnableJournal
        {
            get
            {
                return this.enableJournalField;
            }
            set
            {
                this.enableJournalField = value;
            }
        }

        /// <comentarios/>
        public string InputCancelFileName
        {
            get
            {
                return this.inputCancelFileNameField;
            }
            set
            {
                this.inputCancelFileNameField = value;
            }
        }

        /// <comentarios/>
        public string InputTimeoutFileName
        {
            get
            {
                return this.inputTimeoutFileNameField;
            }
            set
            {
                this.inputTimeoutFileNameField = value;
            }
        }       
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/CardRead/1.0.0.0/")]
    public partial class PropertiesKeyboard
    {
        public bool continueButtonEnableField = true;

        public FDKCode_Type continueFDKField = FDKCode_Type.none;

        public bool cancelButtonEnableField;

        public FDKCode_Type cancelFDKField = FDKCode_Type.none;

        public bool clearButtonEnableField;

        public FDKCode_Type clearFDKField = FDKCode_Type.none;

        public PropertiesKeyboard()
        {

        }

        /// <comentarios/>
        public bool ContinueButton
        {
            get
            {
                return this.continueButtonEnableField;
            }
            set
            {
                this.continueButtonEnableField = value;
            }
        }

        /// <comentarios/>
        public FDKCode_Type ContinueFDK
        {
            get
            {
                return this.continueFDKField;
            }
            set
            {
                this.continueFDKField = value;
            }
        }

        /// <comentarios/>
        public bool CancelButton
        {
            get
            {
                return this.cancelButtonEnableField;
            }
            set
            {
                this.cancelButtonEnableField = value;
            }
        }

        /// <comentarios/>
        public FDKCode_Type CancelFDK
        {
            get
            {
                return this.cancelFDKField;
            }
            set
            {
                this.cancelFDKField = value;
            }
        }

        /// <comentarios/>
        public bool ClearButton
        {
            get
            {
                return this.clearButtonEnableField;
            }
            set
            {
                this.clearButtonEnableField = value;
            }
        }

        /// <comentarios/>
        public FDKCode_Type ClearFDK
        {
            get
            {
                return this.clearFDKField;
            }
            set
            {
                this.clearFDKField = value;
            }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/CardRead/1.0.0.0/")]
    public partial class PropertiesMoreTime
    {

        private string moreTimeScreenNameField;

        private int maxTimeOutField;

        private int maxTimeOutRetriesField;

        private bool moreTimeKeyboardEnabledField;

        public PropertiesMoreTime()
        {
            this.moreTimeScreenNameField = "C00.ndc";
            this.maxTimeOutField = 30;
            this.maxTimeOutRetriesField = 3;
            this.moreTimeKeyboardEnabledField = true;
        }

        /// <comentarios/>
        public string MoreTimeScreenName
        {
            get
            {
                return this.moreTimeScreenNameField;
            }
            set
            {
                this.moreTimeScreenNameField = value;
            }
        }

        /// <comentarios/>
        public int MaxTimeOut
        {
            get
            {
                return this.maxTimeOutField;
            }
            set
            {
                this.maxTimeOutField = value;
            }
        }

        /// <comentarios/>
        public int MaxTimeOutRetries
        {
            get
            {
                return this.maxTimeOutRetriesField;
            }
            set
            {
                this.maxTimeOutRetriesField = value;
            }
        }

        /// <comentarios/>
        public bool MoreTimeKeyboardEnabled
        {
            get
            {
                return this.moreTimeKeyboardEnabledField;
            }
            set
            {
                this.moreTimeKeyboardEnabledField = value;
            }
        }
    }

    public class InfoEntryDetails
    {
        private bool enableScreenKeyboardField;
        private string pageTitleTagField;
        private string keyboardModeField;
        private int maxLengthField;
        private int minLengthField;
        private string inputPrefixField;
        private string screenModeField;
        private SecondaryField secondaryFieldField;


        public InfoEntryDetails()
        {
            this.enableScreenKeyboardField = true;
            this.pageTitleTagField = "";
            this.keyboardModeField = "numeric";
            this.maxLengthField = 0;
            this.minLengthField = 0;
            this.inputPrefixField = "";
            this.screenModeField = "";
            this.secondaryFieldField = new SecondaryField();
        }

        public InfoEntryDetails(bool enableScreenKeyboard, string pageTitleTag, string keyboardMode, int maxLength, int minLength, string inputPrefix, string screenMode, SecondaryField secondaryField = null)
        {
            this.enableScreenKeyboardField = enableScreenKeyboard;
            this.pageTitleTagField = pageTitleTag;
            this.keyboardModeField = keyboardMode;
            this.maxLengthField = maxLength;
            this.minLengthField = minLength;
            this.inputPrefixField = inputPrefix;
            this.screenModeField = screenMode;
            this.secondaryFieldField = secondaryField;
        }

        public bool EnableScreenKeyboard
        {
            get
            {
                return this.enableScreenKeyboardField;
            }
            set
            {
                this.enableScreenKeyboardField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PageTitleTag
        {
            get
            {
                return this.pageTitleTagField;
            }
            set
            {
                this.pageTitleTagField = value;
            }
        }

        public string KeyboardMode
        {
            get
            {
                return this.keyboardModeField;
            }
            set
            {
                this.keyboardModeField = value;
            }
        }

        public int MaxLength
        {
            get
            {
                return this.maxLengthField;
            }
            set
            {
                this.maxLengthField = value;
            }
        }

        public int MinLength
        {
            get
            {
                return this.minLengthField;
            }
            set
            {
                this.minLengthField = value;
            }
        }

        public string InputPrefix
        {
            get
            {
                return this.inputPrefixField;
            }
            set
            {
                this.inputPrefixField = value;
            }
        }

        public string ScreenMode
        {
            get
            {
                return this.screenModeField;
            }
            set
            {
                this.screenModeField = value;
            }
        }

        public SecondaryField SecondaryField
        {
            get
            {
                return this.secondaryFieldField;
            }
            set
            {
                this.secondaryFieldField = value;
            }
        }

    }

    public class SecondaryField
    {
        [JsonIgnore]
        private List<SecondaryFieldOptions> optionsField = new List<SecondaryFieldOptions> { };
        [JsonIgnore]
        private string languageTagField = "";
        [JsonIgnore]
        private string literalTagField = "";

        public SecondaryField()
        {
            this.optionsField = new List<SecondaryFieldOptions> { };
            this.languageTagField = "";
            this.literalTagField = "";
        }

        public SecondaryField(List<SecondaryFieldOptions> options, string languageTag, string literalTag)
        {
            this.optionsField = options;
            this.languageTagField = languageTag;
            this.literalTagField = literalTag;
        }

        [JsonProperty("options")]
        public List<SecondaryFieldOptions> Options
        {
            get
            {
                return this.optionsField;
            }
            set
            {
                this.optionsField = value;
            }
        }
        [JsonProperty("languageTag")]
        public string LanguageTag
        {
            get
            {
                return this.languageTagField;
            }
            set
            {
                this.languageTagField = value;
            }
        }
        [JsonProperty("literalTag")]
        public string LiteralTag
        {
            get
            {
                return this.literalTagField;
            }
            set
            {
                this.literalTagField = value;
            }
        }
    }

    public class SecondaryFieldOptions
    {
        [JsonIgnore]
        private string languageTagField;
        [JsonIgnore]
        private string literalTagField;
        [JsonIgnore]
        private string valueField;
        [JsonIgnore]
        private bool selectedField;

        public SecondaryFieldOptions()
        {
            this.languageTagField = "";
            this.literalTagField = "";
            this.valueField = "";
            this.selectedField = false;
        }

        public SecondaryFieldOptions(string languageTag, string literalTag, string value, bool selected)
        {
            this.languageTagField = languageTag;
            this.literalTagField = literalTag;
            this.valueField = value;
            this.selectedField = selected;
        }
        [JsonProperty("languageTag")]
        public string LanguageTag
        {
            get
            {
                return this.languageTagField;
            }
            set
            {
                this.languageTagField = value;
            }
        }
        [JsonProperty("literalTag")]
        public string LiteralTag
        {
            get
            {
                return this.literalTagField;
            }
            set
            {
                this.literalTagField = value;
            }
        }
        [JsonProperty("value")]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
        [JsonProperty("selected")]
        public bool Selected
        {
            get
            {
                return this.selectedField;
            }
            set
            {
                this.selectedField = value;
            }
        }
    }
}