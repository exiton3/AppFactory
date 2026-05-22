using Amazon;

namespace AppFactory.Framework.DataAccess.DynamoDB.Settings
{
    public interface IAWSSettings
    {
        string GetTableName();
        RegionEndpoint GetAWSRegion();

        string GetEventBusName();
    }
}
