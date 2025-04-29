using System;
using System.Xml;
using System.Xml.Serialization;
using Business.States;
using System.Collections.Generic;
using Entities;

namespace Business.CloseState
{

    [Serializable]
    public partial class PropertiesCloseState
    {
        private string receiptDeliveredScreenNumberField;
        private string nextStateNumberField;
        private string noReceiptDeliveredScreenNumberField;
        private string cardRetainedScreenNumberField;
        private string statementDeliveredScreenNumberField;
        private string bNANotesReturnedScreenNumberField;
        private Extension extensionField;
        private JournalProperties journalField;
        private PropertiesTimeOut timeOutField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnClose;  
        public StateEvent OnOfferReceipt;
        private bool ApiCleanupRequestAfterCancelField;

        public PropertiesCloseState() { }

        public PropertiesCloseState(AlephATMAppData alephATMAppData)
        {
            this.receiptDeliveredScreenNumberField = "";
            this.nextStateNumberField = "";
            this.noReceiptDeliveredScreenNumberField = "";
            this.cardRetainedScreenNumberField = "";
            this.statementDeliveredScreenNumberField = ""; //Este parámetro se utiliza para determinar si se realiza el EndVisit (000) o no (cualquier otro valor)
            this.bNANotesReturnedScreenNumberField = "";
            this.extensionField = new Extension();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.timeOutField = new PropertiesTimeOut();
            this.OnClose = new StateEvent(StateEvent.EventType.navigate, "", "");
            this.OnOfferReceipt = new StateEvent(StateEvent.EventType.navigate, "offerReceipt.htm", "");
            this.ApiCleanupRequestAfterCancelField = false;
            switch (alephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.CTR50:
                    this.ApiCleanupRequestAfterCancelField = true;
                    break;
            }
        }

        #region "Properties"
        [XmlElement()]
        public string ReceiptDeliveredScreenNumber
        {
            get
            {
                return this.receiptDeliveredScreenNumberField;
            }
            set
            {
                this.receiptDeliveredScreenNumberField = value;
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
        public string NoReceiptDeliveredScreenNumber
        {
            get
            {
                return this.noReceiptDeliveredScreenNumberField;
            }
            set
            {
                this.noReceiptDeliveredScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string CardRetainedScreenNumber
        {
            get
            {
                return this.cardRetainedScreenNumberField;
            }
            set
            {
                this.cardRetainedScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string StatementDeliveredScreenNumber
        {
            get
            {
                return this.statementDeliveredScreenNumberField;
            }
            set
            {
                this.statementDeliveredScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string BNANotesReturnedScreenNumber
        {
            get
            {
                return this.bNANotesReturnedScreenNumberField;
            }
            set
            {
                this.bNANotesReturnedScreenNumberField = value;
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
        public PropertiesTimeOut TimeOut
        {
            get
            {
                return this.timeOutField;
            }
            set
            {
                this.timeOutField = value;
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
        public bool ApiCleanupRequestAfterCancel
        {
            get
            {
                return this.ApiCleanupRequestAfterCancelField;
            }
            set
            {
                this.ApiCleanupRequestAfterCancelField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension
    {
        private string stateNumberField;
        private string cPMTakeDocumentScreenNumberField;
        private CPMDocumentRetainReturnFlag_Type cPMDocumentRetainReturnFlagField;
        private string bCACoinsReturnedScreenNumberField;
        private BNANotesReturnRetainFlag_Type bCACoinsReturnRetainFlagField;
        private BNANotesReturnRetainFlag_Type bNANotesReturnRetainFlagField;

        public Extension()
        {
            this.stateNumberField = "";
            this.cPMTakeDocumentScreenNumberField = "";
            this.cPMDocumentRetainReturnFlagField = new CPMDocumentRetainReturnFlag_Type();
            this.cPMDocumentRetainReturnFlagField = CPMDocumentRetainReturnFlag_Type.Undefined;
            this.bCACoinsReturnedScreenNumberField = "";
            this.bCACoinsReturnRetainFlagField = new BNANotesReturnRetainFlag_Type();
            this.bCACoinsReturnRetainFlagField = BNANotesReturnRetainFlag_Type.Undefined;
            this.bNANotesReturnRetainFlagField = new BNANotesReturnRetainFlag_Type();
            this.bNANotesReturnRetainFlagField = BNANotesReturnRetainFlag_Type.Undefined;
        }

        #region "Properties"
        [XmlElement()]
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

        [XmlElement()]
        public string CPMTakeDocumentScreenNumber
        {
            get
            {
                return this.cPMTakeDocumentScreenNumberField;
            }
            set
            {
                this.cPMTakeDocumentScreenNumberField = value;
            }
        }

        [XmlElement()]
        public CPMDocumentRetainReturnFlag_Type CPMDocumentRetainReturnFlag
        {
            get
            {
                return this.cPMDocumentRetainReturnFlagField;
            }
            set
            {
                this.cPMDocumentRetainReturnFlagField = value;
            }
        }

        [XmlElement()]
        public string BCACoinsReturnedScreenNumber
        {
            get
            {
                return this.bCACoinsReturnedScreenNumberField;
            }
            set
            {
                this.bCACoinsReturnedScreenNumberField = value;
            }
        }

        [XmlElement()]
        public BNANotesReturnRetainFlag_Type BCACoinsReturnRetainFlag
        {
            get
            {
                return this.bCACoinsReturnRetainFlagField;
            }
            set
            {
                this.bCACoinsReturnRetainFlagField = value;
            }
        }

        [XmlElement()]
        public BNANotesReturnRetainFlag_Type BNANotesReturnRetainFlag
        {
            get
            {
                return this.bNANotesReturnRetainFlagField;
            }
            set
            {
                this.bNANotesReturnRetainFlagField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public class PropertiesTimeOut
    {
        private int errorScreenField;

        private int cardEjectFailField;

        private int cardCapturedField;

        public PropertiesTimeOut()
        {
            this.errorScreenField = 4;
            this.cardEjectFailField = 3;
            this.cardCapturedField = 3;
        }

        #region "Properties"
        [XmlElement()]
        public int ErrorScreen
        {
            get
            {
                return this.errorScreenField;
            }
            set
            {
                this.errorScreenField = value;
            }
        }

        [XmlElement()]
        public int CardEjectFail
        {
            get
            {
                return this.cardEjectFailField;
            }
            set
            {
                this.cardEjectFailField = value;
            }
        }

        [XmlElement()]
        public int CardCaptured
        {
            get
            {
                return this.cardCapturedField;
            }
            set
            {
                this.cardCapturedField = value;
            }
        }
        #endregion Properties
    }
}