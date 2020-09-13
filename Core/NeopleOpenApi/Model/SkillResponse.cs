using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.NeopleOpenApi.Model
{
    public class SkillResponse
    {
        [JsonProperty("skills")]
        public Skill[] Skills { get; set; } = default!;
    }
}
