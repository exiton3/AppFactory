using Amazon;

namespace AppFactory.Framework.DataAccess.Configuration
{
    public interface IAWSSettings
    {
        string GetTableName();
        RegionEndpoint GetAWSRegion();

        string GetEventBusName();
    }
}
