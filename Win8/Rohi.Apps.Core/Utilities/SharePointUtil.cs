using System;
using System.Globalization;

namespace Rohi.Apps.Core
{
	public static class SharePointUtil
	{
		public static DateTime ToDateTime (string str)
		{
            if (str.Length == "yyyyMMdd HH:mm:ss".Length)
            {
                return DateTime.ParseExact(str, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (str.Length == "yyyy-MM-dd HH:mm:ss".Length)
            {
                return DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTime.Parse(str);
            }
		}

		public static string GetListUrlFromViewUrl(string viewUrl) {
			int index = viewUrl.ToLower().IndexOf("/forms/");
			if (index < 0) {
				return viewUrl;
			}
			return viewUrl.Substring(0, index);
		}

	}
}

