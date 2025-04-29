using System;
using System.IO;

namespace Entities
{
    public class Log
    {
        private static readonly NLog.Logger Trace = NLog.LogManager.GetLogger("TRACE");
        public delegate void DelegateSendTraceData(string sDataReceived);
        public event DelegateSendTraceData EvtSendTraceData; //Evento que envía los mensajes de trace.
        public delegate void DelegateSendLogData(string sDataReceived);
        public event DelegateSendLogData EvtSendLogData; //Evento que envía los mensajes de log de aplicación.
        private AlephATMAppData alephATMAppData;
        //private Utilities.Logger Logger; //Objeto de logeo de aplicación.

        public Log()
        {
            bool retGetAppData = false; //logUDBD = false, logPlain = false;
            alephATMAppData = AlephATMAppData.GetAppData(out retGetAppData);
            if (retGetAppData)
            {
                //logUDBD = alephATMAppData.LogConfig.EnhancedLogEnable ? true : false;
                //logPlain = alephATMAppData.LogConfig.EnhancedLogEnable ? false : true;
                //this.Logger = new Utilities.Logger(alephATMAppData.LogConfig.LogFileName, alephATMAppData.LogConfig.LogPath, alephATMAppData.LogConfig.TraceFileName,
                //    alephATMAppData.LogConfig.TracePath, alephATMAppData.LogConfig.LogFileSize, logUDBD, logPlain, alephATMAppData.LogConfig.TraceToFileEnable, 7, (Utilities.Logger.LogLevel)alephATMAppData.LogConfig.LogAppLevel);
                //this.Logger.ReceivedStringDataError += new Utilities.Logger.EventReceivedErrorData(this.RaiseEvtSendLogData);
                //this.Logger.ReceivedStringDataTrace += new Utilities.Logger.EventReceivedTraceData(this.RaiseEvtSendTraceData);
                //this.DeleteFileLogs();
            }
        }

        /// <summary>
        /// Procesa el logeo de excepciones en archivo, pantalla y EventLog.
        /// </summary>
        /// <param name="ex">Excepción a logear</param>
        public void ProcessLogException(string functionName, Exception ex)
        {
            //try
            //{
            //    if (this.alephATMAppData.LogConfig.LogAppEnable)
            //        this.Logger.LogException(ex, functionName);
            //}
            //catch (Exception exi) { System.IO.File.AppendAllText(string.Format(@"{0}\LogError.log", Const.appPath), string.Format("LogException(): {0}{1}", exi.Message, Environment.NewLine)); }
        }

        /// <summary>
        /// Procesa el traceo por pantalla
        /// </summary>
        /// <param name="ex">Excepción a logear</param>
        //public void ProcessTrace(string sData)
        //{
        //    try
        //    {
        //        Trace.Trace(sData);
        //        //if (this.alephATMAppData.LogConfig.TraceToFileEnable)
        //        //{
        //        //    this.Logger.LogTrace(deviceID, sData, enconding);
        //        //}
        //    }
        //    catch (Exception ex) { System.IO.File.AppendAllText(string.Format(@"{0}\LogError.log", Const.appPath), string.Format("LogAppMessage(): {0}{1}", ex.Message, Environment.NewLine)); }
        //}

        /// <summary>
        /// Procesa el logeo de excepciones en archivo, pantalla y EventLog.
        /// </summary>
        /// <param name="ex">Excepción a logear</param>
        //public void ProcessLog(string functionName, int iD, Utilities.Logger.LogType logLevel, string message)
        //{
        //    //try
        //    //{
        //    //    if (this.alephATMAppData.LogConfig.LogAppEnable)
        //    //    {
        //    //        this.Logger.LogMessage(functionName, iD, logLevel, message);
        //    //    }
        //    //}
        //    //catch (Exception ex) { File.AppendAllText(string.Format(@"{0}\LogError.log", Const.appPath), string.Format("LogAppMessage(): {0}{1}", ex.Message, Environment.NewLine)); }
        //}


        /// <summary>
        /// Toma el evento y envía los datos al DM para mostrarlos por pantalla
        /// </summary>
        /// <param name="sDataReceived">Mensaje a logear</param>
        protected void RaiseEvtSendTraceData(string sDataReceived)
        {
            DelegateSendTraceData tmp = EvtSendTraceData;
            if (tmp != null)
                tmp(sDataReceived);
        }

        /// <summary>
        /// Re lanza el evento de mensaje a logear generado en el Logger
        /// </summary>
        /// <param name="sDataReceived">Mensaje de Log.</param>
        protected void RaiseEvtSendLogData(string sDataReceived)
        {
            DelegateSendLogData tmp = EvtSendLogData;
            if (tmp != null)
                tmp(sDataReceived);
        }

        /// <summary>
        /// Borra los archivos de log y trace antiguos
        /// </summary>
        internal void DeleteFileLogs()
        {
            try
            {
                //string extension = "*";
                //string pathLogApp = string.Format(@"{0}", alephATMAppData.LogConfig.LogPath);
                //this.DeleteLog(pathLogApp, extension);
                //string pathLogTrace = string.Format(@"{0}", alephATMAppData.LogConfig.TracePath);
                //this.DeleteLog(pathLogTrace, extension);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Borra los archivos de Logs y Trace de mas de 30 días
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extension"></param>
        private void DeleteLog(string path, string extension)
        {
            if (Directory.Exists(path))
            {
                foreach (string arch in Directory.GetFiles(path, extension))
                {
                    FileInfo fi = new FileInfo(arch);
                    if (DateTime.Now.Subtract(fi.CreationTime).Days > this.alephATMAppData.MaxDaysBackupLog)
                    {
                        File.Delete(arch);
                    }
                }
            }
        }
    }
}
