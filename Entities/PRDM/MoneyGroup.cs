using System;
using System.Xml;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable]
    public class MoneyGroup
    {
        private string NameField;
        private bool DisabledField;


        public MoneyGroup() { }

        public MoneyGroup(string name, bool disabled)
        {
            this.Name = name;
            this.Disabled = disabled;
        }

        [XmlElement(ElementName = "name")]
        public string Name
        {
            get { return this.NameField; }
            set { this.NameField = value; }
        }

        [XmlElement(ElementName = "disabled")]
        public bool Disabled
        {
            get { return this.DisabledField; }
            set { this.DisabledField = value; }
        }
    }
}
