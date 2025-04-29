using System;

namespace Entities
{
    [Serializable]
    public class Profile
    {
        private string nameField;

        public Profile()
        {

        }
        public Profile(string _name)
        {
            this.name = _name;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    }
}
