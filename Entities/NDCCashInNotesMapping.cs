using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Collections;
using System.IO;


namespace Entities
{

    public class NDCCashInNotesMapping
    {
        private int XFSNoteIdField;

        private string CurrencyField;

        private int ValueField;

        private int ReleaseField;

        private string NDCNoteIDField;

        public NDCCashInNotesMapping() { }

        public NDCCashInNotesMapping(int xFSNoteId, string currency, int value, int release, string nDCNoteID)
        {
            XFSNoteId = xFSNoteId;
            Currency = currency;
            Value = value;
            Release = release;
            NDCNoteID = nDCNoteID;
        }

        /// <summary>
        /// NO se utiliza. Se reemplaza por el método TypeCassetteMapping.xml
        /// </summary>
        /// <param name="listOfCashIn"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool GetNDCCashInNotesMapping(out List<NDCCashInNotesMapping> listOfCashIn)
        {
            bool ret = false;
            string fileName = string.Format(@"{0}Config\NDCCashInNotesMapping.xml", Entities.Const.appPath);
            listOfCashIn = new List<NDCCashInNotesMapping>();
            try
            {
                if (!File.Exists(fileName))
                {
                    listOfCashIn.Add(new NDCCashInNotesMapping(3, "ARS", 10, 1, "01"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(4, "ARS", 20, 1, "02"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(5, "ARS", 50, 1, "03"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(6, "ARS", 100, 1, "04"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(7, "ARS", 100, 2, "04"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(8, "ARS", 50, 2, "03"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(10, "ARS", 10, 2, "01"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(12, "ARS", 20, 2, "02"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(13, "ARS", 50, 3, "03"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(14, "ARS", 100, 3, "04"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(15, "ARS", 200, 1, "05"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(16, "ARS", 500, 1, "06"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(17, "ARS", 1000, 1, "07"));
                    listOfCashIn.Add(new NDCCashInNotesMapping(49, "UYU", 20, 1, "08"));
                }
                listOfCashIn = Utilities.Utils.GetGenericXmlData<List<NDCCashInNotesMapping>>(out ret, fileName, listOfCashIn);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        //////////////////////////////////////////////////////////////////
        ////////////////////////PROPERTIES////////////////////////////////
        //////////////////////////////////////////////////////////////////

        [System.Xml.Serialization.XmlElement("XFSNoteId")]
        public int XFSNoteId
        {
            get { return this.XFSNoteIdField; }
            set { this.XFSNoteIdField = value; }
        }

        [System.Xml.Serialization.XmlElement("Currency")]
        public string Currency
        {
            get { return this.CurrencyField; }
            set { this.CurrencyField = value; }
        }

        [System.Xml.Serialization.XmlElement("Value")]
        public int Value
        {
            get { return this.ValueField; }
            set { this.ValueField = value; }
        }

        [System.Xml.Serialization.XmlElement("Release")]
        public int Release
        {
            get { return this.ReleaseField; }
            set { this.ReleaseField = value; }
        }

        [System.Xml.Serialization.XmlElement("NDCNoteID")]
        public string NDCNoteID
        {
            get { return this.NDCNoteIDField; }
            set { this.NDCNoteIDField = value; }
        }
    } 
}