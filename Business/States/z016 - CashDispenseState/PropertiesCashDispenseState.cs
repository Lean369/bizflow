using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.CashDispenseState
{
    [Serializable]
    public partial class PropertiesCashDispenseState
    {
        private string screenNumberField;
        private string nextStateNumberField;
        private string hardwareErrorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string item1Field;
        private string item2Field;
        private JournalProperties journalField;
        private bool PrintPropertiesEnableField;
        private bool VerifyPrinterField;
        private bool StateXfsPrinterField;
        private string MixSelectionField;
        private string HostNameField;
        private int takeCashTimeoutField;
        [XmlElement()]
        public StateEvent OnShowScreen;
        [XmlElement()]
        public StateEvent OnPleaseWait;
        [XmlElement()]
        public StateEvent OnTakeCash;
        [XmlElement()]
        public StateEvent OnChestDoorOpenAdvice;
        [XmlElement()]
        public StateEvent OnSensorErrorAdvice;
        [XmlElement()]
        public StateEvent OnRetractCashAdvice;
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
        public PropertiesCashDispenseState () { }
        public PropertiesCashDispenseState(AlephATMAppData alephATMAppData)
        {
            this.screenNumberField = "";
            this.nextStateNumberField = "";
            this.hardwareErrorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.item1Field = "";
            this.item2Field = "";
            this.journalField = new JournalProperties();
            this.PrintPropertiesEnableField = true;
            this.VerifyPrinterField = true;
            this.StateXfsPrinterField = true;
            this.HostNameField = "GenericHost";
            this.takeCashTimeoutField = 30;
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "cashDispense.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnChestDoorOpenAdvice = new StateEvent(StateEvent.EventType.runScript, "ChestDoorOpenAdvice", "");
            this.OnSensorErrorAdvice = new StateEvent(StateEvent.EventType.runScript, "SensorErrorAdvice", "");
            this.OnTakeCash = new StateEvent(StateEvent.EventType.runScript, "TakeCash", "");
            this.OnRetractCashAdvice = new StateEvent(StateEvent.EventType.runScript, "RetractCashAdvice", "");
            this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCash", "");
            this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinterError = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashError", "");
            this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashError", "");
            this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.ignore, "0", "");
            this.OnPrintTicketOnReceiptPrinterDeclined = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashDeclined", "");
            this.OnPrintTicketOnJournalPrinterDeclined = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashDeclined", "");

            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.CTR50:
                    this.HostNameField = "RedpagosMulesoftHost";
                    this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
                    this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.ignore, "Pr2DepositCash", "");
                    break;
                case Enums.TerminalModel.Depositario:
                    this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.ignore, "Pr1AccountDepositCash", "");
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

        [XmlElement()]
        public string HardwareErrorNextStateNumber
        {
            get
            {
                return this.hardwareErrorNextStateNumberField;
            }
            set
            {
                this.hardwareErrorNextStateNumberField = value;
            }
        }

        [XmlElement()]
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
        public string Item1
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

        [XmlElement()]
        public string Item2
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

        [XmlElement()]
        public bool VerifyPrinter
        {
            get
            {
                return this.VerifyPrinterField;
            }
            set
            {
                this.VerifyPrinterField = value;
            }
        }

        [XmlElement()]
        public bool StateXfsPrinter
        {
            get
            {
                return this.StateXfsPrinterField;
            }
            set
            {
                this.StateXfsPrinterField = value;
            }
        }

        [XmlElement()]
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

        [XmlElement()]
        public int TakeCashTimeout
        {
            get
            {
                return this.takeCashTimeoutField;
            }
            set
            {
                this.takeCashTimeoutField = value;
            }
        }
        #endregion Properties
    }
}