using Core;
using Core.Config;
using Core.DnfOfficialWebSite;
using Core.NeopleOpenApi;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AradMasterGenerator
{
    class Program
    {
        const string StringTableDirectoryName = "Resources/string_table";
        const string MasterDirectoryName = "master";
        const string SkillImageDirectoryName = "image/skill";
        const string JobImageDirectoryName = "image/job";

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
                    var baseGrowId = jobGrow.JobGrowId;
                    var growCount = 1;
                    for (var nextJobGrow = jobGrow; nextJobGrow != null; nextJobGrow = nextJobGrow.Next)
                    {
                        jobs.Add(new Core.Master.Model.Job
                        {
                            Id = job.JobId,
                            BaseGrowId = baseGrowId,
                            GrowId = nextJobGrow.JobGrowId,
                            GrowCount = growCount++,
                            NameKor = job.JobName,
                            GrowNameKor = nextJobGrow.JobName
                        });
                    }
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/jobs.json", JsonConvert.SerializeObject(jobs, Formatting.Indented));

            var dbJobIds = DB.Instance.Query<Core.Master.Model.Job>("select * from jobs").Select(x => $"{x.Id}{x.GrowId}");
            var diffJobs = jobs.Where(x => !dbJobIds.Contains($"{x.Id}{x.GrowId}"));
            if (diffJobs.Any())
            {
                DB.Instance.Insert(diffJobs);
            }
        }

        static async Task DownloadSkillIcon(HttpClient client, string url, string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateDirectoryIfNotExists(Path.GetDirectoryName(filePath) ?? default!);

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
                new Core.Master.Model.Skill { Id = "8a3b4a6cea49837706c36da3d9904f95", NameKor = "물리 백 어택", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 魔法バックアタック
                new Core.Master.Model.Skill { Id = "8a3b4a6cea49837706c36da3d9904f95", NameKor = "마법 백 어택", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 古代の記憶
                new Core.Master.Model.Skill { Id = "de13113fc6cb4c8880e8d985edb34aea", NameKor = "고대의 기억", RequiredLevel = 15, Type = "active", CostType = "SP" },
                // 不屈の意志
                new Core.Master.Model.Skill { Id = "dummy003", NameKor = "불굴의 의지", RequiredLevel = 15, Type = "active", CostType = "SP" },
                // 投擲マスタリー
                new Core.Master.Model.Skill { Id = "dummy004", NameKor = "투척 마스터리", RequiredLevel = 10, Type = "passive", CostType = "SP" },
                // コンバージョン
                new Core.Master.Model.Skill { Id = "12dca7fbf791e882b025a0d916181673", NameKor = "컨버전", RequiredLevel = 20, Type = "passive", CostType = "SP" }
            };

            foreach (var commonSkill in commonSkills)
            {
                var skillIcon = skillIcons.Where(x => x.Skill.NameKor == commonSkill.NameKor).FirstOrDefault();
                if (skillIcon == default)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(commonSkill, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }));
                    continue;
                    // throw new InvalidDataException();
                }

                commonSkill.IconUrl = skillIcon.Skill.IconUrl;
                skills.Add(commonSkill);
            }

            foreach (var job in await neopleOpenApiClient.GetJobs().ConfigureAwait(false))
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    var baseGrowId = jobGrow.JobGrowId;
                    for (var nextJobGrow = jobGrow; nextJobGrow != null; nextJobGrow = nextJobGrow.Next)
                    {
                        foreach (var skill in await neopleOpenApiClient.GetSkills(job.JobId, nextJobGrow.JobGrowId).ConfigureAwait(false))
                        {
                            var skillIcon = skillIcons
                                .Where(x => x.Skill.NameKor.Replace(" ", "") == skill.Name.Replace(" ", ""))
                                .Where(x => x.JobId == job.JobId)
                                .Where(x => x.BaseGrowId == baseGrowId)
                                .FirstOrDefault();

                            if (skillIcon == default)
                            {
                                // 一部スキル（「刹那の悟り」など）はWebページから削除されているので職業IDを無視して検索する
                                skillIcon = skillIcons
                                    .Where(x => x.Skill.NameKor.Replace(" ", "") == skill.Name.Replace(" ", ""))
                                    .FirstOrDefault();

                                if (skillIcon == default)
                                {
                                    // アイコンが見つからなかった
                                    Console.WriteLine(JsonConvert.SerializeObject(skill, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }));
                                    // throw new InvalidDataException();
                                }
                            }

                            // 一部スキルアイコンは公式サイト側が間違っているので修正する
                            if (skillIcon.Skill?.IconUrl == "http://d-fighter.dn.nexoncdn.co.kr/samsungdnf/neople/swf/2019/skill/7/icon/188.png")
                            {
                                skillIcon.Skill.IconUrl = "https://i.imgur.com/QndDlzz.png";
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
                                IconUrl = skillIcon.Skill?.IconUrl ?? null
                            });
                        }
                    }
                }
            }

            // i18n対応
            var skillStringTableDirectoryName = $"{StringTableDirectoryName}/skill";
            foreach (var fileName in new string[] { "kor_jpn.csv", "kor_eng.csv" })
            {
                Util.LoadStringTable($"{skillStringTableDirectoryName}/{fileName}");
                foreach (var skill in skills)
                {
                    var str = Util.GetString(skill.NameKor)?.Trim();
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (fileName.Contains("jpn"))
                        {
                            skill.NameJpn = str;
                        }
                        else if (fileName.Contains("eng"))
                        {
                            skill.NameEng = str;
                        }
                    }
                }
            }

            // スキルアイコンをダウンロードする
            var client = new HttpClient();
            Parallel.ForEach(skills, new ParallelOptions { MaxDegreeOfParallelism = 10 }, skill =>
            {
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
                filePath.Append($"{skill.Id}{Path.GetExtension(skill.IconUrl ?? ".png")}");

                skill.IconPath = $"/{filePath}";

                if (string.IsNullOrWhiteSpace(skill.IconUrl))
                {
                    return;
                }

                DownloadSkillIcon(client, skill.IconUrl ?? default!, filePath.ToString()).ConfigureAwait(false).GetAwaiter().GetResult();
            });

            File.WriteAllText($"{MasterDirectoryName}/skills.json", JsonConvert.SerializeObject(skills, Formatting.Indented));
            DB.Instance.ExecuteSQL("truncate table skills");
            DB.Instance.Insert(skills);
        }

        static async Task DownloadJobIcon()
        {
            var jobs = DB.Instance.Query<Core.Master.Model.Job>("select * from jobs").ToArray();

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).ConfigureAwait(false);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            }).ConfigureAwait(false);
            var page = await browser.NewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080
            }).ConfigureAwait(false);
            await page.GoToAsync("http://df.nexon.com/df/pg/character/srch").ConfigureAwait(false);
            var elements = await page.XPathAsync("//div[contains(@class, \"mix\")]").ConfigureAwait(false);

            foreach (var element in elements)
            {
                await element.EvaluateFunctionAsync("x => x.scrollIntoView()").ConfigureAwait(false);

                var jobNameKorElement = (await element.XPathAsync("a/div/p/strong").ConfigureAwait(false)).FirstOrDefault();
                var jobNameKor = (jobNameKorElement != null)
                    ? await jobNameKorElement.EvaluateFunctionAsync<string>("x => x.innerText").ConfigureAwait(false)
                    : string.Empty;
                var hasSex = (jobNameKor.Contains("(남)") || jobNameKor.Contains("(여)"));
                var sex = jobNameKor.Contains("(남)") ? "m" : "f";
                jobNameKor = jobNameKor.Replace("(남)", "").Replace("(여)", "").Replace(" ", "");

                var titleElement = (await element.XPathAsync("a/div/p").ConfigureAwait(false)).FirstOrDefault();
                if (titleElement != null)
                {
                    var jobQuery = jobs.Where(x => x.GrowNameKor?.Replace(" ", "") == jobNameKor || x.NameKor.Replace(" ", "") == jobNameKor);
                    if (hasSex)
                    {
                        jobQuery = jobQuery.Where(x => x.sex == sex);
                    }
                    var job = jobQuery.FirstOrDefault();

                    if (job == null)
                    {
                        continue;
                    }

                    await titleElement.EvaluateFunctionAsync("x => x.remove()").ConfigureAwait(false);
                    var path = $"{JobImageDirectoryName}/{job.Id}/{job.BaseGrowId ?? "0"}.png";
                    CreateDirectoryIfNotExists(Path.GetDirectoryName(path) ?? default!);
                    await element.ScreenshotAsync(path, new ScreenshotOptions { BurstMode = true }).ConfigureAwait(false);
                }
            }

            await browser.CloseAsync().ConfigureAwait(false);
        }

        static async Task Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CreateDirectoryIfNotExists(MasterDirectoryName);

            var neopleOpenApiClient = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKeys);
            var dnfOfficialWebSiteClient = new DnfOfficialWebSiteClient();

            // await GenerateJobMaster(neopleOpenApiClient).ConfigureAwait(false);
            // await DownloadJobIcon();

            await GenerateSkillMaster(neopleOpenApiClient, dnfOfficialWebSiteClient).ConfigureAwait(false);
        }
    }
}
