using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DependencyInjection;

public class DependencyContainer
{
    private readonly List<IDependencyRegistrationModule> _modules = new();
    public void RegisterModule<TModule>() where TModule : IDependencyRegistrationModule, new()
    {
        _modules.Add(new TModule());
    }

    public void Build(IServiceCollection services)
    {
        foreach (var module in _modules)
        {
            module.RegisterServices(services);
        }
    }
}