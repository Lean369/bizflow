using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.States
{
    [Serializable]
    public class JournalProperties
    {
        private bool enableJournalField;
        private string inputCancelFileNameField;
        private string inputTimeoutFileNameField;

        public JournalProperties() { this.EnableJournal = true; }

        [XmlElement()]
        public bool EnableJournal
        {
            get
            {
                return this.enableJournalField;
            }
            set
            {
                this.enableJournalField = value;
            }
        }

        [XmlElement()]
        public string InputCancelFileName
        {
            get
            {
                return this.inputCancelFileNameField;
            }
            set
            {
                this.inputCancelFileNameField = value;
            }
        }

        [XmlElement()]
        public string InputTimeoutFileName
        {
            get
            {
                return this.inputTimeoutFileNameField;
            }
            set
            {
                this.inputTimeoutFileNameField = value;
            }
        }
    }

}
