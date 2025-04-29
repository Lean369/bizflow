using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    [Serializable()]
    public class Detail
    {
        public enum ContainerIDType { CashAcceptor, Depository, Hopper_1, Hopper_2, Hopper_3, Hopper_4, Hopper_5, Hopper_6, Hopper_7, Hopper_8, Hopper_9 }

        private string CurrencyField;
        private ContainerIDType ContainerIdField;
        private string ContainerTypeField;
        private string CollectionIdField;
        private List<Item> LstItemsField;

        public Detail() { }

        public Detail(string currency, ContainerIDType containerId, string containerType, string collectionId, List<Item> lstItems)
        {
            this.Currency = currency;
            this.ContainerId = containerId;
            this.ContainerType = containerType;
            this.CollectionId = collectionId;
            this.LstItems = lstItems;
            if (containerId == ContainerIDType.CashAcceptor)
            {
                this.LstItems.ForEach(y => { y.Barcode = null; y.Category = null; });
            }
        }

        #region Propieties

        [XmlIgnore]
        public int IdDetail { get; set; }

        [XmlIgnore]
        public int CountDeposit { get; set; }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "currency")]
        public string Currency
        {
            get { return this.CurrencyField; }
            set { this.CurrencyField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "container_id")]
        public ContainerIDType ContainerId
        {
            get { return this.ContainerIdField; }
            set { this.ContainerIdField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "container_type")]
        public string ContainerType
        {
            get { return this.ContainerTypeField; }
            set { this.ContainerTypeField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "collection_id")]
        public string CollectionId
        {
            get { return this.CollectionIdField; }
            set { this.CollectionIdField = value; }
        }

        [DataMember]
        [XmlElementAttribute("item", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Item> LstItems
        {
            get { return this.LstItemsField; }
            set { this.LstItemsField = value; }
        }
        #endregion
    }
}
