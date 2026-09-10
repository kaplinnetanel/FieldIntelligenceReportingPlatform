using Api.DTO;
using Api.Models;
using Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class ReportController : ControllerBase
{
    private readonly IReportSearchService _service;

    public ReportController(IReportSearchService service)
    {
        _service = service;
    }

    [HttpGet("report/search")]
    public async Task<IActionResult> SearchReports(string text)
    {
        var reports = await _service.SearchMessagesAsync(text);
        return Ok(reports);
    }

    [HttpGet("subjects/{subjectId}/reports")]
    public async Task<IActionResult> GetReportsBySubject(int subjectId)
    {
        var reports = await _service.GetBySubjectIdAsync(subjectId);
        return Ok(reports);
    }

    [HttpGet("reports/")]
    public async Task<IActionResult> GetByAreaAsync(string? theater, string? sector, string? location)
    {
        var reports = await _service.GetByAreaAsync(theater, sector, location);
        return Ok(reports);
    }

    [HttpGet("report/")]
    public async Task<ActionResult<IEnumerable<Report>>> GetReports(string? priorities, DateTime? from, DateTime? to)
    {
        var reports = await _service.GetByPrioritiesAndDateRangeAsync(priorities, from, to);
        return Ok(reports);
    }

    [HttpGet("reports/search")]
    public async Task<ActionResult<IEnumerable<Report>>> SearchAdvanced(string? text,string? theater, string? sector,string? location,string? priorities, string? reportType,DateTime? from,DateTime? to)
    {
        var reports = await _service.SearchReports(text, theater, sector, location, priorities, reportType, from, to);
        return Ok(reports);
    }



    [HttpGet("report/statistics")]
    public async Task<ActionResult<ReportStatisticsDto>> GetStatistics()
    {
        var statistics = await _service.GetStatisticsAsync();
        return Ok(statistics);
    }
}