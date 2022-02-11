using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class GetCompositeScheduleResponse
    {
        [JsonProperty("status", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        [JsonConverter(typeof(StringEnumConverter))]
        public UnlockConnectorResponseStatus Status { get; set; }

        [JsonProperty("connectorId", Required = Required.Always)]
        public int ConnectorId { get; set; }

        [JsonProperty("scheduleStart", Required = Required.Always)]
        public DateTime ScheduleStart { get; set; }

        [JsonProperty("chargingSchedule", Required = Required.Always)]
        public ChargingSchedule ChargingSchedule { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public enum GetCompositeScheduleStatus
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
        Accepted = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
        Rejected = 1,
    }
}
