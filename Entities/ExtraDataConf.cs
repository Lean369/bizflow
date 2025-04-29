using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Entities
{
    [DataContract]
    public class ExtraDataConf
    {
        public ExtraDataConf() { }

        /// <summary>
        /// Configuración para un único campo de ExtraData.
        /// </summary>
        /// <param name="_index">Índice u orden de numeración.</param>
        /// <param name="_extraDataType">Tipo de campo. Values: currency | channel | txInfo | txRef | shifts | amountLimit | dynamic | displayOnly </param>
        /// <param name="_enabled">Mostrar u ocultar el campo.</param>
        /// <param name="_name">Nombre programático.</param>
        /// <param name="_label">Label o etiqueta del campo en la UI. Puede mostrarse tal cual o ser un language tag dependiendo el tipo de campo.</param>
        /// <param name="_editable">Si el campo es editable o no (disabled).</param>
        /// <param name="_controlType">Tipo de control. Values: input | select | radio | select-multiple | single-checkbox </param>
        /// <param name="_controlModel">Subtipo de control disponible para algunos campos, como input (text o numeric). </param>
        /// <param name="_minLength">Largo mínimo en caracteres requerido para el valor del campo.</param>
        /// <param name="_maxLength">Largo máximo en caracteres requerido para el valor del campo.</param>
        /// <param name="_options">Para cargar varias opciones a elegir (select, select-multiple) o solo una (displayOnly, single-checkbox) (</param>
        /// <param name="_value">Valor precargado para inputs.</param>
        /// <param name="_required">Si es requerido de completar o no.</param>
        /// <param name="_editableOnPreloadedValue">Si al tener un valor precargado en inputs se permite editar o no.</param>
        public ExtraDataConf(int _index, Enums.ExtraDataType _extraDataType, bool _enabled, string _name, string _label, bool _editable, string _controlType, string _controlModel, int _minLength, int _maxLength, string[] _options, string _value = "", bool _required = true, bool _editableOnPreloadedValue = false)
        {
            this.index = _index;
            this.extraDataType = _extraDataType;
            this.enabled = _enabled;
            this.name = _name;
            this.label = _label;
            this.editable = _editable;
            this.controlType = _controlType;
            this.controlModel = _controlModel;
            this.minLength = _minLength;
            this.maxLength = _maxLength;    
            this.options = _options;
            this.value = _value;
            this.required = _required;
            this.editableOnPreloadedValue = _editableOnPreloadedValue;
        }
        [XmlAttributeAttribute()]
        [DataMember]
        public int index;
        [XmlAttributeAttribute()]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.ExtraDataType extraDataType;
        [XmlAttributeAttribute()]
        [DataMember]
        public bool enabled;
        [XmlAttributeAttribute()]
        [DataMember]
        public string name;
        [XmlAttributeAttribute()]
        [DataMember]
        public string label;
        [XmlAttributeAttribute()]
        [DataMember]
        public bool editable;
        [XmlAttributeAttribute()]
        [DataMember]
        public string controlType;
        [XmlAttributeAttribute()]
        [DataMember]
        public string controlModel;
        [XmlAttributeAttribute()]
        [DataMember]
        public int minLength;
        [XmlAttributeAttribute()]
        [DataMember]
        public int maxLength;
        [XmlArrayItemAttribute("options", typeof(string), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        [DataMember]
        public string[] options;
        [XmlIgnoreAttribute()]
        [DataMember]
        public string value;
        [XmlAttributeAttribute()]
        [DataMember]
        public bool required; 
        [XmlAttributeAttribute()]
        [DataMember]
        public bool editableOnPreloadedValue;
    }
}
