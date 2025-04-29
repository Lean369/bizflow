using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.ChoicesSelectorState
{
    [Serializable]
    public partial class PropertiesChoicesSelectorState
    {
        private string nextStateNumberAField;
        private string nextStateNumberBField;
        private string nextStateNumberCField;
        private string nextStateNumberDField;
        private string hardwareErrorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string backNextStateNumberField;
        private string hostNameField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnPleaseWait;
        public StateEvent OnFoundServices;
        public StateEvent OnConnectionFailed;
        public StateEvent OnChoicesSelector;


        public PropertiesChoicesSelectorState()
        {

        }

        public PropertiesChoicesSelectorState(AlephATMAppData alephATMAppData)
        {
            this.nextStateNumberAField = "";
            this.hardwareErrorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.backNextStateNumberField = "";
            this.nextStateNumberBField = "";
            this.nextStateNumberCField = "";
            this.nextStateNumberDField = "";
            this.hostNameField = "RedpagosMulesoftHost";
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "choicesSelector.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnFoundServices = new StateEvent(StateEvent.EventType.runScript, "foundServices", "");
            this.OnConnectionFailed = new StateEvent(StateEvent.EventType.runScript, "connectionFailed", "");
            this.OnChoicesSelector = new StateEvent(StateEvent.EventType.runScript, "choicesSelector", "");
        }

        #region "Properties" 

        [XmlElement()]
        public string NextStateNumberA
        {
            get
            {
                return this.nextStateNumberAField;
            }
            set
            {
                this.nextStateNumberAField = value;
            }
        }
        [XmlElement()]
        public string NextStateNumberB
        {
            get
            {
                return this.nextStateNumberBField;
            }
            set
            {
                this.nextStateNumberBField = value;
            }
        }
        [XmlElement()]
        public string NextStateNumberC
        {
            get
            {
                return this.nextStateNumberCField;
            }
            set
            {
                this.nextStateNumberCField = value;
            }
        }
        [XmlElement()]
        public string NextStateNumberD
        {
            get
            {
                return this.nextStateNumberDField;
            }
            set
            {
                this.nextStateNumberDField = value;
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