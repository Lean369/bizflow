using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using Entities;

namespace Business
{
    public class TableItem
    {
        private int idField;
        private string nombreField;
        private string documentoField;
        private Entities.Const.EnvelopeState estadoField;
        private string fechaField;
        private string accionField;

        public TableItem(int id, string nombre, string documento, Entities.Const.EnvelopeState estado, string fecha, string accion)
        {
            this.ID = id;
            this.Nombre = nombre;
            this.Documento = documento;
            this.Estado = estado;
            this.Fecha = fecha;
            this.Accion = accion;
        }

        public TableItem() { }

        public static List<TableItem> GetTableItems()
        {
            XmlSerializer xmlSerializer;
            StreamWriter streamWriter;
            StreamReader streamReader;
            string fileName = string.Empty;
            List<TableItem> listOfTableItem = new List<TableItem>();
            try
            {
                if (!Directory.Exists(string.Format(@"{0}TableItems", Entities.Const.appPath)))
                    Directory.CreateDirectory(string.Format(@"{0}TableItems", Entities.Const.appPath));
                fileName = string.Format(@"{0}TableItems\TableItems.xml", Entities.Const.appPath);
                if (!File.Exists(fileName))
                {
                    listOfTableItem.Add(new TableItem(1, "Pedro García", "27.002.323", Entities.Const.EnvelopeState.delivered, "01/09/2019", "A"));
                    listOfTableItem.Add(new TableItem(2, "Marta Mendez", "25.123.983", Entities.Const.EnvelopeState.Empty, "06/08/2019", "A"));
                    listOfTableItem.Add(new TableItem(3, "Karina López", "24.323.443", Entities.Const.EnvelopeState.Loaded, "05/07/2019", "A"));
                    listOfTableItem.Add(new TableItem(4, "Jose García", "27.345.323", Entities.Const.EnvelopeState.Loaded, "01/04/2019", "A"));
                    listOfTableItem.Add(new TableItem(5, "Lia Crucet", "28.123.983", Entities.Const.EnvelopeState.Empty, "06/03/2019", "A"));
                    listOfTableItem.Add(new TableItem(6, "Pedro Aznar", "24.345.443", Entities.Const.EnvelopeState.Empty, "05/02/2019", "A"));
                    listOfTableItem.Add(new TableItem(7, "Charly Garcia", "27.002.875", Entities.Const.EnvelopeState.Empty, "30/09/2019", "A"));
                    listOfTableItem.Add(new TableItem(8, "Marta Sanchez", "25.123.780", Entities.Const.EnvelopeState.Empty, "26/08/2019", "A"));
                    listOfTableItem.Add(new TableItem(9, "Jenifer López", "24.567.443", Entities.Const.EnvelopeState.Empty, "25/07/2019", "A"));
                    listOfTableItem.Add(new TableItem(10, "Leo García", "27.044.323", Entities.Const.EnvelopeState.Empty, "21/09/2019", "A"));
                    listOfTableItem.Add(new TableItem(11, "Cristian Castro", "23.123.973", Entities.Const.EnvelopeState.Empty, "26/08/2019", "A"));
                    listOfTableItem.Add(new TableItem(12, "Ricardo Arjona", "24.323.443", Entities.Const.EnvelopeState.Empty, "15/07/2019", "A"));
                    listOfTableItem.Add(new TableItem(13, "Gustavo Cerati", "27.789.323", Entities.Const.EnvelopeState.Empty, "14/09/2019", "A"));
                    listOfTableItem.Add(new TableItem(14, "Marcos Aguini", "27.166.983", Entities.Const.EnvelopeState.Empty, "06/08/2019", "A"));
                    listOfTableItem.Add(new TableItem(15, "Aurelio Garcia", "22.323.443", Entities.Const.EnvelopeState.Empty, "10/07/2019", "A"));
                    listOfTableItem.Add(new TableItem(16, "Marta Martinez", "21.002.323", Entities.Const.EnvelopeState.Empty, "23/09/2019", "A"));
                    listOfTableItem.Add(new TableItem(17, "Miguel Sousa", "25.245.983", Entities.Const.EnvelopeState.Empty, "06/09/2019", "A"));
                    listOfTableItem.Add(new TableItem(18, "Marcelo Jonas", "24.323.222", Entities.Const.EnvelopeState.Empty, "05/10/2019", "A"));
                    listOfTableItem.Add(new TableItem(19, "Hector Cardozo", "27.002.344", Entities.Const.EnvelopeState.Empty, "01/12/2019", "A"));
                    listOfTableItem.Add(new TableItem(20, "Julio Garcia", "25.555.983", Entities.Const.EnvelopeState.Empty, "06/01/2019", "A"));
                    listOfTableItem.Add(new TableItem(21, "Pedro López", "34.323.443", Entities.Const.EnvelopeState.Empty, "05/08/2019", "A"));
                    listOfTableItem.Add(new TableItem(22, "Carlos Ponce", "24.002.323", Entities.Const.EnvelopeState.Empty, "01/02/2019", "A"));
                    //Serialize object to a text file.
                    streamWriter = new StreamWriter(fileName);
                    xmlSerializer = new XmlSerializer(typeof(List<TableItem>));
                    xmlSerializer.Serialize(streamWriter, listOfTableItem);
                    streamWriter.Close();
                }
                //Deserialize text file to a new object.
                streamReader = new StreamReader(fileName);
                xmlSerializer = new XmlSerializer(typeof(List<TableItem>));
                listOfTableItem = (List<TableItem>)xmlSerializer.Deserialize(streamReader);
                streamReader.Close();
                return listOfTableItem;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
        }

        //////////////////////////////////////////////////////////////////
        ////////////////////////PROPERTIES////////////////////////////////
        //////////////////////////////////////////////////////////////////
        [System.Xml.Serialization.XmlElement("ID")]
        public int ID
        {
            get { return this.idField; }
            set { this.idField = value; }
        }
        [System.Xml.Serialization.XmlElement("Nombre")]
        public string Nombre
        {
            get { return this.nombreField; }
            set { this.nombreField = value; }
        }

        [System.Xml.Serialization.XmlElement("Documento")]
        public string Documento
        {
            get { return this.documentoField; }
            set { this.documentoField = value; }
        }

        [System.Xml.Serialization.XmlElement("Estado")]
        public Entities.Const.EnvelopeState Estado
        {
            get { return this.estadoField; }
            set { this.estadoField = value; }
        }

        [System.Xml.Serialization.XmlElement("Fecha")]
        public string Fecha
        {
            get { return this.fechaField; }
            set { this.fechaField = value; }
        }

        [System.Xml.Serialization.XmlElement("Accion")]
        public string Accion
        {
            get { return this.accionField; }
            set { this.accionField = value; }
        }
    }
}
