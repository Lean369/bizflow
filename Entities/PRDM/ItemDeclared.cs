namespace Entities
{
    public class ItemDeclared
    {
        private string DenominationField;
        private string Num_ItemsField;
        private string TotalField;
        private string TypeItemsField;
        private string BarcodeField;
        private string CategoryField;

        #region Constructors
        public ItemDeclared(string denomination, string num_items, string total, string typeitems, string barcode, string category)
        {

            this.Denomination = denomination;
            this.Num_Items = num_items;
            this.Total = total;
            this.TypeItems = typeitems;
            this.Barcode = barcode;
            this.Category = category;
        }
        #endregion

        #region Propieties

        public string Denomination
        {
            get { return this.DenominationField; }
            set { this.DenominationField = value; }
        }
        public string Num_Items
        {
            get { return this.Num_ItemsField; }
            set { this.Num_ItemsField = value; }
        }
        public string Total
        {
            get { return this.TotalField; }
            set { this.TotalField = value; }
        }
        public string TypeItems
        {
            get { return this.TypeItemsField; }
            set { this.TypeItemsField = value; }
        }
        public string Barcode
        {
            get { return this.BarcodeField; }
            set { this.BarcodeField = value; }
        }
        public string Category
        {
            get { return this.CategoryField; }
            set { this.CategoryField = value; }
        }
        #endregion

    }
}