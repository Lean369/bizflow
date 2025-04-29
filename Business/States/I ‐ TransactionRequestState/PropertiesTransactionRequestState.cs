//using System.Xml.Serialization;
using Entities;
using System.Xml.Serialization;

namespace Business.TransactionRequestState
{
    public partial class PropertiesTransactionRequestState
    {
        private string nextStateField;
        private string centralResponseTimeoutNextStateNumberField;
        private int sendTrack2DataField;
        private Enums.TransactionType transactionTypeField;
        private int sendOperationCodeDataField;
        private int sendAmountDataField;
        private int operationModeField;
        private SendPINBufferADataSelectExtendedFormat_Type sendPINBufferADataSelectExtendedFormatField;
        private object extensionOrBufFersBCField;
        private bool topOfReceiptTransactionFlagField;
        private bool sendTransactionStatusField;
        private bool twelveDigitsAmountBufferLengthField;
        private bool sendGeneralPurposeBufferBField;
        private bool sendGeneralPurposeBufferCField;
        private bool extensionSendGeneralPurposeBufferBField;
        private bool extensionSendGeneralPurposeBufferCField;
        private PropertiesJournal journalField;
        private PropertiesMoreTime moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnPleaseWait;
        private string HostNameField;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinter1;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinter2;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinter;
        [XmlElement()]
        public StateEvent OnSendTicketToBD;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinterError;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinterError;
        [XmlElement()]
        public StateEvent OnSendTicketToBDError;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinterDeclined;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinterDeclined;

        public PropertiesTransactionRequestState()
        {
            
        }

        public PropertiesTransactionRequestState(AlephATMAppData alephATMAppData)
        {
            this.nextStateField = "";
            this.centralResponseTimeoutNextStateNumberField = "";
            this.sendTrack2DataField = -1;
            this.transactionTypeField = Enums.TransactionType.NONE;
            this.sendOperationCodeDataField = -1;
            this.sendAmountDataField = -1;
            this.sendPINBufferADataSelectExtendedFormatField = new SendPINBufferADataSelectExtendedFormat_Type();
            this.sendPINBufferADataSelectExtendedFormatField = null;
            this.extensionOrBufFersBCField = new object();
            //Propiedades internas
            this.HostNameField = "GenericHost";
            this.extensionOrBufFersBCField = null;
            this.topOfReceiptTransactionFlagField = false;
            this.sendTransactionStatusField = false;
            this.twelveDigitsAmountBufferLengthField = false;
            this.sendGeneralPurposeBufferBField = false;
            this.sendGeneralPurposeBufferCField = false;
            this.extensionSendGeneralPurposeBufferBField = false;
            this.extensionSendGeneralPurposeBufferCField = false;
            this.journalField = new PropertiesJournal();
            this.moreTimeField = new PropertiesMoreTime();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "menu.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCash", "");
            this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinterError = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashError", "");
            this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashError", "");
            this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.ignore, "0", "");
            this.OnPrintTicketOnReceiptPrinterDeclined = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashDeclined", "");
            this.OnPrintTicketOnJournalPrinterDeclined = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashDeclined", "");
            switch (alephATMAppData.Branding)
            {
                case Enums.Branding.RedPagosA:
                    this.HostNameField = "RedpagosMulesoftHost";
                    break;
                case Enums.Branding.Atlas:
                case Enums.Branding.FIC:
                    this.HostNameField = "DepositariosMulesoftHost";
                    this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.ignore, "Pr1AccountDepositCash", "");
                    this.OnPrintTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1AccountDepositCash", "");
                    break;
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

        public string NextState
        {
            get
            {
                return this.nextStateField;
            }
            set
            {
                this.nextStateField = value;
            }
        }

        public string CentralResponseTimeoutNextStateNumber
        {
            get
            {
                return this.centralResponseTimeoutNextStateNumberField;
            }
            set
            {
                this.centralResponseTimeoutNextStateNumberField = value;
            }
        }

        public int SendTrack2Data
        {
            get
            {
                return sendTrack2DataField;
            }
            set
            {
                this.sendTrack2DataField = value;
            }
        }

        public Enums.TransactionType TransactionType
        {
            get
            {
                return this.transactionTypeField;
            }
            set
            {
                this.transactionTypeField = value;
            }
        }

        public int SendOperationCodeData
        {
            get
            {
                return sendOperationCodeDataField;
            }
            set
            {
                this.sendOperationCodeDataField = value;
            }
        }

        public int SendAmountData
        {
            get
            {
                return sendAmountDataField;
            }
            set
            {
                this.sendAmountDataField = value;
            }
        }
        public int OperationMode
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

        public SendPINBufferADataSelectExtendedFormat_Type SendPINBufferADataSelectExtendedFormat
        {
            get
            {
                return sendPINBufferADataSelectExtendedFormatField;
            }
            set
            {
                this.sendPINBufferADataSelectExtendedFormatField = value;
            }
        }

        /// <comentarios/>
        public object ExtensionOrBufFersBC
        {
            get
            {
                return this.extensionOrBufFersBCField;
            }
            set
            {
                this.extensionOrBufFersBCField = value;
            }
        }

        public bool TopOfReceiptTransactionFlag
        {
            get
            {
                return this.topOfReceiptTransactionFlagField;
            }
            set
            {
                this.topOfReceiptTransactionFlagField = value;
            }
        }

        public bool SendTransactionStatus
        {
            get
            {
                return this.sendTransactionStatusField;
            }
            set
            {
                this.sendTransactionStatusField = value;
            }
        }

        public bool TwelveDigitsAmountBufferLength
        {
            get
            {
                return this.twelveDigitsAmountBufferLengthField;
            }
            set
            {
                this.twelveDigitsAmountBufferLengthField = value;
            }
        }

        public bool SendGeneralPurposeBufferB
        {
            get
            {
                return this.sendGeneralPurposeBufferBField;
            }
            set
            {
                this.sendGeneralPurposeBufferBField = value;
            }
        }

        public bool SendGeneralPurposeBufferC
        {
            get
            {
                return this.sendGeneralPurposeBufferCField;
            }
            set
            {
                this.sendGeneralPurposeBufferCField = value;
            }
        }

        public bool ExtensionSendGeneralPurposeBufferB
        {
            get
            {
                return this.extensionSendGeneralPurposeBufferBField;
            }
            set
            {
                this.extensionSendGeneralPurposeBufferBField = value;
            }
        }

        public bool ExtensionSendGeneralPurposeBufferC
        {
            get
            {
                return this.extensionSendGeneralPurposeBufferCField;
            }
            set
            {
                this.extensionSendGeneralPurposeBufferCField = value;
            }
        }

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

        private string fileNameField;

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

        public string FileName
        {
            get
            {
                return this.fileNameField;
            }
            set
            {
                this.fileNameField = value;
            }
        }

        public PropertiesJournal()
        {
            this.enableJournalField = true;
            this.fileNameField = "BuildTransactionRequest.txt";
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
        private string stateNumberField;

        private SendGeneralPurposeBuffersBAndOrC_Type sendGeneralPurposeBuffersBAndOrCField;

        private SendOptionalDataFieldsAH_Type sendOptionalDataFieldsAHField;

        private SendOptionalDataFieldsIL_Type sendOptionalDataFieldsILField;

        private SendOptionalDataFieldsQV_Type sendOptionalDataFieldsQVField;

        private SendOptionalData_Type sendOptionalDataField;

        private EmptyElement_Type itemField;

        private EMVCAMProcessingFlag_Type eMVCAMProcessingFlagField;

        public Extension()
        {
            this.stateNumberField = "";
            this.sendGeneralPurposeBuffersBAndOrCField = new SendGeneralPurposeBuffersBAndOrC_Type();
            this.sendGeneralPurposeBuffersBAndOrCField = null;
            this.sendOptionalDataFieldsAHField = new SendOptionalDataFieldsAH_Type();
            this.sendOptionalDataFieldsAHField = null;
            this.sendOptionalDataFieldsILField = new SendOptionalDataFieldsIL_Type();
            this.sendOptionalDataFieldsILField = null;
            this.sendOptionalDataFieldsQVField = new SendOptionalDataFieldsQV_Type();
            this.sendOptionalDataFieldsQVField = null;
            this.sendOptionalDataFieldsQVField = new SendOptionalDataFieldsQV_Type();
            this.sendOptionalDataFieldsQVField = null;
            this.sendOptionalDataField = new SendOptionalData_Type();
            this.sendOptionalDataField = null;
            this.itemField = new EmptyElement_Type();
            this.eMVCAMProcessingFlagField = new EMVCAMProcessingFlag_Type();
            this.eMVCAMProcessingFlagField = null;
        }

        /// <comentarios/>
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

        /// <comentarios/>
        public SendGeneralPurposeBuffersBAndOrC_Type SendGeneralPurposeBuffersBAndOrC
        {
            get
            {
                return this.sendGeneralPurposeBuffersBAndOrCField;
            }
            set
            {
                this.sendGeneralPurposeBuffersBAndOrCField = value;
            }
        }

        /// <comentarios/>
        public SendOptionalDataFieldsAH_Type SendOptionalDataFieldsAH
        {
            get
            {
                return this.sendOptionalDataFieldsAHField;
            }
            set
            {
                this.sendOptionalDataFieldsAHField = value;
            }
        }

        /// <comentarios/>
        public SendOptionalDataFieldsIL_Type SendOptionalDataFieldsIL
        {
            get
            {
                return this.sendOptionalDataFieldsILField;
            }
            set
            {
                this.sendOptionalDataFieldsILField = value;
            }
        }

        /// <comentarios/>
        public SendOptionalDataFieldsQV_Type SendOptionalDataFieldsQV
        {
            get
            {
                return this.sendOptionalDataFieldsQVField;
            }
            set
            {
                this.sendOptionalDataFieldsQVField = value;
            }
        }

        public SendOptionalData_Type SendOptionalData
        {
            get
            {
                return this.sendOptionalDataField;
            }
            set
            {
                this.sendOptionalDataField = value;
            }
        }

        public EMVCAMProcessingFlag_Type EMVCAMProcessingFlag
        {
            get
            {
                return this.eMVCAMProcessingFlagField;
            }
            set
            {
                this.eMVCAMProcessingFlagField = value;
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