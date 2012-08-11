using System;
using Rohi.EBinder.DataAccess;
using System.Threading;
using System.IO;
using Rohi.EBinder.Entities;
using TinyIoC;
using Rohi.Apps.Core;
using System.Collections.Generic;
using System.Linq;

namespace Rohi.EBinder.DataAccess
{
	public class SyncDriver	 : ISyncDriver
	{
		#region Local Members

		private IBinderSettings BinderSettings {
			get {
				var container = TinyIoCContainer.Current;
				return container.Resolve<IBinderSettings>();
			}
		}

		// library access
		private IDbBrowser Browser {
			get;
			set;
		}

        private bool SyncAllLibraries
        {
            get;
            set;
        }

		private string SelectedSlideSourceLibrary {
			get;
			set;
		}

		private string SelectedPresentationLibrary {
			get;
			set;
		}

		private string SelectedQAList {
			get;
			set;
		}

		private bool FullSync {
			get;
			set;
		}

		#endregion

		#region Interface methods

		//Status Event Handlers
		public event SyncProgressEventHandler ProgressChanged;

        public void Run(IDbBrowser browser, bool fullSync)
        {
            // Save browser info
            Browser = browser;
            SyncAllLibraries = true;

			// Start thread
            StartDownload();
        }

		public void Run(IDbBrowser browser, string slideSourceLibrary, string presentationLibrary, string qaList, bool fullSync) {
			// Save browser info
			Browser = browser;
            SyncAllLibraries = false;
            SelectedSlideSourceLibrary = slideSourceLibrary;
			SelectedPresentationLibrary = presentationLibrary;
			SelectedQAList = qaList;
			FullSync = fullSync;

			// Start thread
            StartDownload();
		}

		public bool Stop() {
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation

		private bool IsSelectedLibrary(DbLibrary library) {
			return ((SyncAllLibraries && BinderSettings.IsDefaultLibrary(library)) ||
			        string.Equals(SelectedPresentationLibrary, library.Title, StringComparison.OrdinalIgnoreCase) ||
			        string.Equals(SelectedSlideSourceLibrary, library.Title, StringComparison.OrdinalIgnoreCase) ||
			        string.Equals(SelectedQAList, library.Title, StringComparison.OrdinalIgnoreCase));
		}

		// 
		private bool IsStopRequested() {
			return false;
		}

		// Start the download
		private async void StartDownload ()
		{
			//Thread gc...
			try {
				NotifySyncStarted();
				Browser.SyncStart(FullSync);
                var libraries = await Browser.GetLibraries();
				foreach (var libary in libraries) {
					if (IsStopRequested()) break;
					if (IsSelectedLibrary(libary)) {
						DownloadLibrary(libary);
					}
				}
				if (IsStopRequested()) {
					NotifyStatusChanged("Cancelling sync ...");
					NotifySyncCanceled();
				} else {
					NotifyStatusChanged("Completing sync ...");
					Browser.SyncComplete(SelectedSlideSourceLibrary, SelectedPresentationLibrary, SelectedQAList);
					NotifySyncCompleted();
				}
			} catch (Exception ex) {
				Log.LogException("Failed downloading file", ex);
                if (!NotifySyncFailed(ex.Message))
                {
                    throw;
                }
			}
		}

		private async void DownloadLibrary (DbLibrary library)
		{
			NotifyStatusChanged(string.Format("Retrieving {0}...", library.Title));
			var documents = await Browser.GetAllDocuments (library.LibraryId);

			if (library.BaseType == 0) {
				// Custom List
				return;
			}
			// Start downloading files
			if (documents != null) {
				foreach (var document in documents) {
					if (IsStopRequested()) break;
					if (document.Type == Rohi.EBinder.Entities.DbFileType.Folder) {
						continue;
					}

					NotifyStatusChanged(string.Format("Downloading {0}...",
					                                  Path.GetFileName(document.DocumentUrl)));
					Browser.DownloadDocument(document);
				}
			}
		}

		#endregion

		#region Notification helper functions

		private void NotifySyncStarted() {
			if (ProgressChanged != null) {
				ProgressChanged(this, new SyncProgressArgs("Completed") { IsStarted = true } );
			}
		}

		private void NotifySyncCompleted() {
			if (ProgressChanged != null) {
				ProgressChanged(this, new SyncProgressArgs("Completed") { IsCompleted = true } );
			}
		}

		private void NotifySyncCanceled() {
			if (ProgressChanged != null) {
				ProgressChanged(this, new SyncProgressArgs("Canceled") { IsCanceled = true } );
			}
		}

		private bool NotifySyncFailed(string message) {
			if (ProgressChanged != null) {
				ProgressChanged(this, new SyncProgressArgs("Failed Downloading files: " + message) { IsFailed = true } );
                return true;
			}
            return false;
		}

		private void NotifyStatusChanged(string status) {
			if (ProgressChanged != null) {
				ProgressChanged(this, new SyncProgressArgs(status));
			}
		}

		#endregion
	}
}

