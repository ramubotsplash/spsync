using System;

namespace Rohi.EBinder.DataAccess
{
	public class SyncProgressArgs : EventArgs
	{
		public SyncProgressArgs(string message) {
			Message = message;
		}
		
		public string Message {
			get;
			private set;
		}

		public bool IsStarted {
			get;
			set;
		}

		public bool IsCompleted {
			get;
			set;
		}

		public bool IsFailed {
			get;
			set;
		}

		public bool IsCanceled {
			get;
			set;
		}
	}

	public delegate void SyncProgressEventHandler(object sender, SyncProgressArgs args);
}
