using System.Xml.Linq;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class XElementExtension
    {
        public static string GetAttribute(this XElement node, string attrName)
        {
            if (node == null) return string.Empty;
            XAttribute attribute = node.Attribute(attrName);
            return attribute == null ? string.Empty : attribute.Value;
        }
    }
}