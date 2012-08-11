using System;
using System.Collections.Generic;
using System.Xml;
using Rohi.Apps.Core;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using SQLite;
using System.Threading.Tasks;

namespace Rohi.EBinder.Entities
{
	public class DbDocument
	{
		#region Members

		[PrimaryKey]
		public	string		UniqueId		{ get; private set; }
		[Indexed]
		public	string		LibraryId		{ get; private set; }
		[Indexed]
		public	string		FolderId		{ get; private set; }
		[Indexed]
		public	string		FolderUrl		{ get; private set; }
		public	int			DocumentId		{ get; private set; }
		public	DbFileType	Type			{ get; private set; }
		public	string		ThumbnailUrl	{ get; private set; }
		public	string		ThumbnailPath	{ get; private set; }
		public	string		DocumentUrl		{ get; private set; }
		public	string		ViewUrl			{ get; private set; }
		public	string		Title			{ get; private set; }
		public	string		Description		{ get; private set; }
		public	string		Etag			{ get; private set; }
		public	string		ContentType		{ get; private set; }
		public	int			ItemCount		{ get; private set; }
		public	int?		Size			{ get; private set; }
		public	DateTime	Created			{ get; private set; }
		public	string		CreatedBy		{ get; private set; }
		public	DateTime	Modified		{ get; private set; }
		public	string		ModifiedBy		{ get; private set; }
		
		public	string		LocalPath		{ get; private set; } 
		public 	DateTime?	LastSyncDate	{ get; set; }

		#endregion
		
		#region Extended members
		
		public bool IsCached {
			get {
				return LastSyncDate.HasValue;
			}
		}
		
		public string FileName {
			get {
				if (string.IsNullOrEmpty(DocumentUrl))
					return null;
				var index = DocumentUrl.TrimEnd('/').LastIndexOf('/');
				return index < 0 ? DocumentUrl : DocumentUrl.Substring(index + 1);
			}
		}

		public string DisplayTitle {
			get {
				if (Type == DbFileType.Folder) {
					return FileName;
				} else {
					return System.IO.Path.GetFileNameWithoutExtension(FileName);
				}
			}
		}
		
		public string DisplayDescription {
			get {
				if (Type == DbFileType.Folder) {
					return string.Empty;
				} else {
					return Description;
				}
			}
		}
		
		public string ExtendedColumnsXml  {
			get;
			private set;
		}

		#endregion

		#region Constructor
		
		public DbDocument ()
		{
		}

		#endregion

		#region Helper Functions

		public void DownloadCompleted(string localPath, string thumbnailPath) {
			this.ThumbnailPath = thumbnailPath;
			this.LocalPath = localPath;
			this.LastSyncDate = DateTime.Now;

            if (string.IsNullOrEmpty(Title))
            {
                this.Title = System.IO.Path.GetFileNameWithoutExtension(localPath);
            }
        }

		private IDictionary<string, string> _extendedProperties;
		public IDictionary<string, string> ExtendedProperties {
			get {
				if (_extendedProperties == null) {
					_extendedProperties = new Dictionary<string, string>();

					if (!string.IsNullOrEmpty(this.ExtendedColumnsXml)) {
						var extendedNode = XElement.Parse(this.ExtendedColumnsXml);
						foreach (XAttribute attr in extendedNode.Attributes()) {
							_extendedProperties.Add(attr.Name.LocalName, attr.Value);
						}
					}
				}
				return _extendedProperties;
			}
		}

		public string GetExtendedColumnValue(string columnName, bool lookupColumn) {
			string columnValue;
			if (ExtendedProperties.TryGetValue(columnName, out columnValue)) {
				if (lookupColumn && !string.IsNullOrEmpty(columnValue)) {
					var index = columnValue.IndexOf(";#");
					if (index >= 0) {
						if (columnValue.Length > index + 2) {
							return columnValue.Substring(index + 2);
						} else {
							return null;
						}
					}
				}
				return columnValue;
			}
			return null;
		}

		public static DbFileType GetDbFileTypeFromContentTypeAndFileType (string contentTypeId, string fileLeafRef)
		{
			if (Constants.SharePointContentType.Compare(contentTypeId, Constants.SharePointContentType.Folder)) {
				return DbFileType.Folder;
			} if (Constants.SharePointContentType.Compare(contentTypeId, Constants.SharePointContentType.Item)) {
				return DbFileType.Item;
			} else if (fileLeafRef.ToLower().EndsWith(".ppt") || fileLeafRef.ToLower().EndsWith(".pptx")) {
				return DbFileType.Ppt;
			} else {
				return DbFileType.Doc;
			}
		}

		public static string GetViewUrlFromLibrary (DbLibrary library, int par1)
		{
			return "";
		}
		
		public static DbDocument CreateFromSharePointXmlNode(XElement docNode, string siteUrl, DbLibrary library) {
			int? id = docNode.GetAttributeIntValue("ows_ID");
			if (!id.HasValue || id <= 0 || string.IsNullOrEmpty(docNode.GetAttributeValue("ows_UniqueId"))) {
				return null;
			}
			var dbDocument = new DbDocument() {
				UniqueId		= docNode.GetAttributeValue("ows_UniqueId").ToGuidString(),
				LibraryId 		= library.LibraryId.ToGuidString(),
				FolderUrl		= PathUtil.GetFolderPath(docNode.GetAttributeValue("ows_ServerUrl")),
				DocumentId		= id.Value,
				Type			= GetDbFileTypeFromContentTypeAndFileType(docNode.GetAttributeValue("ows_ContentTypeId"), docNode.GetAttributeValue("ows_FileLeafRef")),
				ThumbnailUrl	= PathUtil.GetRelativeUrl(docNode.GetAttributeValue("ows_EncodedAbsThumbnailUrl")),
				DocumentUrl		= PathUtil.EscapeUrl(docNode.GetAttributeValue("ows_ServerUrl")),
				ViewUrl			= GetViewUrlFromLibrary(library, Convert.ToInt32(docNode.GetAttributeValue("ows_ID"))),
				Title			= docNode.GetAttributeValue("ows_Title"),
				Description		= docNode.GetAttributeValue("ows_SlideDescription"),
				Etag			= docNode.GetAttributeValue("ows_Etag"),
				ContentType		= docNode.GetAttributeValue("ows_ContentType"),
				ItemCount		= docNode.GetAttributeIntValue("ows_ItemChildCount") ?? 0,
				Size			= docNode.GetAttributeIntValue("ows_FileSizeDisplay"),
				Created			= SharePointUtil.ToDateTime(docNode.GetAttributeValue("ows_Created")),
				CreatedBy		= docNode.GetAttributeValue("ows_Created_x0020_By"),
				Modified 		= SharePointUtil.ToDateTime(docNode.GetAttributeValue("ows_Modified")),
				ModifiedBy		= docNode.GetAttributeValue("ows_Modified_x0020_By")
			};

			// store colums for metadata
			StringBuilder sb = new StringBuilder();
			sb.Append("<item");
			foreach (XAttribute attr in docNode.Attributes()) {
				if (!Constants.SharePointWebService.ExcludeMetadataColumns.Contains(attr.Name.LocalName)) {
					sb.AppendFormat(" {0}=\"{1}\"", attr.Name, SystemUtil.ConverToXmlAttribute(attr.Value));
				}
			}
			sb.Append("/>");
			dbDocument.ExtendedColumnsXml = sb.ToString();
			
			Log.LogMessage("New Document: {0}: {1}: {2}", dbDocument.Title, dbDocument.FileName, dbDocument.LibraryId);
			return dbDocument;
		}
		
		public static IList<DbDocument> CreateListFromSharePointXmlNode(XElement rootNode, string siteUrl, DbLibrary library) {
			IList<DbDocument> dbDocuments = new List<DbDocument>();

            // fix for MOSS 2007
            XNamespace ns = "urn:schemas-microsoft-com:rowset";
            var firstElement = rootNode.Element(ns + "data");
            if (firstElement != null)
            {
                rootNode = firstElement;
            }

			foreach (var node in rootNode.Elements()) {
				var doc = CreateFromSharePointXmlNode((XElement)node, siteUrl, library);
				if (doc != null) {
					dbDocuments.Add(doc);
				}
			}
            return dbDocuments;
		}

		public void SetFolderInfo(string folderId, int count) {
			this.FolderId = folderId;
			this.ItemCount = count;
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

		#endregion

		#region Get functions

        public async static Task<DbDocument> GetDocumentById(IBinderDb db, string libraryId, string uniqueId)
        {
            var items = await db.Query<DbDocument>("SELECT * FROM DbDocument WHERE LibraryId = ? AND UniqueId = ?", libraryId, uniqueId);
            return items.FirstOrDefault();
        }

        public async static Task<IList<DbDocument>> GetAllDocuments(IBinderDb db, string libraryId)
        {
            return await db.Query<DbDocument>("SELECT * FROM DbDocument WHERE LibraryId = ?", libraryId);
        }

        public async static Task<IList<DbDocument>> GetDocumentsByFolder(IBinderDb db, string libraryId, string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                return await db.Query<DbDocument>("SELECT * FROM DbDocument WHERE LibraryId = ? AND FolderId IS NULL", libraryId);
            }
            else
            {
                return await db.Query<DbDocument>("SELECT * FROM DbDocument WHERE LibraryId = ? AND FolderId = ?", libraryId, folderId);
            }
        }
		
		#endregion
	}
}

