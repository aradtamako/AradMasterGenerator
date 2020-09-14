using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Master.Model
{
    [Table("jobs")]
    public class Job : IDataModel
    {
        [Column("id")]
        public string Id { get; set; } = default!;

        [Column("base_grow_id")]
        public string? BaseGrowId { get; set; }

        [Column("grow_id")]
        public string? GrowId { get; set; }

        [Column("grow_count")]
        public int GrowCount { get; set; }

        [Column("combat_type")]
        public string CombatType { get; set; } = default!;

        [Column("attack_type")]
        public string AttackType { get; set; } = default!;

        [Column("damage_type")]
        public string DamageType { get; set; } = default!;

        [Column("sex")]
        public string sex { get; set; } = default!;

        [Column("name_kor")]
        public string NameKor { get; set; } = default!;

        [Column("name_eng")]
        public string? NameEng { get; set; }

        [Column("name_jpn")]
        public string? NameJpn { get; set; }

        [Column("name_zho")]
        public string? NameZho { get; set; }

        [Column("grow_name_kor")]
        public string GrowNameKor { get; set; } = default!;

        [Column("grow_name_eng")]
        public string? GrowNameEng { get; set; }

        [Column("grow_name_jpn")]
        public string? GrowNameJpn { get; set; }

        [Column("grow_name_zho")]
        public string? GrowNameZho { get; set; }
    }
}
