using System;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using Rohi.Apps.Core;
using System.Xml.Linq;
using SQLite;
using System.Threading.Tasks;

namespace Rohi.EBinder.Entities
{
	public class DbLibrary
	{
		public DbLibrary ()
		{
		}
		
		[PrimaryKey]
		public	string		LibraryId		{ get; private set; }
		public	string		SiteId			{ get; private set; }
		public	string		LibraryUrl		{ get; private set; }
		public	string		ViewUrl			{ get; private set; }
		public	string		Title			{ get; private set; }
		public	string		Description		{ get; private set; }
		public	bool		Hidden			{ get; private set; }
		public	int			BaseType		{ get; private set; }
		public	int			ServerTemplate	{ get; private set; }
		public	int			ItemCount		{ get; private set; }
		public	DateTime	Created			{ get; private set; }
		public	DateTime	Modified		{ get; private set; }
		public 	DateTime?	LastMetadataSyncDate	{ get; private set; }

		public	bool		CachedLibraries	{ get; set; }
		
		public bool IsMetadataSynched {
			get {
				return LastMetadataSyncDate.HasValue;
			}
		}
		
		public void MetadataSyncCompleted() {
			this.LastMetadataSyncDate = DateTime.Now;
		}
		
		public	bool AutoSync {
			get
			{
				if (!this.Hidden && 
				    (ListType == DbLibraryType.SlideLibrary ||
				 	(ListType == DbLibraryType.DocumentLibrary && Title.ToLower().IndexOf("presentation") >= 0))) {
					return true;
				}
				return false;
			}
		}
		
		private DbLibraryType? _dbLibraryType;
		public DbLibraryType ListType {
			get {
				if (_dbLibraryType == null) {
					if (BaseType == 1) {
						if (ServerTemplate == (int)SPListTemplateType.SlideLibrary) {
							_dbLibraryType = DbLibraryType.SlideLibrary;
						} else if (ServerTemplate == (int)SPListTemplateType.PictureLibrary) {
							_dbLibraryType = DbLibraryType.PictureLibrary;
						} else if (ServerTemplate == (int)SPListTemplateType.DocumentLibrary && string.Compare(Title, "Presentations", StringComparison.OrdinalIgnoreCase) >= 0) {
							_dbLibraryType = DbLibraryType.PresentationLibrary;
						} else {
							_dbLibraryType = DbLibraryType.DocumentLibrary;
						}
					} else {
						switch (ServerTemplate) {
						case (int)SPListTemplateType.Announcements:
							_dbLibraryType = DbLibraryType.Announcements;
							break;
						case (int)SPListTemplateType.Contacts:
							_dbLibraryType = DbLibraryType.Contacts;
							break;
						case (int)SPListTemplateType.DiscussionBoard:
							_dbLibraryType = DbLibraryType.DiscussionBoard;
							break;
						case (int)SPListTemplateType.Links:
							_dbLibraryType = DbLibraryType.Links;
							break;
						case (int)SPListTemplateType.Events:
							_dbLibraryType = DbLibraryType.Calendar;
							break;
						default:
							_dbLibraryType = DbLibraryType.CustomList;
							break;
						}
					}
				}
				return _dbLibraryType.Value;
			}
		}
		
		#region Creation Helper methods
		
		public static DbLibrary CreateFromSharePointXmlNode(XElement listNode, string siteUrl) {
			var dbLibrary = new DbLibrary() {
				SiteId = listNode.GetAttributeValue("WebId").ToGuidString(),
				LibraryId = listNode.GetAttributeValue("ID").ToGuidString(),
				LibraryUrl = PathUtil.GetRelativeUrl(SharePointUtil.GetListUrlFromViewUrl(listNode.GetAttributeValue("DefaultViewUrl"))),
				ViewUrl = PathUtil.GetRelativeUrl(listNode.GetAttributeValue("DefaultViewUrl")),
				
				Title = listNode.GetAttributeValue("Title"),
				Description = listNode.GetAttributeValue("Description"),
				BaseType = Convert.ToInt32(listNode.GetAttributeValue("BaseType")),
				ServerTemplate = Convert.ToInt32(listNode.GetAttributeValue("ServerTemplate")),
				Hidden = Convert.ToBoolean(listNode.GetAttributeValue("Hidden")),
				
				ItemCount = Convert.ToInt32(listNode.GetAttributeValue("ItemCount")),
                Created = SharePointUtil.ToDateTime(listNode.GetAttributeValue("Created")),
                Modified = SharePointUtil.ToDateTime(listNode.GetAttributeValue("Modified"))
			};
			Log.LogMessage("New Library: {0}: {1}: {2}", dbLibrary.LibraryId, dbLibrary.Title, dbLibrary.LibraryUrl);
			return dbLibrary;
		}
		
		public static IList<DbLibrary> CreateListFromSharePointXmlNode(XElement rootNode, string siteUrl) {
			IList<DbLibrary> dbLibraries = new List<DbLibrary>();
			foreach (XNode node in rootNode.Elements()) {
				var lib = CreateFromSharePointXmlNode((XElement)node, siteUrl);
				dbLibraries.Add(lib);
			}
            return dbLibraries;
		}
		
		#endregion

		#region Crud functions

		public void Insert(IBinderDb db) {
			db.Insert(this);
		}
		
		public void InsertOrUpdate(IBinderDb db) {
			db.InsertOrUpdate(this);
		}
		
		public void Update(IBinderDb db) {
			db.Update(this);
		}
		
		public void Delete(IBinderDb db) {
		}

		#endregion

		#region Get functions

        public async static Task<IList<DbLibrary>> GetAllLibraries(IBinderDb db, string siteId)
        {
            return await db.Query<DbLibrary>("SELECT * FROM DbLibrary WHERE SiteId = ?", siteId);
        }

		#endregion
	}
}

