using System;

namespace OCPP.Core.Server.Models
{
    public class DebugLog
    {
        public string Action { get; set; }

        public int MessageType { get; set; }

        public string TransactionName { get; set; }

        public string ReguestJsonPayload { get; set; }

        public string ResponseJsonPayload { get; set; }

        public DateTime? RequestTime { get; set; }

        public DateTime? ResponseTime { get; set; }
    }
}
