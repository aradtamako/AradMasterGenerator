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
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AradMasterGenerator
{
    class Program
    {
        const string MasterDirectoryName = "master";
        const string SkillImageDirectoryName = "master/image/skill";

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
                jobs.Add(new Core.Master.Model.Job
                {
                    Id = job.JobId,
                    NameKor = job.JobName
                });

                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    for (var nextJobGrow = jobGrow; nextJobGrow != null; nextJobGrow = nextJobGrow.Next)
                    {
                        jobs.Add(new Core.Master.Model.Job
                        {
                            Id = job.JobId,
                            GrowId = nextJobGrow.JobGrowId,
                            NameKor = job.JobName,
                            GrowNameKor = nextJobGrow.JobName
                        });
                    }
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/jobs.json", JsonConvert.SerializeObject(jobs, Formatting.Indented));
        }

        static async Task DownloadSkillIcon(HttpClient client, string url, string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateDirectoryIfNotExists(Path.GetDirectoryName(filePath) ?? default!);

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fs = File.OpenWrite(filePath);
                stream.CopyTo(fs);
            }
        }

        static async Task GenerateSkillMaster(NeopleOpenApiClient neopleOpenApiClient, DnfOfficialWebSiteClient dnfOfficialWebSiteClient)
        {
            var skills = new List<Core.Master.Model.Skill>();
            var skillIcons = await dnfOfficialWebSiteClient.GetSkillIcons();

            // 全職業共通スキル
            var commonSkills = new List<Core.Master.Model.Skill>
            {
                // バックステップ
                new Core.Master.Model.Skill { Id = "7822d6d52e10964a6755f142c666b494", NameKor = "백스텝", RequiredLevel = 1, Type = "active", CostType = "SP" },
                // クイックスタンディング
                new Core.Master.Model.Skill { Id = "ce26c6b69d02a440a81b552bec94f03b", NameKor = "퀵 스탠딩", RequiredLevel = 1, Type = "active", CostType = "SP" },
                // 基本技熟練
                new Core.Master.Model.Skill { Id = "5a56514f35cf0270ae8d6c65f8fefd78", NameKor = "기본기 숙련", RequiredLevel = 1, Type = "passive", CostType = "SP" },
                // 跳躍
                new Core.Master.Model.Skill { Id = "1fea5a626f15230237946a11a9d11582", NameKor = "도약", RequiredLevel = 10, Type = "active", CostType = "SP" },
                // 物理クリティカルヒット
                new Core.Master.Model.Skill { Id = "fc1262c19f3d0477ee8eda47b8db8696", NameKor = "물리 크리티컬 히트", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 魔法クリティカルヒット
                new Core.Master.Model.Skill { Id = "fc1262c19f3d0477ee8eda47b8db8696", NameKor = "마법 크리티컬 히트", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 物理バックアタック
                new Core.Master.Model.Skill { Id = "dummy001", NameKor = "물리 백 어택", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 魔法バックアタック
                new Core.Master.Model.Skill { Id = "dummy002", NameKor = "마법 백 어택", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 古代の記憶
                new Core.Master.Model.Skill { Id = "de13113fc6cb4c8880e8d985edb34aea", NameKor = "고대의 기억", RequiredLevel = 15, Type = "active", CostType = "SP" },
                // 不屈の意志
                new Core.Master.Model.Skill { Id = "dummy003", NameKor = "불굴의 의지", RequiredLevel = 15, Type = "active", CostType = "SP" },
                // 投擲マスタリー
                new Core.Master.Model.Skill { Id = "dummy004", NameKor = "투척 마스터리", RequiredLevel = 10, Type = "passive", CostType = "SP" },
                // コンバージョン
                new Core.Master.Model.Skill { Id = "12dca7fbf791e882b025a0d916181673", NameKor = "컨버전", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // ホウキコントロール（クリエイター用）
                new Core.Master.Model.Skill { Id = "5152480fdde81362575a488d4cec4af9", NameKor = "빗자루 다루기", RequiredLevel = 1, Type = "passive", CostType = "SP" },
            };

            foreach (var commonSkill in commonSkills)
            {
                var skillIcon = skillIcons.Where(x => x.NameKor == commonSkill.NameKor).FirstOrDefault();
                commonSkill.IconUrl = skillIcon.IconUrl;
                skills.Add(commonSkill);
            }

            foreach (var job in await neopleOpenApiClient.GetJobs().ConfigureAwait(false))
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    for (var nextJobGrow = jobGrow; nextJobGrow != null; nextJobGrow = nextJobGrow.Next)
                    {
                        foreach (var skill in await neopleOpenApiClient.GetSkills(job.JobId, nextJobGrow.JobGrowId).ConfigureAwait(false))
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
                                JobGrowId = nextJobGrow.JobGrowId,
                                RequiredLevel = skill.RequiredLevel,
                                Type = skill.Type,
                                CostType = skill.CostType,
                                NameKor = skill.Name,
                                IconUrl = skillIcon.IconUrl
                            });
                        }
                    }
                }
            }

            var client = new HttpClient();
            Parallel.ForEach(skills, new ParallelOptions { MaxDegreeOfParallelism = 10 }, skill =>
            {
                if (string.IsNullOrWhiteSpace(skill.IconUrl))
                {
                    return;
                }

                if(skill.IconUrl.StartsWith("//"))
                {
                    skill.IconUrl = $"http:{skill.IconUrl}";
                }

                var filePath = new StringBuilder();
                filePath.Append($"{SkillImageDirectoryName}/");
                if (!string.IsNullOrEmpty(skill.JobId))
                {
                    filePath.Append($"{skill.JobId}/");
                }
                if (!string.IsNullOrEmpty(skill.JobGrowId))
                {
                    filePath.Append($"{skill.JobGrowId}/");
                }
                filePath.Append($"{skill.Id}.{Path.GetExtension(skill.IconUrl)}");

                skill.IconPath = filePath.ToString();

                DownloadSkillIcon(client, skill.IconUrl ?? default!, filePath.ToString()).ConfigureAwait(false).GetAwaiter().GetResult();
            });

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
