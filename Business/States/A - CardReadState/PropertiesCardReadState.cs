using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.CardReadState
{

    [Serializable()]
    public partial class PropertiesCardReadState : PropertiesState
    {

        private string screenNameField;
        private string goodReadNextStateField;
        private string errorScreenNumberField;
        private ReadCondition_Type readCondition1Field;
        private ReadCondition_Type readCondition2Field;
        private ReadCondition_Type readCondition3Field;
        private CardReturnFlag_Type cardReturnFlagField;
        private string noFitMatchNextStateField;
        private string keyMaskField;
        private string virtualTrackDataField;
        private bool cancelKeyboardEnabledField;
        private bool acceptNonMagneticCardField;
        private bool readOnCardRemovalField;
        private bool fallbackMagneticStripeField;
        private bool enableSupportQrCode;
        private string helpDeskChatURL;
        private string helpDeskPhone;
        private string helpDeskEmail;
        private PropertiesErrorScreens errorScreensField;
        private PropertiesContactlessCardReadOptions contactlessCardReadOptionsField;
        private JournalProperties journalField;
        [XmlAnyElement("OnWelcomeComment")]
        public XmlComment VersionComment { get { return new XmlDocument().CreateComment("HandlerName values: welcome.htm"); } set { } }

        public StateEvent OnWelcome;
        public StateEvent OnWelcomeOptions;

        public PropertiesCardReadState() { }

        public PropertiesCardReadState(AlephATMAppData alephATMAppData)
        {
            this.screenNameField = "";
            this.goodReadNextStateField = "";
            this.errorScreenNumberField = "";
            //this.readCondition1Field = new ReadCondition_Type();
            //this.readCondition2Field = new ReadCondition_Type();
            //this.readCondition3Field = new ReadCondition_Type();
            this.cardReturnFlagField = new CardReturnFlag_Type();
            this.cardReturnFlagField = CardReturnFlag_Type.none;
            this.noFitMatchNextStateField = "";
            this.keyMaskField = "008"; //Activo por defecto la FDK_D
            this.virtualTrackDataField = "589244507200000001=11171000000000000";
            this.cancelKeyboardEnabledField = true;
            this.acceptNonMagneticCardField = true;
            this.readOnCardRemovalField = false;
            this.fallbackMagneticStripeField = false;
            this.enableSupportQrCode = false;
            this.helpDeskChatURL = "";
            this.helpDeskPhone = "";
            this.helpDeskEmail = "";
            this.errorScreensField = new PropertiesErrorScreens();
            this.contactlessCardReadOptionsField = new PropertiesContactlessCardReadOptions();
            this.journalField = new JournalProperties();
            this.OnWelcome = new StateEvent(StateEvent.EventType.navigate, "welcome.htm", "");
            this.OnWelcomeOptions = new StateEvent(StateEvent.EventType.runScript, "welcomeOptions", "");
            switch (alephATMAppData.CountryId)
            {
                case "AR":
                    this.enableSupportQrCode = true;
                    this.helpDeskChatURL = "https://wa.me/541131930000";
                    this.helpDeskPhone = "+54 11 4585-8203";
                    this.helpDeskEmail = "soportetecnicocash@prosegur.com";                 
                    break;
                case "PY":
                    this.enableSupportQrCode = true;
                    this.helpDeskChatURL = "https://wa.me/595984599602";
                    this.helpDeskPhone = "+59 5 9845 99602";
                    this.helpDeskEmail = "soportetecnicocashPY@prosegur.com";
                    break;
                case "UY":
                    this.enableSupportQrCode = true;
                    this.helpDeskChatURL = "https://wa.me/59895552274";
                    this.helpDeskPhone = "+59 8 9555 2274";
                    this.helpDeskEmail = "soportetecnicocashUY@prosegur.com";
                    break;
                //case "CO":
                //    this.enableSupportQrCode = true;
                //    this.helpDeskChatURL = "";
                //    this.helpDeskPhone = "";
                //    this.helpDeskEmail = "";
                //    break;
                //case "IN":
                //    this.enableSupportQrCode = true;
                //    this.helpDeskChatURL = "";
                //    this.helpDeskPhone = "";
                //    this.helpDeskEmail = "";
                //    break;
                //case "BR":
                //    this.enableSupportQrCode = true;
                //    this.helpDeskChatURL = "";
                //    this.helpDeskPhone = "";
                //    this.helpDeskEmail = "";
                //    break;
            }
        }

        #region Properties
        [XmlElement()]
        public string ScreenName
        {
            get
            {
                return this.screenNameField;
            }
            set
            {
                this.screenNameField = value;
            }
        }

        [XmlElement()]
        public string GoodReadNextState
        {
            get
            {
                return this.goodReadNextStateField;
            }
            set
            {
                this.goodReadNextStateField = value;
            }
        }

        [XmlElement()]
        public string ErrorScreenNumber
        {
            get
            {
                return this.errorScreenNumberField;
            }
            set
            {
                this.errorScreenNumberField = value;
            }
        }

        [XmlElement()]
        public ReadCondition_Type ReadCondition1
        {
            get
            {
                return this.readCondition1Field;
            }
            set
            {
                this.readCondition1Field = value;
            }
        }

        [XmlElement()]
        public ReadCondition_Type ReadCondition2
        {
            get
            {
                return this.readCondition2Field;
            }
            set
            {
                this.readCondition2Field = value;
            }
        }

        [XmlElement()]
        public ReadCondition_Type ReadCondition3
        {
            get
            {
                return this.readCondition3Field;
            }
            set
            {
                this.readCondition3Field = value;
            }
        }

        [XmlElement()]
        public CardReturnFlag_Type CardReturnFlag
        {
            get
            {
                return this.cardReturnFlagField;
            }
            set
            {
                this.cardReturnFlagField = value;
            }
        }

        [XmlElement()]
        public string NoFitMatchNextState
        {
            get
            {
                return this.noFitMatchNextStateField;
            }
            set
            {
                this.noFitMatchNextStateField = value;
            }
        }

        [XmlElement()]
        public string KeyMask
        {
            get
            {
                return this.keyMaskField;
            }
            set
            {
                this.keyMaskField = value;
            }
        }

        [XmlElement()]
        public string VirtualTrackData
        {
            get
            {
                return this.virtualTrackDataField;
            }
            set
            {
                this.virtualTrackDataField = value;
            }
        }

        [XmlElement()]
        public bool CancelKeyboardEnabled
        {
            get
            {
                return this.cancelKeyboardEnabledField;
            }
            set
            {
                this.cancelKeyboardEnabledField = value;
            }
        }

        [XmlElement()]
        public bool AcceptNonMagneticCard
        {
            get
            {
                return this.acceptNonMagneticCardField;
            }
            set
            {
                this.acceptNonMagneticCardField = value;
            }
        }

        [XmlElement()]
        public bool ReadOnCardRemoval
        {
            get
            {
                return this.readOnCardRemovalField;
            }
            set
            {
                this.readOnCardRemovalField = value;
            }
        }

        [XmlElement()]
        public bool FallbackMagneticStripe
        {
            get
            {
                return this.fallbackMagneticStripeField;
            }
            set
            {
                this.fallbackMagneticStripeField = value;
            }
        }

        [XmlElement()]
        public bool EnableSupportQrCode
        {
            get
            {
                return this.enableSupportQrCode;
            }
            set
            {
                this.enableSupportQrCode = value;
            }
        }          
        
        [XmlElement()]
        public string HelpDeskChatURL
        {
            get
            {
                return this.helpDeskChatURL;
            }
            set
            {
                this.helpDeskChatURL = value;
            }
        }       

        [XmlElement()]
        public string HelpDeskPhone
        {
            get
            {
                return this.helpDeskPhone;
            }
            set
            {
                this.helpDeskPhone = value;
            }
        }     

        [XmlElement()]
        public string HelpDeskEmail
        {
            get
            {
                return this.helpDeskEmail;
            }
            set
            {
                this.helpDeskEmail = value;
            }
        }   

        [XmlElement()]
        public PropertiesErrorScreens ErrorScreens
        {
            get
            {
                return this.errorScreensField;
            }
            set
            {
                this.errorScreensField = value;
            }
        }

        [XmlElement()]
        public PropertiesContactlessCardReadOptions ContactlessCardReadOptions
        {
            get
            {
                return this.contactlessCardReadOptionsField;
            }
            set
            {
                this.contactlessCardReadOptionsField = value;
            }
        }

        [XmlElement()]
        public JournalProperties Journal
        {
            get
            {
                return this.journalField;
            }
            set
            {
                this.journalField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class PropertiesErrorScreens
    {
        private string cardReaderUnavailableField;
        private string cardReaderErrorField;
        private string cardWrongInsertedField;
        private string invalidCardField;
        private int timeShowErrorField;

        public PropertiesErrorScreens()
        {
            this.cardReaderUnavailableField = "";
            this.cardReaderErrorField = "";
            this.cardWrongInsertedField = "";
            this.invalidCardField = "";
            this.timeShowErrorField = 5;
        }

        [XmlElement()]
        public string CardReaderUnavailable
        {
            get
            {
                return this.cardReaderUnavailableField;
            }
            set
            {
                this.cardReaderUnavailableField = value;
            }
        }

        [XmlElement()]
        public string CardReaderError
        {
            get
            {
                return this.cardReaderErrorField;
            }
            set
            {
                this.cardReaderErrorField = value;
            }
        }

        [XmlElement()]
        public string CardWrongInserted
        {
            get
            {
                return this.cardWrongInsertedField;
            }
            set
            {
                this.cardWrongInsertedField = value;
            }
        }

        [XmlElement()]
        public string InvalidCard
        {
            get
            {
                return this.invalidCardField;
            }
            set
            {
                this.invalidCardField = value;
            }
        }

        [XmlElement()]
        public int TimeShowError
        {
            get
            {
                return this.timeShowErrorField;
            }
            set
            {
                this.timeShowErrorField = value;
            }
        }
    }

    [Serializable]
    public partial class PropertiesContactlessCardReadOptions
    {

        private bool enableContactlessCardReadField;

        private bool acceptNonMagneticCardsField;

        public PropertiesContactlessCardReadOptions()
        {
            this.enableContactlessCardReadField = false;
            this.acceptNonMagneticCardsField = true;
        }

        [XmlElement()]
        public bool EnableContactlessCardRead
        {
            get
            {
                return this.enableContactlessCardReadField;
            }
            set
            {
                this.enableContactlessCardReadField = value;
            }
        }

        [XmlElement()]
        public bool AcceptNonMagneticCards
        {
            get
            {
                return this.acceptNonMagneticCardsField;
            }
            set
            {
                this.acceptNonMagneticCardsField = value;
            }
        }
    }

    [Serializable]
    public partial class PropertiesJournal
    {

        private bool enableJournalField;
        private string cardInsertedFileNameField;
        private string cardWrongInsertedFileNameField;

        public PropertiesJournal()
        {
            this.enableJournalField = true;
            this.cardInsertedFileNameField = "CardInserted.txt";
            this.cardWrongInsertedFileNameField = "CardWrongInserted.txt";
        }

        [XmlElement()]
        public bool EnableJournal
        {
            get
            {
                return this.enableJournalField;
            }
            set
            {
                this.enableJournalField = value;
            }
        }

        [XmlElement()]
        public string CardInsertedFileName
        {
            get
            {
                return this.cardInsertedFileNameField;
            }
            set
            {
                this.cardInsertedFileNameField = value;
            }
        }

        [XmlElement()]
        public string CardWrongInsertedFileName
        {
            get
            {
                return this.cardWrongInsertedFileNameField;
            }
            set
            {
                this.cardWrongInsertedFileNameField = value;
            }
        }
    }
}