using Newtonsoft.Json;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class ChargingSchedulePeriod
    {
        [JsonProperty("startPeriod", Required = Required.Always)]
        public int StartPeriod { get; set; }

        [JsonProperty("limit", Required = Required.Always)]
        public decimal Limit { get; set; }

        [JsonProperty("numberPhases", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int NumberPhases { get; set; }
    }
}