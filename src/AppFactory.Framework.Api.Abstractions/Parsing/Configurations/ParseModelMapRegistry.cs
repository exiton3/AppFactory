namespace AppFactory.Framework.Api.Parsing.Configurations;

public class ParseModelMapRegistry : IParseModelMapRegistry
{
    private readonly Dictionary<string, IParseModelMap> _parseModelMapsDictionary;

    public ParseModelMapRegistry(IEnumerable<IParseModelMap> maps)
    {
        _parseModelMapsDictionary = new Dictionary<string, IParseModelMap>();
        foreach (var map in maps)
        {
            var modelType = map.GetType().BaseType.GetGenericArguments().Single();
            _parseModelMapsDictionary.Add(modelType.Name, map);
        }
    }

    public void AddParseModelMap<T,TParseModel>() where TParseModel: IParseModelMap, new()
    {
        var type = typeof(T).Name;
           _parseModelMapsDictionary.Add(type, new TParseModel());
    }
    public IParseModelMap Get<T>()
    {
        return Get(typeof(T));
    }

    public IParseModelMap Get(Type modeType)
    {
        var modelType = modeType.Name;
        if (!_parseModelMapsDictionary.ContainsKey(modelType))
            throw new KeyNotFoundException(
                $"Key {modelType} was not found in ParseModelMapRegistry. Available keys:{string.Join(",", _parseModelMapsDictionary.Keys)}");

        return _parseModelMapsDictionary[modelType];
    }
}