using System;
using System.Xml;

namespace eSignASPLibrary
{
    internal static class eSignExtensionMethods
    {
        internal static bool CaseLessEquals(this string original,string toCheck)
        {
            if (string.IsNullOrWhiteSpace(original)) return false;
            if (string.IsNullOrWhiteSpace(toCheck)) return false;
            return original.ToLower() == toCheck.ToLower();
        }
        internal static string GetXMLAttributeValue(this XmlNode xmlNode, string AttributeName)
        {
            try
            {
                string AttributeValue = xmlNode == null ? "" : xmlNode.Attributes[AttributeName] == null ? "" : xmlNode.Attributes[AttributeName].Value;
                return AttributeValue;
            }
            catch
            {
                return "";
            }
        }
    }
}
