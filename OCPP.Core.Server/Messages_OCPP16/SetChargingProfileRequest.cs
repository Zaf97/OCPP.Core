using Newtonsoft.Json;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class SetChargingProfileRequest
    {
        [JsonProperty("connectorId", Required = Required.Always)]
        public int ConnectorId { get; set; }

        [JsonProperty("csChargingProfile", Required = Required.Always)]
        public ChargingProfile CsChargingProfile { get; set; }
    }
}
