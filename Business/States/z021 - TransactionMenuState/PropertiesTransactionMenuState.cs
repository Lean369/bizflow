using Business.States;
using Entities;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Business.TransactionMenuState
{
    [Serializable]
    public partial class PropertiesTransactionMenuState
    {
        private string screenNumberField;
        private string timeOutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string nextStateNumberAField;
        private string nextStateNumberBField;
        private string nextStateNumberCField;
        private string nextStateNumberDField;
        private string deviceErrorNextStateNumberField;
        private bool enableBagStatusBar;
        private bool enableExitButton;
        private bool isMobileTopup;
        private string screenModeField;
        private string item1Field;
        private bool loadContentsInMemoryField;
        private MoreTimeProperties MoreTimeField;
        private JournalProperties JournalField;
        public List<TransactionMenuItem> availableTransactions;
        public StateEvent OnEightFDKSelection;
        internal StateEvent OnShowAvailableTransactions;
        private KeyMask_Type activeFDKsField;
        [XmlElement()]
        public StateEvent OnPleaseWait;

        public PropertiesTransactionMenuState()
        {
            this.screenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.nextStateNumberAField = "";
            this.nextStateNumberBField = "";
            this.nextStateNumberCField = "";
            this.nextStateNumberDField = "";
            this.deviceErrorNextStateNumberField = "";
            this.enableBagStatusBar = true;
            this.enableExitButton = true;
            this.isMobileTopup = false;
            this.screenModeField = "001"; //Carga una única pantalla inicial y luego activa los avisos al usuario por Javascript
            this.item1Field = "";
            this.loadContentsInMemoryField = false;
            this.MoreTimeField = new MoreTimeProperties();
            this.JournalField = new JournalProperties();
            this.availableTransactions = new List<TransactionMenuItem>();
            this.OnEightFDKSelection = new StateEvent(StateEvent.EventType.navigate, "transactionMenu.htm", "");
            this.OnShowAvailableTransactions = new StateEvent(StateEvent.EventType.runScript, "ShowAvailableTransactions", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
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
        public string NextStateNumberA
        {
            get
            {
                return this.nextStateNumberAField;
            }
            set
            {
                this.nextStateNumberAField = value;
            }
        }

        [XmlElement()]
        public string NextStateNumberB
        {
            get
            {
                return this.nextStateNumberBField;
            }
            set
            {
                this.nextStateNumberBField = value;
            }
        }

        [XmlElement()]
        public string NextStateNumberC
        {
            get
            {
                return this.nextStateNumberCField;
            }
            set
            {
                this.nextStateNumberCField = value;
            }
        }

        [XmlElement()]
        public string NextStateNumberD
        {
            get
            {
                return this.nextStateNumberDField;
            }
            set
            {
                this.nextStateNumberDField = value;
            }
        }

        [XmlElement()]
        public string DeviceErrorNextStateNumber
        {
            get
            {
                return this.deviceErrorNextStateNumberField;
            }
            set
            {
                this.deviceErrorNextStateNumberField = value;
            }
        }

        public bool EnableBagStatusBar
        {
            get
            {
                return this.enableBagStatusBar;
            }
            set
            {
                this.enableBagStatusBar = value;
            }
        }

        public bool EnableExitButton
        {
            get
            {
                return this.enableExitButton;
            }
            set
            {
                this.enableExitButton = value;
            }
        }

        public bool IsMobileTopup
        {
            get { return this.isMobileTopup; }
            set { this.isMobileTopup = value; }
        }

        [XmlElement()]
        public KeyMask_Type ActiveFDKs
        {
            get
            {
                return this.activeFDKsField;
            }
            set
            {
                this.activeFDKsField = value;
            }
        }

        [XmlElement()]
        public string ScreenMode
        {
            get
            {
                return this.screenModeField;
            }
            set
            {
                this.screenModeField = value;
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

        [XmlElement()]
        public MoreTimeProperties MoreTime
        {
            get
            {
                return this.MoreTimeField;
            }
            set
            {
                this.MoreTimeField = value;
            }
        }

        [XmlElement()]
        public JournalProperties Journal
        {
            get
            {
                return this.JournalField;
            }
            set
            {
                this.JournalField = value;
            }
        }

        [XmlIgnore]
        public List<TransactionMenuItem> avTransactions
        {
            get
            {
                return this.availableTransactions;
            }
            set
            {
                this.availableTransactions = value;
            }
        }

        [XmlElement()]
        public bool LoadContentsInMemory
        {
            get
            {
                return this.loadContentsInMemoryField;
            }
            set
            {
                this.loadContentsInMemoryField = value;
            }
        }
        #endregion "Properties"
    }
}