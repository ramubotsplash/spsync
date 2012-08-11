using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Rohi.Apps.Core
{
	public static class Log
	{
        public static string BaseDir {
            get {
                return ApplicationData.Current.LocalFolder.Path;
            }
        }

        private static long lastTime;
        public static async void LogMessage (string format, params object [] args)
		{
            long now = DateTime.Now.Ticks;
            string message = string.Format("{0} ({1}): {2}", DateTime.Now, now-lastTime, string.Format(format, args));
			lastTime = DateTime.Now.Ticks;

            var crashFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("crash.log", CreationCollisionOption.OpenIfExists);
            if (crashFile != null)
            {
                //var writestream = await crashFile.OpenStreamForWriteAsync();
                //if (writestream != null) {
                //    var bytes = Encoding.UTF8.GetBytes(message);
                //    writestream.Write(bytes, 0, bytes.Length);
                //    await writestream.FlushAsync();
                //    writestream.Dispose();
                //}
            }
        }
		
		public static void LogException (string text, Exception e)
		{
            LogMessage("Message: {0}; Exception: {1}; InnerException: {2}; Stack Trace: {3}",
                 text,
                 e.Message,
                 e.InnerException != null ? e.InnerException.Message : "None",
                 e.StackTrace);
		}
	}
}

