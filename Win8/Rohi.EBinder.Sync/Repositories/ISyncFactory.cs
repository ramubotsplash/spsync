using System;
using Rohi.EBinder.Entities;

namespace Rohi.EBinder.Sync
{
	public interface ISyncFactory
	{
		IServerDataProvider GetDataProvider(ServerEdition edition,
				string url, string loginName, string password, DbBrowseMode browseMode, TimeSpan cacheDuration);
	}
}

