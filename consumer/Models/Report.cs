using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace consumer.Models;
public class Report
{
    [Required]
    public string reportId { get; set; } = string.Empty;
    [Required]
    [JsonPropertyName("@timestamp")]
    public DateTime timestamp { get; set; }
    [Required]
    public string agentId { get; set; } = string.Empty;
    [Required]
    public string unit { get; set; } = string.Empty;
    [Required]
    public string theater { get; set; } = string.Empty;
    [Required]
    public string sector { get; set; } = string.Empty;
    [Required]
    public string location { get; set; } = string.Empty;
    [Required]
    [RegularExpression("^(Observation|Movement|Meeting|Access|Communication|Logistics|Incident)$")]
    public string reportType { get; set; } = string.Empty;
    [Required]
    [RegularExpression("^(Low|Medium|High|Critical)$")]
    public string priority { get; set; } = string.Empty;
    [Required]
    public string sourceType { get; set; } = string.Empty;
    [Required]
    public string message { get; set; } = string.Empty;
    public string? subjectId { get; set; } = null;
    public string? subjectType { get; set; } = null;
    public DateTime processedAt { get; set; } = DateTime.UtcNow;


}
