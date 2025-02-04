using System;
using Newtonsoft.Json;

public class JobData
{
    // [JsonIgnore]
    // public string? JobId { get; set; }

    [JsonProperty("id")] 
    public string? Id { get; set; } 

    [JsonProperty("UserId")]
    public string UserId { get; set; }

    [JsonProperty("title")]
    public string JobTitle { get; set; }

    [JsonProperty("description")]
    public string JobDescription { get; set; }

    // [JsonProperty("tradesrequired")]
    // public string TradesRequired { get; set; }

    // [JsonProperty("joblocation")]
    // public string Location { get; set; }

    // [JsonProperty("assignedtradesman")]
    // public string AssignedTradesman { get; set; }
    

    // [JsonProperty("postedDate")]
    // public DateTime PostedDate { get; set; }

    // [JsonProperty("isActive")]   
    // public bool IsActive { get; set; }
}