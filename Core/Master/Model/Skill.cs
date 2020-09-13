namespace Core.Master.Model
{
    public class Skill
    {
        public string Id { get; set; } = default!;

        public string JobId { get; set; } = default!;
        public string JobGrowId { get; set; } = default!;

        public int RequiredLevel { get; set; }
        public string Type { get; set; } = default!;
        public string CostType { get; set; } = default!;

        public string NameKor { get; set; } = default!;
        public string? NameEng { get; set; }
        public string? NameJpn { get; set; }
        public string? NameZho { get; set; }
    }
}
