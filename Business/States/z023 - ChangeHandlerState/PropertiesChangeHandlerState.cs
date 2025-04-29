using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.ChangeHandlerState
{
    [Serializable]
    public partial class PropertiesChangeHandlerState
    {
        private string screenNumberField;
        private string nextStateNumberField;
        private string hardwareErrorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string item1Field;
        private string item2Field;
        private string hostNameField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnPleaseWait;

        public PropertiesChangeHandlerState()
        {
            this.screenNumberField = "";
            this.nextStateNumberField = "";
            this.hardwareErrorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.item1Field = "";
            this.item2Field = "";
            this.hostNameField = "RedpagosMulesoftHost";
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "changeHandler.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
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
        public string HostName
        {
            get => hostNameField;
            set => hostNameField = value;
        }

        #endregion Properties
    }
}