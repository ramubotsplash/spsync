using System;
using Rohi.EBinder.Entities;
using System.Collections.Generic;
using Rohi.EBinder.Sync;
using Rohi.Apps.Core;
using TinyIoC;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rohi.EBinder.DataAccess
{
	public class AppSession : IAppSession
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

		public AppSession ()
		{
		}
		
		#region IAppSession implementation
		public Task<IList<DbSiteInfo>> GetAllSites ()
		{
			return DbSiteInfo.GetAllSites(Db);
		}
		
		public async Task<IDbBrowser> GetSiteBrowserByTitle(string title)
		{
			var sites = await GetAllSites();
			var siteInfo = sites.ToList().Find(a => string.Equals(a.Title, title, StringComparison.OrdinalIgnoreCase));
			if (siteInfo == null)
			{
				throw new AppException("Unable to find site by title: " + title);
			}
			return DbBrowser.CreateBrowser(siteInfo);
		}
		
		public IDbBrowser GetSiteBrowser (DbSiteInfo siteInfo)
		{
			return DbBrowser.CreateBrowser(siteInfo);
		}

		#endregion

		#region User authentication

		public async Task<IDbBrowser> AuthenticateUser (ServerEdition edition, string url, string loginName, string password, bool autoLogin)
		{
			// validate arguments
			if (string.IsNullOrEmpty(url)) {
				throw new AppException("Please enter SharePoint server Url");
			}
			if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
				throw new AppException("Invalid SharePoint Url");
			}

			if (string.IsNullOrEmpty(loginName)) {
				throw new AppException("Please enter your login name");
			}
			if (!loginName.Contains("\\")) {
				throw new AppException("Invalid login name.  Login name format: Domain\\Username");
			}

			if (string.IsNullOrEmpty(password)) {
				throw new AppException("Please enter your password");
			}

			IServerDataProvider dataService = SyncFactory.GetDataProvider(edition,
			                                                              url, loginName, password,
			                                                              DbBrowseMode.LiveAccess,
			                                                              BinderSettings.CacheDuration);
			
			DbSiteInfo siteInfo = await dataService.GetSiteInfo();
			siteInfo.SetCredentials(edition, url, loginName, password, autoLogin);
			siteInfo.InsertOrUpdate(Db);

			return DbBrowser.CreateBrowser(siteInfo);
		}
		#endregion

		#region User authentication Asynchronous

		//Status Event Handlers
		public event AuthenticationEventHandler AuthenticationChanged;

        // Start the download
        public async void AuthenticateUserAsync(ServerEdition edition, string url, string loginName, string password, bool autoLogin)
		{
			//Thread gc...
			try {
				// notify authentication started
				if (AuthenticationChanged != null) {
					AuthenticationChanged(this, new AuthenticationArgs("Connecting to server ...") { IsStarted = true });
				}

				IDbBrowser dbBrowser = await AuthenticateUser(edition, url, loginName, password, autoLogin);

				// notify authentication completed
				if (AuthenticationChanged != null) {
					AuthenticationChanged(this,
						new AuthenticationArgs("Authentication Successful") {
							LoginSucceeded = true,
							DbBrowser = dbBrowser
						});
				}
			} catch (Exception ex) {
				// notify authentication completed
				if (AuthenticationChanged != null) {
					AuthenticationChanged(this,
						new AuthenticationArgs("Authentication failed: " + ex.Message) {
							LoginFailed = true
						});
				}
			}
		}

		#endregion
	}
}
