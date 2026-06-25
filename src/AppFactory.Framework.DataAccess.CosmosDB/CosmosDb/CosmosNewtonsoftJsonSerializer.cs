using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

/// <summary>
/// Custom CosmosDB serializer using Newtonsoft.Json to properly handle Dictionary&lt;string, object&gt; serialization.
/// This prevents the "valueKind" issue that occurs with System.Text.Json.
/// </summary>
public class CosmosNewtonsoftJsonSerializer : CosmosSerializer
{
    private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
    private readonly JsonSerializer _serializer;

    public CosmosNewtonsoftJsonSerializer(JsonSerializerSettings settings)
    {
        _serializer = JsonSerializer.Create(settings);
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return _serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var streamPayload = new MemoryStream();
        using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
        using (var jsonTextWriter = new JsonTextWriter(streamWriter))
        {
            jsonTextWriter.Formatting = _serializer.Formatting;
            _serializer.Serialize(jsonTextWriter, input);
            jsonTextWriter.Flush();
            streamWriter.Flush();
        }

        streamPayload.Position = 0;
        return streamPayload;
    }
}
