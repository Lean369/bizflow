using System;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable]
    public class UserPrdm
    {
        private string idField;
        private string nameField;
        private string passwordField;
        private string disabledField;
        private string user_profile_nameField;
        private string statusField;
        private string statusDescriptionField;

        public UserPrdm()
        {

        }

        public UserPrdm(string id, string name, string password, string disabled, string user_profile_name)
        {
            this.idField = id;
            this.nameField = name;
            this.passwordField = password;
            this.disabledField = disabled;
            this.user_profile_nameField = user_profile_name;
        }

        #region Properties
        [XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [XmlAttributeAttribute()]
        public string password
        {
            get
            {
                return this.passwordField;
            }
            set
            {
                this.passwordField = value;
            }
        }

        [XmlAttributeAttribute()]
        public string disabled
        {
            get
            {
                return this.disabledField;
            }
            set
            {
                this.disabledField = value;
            }
        }

        [XmlAttributeAttribute()]
        public string user_profile_name
        {
            get
            {
                return this.user_profile_nameField;
            }
            set
            {
                this.user_profile_nameField = value;
            }
        }
        [XmlAttributeAttribute()]
        public string status { get => statusField; set => statusField = value; }

        [XmlAttributeAttribute()]
        public string statusDescription { get => statusDescriptionField; set => statusDescriptionField = value; }

        [XmlIgnore()]
        public bool isLocal { get; set; } = false;
        #endregion Properties
    }
}
