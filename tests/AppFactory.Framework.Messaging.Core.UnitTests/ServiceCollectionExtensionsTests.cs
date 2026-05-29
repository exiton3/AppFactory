using AppFactory.Framework.Messaging.Core.Abstractions;
using AppFactory.Framework.Messaging.Core.Extensions;
using AppFactory.Framework.TestExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppFactory.Framework.Messaging.Core.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMessageHandler_ShouldRegisterHandlerAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMessageHandler<TestMessageHandler, TestMessage>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IMessageHandler<TestMessage>>();
        
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<TestMessageHandler>();
    }

    [Fact]
    public void AddMessageHandler_ShouldNotRegisterDuplicates()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMessageHandler<TestMessageHandler, TestMessage>();
        services.AddMessageHandler<TestMessageHandler, TestMessage>(); // Duplicate registration

        // Assert
        var registrations = services.Where(s => 
            s.ServiceType == typeof(IMessageHandler<TestMessage>)).ToList();
        
        registrations.Count.ShouldBe(1); // TryAddScoped prevents duplicates
    }

    [Fact]
    public void AddMessageHandlers_ShouldRegisterAllHandlersFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ServiceCollectionExtensionsTests).Assembly;

        // Act
        services.AddMessageHandlers(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        var testHandler = serviceProvider.GetService<IMessageHandler<TestMessage>>();
        testHandler.ShouldNotBeNull();
        testHandler.ShouldBeOfType<TestMessageHandler>();

        var anotherHandler = serviceProvider.GetService<IMessageHandler<AnotherTestMessage>>();
        anotherHandler.ShouldNotBeNull();
        anotherHandler.ShouldBeOfType<AnotherTestMessageHandler>();
    }

    [Fact]
    public void AddMessageHandlers_WithMultipleAssemblies_ShouldRegisterAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(ServiceCollectionExtensionsTests).Assembly;
        var assembly2 = typeof(IMessage).Assembly; // Core assembly (no handlers, but valid)

        // Act
        services.AddMessageHandlers(assembly1, assembly2);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IMessageHandler<TestMessage>>();
        
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void AddMessageHandlers_ShouldRegisterContextHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ServiceCollectionExtensionsTests).Assembly;

        // Act
        services.AddMessageHandlers(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IMessageHandler<TestMessage, TestMessageContext>>();
        
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<TestMessageContextHandler>();
    }

    [Fact]
    public void AddMessageHandler_MultipleCalls_ShouldCreateSeparateInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMessageHandler<TestMessageHandler, TestMessage>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope1 = serviceProvider.CreateScope();
        var handler1 = scope1.ServiceProvider.GetService<IMessageHandler<TestMessage>>();
        
        using var scope2 = serviceProvider.CreateScope();
        var handler2 = scope2.ServiceProvider.GetService<IMessageHandler<TestMessage>>();

        // Assert - Different scope instances
        handler1.ShouldNotBeNull();
        handler2.ShouldNotBeNull();
        ReferenceEquals(handler1, handler2).ShouldBe(false);
    }

    // Test message and handlers
    private class TestMessage : Message
    {
        public string Content { get; set; } = string.Empty;
    }

    private class AnotherTestMessage : Message
    {
        public string Data { get; set; } = string.Empty;
    }

    private class TestMessageHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class AnotherTestMessageHandler : IMessageHandler<AnotherTestMessage>
    {
        public Task HandleAsync(AnotherTestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class TestMessageContext : IMessageContext
    {
        public Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AbandonAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeadLetterAsync(string reason, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class TestMessageContextHandler : IMessageHandler<TestMessage, TestMessageContext>
    {
        public Task HandleAsync(TestMessage message, TestMessageContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
