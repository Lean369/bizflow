using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class MySqlDBPDO
    {
        [DataMember]
        public string User;

        [DataMember]
        public string UserName;

        [DataMember]
        public string Password;

        [DataMember]
        public string PassMD5;

        [DataMember]
        public string Role;

        [DataMember]
        public bool Active;

        public MySqlDBPDO()
        {

        }
    }
}
