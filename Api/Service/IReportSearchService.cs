using Api.DTO;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Service;

public interface IReportSearchService
{
    Task<IEnumerable<Report>> SearchMessagesAsync(string text);
    Task<IEnumerable<Report>> GetBySubjectIdAsync(int subjectId);
    Task<IEnumerable<Report>> GetByAreaAsync(string? theater, string? sector, string? location);
    Task<IEnumerable<Report>> GetByPrioritiesAndDateRangeAsync(string? priorities, DateTime? from, DateTime? to);
    Task<IEnumerable<Report>> SearchReports(string? text, string? theater, string? sector, string? location, string? priorities, string? reportType,DateTime? from, DateTime? to);
    Task<ReportStatisticsDto> GetStatisticsAsync();
}
