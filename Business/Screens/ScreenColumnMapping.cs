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
    public class ScreenColumnMapping
    {
        private ScreenColumn_Type NDCColumnNumberField;
        private string NewPositionInPixelsField;
        //private static string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;

        public ScreenColumnMapping(ScreenColumn_Type _NDCColumnNumber, string _NewPositionInPixels)
        {
            this.NDCColumnNumber = _NDCColumnNumber;
            this.NewPositionInPixels = _NewPositionInPixels;
        }

        public ScreenColumnMapping() { }

        public static bool GetScreenColumnMapping(out List<ScreenColumnMapping> listOfScreenColumnMapping, Entities.Const.Resolution resolution)
        {
            bool ret = false;
            string fileName = string.Format(@"{0}\Config\ScreenColumnMapping.xml", Entities.Const.appPath);
            listOfScreenColumnMapping = new List<ScreenColumnMapping>();
            try
            {
                if (!Directory.Exists(string.Format(@"{0}Config", Entities.Const.appPath)))
                    Directory.CreateDirectory(string.Format(@"{0}\Config", Entities.Const.appPath));
                if (!Directory.Exists(string.Format(@"{0}Config\ScreenRosolutionMapping", Entities.Const.appPath)))
                    Directory.CreateDirectory(string.Format(@"{0}Config\ScreenRosolutionMapping", Entities.Const.appPath));
                fileName = string.Format(@"{0}Config\ScreenRosolutionMapping\ScreenColumnMapping{1}.xml", Entities.Const.appPath, resolution.ToString().Substring(1));


                if (!Directory.Exists(string.Format(@"{0}\Config", Entities.Const.appPath)))
                {
                    Directory.CreateDirectory(string.Format(@"{0}\Config", Entities.Const.appPath));
                }
                if (!File.Exists(fileName))
                {
                    switch (resolution)
                    {
                        case Const.Resolution.R640x480:
                            {
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item0, "0"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item1, "20"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item2, "40"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item3, "60"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item4, "80"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item5, "100"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item6, "120"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item7, "140"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item8, "160"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item9, "180"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item10, "200"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item11, "220"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item12, "240"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item13, "260"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item14, "280"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item15, "300"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item16, "320"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item17, "340"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item18, "360"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item19, "380"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item20, "400"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item21, "420"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item22, "440"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item23, "460"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item24, "480"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item25, "500"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item26, "520"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item27, "540"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item28, "560"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item29, "580"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item30, "600"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item31, "620"));
                                break;
                            }
                        case Const.Resolution.R800x600: //+25%
                            {
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item0, "0"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item1, "25"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item2, "50"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item3, "75"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item4, "100"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item5, "125"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item6, "156"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item7, "175"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item8, "200"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item9, "225"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item10, "250"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item11, "275"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item12, "300"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item13, "338"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item14, "350"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item15, "375"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item16, "400"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item17, "425"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item18, "450"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item19, "475"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item20, "500"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item21, "525"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item22, "550"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item23, "575"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item24, "600"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item25, "625"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item26, "650"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item27, "675"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item28, "700"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item29, "725"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item30, "750"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item31, "775"));
                                break;
                            }
                        case Const.Resolution.R1024x600: //+30%
                            {
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item0, "0"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item1, "26"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item2, "52"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item3, "78"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item4, "104"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item5, "130"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item6, "156"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item7, "182"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item8, "208"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item9, "234"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item10, "260"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item11, "286"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item12, "312"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item13, "338"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item14, "364"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item15, "390"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item16, "416"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item17, "442"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item18, "468"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item19, "494"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item20, "520"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item21, "546"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item22, "572"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item23, "598"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item24, "624"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item25, "650"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item26, "676"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item27, "702"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item28, "728"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item29, "364"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item30, "780"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item31, "806"));
                                break;
                            }
                        case Const.Resolution.R1024x768: //+30%
                            {
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item0, "0"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item1, "26"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item2, "52"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item3, "78"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item4, "104"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item5, "130"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item6, "156"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item7, "182"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item8, "208"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item9, "234"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item10, "260"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item11, "286"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item12, "312"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item13, "338"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item14, "364"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item15, "390"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item16, "416"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item17, "442"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item18, "468"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item19, "494"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item20, "520"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item21, "546"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item22, "572"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item23, "598"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item24, "624"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item25, "650"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item26, "676"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item27, "702"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item28, "728"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item29, "364"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item30, "780"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item31, "806"));
                                break;
                            }
                        case Const.Resolution.R1600x900: //+30%
                            {
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item0, "0"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item1, "26"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item2, "52"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item3, "78"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item4, "104"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item5, "130"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item6, "156"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item7, "182"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item8, "208"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item9, "234"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item10, "260"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item11, "286"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item12, "312"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item13, "338"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item14, "364"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item15, "390"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item16, "416"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item17, "442"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item18, "468"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item19, "494"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item20, "520"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item21, "546"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item22, "572"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item23, "598"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item24, "624"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item25, "650"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item26, "676"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item27, "702"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item28, "728"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item29, "364"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item30, "780"));
                                listOfScreenColumnMapping.Add(new ScreenColumnMapping(ScreenColumn_Type.Item31, "806"));
                                break;
                            }
                    }
                }
                listOfScreenColumnMapping = Utilities.Utils.GetGenericXmlData<List<ScreenColumnMapping>>(out ret, fileName, listOfScreenColumnMapping);
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
        [System.Xml.Serialization.XmlElement("NDCColumnNumber")]
        public ScreenColumn_Type NDCColumnNumber
        {
            get { return this.NDCColumnNumberField; }
            set { this.NDCColumnNumberField = value; }
        }

        [System.Xml.Serialization.XmlElement("NewPositionInPixels")]
        public string NewPositionInPixels
        {
            get { return this.NewPositionInPixelsField; }
            set { this.NewPositionInPixelsField = value; }
        }


    }
}
