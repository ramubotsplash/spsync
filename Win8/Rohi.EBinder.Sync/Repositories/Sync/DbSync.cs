using System;
using Rohi.Apps.Core;
using Rohi.EBinder.Entities;

namespace Rohi.EBinder.Sync
{
	public class DbSync : IDbSync
	{
		public DbSync ()
		{
		}

		#region IDbSync implementation
		public event SyncProgressEventHandler DownloadProgress;

		public DbSettings GetGlobalSettings ()
		{
			throw new NotImplementedException ();
		}

		public void UpdateGlobalSettings (DbSettings settings)
		{
			throw new NotImplementedException ();
		}

		public void SyncSiteStructure ()
		{
			throw new NotImplementedException ();
		}

		public void SyncLibrary (int libraryId, bool incremental)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}

