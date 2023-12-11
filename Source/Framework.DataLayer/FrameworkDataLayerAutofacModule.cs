using Autofac;

namespace AppFactory.Framework.DataLayer;

internal class FrameworkDataLayerAutofacModule:Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UnitOfWorkFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}

public static class FrameworkDataLayerModuleExtension
{
    public static void RegisterAppFactoryDataLayerModule(this ContainerBuilder builder)
    {
        builder.RegisterAssemblyModules<FrameworkDataLayerAutofacModule>();
    }
}