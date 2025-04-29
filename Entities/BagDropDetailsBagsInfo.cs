using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class BagDropDetailsBagsInfo
    {
        [DataMember]
        public string Type;

        [DataMember]
        public string Currency;

        [DataMember]
        public string Amount;

        [DataMember]
        public string Barcode;

        public BagDropDetailsBagsInfo()
        {

        }
    }
}
