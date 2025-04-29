namespace Entities
{
    public class TrxMessage
    {
        public Enums.TransactionType TransactionType { get; set; }
        public string Tsn { get; set; }
        public Contents Contents { get; set; }

        public TrxMessage() { }

        public TrxMessage(Enums.TransactionType transactionType, string tsn, Contents contents)
        {
            this.TransactionType = transactionType;
            this.Tsn = tsn;
            this.Contents = contents;
        }

    }
}
