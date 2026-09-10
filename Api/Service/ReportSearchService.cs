using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Service;

public class ReportSearchService : IReportSearchService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "report-index";

    public ReportSearchService(ElasticsearchClient client)
    {
        _client = client; 
    }


}
