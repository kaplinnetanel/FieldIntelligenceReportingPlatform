using Api.DTO;
using Api.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Inference;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch.Aggregations;


namespace Api.Service;

public class ReportSearchService : IReportSearchService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "report-index";

    public ReportSearchService(ElasticsearchClient client)
    {
        _client = client;
    }
    public async Task<IEnumerable<Report>> SearchMessagesAsync(string text)
    {
        Log.Information("Executing Elasticsearch full-text search with query text: {Text}", text);
        var response = await _client.SearchAsync<Report>(s =>
        s.Indices(IndexName).Query(q => q.Match(m => m.Field(f => f.message).Query(text))));
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Elasticsearch search failed: {response.ElasticsearchServerError?.Error?.Reason}");
        }
        return response.Documents;
    }
    public async Task<IEnumerable<Report>> GetBySubjectIdAsync(int subjectId)
    {
        Log.Information("Fetching reports for SubjectId: {SubjectId}", subjectId);
        var response = await _client.SearchAsync<Report>(s =>
        s.Indices(IndexName).Query(q => q.Term(t => t.Field(f => f.subjectId).Value(subjectId))
        ).Sort(sort => sort.Field(f => f.timestamp, f => f.Order(SortOrder.Asc)))
        );
       
        if (!response.IsValidResponse) throw new InvalidOperationException(response.ElasticsearchServerError?.Error?.Reason);
        Log.Information("Successfully retrieved {Count} reports from Elasticsearch for text query.", response.Documents.Count);
        return response.Documents;

    }
    public async Task<IEnumerable<Report>> GetByAreaAsync(string? theater, string? sector, string? location)
    {
        Log.Information("Fetching reports by area - Theater: {Theater}, Sector: {Sector}, Location: {Location}", theater, sector, location);
        var list = new List<Query>();

        if (!string.IsNullOrEmpty(theater))
            list.Add(new TermQuery { Field = "theater.keyword", Value = theater });
        if (!string.IsNullOrEmpty(sector))
            list.Add(new TermQuery { Field = "sector.keyword", Value = sector });
        if (!string.IsNullOrEmpty(location))
            list.Add(new TermQuery { Field = "location.keyword", Value = location });
        var response = await _client.SearchAsync<Report>(s => s
          .Indices(IndexName)
          .Query(q => q
              .Bool(b => b
                  .Filter(list)
              )
          )
      );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(response.ElasticsearchServerError?.Error?.Reason);
        }
        Log.Information("Successfully retrieved {Count} reports by area parameters.", response.Documents.Count);
        return response.Documents;
    }

    public async Task<IEnumerable<Report>> GetByPrioritiesAndDateRangeAsync(string? priorities, DateTime? from, DateTime? to)
    {
        Log.Information("Fetching reports by priorities: {Priorities}, From: {From}, To: {To}", priorities, from, to);
        var list = new List<Query>();

        if (!string.IsNullOrEmpty(priorities))
        {
            var priorityValues = priorities.Split(',')
                                           .Select(p => (FieldValue)p.Trim())
                                           .ToArray();

            list.Add(new TermsQuery
            {
                Field = "priority.keyword",
                Terms = priorityValues
            });
        }

        if (from.HasValue || to.HasValue)
        {
            var dateQuery = new DateRangeQuery { Field = "@timestamp" };

            if (from.HasValue)
                dateQuery.Gte = from.Value;

            if (to.HasValue)
                dateQuery.Lte = to.Value;

            list.Add(dateQuery);
        }

        var response = await _client.SearchAsync<Report>(s => s
            .Indices(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(list)
                )
            )
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(response.ElasticsearchServerError?.Error?.Reason);
        }
        Log.Information("Successfully retrieved {Count} reports by priorities and date range.", response.Documents.Count);
        return response.Documents;
    }

    public async Task<IEnumerable<Report>> SearchReports(string? text, string? theater, string? sector, string? location, string? priorities, string? reportType, DateTime? from, DateTime? to)
    {
        Log.Information("Executing advanced combined search across multiple parameters.");
        var list = new List<Query>();

        if (!string.IsNullOrEmpty(text))
        {
            list.Add(new MatchQuery
            {
                Field = "message",
                Query = text

            });
        }
        if (!string.IsNullOrEmpty(theater))
        {
            list.Add(new TermQuery
            {
                Field = "theater.keyword",
                Value = theater
            });
        }
        if (!string.IsNullOrEmpty(sector))
        {
            list.Add(new TermQuery
            {
                Field = "sector.keyword",
                Value = sector
            });
        }
        if (!string.IsNullOrEmpty(location))
        {
            list.Add(new TermQuery
            {
                Field = "location.keyword",
                Value = location
            });
        }
        if (!string.IsNullOrEmpty(priorities))
        {
            var priorityArray = priorities.Split(',')
                                           .Select(p => (FieldValue)p.Trim())
                                           .ToArray();
            list.Add(new TermsQuery
            {
                Field = "priority.keyword",
                Terms = priorityArray
            });

        }
        if (from.HasValue || to.HasValue)
        {
            var dateQuery = new DateRangeQuery { Field = "@timestamp" };
            if (from.HasValue) dateQuery.Gte = from.Value;
            if (to.HasValue) dateQuery.Lte = to.Value;
            list.Add(dateQuery);
        }

        var response = await _client.SearchAsync<Report>(s => s
            .Indices(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(list)
                )
            )
        );
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(response.ElasticsearchServerError?.Error?.Reason);
        }
        Log.Information("Advanced search completed successfully, returning {Count} documents.", response.Documents.Count);

        return response.Documents;
    }

    public async Task<ReportStatisticsDto> GetStatisticsAsync()
    {
        Log.Information("Generating report statistics aggregation from Elasticsearch.");
        var response = await _client.SearchAsync<Report>(s => s
            .Indices(IndexName)
            .Size(0)
            .Aggregations(a =>
            {
                a.Add("by_priority", new TermsAggregation { Field = "priority.keyword" });
                a.Add("by_theater", new TermsAggregation { Field = "theater.keyword" });
                a.Add("by_report_type", new TermsAggregation { Field = "reportType.keyword" });
            })
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(response.ElasticsearchServerError?.Error?.Reason);
        }

        var priorityStats = ((StringTermsAggregate)response.Aggregations["by_priority"])
         .Buckets
         .Select(b => new StatItemDto { Name = b.Key.ToString()!, Count = (long)b.DocCount })
         .ToList();

        var theaterStats = ((StringTermsAggregate)response.Aggregations["by_theater"])
            .Buckets
            .Select(b => new StatItemDto { Name = b.Key.ToString()!, Count = (long)b.DocCount })
            .ToList();

        var reportTypeStats = ((StringTermsAggregate)response.Aggregations["by_report_type"])
            .Buckets
            .Select(b => new StatItemDto { Name = b.Key.ToString()!, Count = (long)b.DocCount })
            .ToList();
        Log.Information("Report statistics successfully generated. Total reports: {Total}", response.Total);
        return new ReportStatisticsDto
        {
            TotalReports = response.Total,
            ByPriority = priorityStats,
            ByTheater = theaterStats,
            ByReportType = reportTypeStats
        };
    }

}







