using EPubReaderApp.UWP;
using System;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(BaseUrl))]
namespace EPubReaderApp.UWP
{
    public class BaseUrl : IBaseUrl
    {
        public string Get()
        {
            Console.WriteLine(ApplicationData.Current.LocalFolder);
            return "ms-appdata:///local/" + BookEnvironment.TempBookFolderName + "/";
        }
    }
}
