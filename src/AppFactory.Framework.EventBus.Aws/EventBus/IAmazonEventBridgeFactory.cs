using Amazon.EventBridge;

namespace AppFactory.Framework.EventBus.Aws.EventBus;

public interface IAmazonEventBridgeFactory
{
    IAmazonEventBridge Create();
}

class AmazonEventBridgeFactory : IAmazonEventBridgeFactory
{
    public IAmazonEventBridge Create()
    {
        return new AmazonEventBridgeClient();
    }
}