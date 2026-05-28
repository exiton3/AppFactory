using System.Text.Json;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

public class CosmosDbDocument : Dictionary<string, object>
{
    public CosmosDbDocument()
    {
    }

    public CosmosDbDocument(Dictionary<string, object> items)
    {
        foreach (var item in items)
        {
            Add(item.Key, item.Value);
        }
    }

    public CosmosDbDocument(string json)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        if (dict != null)
        {
            foreach (var item in dict)
            {
                Add(item.Key, item.Value);
            }
        }
    }

    public void Merge(Dictionary<string, object> items)
    {
        foreach (var item in items)
        {
            this[item.Key] = item.Value;
        }
    }

    public T GetValue<T>(string key)
    {
        if (TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            return (T)value;
        }
        return default;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
