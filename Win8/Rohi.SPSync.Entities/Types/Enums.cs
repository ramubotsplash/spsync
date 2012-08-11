using System;

namespace Rohi.EBinder.Entities
{
	public enum DbLibraryType
	{
		PresentationLibrary,
		SlideLibrary,
		PictureLibrary,
		DocumentLibrary,
		
		Announcements,
		Contacts,
		DiscussionBoard,
		Links,
		Calendar,
		CustomList
	}
	
	public enum DbFileType
	{
		Folder,
		Doc,
		Ppt,
		Xls,
		Pdf,
		Item,
		Unknown
	}
	
	public enum ServerEdition
	{
		Unknown,
		MOSS,
		SP2010,
		SP2013
	}
	
	public enum DbBrowseMode
	{
		LocalStorage,
		LiveAccess,
		CachedAccess
	}
}

