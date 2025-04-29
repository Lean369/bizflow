using System.Runtime.Serialization;

namespace Entities
{
    public enum ExecutionEnumType
    {
        Unk,
        Sync,
        Async,
        Full
    }

    [DataContract]
    public class CommandData
    {
        [DataMember(Name = "executionType")]
        public ExecutionEnumType ExecutionType { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }

        /// <summary>
        /// Signature of the command for security purposes
        /// </summary>
        [DataMember(Name = "signature")]
        public string Signature { get; set; }

        public CommandData(ExecutionEnumType executionType, string data, string signature)
            : this(executionType, data)
        {
            Signature = signature;
        }

        public CommandData(ExecutionEnumType executionType, string data)
        {
            this.ExecutionType = executionType;
            this.Data = data;
        }

        public CommandData()
        {
            this.ExecutionType = ExecutionEnumType.Unk;
            this.Data = "UNDEFINED";
        }

        //public CommandResponse(T Payload) : 
        //    base ()
        //{ }
    }
}
