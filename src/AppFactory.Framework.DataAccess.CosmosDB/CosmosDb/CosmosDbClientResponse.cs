using System.Net;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

/// <summary>
/// Result of a Cosmos DB write operation
/// </summary>
public sealed class CosmosDbClientResponse
{
    public CosmosDbClientResponse(HttpStatusCode statusCode, string? eTag = null, double requestCharge = 0)
    {
        StatusCode = statusCode;
        ETag = eTag;
        RequestCharge = requestCharge;
    }

    public HttpStatusCode StatusCode { get; }

    public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;

    /// <summary>
    /// The new ETag of the written document (null for deletes and failed operations)
    /// </summary>
    public string? ETag { get; }

    /// <summary>
    /// Request units consumed by the operation
    /// </summary>
    public double RequestCharge { get; }

    /// <summary>
    /// The If-Match condition failed: the document was changed since its ETag was read.
    /// </summary>
    public bool IsPreconditionFailed => StatusCode == HttpStatusCode.PreconditionFailed;
}
