using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Entities
{
    [DataContract]
    public class DenominationsCOIN
    {
        [DataMember]
        public List<DenominationCOIN> Denominations { get; set; }
    }


    [DataContract]
    public class DenominationCOIN
    {
        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public int Quantity { get; set; }

    }

}
