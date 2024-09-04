using Amazon;

namespace AppFactory.Framework.DataAccess.Settings
{
    public interface IAWSSettings
    {
        string GetTableName();
        RegionEndpoint GetAWSRegion();

        string GetEventBusName();
    }
}
