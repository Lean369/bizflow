using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.CashAcceptState
{

    [Serializable]
    public partial class PropertiesCashAcceptState
    {
        private string CancelKeyMaskField;
        private string DepositKeyMaskField;
        private string AddMoreKeyMaskField;
        private string RefundKeyMaskField;
        private Extension1 Extension1Field;
        private Extension2 Extension2Field;
        private Extension3 Extension3Field;
        private int MaxNotesToAcceptField = 200;
        private string CurrencyField;
        private MoreTimeProperties MoreTimeField;
        private JournalProperties JournalField;
        private bool PrintPropertiesEnableField = true;
        private bool VerifyPrinterField = true;
        private bool VerifyAcceptorField = true;
        private bool VerifyBanknoteReaderField = true;
        private bool VerifyLogicalFullBinField = true;
        private int QtyActiveShakerField = 3000;
        private bool AddRecognizedValuesField = false;
        private bool ConfigBankNotesField = true;
        private bool CurrencyAcceptedUniqueField = true;
        private bool ShowDismissButtonOnRejectField = false;
        private bool AutoCashInField = false; //Only MEI
        private bool DirectCashInField = false; //Only P2600
        private bool AnalyzeCashInEndBufferField = true;
        private bool MinAmountVerificationField = false;
        private bool ForceOpenShutterAtRejectField = false;
        private bool ForceOpenInputShutterAtEscrowFullField = false;
        private bool ForceOpenInputShutterAtCashInStartField = false;
        private bool ForceOpenInputShutterAtPickFailField = false;
        private string HostNameField;
        private bool ActiveResetAtCashInStartField = true;
        private bool ActiveResetAtCancelField = true;
        private bool UpdateConfigurationFileField = false;
        private Const.ActionOnCashAcceptError CashHandlingOnOpenCimTimeOutField;
        private Const.ActionOnCashAcceptError CashHandlingOnStatusErrorField;
        private Const.ActionOnCashAcceptError CashHandlingOnCashInErrorField;
        private Const.ActionOnCashAcceptError CashHandlingOnCashInEndErrorField;
        private Const.ActionOnCashAcceptError CashHandlingOnStatusNotesInEscrowField;
        private Const.ActionOnCashAcceptError CashHandlingOnRetractField;
        private Const.ActionOnCashAcceptError CashHandlingOnRollbackField;
        private Const.CountersType CountersTypeField;
        [XmlElement()]
        public StateEvent OnInsertNotes;
        [XmlElement()]
        public StateEvent OnPleaseWait;
        [XmlElement()]
        public StateEvent OnCashInErrorAdvice;
        [XmlElement()]
        public StateEvent OnCashInEndErrorAdvice;
        [XmlElement()]
        public StateEvent OnConfirmDepositedCash;
        [XmlElement()]
        public StateEvent OnShowConfirmModal;//Pregunta si desea continuar con el depòsito
        [XmlElement()]
        public StateEvent OnShowDepositedCash;
        [XmlElement()]
        public StateEvent OnShowRecognizedCash;
        [XmlElement()]
        public StateEvent OnReturningCashAdvice;
        [XmlElement()]
        public StateEvent OnRollbackCashAdvice;
        [XmlElement()]
        public StateEvent OnStoringCashAdvice;
        [XmlElement()]
        public StateEvent OnCashInsertedAdvice;
        [XmlElement()]
        public StateEvent OnDepositCashPrepare;
        [XmlElement()]
        public StateEvent OnRetractCashAdvice;
        [XmlElement()]
        public StateEvent OnCashAcceptCassetteFullAdvice;
        [XmlElement()]
        public StateEvent OnCassetteNotPresentAdvice;
        [XmlElement()]
        public StateEvent OnChestDoorOpenAdvice;
        [XmlElement()]
        public StateEvent OnSensorErrorAdvice;
        [XmlElement()]
        public StateEvent OnStatusNotesInEscrowAdvice;
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
        public StateEvent OnPrintTicketOnReceiptPrinterError1;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinterError;
        [XmlElement()]
        public StateEvent OnSendTicketToBDError;
        [XmlElement()]
        public StateEvent OnPrintTicketOnReceiptPrinterDeclined;
        [XmlElement()]
        public StateEvent OnPrintTicketOnJournalPrinterDeclined;

        public PropertiesCashAcceptState() { }

        public PropertiesCashAcceptState(AlephATMAppData alephATMAppData)
        {
            this.CancelKeyMaskField = "";
            this.DepositKeyMaskField = "";
            this.AddMoreKeyMaskField = "";
            this.RefundKeyMaskField = "";
            this.Extension1Field = new Extension1();
            this.Extension2Field = new Extension2();
            this.Extension3Field = new Extension3();
            this.CurrencyField = alephATMAppData.DefaultCurrency;
            this.MoreTimeField = new MoreTimeProperties();
            this.JournalField = new JournalProperties();
            this.HostNameField = "GenericHost";
            this.CashHandlingOnOpenCimTimeOutField = Const.ActionOnCashAcceptError.Reset; //Acción luego de un error en Open CIM command
            this.CashHandlingOnStatusErrorField = Const.ActionOnCashAcceptError.Reset; //Acción luego de un error en Status command
            this.CashHandlingOnCashInErrorField = Const.ActionOnCashAcceptError.Eject; //Acción luego de un error en CashIn command
            this.CashHandlingOnCashInEndErrorField = Const.ActionOnCashAcceptError.EndSession; //Acción luego de un error en CashInEnd
            this.CashHandlingOnStatusNotesInEscrowField = Const.ActionOnCashAcceptError.EndSession; //Acción luego detecciòn de billetes en escrow 
            this.CashHandlingOnRetractField = Const.ActionOnCashAcceptError.EndSession; //Acción luego de la activación del retract 
            this.CashHandlingOnRollbackField = Const.ActionOnCashAcceptError.Eject; //Acción luego de recibir un error en comando Rollback
            this.CountersTypeField = Const.CountersType.Physical;
            this.OnInsertNotes = new StateEvent(StateEvent.EventType.navigate, "cashDeposit.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnCashInErrorAdvice = new StateEvent(StateEvent.EventType.runScript, "CashInErrorAdvice", "");
            this.OnCashInEndErrorAdvice = new StateEvent(StateEvent.EventType.runScript, "CashInEndErrorAdvice", "");
            this.OnCashInsertedAdvice = new StateEvent(StateEvent.EventType.runScript, "CashInsertedAdvice", "");
            this.OnDepositCashPrepare = new StateEvent(StateEvent.EventType.runScript, "DepositCashPrepare", "");
            this.OnConfirmDepositedCash = new StateEvent(StateEvent.EventType.runScript, "ConfirmDepositedCash", "");
            this.OnShowConfirmModal = new StateEvent(StateEvent.EventType.ignore, "showConfirmModal", "");
            this.OnShowDepositedCash = new StateEvent(StateEvent.EventType.runScript, "ShowDepositedCash", "");
            this.OnShowRecognizedCash = new StateEvent(StateEvent.EventType.runScript, "ShowRecognizedCash", "");
            this.OnStoringCashAdvice = new StateEvent(StateEvent.EventType.runScript, "StoringCashAdvice", "");
            this.OnReturningCashAdvice = new StateEvent(StateEvent.EventType.runScript, "ReturningCashAdvice", "");
            this.OnRollbackCashAdvice = new StateEvent(StateEvent.EventType.runScript, "RollbackCashAdvice", "");
            this.OnRetractCashAdvice = new StateEvent(StateEvent.EventType.runScript, "RetractCashAdvice", "");
            this.OnCashAcceptCassetteFullAdvice = new StateEvent(StateEvent.EventType.runScript, "CashAcceptCassetteFullAdvice", "");
            this.OnCassetteNotPresentAdvice = new StateEvent(StateEvent.EventType.runScript, "CassetteNotPresentAdvice", "");
            this.OnChestDoorOpenAdvice = new StateEvent(StateEvent.EventType.runScript, "ChestDoorOpenAdvice", "");
            this.OnSensorErrorAdvice = new StateEvent(StateEvent.EventType.runScript, "SensorErrorAdvice", "");
            this.OnStatusNotesInEscrowAdvice = new StateEvent(StateEvent.EventType.runScript, "StatusNotesInEscrowAdvice", "");
            this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinter2 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCash", "");
            this.OnSendTicketToBD = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
            this.OnPrintTicketOnReceiptPrinterError = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCashError", "");
            this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.ignore, "Pr2DepositCashError", "");
            this.OnSendTicketToBDError = new StateEvent(StateEvent.EventType.ignore, "0", "");
            this.OnPrintTicketOnReceiptPrinterDeclined = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashDeclined", "");
            this.OnPrintTicketOnJournalPrinterDeclined = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashDeclined", "");
            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.Glory_DE_50:
                case Enums.TerminalModel.TAS1:
                    this.MaxNotesToAcceptField = 100;
                    this.AddRecognizedValuesField = true;
                    this.ConfigBankNotesField = false;
                    this.CurrencyAcceptedUniqueField = false;
                    this.VerifyPrinterField = false;
                    this.VerifyBanknoteReaderField = false;
                    this.ActiveResetAtCancelField = false; //Envía un reset luego de una cancelación de depósito sin billetes previos en escrow
                    this.ShowDismissButtonOnReject = true;
                    break;
                case Enums.TerminalModel.MiniBank_JH6000_D: //MEI
                case Enums.TerminalModel.MiniBank_JH600_A:
                    this.MoreTimeField.MaxTimeOut = 30;
                    this.MaxNotesToAcceptField = 500;
                    this.AddRecognizedValuesField = true;
                    this.ConfigBankNotesField = false;
                    this.CurrencyAcceptedUniqueField = false;
                    this.VerifyPrinterField = false;
                    this.AutoCashInField = true;
                    this.VerifyBanknoteReaderField = false;
                    this.ActiveResetAtCancelField = true; //Envía un reset luego de una cancelación de depósito sin billetes previos en escrow
                    this.UpdateConfigurationFileField = true;
                    this.ShowDismissButtonOnReject = true;
                    break;
                case Enums.TerminalModel.V200:
                    this.MaxNotesToAcceptField = 500;
                    this.AddRecognizedValuesField = true;
                    this.ConfigBankNotesField = false;
                    this.CurrencyAcceptedUniqueField = false;
                    this.VerifyPrinterField = false;
                    this.AutoCashInField = false;
                    this.ActiveResetAtCancelField = false; //Envía un reset luego de una cancelación de depósito sin billetes previos en escrow
                    this.VerifyBanknoteReaderField = true;
                    this.ShowDismissButtonOnReject = true;
                    break;
                case Enums.TerminalModel.GRG_P2600:
                    this.MaxNotesToAcceptField = 200;
                    this.AddRecognizedValuesField = true;
                    this.ConfigBankNotesField = true; //Determina si se envía comando de activación de notas
                    this.CurrencyAcceptedUniqueField = true; //Determina si el reconocimiento de notas sera de una unica moneda. Caso contarario activara notas segun configuración de estado
                    this.VerifyPrinterField = true;
                    this.DirectCashIn = true; //Envía comando CashIn a continuación del CashInStart (sin esperar la activación de sensor de ingreso de billetes)
                    this.ActiveResetAtCancelField = false; //Envía un reset luego de una cancelación de depósito sin billetes previos en escrow
                    this.OnCashInsertedAdvice = new StateEvent(StateEvent.EventType.runScript, "DepositCashPrepare2", "");
                    this.ShowDismissButtonOnReject = true;
                    break;
                case Enums.TerminalModel.CTR50:
                    this.HostNameField = "RedpagosMulesoftHost";
                    this.MoreTimeField.MaxTimeOut = 150; //moreTime 2,5 minutos
                    this.MaxNotesToAcceptField = 300; //VER
                    this.AddRecognizedValuesField = true;
                    this.ConfigBankNotesField = false; //VER | validar solo los templates que quiero que acepte. Seteados en propiedades eje Extension3.SetDenominations1324
                    this.CurrencyAcceptedUniqueField = false;//VER
                    this.VerifyPrinterField = true;
                    this.ForceOpenInputShutterAtCashInStart = true;
                    this.VerifyAcceptorField = false;
                    this.VerifyBanknoteReaderField = false;
                    this.MinAmountVerification = true;
                    this.ForceOpenShutterAtRejectField = true;
                    this.ForceOpenInputShutterAtEscrowFullField = true;
                    this.ForceOpenInputShutterAtPickFailField = true; //Forzar apertura de shutter en caso de pick fail
                    this.ActiveResetAtCancelField = true; //Envía un reset luego de una cancelación de depósito sin billetes previos en escrow
                    this.OnCashInsertedAdvice = new StateEvent(StateEvent.EventType.runScript, "CashInsertedAdvice", "");
                    this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.ignore, "Pr1DepositCash", "");
                    this.OnPrintTicketOnJournalPrinter = new StateEvent(StateEvent.EventType.ignore, "Pr2DepositCash", "");
                    this.OnPrintTicketOnReceiptPrinterError = new StateEvent(StateEvent.EventType.printReceipt, "Pr1DepositCashError", ""); //Error E8
                    this.OnPrintTicketOnJournalPrinterError = new StateEvent(StateEvent.EventType.printJournal, "Pr2DepositCashError", ""); //Error E8
                    this.AnalyzeCashInEndBuffer = false;
                    this.CashHandlingOnRetractField = Const.ActionOnCashAcceptError.Retract;
                    this.CashHandlingOnRollbackField = Const.ActionOnCashAcceptError.Reset;
                    break;
                case Enums.TerminalModel.BDM_300:
                    this.ShowDismissButtonOnRejectField = true;
                    this.AddRecognizedValuesField = true;
                    break;
                case Enums.TerminalModel.Depositario:
                    this.ShowDismissButtonOnRejectField = true;
                    this.OnPrintTicketOnReceiptPrinter1 = new StateEvent(StateEvent.EventType.printReceipt, "Pr1AccountDepositCash", "");
                    break;
                default: //SNBC_CTI90 - SNBC_CTE1 - CTEUL
                    this.ShowDismissButtonOnRejectField = true;
                    break;
            }
        }

        #region Properties
        [XmlElement()]
        public string CancelKeyMask
        {
            get
            {
                return this.CancelKeyMaskField;
            }
            set
            {
                this.CancelKeyMaskField = value;
            }
        }

        [XmlElement()]
        public string DepositKeyMask
        {
            get
            {
                return this.DepositKeyMaskField;
            }
            set
            {
                this.DepositKeyMaskField = value;
            }
        }

        [XmlElement()]
        public string AddMoreKeyMask
        {
            get
            {
                return this.AddMoreKeyMaskField;
            }
            set
            {
                this.AddMoreKeyMaskField = value;
            }
        }

        [XmlElement()]
        public string RefundKeyMask
        {
            get
            {
                return this.RefundKeyMaskField;
            }
            set
            {
                this.RefundKeyMaskField = value;
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
        public Extension2 Extension2
        {
            get
            {
                return this.Extension2Field;
            }
            set
            {
                this.Extension2Field = value;
            }
        }

        [XmlElement()]
        public Extension3 Extension3
        {
            get
            {
                return this.Extension3Field;
            }
            set
            {
                this.Extension3Field = value;
            }
        }

        [XmlElement()]
        public int MaxNotesToAccept
        {
            get
            {
                return this.MaxNotesToAcceptField;
            }
            set
            {
                this.MaxNotesToAcceptField = value;
            }
        }

        [XmlElement()]
        public string Currency
        {
            get
            {
                return this.CurrencyField;
            }
            set
            {
                this.CurrencyField = value;
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
        public bool VerifyAcceptor
        {
            get
            {
                return this.VerifyAcceptorField;
            }
            set
            {
                this.VerifyAcceptorField = value;
            }
        }

        [XmlElement()]
        public bool VerifyBanknoteReader
        {
            get
            {
                return this.VerifyBanknoteReaderField;
            }
            set
            {
                this.VerifyBanknoteReaderField = value;
            }
        }

        [XmlElement()]
        public int QtyActiveShaker
        {
            get
            {
                return this.QtyActiveShakerField;
            }
            set
            {
                this.QtyActiveShakerField = value;
            }
        }

        [XmlElement()]
        public bool AddRecognizedValues
        {
            get
            {
                return this.AddRecognizedValuesField;
            }
            set
            {
                this.AddRecognizedValuesField = value;
            }
        }

        [XmlElement()]
        public bool ConfigBankNotes
        {
            get
            {
                return this.ConfigBankNotesField;
            }
            set
            {
                this.ConfigBankNotesField = value;
            }
        }

        [XmlElement()]
        public bool CurrencyAcceptedUnique
        {
            get
            {
                return this.CurrencyAcceptedUniqueField;
            }
            set
            {
                this.CurrencyAcceptedUniqueField = value;
            }
        }

        [XmlElement()]
        public bool ShowDismissButtonOnReject
        {
            get
            {
                return this.ShowDismissButtonOnRejectField;
            }
            set
            {
                this.ShowDismissButtonOnRejectField = value;
            }
        }

        [XmlElement()]
        public bool AutoCashIn
        {
            get
            {
                return this.AutoCashInField;
            }
            set
            {
                this.AutoCashInField = value;
            }
        }

        [XmlElement()]
        public bool DirectCashIn
        {
            get
            {
                return this.DirectCashInField;
            }
            set
            {
                this.DirectCashInField = value;
            }
        }

        [XmlElement()]
        public bool AnalyzeCashInEndBuffer
        {
            get
            {
                return this.AnalyzeCashInEndBufferField;
            }
            set
            {
                this.AnalyzeCashInEndBufferField = value;
            }
        }

        [XmlElement()]
        public bool MinAmountVerification
        {
            get
            {
                return this.MinAmountVerificationField;
            }
            set
            {
                this.MinAmountVerificationField = value;
            }
        }

        [XmlElement()]
        public bool ForceOpenShutterAtReject
        {
            get
            {
                return this.ForceOpenShutterAtRejectField;
            }
            set
            {
                this.ForceOpenShutterAtRejectField = value;
            }
        }

        [XmlElement()]
        public bool ForceOpenInputShutterAtEscrowFull
        {
            get
            {
                return this.ForceOpenInputShutterAtEscrowFullField;
            }
            set
            {
                this.ForceOpenInputShutterAtEscrowFullField = value;
            }
        }
        
        [XmlElement()]
        public bool ForceOpenInputShutterAtPickFail
        {
            get
            {
                return this.ForceOpenInputShutterAtPickFailField;
            }
            set
            {
                this.ForceOpenInputShutterAtPickFailField = value;
            }
        }

        [XmlElement()]
        public bool ForceOpenInputShutterAtCashInStart
        {
            get
            {
                return this.ForceOpenInputShutterAtCashInStartField;
            }
            set
            {
                this.ForceOpenInputShutterAtCashInStartField = value;
            }
        }

        [XmlElement()]
        public bool UpdateConfigurationFile
        {
            get
            {
                return this.UpdateConfigurationFileField;
            }
            set
            {
                this.UpdateConfigurationFileField = value;
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
        public bool ActiveResetAtCashInStart
        {
            get
            {
                return this.ActiveResetAtCashInStartField;
            }
            set
            {
                this.ActiveResetAtCashInStartField = value;
            }
        }

        [XmlElement()]
        public bool ActiveResetAtCancel
        {
            get
            {
                return this.ActiveResetAtCancelField;
            }
            set
            {
                this.ActiveResetAtCancelField = value;
            }
        }


        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnOpenCimTimeOut
        {
            get
            {
                return this.CashHandlingOnOpenCimTimeOutField;
            }
            set
            {
                this.CashHandlingOnOpenCimTimeOutField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnStatusError
        {
            get
            {
                return this.CashHandlingOnStatusErrorField;
            }
            set
            {
                this.CashHandlingOnStatusErrorField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnCashInError
        {
            get
            {
                return this.CashHandlingOnCashInErrorField;
            }
            set
            {
                this.CashHandlingOnCashInErrorField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnCashInEndError
        {
            get
            {
                return this.CashHandlingOnCashInEndErrorField;
            }
            set
            {
                this.CashHandlingOnCashInEndErrorField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnStatusNotesInEscrow
        {
            get
            {
                return this.CashHandlingOnStatusNotesInEscrowField;
            }
            set
            {
                this.CashHandlingOnStatusNotesInEscrowField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnRetract
        {
            get
            {
                return this.CashHandlingOnRetractField;
            }
            set
            {
                this.CashHandlingOnRetractField = value;
            }
        }

        [XmlElement()]
        public Const.ActionOnCashAcceptError CashHandlingOnRollback
        {
            get
            {
                return this.CashHandlingOnRollbackField;
            }
            set
            {
                this.CashHandlingOnRollbackField = value;
            }
        }

        [XmlElement()]
        public Const.CountersType CountersType
        {
            get
            {
                return this.CountersTypeField;
            }
            set
            {
                this.CountersTypeField = value;
            }
        }

        #endregion Properties
    }

    [Serializable]
    public partial class Extension1
    {
        private string stateNumberField;
        private string pleaseEnterNotesScreenField;
        private string pleaseRemoveNotesScreenField;
        private string confirmationScreenField;
        private string hardwareErrorScreenField;
        private string escrowFullScreenField;
        private string processingNotesScreenField;
        private string pleaseRemoveMoreThan90NotesScreenField;
        private string pleaseWaitScreenField;

        public Extension1() { }

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
        public string PleaseEnterNotesScreen
        {
            get
            {
                return this.pleaseEnterNotesScreenField;
            }
            set
            {
                this.pleaseEnterNotesScreenField = value;
            }
        }

        [XmlElement()]
        public string PleaseRemoveNotesScreen
        {
            get
            {
                return this.pleaseRemoveNotesScreenField;
            }
            set
            {
                this.pleaseRemoveNotesScreenField = value;
            }
        }

        [XmlElement()]
        public string ConfirmationScreen
        {
            get
            {
                return this.confirmationScreenField;
            }
            set
            {
                this.confirmationScreenField = value;
            }
        }

        [XmlElement()]
        public string HardwareErrorScreen
        {
            get
            {
                return this.hardwareErrorScreenField;
            }
            set
            {
                this.hardwareErrorScreenField = value;
            }
        }

        [XmlElement()]
        public string EscrowFullScreen
        {
            get
            {
                return this.escrowFullScreenField;
            }
            set
            {
                this.escrowFullScreenField = value;
            }
        }

        [XmlElement()]
        public string ProcessingNotesScreen
        {
            get
            {
                return this.processingNotesScreenField;
            }
            set
            {
                this.processingNotesScreenField = value;
            }
        }

        [XmlElement()]
        public string PleaseRemoveMoreThan90NotesScreen
        {
            get
            {
                return this.pleaseRemoveMoreThan90NotesScreenField;
            }
            set
            {
                this.pleaseRemoveMoreThan90NotesScreenField = value;
            }
        }

        [XmlElement()]
        public string PleaseWaitScreen
        {
            get
            {
                return this.pleaseWaitScreenField;
            }
            set
            {
                this.pleaseWaitScreenField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension2
    {
        private string stateNumberField;
        private string goodNextStateNumberField;
        private string cancelNextStateNumberField;
        private string deviceErrorNextStateNumberField;
        private string timeOutNextStateNumberField;
        private string declinedNextStateNumberField;
        private string operationModeField;
        private string autoDepositField;
        private string retractingNotesScreenField;

        public Extension2()
        {
            //000: "ATM"    - En la pantalla de billetes leídos, luego de presionar ENTER KEY, realiza un NextState sin depositar los billetes
            //001: "RETAIL" - En la pantalla de billetes leídos, luego de presionar ENTER KEY, deposita los billetes y pasa al estado MultiCash
            //002: "PAY"    - En la pantalla de billetes leídos, luego de presionar ENTER KEY, envía la transacción a autorizar para decidir si deposita o no
            this.autoDepositField = "000";
            //this.operationModeField = "004";
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
        public string GoodNextStateNumber
        {
            get
            {
                return this.goodNextStateNumberField;
            }
            set
            {
                this.goodNextStateNumberField = value;
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
        public string DeviceErrorNextStateNumber
        {
            get
            {
                return this.deviceErrorNextStateNumberField;
            }
            set
            {
                this.deviceErrorNextStateNumberField = value;
            }
        }

        [XmlElement()]
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

        [XmlElement()]
        public string DeclinedNextStateNumber
        {
            get
            {
                return this.declinedNextStateNumberField;
            }
            set
            {
                this.declinedNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string OperationMode
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

        [XmlElement()]
        public string AutoDeposit
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
        public string RetractingNotesScreen
        {
            get
            {
                return this.retractingNotesScreenField;
            }
            set
            {
                this.retractingNotesScreenField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension3
    {
        private string stateNumberField;
        private string setDenominations112Field;
        private string setDenominations1324Field;
        private string setDenominations2536Field;
        private string setDenominations3748Field;
        private string setDenominations4960Field;
        private string setDenominations6172Field;
        private string setDenominations7384Field;
        private string setDenominations8596Field;

        public Extension3() { }

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

        [XmlElementAttribute("SetDenominations1-12")]
        public string SetDenominations112
        {
            get
            {
                return this.setDenominations112Field;
            }
            set
            {
                this.setDenominations112Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations13-24")]
        public string SetDenominations1324
        {
            get
            {
                return this.setDenominations1324Field;
            }
            set
            {
                this.setDenominations1324Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations25-36")]
        public string SetDenominations2536
        {
            get
            {
                return this.setDenominations2536Field;
            }
            set
            {
                this.setDenominations2536Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations37-48")]
        public string SetDenominations3748
        {
            get
            {
                return this.setDenominations3748Field;
            }
            set
            {
                this.setDenominations3748Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations49-60")]
        public string SetDenominations4960
        {
            get
            {
                return this.setDenominations4960Field;
            }
            set
            {
                this.setDenominations4960Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations61-72")]
        public string SetDenominations6172
        {
            get
            {
                return this.setDenominations6172Field;
            }
            set
            {
                this.setDenominations6172Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations73-84")]
        public string SetDenominations7384
        {
            get
            {
                return this.setDenominations7384Field;
            }
            set
            {
                this.setDenominations7384Field = value;
            }
        }

        [XmlElementAttribute("SetDenominations85-96")]
        public string SetDenominations8596
        {
            get
            {
                return this.setDenominations8596Field;
            }
            set
            {
                this.setDenominations8596Field = value;
            }
        }
        #endregion Properties
    }

}