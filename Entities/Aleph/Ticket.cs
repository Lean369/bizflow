using System;

namespace Entities
{
    public class Ticket
    {
        #region Propieties
        public string Value { get; set; }
        public int TransactionId { get; set; }


        #endregion

        #region Constructor
        public Ticket()
        {
            this.Init();
        }

        public Ticket(string value, int transactionId) {
            this.Value = value;
            this.TransactionId = transactionId;
        }
        private void Init() {
            this.TransactionId = 0;
            this.Value = String.Empty;
        }

        #endregion
    }
}
