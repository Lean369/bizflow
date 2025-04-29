using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class StatusCIM
    {
        [DataMember]
        public string Device;

        [DataMember]
        public string SafeDoor;

        [DataMember]
        public string Acceptor;

        [DataMember]
        public string IntermediateStacker;

        [DataMember]
        public string StackerItems;

        [DataMember]
        public string BanknoteReader;

        [DataMember]
        public string DropBox;

        [DataMember]
        public string Positions;

        [DataMember]
        public List<Positions> Pos;

        [DataMember]
        public string DevicePosition;

        [DataMember]
        public string Extra;

        [DataMember]
        public string PowerSaveRecoveryTime;

        public StatusCIM(string device, string safeDoor, string acceptor, string intermediateStacker, string stackerItems, string banknoteReader, string dropBox, string positions, List<Positions> pos, string extra, string devicePosition, string powerSaveRecoveryTime)
        {
            Device = device;
            SafeDoor = safeDoor;
            Acceptor = acceptor;
            IntermediateStacker = intermediateStacker;
            StackerItems = stackerItems;
            BanknoteReader = banknoteReader;
            DropBox = dropBox;
            Positions = positions;
            Pos = pos;
            Extra = extra;
            DevicePosition = devicePosition;
            PowerSaveRecoveryTime = powerSaveRecoveryTime;
        }
    }
}
