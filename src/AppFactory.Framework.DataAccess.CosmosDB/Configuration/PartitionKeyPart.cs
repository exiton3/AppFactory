namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public class PartitionKeyPart<TModel> where TModel : class
{
    public Func<TModel, object?> Getter { get; set; }

    public Action<TModel, object> Setter { get; set; }
  
    public string DestinationPropertyName { get; set; }

    public IPartitionKeyValueResolver? Resolver { get; set; }

    public bool IsResolverSet => Resolver != null;

    public string SourcePropertyName { get; set; }
    
  
    public string Prefix { get; set; }
    public bool IsPrefixSet => !string.IsNullOrEmpty(Prefix);
    public bool IsPropertyNameSet => !string.IsNullOrEmpty(DestinationPropertyName);
}
