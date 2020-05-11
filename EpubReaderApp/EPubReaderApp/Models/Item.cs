using Contentful.Core.Models;
using System;

namespace EPubReaderApp.Models
{
    public class Item
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string AssetId { get; set; }
    }
}