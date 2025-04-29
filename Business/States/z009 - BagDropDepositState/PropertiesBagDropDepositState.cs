using Business.States;
using Entities;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Business.BagDropDepositState
{
    [Serializable]
    public partial class PropertiesBagDropDepositState
    {
        private string Item2Field;
        private string NextStateNumberField;
        private string CancelNextStateNumberField;
        private string HardwareErrorNextStateNumberField;
        private string TimeOutNextStateNumberField;
        private Const.ActionOnTimeOut ActionOnTimeOutField;
        private Enums.CimModel OperationModeField;
        private Extension1 Extension1Field;
        private JournalProperties JournalField;
        private MoreTimeProperties MoreTimeField;
        private bool VerifyPrinterField;
        private bool VerifyLogicalFullBinField = true;
        private bool ActiveResetField = false;
        private string HostNameField;
        [XmlElement()]
        public StateEvent OnShowBagdropDepositConfig;
        [XmlElement()]
        public StateEvent OnDepositBag;
        [XmlElement()]
        public StateEvent OnPleaseWait;
        [XmlElement()]
        public StateEvent OnBagDropSummary;
        [XmlElement()]
        public StateEvent OnOpenEscrowAdvice;
        [XmlElement()]
        public StateEvent OnCancelDepositAdvice;
        [XmlElement()]
        public StateEvent OnCashAcceptCassetteFull;
        [XmlElement()]
        public StateEvent OnEmptyEscrowAdvice;
        [XmlElement()]
        public StateEvent OnNotEmptyEscrowAdvice;
        [XmlElement()]
        public StateEvent OnCassetteNotPresentAdvice;
        [XmlElement()]
        public StateEvent OnChestDoorOpenAdvice;
        [XmlElement()]
        public StateEvent OnPrintFirstTicket;
        [XmlElement()]
        public StateEvent OnPrintSecondTicketOnReceiptPrinter1;
        [XmlElement()]
        public StateEvent OnPrintSecondTicketOnReceiptPrinter2;
        [XmlElement()]
        public StateEvent OnPrintSecondTicketOnJournalPrinter;
        [XmlElement()]
        public StateEvent OnSendTicketToBD;
        [XmlElement()]
        public StateEvent OnPrintSecondTicketOnReceiptPrinterError1;
        [XmlElement()]
        public StateEvent OnPrintSecondTicketOnReceiptPrinterError2;
        [XmlElement()]
        public StateEvent OnPrintSecondTicketOnJournalPrinterError;
        [XmlElement()]
        public StateEvent OnSendTicketToBDError;
        [XmlElement()]
        public StateEvent OnSensorErrorAdvice;
        public List<string> SelectableCurrencies;
        public List<string> SelectableContents = new List<string>() { "billetes", "cheques", "tickets", "documentos" }; //No traducir!

        public PropertiesBagDropDepositState() { }

        public PropertiesBagDropDepositState(AlephATMAppData alephATMAppData)
        {
            this.Item2Field = "";
            this.NextStateNumberField = "";
            this.CancelNextStateNumberField = "";
            this.HardwareErrorNextStateNumberField = "";
            this.TimeOutNextStateNumberField = "";
            this.Extension1Field = new Extension1();
            this.JournalField = new JournalProperties();
            this.MoreTimeField = new MoreTimeProperties();
            this.HostNameField = "PrdmHost";
            this.OnDepositBag = new StateEvent(StateEvent.EventType.navigate, "bagDropDeposit.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnBagDropSummary = new StateEvent(StateEvent.EventType.runScript, "BagDropSummary", "");
            this.OnOpenEscrowAdvice = new StateEvent(StateEvent.EventType.runScript, "OpenEscrowAdvice", "");
            this.OnCancelDepositAdvice = new StateEvent(StateEvent.EventType.runScript, "CancelDepositAdvice", "");
            this.OnEmptyEscrowAdvice = new StateEvent(StateEvent.EventType.runScript, "EmptyEscrowAdvice", "");
            this.OnNotEmptyEscrowAdvice = new StateEvent(StateEvent.EventType.runScript, "NotEmptyEscrowAdvice", "");
            this.OnCashAcceptCassetteFull = new StateEvent(StateEvent.EventType.runScript, "CashAcceptCassetteFull", "");
            this.OnCassetteNotPresentAdvice = new StateEvent(StateEvent.EventType.runScript, "CassetteNotPresentAdvice", "");
            this.OnChestDoorOpenAdvice = new StateEvent(StateEvent.EventType.runScript, "ChestDoorOpenAdvice", "");
            this.OnPrintFirstTicket = new StateEvent(StateEvent.EventType.printReceipt, "Pr1BagDropDepositTicket1", "");
            this.OnPrintSecondTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1BagDropDepositTicket2", "");
            this.OnPrintSecondTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1BagDropDepositTicket2", "");
            this.OnPrintSecondTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr2BagDropDepositTicket2", "");
            this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1BagDropDepositTicket2", "");
            this.OnPrintSecondTicketOnReceiptPrinterError1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1BagDropDepositTicket2Error", "");
            this.OnPrintSecondTicketOnReceiptPrinterError2 = new StateEvent(StateEvent.EventType.ignore, "Pr1BagDropDepositTicket2Error", "");
            this.OnPrintSecondTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "Pr2BagDropDepositTicket2Error", "");
            this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.sendTicketToBD, "Pr1BagDropDepositTicket2Error", "");
            this.OnSensorErrorAdvice = new StateEvent(StateEvent.EventType.runScript, "SensorErrorAdvice", "");
            this.OnShowBagdropDepositConfig = new StateEvent(StateEvent.EventType.runScript, "ShowBackdropDepositConfig", "");
            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.Glory_DE_50:
                    this.ActiveReset = true;
                    this.OperationModeField = Enums.CimModel.Glory;
                    this.VerifyPrinterField = false;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.Persist;
                    this.SelectableCurrencies = new List<string>() { alephATMAppData.DefaultCurrency, "EUR", "USD" };
                    break;
                case Enums.TerminalModel.TAS1:
                    this.ActiveReset = true;
                    this.OperationModeField = Enums.CimModel.Glory;
                    this.VerifyPrinterField = false;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.Persist;
                    this.SelectableCurrencies = new List<string>() { "UYU", "ARS", "USD" };
                    break;
                case Enums.TerminalModel.MiniBank_JH6000_D:
                case Enums.TerminalModel.MiniBank_JH600_A:
                    this.OperationModeField = Enums.CimModel.MEI;
                    this.VerifyPrinterField = false;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.EndSession;
                    this.SelectableCurrencies = new List<string>() { alephATMAppData.DefaultCurrency, "USD" };
                    break;
                case Enums.TerminalModel.V200:
                    this.OperationModeField = Enums.CimModel.MEI;
                    this.VerifyPrinterField = false;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.EndSession;
                    this.SelectableCurrencies = new List<string>() { alephATMAppData.DefaultCurrency, "EUR", "USD" };
                    break;
                case Enums.TerminalModel.SNBC_IN:
                    this.OperationModeField = Enums.CimModel.SNBC;
                    this.VerifyPrinterField = true;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.Persist;
                    this.SelectableCurrencies = new List<string>() { alephATMAppData.DefaultCurrency };
                    break;
                case Enums.TerminalModel.MADERO_BR:
                    this.OperationModeField = Enums.CimModel.MEI;
                    this.VerifyPrinterField = true;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.EndSession;
                    this.SelectableCurrencies = new List<string>() { alephATMAppData.DefaultCurrency, "EUR", "USD" };
                    break;
                default://SNBC_CTI90 - SNBC_CTE1 - CTIUL - Depositario
                    this.OperationModeField = Enums.CimModel.SNBC;
                    this.VerifyPrinterField = true;
                    this.ActionOnTimeOutField = Const.ActionOnTimeOut.Persist;
                    this.SelectableCurrencies = new List<string>() { alephATMAppData.DefaultCurrency, "EUR", "USD" };
                    break;
            }
        }

        #region Properties 
        [XmlElement()]
        public string Item2
        {
            get
            {
                return this.Item2Field;
            }
            set
            {
                this.Item2Field = value;
            }
        }

        [XmlElement()]
        public string NextStateNumber
        {
            get
            {
                return this.NextStateNumberField;
            }
            set
            {
                this.NextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string CancelNextStateNumber
        {
            get
            {
                return this.CancelNextStateNumberField;
            }
            set
            {
                this.CancelNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string HardwareErrorNextStateNumber
        {
            get
            {
                return this.HardwareErrorNextStateNumberField;
            }
            set
            {
                this.HardwareErrorNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string TimeOutNextStateNumber
        {
            get
            {
                return this.TimeOutNextStateNumberField;
            }
            set
            {
                this.TimeOutNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnTimeOut ActionOnTimeOut
        {
            get
            {
                return this.ActionOnTimeOutField;
            }
            set
            {
                this.ActionOnTimeOutField = value;
            }
        }

        [XmlElement()]
        public Enums.CimModel OperationMode
        {
            get
            {
                return this.OperationModeField;
            }
            set
            {
                this.OperationModeField = value;
            }
        }

        [XmlElement()]
        public Extension1 Extension1
        {
            get
            {
                return this.Extension1Field;
            }
            set
            {
                this.Extension1Field = value;
            }
        }

        [XmlElement()]
        public JournalProperties Journal
        {
            get
            {
                return this.JournalField;
            }
            set
            {
                this.JournalField = value;
            }
        }

        [XmlElement()]
        public MoreTimeProperties MoreTime
        {
            get
            {
                return this.MoreTimeField;
            }
            set
            {
                this.MoreTimeField = value;
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

        //[XmlElement()]
        //public bool VerifySensors
        //{
        //    get
        //    {
        //        return this.VerifySensorsField;
        //    }
        //    set
        //    {
        //        this.VerifySensorsField = value;
        //    }
        //}

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

        //[XmlElement()]
        //public bool StateXfsPrinter
        //{
        //    get
        //    {
        //        return this.StateXfsPrinterField;
        //    }
        //    set
        //    {
        //        this.StateXfsPrinterField = value;
        //    }
        //}

        [XmlElement()]
        public bool VerifyLogicalFullBin
        {
            get
            {
                return this.VerifyLogicalFullBinField;
            }
            set
            {
                this.VerifyLogicalFullBinField = value;
            }
        }

        [XmlElement()]
        public bool ActiveReset
        {
            get
            {
                return this.ActiveResetField;
            }
            set
            {
                this.ActiveResetField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension1
    {
        private string stateNumberField;
        private string screenModeField;
        private string mainScreenNumberField;
        private string processScreenNumberField;
        private string confirmationScreenNumberField;
        private string depositMaxQuantityField;
        private string language6Field;
        private string language7Field;
        private string language8Field;

        public Extension1()
        {
            //Manejo de pantalla por HTML y Javascript
            this.screenModeField = "001";
            this.DepositMaxQuantity = "020";
        }

        #region Properties
        [XmlElement()]
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

        [XmlElement()]
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

        [XmlElement()]
        public string MainScreenNumber
        {
            get
            {
                return this.mainScreenNumberField;
            }
            set
            {
                this.mainScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string ProcessScreenNumber
        {
            get
            {
                return this.processScreenNumberField;
            }
            set
            {
                this.processScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string ConfirmationScreenNumber
        {
            get
            {
                return this.confirmationScreenNumberField;
            }
            set
            {
                this.confirmationScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string DepositMaxQuantity
        {
            get
            {
                return this.depositMaxQuantityField;
            }
            set
            {
                this.depositMaxQuantityField = value;
            }
        }

        [XmlElement()]
        public string Language6
        {
            get
            {
                return this.language6Field;
            }
            set
            {
                this.language6Field = value;
            }
        }

        [XmlElement()]
        public string Language7
        {
            get
            {
                return this.language7Field;
            }
            set
            {
                this.language7Field = value;
            }
        }

        [XmlElement()]
        public string Language8
        {
            get
            {
                return this.language8Field;
            }
            set
            {
                this.language8Field = value;
            }
        }


        #endregion Properties
    }
}