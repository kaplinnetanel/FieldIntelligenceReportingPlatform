using Elastic.Clients.Elasticsearch;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using consumer.Models;

namespace consumer.Service;

public class ProcessingService
{
    private readonly ElasticsearchClient _elasticClient;
    public string indexName = "report-index";

    public ProcessingService(ElasticsearchClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task<bool> ProcessEventAsync(string jsonMessage)
    {
        Report? surveyData;
        try
        {
            surveyData = JsonSerializer.Deserialize<Report>(jsonMessage);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to deserialize JSON: {Error}", ex.Message);
            return true;
        }

        if (surveyData == null)
        {
            return true;
        }

        var validationContext = new ValidationContext(surveyData);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(surveyData, validationContext, validationResults, true);

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                Log.Warning("Validation failed for Report ID {Id}: {Error}", surveyData.reportId, validationResult.ErrorMessage);
            }
            return true;
        }

        bool hasSuubjectId = !string.IsNullOrWhiteSpace(surveyData.subjectId);
        bool hasSuubjectype = !string.IsNullOrWhiteSpace(surveyData.subjectType);
        if (hasSuubjectId != hasSuubjectype)
        {
            Log.Warning("Validation failed: subjectId and subjectType must appear together or be absent together for Report ID {Id}", surveyData.reportId);
            return true;
        }

        surveyData.processedAt = DateTime.UtcNow;

        // אינדוקס ישיר עם ה-Id של הדיווח. אלסטיק ידאג לבדוק ייחודיות ודורס/שומר אוטומטית
        var response = await _elasticClient.IndexAsync(surveyData, idx => idx.Index(indexName).Id(surveyData.reportId));

        if (!response.IsValidResponse)
        {
            var errorReason = response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation;
            Log.Error("Failed to index report {Id}. Elasticsearch Error: {Reason}", surveyData.reportId, errorReason);
            return true;
        }

        return true;
    }
}