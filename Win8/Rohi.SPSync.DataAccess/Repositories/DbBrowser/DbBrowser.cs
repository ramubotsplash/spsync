using System;
using System.Collections.Generic;
using Rohi.EBinder.Sync;
using Rohi.EBinder.Entities;
using System.Linq;
using TinyIoC;
using System.IO;
using Rohi.Apps.Core;
using System.Threading.Tasks;

namespace Rohi.EBinder.DataAccess
{
	public class DbBrowser : IDbBrowser
	{
		private ISyncFactory SyncFactory {
			get {
				var container = TinyIoCContainer.Current;
				return container.Resolve<ISyncFactory>();
			}
		}
		
		private IBinderSettings BinderSettings {
			get {
				var container = TinyIoCContainer.Current;
				return container.Resolve<IBinderSettings>();
			}
		}
		
		private IServerDataProvider _dataProvider;
		private IServerDataProvider DataProvider {
			get {
				if (_dataProvider == null) {
                    var siteInfo = SiteInfo; 
					_dataProvider = SyncFactory.GetDataProvider(SiteInfo.Edition,
					                                            SiteInfo.Url, SiteInfo.UserName, SiteInfo.Password,
					                                            DbBrowseMode.CachedAccess,
					                                            BinderSettings.CacheDuration);
				}
				return _dataProvider;
			}
		}
		
		private IList<DbLibrary> _dbLibraries;
		private async Task<IList<DbLibrary>> GetLibrariesInternal()
        {
			if (_dbLibraries == null) {
				_dbLibraries = await DataProvider.GetLibraries();
					
				// sort libraries by type
				_dbLibraries = (from a in _dbLibraries
								orderby TypeUtilities.GetListTypeSortIndex(a.ServerTemplate), a.Title
								select a).ToList();
			}
			return _dbLibraries;
		}

        private void CelarLibrariesInternal()
        {
            _dbLibraries = null;
        }
		
		private DbBrowser (DbSiteInfo siteInfo)
		{
			this.SiteInfo = siteInfo;
		}
		
		#region IDbBrowser implementation

		public DbSiteInfo SiteInfo {
			get;
			private set;
		}
		
		public Task<IList<DbLibrary>> GetLibraries ()
		{
			return GetLibrariesInternal();
		}

		public async Task<DbLibrary> GetLibraryById (string id)
		{
            var libraries = await GetLibrariesInternal();
			var dbLibrary = libraries.Where (a => string.Equals(a.LibraryId, id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault ();
			return dbLibrary;
		}

		public async Task<DbLibrary> GetLibraryByTitle (string title)
		{
            var libraries = await GetLibrariesInternal();
			var dbLibrary = libraries.Where(a => string.Equals(a.Title, title, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			return dbLibrary;
		}

		public Task<DbDocument> GetDocumentById(string libraryId, string documentId) {
			return DataProvider.GetDocumentById(libraryId, documentId);
		}

		public Task<IList<DbDocument>> GetAllDocuments(string libraryId) {
			return DataProvider.GetAllDocuments(libraryId);
		}
		
		public Task<IList<DbDocument>> GetDocumentsByFolder(string libraryId, string folderId) {
			// show current documents
			return DataProvider.GetDocumentsByFolder(libraryId, folderId);
		}

		public async Task<IList<DbDocument>> SearchDocumentsByFolder(string libraryId, string folderId, string filter) {
			// search criteria
			string folderUrl = null;
			if (string.IsNullOrEmpty(folderId)) {
				var library = await GetLibraryById(libraryId);
				if (library != null) {
					folderUrl = library.LibraryUrl;
				}
			} else {
				var document = await GetDocumentById(libraryId, folderId);
				if (document != null) {
					folderUrl = document.DocumentUrl;
				}
			}

			if (string.IsNullOrEmpty(folderUrl)) {
				return new List<DbDocument>();
			}

			// make lower case
			folderUrl = folderUrl.ToLower();
			filter = filter.ToLower();

			// show results from all subfolders based on search criteria
			var documents = await  GetAllDocuments(libraryId);
			return documents.ToList().FindAll(item => {
				return
					(item.DocumentUrl != null && item.DocumentUrl.ToLower().IndexOf(folderUrl) >= 0) &&
					((item.DisplayTitle != null && item.DisplayTitle.ToLower().IndexOf(filter) >= 0) ||
					(item.DisplayDescription != null && item.DisplayDescription.ToLower().IndexOf(filter) >= 0));
			});
		}

		public async Task<IList<string>> GetLibraryColumnDistinctValues(string libraryId, string columnName, bool lookupColumn) {
			var documents = await GetAllDocuments(libraryId);

            var columnValues = new List<string>(documents.Count);
            foreach (var document in documents)
            {
                columnValues.Add(document.GetExtendedColumnValue(columnName, lookupColumn));
            }

			return columnValues
						.Distinct()
						.OrderBy(item => item)
						.ToList();
		}

		public async Task<bool> DownloadDocument(DbDocument document) {
			try {
				var downloaded = await DataProvider.DownloadDocument(document);
                return downloaded;
			} catch (Exception ex) {
				Log.LogException(string.Format("Exception dowloading document: {0}", document.DocumentUrl), ex);
				return false;
			}
		}

		public void SyncStart(bool fullSync) {
			DataProvider.SyncStart(fullSync);
		}

		public void SyncComplete(string slideSourceLibrary, string presentationsLibrary, string qaList) {
            CelarLibrariesInternal();
			SiteInfo.SyncCompleted(slideSourceLibrary, presentationsLibrary, qaList);
			DataProvider.SyncComplete(slideSourceLibrary, presentationsLibrary, qaList);
		}
		
		#endregion
		
		#region Creation Helper
		
		internal static IDbBrowser CreateBrowser(DbSiteInfo siteInfo) {
			return new DbBrowser(siteInfo);
		}
		
		#endregion
	}
}

