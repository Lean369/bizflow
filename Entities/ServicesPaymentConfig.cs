using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Entities
{
    [XmlRoot("ServicesPaymentConfig")]
    [Serializable]
    public class ServicesPaymentConfig
    {
        public ServicesPaymentConfig()
        {

        }

        public static bool GetMapping(out ServicesPaymentConfig serviceConf)
        {
            string fileName = string.Format(@"{0}Config\ServicesPaymentConfig.xml", Const.appPath);
            serviceConf = new ServicesPaymentConfig();
            bool ret;
            try
            {
                if (!File.Exists(fileName))
                {
                    serviceConf.Currencies = new List<CurrencyConf>
                    {
                        new CurrencyConf
                        {
                            Id = 1,
                            Symbol = "$",
                            Value = "UYU",
                        }
                    };
                    serviceConf.ConnectionTimeout = 60000;
                    serviceConf.ReceiveTimeout = 60000;
                    serviceConf.ScheduleBalance = "00:00:01";
                    serviceConf.ExecuteBalanceByPeriodicity = false;
                }
                serviceConf = Utilities.Utils.GetGenericXmlData<ServicesPaymentConfig>(out ret, fileName, serviceConf);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        [XmlElement("CurrencyUnit")]
        public List<CurrencyConf> Currencies { get; set; }

        [XmlElement("ConnectionTimeoutMS")]
        public int ConnectionTimeout { get; set; }

        [XmlElement("ReceiveTimeoutMS")]
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// Momento del día en el que se realizará la ejecución (formato hh:mm:ss). 
        /// Cuando ExecuteBalanceByPeriodicity está establecido en true, este valor representa el intervalo de ejecución.
        /// Ejemplo: "00:00:01" para ejecutar cada segundo.
        /// </summary>
        [XmlElement("ScheduleBalance")]
        public string ScheduleBalance { get; set; }

        /// <summary>
        /// Cuando esta propiedad está establecida en true, ScheduleBalance representa el intervalo de ejecución.
        /// De lo contrario, ScheduleBalance representa la hora del día en la que se realizará la ejecución.
        /// </summary>
        [XmlElement("ExecBalanceByPeriod")]
        public bool ExecuteBalanceByPeriodicity { get; set; }
    }


    [Serializable]
    public class CurrencyConf
    {
        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public string Symbol { get; set; }

        [XmlAttribute]
        public int Id { get; set; }

        public CurrencyConf()
        {

        }

        public CurrencyConf(string value, string symbol)
        {
            Value = value;
            Symbol = symbol;
        }
    }
}
