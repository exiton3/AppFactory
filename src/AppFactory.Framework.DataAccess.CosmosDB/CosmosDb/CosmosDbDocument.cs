using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
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
            // Handle Newtonsoft.Json JToken types
            if (value is JToken jToken)
            {
                return jToken.ToObject<T>();
            }

            // Handle direct conversion
            if (value is T typedValue)
            {
                return typedValue;
            }

            // Try JSON round-trip for complex types
            var json = JsonConvert.SerializeObject(value);
            return JsonConvert.DeserializeObject<T>(json);
        }
        return default;
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}
