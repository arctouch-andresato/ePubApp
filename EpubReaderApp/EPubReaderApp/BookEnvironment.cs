using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace EPubReaderApp
{
    public static class BookEnvironment
    {
        public const string TempBookFolderName = "epubBook";
        public static string GetLocalPathForAssets()
        {
            var fullPath = "";
            if (Device.RuntimePlatform == "UWP")
            {
                var bookFolder = "books\\";
                fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), bookFolder);
            }
            else
            {
                var bookFolder = "books/";
                fullPath = Path.Combine(DependencyService.Get<IBaseUrl>().Get(), bookFolder);
            }

            return fullPath;
        }

        public static string GetTempLocalPathForEpub()
        {
            var fullPath = "";
            if (Device.RuntimePlatform == "UWP")
            {
                var bookFolder = TempBookFolderName + "\\";
                fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), bookFolder);
            }
            else
            {
                var bookFolder = TempBookFolderName + "/";
                fullPath = Path.Combine(DependencyService.Get<IBaseUrl>().Get(), bookFolder);
            }

            return fullPath;
        }

        public static string AddSourceTempPath(string htmlFileName)
        {
            if (Device.RuntimePlatform == "UWP")
            {
                return DependencyService.Get<IBaseUrl>().Get() + htmlFileName;
            }
            var bookFolder = TempBookFolderName + "/";
            return Path.Combine(DependencyService.Get<IBaseUrl>().Get(), bookFolder) + htmlFileName;
        }

        public static string CombinePath(string path1, string path2)
        {
            var fullPath = Path.Combine(path1, path2);
            if (Device.RuntimePlatform == "UWP")
            {
                return fullPath.Replace("/", "\\");
            }
            return fullPath;
        }
    }
}
