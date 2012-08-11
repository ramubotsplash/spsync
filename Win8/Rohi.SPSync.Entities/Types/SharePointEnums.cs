using System;

namespace Rohi.EBinder.Entities
{
	public enum SPListTemplateType : int
	{
		InvalidType 		= -1,
		GenericList 		= 100,
		DocumentLibrary		= 101,
		Survey 				= 102,
		Links 				= 103,
		Announcements		= 104,
		Contacts 			= 105,
		Events 				= 106,
		asks 				= 107,
		DiscussionBoard 	= 108,
		PictureLibrary 		= 109,
		DataSources 		= 110,
		WebTemplateCatalog 	= 111,
		WebPartCatalog 		= 113,
		ListTemplateCatalog = 114,
		XMLForm 			= 115,
		CustomGrid 			= 120,
		IssueTracking 		= 1100,
		SlideLibrary		= 2100
	}
}

