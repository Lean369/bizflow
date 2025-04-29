using Business.States;
using Entities;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Business.SetExtraDataState
{
    [Serializable]
    public partial class PropertiesSetExtraDataState
    {
        private string screenNumberField;
        private string setGoodNextStateNumberField;
        private string setAuxiliaryNextStateNumberField;
        private string cancelNextStateNumberField;
        private string hardwareErrorNextStateNumberField;
        private string timeOutNextStateNumberField;
        private string item1Field;
        private string screenTitleField;
        private bool clearDataOnExitField = true; //new item
        private Extension1 extension1Field;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        [XmlAnyElement("OnSetExtraDataComment")]
        public XmlComment VersionComment { get { return new XmlDocument().CreateComment("HandlerName values: setExtraData.htm | setExtraData-autocity.htm | setExtraData-macro.htm | setExtraData-coto.htm"); } set { } }
        public StateEvent OnShowSetExtraDataScreen;
        public StateEvent OnConfigurationScreenData;
        private List<ExtraDataConf> ConfigurationsField;

        public PropertiesSetExtraDataState()
        {
            this.extension1Field = new Extension1();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnShowSetExtraDataScreen = new StateEvent(StateEvent.EventType.navigate, "setExtraData.htm", "");
            this.OnConfigurationScreenData = new StateEvent(StateEvent.EventType.runScript, "ConfigurationScreenData", "");
            this.ConfigurationsField = new List<ExtraDataConf>();
            this.screenTitleField = "";
        }

        internal void LoadDefaultConfiguration(AlephATMAppData alephATMAppData)
        {
            bool currSel = false;
            string[] currencies = new string[0];
            if ((alephATMAppData.TerminalModel == Enums.TerminalModel.SNBC_CTI90 ||
                alephATMAppData.TerminalModel == Enums.TerminalModel.SNBC_CTE1 ||
                alephATMAppData.TerminalModel == Enums.TerminalModel.GRG_P2600 ||
                alephATMAppData.TerminalModel == Enums.TerminalModel.Depositario ||
                alephATMAppData.TerminalModel == Enums.TerminalModel.CTIUL) && (
                alephATMAppData.DefaultCurrency.Equals("ARS") ||
                alephATMAppData.DefaultCurrency.Equals("PYG") ||
                alephATMAppData.DefaultCurrency.Equals("UYU")))
            {
                currSel = true;
                currencies = new string[] { alephATMAppData.DefaultCurrency, "USD", "EUR" };
            }
            //Para el correcto funcionamiento de las configuraciones es importante que el orden de los elementos en la lista coincida con el índice.
            //Channel settings
            switch (alephATMAppData.Branding)
            {
                case Enums.Branding.Coto:
                    this.ConfigurationsField.Add(new ExtraDataConf(2, Enums.ExtraDataType.channel, true, "channel1", "TERMINAL", true, "input", "numeric", 4, 4, new string[] { }, "", true, true));
                    this.ConfigurationsField.Add(new ExtraDataConf(3, Enums.ExtraDataType.channel, true, "channel2", "LEGAJO", true, "input", "numeric", 8, 8, new string[] { }, "", true, true));
                    this.ConfigurationsField.Add(new ExtraDataConf(4, Enums.ExtraDataType.channel, true, "channel3", "TRANSACCION", true, "input", "numeric", 4, 4, new string[] { }, "", true, true));
                    break;
                //case Enums.Branding.Macro:
                //    this.ConfigurationsField.Add(new ExtraDataConf(2, Enums.ExtraDataType.channel, true, "channel1", "channel1", "input", "numeric", 0, 10, new string[] { }));
                //    this.ConfigurationsField.Add(new ExtraDataConf(3, Enums.ExtraDataType.channel, true, "channel2", "channel2", "input", "numeric", 0, 10, new string[] { }));
                //    this.ConfigurationsField.Add(new ExtraDataConf(4, Enums.ExtraDataType.channel, false, "channel3", "channel3", "input", "numeric", 0, 10, new string[] { }));
                //    break;
                //case Enums.Branding.RedPagosA:
                //    this.ConfigurationsField.Add(new ExtraDataConf(2, Enums.ExtraDataType.channel, false, "channel1", "Ingrese número de teléfono", true, "input", "numeric", 8, 8, new string[] { }));
                //    this.ConfigurationsField.Add(new ExtraDataConf(3, Enums.ExtraDataType.channel, false, "channel2", "channel2", true, "input", "numeric", 0, 10, new string[] { }));
                //    this.ConfigurationsField.Add(new ExtraDataConf(4, Enums.ExtraDataType.channel, false, "channel3", "channel3", true, "input", "numeric", 0, 10, new string[] { }));
                //    break;
                case Enums.Branding.FIC:
                case Enums.Branding.Atlas:
                    currSel = false;
                    currencies = new string[] { alephATMAppData.DefaultCurrency };
                    // this.ConfigurationsField.Add(new ExtraDataConf(2, Enums.ExtraDataType.channel, true, "channel1", "Tipo de documento", true, "select", "", 0, 0, new string[] { "CI", "RUC", "CIE", "CRC", "CRP", "CRT", "DNC", "DNI", "PAS" }));
                    //this.ConfigurationsField.Add(new ExtraDataConf(3, Enums.ExtraDataType.channel, true, "channel2", "Número de documento", true, "input", "numeric", 4, 16, new string[] { }));
                    break;
                default:
                    this.ConfigurationsField.Add(new ExtraDataConf(2, Enums.ExtraDataType.channel, false, "channel1", "channel1", true, "select", "", 0, 0, new string[] { "AVANT", "RUBIC", "TAGLE", "NIX", "MOTCOR", "MOTOLIFE" }));
                    this.ConfigurationsField.Add(new ExtraDataConf(3, Enums.ExtraDataType.channel, false, "channel2", "channel2", true, "input", "numeric", 0, 10, new string[] { }));
                    this.ConfigurationsField.Add(new ExtraDataConf(4, Enums.ExtraDataType.channel, false, "channel3", "channel3", true, "input", "numeric", 0, 10, new string[] { }));
                    break;
            }
            this.ConfigurationsField.Add(new ExtraDataConf(5, Enums.ExtraDataType.txInfo, false, "transactionInfo", "transactionInfo", true, "input", "text", 0, 20, new string[] { }));
            this.ConfigurationsField.Add(new ExtraDataConf(6, Enums.ExtraDataType.txRef, false, "transactionRef", "transactionRef", true, "input", "numeric", 0, 20, new string[] { }));
            this.ConfigurationsField.Add(new ExtraDataConf(7, Enums.ExtraDataType.shifts, false, "shiftSelection", "selectShift", true, "select", "", 0, 0, new string[] { "MAÑANA - 6hs-14hs", "TARDE - 14hs-22hs", "NOCHE - 22hs-6hs" }));
            this.ConfigurationsField.Add(new ExtraDataConf(8, Enums.ExtraDataType.amountLimit, false, "amountLimit", "amountLimit", true, "input", "numeric", 0, 9, new string[] { }, "", false)); // Unico campo no requerido de Aleph (por ultimo param)
            this.ConfigurationsField.Insert(0, new ExtraDataConf(1, Enums.ExtraDataType.currency, currSel, "currencySelection", "currencySelection", true, "select", "", 0, 0, currencies));
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
        public string SetGoodNextStateNumber
        {
            get
            {
                return this.setGoodNextStateNumberField;
            }
            set
            {
                this.setGoodNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string SetAuxiliaryNextStateNumber
        {
            get
            {
                return this.setAuxiliaryNextStateNumberField;
            }
            set
            {
                this.setAuxiliaryNextStateNumberField = value;
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
        public string HardwareErrorNextStateNumber
        {
            get
            {
                return this.hardwareErrorNextStateNumberField;
            }
            set
            {
                this.hardwareErrorNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string TimeOutNextStateNumber
        {
            get
            {
                return this.timeOutNextStateNumberField;
            }
            set
            {
                this.timeOutNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string Item1
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

        public string ScreenTitle
        {
            get
            {
                return this.screenTitleField;
            }
            set
            {
                this.screenTitleField = value;
            }
        }

        
        [XmlElement()]
        public bool ClearDataOnExit
        {
            get
            {
                return this.clearDataOnExitField;
            }
            set
            {
                this.clearDataOnExitField = value;
            }
        }

        //public bool RequestCustomerPhoneNumber
        //{
        //    get
        //    {
        //        return this.requestCustomerPhoneNumberField;
        //    }
        //    set
        //    {
        //        this.requestCustomerPhoneNumberField = value;
        //    }
        //}

        [XmlElement()]
        public Extension1 Extension1
        {
            get
            {
                return this.extension1Field;
            }
            set
            {
                this.extension1Field = value;
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

        [XmlArrayAttribute(elementName: "ExtraDataConfigurations", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItemAttribute("ExtraDataConfiguration", typeof(ExtraDataConf), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public List<ExtraDataConf> ExtraDataConfigurations
        {
            get
            {
                return this.ConfigurationsField;
            }
            set
            {
                this.ConfigurationsField = value;
            }
        }
        #endregion "Properties"
    }

    [Serializable]
    public class Extension1
    {
        private string stateNumberField;
        private string setChannelEnabledField;
        private string setTransactionInfoEnabledField;
        private string setTransactionRefEnabledField;
        private string setShiftsEnabledField;
        private string setCurrencyEnabledField;
        private string setAmountLimitEnabledField;
        private string setDenominations7384Field;
        private string setDenominations8596Field;
        private string channelTagNameField;
        private string transactionInfoTagNameField;
        private string transactionRefTagNameField;
        private string shiftsTagNameField;

        public Extension1()
        {
            this.setChannelEnabledField = "";
            this.setTransactionInfoEnabledField = "";
            this.setTransactionRefEnabledField = "";
            this.setShiftsEnabledField = "";
            this.setCurrencyEnabledField = "";
            this.setAmountLimitEnabledField = "";
            this.channelTagNameField = "CANAL";
            this.transactionInfoTagNameField = "TransactionInfo";
            this.transactionRefTagNameField = "TransactionRef";
            this.shiftsTagNameField = "TURNO";

        }

        #region Properties
        [XmlElement()]
        public string StateNumber { get => stateNumberField; set => stateNumberField = value; }

        [XmlElement()]
        public string SetChannelEnabled { get => setChannelEnabledField; set => setChannelEnabledField = value; }

        [XmlElement()]
        public string SetTransactionInfoEnabled { get => setTransactionInfoEnabledField; set => setTransactionInfoEnabledField = value; }

        [XmlElement()]
        public string SetTransactionRefEnabled { get => setTransactionRefEnabledField; set => setTransactionRefEnabledField = value; }

        [XmlElement()]
        public string SetShiftsEnabled { get => setShiftsEnabledField; set => setShiftsEnabledField = value; }

        [XmlElement()]
        public string SetCurrencyEnabled { get => setCurrencyEnabledField; set => setCurrencyEnabledField = value; }

        [XmlElement()]
        public string SetAmountLimitEnabled { get => setAmountLimitEnabledField; set => setAmountLimitEnabledField = value; }

        [XmlElement()]
        public string SetDenominations7384 { get => setDenominations7384Field; set => setDenominations7384Field = value; }

        [XmlElement()]
        public string SetDenominations8596 { get => setDenominations8596Field; set => setDenominations8596Field = value; }

        [XmlElement()]
        public string ChannelTagName { get => channelTagNameField; set => channelTagNameField = value; }

        [XmlElement()]
        public string TransactionInfoTagName { get => transactionInfoTagNameField; set => transactionInfoTagNameField = value; }

        [XmlElement()]
        public string TransactionRefTagName { get => transactionRefTagNameField; set => transactionRefTagNameField = value; }

        [XmlElement()]
        public string ShiftsTagName { get => shiftsTagNameField; set => shiftsTagNameField = value; }


        #endregion "Properties"
    }

    public class ConfigurationData
    {
        public List<ExtraDataConf> ExtraDataList { get; set; }
        public string ExtraInfo { get; set; }
        public string ScreenTitle { get; set; }
    }
}