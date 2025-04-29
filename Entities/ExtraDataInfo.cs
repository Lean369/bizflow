using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class ExtraDataInfo
    {
        public ExtraDataInfo(Enums.ExtraDataType _extraDataType, string _tagName, string _tagValue)
        {
            this.extraDataType = _extraDataType;
            this.tagName = _tagName;
            this.tagValue = _tagValue;
        }

        public ExtraDataInfo() { }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.ExtraDataType extraDataType;

        [DataMember]
        public string tagName;

        [DataMember]
        public string tagValue;

    }
}