namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

internal sealed class DiscriminatorConfigBuilder<TBase> : IPolymorphicConfigOptions<TBase>, IDiscriminatorConfigOptions<TBase>
    where TBase : class
{
    private readonly DiscriminatorConfig _config;

    public DiscriminatorConfigBuilder(DiscriminatorConfig config)
    {
        _config = config;
    }

    public IDiscriminatorConfigOptions<TBase> DiscriminateOn(string propertyName)
    {
        _config.SetPropertyName(propertyName);
        return this;
    }

    public IDiscriminatorConfigOptions<TBase> WithValue<TDerived>(string value) where TDerived : class, TBase
    {
        _config.AddValue(typeof(TDerived), value);
        return this;
    }

    public IDiscriminatorConfigOptions<TBase> DefaultTo<TDerived>() where TDerived : class, TBase
    {
        _config.SetDefaultType(typeof(TDerived));
        return this;
    }
}
