using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.CassetteEntities
{
    [DataContract]
    public class TypeCassettes
    {
        [DataMember]
        public List<TypeCassette> Cassettes { get; set; }
    }

    [DataContract]
    public class TypeCassette
    {
        [DataMember]
        public CassetteType CassetteType { get; set; }

        [DataMember]
        public List<Cassette> PhysicalCassettes { get; set; }

        [DataMember]
        public int Initial { get; set; }

        [DataMember]
        public int Remaining { get; set; }

        [DataMember]
        public int Rejected { get; set; }

        [DataMember(Name = "typeSeverity")]
        public Const.Fitness TypeSeverity { get; set; }

        [DataMember(Name = "suppliesState")]
        public Const.Supplies SuppliesState { get; set; }
    }

    [DataContract]
    public class Cassette
    {
        [DataMember]
        public short ID { get; set; }

        [DataMember]
        public int Initial { get; set; }

        [DataMember]
        public int Remaining { get; set; }

        [DataMember]
        public int Rejected { get; set; }
    }


    [DataContract]
    public class CassetteType
    {
        [DataMember(Name = "type")]
        public Const.TypeEnum Type { get; set; }

        [DataMember]
        public int Denomination { get; set; }

        [DataMember]
        public string Currency { get; set; }
    }
}
