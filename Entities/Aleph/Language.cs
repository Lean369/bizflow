using System;
using System.Xml.Serialization;

namespace Entities
{
    // Enum for language names
    public enum LanguageName
    {
        Spanish,
        English,
        Portuguese
    }

    // Class for each language entry
    public class LanguageItem
    {
        [XmlAttribute("name")]
        public LanguageName Name { get; set; }

        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }
    }
}
    