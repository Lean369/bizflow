using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities
{
    /// <summary>
    /// Clase utilizada para manejar los billetes reconocidos por el aceptador que se muestran por pantalla
    /// </summary>
    [DataContract]
    public class CashInInfo
    {
        [DataMember]
        public List<Bills> Bills;

        [DataMember]
        public bool MoreAvailable;

        [DataMember]
        public bool EnterAvailable;

        [DataMember]
        public bool CancelAvailable;

        [DataMember]
        public bool EscrowFull;

        [DataMember]
        public bool PrinterNotAvailable;

        public CashInInfo()
        {
            Bills = new List<Entities.Bills>();
            this.MoreAvailable = true;
            this.EnterAvailable = true;
            this.CancelAvailable = true;
            this.EscrowFull = false;
            this.PrinterNotAvailable = false;
        }

        public object Clone()
        {
            return new CashInInfo
            {
                Bills = new List<Bills>(this.Bills),
                MoreAvailable = this.MoreAvailable,
                EnterAvailable = this.EnterAvailable,
                CancelAvailable = this.CancelAvailable,
                EscrowFull = this.EscrowFull,
                PrinterNotAvailable = this.PrinterNotAvailable
            };
        }
    }

    [DataContract]
    public class Bills
    {
        [DataMember]
        public string Currency;
        [DataMember]
        public long Quantity;
        [DataMember]
        public int Id;
        [DataMember]
        public long Value;
        [DataMember]
        public int Release;
        [DataMember]
        public string NDCNoteID;

        public Bills() { }
        public Bills(string currency, long quantity, int id, long value, int release, string ndcNoteId)
        {
            Currency = currency;
            Quantity = quantity;
            Id = id;
            Value = value;
            Release = release;
            NDCNoteID = ndcNoteId;
        }
    }
}
