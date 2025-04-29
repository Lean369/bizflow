using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.States
{
    [Serializable]
    public class MoreTimeProperties
    {
        private string moreTimeScreenNameField;
        private int maxTimeOutField;
        private int maxTimeOutRetriesField;
        private bool moreTimeKeyboardEnabledField;

        public MoreTimeProperties()
        {
            this.moreTimeScreenNameField = "C00.ndc";
            this.maxTimeOutField = 60;
            this.maxTimeOutRetriesField = 3;
            this.moreTimeKeyboardEnabledField = true;
        }

        [XmlElement()]
        public string MoreTimeScreenName
        {
            get
            {
                return this.moreTimeScreenNameField;
            }
            set
            {
                this.moreTimeScreenNameField = value;
            }
        }

        [XmlElement()]
        public int MaxTimeOut
        {
            get
            {
                return this.maxTimeOutField;
            }
            set
            {
                this.maxTimeOutField = value;
            }
        }

        [XmlElement()]
        public int MaxTimeOutRetries
        {
            get
            {
                return this.maxTimeOutRetriesField;
            }
            set
            {
                this.maxTimeOutRetriesField = value;
            }
        }

        [XmlElement()]
        public bool MoreTimeKeyboardEnabled
        {
            get
            {
                return this.moreTimeKeyboardEnabledField;
            }
            set
            {
                this.moreTimeKeyboardEnabledField = value;
            }
        }
    }
}
