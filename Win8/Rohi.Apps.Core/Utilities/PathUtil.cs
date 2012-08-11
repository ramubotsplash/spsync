using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Rohi.Apps.Core
{
	public static class PathUtil
	{
		public static string EscapeUrl(string url) {
			if (string.IsNullOrEmpty(url)) {
				return url;
			}
			return url.Replace("%20", " ");
		}

		public static string CombineUrl(string hostUrl, string path) {
			if (string.IsNullOrEmpty(path)) {
				return hostUrl;
			}
			if (string.IsNullOrEmpty(hostUrl)) {
				return path;
			}
			return EscapeUrl(hostUrl.TrimEnd('/') + '/' + path.TrimStart('/'));
		}

		public static string GetRelativeUrl (string docUrl)
		{
			if (string.IsNullOrEmpty(docUrl)) {
				return null;
			}

            string absolutePath = docUrl;
            if (Uri.IsWellFormedUriString(docUrl, UriKind.Absolute)) {
			    Uri uri = new Uri(docUrl, UriKind.RelativeOrAbsolute);
                docUrl = uri.AbsolutePath;
            }
			return EscapeUrl(docUrl);
		}

		public static async void EnsureDirectoryPath (string filePath)
		{
            var installationFolder = Package.Current.InstalledLocation;
            if (!filePath.ToLower().StartsWith(installationFolder.Path.ToLower())) {
                throw new AppException("Invalid file path for the selected file");
            }

            var additionalPath = filePath.Substring(installationFolder.Path.Length - 1);

            var parentFolder = installationFolder;
            var additionalFolderTokens = additionalPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var folderToken in additionalFolderTokens)
            {
                var childFolder = await parentFolder.GetFolderAsync(folderToken);
                if (childFolder == null)
                {
                    // create child folder
                    childFolder = await parentFolder.CreateFolderAsync(folderToken);
                    if (childFolder == null)
                    {
                        throw new AppException(
                            string.Format("Failed to create storage folder: {0};{1}",
                                parentFolder.Path, folderToken));
                    }
                }
                parentFolder = childFolder;
			}
		}

		public static string GetFolderPath (string filePath) {
			if (string.IsNullOrEmpty(filePath)) {
				return null;
			}
			var index = filePath.TrimEnd('/').LastIndexOf('/');
			return index < 0 ? null : EscapeUrl(filePath.Substring(0, index));
		}
	}
}

