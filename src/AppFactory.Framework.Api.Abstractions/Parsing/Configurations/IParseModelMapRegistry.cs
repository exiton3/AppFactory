namespace AppFactory.Framework.Api.Parsing.Configurations
{
    public interface IParseModelMapRegistry
    {
        IParseModelMap Get<T>();

        IParseModelMap Get(Type modeType);
        void AddParseModelMap<T,TParseModel>() where TParseModel: IParseModelMap, new();
    }
}