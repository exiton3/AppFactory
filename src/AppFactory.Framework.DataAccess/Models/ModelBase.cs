using System.Text.Json.Serialization;

namespace AppFactory.Framework.DataAccess.Models;

public class ModelBase
{
    [JsonPropertyName("PK")]
    public string PK { get; set; }

    [JsonPropertyName("SK")]
    public string SK { get; set; }

    public string Id { get; set; }
}