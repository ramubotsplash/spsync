using System;
using System.Collections.Generic;

namespace Rohi.EBinder.Entities
{
	public static class Constants
	{
		public static class App
		{
			public static string CompanyName = "© Rohi Mac";
			public static string AppName = "SharePoint Sync";
			public static string DbName = "RohiTouchBrowserDb";

			public const string ContactName = "Rp36";
			public const string ContactEmail = "ramu@rohilabs.com";

			public static string ListAllSelection = "All";
			public static string ListNoneSelection = "(none)";
		}

		public static class SharePointWebService {
			public static string QueryItemsRowLimit = "5000";

            public readonly static List<string> SharePointInternalLibraries = new List<string>() {
			    "Master Page Gallery", "Reporting Metadata", "Site Template Gallery", "User Information List",
                "Web Part Gallery", "Form Templates","Site Assets", "Site Collection Documents", "Site Pages",
			    "Content and Structure Reports", "Reusable Content", "Tasks", "Workflow Tasks",
                "Reporting Templates", "Converted Forms", "List Template Gallery"
		    };

            public readonly static IList<string> ExcludeMetadataColumns = new List<string> {
				"ows_ContentTypeId", "ows_FileLeafRef", "ows_Modified_x0020_By", "ows_Created_x0020_By", "ows_File_x0020_Type",
				"ows_Presentation", "ows_SlideDescription", "ows_Title", "ows_EncodedAbsThumbnailUrl", "ows_SelectedFlag", "ows_Thumbnail",
				"ows_LargeThumbnailows_ID", "ows_ContentType", "ows_Created", "ows_Author", "ows_Modified", "ows_Editor", "ows__ModerationStatus",
				"ows_FileRef", "ows_FileDirRef", "ows_Last_x0020_Modified", "ows_Created_x0020_Date", "ows_File_x0020_Size", "ows_FSObjType",
				"ows_SortBehavior", "ows_PermMask", "ows_CheckedOutUserId", "ows_IsCheckedoutToLocal", "ows_UniqueId", "ows_ProgId", "ows_VirusStatus",
				"ows_CheckedOutTitle", "ows__CheckinComment", "ows__EditMenuTableStart", "ows__EditMenuTableStart2", "ows__EditMenuTableEnd",
				"ows_LinkFilenameNoMenu", "ows_LinkFilename", "ows_LinkFilename2", "ows_DocIcon", "ows_ServerUrl", "ows_EncodedAbsUrl", "ows_BaseName",
				"ows_FileSizeDisplay", "ows_MetaInfo", "ows__Level", "ows__IsCurrentVersion", "ows_ItemChildCount", "ows_FolderChildCount",
				"ows_SelectTitle", "ows_SelectFilename", "ows_owshiddenversion", "ows__UIVersion", "ows__UIVersionString", "ows_Order", "ows_GUID",
				"ows_WorkflowVersion", "ows_ParentVersionString", "ows_ParentLeafName", "Etag"
			};
			
		}
		
		public static class SharePointContentType
		{
			public const string Folder = "0x0120";
			public const string Document = "0x0101";
			public const string Item = "0x01";
			public const string ContentTypeSeparator = "00";
			
			public static bool Compare(string itemtContentType, string ct) {
				return itemtContentType.StartsWith(ct + ContentTypeSeparator);
			}
		}
	}
}

