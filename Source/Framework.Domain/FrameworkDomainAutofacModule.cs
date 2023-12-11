using AppFactory.Framework.Domain.Commands;
using Autofac;

namespace AppFactory.Framework.Domain;

internal class FrameworkDomainAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>().InstancePerLifetimeScope();
    }
}

public static class FrameworkDomainModuleExtension
{
    public static void RegisterAppFactoryDomainModule(this ContainerBuilder builder)
    {
        builder.RegisterAssemblyModules(typeof(FrameworkDomainAutofacModule).Assembly);
    }
}