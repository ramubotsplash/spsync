using System;
using System.Collections.Generic;
using Rohi.EBinder.Entities;
using TinyIoC;
using System.Linq;
using System.Threading.Tasks;

namespace Rohi.EBinder.Sync
{
	public class CachedServerDataProvider : IServerDataProvider
	{
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

		private IBinderSettings BinderSettings {
			get {
				var container = TinyIoCContainer.Current;
				return container.Resolve<IBinderSettings>();
			}
		}
		
		public IServerDataProvider LocalStorageDataProvider  {
			get;
			set;
		}
		
		public IServerDataProvider LiveDataProvider  {
			get;
			set;
		}
		
		internal CachedServerDataProvider() {
		}
		
		internal CachedServerDataProvider (IServerDataProvider localStorageDataProvider, IServerDataProvider liveStorageProvider)
		{
			this.LocalStorageDataProvider = localStorageDataProvider;
			this.LiveDataProvider = liveStorageProvider;
		}

		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion

		#region IServerDataProvider implementation

		private DbSiteInfo _siteInfo;
		public async Task<DbSiteInfo> GetSiteInfo ()
		{
			if (_siteInfo == null) {
				_siteInfo = await LocalStorageDataProvider.GetSiteInfo();
			}
            return _siteInfo;
		}

		private IList<DbLibrary> _libraries;
		public async Task<IList<DbLibrary>> GetLibraries ()
		{
			if (_libraries == null) {
				DbSiteInfo siteInfo = await GetSiteInfo();
				if (siteInfo.IsMetadataSyched) {
					_libraries = await LocalStorageDataProvider.GetLibraries();
				} else {
					_libraries = await LiveDataProvider.GetLibraries();
					foreach (var library in _libraries) {
                        if (BinderSettings.IsDefaultLibrary(library))
                        {
                            library.InsertOrUpdate(Db);
                        }
					}
					siteInfo.MetadataSyncCompleted();
					siteInfo.Update(Db);
				}
			}
			return _libraries;
		}

		public async Task<IList<DbDocument>> GetAllDocuments (string libraryId)
		{
            DbLibrary library = await GetLibraryById(libraryId);
			if (library == null) {
				throw new ArgumentException("Invalid library id");
			}

			IList<DbDocument> documents;
			if (library.IsMetadataSynched) {
				documents = await LocalStorageDataProvider.GetAllDocuments(libraryId);
			} else {
                documents = await InsertOrUpdateAllDocuments(library);
			}
			return documents;
		}

		public async Task<IList<DbDocument>> GetDocumentsByFolder (string libraryId, string folderId)
		{
			DbLibrary library = await GetLibraryById(libraryId);
			if (library == null) {
				throw new ArgumentException("Invalid library id");
			}

			IList<DbDocument> documents;
			if (library.IsMetadataSynched) {
				documents = await LocalStorageDataProvider.GetDocumentsByFolder(libraryId, folderId);
			} else {
				await InsertOrUpdateAllDocuments (library);
				documents = await LocalStorageDataProvider.GetDocumentsByFolder(libraryId, folderId);
			}
			return documents;
		}

		private async Task<IList<DbDocument>> InsertOrUpdateAllDocuments (DbLibrary library)
		{
            IList<DbDocument> documents = await LiveDataProvider.GetAllDocuments(library.LibraryId);
			foreach (var document in documents) {
				document.InsertOrUpdate (Db);
			}
			library.MetadataSyncCompleted ();
			library.Update (Db);
			return documents;
		}

		public async Task<DbDocument> GetDocumentById(string libraryId, string documentId) {
			var document = await LocalStorageDataProvider.GetDocumentById(libraryId, documentId);
			//if (document == null) {
			//	document = LiveDataProvider.GetDocumentById(libraryId, documentId);
			//}
			return document;
		}

		public async Task<bool> DownloadDocument (DbDocument document) {
            if (!string.IsNullOrEmpty(document.LocalPath))
            {
                return false;
            }

            bool downloaded = await LiveDataProvider.DownloadDocument(document);
            if (!downloaded)
            {
                return false;
            }
            document.InsertOrUpdate(Db);
            return true;
		}

		public void SyncStart(bool fullSync) {
			LocalStorageDataProvider.SyncStart(fullSync);
		}

		public void SyncComplete(string slideSourceLibrary, string presentationsLibrary, string qaList) {
			LocalStorageDataProvider.SyncComplete(slideSourceLibrary, presentationsLibrary, qaList);
		}

		#endregion
		
		#region Helper functions
		
		private async Task<DbLibrary> GetLibraryById(string libraryId) {
			IList<DbLibrary> libraries = await GetLibraries();
			return libraries.ToList().Find(a => a.LibraryId == libraryId);
		}
		
		#endregion
	}
}

