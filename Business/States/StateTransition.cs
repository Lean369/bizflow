using Entities;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Business
{
    public abstract class StateTransition : State
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public enum ProcessState { INPROGRESS = 0, FINALIZED = 1, ERROR = 2 }

        public enum StateResult
        {
            SUCCESS = 0,
            CANCEL = -1,
            TIMEOUT = -2,
            HWERROR = -3,
            SWERROR = -4,
            SUSPEND = -5,
            DECLINED = -6
        }

        public ProcessState CurrentState { get; set; }
        public virtual string ActivityName { get; set; }
        public virtual StateResult ActivityResult { get; set; }

        public abstract bool InitializeActivity(Core core);
        public abstract void ActivityStart();
        public abstract void Quit();
        public abstract void SetActivityResult(StateResult result, string nextState);

        internal T GetProperties<T>(out bool ret, object prop)
        {
            ret = false;
            string pathFile = $"{Const.appPath}StatesSets\\Properties{this.ActivityName}.xml";
            //return GetGenericXmlData<T>(out ret, pathFile, prop);
            return Utilities.Utils.GetGenericXmlData<T>(out ret, pathFile, prop);
        }

        internal void PrintProperties(object obj, string stateNumber)
        {
            this.PrintProperties(obj, stateNumber, this.ActivityName);
        }

        //public static T GetGenericXmlData<T>(out bool ret, string pathFile, object obj)
        //{
        //    XmlSerializer xmlSerializer;
        //    ret = true;
        //    XmlDocument xmlDocument = new XmlDocument();
        //    //if (string.IsNullOrEmpty(pathFile))
        //    //    return default(T);
        //    T output = default(T);
        //    try
        //    {
        //        Type outType = typeof(T);
        //        if (!File.Exists(pathFile))
        //        {
        //            xmlSerializer = new XmlSerializer(outType);
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                xmlSerializer.Serialize(ms, obj);
        //                ms.Position = 0;
        //                xmlDocument.Load(ms);
        //                xmlDocument.Save(pathFile);
        //                ms.Close();
        //            }
        //        }
        //        xmlDocument.Load(pathFile);
        //        string xmlString = xmlDocument.OuterXml;
        //        using (StringReader sr = new StringReader(xmlString))
        //        {
        //            XmlSerializer serializer = new XmlSerializer(outType);
        //            using (XmlReader xr = new XmlTextReader(sr))
        //            {
        //                output = (T)serializer.Deserialize(xr);
        //                xr.Close();
        //            }
        //            sr.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ret = false;
        //    }
        //    return output;
        //}

    }
}
