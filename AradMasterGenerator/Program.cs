using Core;
using Core.Config;
using Core.DnfOfficialWebSite;
using Core.Master.Model;
using Core.NeopleOpenApi;
using Core.NeopleOpenApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        static async Task GenerateJobMaster(NeopleOpenApiClient neopleOpenApiClient)
        {
            var jobs = new List<Core.Master.Model.Job>();
            foreach (var job in await neopleOpenApiClient.GetJobs().ConfigureAwait(false))
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

        static async Task GenerateSkillMaster(NeopleOpenApiClient neopleOpenApiClient, DnfOfficialWebSiteClient dnfOfficialWebSiteClient)
        {
            var skills = new List<Core.Master.Model.Skill>();
            var skillIcons = await dnfOfficialWebSiteClient.GetSkillIcons();

            foreach (var job in await neopleOpenApiClient.GetJobs().ConfigureAwait(false))
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    foreach (var skill in await neopleOpenApiClient.GetSkills(job.JobId, jobGrow.JobGrowId).ConfigureAwait(false))
                    {
                        var skillIcon = skillIcons.Where(x => x.NameKor.Replace(" ", "") == skill.Name.Replace(" ", "")).FirstOrDefault();

                        if (skillIcon == null)
                        {
                            throw new InvalidDataException();
                        }

                        skills.Add(new Core.Master.Model.Skill
                        {
                            Id = skill.SkillId,
                            JobId = job.JobId,
                            JobGrowId = jobGrow.JobGrowId,
                            RequiredLevel = skill.RequiredLevel,
                            Type = skill.Type,
                            CostType = skill.CostType,
                            NameKor = skill.Name,
                            IconUrl = skillIcon.IconUrl
                        });
                    }
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/skills.json", JsonConvert.SerializeObject(skills, Formatting.Indented));
        }

        static async Task Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CreateDirectoryIfNotExists(MasterDirectoryName);

            var neopleOpenApiClient = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKeys);
            var dnfOfficialWebSiteClient = new DnfOfficialWebSiteClient();

            await GenerateJobMaster(neopleOpenApiClient).ConfigureAwait(false);
            await GenerateSkillMaster(neopleOpenApiClient, dnfOfficialWebSiteClient).ConfigureAwait(false);

            // DB.Instance.Insert(JsonConvert.DeserializeObject<Core.Master.Model.Skill[]>(File.ReadAllText($"{MasterDirectoryName}/skills.json")));
        }
    }
}
