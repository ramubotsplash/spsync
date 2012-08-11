using System;
using Rohi.EBinder.Entities;
using TinyIoC;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rohi.EBinder.Sync
{
	public class LocalStorageServerDataProvider : IServerDataProvider
	{
		private string SiteUrl {
			get;
			set;
		}

		private IBinderDb _db;
		private IBinderDb Db {
			get {
				if (_db == null) {
					var container = TinyIoCContainer.Current;
					_db = container.Resolve<IBinderDb>();
				}
				return _db;
			}
		}
		
		public LocalStorageServerDataProvider (string siteUrl)
		{
			this.SiteUrl = siteUrl;
		}

		#region IServerData implementation
		private DbSiteInfo _siteInfo;
		public async Task<DbSiteInfo> GetSiteInfo ()
		{
			if (_siteInfo == null) {
				_siteInfo = await DbSiteInfo.GetByUrl(Db, SiteUrl);
			}
			return _siteInfo;
		}

		public async Task<IList<DbLibrary>> GetLibraries ()
		{
			DbSiteInfo siteInfo = await GetSiteInfo();
			IList<DbLibrary> libraries = await DbLibrary.GetAllLibraries(Db,siteInfo.SiteId); 
			return libraries;
		}

		private async Task<DbLibrary> GetLibraryById(string libraryId)
		{
			IList<DbLibrary> libaries = await GetLibraries ();
			return libaries.ToList().Find(a => a.LibraryId == libraryId);
		}
		
		public Task<IList<DbDocument>> GetAllDocuments (string libraryId)
		{
			return DbDocument.GetAllDocuments(Db, libraryId);
		}

		public Task<IList<DbDocument>> GetDocumentsByFolder (string libraryId, string folderId)
		{
			return DbDocument.GetDocumentsByFolder(Db, libraryId, folderId);
		}

		public Task<DbDocument> GetDocumentById(string libraryId, string documentId)
		{
			return DbDocument.GetDocumentById(Db, libraryId, documentId);
		}

		public Task<bool> DownloadDocument (DbDocument document)
		{
			throw new NotImplementedException ();
		}

		public void SyncStart(bool fullSync) {
		}

		private string GetParentFolderId(IList<DbDocument> allDocuments, DbDocument document) {
			var index = document.DocumentUrl.LastIndexOf('/');
			if (index <= 0) {
				return null;
			}
			var folderUrl = document.DocumentUrl.Substring(0, index);
			var parentFolder = allDocuments
								.ToList()
								.Find(item => item.Type == DbFileType.Folder &&
										      string.Equals(folderUrl, item.DocumentUrl));
			if (parentFolder == null) {
				return null;
			}
			return parentFolder.UniqueId;
		}

		private int GetSubDocumentsCount(IList<DbDocument> allDocuments, DbDocument document) {
			if (document.Type == DbFileType.Folder) {
				var documentUrl = document.DocumentUrl.ToLower();
				return allDocuments
						.ToList()
						.FindAll(item => item.Type != DbFileType.Folder && item.DocumentUrl.ToLower().StartsWith(documentUrl))
						.Count();
			} else {
				return 0;
			}
		}

		public async void SyncComplete(string slideSourceLibrary, string presentationsLibrary, string qaList) {
			DbSiteInfo siteInfo = await GetSiteInfo();
			siteInfo.SyncCompleted(slideSourceLibrary, presentationsLibrary, qaList);
			siteInfo.Update(Db);

			// Update 
			var libraries = await GetLibraries ();
			foreach (var library in libraries) {
				if (!string.Equals(library.Title, slideSourceLibrary, StringComparison.OrdinalIgnoreCase) &&
				    !string.Equals(library.Title, presentationsLibrary, StringComparison.OrdinalIgnoreCase) &&
				    !string.Equals(library.Title, qaList, StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				var allDocuments = await GetAllDocuments(library.LibraryId);
				foreach (var document in allDocuments) {
					var folderId = GetParentFolderId(allDocuments, document);
					var itemCount = GetSubDocumentsCount(allDocuments, document);
					document.SetFolderInfo(folderId, itemCount);
					document.InsertOrUpdate(Db);
				}
			}
		}

		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion
	}
}

