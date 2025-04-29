using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Entities
{
    [XmlRoot("TypeCassetteList")]
    [Serializable]
    public class TypeCassetteMapping
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public TypeCassetteMapping()
        {

        }
        public static bool GetMapping(out TypeCassetteMapping mapping, string defautCurrency)
        {
            string fileName = string.Format(@"{0}Config\TypeCassetteMapping.xml", Entities.Const.appPath);
            mapping = new TypeCassetteMapping();
            bool ret;
            try
            {
                Log.Info($"Getting TypeCassetteMapping configuration from: {defautCurrency}");
                if (!File.Exists(fileName))
                {
                    if (defautCurrency.Equals("UYU"))
                    {
                        mapping.TypeCassetteList = new List<TypeCassetteConf>
                        {
                            new TypeCassetteConf(Const.TypeEnum.Type1, 100, "UYU", 10),
                            new TypeCassetteConf(Const.TypeEnum.Type2, 200, "UYU", 10),
                            new TypeCassetteConf(Const.TypeEnum.Type3, 500, "UYU", 10),
                            new TypeCassetteConf(Const.TypeEnum.Type4, 1000, "UYU", 10),
                        };
                    }
                    else
                    {
                        mapping.TypeCassetteList = new List<TypeCassetteConf>
                        {
                            new TypeCassetteConf(Const.TypeEnum.Type1, 100, "ARS", 10),
                            new TypeCassetteConf(Const.TypeEnum.Type2, 200, "ARS", 10),
                            new TypeCassetteConf(Const.TypeEnum.Type3, 500, "ARS", 10),
                            new TypeCassetteConf(Const.TypeEnum.Type4, 1000, "ARS", 10),
                        };
                    }
                }
                mapping = Utilities.Utils.GetGenericXmlData<TypeCassetteMapping>(out ret, fileName, mapping);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        public static bool SetMapping(TypeCassetteMapping mapping)
        {
            string path = string.Format(@"{0}Config\TypeCassetteMapping.xml", Const.appPath);
            string pathBk = string.Format(@"{0}Config\TypeCassetteMapping.xml.bk", Const.appPath);

            if (File.Exists(path))
                File.Copy(path, pathBk, true);

            Utilities.Utils.ObjectToXml<TypeCassetteMapping>(out bool ret, mapping, path);
            return ret;
        }

        [XmlElement("TypeCassetteUnit", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<TypeCassetteConf> TypeCassetteList { get; set; }

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool UseXFSUnitsMinimumThreshold { get; set; }

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool AllowEmptyStateXFSUnits { get; set; }

    }

    [Serializable]
    public class TypeCassetteConf
    {
        [XmlAttribute]
        public Const.TypeEnum Type { get; set; }

        [XmlAttribute]
        public int Denomination { get; set; }

        [XmlAttribute]
        public string CurrencyIso { get; set; }

        [XmlAttribute]
        public int MinItemsRequired { get; set; }

        public TypeCassetteConf()
        {

        }

        public TypeCassetteConf(Const.TypeEnum type, int denomination, string currencyIso, int minItemsRequired)
        {
            Type = type;
            Denomination = denomination;
            CurrencyIso = currencyIso;
            MinItemsRequired = minItemsRequired;
        }
    }
}
