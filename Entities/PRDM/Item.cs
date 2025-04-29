using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    [Serializable]
    public class Item
    {

        private decimal DenominationField;
        private decimal Num_ItemsField;
        private decimal TotalField;
        private string TypeField;
        private string BarcodeField;
        private string CategoryField;
        private int ExponentField;

        public Item() { }
        public Item(decimal denomination, decimal num_items, decimal total, string type, string barcode, string category)
        {

            this.DenominationField = denomination;
            this.Num_ItemsField = num_items;
            this.TotalField = total;
            this.TypeField = type;
            this.BarcodeField = barcode;
            this.CategoryField = category;
            this.ExponentField = -2;
        }

        #region Propieties
        [XmlIgnore]
        public int IdItem { get; set; }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "denomination")]
        public decimal Denomination
        {
            get { return this.DenominationField; }
            set { this.DenominationField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "num_items")]
        public decimal Num_Items
        {
            get { return this.Num_ItemsField; }
            set { this.Num_ItemsField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "total")]
        public decimal Total
        {
            get { return this.TotalField; }
            set { this.TotalField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "type")]
        public string Type
        {
            get { return this.TypeField; }
            set { this.TypeField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute(attributeName: "exponent")]
        public int Exponent
        {
            get { return this.ExponentField; }
            set { this.ExponentField = value; }
        }

        [DataMember]
        [XmlAttributeAttribute()]
        public string Barcode { get => BarcodeField; set => BarcodeField = value; }

        [DataMember]
        [XmlAttributeAttribute()]
        public string Category { get => CategoryField; set => CategoryField = value; }

        #endregion

    }
}
