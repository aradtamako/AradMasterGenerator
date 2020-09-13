using Newtonsoft.Json;
using System.IO;
using Core.Config.Model;

namespace Core.Config
{
    public class Config
    {
        const string FileName = "Resources/config.json";

        public static Config Instance { get; private set; } = new Config();

        [JsonProperty("neople_open_api")]
        public Model.NeopleOpenApi NeopleOpenApi { get; set; } = default!;

        [JsonProperty("proxy")]
        public Proxy Proxy { get; set; } = default!;

        [JsonProperty("database")]
        public Database Database { get; set; } = default!;

        static Config()
        {
            Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(FileName));
        }
    }
}
