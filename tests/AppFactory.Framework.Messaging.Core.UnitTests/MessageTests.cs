using AppFactory.Framework.Messaging.Core.Abstractions;
using AppFactory.Framework.TestExtensions;
using Xunit;

namespace AppFactory.Framework.Messaging.Core.UnitTests;

public class MessageTests
{
    [Fact]
    public void Message_Constructor_ShouldInitializeWithEmptyProperties()
    {
        // Act
        var message = new TestMessage { Body = "test-body" };

        // Assert
        message.MessageId.ShouldNotBeNull();
        message.Body.ShouldBe("test-body");
        message.Properties.ShouldNotBeNull();
        message.Properties.Count.ShouldBe(0);
        message.EnqueuedTimeUtc.ShouldBe(default);
        message.DeliveryCount.ShouldBe(0);
    }

    [Fact]
    public void AddCorrelationId_ShouldAddCorrelationIdToProperties()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };
        var correlationId = Guid.NewGuid().ToString();

        // Act
        message.AddCorrelationId(correlationId);

        // Assert
        message.Properties.ContainsKey("CorrelationId").ShouldBe(true);
        message.Properties["CorrelationId"].ShouldBe(correlationId);
    }

    [Fact]
    public void AddCausationId_ShouldAddCausationIdToProperties()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };
        var causationId = Guid.NewGuid().ToString();

        // Act
        message.AddCausationId(causationId);

        // Assert
        message.Properties.ContainsKey("CausationId").ShouldBe(true);
        message.Properties["CausationId"].ShouldBe(causationId);
    }

    [Fact]
    public void AddUserId_ShouldAddUserIdToProperties()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };
        var userId = "user-123";

        // Act
        message.AddUserId(userId);

        // Assert
        message.Properties.ContainsKey("UserId").ShouldBe(true);
        message.Properties["UserId"].ShouldBe(userId);
    }

    [Fact]
    public void Message_ShouldSupportChainedPropertyAdditions()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };
        var correlationId = Guid.NewGuid().ToString();
        var causationId = Guid.NewGuid().ToString();
        var userId = "user-456";

        // Act
        message.AddCorrelationId(correlationId)
               .AddCausationId(causationId)
               .AddUserId(userId);

        // Assert
        message.Properties.Count.ShouldBe(3);
        message.Properties["CorrelationId"].ShouldBe(correlationId);
        message.Properties["CausationId"].ShouldBe(causationId);
        message.Properties["UserId"].ShouldBe(userId);
    }

    [Fact]
    public void Message_ShouldAllowCustomProperties()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };

        // Act
        message.Properties["CustomKey"] = "CustomValue";
        message.Properties["Priority"] = "High";

        // Assert
        message.Properties.Count.ShouldBe(2);
        message.Properties["CustomKey"].ShouldBe("CustomValue");
        message.Properties["Priority"].ShouldBe("High");
    }

    [Fact]
    public void Message_WithEnqueuedTime_ShouldStoreCorrectly()
    {
        // Arrange
        var enqueuedTime = DateTime.UtcNow;

        // Act
        var message = new TestMessage 
        { 
            Body = "test",
            EnqueuedTimeUtc = enqueuedTime 
        };

        // Assert
        message.EnqueuedTimeUtc.ShouldBe(enqueuedTime);
    }

    [Fact]
    public void Message_WithDeliveryCount_ShouldStoreCorrectly()
    {
        // Arrange & Act
        var message = new TestMessage 
        { 
            Body = "test",
            DeliveryCount = 3 
        };

        // Assert
        message.DeliveryCount.ShouldBe(3);
    }

    [Fact]
    public void AddCorrelationId_WithNullValue_ShouldNotAddProperty()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };

        // Act
        message.AddCorrelationId(null);

        // Assert
        message.Properties.ContainsKey("CorrelationId").ShouldBe(false);
    }

    [Fact]
    public void AddCausationId_WithNullValue_ShouldNotAddProperty()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };

        // Act
        message.AddCausationId(null);

        // Assert
        message.Properties.ContainsKey("CausationId").ShouldBe(false);
    }

    [Fact]
    public void AddUserId_WithNullValue_ShouldNotAddProperty()
    {
        // Arrange
        var message = new TestMessage { Body = "test" };

        // Act
        message.AddUserId(null);

        // Assert
        message.Properties.ContainsKey("UserId").ShouldBe(false);
    }

    private class TestMessage : Message
    {
        public string Body { get; set; } = string.Empty;
    }
}
