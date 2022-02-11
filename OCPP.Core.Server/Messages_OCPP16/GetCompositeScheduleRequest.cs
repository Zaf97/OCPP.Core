using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class GetCompositeScheduleRequest
    {
        [JsonProperty("connectorId", Required =Required.Always)]
        public int ConnectorId { get; set; }

        [JsonProperty("duration", Required = Required.Always)]
        public int Duration { get; set; }

        [JsonProperty("chargingRateUnit", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ChargingRateUnitType ChargingRateUnit { get; set; }

    }
}
