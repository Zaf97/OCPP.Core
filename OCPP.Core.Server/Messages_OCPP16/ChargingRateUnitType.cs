using System.Runtime.Serialization;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public enum ChargingRateUnitType
    {
        [EnumMember(Value = @"W")]
        Watts = 0,
        [EnumMember(Value = @"A")]
        Amperes = 0,
    }
}
