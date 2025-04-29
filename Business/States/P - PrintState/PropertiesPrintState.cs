using Business.States;
using Entities;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Business.PrintState
{
    [Serializable]
    public partial class PropertiesPrintState
    {
        private string screenNumberField;
        private string goodOperationNextStateField;
        private string hardwareFaultNextStateField;
        private PrinterFlag_Type unitNumberField;
        private string operationField;
        private int screenTimerField;
        private int operationTimerField;
        private string fdkActiveMaskField;
        private string printBufferIDField;
        private string itemField;
        private string hostNameField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        [XmlElement()]
        public StateEvent OnPrint;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinter1;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinter2;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptDataFail;
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
        [XmlElement()]
        public StateEvent OnShowReceiptOptions;
        [XmlElement()]
        public StateEvent OnTakeReceipt;
        public List<StateEvent> OnPrintItems;

        public PropertiesPrintState() { }

        public PropertiesPrintState(AlephATMAppData alephATMAppData)
        {
            this.screenNumberField = "";
            this.goodOperationNextStateField = "";
            this.hardwareFaultNextStateField = "";
            this.unitNumberField = PrinterFlag_Type.none;
            this.operationField = "";
            this.screenTimerField = 0;
            this.operationTimerField = 0;
            this.fdkActiveMaskField = "";
            this.printBufferIDField = "";
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnPrint = new StateEvent(StateEvent.EventType.ignore, "print.htm", "");
            this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1RetrievalDepositCash", "");
            this.OnPrintTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1RetrievalDepositCash", "");
            this.OnPrintTicketOnReceiptDataFail = new StateEvent(StateEvent.EventType.printReceipt, "Pr1CompletedDataRetrievalFail", "");
            this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr2RetrievalDepositCash", "");
            this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1RetrievalDepositCash", "");
            this.OnPrintTicketOnReceiptPrinterError1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1RetrievalDepositCashError", "");
            this.OnPrintTicketOnReceiptPrinterError2 = new StateEvent(StateEvent.EventType.ignore, "Pr1RetrievalDepositCashError", "");
            this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "Pr2RetrievalDepositCashError", "");
            this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1RetrievalDepositCashError", "");
            this.OnShowReceiptOptions = new StateEvent(StateEvent.EventType.runScript, "OfferReceiptOptions", "");
            this.OnTakeReceipt = new StateEvent(StateEvent.EventType.runScript, "TakeReceipt", "");

            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.CTR50:
                    this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1PaymentCompletedTicket1", "");
                    this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr1PaymentCompletedTicket1", "");
                    this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.ignore, "Pr1RetrievalDepositCash", "");
                    this.OnPrintTicketOnReceiptPrinterError1 = new StateEvent(StateEvent.EventType.printReceipt, "PaymentTicketError", "");
                    this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "PaymentTicketError", "");
                    this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.ignore, "Pr1RetrievalDepositCashError", "");
                    this.hostNameField = "RedpagosMulesoftHost";
                    break;
                case Enums.TerminalModel.Depositario:
                    this.OnPrint.Action = StateEvent.EventType.navigate;
                    this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.ignore, "Pr1RetrievalDepositCash", "");
                    this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.ignore, "Pr1RetrievalDepositCashError", "");
                    this.operationTimerField = 30;
                    this.screenTimerField = 10;
                    break;
            }
        }

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
        public string GoodOperationNextState
        {
            get
            {
                return this.goodOperationNextStateField;
            }
            set
            {
                this.goodOperationNextStateField = value;
            }
        }

        [XmlElement()]
        public string HardwareFaultNextState
        {
            get
            {
                return this.hardwareFaultNextStateField;
            }
            set
            {
                this.hardwareFaultNextStateField = value;
            }
        }

        [XmlElement()]
        public PrinterFlag_Type UnitNumber
        {
            get
            {
                return this.unitNumberField;
            }
            set
            {
                this.unitNumberField = value;
            }
        }

        [XmlElement()]
        public string Operation
        {
            get
            {
                return this.operationField;
            }
            set
            {
                this.operationField = value;
            }
        }

        [XmlElement()]
        public int ScreenTimer
        {
            get
            {
                return this.screenTimerField;
            }
            set
            {
                this.screenTimerField = value;
            }
        }

        [XmlElement()]
        public int OperationTimer
        {
            get
            {
                return this.operationTimerField;
            }
            set
            {
                this.operationTimerField = value;
            }
        }

        [XmlElement()]
        public string FdkActiveMask
        {
            get
            {
                return this.fdkActiveMaskField;
            }
            set
            {
                this.fdkActiveMaskField = value;
            }
        }

        [XmlElement()]
        public string PrintBufferID
        {
            get
            {
                return this.printBufferIDField;
            }
            set
            {
                this.printBufferIDField = value;
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
        public string HostName
        {
            get => hostNameField;
            set => hostNameField = value;
        }
    }
}