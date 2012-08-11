using System;

namespace Rohi.EBinder.Entities
{
	public static class TypeUtilities
	{
		public static int GetListTypeSortIndex(int listType) {
			SPListTemplateType listTemplateType = (SPListTemplateType)listType;
			switch (listTemplateType) {
				case SPListTemplateType.SlideLibrary:
					return 1;
				case SPListTemplateType.PictureLibrary:
					return 2;
				case SPListTemplateType.DocumentLibrary:
					return 3;
				default:
					return 10;
			}
		}

		public static string GetListTypeDisplayName(int listType) {
			SPListTemplateType listTemplateType = (SPListTemplateType)listType;
			switch (listTemplateType) {
				case SPListTemplateType.SlideLibrary:
					return "Slides";
				case SPListTemplateType.PictureLibrary:
					return "Pictures";
				case SPListTemplateType.DocumentLibrary:
					return "Documents";
				default:
					return "Other";
			}
		}
	}
}

