namespace AppFactory.Framework.Domain;

public class EntityIdProvider : IEntityIdProvider
{
    public string GenerateId()
    {
        return Guid.NewGuid().ToString("N");
    }
}