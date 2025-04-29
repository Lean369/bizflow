using Business.States;
using Entities;

namespace Business.AmountEntryState
{

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesAmountEntryState/1.0.0.0/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.AlephATM.com/states/PropertiesAmountEntryState/1.0.0.0/", IsNullable = false)]
    public partial class PropertiesAmountEntryState
    {
        private string screenNumberField;
        private string timeOutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string fDKAorINextStateNumberField;
        private string fDKBorHNextStateNumberField;
        private string fDKCorGNextStateNumberField;
        private string fDKDorFNextStateNumberField;
        private string amountDisplayScreenNumberField;
        private PropertiesKeyboard keyboardField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public AmountEntryDetails amountEntryDetails;
        public StateEvent OnShowAmountEntryScreen;
        internal StateEvent OnShowAmountDetails;

        public PropertiesAmountEntryState()
        {
            this.screenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.fDKAorINextStateNumberField = "";
            this.fDKBorHNextStateNumberField = "";
            this.fDKCorGNextStateNumberField = "";
            this.fDKDorFNextStateNumberField = "";
            this.amountDisplayScreenNumberField = "";
            this.keyboardField = new PropertiesKeyboard();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.amountEntryDetails = new AmountEntryDetails();
            this.OnShowAmountEntryScreen = new StateEvent(StateEvent.EventType.navigate, "amountEntry.htm", "");
            this.OnShowAmountDetails = new StateEvent(StateEvent.EventType.runScript, "ShowAmountDetails", "");
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

        /// <comentarios/>
        public string AmountDisplayScreenNumber
        {
            get
            {
                return this.amountDisplayScreenNumberField;
            }
            set
            {
                this.amountDisplayScreenNumberField = value;
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
        public JournalProperties Journal
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
        public MoreTimeProperties MoreTime
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

    public class AmountEntryDetails
    {
        private int previousAttempts;
        private string retryReason;
        private bool enableKeyboard;
        private bool allowDecimals;
        private int maxLength;
        private bool enableFastCash;
        private int fastCashButtonsCount;
        private bool virtualPinpad;

        public AmountEntryDetails()
        {
            this.previousAttempts = 0;
            this.retryReason = "";
            this.enableKeyboard = true;
            this.allowDecimals = true;
            this.maxLength = 4;
            this.enableFastCash = false;
            this.fastCashButtonsCount = 8;
            this.virtualPinpad = true;
        }

        public AmountEntryDetails(int _previousAttempts, string _retryReason, bool _enableKeyboard, bool _allowDecimals, int _maxLength, bool _enableFastCash, int _fastCashButtonsCount, bool _virtualPinpad)
        {
            this.previousAttempts = _previousAttempts;
            this.retryReason = _retryReason;
            this.enableKeyboard = _enableKeyboard;
            this.allowDecimals = _allowDecimals;
            this.maxLength = _maxLength;
            this.enableFastCash = _enableFastCash;
            this.fastCashButtonsCount = _fastCashButtonsCount;
            this.virtualPinpad = _virtualPinpad;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int PreviousAttempts
        {
            get { return this.previousAttempts; }
            set { this.previousAttempts = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string RetryReason
        {
            get { return this.retryReason; }
            set { this.retryReason = value; }
        }

        public bool EnableKeyboard
        {
            get { return this.enableKeyboard; }
            set { this.enableKeyboard = value; }
        }

        public bool AllowDecimals
        {
            get { return this.allowDecimals; }
            set { this.allowDecimals = value; }
        }

        public int MaxLength
        {
            get { return this.maxLength; }
            set { this.maxLength = value; }
        }

        public bool EnableFastCash
        {
            get { return this.enableFastCash; }
            set { this.enableFastCash = value; }
        }

        public int FastCashButtonsCount
        {
            get { return this.fastCashButtonsCount; }
            set { this.fastCashButtonsCount = value; }
        }

        public bool VirtualPinpad
        {
            get { return this.virtualPinpad; }
            set { this.virtualPinpad = value; }
        }
    }
}
