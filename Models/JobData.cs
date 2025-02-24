using System;
using Newtonsoft.Json;

public class JobData
{
    [JsonProperty("id")] 
    public string? Id { get; set; } 

    [JsonProperty("UserId")]
    public required string UserId { get; set; }

    [JsonProperty("JobTitle")]
    public required string JobTitle { get; set; }

    [JsonProperty("JobDescription")]
    public required string JobDescription { get; set; }

    [JsonProperty("TradesRequired")]
    public required string TradesRequired { get; set; }

    [JsonProperty("JobLocation")]
    public required string JobLocation { get; set; }

    [JsonProperty("JobImage")]
    public string JobImage { get; set; }

    [JsonProperty("AssignedTradesman")]
    public string? AssignedTradesman { get; set; } = null;    

    [JsonProperty("JobCreatedAt")]
    public DateTime JobCreatedAt { get; set; } = DateTime.UtcNow; 

    [JsonProperty("JobStatus")]   
    public string JobStatus { get; set; } = "Open";
}