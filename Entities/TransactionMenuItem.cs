using System.Runtime.Serialization;
using System.Xml.Serialization;
using static Entities.Enums;

namespace Entities
{
    public class TransactionMenuItem
    {
        private string TransactionTag;
        private string ButtonColor;
        private string ButtonIcon;
        private char FDK;
        private bool Enabled;

        public TransactionMenuItem() { }

        public TransactionMenuItem(string transactionTag, string buttonColor, string buttonIcon, char fdk, bool enabled = true)
        {
            this.TransactionTag = transactionTag;
            this.ButtonColor = buttonColor;
            this.ButtonIcon = buttonIcon;
            this.FDK = fdk;
            this.Enabled = enabled;
        }

        public string transactionTag
        {
            get { return this.TransactionTag; }
            set { this.TransactionTag = value; }
        }

        public string buttonColor
        {
            get { return this.ButtonColor; }
            set { this.ButtonColor = value; }
        }

        public string buttonIcon
        {
            get { return this.ButtonIcon; }
            set { this.ButtonIcon = value; }
        }

        public char fdk
        {
            get { return this.FDK; }
            set { this.FDK = value; }
        }

        public bool enabled {
            get { return this.Enabled; }
            set { this.Enabled = value; }
        }
    }
}
