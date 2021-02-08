using System;
using Android.Content;
using Android.OS;
using Java.IO;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Live.Utils
{
    public static class FileUtil
    {
        private static string LogFolderName = "log";
        private static string LogFileName = "agora-rtc.log";
         
        /// <summary>
        /// Initialize the log folder
        /// </summary>
        /// <param name="context">Context to find the accessible file folder</param>
        /// <returns>the absolute path of the log file</returns>
        public static string InitializeLogFile(Context context)
        {
            try
            {
                File folder;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                {
                    folder = new File(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments), LogFolderName);
                }
                else
                {
                    string path = Methods.Path.FolderDcimMyApp + File.Separator + context.PackageName + File.Separator + LogFolderName;
                    folder = new File(path);
                    if (!folder.Exists() && !folder.Mkdir()) folder = null;
                }

                if (folder != null && !folder.Exists() && !folder.Mkdir())
                    return "";
                else
                    return new File(folder, LogFileName).AbsolutePath;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }
    }

}