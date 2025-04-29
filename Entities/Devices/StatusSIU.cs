using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class StatusSIU
    {
        [DataMember]
        public string Device;

        [DataMember]
        public int DoorCabinet;

        [DataMember]
        public int DoorSafe;

        [DataMember]
        public int VandalShield;

        //[DataMember]
        //public List<int> Indicators;

        //[DataMember]
        //public List<int> Auxiliaries;

        //[DataMember]
        //public List<int> GuidLights;

        public StatusSIU() { }

        public StatusSIU(string device, int doorCabinet, int doorSafe, int vandalShield)
        {
            this.Device = device;
            this.DoorCabinet = doorCabinet;
            this.DoorSafe = doorSafe;
            this.VandalShield = vandalShield;
        }
    }
}
