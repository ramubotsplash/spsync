using System;
using System.Xml;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;

namespace Rohi.Apps.Core
{
	public static class ExtenstionMethods
	{
		public static string ToGuidString(this string source) {
			if (string.IsNullOrEmpty(source)) {
				return source;
			}
			return source.Replace("{", "").Replace("}", "").ToLower();
		}
		
		public static string GetAttributeValue(this XElement node, string attribute) {
			var elem = node.Attribute(attribute);
			if (elem == null) {
				return null;
			}
			return elem.Value;
		}

        public static int? GetAttributeIntValue(this XElement node, string attribute)
        {
			int intVal;
			if (int.TryParse(GetAttributeValue(node, attribute), out intVal)) {
				return intVal;
			}
			return null;
		}
	}
}

