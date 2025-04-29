using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.ConfigurationState
{

    [Serializable]
    public partial class PropertiesConfigurationState
    {
        private string screenNumberField;
        private string exitNextStateNumberField;
        private string timeOutNextStateNumberField;
        private string errorNextStateNumberField;
        private string item1Field;
        private string item2Field;
        private string item3Field;
        private string HostNameField;
        private bool UpdateExternalMachineID_1Field;
        private string PathExternalMachineID_1Field;
        private string XPathExternalMachineID_1Field;
        private string XPathCountryId_1Field;
        private bool UpdateExternalMachineID_2Field;
        private string PathExternalMachineID_2Field;
        private string XPathExternalMachineID_2Field;
        private bool UpdateComputerNameField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnConfigurationStart;
        public StateEvent OnConfigurationLoad;
        public StateEvent OnConfigurationOptions;

        public PropertiesConfigurationState()
        {
            this.screenNumberField = "";
            this.exitNextStateNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.errorNextStateNumberField = "";
            this.item1Field = "";
            this.item2Field = "";
            this.item3Field = "";
            this.HostNameField = "PrdmHost";
            this.UpdateExternalMachineID_1 = true;
            this.PathExternalMachineID_1 = @"C:\software\cashmanagerprm\conf\config.xml";
            this.XPathExternalMachineID_1 = "Config/OutputXMLInfo/Device_Id";
            this.XPathCountryId = "Config/OutputXMLInfo/Country_Id";
            this.UpdateExternalMachineID_2 = false;
            this.PathExternalMachineID_2 = "";
            this.XPathExternalMachineID_2 = "";
            this.UpdateComputerNameField = true;
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.moreTimeField.MaxTimeOut = 90;
            this.OnConfigurationStart = new StateEvent(StateEvent.EventType.navigate, "configuration.htm", "");
            this.OnConfigurationLoad = new StateEvent(StateEvent.EventType.runScript, "LoadConfigurationData", "");
            this.OnConfigurationOptions = new StateEvent(StateEvent.EventType.runScript, "LoadConfigurationOptions", "");
        }

        #region Properties
        [XmlElement()]
        public string ScreenNumber { get => screenNumberField; set => screenNumberField = value; }

        [XmlElement()]
        public string ExitNextStateNumber { get => exitNextStateNumberField; set => exitNextStateNumberField = value; }

        [XmlElement()]
        public string TimeOutNextStateNumber { get => timeOutNextStateNumberField; set => timeOutNextStateNumberField = value; }

        [XmlElement()]
        public string ErrorNextStateNumber { get => errorNextStateNumberField; set => errorNextStateNumberField = value; }

        [XmlElement()]
        public string Item1 { get => item1Field; set => item1Field = value; }

        [XmlElement()]
        public string Item2 { get => item2Field; set => item2Field = value; }

        [XmlElement()]
        public string Item3 { get => item3Field; set => item3Field = value; }

        [XmlElement()]
        public string HostName { get => HostNameField; set => HostNameField = value; }

        [XmlElement()]
        public bool UpdateExternalMachineID_1 { get => UpdateExternalMachineID_1Field; set => UpdateExternalMachineID_1Field = value; }

        [XmlElement()]
        public string PathExternalMachineID_1 { get => PathExternalMachineID_1Field; set => PathExternalMachineID_1Field = value; }

        [XmlElement()]
        public string XPathExternalMachineID_1 { get => XPathExternalMachineID_1Field; set => XPathExternalMachineID_1Field = value; }

        [XmlElement()]
        public string XPathCountryId { get => XPathCountryId_1Field; set => XPathCountryId_1Field = value; }

        [XmlElement()]
        public bool UpdateExternalMachineID_2 { get => UpdateExternalMachineID_2Field; set => UpdateExternalMachineID_2Field = value; }

        [XmlElement()]
        public string PathExternalMachineID_2 { get => PathExternalMachineID_2Field; set => PathExternalMachineID_2Field = value; }

        [XmlElement()]
        public string XPathExternalMachineID_2 { get => XPathExternalMachineID_2Field; set => XPathExternalMachineID_2Field = value; }

        [XmlElement()]
        public bool UpdateComputerName { get => UpdateComputerNameField; set => UpdateComputerNameField = value; }

        [XmlElement()]
        public JournalProperties Journal { get => journalField; set => journalField = value; }

        [XmlElement()]
        public MoreTimeProperties MoreTime { get => moreTimeField; set => moreTimeField = value; }

        #endregion Properties
    }
}