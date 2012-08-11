using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using Rohi.Apps.Core;
using Rohi.EBinder.Entities;
using Rohi.EBinder.Sync.ListsReference;
using Rohi.EBinder.Sync.WebsReference;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;
using Windows.Storage;

namespace Rohi.EBinder.Sync
{
	public class SP2010ServerDataProvider : IServerDataProvider
	{
		private string				UserName		{ get; set; }
		private string				Password		{ get; set; }
		private string				Url				{ get; set; }
        private WebsSoapClient      WebsService     { get; set; }
		private ListsSoapClient		ListsService	{ get; set; }

        private NetworkCredential Credentials
        {
			get {
				return new NetworkCredential(this.UserName, this.Password);
			}
		}
		
		public SP2010ServerDataProvider (string url, string userName, string password)
		{
			this.Url = url;
			this.UserName = userName;
			this.Password = password;
			
			CreateWebServices();
		}

        private void SetupCredentials(ClientCredentials credentials)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                return;
            }

            var sepIndex = UserName.IndexOf('\\');
            var domain = sepIndex >= 0 ? UserName.Substring(0, sepIndex) : string.Empty;
            var userName = sepIndex >= 0 && UserName.Length > sepIndex + 1? UserName.Substring(sepIndex + 1, UserName.Length - sepIndex - 1) : UserName;

            credentials.Windows.ClientCredential.Domain = domain;
            credentials.Windows.ClientCredential.UserName = userName;
            credentials.Windows.ClientCredential.Password = this.Password;
        }


        private System.ServiceModel.Channels.Binding _bindings;
        private System.ServiceModel.Channels.Binding GetServiceBindings()
        {
            if (_bindings == null)
            {
                var newBinding = new BasicHttpBinding();
                newBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                newBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                newBinding.MaxReceivedMessageSize = Int32.MaxValue;

                _bindings = newBinding;
            }
            return _bindings;
        }
		
		private void CreateWebServices() {

            // create webs service
            if (WebsService == null)
            {
                var endpoint = new EndpointAddress(PathUtil.CombineUrl(this.Url, "_vti_bin/webs.asmx"));
                WebsService = new WebsSoapClient(GetServiceBindings(), endpoint);
                //WebsService = new WebsSoapClient(WebsSoapClient.EndpointConfiguration.WebsSoap, endpoint);
                //WebsService.ClientCredentials.UserName.UserName = this.UserName;
                //WebsService.ClientCredentials.UserName.Password = this.Password;
                //SetupCredentials(WebsService.ClientCredentials);
                WebsService.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                WebsService.ChannelFactory.Credentials.Windows.ClientCredential = this.Credentials;
            }

            // create lists service
			if (ListsService == null) {
                var endpoint = new EndpointAddress(PathUtil.CombineUrl(this.Url, "_vti_bin/lists.asmx"));
                ListsService = new ListsSoapClient(GetServiceBindings(), endpoint);
                ListsService.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                ListsService.ChannelFactory.Credentials.Windows.ClientCredential = this.Credentials;
            }
		}

		#region IDisposable implementation
		public void Dispose ()	
		{
			if (WebsService != null) {
				WebsService.Abort();
				WebsService = null;
			}
			
			if (ListsService != null) {
				ListsService.Abort();
				ListsService = null;
			}
		}
		#endregion
		
		#region IServerData implementation

		private DbSiteInfo _siteInfo;
		public async Task<DbSiteInfo> GetSiteInfo ()
		{
			if (_siteInfo == null)
            {
				Log.LogMessage("GetSiteInfo: {0} using {1}", this.Url, this.UserName);
                var webInfo = await WebsService.GetWebAsync(this.Url);
                Log.LogMessage("SiteInfo Xml: {0}", webInfo.Body.GetWebResult.ToString());
				
				_siteInfo = DbSiteInfo.CreateNew(ServerEdition.SP2010, this.Url, this.UserName, this.Password);
				_siteInfo.LoadFromSharePointNode(webInfo.Body.GetWebResult);
            }
			return _siteInfo;
		}

		private IList<DbLibrary> _libraries;
		public async Task<IList<DbLibrary>> GetLibraries ()
		{
			if (_libraries == null) {
	            var listsData = await ListsService.GetListCollectionAsync();
				_libraries = DbLibrary.CreateListFromSharePointXmlNode(
                                listsData.Body.GetListCollectionResult,
                                this.Url);
			}
			return _libraries;
		}

		public async Task<IList<DbDocument>> GetAllDocuments (string libraryId)
		{
			IList<DbLibrary> libraries = await GetLibraries();
			DbLibrary library = (from a in libraries where a.LibraryId == libraryId select a).FirstOrDefault();
			if (library == null) {
				throw new AppException("Unable to find library by id");
			}

			XElement doc = XElement.Parse("<Document><Query /><ViewFields /><QueryOptions><ViewAttributes Scope=\"RecursiveAll\" /></QueryOptions></Document>");

			var listQuery = doc.Element("Query");
			var listViewFields = doc.Element("ViewFields");
            var listQueryOptions = doc.Element("QueryOptions");
			var items = await ListsService.GetListItemsAsync(
								library.Title,
                                string.Empty,
                                listQuery,
								listViewFields,
                                Constants.SharePointWebService.QueryItemsRowLimit,
                                listQueryOptions,
                                null);
			IList<DbDocument> docs = DbDocument.CreateListFromSharePointXmlNode(
                                        items.Body.GetListItemsResult,
                                        this.Url,
                                        library);
			return docs;
		}

		public Task<IList<DbDocument>> GetDocumentsByFolder (string libraryId, string folderId)
		{
			throw new NotImplementedException();
		}

		public Task<DbDocument> GetDocumentById(string libraryId, string documentId)
		{
			throw new NotImplementedException();
		}

        private string GetDocumentPath(string documentUrl)
        {
            return Path.Combine(
                                    ApplicationData.Current.LocalFolder.Path,
                                    documentUrl.Replace('/', '\\'));
        }

		public async Task<bool> DownloadDocument (DbDocument document) {
			// thumbnail download
			string thumbnailPath = null;
			if (document.Type != DbFileType.Folder && !string.IsNullOrEmpty(document.ThumbnailUrl)) {
				thumbnailPath = PathUtil.CombineUrl(
                                        ApplicationData.Current.LocalFolder.Path,
									    document.ThumbnailUrl);
				Log.LogMessage("ThumbnailPath Path: {0}", thumbnailPath);
                var downloaded = await DownloadFile(document.ThumbnailUrl, thumbnailPath, true);
				if (!downloaded) {
					thumbnailPath = null;
				}
			}

			// document download
			string localPath = GetDocumentPath(document.DocumentUrl);
			Log.LogMessage("Local Path: {0}", localPath);
			for(int downloadAttempt = 1; downloadAttempt <= 3; downloadAttempt++) {
				try {
                    var downloaded = await DownloadFile(document.DocumentUrl, localPath, true);
					if (downloaded) {
						break;
					}
				} catch (Exception ex) {
					// do nothing
					Log.LogException(
						string.Format("Failed downloading file attempt: {0}; {1}", downloadAttempt, document.DocumentUrl),
						ex);
				}
			}
			   

			document.DownloadCompleted(localPath, thumbnailPath);
			return true;
		}

		private async Task<bool> DownloadFile (string downloadUrl, string savePath, bool force)
		{
            var saveFile = await StorageFile.GetFileFromPathAsync(savePath);
            if (!force && saveFile != null)
            {
				return false;
			}

			// Delete existing file
			if (saveFile != null) {
                await saveFile.DeleteAsync();
			}

			// Ensure local path
			PathUtil.EnsureDirectoryPath(savePath);

			// Download file
			var fullUrl = new Uri(new Uri(this.Url), downloadUrl);
			//using (var httpclient = new HttpClient()) {
			//	wc.Credentials = this.Credentials;
			//	wc.DownloadFile(fullUrl, savePath);
			//}

			return true;
		}

		public void SyncStart(bool fullSync) {
		}

		public void SyncComplete(string slideSourceLibrary, string presentationsLibrary, string qaList) {
		}

		#endregion
	}
}

