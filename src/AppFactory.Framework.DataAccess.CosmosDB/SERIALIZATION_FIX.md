# CosmosDB Serialization Fix

## Problem

When storing objects in CosmosDB, they were being serialized with `valueKind` metadata instead of actual values:

```json
{
    "name": {
        "valueKind": 3
    },
    "description": {
        "valueKind": 3
    },
    "isActive": {
        "valueKind": 5
    }
}
```

## Root Cause

The Azure Cosmos SDK uses **System.Text.Json** by default. When deserializing JSON into `Dictionary<string, object>`, System.Text.Json creates `JsonElement` objects for values. When these `JsonElement` objects are serialized back to CosmosDB, they expose their internal structure (`valueKind`) rather than the actual values.

## Solution

Configured CosmosDB to use **Newtonsoft.Json** instead of System.Text.Json, which properly handles `Dictionary<string, object>` serialization.

### Changes Made

#### 1. Created Custom Serializer (`CosmosNewtonsoftJsonSerializer.cs`)

```csharp
public class CosmosNewtonsoftJsonSerializer : CosmosSerializer
{
    // Implements CosmosSerializer using Newtonsoft.Json
}
```

#### 2. Updated `CosmosDbClientFactory.cs`

```csharp
var clientOptions = new CosmosClientOptions
{
    Serializer = new CosmosNewtonsoftJsonSerializer(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Formatting = Formatting.None
    })
};
```

**Benefits:**
- ✅ Proper handling of `Dictionary<string, object>`
- ✅ CamelCase property naming (consistent with System.Text.Json behavior)
- ✅ Null value handling
- ✅ UTC timezone handling

#### 3. Updated `CosmosDbDocument.cs`

Changed from `System.Text.Json` to `Newtonsoft.Json`:

```csharp
// Before:
using System.Text.Json;
var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

// After:
using Newtonsoft.Json;
var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
```

**GetValue<T>() improvements:**
- Handles `JToken` types from Newtonsoft.Json
- Proper type conversion
- JSON round-trip for complex types

## Result

Objects are now serialized correctly:

```json
{
    "name": "My Catalog Item",
    "description": "Description text",
    "isActive": true,
    "parameters": [...],
    "id": "catalog-repository-roundtrip-probe"
}
```

## Testing

To verify the fix:

```csharp
var document = new CosmosDbDocument();
document["name"] = "Test Item";
document["isActive"] = true;
document["tags"] = new List<string> { "tag1", "tag2" };

await cosmosDbClient.UpsertItemAsync(document, "myContainer");

// Verify in CosmosDB - should see actual values, not valueKind
```

## Migration Notes

### For Existing Data

If you have existing documents with `valueKind` metadata:
1. Read documents using the old serializer
2. Extract actual values
3. Re-save using the new Newtonsoft.Json serializer

### Dependencies

The package already references `Newtonsoft.Json`, so no additional dependencies are required.

## Performance Considerations

- **Newtonsoft.Json** is slightly slower than System.Text.Json but more flexible
- For `Dictionary<string, object>` scenarios, it's the recommended approach
- Minimal performance difference for typical CRUD operations

## References

- [CosmosDB Custom Serializer Documentation](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-custom-serializer)
- [Newtonsoft.Json Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm)
- [System.Text.Json Dictionary Limitations](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/migrate-from-newtonsoft#dictionary-with-non-string-key)
