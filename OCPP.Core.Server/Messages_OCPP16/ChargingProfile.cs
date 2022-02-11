using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class ChargingProfile
    {
        [JsonProperty("chargingProfileId", Required = Required.Always)]
        public int ChargingProfileId { get; set; }

        [JsonProperty("transactionId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int TransactionId { get; set; }

        [JsonProperty("stackLevel", Required = Required.Always)]
        public int StackLevel { get; set; }

        [JsonProperty("chargingProfilePurpose", Required = Required.Always)]
        public ChargingProfilePurposeType ChargingProfilePurpose { get; set; }

        [JsonProperty("chargingProfileKind", Required = Required.Always)]
        public ChargingProfileKindType ChargingProfileKind { get; set; }

        [JsonProperty("recurencyKind", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public RecurencyKindType RecurencyKind { get; set; }

        [JsonProperty("validFrom", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ValidFrom { get; set; }

        [JsonProperty("validTo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ValidTo { get; set; }

        [JsonProperty("chargingSchedule", Required = Required.Always)]
        public ChargingSchedule ChargingSchedule { get; set; }
    }

    [GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public enum ChargingProfilePurposeType
    {
        [EnumMember(Value = @"ChargingPointMaxProfile")]
        ChargingPointMaxProfile,
        [EnumMember(Value = @"TxDefaultProfile")]
        TxDefaultProfile,
        [EnumMember(Value = @"TxProfile")]
        TxProfile
    }

    [GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public enum ChargingProfileKindType
    {
        [EnumMember(Value = @"Absolute")]
        Absolute,
        [EnumMember(Value = @"Recurring")]
        Recurring,
        [EnumMember(Value = @"Relative")]
        Relative
    }

    [GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v9.0.0.0)")]
    public enum RecurencyKindType {
        [EnumMember(Value = @"Daily")]
        Daily,
        [EnumMember(Value = @"Weekly")]
        Weekly
    }
}
