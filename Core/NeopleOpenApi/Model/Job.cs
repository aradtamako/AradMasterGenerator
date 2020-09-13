using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.NeopleOpenApi.Model
{
    public class Job
    {
        [JsonProperty("jobId")]
        public string JobId { get; set; } = default!;

        [JsonProperty("jobName")]
        public string JobName { get; set; } = default!;

        [JsonProperty("rows")]
        public JobGrow[]? JobGrows { get; set; }
    }
}
