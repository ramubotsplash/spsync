using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rohi.EBinder.Entities;

namespace Rohi.EBinder.DataAccess
{
	public interface IDbBrowser
	{
		DbSiteInfo SiteInfo { get; }
		
		Task<IList<DbLibrary>> GetLibraries();

		Task<DbLibrary> GetLibraryById(string id);
		Task<DbLibrary> GetLibraryByTitle(string title);

		Task<IList<DbDocument>> GetAllDocuments(string libraryId);
		// get is flat retrieval
		Task<IList<DbDocument>> GetDocumentsByFolder(string libraryId, string folderId);
		// search is recursive
		Task<IList<DbDocument>> SearchDocumentsByFolder(string libraryId, string folderId, string filter);

		Task<IList<string>> GetLibraryColumnDistinctValues(string libraryId, string columnName, bool lookupColumn);

		Task<bool> DownloadDocument(DbDocument document);

		void SyncStart(bool fullSync);
		void SyncComplete(string slideSourceLibrary, string presentationsLibrary, string qaList);
	}
}

