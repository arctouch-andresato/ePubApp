using Contentful.Core;
using EPubReaderApp.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using VersOne.Epub;

namespace EPubReaderApp.Services
{
    public interface IContentService
    {
        void Initialize();
        ContentfulClient Client { get; }

        Task SyncContentAsync();

        IList<Item> GetAllContentEntries();

        EpubBook ExtractEpubFromId(string assetId);

    }
}