using System;
using Newtonsoft.Json;

public class JobData
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("postedDate")]
    public DateTime PostedDate { get; set; }

    [JsonProperty("isActive")]
    public bool IsActive { get; set; }
}