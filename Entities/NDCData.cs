using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Datos para enviar en el Transaction Request.
    /// </summary>
    public class NDCData
    {
        private string coOrdination = "0";

        private string timeVariant = "00000000";

        private string operationCodeData = "        ";

        private string bufferB = "";

        private string bufferC = "";

        private int languageCode;

        private bool retainCustomerCard;

        private string transactionReplyScreenNumber = "";

        private TransactionReplyCommand_Type transactionReply;

        private ITR_Type itr;

        private SpecificCommandRejectStatus_Type specificCmdRejectStatus;

        private bool sidewaysReceiptMode;

        private PrintingInstruction_Type[] receiptData;

        private PrintingInstruction_Type[] journalData;

        private PrintingInstruction_Type[] statementData;

        private PassbookPrinterData_Type passbookData;

        private byte[] eMVKernelCurrencyCode;

        private byte[] eMVKernelCurrencyExponent;

        private byte[] eMVKernelTransactionType;

        public NDCData()
        {
            this.itr = new ITR_Type();
        }

        public string CoOrdination
        {
            get
            {
                return this.coOrdination;
            }
            set
            {
                this.coOrdination = value;
            }
        }

        public string TimeVariant
        {
            get
            {
                return this.timeVariant;
            }
            set
            {
                this.timeVariant = value;
            }
        }

        public string OperationCodeData
        {
            get
            {
                return this.operationCodeData;
            }
            set
            {
                this.operationCodeData = value;
            }
        }

        public string BufferB
        {
            get
            {
                return this.bufferB;
            }
            set
            {
                this.bufferB = value;
            }
        }

        public string BufferC
        {
            get
            {
                return this.bufferC;
            }
            set
            {
                this.bufferC = value;
            }
        }

        public int LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = value;
            }
        }

        public bool RetainCustomerCard
        {
            get
            {
                return this.retainCustomerCard;
            }
            set
            {
                this.retainCustomerCard = value;
            }
        }

        public string TransactionReplyScreenNumber
        {
            get
            {
                return this.transactionReplyScreenNumber;
            }
            set
            {
                this.transactionReplyScreenNumber = value;
            }
        }

        public TransactionReplyCommand_Type TransactionReply
        {
            get
            {
                return this.transactionReply;
            }
            set
            {
                this.transactionReply = value;
            }
        }

        public ITR_Type ITR
        {
            get
            {
                return this.itr;
            }
            set
            {
                this.itr = value;
            }
        }

        public SpecificCommandRejectStatus_Type SpecificCmdRejectStatus
        {
            get
            {
                return this.specificCmdRejectStatus;
            }
            set
            {
                this.specificCmdRejectStatus = value;
            }
        }

        public bool SidewaysReceiptMode
        {
            get
            {
                return this.sidewaysReceiptMode;
            }
            set
            {
                this.sidewaysReceiptMode = value;
            }
        }

        public PrintingInstruction_Type[] ReceiptData
        {
            get
            {
                return this.receiptData;
            }
            set
            {
                this.receiptData = value;
            }
        }

        public PrintingInstruction_Type[] JournalData
        {
            get
            {
                return this.journalData;
            }
            set
            {
                this.journalData = value;
            }
        }

        public byte[] EMVKernelCurrencyCode
        {
            get
            {
                return this.eMVKernelCurrencyCode;
            }
            set
            {
                this.eMVKernelCurrencyCode = value;
            }
        }

        public byte[] EMVKernelCurrencyExponent
        {
            get
            {
                return this.eMVKernelCurrencyExponent;
            }
            set
            {
                this.eMVKernelCurrencyExponent = value;
            }
        }

        public byte[] EMVKernelTransactionType
        {
            get
            {
                return this.eMVKernelTransactionType;
            }
            set
            {
                this.eMVKernelTransactionType = value;
            }
        }
    }
}
