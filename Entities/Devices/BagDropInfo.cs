using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class BagDropInfo
    {
        [DataMember]
        public List<BagDrop> baglist;

        [DataMember]
        public bool MoreAvailable;

        public BagDropInfo()
        {
            baglist = new List<Entities.BagDrop>();
        }

        public BagDropInfo(List<BagDrop> _baglist)
        {
            baglist = _baglist;
        }
    }

    [DataContract]
    public class BagDrop
    {
        [DataMember]
        public string type;
        [DataMember]
        public string currency;
        [DataMember]
        public string amount;
        [DataMember]
        public string barcode;

        public BagDrop(string _type, string _currency, string _amount, string _barcode)
        {
            type = _type;
            currency = _currency;
            amount = _amount;
            barcode = _barcode;
        }
    }
}
