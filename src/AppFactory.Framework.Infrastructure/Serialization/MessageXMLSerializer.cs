using System.Xml;
using System.Xml.Serialization;

namespace AppFactory.Framework.Infrastructure.Serialization;

public class MessageXMLSerializer : IMessageXmlSerializer
{
    public string Serialize(object objectToSerialize)
    {
        var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var serializer = new XmlSerializer(objectToSerialize.GetType());
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true
        };

        using var stream = new StringWriter();
        using var writer = XmlWriter.Create(stream, settings);
        serializer.Serialize(writer, objectToSerialize, emptyNamespaces);

        return stream.ToString();
    }

    public T Deserialize<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var streamReader = new StringReader(xml);

        return (T)serializer.Deserialize(streamReader);
    }

    public T DeserializeInnerSoapObject<T>(string soapResponse, string tagName)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(soapResponse);

        var soapBody = xmlDocument.GetElementsByTagName(tagName)[0];
        var innerObject = soapBody.InnerXml;

        return Deserialize<T>(innerObject);
    }
}