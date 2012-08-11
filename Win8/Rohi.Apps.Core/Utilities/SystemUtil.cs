using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Rohi.Apps.Core
{
	public static class SystemUtil
	{
		/// <summary>
		/// Remove HTML from string with Regex.
		/// </summary>
		public static string StripTagsRegex(string source)
		{
			if (source == null)
				return null;
			return Regex.Replace(source, "<.*?>", string.Empty);
		}

		public static string ConverToXmlAttribute(string source) {
			if (source == null)
				return null;
			return StripTagsRegex(source)
						.Replace("\"", "&quot;");
		}

		public static string ReplaceHtml(string source) {
			if (source == null)
				return null;
			return StripTagsRegex(source)
				.Replace("&nbsp;", " ");
		}
	}
}

