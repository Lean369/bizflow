using System;
using System.Collections.Generic;

namespace Entities
{
    public class Shipout : ITransaction
    {
        public int ID_SHIPOUT { get; set; }
        public long MACHINE_ID { get; set; }
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public int LOT { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public int NUM_OPERATION { get; set; }
        public int COUNT_TRANSACTIONS { get; set; }

        public List<Transaction> TRANSACTIONS { get; set; }
        public Ticket TICKETS { get; set; }

        public Shipout()
        {

        }

        public Shipout(long machine_id, string user_id, string user_name, int lot, int num_operation) : this()
        {
            this.MACHINE_ID = machine_id;
            this.USER_ID = user_id;
            this.USER_NAME = user_name;
            this.LOT = lot;
            this.NUM_OPERATION = num_operation;
        }
    }
}
