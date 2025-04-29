using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.VerifyNotesState
{
    [Serializable]
    public partial class PropertiesVerifyNotesState
    {
        private string screenNumberField;
        private string nextStateNumberField;
        private string hardwareErrorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string item1Field;
        private string item2Field;
        private bool UpdateConfigurationFileField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnPleaseWait;
        public StateEvent OnDepositCashPrepare;
        public StateEvent OnCashInsertedAdvice;
        public StateEvent OnConfirmDepositedCash;

        public PropertiesVerifyNotesState()
        {
            this.screenNumberField = "";
            this.nextStateNumberField = "";
            this.hardwareErrorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.item1Field = "";
            this.item2Field = "";
            this.UpdateConfigurationFileField = true;
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "verifyNotes.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnDepositCashPrepare = new StateEvent(StateEvent.EventType.runScript, "DepositCashPrepare", "");
            this.OnCashInsertedAdvice = new StateEvent(StateEvent.EventType.runScript, "CashInsertedAdvice", "");
            this.OnConfirmDepositedCash = new StateEvent(StateEvent.EventType.runScript, "ConfirmDepositedCash", "");
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
        #endregion Properties
    }
}