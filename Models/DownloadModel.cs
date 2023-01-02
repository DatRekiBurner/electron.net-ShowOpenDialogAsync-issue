using System;
using System.Collections.Generic;

namespace Downloader.Models
{
    public class DownloadModel
    {
        public Uri? Url { get; set; }
        public string Path { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
        public string SelectHttpMethod { get; set; } = string.Empty;
    }
}
