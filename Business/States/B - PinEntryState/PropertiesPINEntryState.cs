//using System.Xml.Serialization;
using Entities;

namespace Business.PINEntryState
{

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesPINEntryState/1.0.0.0/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.AlephATM.com/states/PropertiesPINEntryState/1.0.0.0/", IsNullable = false)]
    public partial class PropertiesPINEntryState
    {
        private string screenNumberField;

        private string timeOutNextStateNumberField;

        private string cancelNextStateNumberField;

        private string localPINCheckGoodPINNextStateNumberField;

        private string localPINCheckMaximumBadPINNextStateNumberField;

        private string localPINCheckErrorScreenNumberField;

        private string remotePINCheckNextStateNumberField;

        private int localPINCheckMaximumPINRetriesField;

        private KeyMask_Type activeFDKsField;

        private PropertiesKeyboard keyboardField;

        private PropertiesJournal journalField;

        private PropertiesMoreTime moreTimeField;

        public PropertiesPINEntryState()
        {
            this.screenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.localPINCheckGoodPINNextStateNumberField = "";
            this.localPINCheckMaximumBadPINNextStateNumberField = "";
            this.localPINCheckErrorScreenNumberField = "";
            this.remotePINCheckNextStateNumberField = "";
            this.localPINCheckMaximumPINRetriesField = 0;
            this.activeFDKsField = new KeyMask_Type();
            this.activeFDKsField.FDKA = true;
            this.keyboardField = new PropertiesKeyboard();
            this.journalField = new PropertiesJournal();
            this.moreTimeField = new PropertiesMoreTime();
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
        public string LocalPINCheckGoodPINNextStateNumber
        {
            get
            {
                return this.localPINCheckGoodPINNextStateNumberField;
            }
            set
            {
                this.localPINCheckGoodPINNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string LocalPINCheckMaximumBadPINNextStateNumber
        {
            get
            {
                return this.localPINCheckMaximumBadPINNextStateNumberField;
            }
            set
            {
                this.localPINCheckMaximumBadPINNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public string LocalPINCheckErrorScreenNumber
        {
            get
            {
                return this.localPINCheckErrorScreenNumberField;
            }
            set
            {
                this.localPINCheckErrorScreenNumberField = value;
            }
        }

        /// <comentarios/>
        public string RemotePINCheckNextStateNumber
        {
            get
            {
                return this.remotePINCheckNextStateNumberField;
            }
            set
            {
                this.remotePINCheckNextStateNumberField = value;
            }
        }

        /// <comentarios/>
        public int LocalPINCheckMaximumPINRetries
        {
            get
            {
                return this.localPINCheckMaximumPINRetriesField;
            }
            set
            {
                this.localPINCheckMaximumPINRetriesField = value;
            }
        }

        /// <comentarios/>
        public KeyMask_Type ActiveFDKs
        {
            get
            {
                return this.activeFDKsField;
            }
            set
            {
                this.activeFDKsField = value;
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
}