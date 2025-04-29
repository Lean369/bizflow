using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities
{
    public class LogConfiguration
    {
        private bool TraceToFileEnableField = true;
        private bool TraceToScreenEnableField = true;
        private string TracePathField = @"LogsTrace";
        private string TraceFileNameField = "Trace.log";
        private int LogFileSizeField = 10485760;
        private bool LogAppEnableField = true;
        private int LogAppLevelField = 3;
        private bool EnhancedLogEnableField = false;
        private bool EventLogEnableField = false;
        private string EventLogSourceField = string.Empty;
        private string LogPathField = @"LogsApp";
        private string LogFileNameField = "LogApp.log";

        public LogConfiguration()
        {

        }

        [System.Xml.Serialization.XmlElement("TraceToFileEnable")]
        public bool TraceToFileEnable
        {
            get { return this.TraceToFileEnableField; }
            set { this.TraceToFileEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("TraceToScreenEnable")]
        public bool TraceToScreenEnable
        {
            get { return this.TraceToScreenEnableField; }
            set { this.TraceToScreenEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("TracePath")]
        public string TracePath
        {
            get { return this.TracePathField; }
            set { this.TracePathField = value; }
        }

        [System.Xml.Serialization.XmlElement("TraceFileName")]
        public string TraceFileName
        {
            get { return this.TraceFileNameField; }
            set { this.TraceFileNameField = value; }
        }

        [System.Xml.Serialization.XmlElement("LogFileSize")]
        public int LogFileSize
        {
            get { return this.LogFileSizeField; }
            set { this.LogFileSizeField = value; }
        }

        [System.Xml.Serialization.XmlElement("LogAppEnable")]
        public bool LogAppEnable
        {
            get { return this.LogAppEnableField; }
            set { this.LogAppEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("LogAppLevel")]
        public int LogAppLevel
        {
            get { return this.LogAppLevelField; }
            set { this.LogAppLevelField = value; }
        }

        [System.Xml.Serialization.XmlElement("EnhancedLogEnable")]
        public bool EnhancedLogEnable
        {
            get { return this.EnhancedLogEnableField; }
            set { this.EnhancedLogEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("EventLogEnable")]
        public bool EventLogEnable
        {
            get { return this.EventLogEnableField; }
            set { this.EventLogEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("EventLogSource")]
        public string EventLogSource
        {
            get { return this.EventLogSourceField; }
            set { this.EventLogSourceField = value; }
        }

        [System.Xml.Serialization.XmlElement("LogPath")]
        public string LogPath
        {
            get { return this.LogPathField; }
            set { this.LogPathField = value; }
        }

        [System.Xml.Serialization.XmlElement("LogFileName")]
        public string LogFileName
        {
            get { return this.LogFileNameField; }
            set { this.LogFileNameField = value; }
        }
    }
}
