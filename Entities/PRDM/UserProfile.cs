using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable]
    public class UserProfile
    {
        private string nameField;
        private bool disabledField;
        private string statusField;
        private string statusDescriptionField;
        private List<Profile> profilesField;

        public UserProfile() { }

        public UserProfile(string _name, bool _disabled, List<Profile> profiles)
        {
            this.name = _name;
            this.disabledField = _disabled;
            this.profilesField = profiles;
        }

        [XmlElementAttribute("profile", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Profile> profiles
        {
            get
            {
                return this.profilesField;
            }
            set
            {
                this.profilesField = value;
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
        public bool disabled
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
        public string status { get => statusField; set => statusField = value; }

        [XmlAttributeAttribute()]
        public string statusDescription { get => statusDescriptionField; set => statusDescriptionField = value; }
    }
}
