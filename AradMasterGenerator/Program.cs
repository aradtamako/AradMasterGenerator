using Core.Config;
using Core.NeopleOpenApi;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AradMasterGenerator
{
    class Program
    {
        static async Task Main()
        {
            var client = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKey);
            var jobs = await client.GetJobList();
            Console.WriteLine(JsonConvert.SerializeObject(jobs));

            var jobId = "41f1cdc2ff58bb5fdc287be0db2a8df3";
            var jobGrowId = "df3870efe8e8754011cd12fa03cd275f";
            var skills = (await client.GetSkillList(jobId, jobGrowId)).Skills;
            Console.WriteLine(JsonConvert.SerializeObject(skills));

            foreach (var skill in skills)
            {
                var skillDetail = await client.GetSkillDetail(jobId, skill.SkillId);
                Console.WriteLine(JsonConvert.SerializeObject(skillDetail));
            }
        }
    }
}
