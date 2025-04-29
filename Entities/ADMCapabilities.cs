using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class ADMCapabilities
    {
        [DataMember]
        public string ID;
        [DataMember]
        public string FirmwareVers;
        [DataMember]
        public string EnvelopesMaxQty;

        public ADMCapabilities() { }

        public ADMCapabilities(string id, string firmwareVers, string envelopesMaxQty)
        {
            this.ID = id;
            this.FirmwareVers = firmwareVers;
            this.EnvelopesMaxQty = envelopesMaxQty;
        }

        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);
            tab = tab + "\t";

            sb.Append(tab + "├> ");
            sb.Append("ID".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(this.ID.ToString());

            sb.Append(tab + "├> ");
            sb.Append("FirmwareVers".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(this.FirmwareVers.ToString());

            sb.Append(tab + "└> ");
            sb.Append("EnvelopesMaxQty".PadRight(19, ' '));
            sb.Append(": ");
            sb.Append(this.EnvelopesMaxQty.ToString());

            return sb.ToString();
        }

    }
}
