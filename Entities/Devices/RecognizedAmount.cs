using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    /// <summary>
    /// Se utiliza para almacenar los datos de billetes capturados hasta el momento
    /// </summary>
    [DataContract]
    public class RecognizedAmount
    {
        [DataMember]
        public List<Values> total;

        [DataMember]
        public bool MoreAvailable;

        public RecognizedAmount()
        {
            total = new List<Values>();
        }

        public RecognizedAmount(List<Values> items)
        {
            total = items;
        }
    }

    [DataContract]
    public class Values
    {
        [DataMember]
        public string currency;

        [DataMember]
        public string amount;

        public Values(string _currency, string _amount)
        {
            this.currency = _currency;
            this.amount = _amount;
        }
    }
}
