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
        const string MasterDirectoryName = "master";

        static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        static async Task GenerateJobMaster(NeopleOpenApiClient client)
        {
            var jobs = new List<Core.Master.Model.Job>();
            foreach (var job in (await client.GetJobList().ConfigureAwait(false)).Jobs)
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    jobs.Add(new Core.Master.Model.Job
                    {
                        Id = job.JobId,
                        GrowId = jobGrow.JobGrowId,
                        NameKor = job.JobName,
                        GrowNameKor = jobGrow.JobName
                    });
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/jobs.json", JsonConvert.SerializeObject(jobs, Formatting.Indented));
        }

        static async Task GenerateSkillMaster(NeopleOpenApiClient client)
        {
            var skills = new List<Core.Master.Model.Skill>();
            foreach (var job in (await client.GetJobList().ConfigureAwait(false)).Jobs)
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    foreach (var skill in (await client.GetSkillList(job.JobId, jobGrow.JobGrowId).ConfigureAwait(false)).Skills)
                    {
                        skills.Add(new Core.Master.Model.Skill
                        {
                            Id = skill.SkillId,
                            JobId = job.JobId,
                            JobGrowId = jobGrow.JobGrowId,
                            RequiredLevel = skill.RequiredLevel,
                            Type = skill.Type,
                            CostType = skill.CostType,
                            NameKor = skill.Name
                        });
                    }
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/skills.json", JsonConvert.SerializeObject(skills, Formatting.Indented));
        }

        static async Task Main()
        {
            CreateDirectoryIfNotExists(MasterDirectoryName);

            var client = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKeys);
            await GenerateJobMaster(client).ConfigureAwait(false);
            await GenerateSkillMaster(client).ConfigureAwait(false);
        }
    }
}
