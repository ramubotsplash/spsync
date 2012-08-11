using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rohi.EBinder.Entities;

namespace Rohi.EBinder.DataAccess
{
	public interface IAppSession
	{
		event AuthenticationEventHandler AuthenticationChanged;
		void AuthenticateUserAsync(ServerEdition edition, string url, string loginName, string password, bool autoLogin);

		Task<IDbBrowser> AuthenticateUser(ServerEdition edition, string url, string loginName, string password, bool autoLogin);

		Task<IDbBrowser> GetSiteBrowserByTitle(string title);
		IDbBrowser GetSiteBrowser(DbSiteInfo siteInfo);
        Task<IList<DbSiteInfo>> GetAllSites();
	}
}
