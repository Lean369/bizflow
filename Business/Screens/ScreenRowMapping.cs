using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Collections;
using System.IO;
//using System.Xml.Serialization;
using Entities;

namespace Business
{
    public class ScreenRowMapping
    {
        private ScreenRow_Type NDCRowNumberField;
        private string NewPositionInPixelsField;

        public ScreenRowMapping(ScreenRow_Type _NDCRowNumber, string _NewPositionInPixels)
        {
            this.NDCRowNumber = _NDCRowNumber;
            this.NewPositionInPixels = _NewPositionInPixels;
        }

        public ScreenRowMapping() { }

        public static bool GetScreenRowMapping(out List<ScreenRowMapping> listOfMappedRow, Entities.Const.Resolution resolution)
        {
            bool ret = false;
            string fileName = string.Empty;
            listOfMappedRow = new List<ScreenRowMapping>();
            try
            {
                if (!Directory.Exists(string.Format(@"{0}Config", Entities.Const.appPath)))
                    Directory.CreateDirectory(string.Format(@"{0}\Config", Entities.Const.appPath));
                if (!Directory.Exists(string.Format(@"{0}Config\ScreenRosolutionMapping", Entities.Const.appPath)))
                    Directory.CreateDirectory(string.Format(@"{0}Config\ScreenRosolutionMapping", Entities.Const.appPath));
                fileName = string.Format(@"{0}Config\ScreenRosolutionMapping\ScreenRowMapping{1}.xml", Entities.Const.appPath, resolution.ToString().Substring(1));
                if (!File.Exists(fileName))
                {
                    switch (resolution)
                    {
                        case Const.Resolution.R640x480:
                            {
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item0, "0"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item1, "30"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item2, "60"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item3, "150"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item4, "175"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item5, "212"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item6, "236"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item7, "260"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item8, "284"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item9, "308"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item10, "332"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item11, "356"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item12, "380"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item13, "404"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item14, "428"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item15, "452"));
                                break;
                            }
                        case Const.Resolution.R800x600: //+25%
                            {
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item0, "0"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item1, "117"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item2, "155"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item3, "192"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item4, "230"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item5, "265"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item6, "295"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item7, "325"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item8, "355"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item9, "385"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item10, "415"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item11, "445"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item12, "475"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item13, "505"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item14, "535"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item15, "565"));
                                break;
                            }
                        case Const.Resolution.R1024x600: //+25%
                            {
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item0, "0"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item1, "117"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item2, "155"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item3, "192"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item4, "230"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item5, "265"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item6, "295"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item7, "325"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item8, "355"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item9, "385"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item10, "415"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item11, "445"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item12, "475"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item13, "505"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item14, "535"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item15, "565"));
                                break;
                            }
                        case Const.Resolution.R1024x768://+30%
                            {
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item0, "0"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item1, "150"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item2, "198"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item3, "246"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item4, "294"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item5, "339"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item6, "377"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item7, "416"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item8, "454"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item9, "492"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item10, "531"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item11, "569"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item12, "608"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item13, "646"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item14, "684"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item15, "723"));
                                break;
                            }
                        case Const.Resolution.R1600x900://+30%
                            {
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item0, "0"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item1, "150"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item2, "198"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item3, "246"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item4, "294"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item5, "339"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item6, "377"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item7, "416"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item8, "454"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item9, "492"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item10, "531"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item11, "569"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item12, "608"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item13, "646"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item14, "684"));
                                listOfMappedRow.Add(new ScreenRowMapping(ScreenRow_Type.Item15, "723"));
                                break;
                            }
                    }
                }
                listOfMappedRow = Utilities.Utils.GetGenericXmlData<List<ScreenRowMapping>>(out ret, fileName, listOfMappedRow);
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
        [System.Xml.Serialization.XmlElement("NDCRowNumber")]
        public ScreenRow_Type NDCRowNumber
        {
            get { return this.NDCRowNumberField; }
            set { this.NDCRowNumberField = value; }
        }

        [System.Xml.Serialization.XmlElement("NewPositionInPixels")]
        public string NewPositionInPixels
        {
            get { return this.NewPositionInPixelsField; }
            set { this.NewPositionInPixelsField = value; }
        }


    }
}
