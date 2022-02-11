using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class SetChargingProfileResponse
    {
        [JsonProperty("status", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChargingProfileStatus Status { get; set; }
    }

    [GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public enum ChargingProfileStatus
    {
        [EnumMember(Value = @"Accepted")]
        Accepted = 0,
        [EnumMember(Value = @"Rejected")]
        Rejected = 1,
        [EnumMember(Value = @"NotSupported")]
        NotSupported = 2
    }

}
