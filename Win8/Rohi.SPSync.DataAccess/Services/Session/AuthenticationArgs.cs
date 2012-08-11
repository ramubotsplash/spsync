using System;

namespace Rohi.EBinder.DataAccess
{
	public class AuthenticationArgs : EventArgs
	{
		public AuthenticationArgs(string message) {
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

		public bool? LoginFailed {
			get;
			set;
		}

		public bool? LoginSucceeded {
			get;
			set;
		}

		public IDbBrowser DbBrowser {
			get;
			set;
		}
	}

	public delegate void AuthenticationEventHandler(object sender, AuthenticationArgs args);
}
