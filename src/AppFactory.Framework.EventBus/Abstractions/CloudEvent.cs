using System.Text.Json.Serialization;

namespace AppFactory.Framework.EventBus.Abstractions;

/// <summary>
/// Base implementation of CloudEvents specification
/// https://cloudevents.io/
/// </summary>
public class CloudEvent : IEvent
{
    public CloudEvent()
    {
        EventId = Guid.NewGuid().ToString();
        OccurredAt = DateTime.UtcNow;
        Version = 1;
        Metadata = new Dictionary<string, string>();
    }

    [JsonPropertyName("id")]
    public string EventId { get; set; }

    [JsonPropertyName("type")]
    public string EventType { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("time")]
    public DateTime OccurredAt { get; set; }

    [JsonPropertyName("specversion")]
    public string SpecVersion { get; set; } = "1.0";

    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; set; } = "application/json";

    [JsonPropertyName("data")]
    public object Data { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonIgnore]
    public int Version { get; set; }

    [JsonExtensionData]
    public IDictionary<string, string> Metadata { get; set; }
}
