using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.EventBus.Aws.EventBus;
using AppFactory.Framework.EventBus.EventBus;
using AppFactory.Framework.EventBus.EventBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.EventBus.Aws
{
    public class ServiceInfrastructureDependencyModule: IDependencyRegistrationModule
    {
        public void RegisterServices(IServiceCollection service)
        {
            service.AddSingleton<IEventBus, EventBridgeServiceBus>();
            service.AddSingleton<IEventBusSubscriptionsManager,InMemoryEventBusSubscriptionsManager>();
            service.AddSingleton<IAmazonEventBridgeFactory, AmazonEventBridgeFactory>();
        }
    }

    public static class EventBusDependencyRegistration
    {
        public static void RegisterEventBus(this IServiceCollection service)
        {
            var module = new ServiceInfrastructureDependencyModule();

            module.RegisterServices(service);
        }
    }
}