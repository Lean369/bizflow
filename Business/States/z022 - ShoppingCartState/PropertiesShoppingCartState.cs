using Business.States;
using Entities;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Business.ShoppingCartState
{
    [Serializable]
    public partial class PropertiesShoppingCartState
    {
        private string screenNumberField;
        private string nextStateNumberField;
        private string hardwareErrorNextStateNumberField;
        private string timeoutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string backNextStateNumberField;
        private string item2Field;
        private string hostNameField;
        private bool singleCartItemField;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnShowScreen;
        public StateEvent OnPleaseWait;
        public StateEvent OnShowCartItems;
        public StateEvent OnShowCartMessage;
        public StateEvent OnRemoveOperation;

        public PropertiesShoppingCartState()
        {

        }

        public PropertiesShoppingCartState(AlephATMAppData alephATMAppData)
        {
            this.screenNumberField = "";
            this.nextStateNumberField = "";
            this.hardwareErrorNextStateNumberField = "";
            this.timeoutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.backNextStateNumberField = "";
            this.item2Field = "";
            this.hostNameField = "RedpagosMulesoftHost";
            this.singleCartItemField = true; //unico item en carrito
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnShowScreen = new StateEvent(StateEvent.EventType.navigate, "shoppingCart.htm", "");
            this.OnPleaseWait = new StateEvent(StateEvent.EventType.runScript, "ShowPleaseWait", "");
            this.OnShowCartItems = new StateEvent(StateEvent.EventType.runScript, "ShowCartItems", "");
            this.OnShowCartMessage = new StateEvent(StateEvent.EventType.runScript, "ShowCartMessage", "");
            this.OnRemoveOperation = new StateEvent(StateEvent.EventType.runScript, "RemoveOperation", "");

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
        public string NextStateNumber
        {
            get
            {
                return this.nextStateNumberField;
            }
            set
            {
                this.nextStateNumberField = value;
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
        public string BackNextStateNumber
        {
            get
            {
                return this.backNextStateNumberField;
            }
            set
            {
                this.backNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string Item2
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

        [XmlElement()]
        public string HostName
        {
            get => hostNameField;
            set => hostNameField = value;
        }

        [XmlElement()]
        public bool SingleCartItem
        {
            get => singleCartItemField;
            set => singleCartItemField = value;
        }

        #endregion Properties
    }
}