using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DependencyInjection;

public interface IDependencyRegistrationModule
{
    void RegisterServices(IServiceCollection service);
}