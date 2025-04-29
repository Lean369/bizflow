using System;
using System.Xml;
using System.Xml.Serialization;
using Business.States;
using Entities;

namespace Business.BarcodeReadState
{
    [Serializable]
    public partial class PropertiesBarcodeReadState : PropertiesState
    {
        private string screenNumberField;
        private string goodBarcodeReadStateNumberField;
        private string cancelNextStateNumberField;
        private string errorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private BarcodeDataDestination_Type barcodeDataDestinationField;
        private bool showConfirmBarcodesField = true;
        private string activeCancelFDKKeyMaskField;
        private Extension extensionField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnBarcodeReadConfig;
        public StateEvent OnPleaseWait;
        public StateEvent OnBarcodeChecks;
        public StateEvent OnWrongBarcode;
        private string multipleCodesSeparatorField;
        private string bankCodesSeparatorField;
        private Coordinates CoordChannel_1Field;
        private Coordinates CoordChannel_2Field;
        private Coordinates CoordChannel_3Field;
        private string[] DepositPrefixesField = new string[]
        {
            "1EQ", "1DZ", "1DO", "1PB", "1DL", "1BD", "1DS", "1HS", "1DI", "1DT", "1DV", "1DB"
        };

        private string[] WithdrawalPrefixesField = new string[]
        {
            "1CB", "1RC", "1RB", "1RI", "1RS", "1RH", "1RA", "1RL", "1RF", "1EO", "1WZ", "1ER", "1VR", "1GI"
        };

        public PropertiesBarcodeReadState() { }

        public PropertiesBarcodeReadState(AlephATMAppData alephATMAppData)
        {
            this.screenNumberField = "";
            this.goodBarcodeReadStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.errorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.activeCancelFDKKeyMaskField = "";
            this.extensionField = new Extension();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "barcodeRead.htm", "");
            this.OnBarcodeReadConfig = new StateEvent(StateEvent.EventType.runScript, "barcodeRead", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnBarcodeChecks = new StateEvent(StateEvent.EventType.runScript, "barcodeChecks", "");
            this.OnWrongBarcode = new StateEvent(StateEvent.EventType.runScript, "wrongBarcode", "");
            this.multipleCodesSeparatorField = "|";
            this.bankCodesSeparatorField = "!";
            switch(alephATMAppData.Branding) 
            {
                case Enums.Branding.Coto:
                    this.CoordChannel_1Field = new Coordinates(0, 4);
                    this.CoordChannel_2Field = new Coordinates(12, 4);
                    this.CoordChannel_3Field = new Coordinates(4, 8);
                    this.barcodeDataDestinationField = BarcodeDataDestination_Type.ChannelsA;
                    this.EnableState = true;
                    break;
                case Enums.Branding.RedPagosA:
                    this.barcodeDataDestinationField = BarcodeDataDestination_Type.none;
                    this.EnableState = true;
                    break;
                default:
                    this.barcodeDataDestinationField = BarcodeDataDestination_Type.none;
                    this.EnableState = false;
                    break;
            }   
        }

        #region "Properties"
        [XmlElement()]
        public string ScreenNumber
        {
            get
            {
                return this.screenNumberField;
            }
            set
            {
                this.screenNumberField = value;
            }
        }

        [XmlElement()]
        public string GoodBarcodeReadStateNumber
        {
            get
            {
                return this.goodBarcodeReadStateNumberField;
            }
            set
            {
                this.goodBarcodeReadStateNumberField = value;
            }
        }

        [XmlElement()]
        public string CancelNextStateNumber
        {
            get
            {
                return this.cancelNextStateNumberField;
            }
            set
            {
                this.cancelNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string ErrorNextStateNumber
        {
            get
            {
                return this.errorNextStateNumberField;
            }
            set
            {
                this.errorNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string TimeoutNextStateNumber
        {
            get
            {
                return this.timeoutNextStateNumberField;
            }
            set
            {
                this.timeoutNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string MultipleCodesSeparator
        {
            get
            {
                return this.multipleCodesSeparatorField;
            }
            set
            {
                this.multipleCodesSeparatorField = value;
            }
        }

        [XmlElement()]
        public string BankCodesSeparator
        {
            get
            {
                return this.bankCodesSeparatorField;
            }
            set
            {
                this.bankCodesSeparatorField = value;
            }
        }

        [XmlElement()]
        public BarcodeDataDestination_Type BarcodeDataDestination
        {
            get
            {
                return this.barcodeDataDestinationField;
            }
            set
            {
                this.barcodeDataDestinationField = value;
            }
        }

        [XmlElement()]
        public bool ShowConfirmBarcodes
        {
            get
            {
                return this.showConfirmBarcodesField;
            }
            set
            {
                this.showConfirmBarcodesField = value;
            }
        }

        [XmlElement()]
        public string ActiveCancelFDKKeyMask
        {
            get
            {
                return this.activeCancelFDKKeyMaskField;
            }
            set
            {
                this.activeCancelFDKKeyMaskField = value;
            }
        }

        [XmlElement()]
        public Extension Extension
        {
            get
            {
                return this.extensionField;
            }
            set
            {
                this.extensionField = value;
            }
        }

        [XmlElement()]
        public Coordinates CoordChannel_1
        {
            get
            {
                return this.CoordChannel_1Field;
            }
            set
            {
                this.CoordChannel_1Field = value;
            }
        }
        [XmlElement()]
        public Coordinates CoordChannel_2
        {
            get
            {
                return this.CoordChannel_2Field;
            }
            set
            {
                this.CoordChannel_2Field = value;
            }
        }

        [XmlElement()]
        public Coordinates CoordChannel_3
        {
            get
            {
                return this.CoordChannel_3Field;
            }
            set
            {
                this.CoordChannel_3Field = value;
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

        [XmlElement()]
        public MoreTimeProperties MoreTime
        {
            get
            {
                return this.moreTimeField;
            }
            set
            {
                this.moreTimeField = value;
            }
        }

        [XmlArray("DepositPrefixes")]
        [XmlArrayItem("Prefix")]
        public string[] DepositPrefixes
        {
            get { return this.DepositPrefixesField; }
            set { this.DepositPrefixesField = value; }
        }

        [XmlArray("WithdrawalPrefixes")]
        [XmlArrayItem("Prefix")]
        public string[] WithdrawalPrefixes
        {
            get { return this.WithdrawalPrefixesField; }
            set { this.WithdrawalPrefixesField = value; }
        }

        #endregion Properties
    }

    [Serializable()]
    public class Coordinates
    {
        private int iniPosField;
        private int lengthField;
        public Coordinates() { }

        public Coordinates(int _iniPos, int _length)
        {
            this.iniPosField = _iniPos;
            this.lengthField = _length;
        }

        #region Properties
        [XmlElement("iniPos")]
        public int iniPos
        {
            get { return this.iniPosField; }
            set { this.iniPosField = value; }
        }

        [XmlElement("length")]
        public int length
        {
            get { return this.lengthField; }
            set { this.lengthField = value; }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension
    {
        private string stateNumberField;
        private EmptyElement_Type itemField;
        private EmptyElement_Type item1Field;
        private EmptyElement_Type item2Field;
        private EmptyElement_Type item3Field;
        private EmptyElement_Type item4Field;
        private EmptyElement_Type item5Field;
        private EmptyElement_Type item6Field;
        private object item7Field;

        /// <remarks/>
        public string StateNumber
        {
            get
            {
                return this.stateNumberField;
            }
            set
            {
                this.stateNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry2")]
        public EmptyElement_Type Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry3")]
        public EmptyElement_Type Item1
        {
            get
            {
                return this.item1Field;
            }
            set
            {
                this.item1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry4")]
        public EmptyElement_Type Item2
        {
            get
            {
                return this.item2Field;
            }
            set
            {
                this.item2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry5")]
        public EmptyElement_Type Item3
        {
            get
            {
                return this.item3Field;
            }
            set
            {
                this.item3Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry6")]
        public EmptyElement_Type Item4
        {
            get
            {
                return this.item4Field;
            }
            set
            {
                this.item4Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry7")]
        public EmptyElement_Type Item5
        {
            get
            {
                return this.item5Field;
            }
            set
            {
                this.item5Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BarcodeReadStateTableReservedEntry8")]
        public EmptyElement_Type Item6
        {
            get
            {
                return this.item6Field;
            }
            set
            {
                this.item6Field = value;
            }
        }
    }
}