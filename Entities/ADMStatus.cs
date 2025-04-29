using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class ADMStatus
    {
        [DataMember]
        public eDevice Device;
        [DataMember]
        public ePosition Position;
        [DataMember]
        public eAntiFraudModule AntiFraudModule;
        [DataMember]
        public string lpszExtra;

        public ADMStatus() { }
        public ADMStatus(eDevice device, ePosition position, eAntiFraudModule antiFraud, string lpszExtra)
        {
            this.Device = device;
            this.Position = position;
            this.AntiFraudModule = antiFraud;
            this.lpszExtra = lpszExtra;
        }

        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);
            tab = tab + "\t";

            sb.Append(tab + "├> ");
            sb.Append("Device".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(this.Device + " (" + this.Device.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Position".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(this.Position + " (" + this.Position.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("AntiFraudModule".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(this.AntiFraudModule + " (" + this.AntiFraudModule.ToString("D") + ") ");

            sb.Append(tab + "└> ");
            sb.Append("lpszExtra".PadRight(19, ' '));
            sb.Append(": ");
            sb.Append(this.lpszExtra);

            return sb.ToString();
        }

        public enum eDevice
        {
            DFS_ADM_DEVONLINE = 0,
            DFS_ADM_DEVOFFLINE = 1,
            DFS_ADM_DEVPOWEROFF = 2,
            DFS_ADM_DEVHWERROR = 3,
            DFS_ADM_DEVBUSY = 4,
            DFS_ADM_DEVFRAUDATTEMPT = 5
        }

        public enum ePosition
        {
            DFS_ADM_DEVICEINPOSITION = 0,
            DFS_ADM_DEVICENOTINPOSITION = 1,
            DFS_ADM_DEVICEPOSUNKNOWN = 2
        }

        public enum eAntiFraudModule
        {
            DFS_ADM_DEVOK = 0,
            DFS_ADM_DEV_FRAUD_0 = 1,
            DFS_ADM_DEV_FRAUD_1 = 2
        }
    }
}
