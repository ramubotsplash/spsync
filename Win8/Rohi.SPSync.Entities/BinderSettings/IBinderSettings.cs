using System;
using System.Collections.Generic;

namespace Rohi.EBinder.Entities
{
	public interface IBinderSettings
	{
		// Get File type
		DbFileType GetFileType(string fileName);
		DbLibraryType GetLibraryType(int baseTemplate, string templateName);

		// Show hide documents/libraries
		bool IsDefaultLibrary(DbLibrary library);
		bool IsDefaultDocument(DbDocument document);

		bool IsValidLibrary(DbLibraryType libraryType, DbLibrary library);

		// Application settings
		bool AllowMultipleSites { get; }
		TimeSpan CacheDuration { get; }
		
		// Site settings
		bool ShowSubsites { get; }
		
		// Sync settings
		bool SyncCompleteSite { get; }
		bool SyncCompleteLibrary { get; }

		// About application
		string ApplicationTitle { get; }
		string About { get; }
		string Copyright { get; }
	}
}
