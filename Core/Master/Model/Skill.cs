using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Master.Model
{
    [Table("skills")]
    public class Skill
    {
        [Column("id")]
        public string Id { get; set; } = default!;

        [Column("job_id")]
        public string JobId { get; set; } = default!;

        [Column("job_grow_id")]
        public string JobGrowId { get; set; } = default!;

        [Column("required_level")]
        public int RequiredLevel { get; set; }

        [Column("type")]
        public string Type { get; set; } = default!;

        [Column("cost_type")]
        public string CostType { get; set; } = default!;

        [Column("name_kor")]
        public string NameKor { get; set; } = default!;

        [Column("name_eng")]
        public string? NameEng { get; set; }

        [Column("name_jpn")]
        public string? NameJpn { get; set; }

        [Column("name_zho")]
        public string? NameZho { get; set; }

        [Column("icon_url")]
        public string? IconUrl { get; set; }
    }
}
