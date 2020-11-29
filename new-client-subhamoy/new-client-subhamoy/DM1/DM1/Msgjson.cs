using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM1
{
    public partial class Msgjson
    {
        [JsonProperty("payload")]
        public Payload1 Payload1 { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("virtual_sensor_id")]
        public long VirtualSensorId { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    public partial class Payload1
    {
        [JsonProperty("Message")]
        public string Message { get; set; }
    }

}


