using System.Runtime.Serialization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Entities
{
    [DataContract]
    [Serializable()]
    [XmlRoot("Counters")]
    public class counters
    {
        private int TSNField;
        private int BATCHField;
        private string COLLECTIONIDField;
        private Contents ContentsField;//Guarda los contadores de valores declarados y validados
        public bool LogicalFullBin = false;
        public int LogicalFullBinThreshold = 0;//Se carga por defecto al iniciar la aplicación
        public int TotalDepositedNotes = 0;
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private static counters _instance;
        private counters() //instance must be retrieved from GetInstanceFromXML()
        { }

        public static counters GetInstanceFromXML()
        {
            if(_instance == null)
                _instance = new counters();
            string path = $"{Const.appPath}Counters\\Counters.xml";
            if (!File.Exists(path))
            {
                Log.Error("Counters.xml file does not exist.");
                return null;
            }
            _instance = Utilities.Utils.GetGenericXmlData<counters>(out bool ret, path, _instance);
            if (!ret)
            {
                Log.Error("Counters.xml could not be deserialized.");
                return null;
            }
            return _instance;
        }

        #region Properties

        [DataMember]
        [XmlElement("TSN")]
        public int TSN
        {
            get { return this.TSNField; }
            set
            {
                this.TSNField = value;
            }
        }

        //Lote
        [DataMember]
        [XmlElement("BATCH")]
        public int BATCH
        {
            get { return this.BATCHField; }
            set
            {
                this.BATCHField = value;
            }
        }

        [DataMember]
        [XmlElement("COLLECTIONID")]
        public string COLLECTIONID
        {
            get
            {
                return COLLECTIONIDField;
            }
            set
            {
                COLLECTIONIDField = value;
            }
        }

        [DataMember]
        [XmlElement("Contents")]
        public Contents Contents
        {
            get { return this.ContentsField; }
            set
            {
                this.ContentsField = value;
            }
        }


        #endregion Properties
    }
}
