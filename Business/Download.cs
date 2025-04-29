using Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Utilities;

namespace Business
{
    [XmlInclude(typeof(StateTable_Type))]
    [XmlInclude(typeof(ScreenData_Type))]
    public class Download
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public List<StateTable_Type> StateTables;
        public Dictionary<string, ScreenData_Type> ScreenData;
        public SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreenR00;
        public SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreenR01;
        public Dictionary<string, StateTransition> DicOfStatesTransitions = new Dictionary<string, StateTransition>();
        public EnhancedConfigurationParametersData_Type EnhancedConfigurationParametersData;

        /// <summary>
        /// Obtine los datos de pantallas desde un archivo XML y los guarda en una colección tipo Diccionario.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ScreenData_Type> GetScreenData()
        {
            string fileName = string.Empty;
            string screenName = string.Empty;
            string path = $"{Entities.Const.appPath}Screens\\Screens";
            Dictionary<string, ScreenData_Type> dicScreenData = new Dictionary<string, ScreenData_Type>();
            ScreenData_Type screenData;
            List<string> pathFiles = new List<string>();
            try
            {
                if (Directory.Exists(path))
                {
                    String[] dirs = System.IO.Directory.GetDirectories(path);
                    foreach (String dir in dirs)
                    {
                        string[] folders = dir.Split('\\');
                        //if (folders[folders.Length - 1].Length < 4) //Solo procesa si la ultima carpeta tiene un largo menor a 3.
                        //{
                        pathFiles = Directory.GetFiles(dir).ToList();
                        foreach (String pathFile in pathFiles)
                        {
                            screenName = pathFile.Substring(pathFile.Length - 7, 3);
                            //if (screenName.Equals("796"))
                            //{

                            //}
                            if (!screenName.Equals("R00") && !screenName.Equals("R01"))
                            {
                                if (DeserializeScreen(pathFile, out screenData))//Si la deserialización es exitosa, inserto el screen
                                {
                                    if (dicScreenData.ContainsKey(screenName))
                                        dicScreenData.Remove(screenName); //Si ya existe el screen, lo elimino para almacenar lo ultimo que se envió
                                    dicScreenData.Add(screenName, screenData);
                                }
                                else
                                {
                                    //Error de serialización
                                }
                            }
                        }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dicScreenData;
        }

        /// <summary>
        /// Obtine los datos de pantallas desde un archivo XML y los guarda en una colección tipo Diccionario.
        /// </summary>
        /// <returns></returns>
        public static SimulatedPrePrintedReceiptScreen_Type GetSimulatedPrePrintedReceiptScreenData(string screen)
        {
            string fileName = string.Empty;
            string screenName = string.Empty;
            string path = string.Format(@"{0}Screens\Screens", Entities.Const.appPath);
            SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreen = null;
            List<string> pathFiles = new List<string>();
            XmlSerializer xmlSerializer;
            StreamReader streamReader = null;
            try
            {
                if (Directory.Exists(path))
                {
                    String[] dirs = System.IO.Directory.GetDirectories(path);
                    foreach (String dir in dirs)
                    {
                        string[] folders = dir.Split('\\');
                        if (folders[folders.Length - 1].Length < 4) //Solo procesa si la ultima carpeta tiene un largo menor a 3.
                        {
                            pathFiles = Directory.GetFiles(dir).ToList();
                            foreach (String pathFile in pathFiles)
                            {
                                screenName = pathFile.Substring(pathFile.Length - 7, 3);
                                if (screenName.Equals(screen))
                                {
                                    streamReader = new StreamReader(pathFile);
                                    xmlSerializer = new XmlSerializer(typeof(SimulatedPrePrintedReceiptScreen_Type));
                                    simulatedPrePrintedReceiptScreen = xmlSerializer.Deserialize(streamReader) as SimulatedPrePrintedReceiptScreen_Type;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return simulatedPrePrintedReceiptScreen;
        }

        private static bool DeserializeScreen(string pathFile, out ScreenData_Type screenData)
        {
            bool ret = false;
            XmlSerializer xmlSerializer;
            StreamReader streamReader = null;
            screenData = new ScreenData_Type();
            try
            {
                streamReader = new StreamReader(pathFile);
                xmlSerializer = new XmlSerializer(typeof(ScreenData_Type));
                screenData = xmlSerializer.Deserialize(streamReader) as ScreenData_Type;
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            finally
            {
                streamReader.Close();
            }
            return ret;
        }

        /// <summary>
        /// Obtiene la tabla de estados desde un archivo XML y los guarda en una colección tipo List
        /// </summary>
        /// <returns></returns>
        public static List<StateTable_Type> GetStateTable(string path, out bool ret, string key, bool secure)
        {
            ret = false;
            XmlSerializer xmlSerializer;
            StreamReader streamReader;
            List<StateTable_Type> listofStateTable = new List<StateTable_Type>();
            try
            {
                if (File.Exists(path))
                {
                    if (secure)
                    {
                        //ret = GetAlephObject<List<StateTable_Type>>(path, key, out listofStateTable);
                        ret = GlobalAppData.Instance.GetAlephObject<List<StateTable_Type>>(path, key, out listofStateTable); 
                    }
                    else
                    {
                        streamReader = new StreamReader(path);
                        xmlSerializer = new XmlSerializer(typeof(List<StateTable_Type>));
                        listofStateTable = xmlSerializer.Deserialize(streamReader) as List<StateTable_Type>;
                        streamReader.Close();
                        ret = true;
                    }
                }
                else
                    Log.Error($"File \"{path}\" not found");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return listofStateTable;
        }

        /// <summary>
        /// Obtiene la tabla de estados desde un archivo XML y los guarda en una colección tipo List
        /// </summary>
        /// <returns></returns>
        public static EnhancedConfigurationParametersData_Type GetEnhParameters(string key, string fileName, bool secure)
        {
            bool ret = false;
            XmlSerializer xmlSerializer;
            StreamReader streamReader;
            string path = $"{Const.appPath}Config\\{fileName}";
            EnhancedConfigurationParametersData_Type EnhParamTable = new EnhancedConfigurationParametersData_Type();
            try
            {
                if (File.Exists(path))
                {
                    if(secure)
                    {
                        //GetAlephObject<EnhancedConfigurationParametersData_Type>(path, key, out EnhParamTable);
                        ret = GlobalAppData.Instance.GetAlephObject<EnhancedConfigurationParametersData_Type>(path, key, out EnhParamTable);
                    }
                    else
                    {
                        streamReader = new StreamReader(path);
                        xmlSerializer = new XmlSerializer(typeof(EnhancedConfigurationParametersData_Type));
                        EnhParamTable = xmlSerializer.Deserialize(streamReader) as EnhancedConfigurationParametersData_Type;
                        streamReader.Close();
                    }
                }
                else
                    Log.Error($"File \"{path}\" not found");
            }
            catch (Exception ex) { throw ex; }
            return EnhParamTable;
        }

        //private static bool GetAlephObject<T>(string path, string key, out T output)
        //{
        //    bool ret = false;
        //    output = default(T);
        //    try
        //    {
        //        string certPath = $"{Const.appPath}AlephCERT.pfx";
        //        if (File.Exists(certPath))
        //        {
        //            if (File.Exists(path))
        //            {
        //                var cert = Utilities.Encryption.GetX509CertFromFile(certPath, Utils.HexToStr(key));
        //                if (cert.PublicKey == null)
        //                    throw new Exception("Invalid public key");
        //                if (Utilities.Encryption.ProDecryptObject<T>(path, out output, cert.SerialNumber))
        //                {
        //                    Log.Info($"Get Aleph Object \"{Path.GetFileNameWithoutExtension(path)}\" OK");
        //                    ret = true;
        //                }
        //                else
        //                    Log.Error($"Get Aleph Object \"{Path.GetFileNameWithoutExtension(path)}\" ERROR");
        //            }
        //            else
        //                Log.Error($"File \"{path}\" not found");
        //        }
        //        else
        //            Log.Error($"File \"{certPath}\" not found");
        //    }
        //    catch (Exception ex) { Log.Fatal(ex); }
        //    return ret;
        //}
    }
}
