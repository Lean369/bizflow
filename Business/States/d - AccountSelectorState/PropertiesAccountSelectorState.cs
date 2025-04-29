
using Newtonsoft.Json.Linq;
using System;
using System.Xml;
using System.Xml.Serialization;
using Business.States;
using System.Collections.Generic;
using Entities;

namespace Business.AccountSelectorState
{

    [Serializable]
    public partial class PropertiesAccountSelectorState
    {
        private string screenNumberField;
        private string timeOutNextStateNumberField;
        private string cancelNextStateNumberField;
        private GetDataOperationMode_Type operationCodeDataField;
        private string nextStateNumberField;
        private string backNextStateNumberField;
        private string aux2Field;
        private object itemField;
        private string HostNameField;
        private bool bypassPaymentConfirm;
        private List<AccountDetail> accountListField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        private Enums.TerminalModel terminalModel;

        [XmlElement()]
        public StateEvent OnAccountSelector;
        [XmlElement()]
        public StateEvent OnPleaseWait;
        [XmlElement()]
        public StateEvent OnShowAccounts;
        

        public PropertiesAccountSelectorState() { }
        public PropertiesAccountSelectorState(AlephATMAppData alephATMAppData)
        {
            this.terminalModel = alephATMAppData.TerminalModel;
            this.screenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.operationCodeDataField = GetDataOperationMode_Type.none;
            this.nextStateNumberField = "";
            this.backNextStateNumberField = "";
            this.aux2Field = "";
            this.itemField = null;
            this.HostNameField = "BvaHost";
            this.accountListField = new List<AccountDetail>();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnAccountSelector = new StateEvent(StateEvent.EventType.navigate, "accountSelector.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnShowAccounts = new StateEvent(StateEvent.EventType.runScript, "ShowAccounts", "");
            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.CTR50:
                    this.HostNameField = "RedpagosMulesoftHost";
                    this.operationCodeDataField = GetDataOperationMode_Type.fromList;
                    this.bypassPaymentConfirm = true;
                    break;
            }
        }

        internal void LoadDefaultConfiguration(Enums.TerminalModel terminalModel)
        {
            switch (terminalModel)
            {
                case Enums.TerminalModel.CTR50:
                case Enums.TerminalModel.Depositario:
                    //"accountListField" will be loaded dynamically
                    break;
                default:
                    this.accountListField.Add(new AccountDetail(1, true, "CHEQUE"));
                    this.accountListField.Add(new AccountDetail(2, true, "SAVING"));
                    this.accountListField.Add(new AccountDetail(3, true, "CREDIT"));
                    break;
            }
        }

        #region Properties
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
        public GetDataOperationMode_Type OperationCodeData
        {
            get
            {
                return this.operationCodeDataField;
            }
            set
            {
                this.operationCodeDataField = value;
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
        public string BackNextStateNumber
        {
            get
            {
                return this.backNextStateNumberField;
            }
            set
            {
                this.backNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string Aux2
        {
            get
            {
                return this.aux2Field;
            }
            set
            {
                this.aux2Field = value;
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
        public List<AccountDetail> accountList
        {
            get
            {
                return this.accountListField;
            }
            set
            {
                this.accountListField = value;
            }
        }

        [XmlElement()]
        public object Item
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
        public bool BypassPaymentConfirm
        {
            get
            {
                return this.bypassPaymentConfirm;
            }
            set
            {
                this.bypassPaymentConfirm = value;
            }
        }
        #endregion Properties
    }
}