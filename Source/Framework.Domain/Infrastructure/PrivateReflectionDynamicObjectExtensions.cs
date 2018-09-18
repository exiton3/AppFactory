namespace Framework.Domain.Infrastructure
{
    internal static class PrivateReflectionDynamicObjectExtensions
    {
        public static dynamic AsDynamic(this AggregateRoot o)
        {
            return new PrivateReflectionDynamicObject { RealObject = o };
        }
    }
}