using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class TicketLines
    {
        private string[] LinesField;

        public TicketLines(string ticket)
        {
            this.Lines = ticket.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        [DataMember]
        public string[] Lines
        {
            get { return this.LinesField; }
            set { this.LinesField = value; }
        }
    }
}
