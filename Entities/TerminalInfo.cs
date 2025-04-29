using System;
using System.IO;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable()]
    public class TerminalInfo
    {
        private string logicalUnitNumberField = "009999009973";
        private string addressField = "Av. Amancio Alcorta 2565 ";
        private string cityField = "CABA";
        private string phoneField = "+54 011 2206 6200";
        private string macFlagsField = "000000000000100";
        private string subagenciaField = "901";
        private string cajaField = "1";

        public static bool GetTerminalInfo(out TerminalInfo terminalInfo)
        {
            string fileName = $@"{Const.appPath}Config\\TerminalInfo.xml";
            bool ret = false;
            try
            {
                if (!Directory.Exists($"{Const.appPath}\\Config"))
                {
                    Directory.CreateDirectory($"{Const.appPath}\\Config");
                }
                terminalInfo = new TerminalInfo();
                terminalInfo = Utilities.Utils.GetGenericXmlData<TerminalInfo>(out ret, fileName, terminalInfo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in {fileName} file. {ex.Message} - {ex.InnerException}");
            }
            return ret;
        }

        #region "Properties"
        [XmlElement("LogicalUnitNumber")]
        public string LogicalUnitNumber
        {
            get { return this.logicalUnitNumberField; }
            set { this.logicalUnitNumberField = value; }
        }

        [XmlElement("Address")]
        public string Address
        {
            get { return this.addressField; }
            set { this.addressField = value; }
        }

        [XmlElement("City")]
        public string City
        {
            get { return this.cityField; }
            set { this.cityField = value; }
        }

        [XmlElement("Phone")]
        public string Phone
        {
            get { return this.phoneField; }
            set { this.phoneField = value; }
        }

        [XmlElement("MacFlags")]
        public string MacFlags
        {
            get { return this.macFlagsField; }
            set { this.macFlagsField = value; }
        }

        [XmlElement("Subagencia")]
        public string Subagencia
        {
            get { return this.subagenciaField; }
            set { this.subagenciaField = value; }
        }

        [XmlElement("Caja")]
        public string Caja
        {
            get { return this.cajaField; }
            set { this.cajaField = value; }
        }

        #endregion "Properties"

    }
}
