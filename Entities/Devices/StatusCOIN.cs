using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Entities.Devices
{
    [DataContract]
    public class StatusListCOIN
    {
        public StatusListCOIN() { }

        [DataMember]
        public List<StatusCOIN> Items { get; set; } //set for all coin hoppers command

        [DataMember]
        public List<Entities.Detail> Details { get; set; }  //set after dispense command
    }


    [DataContract]
    public class StatusCOIN
    {

        public StatusCOIN() { }

        public StatusCOIN(int address, int? dispensed = null, string data = null) 
        {
            Address = address;
            Dispensed = dispensed;
            Data = data;
            //Denomination = new DenominationCOIN
            //{
            //    Currency = currency,
            //    Value = value
            //};
        }

        [DataMember]
        public int Address { get; set; }

        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Dispensed { get; set; }

        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        //[DataMember]
        //public DenominationCOIN Denomination { get; set; }

        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InnerState { get; set; }

        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorCode { get; set; }

        //[DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Code { get; set; }

        //[DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int? CurrentlyDispensed { get; set; }

        //[DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int? CurrentlyPending { get; set; }
    }
}
