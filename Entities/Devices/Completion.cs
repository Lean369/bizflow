using System.Runtime.Serialization;

namespace Entities
{
    public enum CompletionCodeEnum
    {
        Unk,
        Success,
        TimeOut,
        HardwareError,
        Reject,
        NotelistEmpty,
        Canceled,
        DeviceNotready,
        InternalError,
        InvalidCommand,
        InvalidRequestID,
        UnsupportedCommand,
        InvalidData,
        ConnectionLost,
        UserError,
        UnsupportedData,
        FraudAttempt,
        SequenceError,
        AuthorisationRequired,
        SignatureVerifyFailed
    }

    [DataContract]
    public class Completion
    {
        [DataMember(Name = "completionCode")]
        public CompletionCodeEnum CompletionCode { get; set; }

        [DataMember(Name = "errorDescription")]
        public string ErrorDescription { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }

        public Completion(CompletionCodeEnum result, string errorDescription, string data)
        {
            this.CompletionCode = result;
            this.ErrorDescription = errorDescription;
            this.Data = data;
        }

        public Completion()
        {
            this.CompletionCode = CompletionCodeEnum.Unk;
            this.ErrorDescription = "UNDEFINED";
        }

        //public CommandResponse(T Payload) : 
        //    base ()
        //{ }
    }
}
