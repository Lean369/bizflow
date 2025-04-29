using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.FDKSwitchState
{
    [Serializable]
    public partial class PropertiesFDKSwitchState
    {
        private string fDKANextStateNumberField;
        private string fDKBNextStateNumberField;
        private string fDKCNextStateNumberField;
        private string fDKDNextStateNumberField;
        private string fDKFNextStateNumberField;
        private string fDKGNextStateNumberField;
        private string fDKHNextStateNumberField;
        private string fDKINextStateNumberField;
        private JournalProperties JournalField;

        public PropertiesFDKSwitchState()
        {
            this.fDKANextStateNumberField = "";
            this.fDKBNextStateNumberField = "";
            this.fDKCNextStateNumberField = "";
            this.fDKDNextStateNumberField = "";
            this.fDKFNextStateNumberField = "";
            this.fDKGNextStateNumberField = "";
            this.fDKHNextStateNumberField = "";
            this.fDKINextStateNumberField = "";
            this.JournalField = new JournalProperties();
        }

        #region "Properties"
        [XmlElement()]
        public string FDKANextStateNumber
        {
            get
            {
                return this.fDKANextStateNumberField;
            }
            set
            {
                this.fDKANextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKBNextStateNumber
        {
            get
            {
                return this.fDKBNextStateNumberField;
            }
            set
            {
                this.fDKBNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKCNextStateNumber
        {
            get
            {
                return this.fDKCNextStateNumberField;
            }
            set
            {
                this.fDKCNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKDNextStateNumber
        {
            get
            {
                return this.fDKDNextStateNumberField;
            }
            set
            {
                this.fDKDNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKFNextStateNumber
        {
            get
            {
                return this.fDKFNextStateNumberField;
            }
            set
            {
                this.fDKFNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKGNextStateNumber
        {
            get
            {
                return this.fDKGNextStateNumberField;
            }
            set
            {
                this.fDKGNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKHNextStateNumber
        {
            get
            {
                return this.fDKHNextStateNumberField;
            }
            set
            {
                this.fDKHNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKINextStateNumber
        {
            get
            {
                return this.fDKINextStateNumberField;
            }
            set
            {
                this.fDKINextStateNumberField = value;
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
        #endregion "Properties"
    }
}