namespace AppFactory.Framework.DataAccess.Queries.Expressions;

class KeyConditionOptions : ISKConditionOptions
{
    public IQueryExpression QueryExpression { get; private set; }

    public KeyConditionOptions(string keyName, string keyValue)
    {
        QueryExpression = new DefaultQueryExpression();
        KeyName = keyName;
        KeyValue = keyValue;
    }

    public string KeyName { get; set; }
    public string KeyValue { get; set; }
    public string AttributeValue { get; set; }


    public IKeyConditionOptions BeginsWith(string value)
    {
        QueryExpression = new BeginsWithCondition(new KeyName(KeyName), new KeyValue(KeyValue));
        AttributeValue = value;

        return this;
    }

    public IKeyConditionOptions Equals(string value)
    {
        QueryExpression = new EqualsQueryExpression(new KeyName(KeyName), new KeyValue(KeyValue));
        AttributeValue = value;

        return this;
    }

    public IKeyConditionOptions PK(string value)
    {
        QueryExpression = new PKExpression(KeyName);

        return this;
    }


    public IKeyConditionOptions WithName(string name)
    {
        KeyName = name;

        return this;
    }

    public IKeyConditionOptions WithKeyValue(string name)
    {
        KeyValue = name;

        return this;
    }
}