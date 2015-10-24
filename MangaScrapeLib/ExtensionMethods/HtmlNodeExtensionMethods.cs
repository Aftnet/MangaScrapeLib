using HtmlAgilityPack;

namespace MangaScrapeLibPortable.ExtensionMethods
{
    public static class HtmlNodeExtensionMethods
    {
        public static bool HasNameAttributeValue(this HtmlNode Node, string NodeName, string AttributeName, string AttributeValue)
        {
            if(Node.Name != NodeName)
            {
                return false;
            }

            return HasAttributeValue(Node, AttributeName, AttributeValue);
        }

        public static bool HasAttributeValue(this HtmlNode Node, string AttributeName, string AttributeValue)
        {
            if(!Node.Attributes.Contains(AttributeName))
            {
                return false;
            }

            return Node.Attributes[AttributeName].Value == AttributeValue;
        }
    }
}
