using System;
using System.Linq;
using EPubReaderApp.Models;
using EPubReaderApp.Services;
using Xamarin.Forms;

namespace EPubReaderApp.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        private IContentService contentService = DependencyService.Get<IContentService>();
        public Item Item { get; set; }

        public string Source { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;

            var book = contentService.ExtractEpubFromId(item.AssetId);

            Source = BookEnvironment.AddSourceTempPath(book.Content.Html.First().Key);
        }
    }
}
