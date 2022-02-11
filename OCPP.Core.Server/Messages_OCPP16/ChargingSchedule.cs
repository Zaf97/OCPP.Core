using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OCPP.Core.Server.Messages_OCPP16
{
    public class ChargingSchedule
    {
        [JsonProperty("duration", Required = Required.Always)]
        public int Duration { get; set; }

        [JsonProperty("startSchedule", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime StartSchedule { get; set; }

        [JsonProperty("chargingRateUnitType", Required = Required.Always)]
        public ChargingRateUnitType ChargingRateUnitType { get; set; }

        [JsonProperty("chargingSchedulePeriod", Required = Required.Always)]
        public List<ChargingSchedulePeriod> ChargingSchedulePeriod { get; set; }

        [JsonProperty("minChargingRate", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public decimal MinChargingRate { get; set; }
    }
}
