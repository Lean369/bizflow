using System;
using System.Collections.Generic;

namespace Entities
{
    public class Close : ITransaction
    {
        public int ID_CLOSE { get; set; }
        public long MACHINE_ID { get; set; }
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public int COUNT_TRANSACTIONS { get; set; }

        public List<Transaction> TRANSACTIONS { get; set; }

        public Close()
        {

        }

        public Close(long machine_id, string user_id, string user_name) : this()
        {
            this.MACHINE_ID = machine_id;
            this.USER_ID = user_id;
            this.USER_NAME = user_name;
        }
    }
}
