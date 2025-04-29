using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Entities
{
    public class CashInAcceptedNotes
    {
        [XmlIgnore]
        private bool configuredField;
        [XmlIgnore]
        private long valuesField;
        //[XmlIgnore]
        //private int releaseField;
        //[XmlIgnore]
        //private int noteIdField;
        [XmlIgnore]
        private string curIdField;

        public CashInAcceptedNotes(bool _configured, long _values, string _curId)
        {
            this.Configured = _configured;
            this.Values = _values;
            //this.Release = _release;
            //this.NoteId = _noteId;
            this.CurId = _curId;
        }

        public CashInAcceptedNotes() { }

        public static bool GetCashInAcceptedNotes(out List<CashInAcceptedNotes> listOfCashInAcceptedNotes)
        {
            bool ret = false;
            string fileName = string.Empty;
            listOfCashInAcceptedNotes = new List<CashInAcceptedNotes>();
            try
            {
                if (!Directory.Exists($"{Const.appPath}Config"))
                    Directory.CreateDirectory($"{Const.appPath}Config");
                fileName = $"{Const.appPath}Config\\CashInAcceptedNotes.xml";
                if (!File.Exists(fileName))
                {
                    listOfCashInAcceptedNotes = new List<CashInAcceptedNotes>();
                    //switch (terminalModel)
                    //{
                    //    case Enums.TerminalModel.MiniBank_JH600_D:
                    //        listOfCashInAcceptedNotes = new List<CashInAcceptedNotes>();
                    //        break;
                    //    default:
                    //        listOfCashInAcceptedNotes = new List<CashInAcceptedNotes>();
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 1, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 2, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 5, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 10, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 20, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 50, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 100, "USD"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 5, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 10, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 20, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 50, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 100, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 200, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 500, "EUR"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 5, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 10, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 20, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 50, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 100, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 200, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 500, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(true, 1000, "ARS"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 2, "BRL"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 5, "BRL"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 10, "BRL"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 20, "BRL"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 50, "BRL"));
                    //        listOfCashInAcceptedNotes.Add(new CashInAcceptedNotes(false, 100, "BRL"));
                    //        listOfCashInAcceptedNotes = Utilities.Utils.GetGenericXmlData<List<CashInAcceptedNotes>>(out ret, fileName, listOfCashInAcceptedNotes);
                    //        break;
                    //}
                }
                else
                    listOfCashInAcceptedNotes = Utilities.Utils.GetGenericXmlData<List<CashInAcceptedNotes>>(out ret, fileName, listOfCashInAcceptedNotes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in {fileName} file. {ex.InnerException}");
            }
            return ret;
        }

        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();
            if(this.CurId != null)
            {
                sb.Append(" " + this.Values.ToString().PadRight(6, ' '));
                sb.Append(" " + this.CurId.PadRight(6, ' '));
                sb.Append(" " + this.Configured.ToString().PadRight(4, ' '));
            }
            return sb.ToString();
        }

        #region "Properties"
        //////////////////////////////////////////////////////////////////
        ////////////////////////PROPERTIES////////////////////////////////
        //////////////////////////////////////////////////////////////////
        [XmlAttributeAttribute(attributeName: "Configured")]
        public bool Configured
        {
            get { return this.configuredField; }
            set { this.configuredField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Value")]
        public long Values
        {
            get { return this.valuesField; }
            set { this.valuesField = value; }
        }

        //[XmlAttributeAttribute(attributeName: "Release")]
        //public int Release
        //{
        //    get { return this.releaseField; }
        //    set { this.releaseField = value; }
        //}

        //[XmlAttributeAttribute(attributeName: "NoteId")]
        //public int NoteId
        //{
        //    get { return this.noteIdField; }
        //    set { this.noteIdField = value; }
        //}

        [XmlAttributeAttribute(attributeName: "Currency")]
        public string CurId
        {
            get { return this.curIdField; }
            set { this.curIdField = value; }
        }
        #endregion "Properties"
    }
}
