using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


namespace Entities
{
    [DataContract]
    public class PresentStatusCDM
    {
        public PresentStatusCDM() { }
        public PresentStatusCDM(DenominationCDM denomination, int presentState, string extra)
        {
            this.Denomination = denomination;
            this.PresentState = presentState;
            this.Extra = extra;
        }

        [DataMember]
        public DenominationCDM Denomination = new DenominationCDM();

        [DataMember]
        public int PresentState;

        [DataMember]
        public string Extra;

        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);

            tab = tab + "\t";

            sb.Append(tab + "├> ");
            sb.Append("Denomination".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Denomination.ToString(tab));

            sb.Append(tab + "├> ");
            sb.Append("PresentState".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(PresentState + " (" + PresentState.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Extra".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Extra);

            return sb.ToString();
        }
    }
}
