namespace Core.Master.Model
{
    public class Job
    {
        public string Id { get; set; } = default!;
        public string GrowId { get; set; } = default!;

        public string NameKor { get; set; } = default!;
        public string? NameEng { get; set; }
        public string? NameJpn { get; set; }
        public string? NameZho { get; set; }

        public string GrowNameKor { get; set; } = default!;
        public string? GrowNameEng { get; set; }
        public string? GrowNameJpn { get; set; }
        public string? GrowNameZho { get; set; }
    }
}
