using Business.States;
using Entities;
using System;
using System.Xml.Serialization;

namespace Business.SupervisorState
{
    [Serializable]
    public partial class PropertiesSupervisorState
    {
        private JournalProperties journalField;
        private bool ResetCimAtExitField;
        private bool ResetCdmAtExitField;
        private bool ResetPtrAtExitField;
        public StateEvent OnSupervisorStart;
        internal StateEvent OnPleaseWait;

        public PropertiesSupervisorState()
        {
            this.journalField = new JournalProperties();
            this.ResetCimAtExitField = true;
            this.OnSupervisorStart = new StateEvent(StateEvent.EventType.navigate, "C03.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
        }

        [XmlElement]
        public bool ResetCimAtExit
        {
            get
            {
                return this.ResetCimAtExitField;
            }
            set
            {
                this.ResetCimAtExitField = value;
            }
        }

        [XmlElement]
        public bool ResetCdmAtExit
        {
            get
            {
                return this.ResetCdmAtExitField;
            }
            set
            {
                this.ResetCdmAtExitField = value;
            }
        }

        [XmlElement]
        public bool ResetPtrAtExit
        {
            get
            {
                return this.ResetPtrAtExitField;
            }
            set
            {
                this.ResetPtrAtExitField = value;
            }
        }

        [XmlElement]
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
    }
}