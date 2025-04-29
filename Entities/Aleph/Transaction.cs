using System;

namespace Entities
{
    public class Transaction : ITransaction
    {
        #region Propieties
        public int NUM_OPERATION { get; set; }
        public long MACHINE_ID { get; set; }
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public string CANAL { get; set; }
        public DateTime CREATED { get; set; }
        public int SHIPOUT_ID { get; set; }
        public int CLOSE_ID { get; set; }
        public Enums.TransactionType TYPE { get; set; }
        public int LOT { get; set; }


        public Contents CONTENTS { get; set; }
        public Ticket TICKETS { get; set; }

        #endregion

        #region Constructor
        public Transaction()
        {
            
        }

        public Transaction(long machine_id, string user_id, string user_name, string canal,
            int num_operation, Contents contents, Enums.TransactionType type) : this()
        {
            this.MACHINE_ID = machine_id;
            this.USER_ID = user_id;
            this.USER_NAME = user_name;
            this.CANAL = canal;
            this.NUM_OPERATION = num_operation;
            if (type == Enums.TransactionType.DEPOSIT)
            {
                foreach (Detail detail in contents.LstDetail)
                {
                    detail.LstItems.ForEach(y => { y.Barcode = ""; y.Category = ""; });
                }
            }
            this.CONTENTS = contents;
            this.TYPE = type;
        }
        #endregion

    }
}
