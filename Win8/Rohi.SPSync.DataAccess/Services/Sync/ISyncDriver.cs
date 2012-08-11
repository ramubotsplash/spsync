using System;
using Rohi.EBinder.Entities;
using System.Collections.Generic;

namespace Rohi.EBinder.DataAccess
{
	public interface ISyncDriver
	{
		event SyncProgressEventHandler ProgressChanged;

        void Run(IDbBrowser browser, bool fullSync);
        void Run(IDbBrowser browser, string slideSourceLibrary, string presentationLibrary, string qaList, bool fullSync);
		bool Stop();
	}
}

