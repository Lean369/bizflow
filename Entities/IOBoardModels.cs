using Newtonsoft.Json;

namespace Entities.IOBoardModels
{

    public class IOBoardResponse
    {
        public int StatusCode { get; set; }  //0=ok | 1=info | 2=warning | 3=bad request | 4=error | 5=fatal
        public StatusTypes Status { get { return (StatusTypes)StatusCode; } }
        public string Message { get; set; }
        public int RequestID { get; set; }
        public SensorStatesLocal SensorsStates { get; set; }
        public enum StatusTypes { OK, INFO, WARN, BAD_REQUEST, ERROR, FATAL }
    }

    public class IOBoardRequest
    {
        public int RequestID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LedSet LedSetColor { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LedSet LedSetFading { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GenericCommand GetState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GenericCommand GetVersion { get; set; }
    }

    public class SensorStatesLocal
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor1 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor2 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor3 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor4 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor5 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor6 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor7 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sensor8 { get; set; }
    }

    public class SensorConfig
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string TrueState { get; set; }
        public string FalseState { get; set; }
    }

    public class LedConfig
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }

    public class LedSet
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class GenericCommand
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Parameters { get; set; }
    }
}
