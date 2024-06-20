namespace AppFactory.Framework.DataAccess.Queries.Expressions;

public interface IKeyConditionOptions
{
    IKeyConditionOptions WithName(string name);
    IKeyConditionOptions WithKeyValue(string name);
    
}

public interface IPKConditionOptions : IKeyConditionOptions
{
    IKeyConditionOptions PK(string value);
}


public interface ISKConditionOptions : IKeyConditionOptions
{
    IKeyConditionOptions BeginsWith(string value);
    IKeyConditionOptions Equals(string value);
    IKeyConditionOptions WithName(string name);
    IKeyConditionOptions WithKeyValue(string name);
}