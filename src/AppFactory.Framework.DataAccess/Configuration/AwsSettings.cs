using Amazon;
using AppFactory.Framework.Shared.Config;

namespace AppFactory.Framework.DataAccess.Configuration;

public class AwsSettings : IAWSSettings
{
    private readonly IConfigSettings _config;

    public AwsSettings(IConfigSettings config)
    {
        _config = config;
    }
    public string GetTableName()
    {
        return _config.GetValue("db_table_name");
    }

    public RegionEndpoint GetAWSRegion()
    {
        var awsRegion = _config.GetValue("aws_region");

        return RegionEndpoint.GetBySystemName(awsRegion);
    }

    public string GetEventBusName()
    {
       return _config.GetValue("eventbus_name");
    }
}