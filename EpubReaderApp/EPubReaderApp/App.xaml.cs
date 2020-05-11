using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EPubReaderApp.Services;
using EPubReaderApp.Views;
using MonkeyCache.SQLite;

namespace EPubReaderApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<ContentService>();
            
            InitializeServices();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void InitializeServices()
        {
            Barrel.ApplicationId = "EpubReaderAppCacheId";
            DependencyService.Get<IContentService>().Initialize();
        }
    }
}
