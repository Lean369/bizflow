//using System.Xml.Serialization;
using Entities;

namespace Business.FourFDKSelectionFunctionState
{

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesFourFDKSelectionFunctionState/1.0.0.0/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.AlephATM.com/states/PropertiesFourFDKSelectionFunctionState/1.0.0.0/", IsNullable = false)]
    public partial class PropertiesFourFDKSelectionFunctionState
    {
        private string screenNumberField;
        private string timeOutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string fDKAorINextStateNumberField;
        private string fDKBorHNextStateNumberField;
        private string fDKCorGNextStateNumberField;
        private string fDKDorFNextStateNumberField;
        private OperationCodeBufferEntryNumberChar3_Type operationCodeBufferEntryNumberField;
        private PropertiesJournal journalField;
        private PropertiesMoreTime moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnPleaseWait;

        public PropertiesFourFDKSelectionFunctionState()
        {
            this.screenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.fDKAorINextStateNumberField = "";
            this.fDKAorINextStateNumberField = "";
            this.fDKAorINextStateNumberField = "";
            this.fDKAorINextStateNumberField = "";
            this.operationCodeBufferEntryNumberField = OperationCodeBufferEntryNumberChar3_Type.Item0;
            this.journalField = new PropertiesJournal();
            this.moreTimeField = new PropertiesMoreTime();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "menu.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
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

        /// <remarks/>
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

        /// <remarks/>
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

        /// <remarks/>
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

        /// <remarks/>
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

        /// <remarks/>
        public OperationCodeBufferEntryNumberChar3_Type OperationCodeBufferEntryNumber
        {
            get
            {
                return this.operationCodeBufferEntryNumberField;
            }
            set
            {
                this.operationCodeBufferEntryNumberField = value;
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
    public partial class Extension
    {

        private string codeFDKAField;
        private string codeFDKBField;
        private string codeFDKCField;
        private string codeFDKDField;
        private string codeFDKFField;
        private string codeFDKGField;
        private string codeFDKHField;
        private string codeFDKIField;

        /// <comentarios/>
        public string CodeFDKA
        {
            get
            {
                return this.codeFDKAField;
            }
            set
            {
                this.codeFDKAField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKB
        {
            get
            {
                return this.codeFDKBField;
            }
            set
            {
                this.codeFDKBField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKC
        {
            get
            {
                return this.codeFDKCField;
            }
            set
            {
                this.codeFDKCField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKD
        {
            get
            {
                return this.codeFDKDField;
            }
            set
            {
                this.codeFDKDField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKF
        {
            get
            {
                return this.codeFDKFField;
            }
            set
            {
                this.codeFDKFField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKG
        {
            get
            {
                return this.codeFDKGField;
            }
            set
            {
                this.codeFDKGField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKH
        {
            get
            {
                return this.codeFDKHField;
            }
            set
            {
                this.codeFDKHField = value;
            }
        }

        /// <comentarios/>
        public string CodeFDKI
        {
            get
            {
                return this.codeFDKIField;
            }
            set
            {
                this.codeFDKIField = value;
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