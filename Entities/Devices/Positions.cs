using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class Positions
    {
        public Positions() { }

        [DataMember]
        public string Position;
        [DataMember]
        public string Shutter;
        [DataMember]
        public string PositionStatus;
        [DataMember]
        public string TransportStatus;
        [DataMember]
        public string Transport;

        public Positions(string position, string shutter, string positionStatus, string transport, string transportStatus)
        {
            Position = position;
            Shutter = shutter;
            PositionStatus = positionStatus;
            Transport = transport;
            TransportStatus = transportStatus;
        }
    }
}
