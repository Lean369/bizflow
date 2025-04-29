//using System.Xml.Serialization;
using Entities;

namespace Business.EnvelopeDispenserState
{
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesEnvelopeDispenserState/1.0.0.0/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.AlephATM.com/states/PropertiesEnvelopeDispenserState/1.0.0.0/", IsNullable = false)]
    public partial class PropertiesEnvelopeDispenserState
    {
        private string screenNumberField;
        private string nextStateNumberField;
        private string cancelNextStateNumberField;
        private string errorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private EnvelopeOperationMode_Type envelopeOperationModeField;
        private EmptyElement_Type itemField;
        private EmptyElement_Type item1Field;
        private string HostNameField;
        private PropertiesKeyboard keyboardField;
        private PropertiesJournal journalField;
        private PropertiesMoreTime moreTimeField;
        public StateEvent OnShowEnvelopeScreen;
        internal StateEvent OnPleaseWait;
        internal StateEvent OnTakeEnvelopeAdvice;
        internal StateEvent OnInsertEnvelopeAdvice;
        internal StateEvent OnClearModals;

        public PropertiesEnvelopeDispenserState()
        {
            this.screenNumberField = "";
            this.nextStateNumberField = "";
            this.errorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.EnvelopeOperationMode = EnvelopeOperationMode_Type.none;
            this.itemField = null;
            this.item1Field = null;
            this.HostNameField = "BvaHost";
            this.keyboardField = new PropertiesKeyboard();
            this.journalField = new PropertiesJournal();
            this.moreTimeField = new PropertiesMoreTime();
            this.OnShowEnvelopeScreen = new StateEvent(StateEvent.EventType.navigate, "envelopeDispenser.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnTakeEnvelopeAdvice = new StateEvent(StateEvent.EventType.runScript, "takeEnvelopeAdvice", "");
            this.OnInsertEnvelopeAdvice = new StateEvent(StateEvent.EventType.runScript, "insertEnvelopeAdvice", "");
            this.OnClearModals = new StateEvent(StateEvent.EventType.runScript, "clearModals", "");
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

        /// <remarks/>
        public string NextStateNumber
        {
            get
            {
                return this.nextStateNumberField;
            }
            set
            {
                this.nextStateNumberField = value;
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
        public string ErrorNextStateNumber
        {
            get
            {
                return this.errorNextStateNumberField;
            }
            set
            {
                this.errorNextStateNumberField = value;
            }
        }

        /// <remarks/>
        public string TimeoutNextStateNumber
        {
            get
            {
                return this.timeoutNextStateNumberField;
            }
            set
            {
                this.timeoutNextStateNumberField = value;
            }
        }

        /// <remarks/>
        public EnvelopeOperationMode_Type EnvelopeOperationMode
        {
            get
            {
                return this.envelopeOperationModeField;
            }
            set
            {
                this.envelopeOperationModeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EnvelopeDispenserStateTableReservedEntry3")]
        public EmptyElement_Type Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EnvelopeDispenserStateTableReservedEntry4")]
        public EmptyElement_Type Item1
        {
            get
            {
                return this.item1Field;
            }
            set
            {
                this.item1Field = value;
            }
        }

        public string HostName
        {
            get
            {
                return this.HostNameField;
            }
            set
            {
                this.HostNameField = value;
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesEnvelopeDispenserState/1.0.0.0/")]
    public partial class Extension
    {
        private string stateNumberField;

        private EmptyElement_Type itemField;

        private EmptyElement_Type item1Field;

        private EmptyElement_Type item2Field;

        private EmptyElement_Type item3Field;

        private EmptyElement_Type item4Field;

        private EmptyElement_Type item5Field;

        private EmptyElement_Type item6Field;

        private object item7Field;

        /// <remarks/>
        public string StateNumber
        {
            get
            {
                return this.stateNumberField;
            }
            set
            {
                this.stateNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry2")]
        public EmptyElement_Type Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry3")]
        public EmptyElement_Type Item1
        {
            get
            {
                return this.item1Field;
            }
            set
            {
                this.item1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry4")]
        public EmptyElement_Type Item2
        {
            get
            {
                return this.item2Field;
            }
            set
            {
                this.item2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry5")]
        public EmptyElement_Type Item3
        {
            get
            {
                return this.item3Field;
            }
            set
            {
                this.item3Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry6")]
        public EmptyElement_Type Item4
        {
            get
            {
                return this.item4Field;
            }
            set
            {
                this.item4Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry7")]
        public EmptyElement_Type Item5
        {
            get
            {
                return this.item5Field;
            }
            set
            {
                this.item5Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry8")]
        public EmptyElement_Type Item6
        {
            get
            {
                return this.item6Field;
            }
            set
            {
                this.item6Field = value;
            }
        }
    }

     /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesEnvelopeDispenserState/1.0.0.0/")]
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesEnvelopeDispenserState/1.0.0.0/")]
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesEnvelopeDispenserState/1.0.0.0/")]
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