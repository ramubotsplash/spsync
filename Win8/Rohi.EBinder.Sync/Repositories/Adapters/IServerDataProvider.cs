using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rohi.EBinder.Entities;

namespace Rohi.EBinder.Sync
{
	public interface IServerDataProvider : IDisposable
	{
		Task<DbSiteInfo> GetSiteInfo();
		Task<IList<DbLibrary>> GetLibraries();
		Task<IList<DbDocument>> GetAllDocuments(string libraryId);
		Task<IList<DbDocument>> GetDocumentsByFolder(string libraryId, string folderId);

        Task<DbDocument> GetDocumentById(string libraryId, string documentId);
        Task<bool> DownloadDocument(DbDocument document);

		void SyncStart(bool fullSync);
		void SyncComplete(string slideSourceLibrary, string presentationsLibrary, string qaList);
	}
}

