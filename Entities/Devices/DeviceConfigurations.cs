using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable()]
    public class DeviceConfigurations
    {
        private string AlephDEVIpHostField = "none";
        private int AlephDEVLocalPortField = 8080;
        private bool ApiXfsEnableField = true;
        private bool XfsEventsEnableField = true;
        private BAGconfig BAGconfigField;
        private PINconfig PINconfigField;
        private CIMconfig CIMconfigField;
        private CDMconfig CDMconfigField;
        private CDM2config CDM2configField;
        private IDCconfig IDCconfigField;
        private PADconfig PADconfigField;
        private BCRconfig BCRconfigField;
        private PRTconfig PRTconfigField;
        private STM_1config STM_1configField;
        private FPRconfig FPRconfigField;
        private ADMconfig ADMconfigField;
        private COINconfig COINconfigField;
        private IOBoardConfig IOBoardConfigField;
        private TERconfig TERconfigField;

        public DeviceConfigurations() { }

        public static DeviceConfigurations GetObject(out bool ret, Enums.TerminalModel terminalModel, string defaultCurrency)
        {
            ret = false;
            DeviceConfigurations devConf = new DeviceConfigurations(terminalModel, defaultCurrency);
            string configFolder = $"{Const.appPath}Config";
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
            string fileName = $"{Const.appPath}Config\\DeviceConfigurations.xml";
            devConf = Utilities.Utils.GetGenericXmlData<DeviceConfigurations>(out ret, fileName, devConf);
            return devConf;
        }

        public DeviceConfigurations(Enums.TerminalModel terminalModel, string defaultCurrency)
        {
            this.PINconfigField = new PINconfig();
            this.BAGconfigField = new BAGconfig(terminalModel);
            this.CIMconfigField = new CIMconfig(terminalModel, defaultCurrency);
            this.CDMconfigField = new CDMconfig(terminalModel);
            this.CDM2configField = new CDM2config();
            this.IDCconfigField = new IDCconfig();
            this.PADconfigField = new PADconfig();
            this.BCRconfig = new BCRconfig(terminalModel);
            this.PRTconfigField = new PRTconfig(terminalModel);
            this.STM_1configField = new STM_1config();
            this.FPRconfigField = new FPRconfig();
            this.ADMconfigField = new ADMconfig();
            this.COINconfigField = new COINconfig();
            this.IOBoardConfigField = new IOBoardConfig(terminalModel);
            this.TERconfigField = new TERconfig();
            this.ApiXfsEnableField = true;
            this.XfsEventsEnableField = true;
            this.CIMconfigField.Enable = true;
            this.PRTconfig.Enable = true;
            this.BAGconfigField.Enable = true;
            this.ADMconfigField.Enable = false;
            this.CDMconfigField.Enable = false;
            this.BCRconfigField.Enable = true;
            this.COINconfigField.Enable = false;

            switch (terminalModel)
            {
                case Enums.TerminalModel.CTR50:
                    this.CDMconfigField.Enable = true;
                    this.COINconfigField.Enable = true;
                    this.BAGconfigField.Enable = false;
                    break;
                case Enums.TerminalModel.ADM:
                    this.ApiXfsEnableField = false;
                    this.XfsEventsEnableField = false;
                    this.CIMconfigField.Enable = false;
                    this.PRTconfig.Enable = false;
                    this.BAGconfigField.Enable = false;
                    this.ADMconfigField.Enable = true;
                    break;
            }
        }

        #region "Properties"
        [XmlElement("AlephDEVIpHost")]
        public string AlephDEVIpHost
        {
            get { return this.AlephDEVIpHostField; }
            set { this.AlephDEVIpHostField = value; }
        }

        [XmlElement("AlephDEVLocalPort")]
        public int AlephDEVLocalPort
        {
            get { return this.AlephDEVLocalPortField; }
            set { this.AlephDEVLocalPortField = value; }
        }

        [XmlElement("ApiXfsEnable")]
        public bool ApiXfsEnable
        {
            get { return this.ApiXfsEnableField; }
            set { this.ApiXfsEnableField = value; }
        }

        [XmlElement("XfsEventsEnable")]
        public bool XfsEventsEnable
        {
            get { return this.XfsEventsEnableField; }
            set { this.XfsEventsEnableField = value; }
        }

        [XmlElement("BAGconfig")]
        public BAGconfig BAGconfig
        {
            get { return this.BAGconfigField; }
            set { this.BAGconfigField = value; }
        }

        [XmlElement("PINconfig")]
        public PINconfig PINconfig
        {
            get { return this.PINconfigField; }
            set { this.PINconfigField = value; }
        }

        [XmlElement("CIMconfig")]
        public CIMconfig CIMconfig
        {
            get { return this.CIMconfigField; }
            set { this.CIMconfigField = value; }
        }

        [XmlElement("CDMconfig")]
        public CDMconfig CDMconfig
        {
            get { return this.CDMconfigField; }
            set { this.CDMconfigField = value; }
        }

        [XmlElement("CDM2config")]
        public CDM2config CDM2config
        {
            get { return this.CDM2configField; }
            set { this.CDM2configField = value; }
        }

        [XmlElement("PADconfig")]
        public PADconfig PADconfig
        {
            get { return this.PADconfigField; }
            set { this.PADconfigField = value; }
        }

        [XmlElement("IDCconfig")]
        public IDCconfig IDCconfig
        {
            get { return this.IDCconfigField; }
            set { this.IDCconfigField = value; }
        }

        [XmlElement("BCRconfig")]
        public BCRconfig BCRconfig
        {
            get { return this.BCRconfigField; }
            set { this.BCRconfigField = value; }
        }

        [XmlElement("PRTconfig")]
        public PRTconfig PRTconfig
        {
            get { return this.PRTconfigField; }
            set { this.PRTconfigField = value; }
        }

        [XmlElement("STM_1config")]
        public STM_1config STM_1config
        {
            get { return this.STM_1configField; }
            set { this.STM_1configField = value; }
        }

        [XmlElement("FPRconfig")]
        public FPRconfig FPRconfig
        {
            get { return this.FPRconfigField; }
            set { this.FPRconfigField = value; }
        }

        [XmlElement("ADMconfig")]
        public ADMconfig ADMconfig
        {
            get { return this.ADMconfigField; }
            set { this.ADMconfigField = value; }
        }

        [XmlElement("COINconfig")]
        public COINconfig COINconfig
        {
            get { return this.COINconfigField; }
            set { this.COINconfigField = value; }
        }

        [XmlElement("IOBoardConfig")]
        public IOBoardConfig IOBoardConfig
        {
            get { return this.IOBoardConfigField; }
            set { this.IOBoardConfigField = value; }
        }

        [XmlElement("TERconfig")]
        public TERconfig TERconfig
        {
            get { return this.TERconfigField; }
            set { this.TERconfigField = value; }
        }
        #endregion
    }

    [Serializable]
    public class CassettesConfig
    {

    }

    [Serializable()]
    public class PINconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;

        public PINconfig() { }

        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }
    }

    [Serializable()]
    public class BAGconfig
    {
        private bool EnableField = true;
        private bool RequiredField = false;
        private int BagCapacityField = 15000;

        public BAGconfig() { }

        public BAGconfig(Enums.TerminalModel terminalModel)
        {
            switch (terminalModel)
            {
                case Enums.TerminalModel.SNBC_CTE1:
                    this.BagCapacityField = 5000;
                    break;
                case Enums.TerminalModel.GRG_P2600:
                    this.BagCapacityField = 9000;
                    break;
                case Enums.TerminalModel.Depositario:
                    this.BagCapacityField = 8500;
                    break;
                case Enums.TerminalModel.MiniBank_JH6000_D:
                    this.BagCapacityField = 5000;
                    break;
                case Enums.TerminalModel.MiniBank_JH600_A:
                    this.BagCapacityField = 600;
                    break;
                case Enums.TerminalModel.BDM_300:
                    this.BagCapacityField = 10000;
                    break;
                default:
                    this.BagCapacityField = 15000;
                    break;
            }
        }

        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("BagCapacity")]
        public int BagCapacity
        {
            get { return this.BagCapacityField; }
            set { this.BagCapacityField = value; }
        }
    }

    [Serializable()]
    public class CIMconfig
    {
        private bool EnableField = true;
        private bool RequiredField = false;
        private bool VerifyEscrowNotesAtInitField = false;
        private bool OpenEscrowAtInitField = false;
        private bool KeepConnectionOpenField = false;
        private bool ConfigAcceptedNotesAtInitField = true;
        private string EnableCurrencyField = "";
        private string AcceptedCurrenciesField = "";

        public CIMconfig() { }

        public CIMconfig(Enums.TerminalModel terminalModel, string defaultCurrency)
        {
            this.EnableCurrencyField = defaultCurrency;
            if (defaultCurrency.Equals("ARS"))
                this.AcceptedCurrenciesField = "ARS,EUR,USD";
            else if (defaultCurrency.Equals("UYU"))
                this.AcceptedCurrenciesField = "UYU,EUR,USD";
            else if (defaultCurrency.Equals("PYG"))
                this.AcceptedCurrenciesField = "PYG,EUR,USD,BRL";
            else
                this.AcceptedCurrenciesField = defaultCurrency;
            switch (terminalModel)
            {
                case Enums.TerminalModel.SNBC_CTE1:
                case Enums.TerminalModel.SNBC_CTI90:
                case Enums.TerminalModel.CTIUL:
                case Enums.TerminalModel.Depositario:
                case Enums.TerminalModel.MADERO_BR:
                case Enums.TerminalModel.SNBC_IN:
                case Enums.TerminalModel.BDM_300:
                    this.ConfigAcceptedNotesAtInitField = true;
                    this.KeepConnectionOpen = false;
                    break;
                case Enums.TerminalModel.CTR50:
                case Enums.TerminalModel.GRG_P2600:
                    this.KeepConnectionOpenField = true;
                    break;
                default:
                    this.ConfigAcceptedNotesAtInitField = false;
                    break;
            }
        }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("VerifyEscrowNotesAtInit")]
        public bool VerifyEscrowNotesAtInit
        {
            get { return this.VerifyEscrowNotesAtInitField; }
            set { this.VerifyEscrowNotesAtInitField = value; }
        }

        [XmlElement("OpenEscrowAtInit")]
        public bool OpenEscrowAtInit
        {
            get { return this.OpenEscrowAtInitField; }
            set { this.OpenEscrowAtInitField = value; }
        }

        [XmlElement("KeepConnectionOpen")]
        public bool KeepConnectionOpen
        {
            get { return this.KeepConnectionOpenField; }
            set { this.KeepConnectionOpenField = value; }
        }

        [XmlElement("ConfigAcceptedNotesAtInit")]
        public bool ConfigAcceptedNotesAtInit
        {
            get { return this.ConfigAcceptedNotesAtInitField; }
            set { this.ConfigAcceptedNotesAtInitField = value; }
        }

        [XmlElement("EnableCurrency")]
        public string EnableCurrency
        {
            get { return this.EnableCurrencyField; }
            set { this.EnableCurrencyField = value; }
        }

        [XmlElement("AcceptedCurrencies")]
        public string AcceptedCurrencies
        {
            get { return this.AcceptedCurrenciesField; }
            set { this.AcceptedCurrenciesField = value; }
        }
        #endregion Properties
    }


    [Serializable()]
    public class CDMconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;
        private bool KeepConnectionOpenField = false;

        public CDMconfig() { }

        public CDMconfig(Enums.TerminalModel terminalModel)
        {
            switch (terminalModel)
            {
                case Enums.TerminalModel.CTR50:
                    this.KeepConnectionOpenField = true;
                    break;
                default:
                    this.KeepConnectionOpen = false;
                    break;
            }
        }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("KeepConnectionOpen")]
        public bool KeepConnectionOpen
        {
            get { return this.KeepConnectionOpenField; }
            set { this.KeepConnectionOpenField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class CDM2config
    {
        private bool EnableField = false;
        private bool RequiredField = false;

        public CDM2config() { }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class IDCconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;

        public IDCconfig() { }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class PADconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;

        public PADconfig() { }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class BCRconfig
    {
        private bool EnableField;
        private bool RequiredField = false;
        private Enums.BarcodeModel BarcodeModelField;
        private int PortField = 9;
        private int BaudField = 115200;
        private bool MultiReadField = false;

        public BCRconfig() { }

        public BCRconfig(Enums.TerminalModel terminalModel)
        {
            switch (terminalModel)
            {
                case Enums.TerminalModel.SNBC_CTI90:
                case Enums.TerminalModel.SNBC_CTE1:
                case Enums.TerminalModel.CTIUL:
                    this.BarcodeModelField = Enums.BarcodeModel.ZBCR;
                    this.PortField = 5;
                    break;
                case Enums.TerminalModel.CTR50:
                    this.BarcodeModelField = Enums.BarcodeModel.Opticon_M11; //Multi-read
                    this.MultiReadField = true;
                    break;
                default:
                    break;
            }
        }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }


        [XmlElement("BarcodeModel")]
        public Enums.BarcodeModel BarcodeModel
        {
            get { return this.BarcodeModelField; }
            set { this.BarcodeModelField = value; }
        }

        [XmlElement("Port")]
        public int Port
        {
            get { return this.PortField; }
            set { this.PortField = value; }
        }

        [XmlElement("Baud")]
        public int Baud
        {
            get { return this.BaudField; }
            set { this.BaudField = value; }
        }

        [XmlElement("MultiRead")]
        public bool MultiRead
        {
            get { return this.MultiReadField; }
            set { this.MultiReadField = value; }
        }

        #endregion Properties
    }

    [Serializable()]
    public class PRTconfig
    {
        private bool EnableField = true;
        private bool RequiredField = false;
        private Enums.PrinterModel PrinterNameField = Enums.PrinterModel.BK_C310;
        private string InternalPrinterNameField = "";
        private string LogoFileNameField = "LogoTicket.png";
        //[XmlAnyElement("OnLogoFileName")]
        //public XmlComment VersionComment { get { return new XmlDocument().CreateComment("LogoFileName values: LogoProsegur.png | LogoMacro.png"); } set { } }
        private string FontTypeField = "Lucida Console";
        //private float FontSizeField = 9;
        private float FontSizeField;
        private bool BoldActiveField;
        private Coordinates LogoConfigField;
        private Coordinates BodyConfigField;
        private Coordinates BarcodeConfigField;
        private Coordinates QrConfigField;
        private bool IncludeBarcodeLabelField = true;
        private bool UsePrintSpoolerField = false;

        public PRTconfig() { }

        public PRTconfig(Enums.TerminalModel terminalModel)
        {
            LogoConfigField = new Coordinates();
            LogoConfigField.Y = 10;
            LogoConfigField.Width = 112;
            LogoConfigField.Height = 50;
            LogoConfigField.AddLineFeeds = 0;

            BodyConfigField = new Coordinates();
            BodyConfigField.Y = 60;
            BodyConfigField.Width = 400;
            BodyConfigField.Height = 1600;
            BodyConfigField.AddLineFeeds = 0;

            BarcodeConfigField = new Coordinates();
            BarcodeConfigField.X = 65;
            BarcodeConfigField.Y = 15;
            BarcodeConfigField.Width = 150;
            BarcodeConfigField.Height = 60;
            BarcodeConfigField.AddLineFeeds = 5;

            QrConfigField = new Coordinates();
            QrConfigField.X = 65;
            QrConfigField.Y = 15;
            QrConfigField.Width = 150;
            QrConfigField.Height = 150;
            QrConfigField.AddLineFeeds = 5;

            switch (terminalModel)
            {
                case Enums.TerminalModel.SNBC_CTI90:
                case Enums.TerminalModel.SNBC_CTE1:
                case Enums.TerminalModel.CTIUL:
                case Enums.TerminalModel.MADERO_BR:
                case Enums.TerminalModel.SNBC_IN:
                    PrinterName = Enums.PrinterModel.BK_C310;
                    LogoConfigField.X = 95;
                    BodyConfigField.X = 45;
                    BarcodeConfigField.X = 65;
                    this.FontSizeField = 7;
                    this.BoldActiveField = false;
                    break;
                case Enums.TerminalModel.Glory_DE_50:
                case Enums.TerminalModel.TAS1:
                    PrinterName = Enums.PrinterModel.NII_Printer_DS;
                    LogoConfigField.X = 50;
                    BodyConfigField.X = 0;
                    BarcodeConfigField.X = 20;
                    this.FontSizeField = 6;
                    this.BoldActiveField = true;
                    break;
                case Enums.TerminalModel.BDM_300:
                    PrinterName = Enums.PrinterModel.SPRT;
                    LogoConfigField.X = 50;
                    BodyConfigField.X = 0;
                    BarcodeConfigField.X = 20;
                    this.FontSizeField = 6;
                    this.BoldActiveField = true;
                    break;
                case Enums.TerminalModel.MiniBank_JH6000_D:
                case Enums.TerminalModel.MiniBank_JH600_A:
                    PrinterName = Enums.PrinterModel.PLUSII;
                    LogoConfigField.X = 50;
                    BodyConfigField.X = 0;
                    BarcodeConfigField.X = 20;
                    this.FontSizeField = 6;
                    this.BoldActiveField = true;
                    break;
                case Enums.TerminalModel.GRG_P2600:
                    PrinterName = Enums.PrinterModel.XFS;
                    break;
                case Enums.TerminalModel.Depositario:
                    PrinterName = Enums.PrinterModel.POS_80;
                    LogoConfigField.X = 50;
                    BodyConfigField.X = 0;
                    BarcodeConfigField.X = 20;
                    this.FontSizeField = 7;
                    this.BoldActiveField = false;
                    break;
                case Enums.TerminalModel.CTR50:
                    PrinterName = Enums.PrinterModel.BK_C310;
                    LogoConfigField.X = 50;
                    BodyConfigField.X = 0;
                    BarcodeConfigField.X = 20;
                    this.FontSizeField = 7;
                    this.BoldActiveField = false;
                    break;
            }
        }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("InternalPrinterName")]
        public string InternalPrinterName
        {
            get { return this.InternalPrinterNameField; }
            set { this.InternalPrinterNameField = value; }
        }

        [XmlElement("PrinterName")]
        public Enums.PrinterModel PrinterName
        {
            get { return this.PrinterNameField; }
            set { this.PrinterNameField = value; }
        }

        [XmlElement("LogoFileName")]
        public string LogoFileName
        {
            get { return this.LogoFileNameField; }
            set { this.LogoFileNameField = value; }
        }

        [XmlElement("FontType")]
        public string FontType
        {
            get { return this.FontTypeField; }
            set { this.FontTypeField = value; }
        }

        [XmlElement("FontSize")]
        public float FontSize
        {
            get { return this.FontSizeField; }
            set { this.FontSizeField = value; }
        }

        [XmlElement("BoldActive")]
        public bool BoldActive
        {
            get { return this.BoldActiveField; }
            set { this.BoldActiveField = value; }
        }

        [XmlElement("LogoConfig")]
        public Coordinates LogoConfig
        {
            get { return this.LogoConfigField; }
            set { this.LogoConfigField = value; }
        }

        [XmlElement("BodyConfig")]
        public Coordinates BodyConfig
        {
            get { return this.BodyConfigField; }
            set { this.BodyConfigField = value; }
        }

        [XmlElement("BarcodeConfig")]
        public Coordinates BarcodeConfig
        {
            get { return this.BarcodeConfigField; }
            set { this.BarcodeConfigField = value; }
        }

        [XmlElement("QrConfig")]
        public Coordinates QrConfig
        {
            get { return this.QrConfigField; }
            set { this.QrConfigField = value; }
        }

        [XmlElement("IncludeBarcodeLabel")]
        public bool IncludeBarcodeLabel
        {
            get { return this.IncludeBarcodeLabelField; }
            set { this.IncludeBarcodeLabelField = value; }
        }

        [XmlElement("UsePrintSpooler")]
        public bool UsePrintSpooler
        {
            get { return this.UsePrintSpoolerField; }
            set { this.UsePrintSpoolerField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class STM_1config
    {
        private string PrintFileNameField = @"C:\AlephATM\PrintOut\Document.pdf";
        private string PrinterNameField = "Brother HL-L2320D series";
        private string AppFileNameField = @"C:\Program Files (x86)\2Printer\2Printer.exe";

        public STM_1config() { }

        #region Properties
        [XmlElement("PrintFileName")]
        public string PrintFileName
        {
            get { return this.PrintFileNameField; }
            set { this.PrintFileNameField = value; }
        }

        [XmlElement("PrinterName")]
        public string PrinterName
        {
            get { return this.PrinterNameField; }
            set { this.PrinterNameField = value; }
        }

        [XmlElement("AppFileName")]
        public string AppFileName
        {
            get { return this.AppFileNameField; }
            set { this.AppFileNameField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class Coordinates
    {
        private int XField;
        private int YField;
        private int WidthField;
        private int HeightField;
        private int AddLineFeedsField;

        public Coordinates() { }

        #region Properties
        [XmlElement("X")]
        public int X
        {
            get { return this.XField; }
            set { this.XField = value; }
        }

        [XmlElement("Y")]
        public int Y
        {
            get { return this.YField; }
            set { this.YField = value; }
        }

        [XmlElement("Width")]
        public int Width
        {
            get { return this.WidthField; }
            set { this.WidthField = value; }
        }

        [XmlElement("Height")]
        public int Height
        {
            get { return this.HeightField; }
            set { this.HeightField = value; }
        }

        [XmlElement("AddLineFeeds")]
        public int AddLineFeeds
        {
            get { return this.AddLineFeedsField; }
            set { this.AddLineFeedsField = value; }
        }
        #endregion Properties
    }

    [Serializable()]
    public class FPRconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;

        public FPRconfig() { }

        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }
    }


    [Serializable()]
    public class IOBoardConfig
    {
        private bool EnableField;
        private bool RequiredField = false;
        private Enums.IOBoardModel ModelField = Enums.IOBoardModel.AIO;
        private string PortField;
        private string BaudField = "38400";
        private string AutoDetectNameField = "Arduino,Arduino Uno,USB-SERIAL CH340";
        private int ChangesPerSecThresholdField = 5; //Cantidad máxima de camnbios de estado por segundo para pasar a estado de error
        private int RecoveryErrorTimerField = 900000; //15 minutes; //Espera para recuperar el estado de error 
        private int CombSensorPosField = 6;
        private int PreDoorSensorPosField = 5;
        private int UpperDoorSensorPosField = 4;
        private int PresenceSensorPosField = 3;
        private int CoverSensorPosField = 2;
        private int DoorSensorPosField = 1;
        private int LockSensorPosField = 0;
        private int ResponseTimerField = 5000;
        private bool EnableGuidelightsField = false;
        private string GuidelightDefaultColorField = "green";
        private string CashDepositSlotLedNameField = "LED1";
        private string CashDispenserLedNameField = "LED2";
        private string CoinDispenserLedNameField = "LED3";

        public IOBoardConfig() { }

        public IOBoardConfig(Enums.TerminalModel terminalModel)
        {
            switch (terminalModel)
            {
                case Enums.TerminalModel.Glory_DE_50:
                    this.EnableField = true;
                    this.ModelField = Enums.IOBoardModel.PicBoard;
                    this.PortField = "2";
                    break;
                case Enums.TerminalModel.SNBC_CTE1:
                case Enums.TerminalModel.SNBC_CTI90:
                case Enums.TerminalModel.TAS1:
                case Enums.TerminalModel.CTIUL:
                case Enums.TerminalModel.V200:
                case Enums.TerminalModel.Depositario:
                case Enums.TerminalModel.MADERO_BR:
                    this.EnableField = true;
                    this.ModelField = Enums.IOBoardModel.AIO;
                    this.PortField = "";//If it is empty, apply AutoDetectName
                    break;
                case Enums.TerminalModel.GRG_P2600:
                    this.EnableField = true;
                    this.ModelField = Enums.IOBoardModel.SIU;
                    break;
                case Enums.TerminalModel.BDM_300:
                    this.EnableField = false;
                    this.ModelField = Enums.IOBoardModel.NONE;
                    break;
                case Enums.TerminalModel.MiniBank_JH6000_D:
                case Enums.TerminalModel.MiniBank_JH600_A:
                case Enums.TerminalModel.ADM:
                case Enums.TerminalModel.SNBC_IN:
                    this.ModelField = Enums.IOBoardModel.NONE;
                    this.EnableField = false;
                    break;
                case Enums.TerminalModel.CTR50:
                    this.EnableField = true;
                    this.ModelField = Enums.IOBoardModel.BtLNX;
                    this.EnableGuidelightsField = true;
                    this.Port = "9";
                    this.Baud = "115200";
                    this.AutoDetectName = "Silicon Labs CP210x USB to UART Bridge";
                    break;
            }
        }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("Model")]
        public Enums.IOBoardModel Model
        {
            get { return this.ModelField; }
            set { this.ModelField = value; }
        }

        [XmlElement("Port")]
        public string Port
        {
            get { return this.PortField; }
            set { this.PortField = value; }
        }

        [XmlElement("Baud")]
        public string Baud
        {
            get { return this.BaudField; }
            set { this.BaudField = value; }
        }

        [XmlElement("AutoDetectName")]
        public string AutoDetectName
        {
            get { return this.AutoDetectNameField; }
            set { this.AutoDetectNameField = value; }
        }

        [XmlElement("ChangesPerSecThreshold")]
        public int ChangesPerSecThreshold
        {
            get { return this.ChangesPerSecThresholdField; }
            set { this.ChangesPerSecThresholdField = value; }
        }

        [XmlElement("RecoveryErrorTimer")]
        public int RecoveryErrorTimer
        {
            get { return this.RecoveryErrorTimerField; }
            set { this.RecoveryErrorTimerField = value; }
        }

        [XmlElement("CombSensorPos")]
        public int CombSensorPos
        {
            get { return this.CombSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"CombSensorPos\" must be more than 0 and less than 8.");
                }
                this.CombSensorPosField = value;
            }
        }

        [XmlElement("PreDoorSensorPos")]
        public int PreDoorSensorPos
        {
            get { return this.PreDoorSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"PreDoorSensorPos\" must be more than 0 and less than 8.");
                }
                this.PreDoorSensorPosField = value;
            }
        }

        [XmlElement("PresenceSensorPos")]
        public int PresenceSensorPos
        {
            get { return this.PresenceSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"PresenceSensorPos\" must be more than 0 and less than 8.");
                }
                this.PresenceSensorPosField = value;
            }
        }

        [XmlElement("UpperDoorSensorPos")]
        public int UpperDoorSensorPos
        {
            get { return this.UpperDoorSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"UpperDoorSensorPos\" must be more than 0 and less than 8.");
                }
                this.UpperDoorSensorPosField = value;
            }
        }

        [XmlElement("CoverSensorPos")]
        public int CoverSensorPos
        {
            get { return this.CoverSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"CoverSensorPos\" must be more than 0 and less than 8.");
                }
                this.CoverSensorPosField = value;
            }
        }

        [XmlElement("DoorSensorPos")]
        public int DoorSensorPos
        {
            get { return this.DoorSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"DoorSensorPos\" must be more than 0 and less than 8.");
                }
                this.DoorSensorPosField = value;
            }
        }

        [XmlElement("LockSensorPos")]
        public int LockSensorPos
        {
            get { return this.LockSensorPosField; }
            set
            {
                if (value > 7 || value < 0)
                {
                    throw new Exception("Parameter \"LockSensorPos\" must be more than 0 and less than 8.");
                }
                this.LockSensorPosField = value;
            }
        }

        [XmlElement("ResponseTimer")]
        public int ResponseTimer
        {
            get { return this.ResponseTimerField; }
            set
            {
                this.ResponseTimerField = value;
            }
        }

        [XmlElement("EnableGuidelights")]
        public bool EnableGuidelights
        {
            get { return this.EnableGuidelightsField; }
            set { this.EnableGuidelightsField = value; }
        }
        [XmlElement("GuidelightDefaultColor")]
        public string GuidelightDefaultColor
        {
            get { return this.GuidelightDefaultColorField; }
            set
            {
                this.GuidelightDefaultColorField = value;
            }
        }
        [XmlElement("CashDepositSlotLedName")]
        public string CashDepositSlotLedName
        {
            get { return this.CashDepositSlotLedNameField; }
            set
            {
                this.CashDepositSlotLedNameField = value;
            }
        }
        [XmlElement("CashDispenserLedName")]
        public string CashDispenserLedName
        {
            get { return this.CashDispenserLedNameField; }
            set
            {
                this.CashDispenserLedNameField = value;
            }
        }
        [XmlElement("CoinDispenserLedName")]
        public string CoinDispenserLedName
        {
            get { return this.CoinDispenserLedNameField; }
            set
            {
                this.CoinDispenserLedNameField = value;
            }
        }
        #endregion Properties
    }

    [Serializable()]
    public class ADMconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;
        private string PortField = "5";
        private string BaudField = "38400";
        private string AutoDetectNameField = "";
        private uint TimeOutStatusField = 50;
        private uint TimeOutCapabilitiesField = 50;
        private uint TimeOutDispenseField = 7000;
        private uint TimeOutAsyncDispenseField = 7000;
        private uint TimeOutLoadField = 7000;

        public ADMconfig() { }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("Port")]
        public string Port
        {
            get { return this.PortField; }
            set { this.PortField = value; }
        }

        [XmlElement("Baud")]
        public string Baud
        {
            get { return this.BaudField; }
            set { this.BaudField = value; }
        }

        [XmlElement("AutoDetectName")]
        public string AutoDetectName
        {
            get { return this.AutoDetectNameField; }
            set { this.AutoDetectNameField = value; }
        }

        [XmlElement("TimeOutStatus")]
        public uint TimeOutStatus
        {
            get { return this.TimeOutStatusField; }
            set { this.TimeOutStatusField = value; }
        }

        [XmlElement("TimeOutCapabilities")]
        public uint TimeOutCapabilities
        {
            get { return this.TimeOutCapabilitiesField; }
            set { this.TimeOutCapabilitiesField = value; }
        }

        [XmlElement("TimeOutDispense")]
        public uint TimeOutDispense
        {
            get { return this.TimeOutDispenseField; }
            set { this.TimeOutDispenseField = value; }
        }

        [XmlElement("TimeOutAsyncDispense")]
        public uint TimeOutAsyncDispense
        {
            get { return this.TimeOutAsyncDispenseField; }
            set { this.TimeOutAsyncDispenseField = value; }
        }

        [XmlElement("TimeOutLoad")]
        public uint TimeOutLoad
        {
            get { return this.TimeOutLoadField; }
            set { this.TimeOutLoadField = value; }
        }
        #endregion Properties
    }


    [Serializable()]
    public class COINconfig
    {
        private bool EnableField = false;
        private bool RequiredField = false;
        private string PortField = "6";
        private string BaudField = "9600";
        private string AutoDetectNameField = "";
        private uint TimeOutStatusField = 3000;
        private uint TimeOutScanField = 1000;
        private uint TimeOutCapabilitiesField = 50;
        private uint TimeOutDispenseField = 7000;

        public COINconfig() { }

        #region Properties
        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("Port")]
        public string Port
        {
            get { return this.PortField; }
            set { this.PortField = value; }
        }

        [XmlElement("Baud")]
        public string Baud
        {
            get { return this.BaudField; }
            set { this.BaudField = value; }
        }

        [XmlElement("AutoDetectName")]
        public string AutoDetectName
        {
            get { return this.AutoDetectNameField; }
            set { this.AutoDetectNameField = value; }
        }

        [XmlElement("TimeOutStatus")]
        public uint TimeOutStatus
        {
            get { return this.TimeOutStatusField; }
            set { this.TimeOutStatusField = value; }
        }

        [XmlElement("TimeOutScan")]
        public uint TimeOutScan
        {
            get { return this.TimeOutScanField; }
            set { this.TimeOutScanField = value; }
        }

        [XmlElement("TimeOutCapabilities")]
        public uint TimeOutCapabilities
        {
            get { return this.TimeOutCapabilitiesField; }
            set { this.TimeOutCapabilitiesField = value; }
        }

        [XmlElement("TimeOutDispense")]
        public uint TimeOutDispense
        {
            get { return this.TimeOutDispenseField; }
            set { this.TimeOutDispenseField = value; }
        }

        #endregion Properties
    }


    [Serializable()]
    public class TERconfig
    {
        private bool EnableField = true;
        private bool RequiredField = false;
        private string KeyCertificateField = "433473685430643479";

        public TERconfig() { }

        [XmlElement("Enable")]
        public bool Enable
        {
            get { return this.EnableField; }
            set { this.EnableField = value; }
        }

        [XmlElement("Required")]
        public bool Required
        {
            get { return this.RequiredField; }
            set { this.RequiredField = value; }
        }

        [XmlElement("KeyCertificate")]
        public string KeyCertificate
        {
            get { return this.KeyCertificateField; }
            set { this.KeyCertificateField = value; }
        }
    }
}
