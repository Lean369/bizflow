using System.Runtime.Serialization;
using System.Text;

namespace Entities
{
    [DataContract]
    public class SensorsState
    {
        [DataMember]
        public bool Comb { get; set; }
        [DataMember]
        public bool PreDoor { get; set; }
        [DataMember]
        public bool UpperDoor { get; set; }
        [DataMember]
        public bool Presence { get; set; }
        [DataMember]
        public bool Cover { get; set; }
        [DataMember]
        public bool Door { get; set; }
        [DataMember]
        public bool Lock { get; set; }

        public SensorsState()
        {
            this.Presence = false;
            this.Cover = true;
            this.Door = true;
            this.Lock = true;
            this.UpperDoor = true;
            this.PreDoor = true;
            this.Comb = true;
        }

        public static SensorsState GetSensorsState(byte[] sensorState, IOBoardConfig aIOConfig)
        {
            SensorsState sensorsState = new SensorsState();
            if (sensorState.Length > 7)
            {
                sensorsState.Comb = sensorState[aIOConfig.CombSensorPos] == 0x01 ? true : false;
                sensorsState.PreDoor = sensorState[aIOConfig.PreDoorSensorPos] == 0x01 ? true : false;
                sensorsState.UpperDoor = sensorState[aIOConfig.UpperDoorSensorPos] == 0x01 ? true : false;
                sensorsState.Presence = sensorState[aIOConfig.PresenceSensorPos] == 0x01 ? true : false;
                sensorsState.Cover = sensorState[aIOConfig.CoverSensorPos] == 0x01 ? true : false;
                sensorsState.Door = sensorState[aIOConfig.DoorSensorPos] == 0x01 ? true : false;
                sensorsState.Lock = sensorState[aIOConfig.LockSensorPos] == 0x01 ? true : false;
            }
            return sensorsState;
        }

        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);

            tab = tab + "\t";

            sb.Append(tab + "├> ");
            sb.Append("Presence".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(Presence ? "Close" : "Open");

            sb.Append(tab + "├> ");
            sb.Append("Cover".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(Cover ? "Close" : "Open");

            sb.Append(tab + "├> ");
            sb.Append("Door".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(Door ? "Close" : "Open");

            sb.Append(tab + "├> ");
            sb.Append("Lock".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(Lock ? "Close" : "Open");

            sb.Append(tab + "└> ");
            sb.Append("UpperDoor".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(UpperDoor ? "Close" : "Open");

            sb.Append(tab + "└> ");
            sb.Append("PreDoor".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(PreDoor ? "Close" : "Open");

            sb.Append(tab + "└> ");
            sb.Append("Comb".PadRight(10, ' '));
            sb.Append(": ");
            sb.AppendLine(Comb ? "Close" : "Open");

            return sb.ToString();
        }
    }
}
