using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Entities
{

    [DataContract]
    public class AccountDetail
    {
        private int IdAccountField;
        private bool EnabledField;
        private string TypeField;

        private bool selectableField;
        private string detailsField;
        private bool dynamicField;

        public AccountDetail() { }

        public AccountDetail(int id, bool enabled, string type)
        {
            this.IdAccountField = id;
            this.EnabledField = enabled;
            this.TypeField = type;
        }

        [XmlAttributeAttribute(attributeName: "Id")]
        [DataMember]
        public int id
        {
            get { return this.IdAccountField; }
            set { this.IdAccountField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Enabled")]
        [DataMember]
        public bool enabled
        {
            get { return this.EnabledField; }
            set { this.EnabledField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Type")]
        [DataMember]
        public string type
        {
            get { return this.TypeField; }
            set { this.TypeField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Selectable")]
        [DataMember]
        public bool selectable
        {
            get { return this.selectableField; }
            set { this.selectableField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Details")]
        [DataMember]
        public string details
        {
            get { return this.detailsField; }
            set { this.detailsField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Dynamic")]
        [DataMember]
        public bool dynamic
        {
            get { return this.dynamicField; }
            set { this.dynamicField = value; }
        }

    }
}
