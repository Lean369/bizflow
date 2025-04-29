using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


namespace Entities
{
    [DataContract]
    public class DenominationCDM
    {
        public DenominationCDM() { }

        public DenominationCDM(string currencyID, int amount, int count, List<int> values, int cashBox)
        {
            this.CurrencyID = currencyID;
            this.Amount = amount;
            this.Count = count;
            this.Values = values;
            this.CashBox = cashBox;
        }

        [DataMember]
        public string CurrencyID;

        [DataMember]
        public int Amount;

        [DataMember]
        public int Count;

        [DataMember]
        public List<int> Values = new List<int>();

        [DataMember]
        public int CashBox;

        /// <summary>
        /// Retorna una cadena que representa al objeto actual.
        /// </summary>
        /// <param name="tab">Se le pasa una cadena como parámetro.</param>
        /// <returns>Retorna una cadena que representa al objeto actual</returns>
        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);

            tab = tab + "\t";

            sb.Append(tab + "├> ");
            sb.Append("CurrencyID".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(CurrencyID + " (" + CurrencyID.ToString() + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Amount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Amount + " (" + Amount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Count".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Count + " (" + Count.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Values".PadRight(19, ' '));
            sb.AppendLine(": ");
            for (int i = 0; i < Values.Count; i++)
            {
                sb.AppendLine(tab + tab + Values[i] + " (" + Values[i].ToString("D") + ") ");
            }

            sb.Append(tab + "├> ");
            sb.Append("CashBox".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(CashBox + " (" + CashBox.ToString("D") + ") ");

            return sb.ToString();
        }

        public string ToSalidaRequerida()
        {
            return this.ToString("\t\t");
        }
    }
}

