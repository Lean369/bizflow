using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.MultiCashAcceptState
{
    [Serializable]
    public partial class PropertiesMultiCashAcceptState
    {
        private string screenNumberField;
        private string enterNextStateNumberField;
        private string timeOutNextSateNumberField;
        private string cancelNextStateNumberField;
        private string moreDepositNextStateNumberField;
        private string hostNameField;
        private string maximumNumberOfDepositField;
        private bool autoDepositField = false;
        private bool depositAtErrorField = true;
        private bool depositWithoutEscrowFullField = true;
        private bool rejectDetectField = true;
        private bool sendDepositStatusField = true;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        private bool PrintPropertiesEnableField;
        [XmlElement()]
        public StateEvent OnMultiCashDeposit;
        [XmlElement()]
        public StateEvent OnPleaseWait;
        [XmlElement()]
        public StateEvent OnReturningCashAdvice;
        [XmlElement()]
        public StateEvent OnMultiCashDepositSummary;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinter1;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinter2;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinter;
        [XmlElement()]
        public StateEvent OnSendTicketToBD;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinterError1;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinterError2;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinterError;
        [XmlElement()]
        public StateEvent OnSendTicketToBDError;

        public PropertiesMultiCashAcceptState() { }

        public PropertiesMultiCashAcceptState(AlephATMAppData alephATMAppData)
        {
            this.screenNumberField = "";
            this.enterNextStateNumberField = "";
            this.timeOutNextSateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.moreDepositNextStateNumberField = "";
            this.hostNameField = "PrdmHost";
            this.maximumNumberOfDepositField = "30";
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.PrintPropertiesEnableField = true;
            this.OnMultiCashDeposit = new StateEvent(StateEvent.EventType.navigate, "multiCashDeposit.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnReturningCashAdvice = new StateEvent(StateEvent.EventType.runScript, "ReturningCashAdvice", "");
            this.OnMultiCashDepositSummary = new StateEvent(StateEvent.EventType.runScript, "MultiCashDepositSummary", "");
            this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCash", "");
            this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinterError1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashError", "");
            this.OnPrintTicketOnReceiptPrinterError2 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCashError", "");
            this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashError", "");
            this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1DepositCashError", "");

            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.MiniBank_JH6000_D:
                case Enums.TerminalModel.MiniBank_JH600_A:
                    this.depositWithoutEscrowFullField = false;
                    this.autoDepositField = true;
                    break;
            }
        }

        #region "Properties"
        [XmlElement()]
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

        [XmlElement()]
        public string EnterNextStateNumber
        {
            get
            {
                return this.enterNextStateNumberField;
            }
            set
            {
                this.enterNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string TimeOutNextStateNumber
        {
            get
            {
                return this.timeOutNextSateNumberField;
            }
            set
            {
                this.timeOutNextSateNumberField = value;
            }
        }

        [XmlElement()]
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

        [XmlElement()]
        public string MoreDepositNextStateNumber
        {
            get
            {
                return this.moreDepositNextStateNumberField;
            }
            set
            {
                this.moreDepositNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string HostName
        {
            get
            {
                return this.hostNameField;
            }
            set
            {
                this.hostNameField = value;
            }
        }

        [XmlElement()]
        public bool AutoDeposit
        {
            get
            {
                return this.autoDepositField;
            }
            set
            {
                this.autoDepositField = value;
            }
        }

        [XmlElement()]
        public bool DepositAtError
        {
            get
            {
                return this.depositAtErrorField;
            }
            set
            {
                this.depositAtErrorField = value;
            }
        }


        [XmlElement()]
        public bool DepositWithoutEscrowFull
        {
            get
            {
                return this.depositWithoutEscrowFullField;
            }
            set
            {
                this.depositWithoutEscrowFullField = value;
            }
        }

        [XmlElement()]
        public bool RejectDetect
        {
            get
            {
                return this.rejectDetectField;
            }
            set
            {
                this.rejectDetectField = value;
            }
        }
        
        [XmlElement()]
        public bool SendDepositStatus
        {
            get
            {
                return this.sendDepositStatusField;
            }
            set
            {
                this.sendDepositStatusField = value;
            }
        }

        [XmlElement()]
        public string MaximumNumberOfDeposit
        {
            get
            {
                return this.maximumNumberOfDepositField;
            }
            set
            {
                this.maximumNumberOfDepositField = value;
            }
        }

        [XmlElement()]
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

        [XmlElement()]
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

        [XmlElement()]
        public bool PrintPropertiesEnable
        {
            get
            {
                return this.PrintPropertiesEnableField;
            }
            set
            {
                this.PrintPropertiesEnableField = value;
            }
        }
        #endregion "Properties"
    }
}