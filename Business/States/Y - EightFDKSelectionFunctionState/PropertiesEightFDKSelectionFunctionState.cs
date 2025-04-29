using Business.States;
using Entities;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Business.EightFDKSelectionFunctionState
{
    [Serializable]
    public partial class PropertiesEightFDKSelectionFunctionState
    {
        private string screenNumberField;
        private string timeOutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string fDKNextStateNumberField;
        private string operationCodeBufferPositionsField;
        //Example: 028 -> 0 0 0 1 1 1 0 0
        //             -> I H G F D C B A
        private bool enableBagStatusBar;
        private bool enableExitButton;
        private KeyMask_Type activeFDKsField;
        private string screenModeField;
        private Extension extensionField;
        private MoreTimeProperties MoreTimeField;
        private JournalProperties JournalField;
        public List<TransactionMenuItem> availableTransactions;
        public StateEvent OnEightFDKSelection;
        internal StateEvent OnShowAvailableTransactions;

        public PropertiesEightFDKSelectionFunctionState()
        {
            this.screenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.fDKNextStateNumberField = "";
            this.operationCodeBufferPositionsField = "";
            this.enableBagStatusBar = true;
            this.enableExitButton = true;
            this.activeFDKsField = null;
            this.screenModeField = "001"; //Carga una única pantalla inicial y luego activa los avisos al usuario por Javascript
            this.extensionField = new Extension();
            this.MoreTimeField = new MoreTimeProperties();
            this.JournalField = new JournalProperties();
            this.availableTransactions = new List<TransactionMenuItem>();
            this.OnEightFDKSelection = new StateEvent(StateEvent.EventType.navigate, "transactionMenu.htm", "");
            this.OnShowAvailableTransactions = new StateEvent(StateEvent.EventType.runScript, "ShowAvailableTransactions", "");
        }


        //internal void LoadDefaultTransactions()
        //{
        //    this.availableTransactions.Add(new TransactionMenuItem(1, true, "depositCash", Const.Colors.Primary, Const.Icons.Money, "140"));
        //    this.availableTransactions.Add(new TransactionMenuItem(2, true, "depositEnvelope", Const.Colors.Primary, Const.Icons.Envelope, "110"));
        //}

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
        public string FDKNextStateNumber
        {
            get
            {
                return this.fDKNextStateNumberField;
            }
            set
            {
                this.fDKNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string OperationCodeBufferPositions
        {
            get
            {
                return this.operationCodeBufferPositionsField;
            }
            set
            {
                this.operationCodeBufferPositionsField = value;
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
        #endregion "Properties"
    }

    [Serializable]
    public partial class Extension
    {

        private string codeFDKAField;
        private string codeFDKBField;
        private string codeFDKCField;
        private string codeFDKDField;
        private string codeFDKFField;
        private string codeFDKGField;
        private string codeFDKHField;
        private string codeFDKIField;

        public Extension() { }

        #region "Properties"
        [XmlElement()]
        public string CodeFDKA
        {
            get
            {
                return this.codeFDKAField;
            }
            set
            {
                this.codeFDKAField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKB
        {
            get
            {
                return this.codeFDKBField;
            }
            set
            {
                this.codeFDKBField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKC
        {
            get
            {
                return this.codeFDKCField;
            }
            set
            {
                this.codeFDKCField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKD
        {
            get
            {
                return this.codeFDKDField;
            }
            set
            {
                this.codeFDKDField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKF
        {
            get
            {
                return this.codeFDKFField;
            }
            set
            {
                this.codeFDKFField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKG
        {
            get
            {
                return this.codeFDKGField;
            }
            set
            {
                this.codeFDKGField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKH
        {
            get
            {
                return this.codeFDKHField;
            }
            set
            {
                this.codeFDKHField = value;
            }
        }

        [XmlElement()]
        public string CodeFDKI
        {
            get
            {
                return this.codeFDKIField;
            }
            set
            {
                this.codeFDKIField = value;
            }
        }
        #endregion "Properties"
    }
}