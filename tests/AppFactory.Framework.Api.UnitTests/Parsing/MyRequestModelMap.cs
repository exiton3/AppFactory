using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Api.Parsing.Converters;

namespace AppFactory.Framework.Api.UnitTests.Parsing;

class MyRequestModelMap : ParseModelMap<MyRequest>
{
    public MyRequestModelMap()
    {
        Map(x => x.Name, "name").FromPath();
        Map(x => x.Price, "price").FromQuery().UseConverter<StringToDecimalConverter>();
    }
}