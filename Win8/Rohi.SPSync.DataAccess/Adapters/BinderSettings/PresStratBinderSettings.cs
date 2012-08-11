using System;
using Rohi.EBinder.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Rohi.EBinder.DataAccess
{
	public class PresStratBinderSettings : IBinderSettings
	{
		public const string SlidesLibrary_Title = "SlideSource";
		public const string PresentationsLibrary_Title = "Core Presentation";
		
		public PresStratBinderSettings ()
		{
		}

		#region IBinderSettings implementation
		public Rohi.EBinder.Entities.DbFileType GetFileType (string fileName)
		{
			throw new System.NotImplementedException ();
		}

		public Rohi.EBinder.Entities.DbLibraryType GetLibraryType (int baseTemplate, string templateName)
		{
			throw new System.NotImplementedException ();
		}

		public bool IsDefaultLibrary (Rohi.EBinder.Entities.DbLibrary library)
		{
			if (string.Equals(library.Title, SlidesLibrary_Title, StringComparison.OrdinalIgnoreCase) ||
			    string.Equals(library.Title, PresentationsLibrary_Title, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
			return false;
		}

		public bool IsDefaultDocument (Rohi.EBinder.Entities.DbDocument document)
		{
			return true;
		}

		private static List<string> SharePointInternalLibraries = new List<string>() {
			"Site Assets", "Site Collection Documents", "Site Pages",
			"Content and Structure Reports", "Reusable Content", "Tasks", "Workflow Tasks"
		};

		public bool IsValidLibrary (DbLibraryType libraryType, Rohi.EBinder.Entities.DbLibrary library)
		{
			return library.ListType == libraryType &&
				   library.Hidden != true &&
				   !SharePointInternalLibraries.Any(item => string.Equals(item, library.Title, StringComparison.OrdinalIgnoreCase));
		}

		public bool ShowSubsites {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public bool SyncCompleteSite {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public bool SyncCompleteLibrary {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public string ApplicationTitle {
			get {
				return Constants.App.AppName;
			}
		}

		public string About {
			get {
				return Constants.App.AppName;
			}
		}

		public string Copyright {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public bool AllowMultipleSites {
			get {
				return false;
			}
		}


		public TimeSpan CacheDuration {
			get {
				return new TimeSpan(0, 5, 0);
			}
		}
		
		#endregion
	}
}

