using Business.States;
using Entities;
using System;
using System.Xml.Serialization;

namespace Business.ShipoutState
{
    [Serializable]
    public class PropertiesShipoutState
    {
        private string sendCollectionField;
        private string sendCollectionDeclaredField;
        private string sendContentsField;
        private string printTicketField;
        private string updateTSNField;
        private string clearLogicalCountersField;
        private string clearPhysicalCountersField;
        private string nextStateField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        private bool showScreenField;
        private string hostNameField;
        public StateEvent OnPrintTicketOnReceiptPrinterCashDeposit;
        public StateEvent OnPrintTicketOnReceiptPrinterBagDropDeposit;
        public StateEvent OnPrintTicketOnReceiptPrinterCoinDeposit;
        public StateEvent OnPrintTicketOnReceiptPrinterChequeDeposit;
        public StateEvent OnPrintTicketOnJournalPrinter;
        public StateEvent OnPrintTicketToBDCashDeposit;
        public StateEvent OnPrintTicketToBDBagDropDeposit;
        public StateEvent OnPrintTicketToBDCoinDeposit;
        public StateEvent OnPrintTicketToBDChequeDeposit;
        public StateEvent OnPrintTicketOnReceiptPrinterCashDepositError;
        public StateEvent OnPrintTicketOnReceiptPrinterBagDropDepositError;
        public StateEvent OnPrintTicketOnReceiptPrinterCoinDepositError;
        public StateEvent OnPrintTicketOnReceiptPrinterChequeDepositError;
        public StateEvent OnPrintTicketOnJournalPrinterError;
        public StateEvent OnPrintTicketToBDCashDepositError;
        public StateEvent OnPrintTicketToBDBagDropDepositError;
        public StateEvent OnPrintTicketToBDCoinDepositError;
        public StateEvent OnPrintTicketToBDChequeDepositError;
        public StateEvent OnPrintLocalCounters;
        public StateEvent OnShipoutStart;
        public StateEvent OnShipoutLoad;

        public PropertiesShipoutState() { }

        #region Properties
        [XmlElement]
        public string SendCollection
        {
            get
            {
                return sendCollectionField;
            }
            set
            {
                sendCollectionField = value;
            }
        }

        [XmlElement]
        public string SendCollectionDeclared
        {
            get
            {
                return sendCollectionDeclaredField;
            }
            set
            {
                sendCollectionDeclaredField = value;
            }
        }

        [XmlElement]
        public string SendContents
        {
            get
            {
                return sendContentsField;
            }
            set
            {
                sendContentsField = value;
            }
        }

        [XmlElement]
        public string PrintTicket
        {
            get
            {
                return printTicketField;
            }
            set
            {
                printTicketField = value;
            }
        }

        [XmlElement]
        public string UpdateTSN
        {
            get
            {
                return updateTSNField;
            }
            set
            {
                updateTSNField = value;
            }
        }

        [XmlElement]
        public bool ShowScreen
        {
            get
            {
                return showScreenField;
            }
            set
            {
                showScreenField = value;
            }
        }

        [XmlElement]
        public string HostName
        {
            get
            {
                return hostNameField;
            }
            set
            {
                hostNameField = value;
            }
        }

        [XmlElement]
        public string ClearLogicalCounters
        {
            get
            {
                return clearLogicalCountersField;
            }
            set
            {
                clearLogicalCountersField = value;
            }
        }

        [XmlElement]
        public string ClearPhysicalCounters
        {
            get
            {
                return clearPhysicalCountersField;
            }
            set
            {
                clearPhysicalCountersField = value;
            }
        }

        [XmlElement]
        public string NextState
        {
            get
            {
                return nextStateField;
            }
            set
            {
                nextStateField = value;
            }
        }

        [XmlElement]
        public JournalProperties Journal
        {
            get
            {
                return journalField;
            }
            set
            {
                journalField = value;
            }
        }

        [XmlElement]
        public MoreTimeProperties MoreTime
        {
            get
            {
                return moreTimeField;
            }
            set
            {
                moreTimeField = value;
            }
        }
        #endregion Properties

        public PropertiesShipoutState(AlephATMAppData alephATMAppData)
        {
            sendCollectionField = "";
            sendCollectionDeclaredField = "";
            sendContentsField = "";
            printTicketField = "";
            updateTSNField = "";
            clearLogicalCountersField = "";
            nextStateField = "";
            journalField = new JournalProperties();
            moreTimeField = new MoreTimeProperties();
            showScreenField = true;
            hostNameField = "PrdmHost";
            clearPhysicalCountersField = "001";
            OnShipoutStart = new StateEvent(StateEvent.EventType.navigate, "shipout.htm", "");
            OnShipoutLoad = new StateEvent(StateEvent.EventType.runScript, "LoadConfigurationData", "");
            OnPrintTicketOnReceiptPrinterCashDeposit = new StateEvent(StateEvent.EventType.printJournalAndReceipt, "Pr1ShipOutNotes", "");
            OnPrintTicketOnReceiptPrinterBagDropDeposit = new StateEvent(StateEvent.EventType.printJournalAndReceipt, "Pr1ShipOutBags", "");
            OnPrintTicketOnReceiptPrinterCoinDeposit = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketOnReceiptPrinterChequeDeposit = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketToBDCashDeposit = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1ShipOutNotes", "");
            OnPrintTicketToBDBagDropDeposit = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1ShipOutBags", "");
            OnPrintTicketToBDCoinDeposit = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketToBDChequeDeposit = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketOnReceiptPrinterCashDepositError = new StateEvent(StateEvent.EventType.printJournalAndReceipt, "Pr1ShipOutNotesError", "");
            OnPrintTicketOnReceiptPrinterBagDropDepositError = new StateEvent(StateEvent.EventType.printJournalAndReceipt, "Pr1ShipOutBagsError", "");
            OnPrintTicketOnReceiptPrinterCoinDepositError = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketOnReceiptPrinterChequeDepositError = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketToBDCashDepositError = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1ShipOutNotesError", "");
            OnPrintTicketToBDBagDropDepositError = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1ShipOutBagsError", "");
            OnPrintTicketToBDCoinDepositError = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketToBDChequeDepositError = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.ignore, "", "");
            OnPrintLocalCounters = new StateEvent(StateEvent.EventType.printJournal, "Pr1LocalCounters", "");
            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.CTR50:
                case Enums.TerminalModel.Depositario:
                    sendCollectionDeclaredField = "000";
                    break;
            }
        }
    }
}
