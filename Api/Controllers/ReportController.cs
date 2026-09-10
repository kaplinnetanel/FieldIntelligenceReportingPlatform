using Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
using Serilog;

[ApiController]
[Route("[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportSearchService _service;

    public ReportController(IReportSearchService service)
    {
        _service = service;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
