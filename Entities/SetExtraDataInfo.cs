using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Entities
{
    [DataContract]
    public class SetExtraDataInfo
    {
        public int CurrentStep { get; set; }
        public int StepLength { get; set; }
        public long? ServiceId { get; set; }
        public string ServiceName { get; set; }
    }
}
