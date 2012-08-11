using System;
using Rohi.EBinder.Entities;
using Rohi.Apps.Core;
using TinyIoC;

namespace Rohi.EBinder.Sync
{
	public class DefaultSyncFactory : ISyncFactory
	{
		public DefaultSyncFactory ()
		{
		}

		#region Server Data Management
		
		private IServerDataProvider GetLocalStorageProvider(string url)
		{
			LocalStorageServerDataProvider newProvider = new LocalStorageServerDataProvider(url);
			return newProvider;
		}

		private IServerDataProvider GetLiveDataProvider (ServerEdition edition, string url, string loginName, string password)
		{
			IServerDataProvider liveDataProvider;
			switch (edition) {
			case ServerEdition.MOSS:
			case ServerEdition.SP2010:
			case ServerEdition.SP2013:
				liveDataProvider = new SP2010ServerDataProvider(url, loginName, password);
				break;
			default:
				throw new AppException ("Invalid server edition: " + edition.ToString ());
			}
			return liveDataProvider;
		}
		
		private IServerDataProvider GetCachedDataProvider(ServerEdition edition, string url, string loginName, string password,
		                                                  TimeSpan cacheDuration)
		{
			IServerDataProvider localProvider = GetLocalStorageProvider(url);
			IServerDataProvider liveProvider = GetLiveDataProvider (edition, url, loginName, password);
			
			CachedServerDataProvider newProvider = new CachedServerDataProvider();
			newProvider.LocalStorageDataProvider = localProvider;
			newProvider.LiveDataProvider = liveProvider;
			
			return newProvider;
		}
		
		public IServerDataProvider GetDataProvider(ServerEdition edition, string url, string loginName, string password,
		                                           DbBrowseMode browseMode, TimeSpan cacheDuration)
		{
			switch (browseMode) {
			case DbBrowseMode.LocalStorage:
				return GetLocalStorageProvider(url);
			case DbBrowseMode.LiveAccess:
				return GetLiveDataProvider(edition, url, loginName, password);
			case DbBrowseMode.CachedAccess:
				return GetCachedDataProvider(edition, url, loginName, password, cacheDuration);
			}
			throw new AppException("Data provider not found: " + edition);
		}
		
		#endregion
	}
}

