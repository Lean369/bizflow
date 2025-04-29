using System;
using System.IO;
using System.Windows.Forms;

namespace Entities
{
    [Serializable()]
    public class ScreenConfiguration
    {
        private Const.Resolution MainBrowserResolutionField;
        private bool MainBrowserTopMostField = false;
        private int MainBrowserPosXField = 0;
        private int MainBrowserPosYField = 0;
        private int RefreshIntervalField = 300;
        private Const.Resolution SecBrowserResolutionField;
        private bool SecBrowserEnableField = false;
        private bool SecBrowserTopMostField = false;
        private int SecBrowserPosXField = 0;
        private int SecBrowserPosYField = 0;
        private bool AdBannerEnableField = false;
        private int AdBannerHeightField = 0;
        private DockStyle AdBannerDockField;
        private bool Digit7aEnableField = false;
        private KeyboardEntryMode_Type keyboardEntryModeField = KeyboardEntryMode_Type.NumericKeyboardOnScreen;
        private int addPxRowPosAmountInputField = 3;
        private int addPxRowPosITRInputField = -20;
        private int addPxRowPosInfEntryInputField = 0;
        private int addPxRowPosPinEntryInputField = -20;
        private bool urlScreenEnableField = false;
        private string urlScreenField = "http://localhost/";

        public ScreenConfiguration() { }

        public ScreenConfiguration(AlephATMAppData alephATMAppData)
        {
            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.SNBC_CTI90:
                case Enums.TerminalModel.SNBC_CTE1:
                case Enums.TerminalModel.MADERO_BR:
                    this.MainBrowserResolutionField = Const.Resolution.R1024x600;
                    break;
                case Enums.TerminalModel.Glory_DE_50:
                case Enums.TerminalModel.Depositario:
                    this.MainBrowserResolutionField = Const.Resolution.R1024x768;
                    if (alephATMAppData.Branding == Enums.Branding.Atlas
                        || alephATMAppData.Branding == Enums.Branding.GNB
                        || alephATMAppData.Branding == Enums.Branding.FIC)
                    {
                        this.SecBrowserEnableField = true;
                        this.SecBrowserResolutionField = Const.Resolution.R1024x768;
                    }
                    break;
                case Enums.TerminalModel.MiniBank_JH6000_D:
                case Enums.TerminalModel.MiniBank_JH600_A:
                case Enums.TerminalModel.BDM_300:
                    this.MainBrowserResolutionField = Const.Resolution.R800x480;
                    break;
                case Enums.TerminalModel.ADM:
                    this.MainBrowserResolutionField = Const.Resolution.R768x1366;
                    this.SecBrowserEnableField = true;
                    this.SecBrowserResolutionField = Const.Resolution.R768x1366;
                    break;
                case Enums.TerminalModel.CTR50:
                    this.MainBrowserResolutionField = Const.Resolution.R768x1366;
                    this.SecBrowserEnableField = true;
                    this.SecBrowserResolutionField = Const.Resolution.R768x1366;
                    this.AdBannerEnableField = true;
                    this.AdBannerHeightField = 347;
                    this.AdBannerDockField = DockStyle.Top;
                    break;
                case Enums.TerminalModel.GRG_P2600:
                    this.MainBrowserResolutionField = Const.Resolution.R1024x768;
                    break;
                case Enums.TerminalModel.TAS1:
                    this.MainBrowserResolutionField = Const.Resolution.R1280x1024;
                    break;
                case Enums.TerminalModel.CTIUL:
                case Enums.TerminalModel.V200:
                    this.MainBrowserResolutionField = Const.Resolution.R600x1024;
                    break;
                case Enums.TerminalModel.SNBC_IN:
                    this.MainBrowserResolutionField = Const.Resolution.R1366x768;
                    break;
                default:
                    this.MainBrowserResolutionField = Const.Resolution.R1024x600;
                    break;
            }
        }
        public static bool GetScreenConfiguration(out ScreenConfiguration screenConfiguration, AlephATMAppData alephATMAppData)
        {
            bool ret = false;
            string fileName = $"{Const.appPath}Config\\ScreenConfiguration.xml";
            try
            {
                if (!Directory.Exists($"{Const.appPath}\\Config"))
                {
                    Directory.CreateDirectory($"{Const.appPath}\\Config");
                }
                screenConfiguration = new ScreenConfiguration(alephATMAppData);
                screenConfiguration = Utilities.Utils.GetGenericXmlData<ScreenConfiguration>(out ret, fileName, screenConfiguration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in {fileName} file. {ex.Message} - {ex.InnerException}");
            }
            return ret;
        }

        #region "Properties"

        [System.Xml.Serialization.XmlElement("MainBrowserResolution")]
        public Entities.Const.Resolution MainBrowserResolution
        {
            get { return this.MainBrowserResolutionField; }
            set { this.MainBrowserResolutionField = value; }
        }

        [System.Xml.Serialization.XmlElement("MainBrowserTopMost")]
        public bool MainBrowserTopMost
        {
            get { return this.MainBrowserTopMostField; }
            set { this.MainBrowserTopMostField = value; }
        }

        [System.Xml.Serialization.XmlElement("MainBrowserPosX")]
        public int MainBrowserPosX
        {
            get { return this.MainBrowserPosXField; }
            set { this.MainBrowserPosXField = value; }
        }

        [System.Xml.Serialization.XmlElement("MainBrowserPosY")]
        public int MainBrowserPosY
        {
            get { return this.MainBrowserPosYField; }
            set { this.MainBrowserPosYField = value; }
        }

        [System.Xml.Serialization.XmlElement("RefreshInterval")]
        public int RefreshInterval
        {
            get { return this.RefreshIntervalField; }
            set { this.RefreshIntervalField = value; }
        }

        [System.Xml.Serialization.XmlElement("SecBrowserResolution")]
        public Entities.Const.Resolution SecBrowserResolution
        {
            get { return this.SecBrowserResolutionField; }
            set { this.SecBrowserResolutionField = value; }
        }

        [System.Xml.Serialization.XmlElement("SecBrowserEnable")]
        public bool SecBrowserEnable
        {
            get { return this.SecBrowserEnableField; }
            set { this.SecBrowserEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("SecBrowserTopMost")]
        public bool SecBrowserTopMost
        {
            get { return this.SecBrowserTopMostField; }
            set { this.SecBrowserTopMostField = value; }
        }

        [System.Xml.Serialization.XmlElement("SecBrowserPosX")]
        public int SecBrowserPosX
        {
            get { return this.SecBrowserPosXField; }
            set { this.SecBrowserPosXField = value; }
        }

        [System.Xml.Serialization.XmlElement("SecBrowserPosY")]
        public int SecBrowserPosY
        {
            get { return this.SecBrowserPosYField; }
            set { this.SecBrowserPosYField = value; }
        }

        [System.Xml.Serialization.XmlElement("AdBannerEnable")]
        public bool AdBannerEnable
        {
            get { return this.AdBannerEnableField; }
            set { this.AdBannerEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("AdBannerHeight")]
        public int AdBannerHeight
        {
            get { return this.AdBannerHeightField; }
            set { this.AdBannerHeightField = value; }
        }

        [System.Xml.Serialization.XmlElement("AdBannerDock")]
        public DockStyle AdBannerDock
        {
            get { return this.AdBannerDockField; }
            set { this.AdBannerDockField = value; }
        }

        [System.Xml.Serialization.XmlElement("Digit7aEnable")]
        public bool Digit7aEnable
        {
            get { return this.Digit7aEnableField; }
            set { this.Digit7aEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("KeyboardEntryMode")]
        public KeyboardEntryMode_Type KeyboardEntryMode
        {
            get { return this.keyboardEntryModeField; }
            set { this.keyboardEntryModeField = value; }
        }

        [System.Xml.Serialization.XmlElement("AddPxRowPosAmountInput")]
        public int AddPxRowPosAmountInput
        {
            get { return this.addPxRowPosAmountInputField; }
            set { this.addPxRowPosAmountInputField = value; }
        }

        [System.Xml.Serialization.XmlElement("AddPxRowPosITRInput")]
        public int AddPxRowPosITRInput
        {
            get { return this.addPxRowPosITRInputField; }
            set { this.addPxRowPosITRInputField = value; }
        }

        [System.Xml.Serialization.XmlElement("AddPxRowPosInfEntryInput")]
        public int AddPxRowPosInfEntryInput
        {
            get { return this.addPxRowPosInfEntryInputField; }
            set { this.addPxRowPosInfEntryInputField = value; }
        }

        [System.Xml.Serialization.XmlElement("AddPxRowPosPinEntryInput")]
        public int AddPxRowPosPinEntryInput
        {
            get { return this.addPxRowPosPinEntryInputField; }
            set { this.addPxRowPosPinEntryInputField = value; }
        }

        [System.Xml.Serialization.XmlElement("UrlScreenEnable")]
        public bool UrlScreenEnable
        {
            get { return this.urlScreenEnableField; }
            set { this.urlScreenEnableField = value; }
        }

        [System.Xml.Serialization.XmlElement("UrlScreen")]
        public string UrlScreen
        {
            get { return this.urlScreenField; }
            set { this.urlScreenField = value; }
        }

        #endregion "Properties"

    }
}
