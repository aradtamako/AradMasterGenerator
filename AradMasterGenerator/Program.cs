using Core.Config;
using Core.NeopleOpenApi;
using Core.NeopleOpenApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AradMasterGenerator
{
    class Program
    {
        static async Task GenerateJobMaster(NeopleOpenApiClient client)
        {
            var jobs = new List<Master.Model.Job>();
            foreach (var job in (await client.GetJobList().ConfigureAwait(false)).Jobs)
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    jobs.Add(new Master.Model.Job
                    {
                        Id = job.JobId,
                        GrowId = jobGrow.JobGrowId,
                        NameKor = job.JobName,
                        GrowNameKor = jobGrow.JobName
                    });
                }
            }

            File.WriteAllText("job_master.json", JsonConvert.SerializeObject(jobs, Formatting.Indented));
        }

        static async Task Main()
        {
            var client = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKeys);
            await GenerateJobMaster(client).ConfigureAwait(false);
            return;

            var jobId = "41f1cdc2ff58bb5fdc287be0db2a8df3";
            var jobGrowId = "df3870efe8e8754011cd12fa03cd275f";
            var skills = (await client.GetSkillList(jobId, jobGrowId).ConfigureAwait(false)).Skills;
            Console.WriteLine(JsonConvert.SerializeObject(skills));

            foreach (var skill in skills)
            {
                var skillDetail = await client.GetSkillDetail(jobId, skill.SkillId).ConfigureAwait(false);
                Console.WriteLine(JsonConvert.SerializeObject(skillDetail));
            }
        }
    }
}
