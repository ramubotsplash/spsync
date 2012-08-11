using System;
using System.Collections.Generic;
using Rohi.EBinder.Entities;
using System.Linq;

namespace Rohi.EBinder.DataAccess
{
	public class DefaultBinderSettings : IBinderSettings
	{
		public DefaultBinderSettings ()
		{
		}

        public DbFileType GetFileType(string fileName)
        {
            throw new NotImplementedException();
        }

        public DbLibraryType GetLibraryType(int baseTemplate, string templateName)
        {
            throw new NotImplementedException();
        }

        public bool IsDefaultLibrary(DbLibrary library)
        {
            return !Constants.SharePointWebService.SharePointInternalLibraries
                        .Any(item =>
                            string.Equals(item, library.Title, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsDefaultDocument(DbDocument document)
        {
            return true;
        }

        public bool IsValidLibrary(DbLibraryType libraryType, DbLibrary library)
        {
            return library.ListType == libraryType &&
                   library.Hidden != true &&
                   !Constants.SharePointWebService.SharePointInternalLibraries.Any(item => string.Equals(item, library.Title, StringComparison.OrdinalIgnoreCase));
        }

        public bool AllowMultipleSites
        {
            get { return true; }
        }

        public TimeSpan CacheDuration
        {
            get { return new TimeSpan(0, 5, 0); }
        }

        public bool ShowSubsites
        {
            get { throw new NotImplementedException(); }
        }

        public bool SyncCompleteSite
        {
            get { throw new NotImplementedException(); }
        }

        public bool SyncCompleteLibrary
        {
            get { throw new NotImplementedException(); }
        }

        public string ApplicationTitle
        {
            get
            {
                return Constants.App.AppName;
            }
        }

        public string About
        {
            get
            {
                return Constants.App.AppName;
            }
        }

        public string Copyright
        {
            get { throw new NotImplementedException(); }
        }
    }
}

