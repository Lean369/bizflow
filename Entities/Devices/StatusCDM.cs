using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class StatusCDM
    {
        [DataMember]
        public string Device;

        [DataMember]
        public string SafeDoor;

        [DataMember]
        public string Dispenser;

        [DataMember]
        public string IntermediateStacker;

        [DataMember]
        public List<Positions> Pos;

        [DataMember]
        public string Extra;

        [DataMember]
        public string DevicePosition;

        [DataMember]
        public string PowerSaveRecoveryTime;

        [DataMember]
        public string AntiFraudModule;

        public StatusCDM(string device, string safeDoor, string dispenser, string intermediateStacker, List<Positions> pos, string extra, string devicePosition, string powerSaveRecoveryTime, string antiFraudModule)
        {
            Device = device;
            SafeDoor = safeDoor;
            Dispenser = dispenser;
            IntermediateStacker = intermediateStacker;
            Pos = pos;
            Extra = extra;
            DevicePosition = devicePosition;
            PowerSaveRecoveryTime = powerSaveRecoveryTime;
            AntiFraudModule = antiFraudModule;
        }
    }
}
