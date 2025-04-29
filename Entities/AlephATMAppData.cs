using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
namespace Entities
{
    [Serializable()]
    public class AlephATMAppData
    {
        //App
        private bool SendMailNotificationsEnableField = true;
        private int MaxDaysBackupLogField = 30;
        //[XmlAnyElement("OperationMode")]
        //public XmlComment OperationModeComment { get { return new XmlDocument().CreateComment("OperationMode values: Batch | OnLine"); } set { } }
        private Const.OperationMode OperationModeField = Const.OperationMode.Batch;
        private Enums.TerminalModel TerminalModelField = Enums.TerminalModel.SNBC_CTI90;
        private Enums.Branding BrandingField = Enums.Branding.Prosegur;
        private string ThumbPrintCertificateField = "6ad682bd2b7b643764995c25eec869cb40d4025f";
        private string KeyCertificateField = "433473685430643479";
        private string FlowFileNameField = "StatesRTLA.aph";
        private string EnhParamFileNameField = "EnhancedParametersRTLA.aph";
        private bool SecureDownloadField = true;
        private bool RetrievalTransactionEnableField = true;
        private bool ShipOutInBackgroundEnableField = true;
        private LanguageItem[] LanguageListField = new LanguageItem[]
        {
            new LanguageItem { Name = LanguageName.Spanish, Enabled = true },
            new LanguageItem { Name = LanguageName.English, Enabled = true },
            new LanguageItem { Name = LanguageName.Portuguese, Enabled = true },
        };
        private string DefaultLanguageField = "Spanish";
        private string DefaultCurrencyField = "ARS";
        private string CountryIdField = "AR";
        private string RegionField = "es-AR";
        private string PrinterTemplateFileNameField = "Template56mm-Prosegur.xml";
        private string StateShipoutField = "200";
        private string StateSupervisorField = "201";
        private string PrintStateField = "120";
        private string ConfigurationStateField = "203";
        private bool AppendCollectionIdField = true;
        private string CollectionIdTagNameField = "COLLECTIONID";
        private int CurrencyFormatDecimalsField = 2;
        //Host
        private bool RoleServerNDCHostField = true;
        private bool LocalConnectionNDCHostField = true;
        private string IpHostServerNDCHostField = "none";
        private int LocalPortNDCHostField = 9000;
        private string IpHostClientNDCHostField = "127.0.0.1";
        private int RemotePortNDCHostField = 33103;
        private bool HeaderEnableField = true;
        private string DefaultHostNameField = "PrdmHost";
        private string StatusHostNameField = "PrdmHost";
        private bool SendStatusEnabledField = true;
        private bool HostEnabledField = true;
        //AlephDEV
        private bool AlephDEVEnableField = true;
        private bool AutoStartAlephDEVField = true;
#if DEBUG
        private string AlephDEVPathField = @"C:\Users\Nico\Documents\GitHub\AlephATM.v.4\iot_devices\AlephDEV\bin\Debug";
        private bool PrintConfigPropertiesField = false;
#else
        private string AlephDEVPathField = "";
        private bool PrintConfigPropertiesField = false;
#endif
        private ProcessWindowStyle AlephDEVWindowStyleField = ProcessWindowStyle.Hidden;
        private string AlephDEVuriField = "ws://localhost:8080/AlephATM";
        private int AlephDEVTimeOutField = 60000;
        //Supervisor
        private bool SupervisorAppEnabledField = true;
        private string SupervisorAppPathField = @"C:\Program Files (x86)\AlephMenuSupervisor\AlephMenuSupervisor.exe";
        private ProcessWindowStyle SupervisorWindowStyleField = ProcessWindowStyle.Maximized;
        private bool KillAlephDevField = true;
        private bool SpaEnableField = false;
        private string SpaUrlField = "dist\\index.html";

        public AlephATMAppData() { }

        public static AlephATMAppData GetAppData(out bool ret)
        {
            ret = false;
            AlephATMAppData appData = new AlephATMAppData();
            string configFolder = $"{Const.appPath}Config";
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
            string fileName = $"{configFolder}\\AlephATMAppData.xml";
            appData = Utilities.Utils.GetGenericXmlData<AlephATMAppData>(out ret, fileName, appData);
            return appData;
        }

        #region "Properties"
        [XmlElement("SendMailNotificationsEnable")]
        public bool SendMailNotificationsEnable
        {
            get { return this.SendMailNotificationsEnableField; }
            set { this.SendMailNotificationsEnableField = value; }
        }

        [XmlElement("MaxDaysBackupLog")]
        public int MaxDaysBackupLog
        {
            get { return this.MaxDaysBackupLogField; }
            set { this.MaxDaysBackupLogField = value; }
        }

        [XmlElement("OperationMode")]
        public Const.OperationMode OperationMode
        {
            get { return this.OperationModeField; }
            set { this.OperationModeField = value; }
        }

        [XmlElement("TerminalModel")]
        public Enums.TerminalModel TerminalModel
        {
            get { return this.TerminalModelField; }
            set { this.TerminalModelField = value; }
        }

        [XmlElement("Branding")]
        public Enums.Branding Branding
        {
            get { return this.BrandingField; }
            set { this.BrandingField = value; }
        }

        [XmlElement("ThumbPrintCertificate")]
        public string ThumbPrintCertificate
        {
            get { return this.ThumbPrintCertificateField; }
            set { this.ThumbPrintCertificateField = value; }
        }

        [XmlElement("KeyCertificate")]
        public string KeyCertificate
        {
            get { return this.KeyCertificateField; }
            set { this.KeyCertificateField = value; }
        }

        [XmlElement("FlowFileName")]
        public string FlowFileName
        {
            get { return this.FlowFileNameField; }
            set { this.FlowFileNameField = value; }
        }


        [XmlElement("EnhParamFileName")]
        public string EnhParamFileName
        {
            get { return this.EnhParamFileNameField; }
            set { this.EnhParamFileNameField = value; }
        }

        [XmlElement("SecureDownload")]
        public bool SecureDownload
        {
            get { return this.SecureDownloadField; }
            set { this.SecureDownloadField = value; }
        }

        [XmlElement("RetrievalTransactionEnable")]
        public bool RetrievalTransactionEnable
        {
            get { return this.RetrievalTransactionEnableField; }
            set { this.RetrievalTransactionEnableField = value; }
        }

        [XmlElement("ShipOutInBackgroundEnable")]
        public bool ShipOutInBackgroundEnable
        {
            get { return this.ShipOutInBackgroundEnableField; }
            set { this.ShipOutInBackgroundEnableField = value; }
        }

        [XmlArray("LanguageList")]
        [XmlArrayItem("Language")]
        public LanguageItem[] LanguageList
        {
            get { return this.LanguageListField; }
            set { this.LanguageListField = value; }
        }

        [XmlElement("DefaultLanguage")]
        public string DefaultLanguage
        {
            get { return this.DefaultLanguageField; }
            set { this.DefaultLanguageField = value; }
        }

        [XmlElement("DefaultCurrency")]
        public string DefaultCurrency
        {
            get { return this.DefaultCurrencyField; }
            set { this.DefaultCurrencyField = value; }
        }

        [XmlElement("CountryId")]
        public string CountryId
        {
            get { return this.CountryIdField; }
            set { this.CountryIdField = value; }
        }

        [XmlElement("Region")]
        public string Region
        {
            get { return this.RegionField; }
            set { this.RegionField = value; }
        }
        //Host

        [XmlElement("RoleServerNDCHost")]
        public bool RoleServerNDCHost
        {
            get { return this.RoleServerNDCHostField; }
            set { this.RoleServerNDCHostField = value; }
        }

        [XmlElement("LocalConnectionNDCHost")]
        public bool LocalConnectionNDCHost
        {
            get { return this.LocalConnectionNDCHostField; }
            set { this.LocalConnectionNDCHostField = value; }
        }

        [XmlElement("IpHostServerNDCHost")]
        public string IpHostServerNDCHost
        {
            get { return this.IpHostServerNDCHostField; }
            set { this.IpHostServerNDCHostField = value; }
        }

        [XmlElement("LocalPortNDCHost")]
        public int LocalPortNDCHost
        {
            get { return this.LocalPortNDCHostField; }
            set { this.LocalPortNDCHostField = value; }
        }

        [XmlElement("IpHostClientNDCHost")]
        public string IpHostClientNDCHost
        {
            get { return this.IpHostClientNDCHostField; }
            set { this.IpHostClientNDCHostField = value; }
        }

        [XmlElement("RemotePortNDCHost")]
        public int RemotePortNDCHost
        {
            get { return this.RemotePortNDCHostField; }
            set { this.RemotePortNDCHostField = value; }
        }

        [XmlElement("DefaultHostName")]
        public string DefaultHostName
        {
            get { return this.DefaultHostNameField; }
            set { this.DefaultHostNameField = value; }
        }

        [XmlElement("StatusHostName")]
        public string StatusHostName
        {
            get { return this.StatusHostNameField; }
            set { this.StatusHostNameField = value; }
        }

        [XmlElement("SendStatusEnabled")]
        public bool SendStatusEnabled
        {
            get { return this.SendStatusEnabledField; }
            set { this.SendStatusEnabledField = value; }
        }

        [XmlElement("HeaderEnable")]
        public bool HeaderEnable
        {
            get { return this.HeaderEnableField; }
            set { this.HeaderEnableField = value; }
        }

        [XmlElement("HostEnabled")]
        public bool HostEnabled
        {
            get { return this.HostEnabledField; }
            set { this.HostEnabledField = value; }
        }
        ///AlephDEV

        [XmlElement("AlephDEVEnable")]
        public bool AlephDEVEnable
        {
            get { return this.AlephDEVEnableField; }
            set { this.AlephDEVEnableField = value; }
        }

        [XmlElement("AutoStartAlephDEV")]
        public bool AutoStartAlephDEV
        {
            get { return this.AutoStartAlephDEVField; }
            set { this.AutoStartAlephDEVField = value; }
        }

        [XmlElement("AlephDEVPath")]
        public string AlephDEVPath
        {
            get { return this.AlephDEVPathField; }
            set { this.AlephDEVPathField = value; }
        }

        [XmlElement("AlephDEVWindowStyle")]
        public ProcessWindowStyle AlephDEVWindowStyle
        {
            get { return this.AlephDEVWindowStyleField; }
            set { this.AlephDEVWindowStyleField = value; }
        }

        [XmlElement("AlephDEVTimeOut")]
        public int AlephDEVTimeOut
        {
            get { return this.AlephDEVTimeOutField; }
            set { this.AlephDEVTimeOutField = value; }
        }

        [XmlElement("AlephDEVuri")]
        public string AlephDEVuri
        {
            get { return this.AlephDEVuriField; }
            set { this.AlephDEVuriField = value; }
        }

        //Supervisor

        [XmlElement("SupervisorAppEnabled")]
        public bool SupervisorAppEnabled
        {
            get { return this.SupervisorAppEnabledField; }
            set { this.SupervisorAppEnabledField = value; }
        }

        [XmlElement("SupervisorAppPath")]
        public string SupervisorAppPath
        {
            get { return this.SupervisorAppPathField; }
            set { this.SupervisorAppPathField = value; }
        }

        [XmlElement("SupervisorWindowStyle")]
        public ProcessWindowStyle SupervisorWindowStyle
        {
            get { return this.SupervisorWindowStyleField; }
            set { this.SupervisorWindowStyleField = value; }
        }

        [XmlElement("PrinterTemplateFileName")]
        public string PrinterTemplateFileName
        {
            get { return this.PrinterTemplateFileNameField; }
            set { this.PrinterTemplateFileNameField = value; }
        }

        [XmlElement("StateShipout")]
        public string StateShipout
        {
            get { return this.StateShipoutField; }
            set { this.StateShipoutField = value; }
        }
        [XmlElement("StateSupervisor")]
        public string StateSupervisor
        {
            get { return this.StateSupervisorField; }
            set { this.StateSupervisorField = value; }
        }

        [XmlElement("PrintState")]
        public string PrintState
        {
            get { return this.PrintStateField; }
            set { this.PrintStateField = value; }
        }

        
        [XmlElement("ConfigurationState")]
        public string ConfigurationState
        {
            get { return this.ConfigurationStateField; }
            set { this.ConfigurationStateField = value; }
        }


        [XmlElement("AppendCollectionId")]
        public bool AppendCollectionId
        {
            get { return this.AppendCollectionIdField; }
            set { this.AppendCollectionIdField = value; }
        }

        [XmlElement("CollectionIdTagName")]
        public string CollectionIdTagName
        {
            get { return this.CollectionIdTagNameField; }
            set { this.CollectionIdTagNameField = value; }
        }


        [XmlElement("PrintConfigProperties")]
        public bool PrintConfigProperties
        {
            get { return this.PrintConfigPropertiesField; }
            set { this.PrintConfigPropertiesField = value; }
        }

        [XmlElement("KillAlephDev")]
        public bool KillAlephDev
        {
            get { return this.KillAlephDevField; }
            set { this.KillAlephDevField = value; }
        }

        [XmlElement("SpaEnable")]
        public bool SpaEnable
        {
            get { return this.SpaEnableField; }
            set { this.SpaEnableField = value; }
        }

        [XmlElement("SpaUrl")]
        public string SpaUrl
        {
            get { return this.SpaUrlField; }
            set { this.SpaUrlField = value; }
        }

        [XmlElement("CurrencyFormatDecimals")]
        public int CurrencyFormatDecimals
        {
            get { return this.CurrencyFormatDecimalsField; }
            set { this.CurrencyFormatDecimalsField = value; }
        }
        #endregion
    }
}
