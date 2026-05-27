namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public class PartitionKeyPart<TModel> where TModel : class
{
    public Func<TModel, object?> Selector { get; set; }

    public Action<TModel, object> Setter { get; set; }
  
    public string PropertyName { get; set; }
    
  
    public string Prefix { get; set; }
    public bool IsPrefixSet => !string.IsNullOrEmpty(Prefix);
}
