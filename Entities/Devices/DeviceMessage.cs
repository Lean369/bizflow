using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using System.Text;


namespace Entities
{

    //[DataContract(Name = "type")]
    public enum Types
    {
        [EnumMember(Value = "command")]
        Command,
        [EnumMember(Value = "event")]
        Event,
        [EnumMember(Value = "completion")]
        Completion,
        [EnumMember(Value = "acknowledge")]
        Acknowledge,
        [EnumMember(Value = "unsolicited")]
        Unsolicited
    }
    /// <summary>
    /// A payload object
    /// </summary>
    [DataContract]
    public class MessagePayloadBase
    {
        /// <summary>
        /// MessageHeader class representing XFS4 message header
        /// The XFS4IoT payload can't be a null value and need an empty structure to be set for an empty payload in brackets
        /// </summary>
        public MessagePayloadBase()
        { }
    }

    [DataContract]
    //public class DeviceMessage<T> : ICloneable where T : MessagePayloadBase
    public class DeviceMessage : ICloneable
    {
        /// <summary>
        /// Header of the message for command
        /// </summary>
        [DataMember(IsRequired = true, Name = "header")]
        public MessageHeader Header { get; set; }

        [DataMember(IsRequired = true, Name = "payload")]
        public object Payload { get; set; }

        public Enums.Devices Device { get; set; }

        public Enums.Commands Command { get; set; }

        public int ConnectionID { get; set; }

        public DeviceMessage() { }

        public DeviceMessage(Types type, Enums.Devices device, Enums.Commands command, object payload)
        {
            this.Device = device;
            this.Command = command;
            this.Payload = payload;
            this.Header = new MessageHeader(type, $"{this.Device}.{this.Command}", 0);
        }

        public DeviceMessage(Enums.Devices device, Enums.Commands command, int? requestId, object payload)
        {
            this.Device = device;
            this.Command = command;
            this.Payload = payload;
            //Types type = command == Enums.Commands.Event ? Types.Event : Types.Command;
            this.Header = new MessageHeader(Types.Command, $"{this.Device}.{this.Command}", requestId);
        }

        public DeviceMessage(Enums.Devices device, Enums.Commands command)
        {
            this.Device = device;
            this.Command = command;
            //this.Result = Enums.Results.UNK;
            //this.Payload = "UNDEFINED";
            this.Header = new MessageHeader(Types.Command, $"{this.Device}.{this.Command}", 0);
        }

        public DeviceMessage(string msg, int connectionID, out bool ret)
        {
            ret = false;
            ret = this.Parse(msg);
            this.ConnectionID = connectionID;
        }

        public DeviceMessage(string msg, out bool ret)
        {
            ret = false;
            ret = this.Parse(msg);
        }

        /// <summary>
        /// Deep copy of the message object
        /// </summary>
        /// <returns>Copied message object</returns>
        public object Clone() => MemberwiseClone();

        internal bool Parse(string input)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            try
            {
                //string output = new string(input.Where(c => !char.IsControl(c)).ToArray());
                //string output = this.ReplaceControlCharacters(input);
                DeviceMessage dm = JsonConvert.DeserializeObject<DeviceMessage>(input, settings);
                this.Header = dm.Header;
                this.Device = Utilities.Utils.StringToEnum<Enums.Devices>(dm.Header.Name.Split('.')[0]);
                this.Command = Utilities.Utils.StringToEnum<Enums.Commands>(dm.Header.Name.Split('.')[1]);
                switch (dm.Header.Type)
                {
                    case Types.Command:
                        this.Payload = JsonConvert.DeserializeObject<CommandData>(dm.Payload.ToString());
                        break;
                    case Types.Completion:
                        this.Payload = JsonConvert.DeserializeObject<Completion>(dm.Payload.ToString());
                        break;
                    case Types.Unsolicited:
                        this.Payload = dm.Payload;
                        break;
                    case Types.Event:
                        this.Payload = dm.Payload;
                        break;
                    default:
                        dm.Payload = new object();
                        break;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new StringEnumConverter());
        }

        public string ReplaceControlCharacters(string sDataIn)
        {
        //    string a;
            System.Collections.ArrayList Characters = new System.Collections.ArrayList();
            StringBuilder sb = new StringBuilder();
            try
            {
                Characters.AddRange(sDataIn.ToCharArray());
                foreach (char aux in Characters)
                {
                    if (aux > 31)
                        sb.Append(aux);
                    //else
                    //    a = "";
                }
                return sb.ToString();
            }
            catch (Exception ex) { throw ex; }
        }
    }

    [DataContract]
    public class MessageHeader
    {
        public MessageHeader() { }

        /// <summary>
        /// MessageHeader class representing XFS4 message header
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="RequestId"></param>
        /// <param name="Type"></param>
        public MessageHeader(Types _type, string _name, int? _requestId)
        {
            Type = _type;
            Name = _name;
            RequestId = _requestId;
        }

        [DataMember(IsRequired = true, Name = "type")]
        public Types Type { get; set; }

        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Name = "requestId")]
        public int? RequestId { get; set; }

    }


}
