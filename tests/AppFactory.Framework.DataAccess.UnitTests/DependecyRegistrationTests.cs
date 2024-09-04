using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.TestExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppFactory.Framework.DataAccess.UnitTests;

public class DependencyRegistrationTests
{
    [Fact]
    public void RegisterModelConfig()
    {
        var serviceCollection = new ServiceCollection();


        serviceCollection.RegisterModelConfig<TestModelConfig, TestModel>();

        var service = serviceCollection.BuildServiceProvider();

        var model = service.GetService<IModelConfig<TestModel>>();

        model.ShouldBeInstanceOf<TestModelConfig>();
    }
}