namespace AppFactory.Framework.Api.Parsing;

public  class InputRequest
{
    public InputRequest()
    {
        Query = new Dictionary<string, string>();
        Path = new Dictionary<string, string>();
    }
    public IDictionary<string, string> Query { get; set; }
    public IDictionary<string, string> Path { get; set; }
    public string Body { get; set; }
}