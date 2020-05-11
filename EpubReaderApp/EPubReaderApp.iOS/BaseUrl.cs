using System;
using EPubReaderApp.iOS;
using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(BaseUrl))]

namespace EPubReaderApp.iOS
{
	public class BaseUrl : IBaseUrl
	{
		public string Get()
		{
			return NSBundle.MainBundle.ResourcePath +"/" + BookEnvironment.TempBookFolderName + "/";
		}
	}
}
