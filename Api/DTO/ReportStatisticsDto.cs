namespace Api.DTO
{

    public class ReportStatisticsDto
    {
        public long TotalReports { get; set; }
        public List<StatItemDto> ByPriority { get; set; } = new();
        public List<StatItemDto> ByTheater { get; set; } = new();
        public List<StatItemDto> ByReportType { get; set; } = new();
    }

    public class StatItemDto
    {
        public string Name { get; set; } = string.Empty;
        public long Count { get; set; }
    }
}
