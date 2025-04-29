namespace Entities
{
    public class ExtraData
    {
        private Enums.ExtraDataType ExtraDataTypeField;
        private string TagNameField;
        private string TagValueField;

        public ExtraData() { }
        public ExtraData(Enums.ExtraDataType extraDataType, string tagName, string tagValue)
        {
            this.ExtraDataTypeField = extraDataType;
            this.TagNameField = tagName;
            this.TagValueField = tagValue;
        }

        #region Properties
        public Enums.ExtraDataType ExtraDataType { get => ExtraDataTypeField; set => ExtraDataTypeField = value; }
        public string TagName { get => TagNameField; set => TagNameField = value; }
        public string TagValue { get => TagValueField; set => TagValueField = value; }
        #endregion Properties
    }
}
