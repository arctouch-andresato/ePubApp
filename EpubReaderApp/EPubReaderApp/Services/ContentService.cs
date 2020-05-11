using Contentful.Core;
using Contentful.Core.Configuration;
using EPubReaderApp.Models;
using MonkeyCache.SQLite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VersOne.Epub;
using Xamarin.Forms.Internals;

namespace EPubReaderApp.Services
{
    class ContentService : IContentService
    {
        private const string EntriesKeyListId = "itemEntriesKeysList";
        private const string AssetsKeyListId = "itemAssetsKeysList";
        private const string locale = "en-US";
        private readonly TimeSpan ExpirationSpan = TimeSpan.FromDays(3);
        private List<string> itemEntriesKeysList;
        private List<string> itemAssetsKeysList;
        private string nextSyncUrl;
        public ContentfulClient Client { get; private set; }

        public void Initialize()
        {
            //Barrel.Current.EmptyAll();
            var httpClient = new HttpClient();
            var options = new ContentfulOptions()
            {
                DeliveryApiKey = "i1sZI3TtJdmSCExSb99A-7mKPOAdUO7YwfIe2sFLMmI",
                PreviewApiKey = "_LEWxIyIa2o7JBhdhKXyVkERhqDpqj18oHUiTmmswu8",
                SpaceId = "1eifs2l9mf3m",
                UsePreviewApi = true
            };
            Client = new ContentfulClient(httpClient, options);

            itemEntriesKeysList = Barrel.Current.Get<List<string>>(EntriesKeyListId) ?? new List<string>();
            itemAssetsKeysList = Barrel.Current.Get<List<string>>(AssetsKeyListId) ?? new List<string>();

            nextSyncUrl = Barrel.Current.Get<string>(nameof(nextSyncUrl));
        }

        public async Task SyncContentAsync()
        {
            var httpClient = new HttpClient();
            var barrel = Barrel.Current;
            var res = string.IsNullOrWhiteSpace(nextSyncUrl) ? await Client.SyncInitial() : await Client.SyncNextResult(nextSyncUrl);
            nextSyncUrl = res.NextSyncUrl;
            barrel.Add(nameof(nextSyncUrl), nextSyncUrl, ExpirationSpan);
            var hasMore = true;
            while (hasMore)
            {
                foreach (var entry in res.Entries)
                {
                    //if (entry.SystemProperties.con)
                    var jObject = entry.Fields as JObject;
                    if (ValidateEntry(jObject))
                    {
                        var newItem = new Item
                        {
                            Name = jObject["Name"][locale].ToString(),
                            Text = jObject["Description"][locale]?.ToString(),
                            AssetId = jObject["File"][locale]["sys"]["id"]?.ToString()
                        };
                        if (!itemEntriesKeysList.Contains(newItem.Name))
                        {
                            itemEntriesKeysList.Add(newItem.Name);
                        }
                        System.Diagnostics.Debug.WriteLine($"Sync Entries [{newItem.Name}]");
                        barrel.Add<Item>(newItem.Name, newItem, ExpirationSpan);
                        barrel.Add<List<string>>(EntriesKeyListId, itemEntriesKeysList, ExpirationSpan);
                    }
                }

                foreach (var entry in res.Assets)
                {
                    var jObject = entry.Fields as JObject;
                    var newItem = new EpubAsset
                    {
                        FileName = jObject["file"][locale]["fileName"].ToString(),
                        Title = jObject["title"][locale].ToString(),
                        Url = jObject["file"][locale]["url"].ToString()
                    };

                    newItem.Id = entry.SystemProperties.Id;// newItem.Url.Split('/')[4];
                    //var asset = await Client.GetAsset(newItem.Id);
                    System.Diagnostics.Debug.WriteLine($"Sync Assets [{newItem.FileName}]");

                    var response = await httpClient.GetAsync("https:" + newItem.Url);
                    var fullPath = Path.Combine(BookEnvironment.GetLocalPathForAssets(), newItem.FileName);
                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }

                    File.WriteAllBytes(fullPath, await response.Content.ReadAsByteArrayAsync());
                    newItem.Path = fullPath;
                    if (!itemAssetsKeysList.Contains(newItem.Id))
                    {
                        itemAssetsKeysList.Add(newItem.Id);
                    }

                    barrel.Add<EpubAsset>(newItem.Id, newItem, ExpirationSpan);
                    barrel.Add<List<string>>(AssetsKeyListId, itemAssetsKeysList, ExpirationSpan);
                }

                var nextUrl = res.NextPageUrl;
                if (string.IsNullOrEmpty(nextUrl))
                {
                    hasMore = false;
                }
                else
                {
                    res = await Client.SyncNextResult(nextUrl);
                }
            }
        }

        public IList<Item> GetAllContentEntries()
        {
            var result = new List<Item>();
            foreach(var key in itemEntriesKeysList)
            {
                result.Add(Barrel.Current.Get<Item>(key));
            }
            return result;
        }

        public IList<EpubAsset> GetAllContentAssets()
        {
            var result = new List<EpubAsset>();
            foreach (var key in itemAssetsKeysList)
            {
                result.Add(Barrel.Current.Get<EpubAsset>(key));
            }
            return result;
        }

        //public void AddOrUpdate<T>(this IBarrel barrel, string key, T value, TimeSpan timeSpan)
        //{
        //    if (barrel.Exists(key))
        //        barrel.Empty(key);
        //    barrel.Add<T>(key, value, timeSpan);
        //}

        public EpubBook ExtractEpubFromId(string assetId)
        {
            var asset = Barrel.Current.Get<EpubAsset>(assetId);
            var epubBook = EpubReader.ReadBook(asset.Path);

            var tempFolder = BookEnvironment.GetTempLocalPathForEpub();
            
            ExtractAllFilesFromEpub(epubBook, tempFolder);

            return epubBook;
        }

        public void ExtractAllFilesFromEpub(EpubBook epubBook, string folder)
        {
            if (!Directory.Exists(Path.GetDirectoryName(folder)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(folder));
            }
            else
            {
                Directory.Delete(Path.GetDirectoryName(folder), true);
                Directory.CreateDirectory(Path.GetDirectoryName(folder));
            }
            foreach (var file in epubBook.Content.AllFiles)
            {
                var fileName = file.Key;
                var fullPath = BookEnvironment.CombinePath(folder, fileName);

                if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }

                switch (file.Value)
                {
                    case EpubTextContentFile text:
                        File.WriteAllText(fullPath, text.Content);
                        break;
                    case EpubByteContentFile binFile:
                        File.WriteAllBytes(fullPath, binFile.Content);
                        break;
                }

            }
        }

        private bool ValidateEntry(JObject entry)
        {
            return entry.ContainsKey("Name") && entry.ContainsKey("Description") && entry.ContainsKey("File");
        }
    }
}
