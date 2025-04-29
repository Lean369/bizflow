using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Entities
{

    public class CoinHoppersConf
    {
        private CoinHopperMapping CoinHoppersConfig;
        public CoinHopperMapping Hoppers
        {
            get
            {
                CoinHopperMapping.GetMapping(out CoinHoppersConfig);
                return CoinHoppersConfig;
            }
            set
            {
                CoinHopperMapping.SetMapping(value);
            }
        }
    }


    [XmlRoot("CoinHopperList")]
    [Serializable]
    public class CoinHopperMapping
    {
        public CoinHopperMapping()
        {

        }
        public static bool GetMapping(out CoinHopperMapping mapping)
        {
            string fileName = string.Format(@"{0}Config\CoinHopperMapping.xml", Const.appPath);
            mapping = new CoinHopperMapping();
            bool ret;
            try
            {
                if (!File.Exists(fileName))
                {
                    //ya no lo vamos a crear automaticamenta ya que se debe crear desde menu admin para que tambien cree los contadores asociados
                    //mapping.CoinHopperList = new List<CoinHopperUnit>
                    //{
                    //    new CoinHopperUnit(1, "URY", 3, 3139423, 0, -2),
                    //    new CoinHopperUnit(2, "URY", 4, 3139428, 0, -2),
                    //    new CoinHopperUnit(5, "URY", 5, 3139427, 0, -2),
                    //    new CoinHopperUnit(10, "URY", 6, 3139434, 0, -2),
                    //};
                    mapping.MonitorLoopDelayMS = 400; //Bucle de consulta dispensación monedas
                    mapping.TimeoutFailoverWaitMS = 2000; //Tiempo de espera de respuesta de un hopper durante la dispensación
                    mapping.ForceContinueOnTimeout = false; //FALSE: Si falla algún hopper, No continua con la dispensación del resto de los Hoppers
                    mapping.IgnoreEmtpySignal = true; //TRUE: Ignora sensor de supply low de monedas
                }
                mapping = Utilities.Utils.GetGenericXmlData<CoinHopperMapping>(out ret, fileName, mapping);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        public static bool SetMapping(CoinHopperMapping mapping)
        {
            string path = string.Format(@"{0}Config\CoinHopperMapping.xml", Const.appPath);
            string pathBk = string.Format(@"{0}Config\CoinHopperMapping.xml.bk", Const.appPath);

            if (File.Exists(path))
                File.Copy(path, pathBk, true);

            Utilities.Utils.ObjectToXml<CoinHopperMapping>(out bool ret, mapping, path);
            return ret;
        }

        [XmlElement("CoinHopperUnit", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<CoinHopperUnit> CoinHopperList { get; set; }

        public int MonitorLoopDelayMS { get; set; }
        public int TimeoutFailoverWaitMS { get; set; }
        public bool ForceContinueOnTimeout { get; set; }
        public bool IgnoreEmtpySignal { get; set; }

    }

    [Serializable]
    public class CoinHopperUnit
    {
        [XmlAttribute]
        public int Value { get; set; }

        [XmlAttribute]
        public string Currency { get; set; }

        [XmlAttribute]
        public int Address { get; set; }

        [XmlAttribute]
        public int Serial { get; set; }  //es el numero del hopper en la etiqueta gris frontal, no en la lateral azul que dice "serial no"

        [XmlAttribute]
        public int Min_Items_Required { get; set; }

        [XmlAttribute]
        public int Exponent { get; set; }

        public CoinHopperUnit()
        {

        }

        public CoinHopperUnit(int value, string currencyIso, int address, int serial, int minItemsRequired, int exponent)
        {
            Address = address;
            Value = value;
            Currency = currencyIso;
            Serial = serial;
            Min_Items_Required = minItemsRequired;
            Exponent = exponent;
        }
    }
}
