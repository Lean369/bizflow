using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    [Serializable()]
    public class Contents
    {
        private List<Detail> LstDetailField;

        public Contents() { }

        public Contents(List<Detail> lstDetail)
        {
            this.LstDetail = lstDetail;
        }

        [DataMember]
        [XmlElementAttribute("detail", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Detail> LstDetail
        {
            get { return this.LstDetailField; }
            set { this.LstDetailField = value; }
        }
    }
}
