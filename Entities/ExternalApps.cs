using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Entities
{
    [XmlRoot("ExternalApps")]
    [Serializable]
    public class ExternalApps
    {
        public ExternalApps()
        {

        }
        public static bool GetMapping(out ExternalApps mapping, Enums.Branding branding = Enums.Branding.RedPagosA)
        {
            string fileName = string.Format(@"{0}Config\ExternalApps.xml", Entities.Const.appPath);
            mapping = new ExternalApps();
            bool ret;
            try
            {
                if (!File.Exists(fileName))
                {
                    if (branding == Enums.Branding.RedPagosA)
                    {
                        mapping.List = new List<ExternalApp>
                        {
                            new ExternalApp("LOG Baretail", "C:\\Tools\\Baretail\\baretail.exe", ""),
                            new ExternalApp("Cash Config Tool", "D:\\SNBC\\SnbcTCR\\CashConfigTool\\CashConfigTool.exe", ""),
                            new ExternalApp("CCtalk Coin Config Tool", "C:\\Program Files (x86)\\Asahi Seiko (Europe) Ltd\\cctalk Configuration Tool\\cctalk Config.exe", ""),
                            new ExternalApp("Resolucion 1024x1280", "C:\\Tools\\QResolution\\QRes1024x1280.bat", ""),
                            new ExternalApp("Resolucion 768x1366", "C:\\Tools\\QResolution\\QRes768x1366.bat", ""),
                            new ExternalApp("SendCommandDirect", "C:\\Program Files (x86)\\AlephATMSecurity\\SendCommandDirect\\SendCommandDirect.exe", "EnableAdmin"),
                            new ExternalApp("Start AlephLogViewer", "C:\\Program Files (x86)\\AlephATMSecurity\\AlephLogViewer\\AlephLogViewer.exe", "", Enums.ProcessAction.StartProcess)
                        };
                    }
                    else
                    {
                        mapping.List = new List<ExternalApp>
                        {
                            new ExternalApp("Enable Admin", "C:\\Program Files (x86)\\AlephATMSecurity\\SendCommandDirect\\SendCommandDirect.exe", "EnableAdmin", Enums.ProcessAction.StartProcess),
                            new ExternalApp("Enable Pendrive", "C:\\Program Files (x86)\\AlephATMSecurity\\SendCommandDirect\\SendCommandDirect.exe", "EnableUSB", Enums.ProcessAction.StartProcess),
                            new ExternalApp("Start HeidiSQL", "C:\\Program Files\\HeidiSQL\\heidisql.exe", "", Enums.ProcessAction.StartProcess),
                            new ExternalApp("Stop prmWRAPPER", "wrapper", "", Enums.ProcessAction.KillProcess),
                            new ExternalApp("Start StartBneHostDemo", "C:\\Program Files (x86)\\AlephATMSecurity\\SendCommandDirect\\SendCommandDirect.exe", "BneHostDemo", Enums.ProcessAction.StartProcess),
                            new ExternalApp("Start Xloader", "C:\\Program Files (x86)\\AlephATMSecurity\\SendCommandDirect\\SendCommandDirect.exe", "XLoader", Enums.ProcessAction.StartProcess),
                            new ExternalApp("Start AlephLogViewer", "C:\\Program Files (x86)\\AlephATMSecurity\\AlephLogViewer\\AlephLogViewer.exe", "", Enums.ProcessAction.StartProcess)
                        };
                    }

                }
                mapping = Utilities.Utils.GetGenericXmlData<ExternalApps>(out ret, fileName, mapping);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        [XmlElement("ExternalAppUnit", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<ExternalApp> List { get; set; }

    }

    [Serializable]
    public class ExternalApp
    {
        [XmlIgnore]
        public int Index { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Path")]
        public string Path { get; set; }

        [XmlElement("Arguments")]
        public string Arguments { get; set; }

        [XmlElement("ProcessAction")]
        public Enums.ProcessAction ProcessAction { get; set; }

        public ExternalApp()
        {

        }

        public ExternalApp(string description, string path, string argument, Enums.ProcessAction processAction = Enums.ProcessAction.StartProcess)
        {
            Description = description;
            Path = path;
            Arguments = argument;
            ProcessAction = processAction;
        }
    }
}
