using System;
using System.Collections.Generic;
using System.Text;
using VersOne.Epub;

namespace EPubReaderApp.Models
{
    public class EpubAsset
    {
        public string Id { get; set; }

        public string FileName { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }
    }
}
