using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EPubReaderApp.Models;
using Plugin.FilePicker;
using VersOne.Epub;
using System.IO;
using System.Linq;
using Xamarin.Forms.Internals;
using EPubReaderApp.Services;

namespace EPubReaderApp.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        private int index = 0;
        private EpubBook epubBook;
        private string baseUrl = DependencyService.Get<IBaseUrl>().Get();
        private IContentService contentService = DependencyService.Get<IContentService>();

        public NewItemPage()
        {
            InitializeComponent();

            Item = new Item();

            BindingContext = this;
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopModalAsync();
        }

        async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var fileData = await CrossFilePicker.Current.PickFile();
            if (fileData == null)
                return; // user canceled file picking

            epubBook = EpubReader.ReadBook(fileData.FilePath);
            //SaveImages();
            Item.Text = epubBook.Title;
            
            try
            {
                var coverFileName = "";
                if (Device.RuntimePlatform == "UWP")
                {
                    coverFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempCover.jpg");
                }
                else
                {
                    coverFileName = Path.Combine(baseUrl, "tempCover.jpg");
                }
                File.WriteAllBytes(coverFileName, epubBook.CoverImage ?? epubBook.Content.Images.FirstOrDefault().Value.Content);
                this.CoverImage.Source = coverFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            this.OnPropertyChanged("Item");

            //SaveHtmlCssFiles();
            //ExtractAllFiles();

            renderHtml();
        }

        void Button_Prev_Clicked(System.Object sender, System.EventArgs e)
        {
            var pageName = (this.ContentWebView.Source as UrlWebViewSource).Url.Split('/').LastOrDefault();
            var pageNumber = epubBook.Content.Html.IndexOf(h => h.Key == pageName) - 1;
            if (pageNumber >= 0)
            {
                index = pageNumber;
                renderHtml();
            }

        }
        void Button_Next_Clicked(System.Object sender, System.EventArgs e)
        {
            var pageName = (this.ContentWebView.Source as UrlWebViewSource).Url.Split('/').LastOrDefault();
            var pageNumber = epubBook.Content.Html.IndexOf(h => h.Key == pageName) + 1;
            if (pageNumber < epubBook?.Content.Html.Count)
            {
                index = pageNumber;
                renderHtml();
            }
        }
        async void Button_Start_Clicked(System.Object sender, System.EventArgs e)
        {
            index = 0;
            renderHtml();
            this.UserAgent.Text = await this.ContentWebView.EvaluateJavaScriptAsync("navigator.userAgent;");// + "||" + await this.ContentWebView.EvaluateJavaScriptAsync("navigator.appName;") + "||" + await this.ContentWebView.EvaluateJavaScriptAsync("navigator.appVersion;");
        }
        void Button_End_Clicked(System.Object sender, System.EventArgs e)
        {

            index = epubBook?.Content.Html.Count - 1 ?? 0;
            renderHtml();
        }

        private void renderHtml()
        {
            if (epubBook == null) return;

            var url = $"{baseUrl}{epubBook.Content.Html.ElementAt(index).Key}";
            this.ContentWebView.Source = url;

            

            /*
            var webviewSource = new HtmlWebViewSource();
            var html = epubBook.Content.Html.ElementAt(index).Value.Content;
            string css = "";
            foreach(var localCss in epubBook.Content.Css)
            {
                css += localCss.Value.Content;
            }
            html.Replace("<head>", $"<head><style>{css}</style>");
            webviewSource.Html = html;
            webviewSource.BaseUrl = baseUrl;
            this.ContentWebView.Source = webviewSource;*/
        }

        private void SaveImages()
        {
            foreach (var image in epubBook.Content.Images)
            {
                try
                {
                    var fileName = image.Value.FileName;
                    
                    var fullPath = "";
                    if (Device.RuntimePlatform == "UWP")
                    {
                        fileName = "book\\" + fileName.Replace("/", "\\"); ;
                        fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
                    }
                    else 
                    {
                        fullPath = Path.Combine(baseUrl, fileName);

                    }
                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }
                    File.WriteAllBytes(fullPath, image.Value.Content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void SaveHtmlCssFiles()
        {
            string css = "";
            foreach (var localCss in epubBook.Content.Css)
            {
                css += localCss.Value.Content;
            }
            

            foreach (var file in epubBook.Content.Html)
            {
                var fileName = file.Key;
                
                var fullPath = "";
                if (Device.RuntimePlatform == "UWP")
                {
                    fileName =   "book\\" + fileName.Replace("/", "\\");
                    fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
                }
                else
                {
                    fullPath = Path.Combine(baseUrl, fileName);
                }
                if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }
                var html = file.Value.Content;
                html = html.Replace("<head>", $"<head><style>{css}</style>");
                File.WriteAllText(fullPath,html);
            }
        }

        
    }
}