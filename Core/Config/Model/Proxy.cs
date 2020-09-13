using Newtonsoft.Json;

namespace Core.Config.Model
{
    public class Proxy
    {
        [JsonProperty("host")]
        public string? Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
