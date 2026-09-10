using Api.Models;

namespace Api.Service;

public interface IReportSearchService
{
    Task<IEnumerable<Report>> SearchMessagesAsync(string text);

}
