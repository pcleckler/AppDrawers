using AppDrawers.ApiServer;
using System;

namespace AppDrawers.JavaScriptMessages
{
    public class ContentItem
    {
        public Uri ImageUri { get; set; } = null;
        public object Target { get; set; } = null;
        public string Text { get; set; } = string.Empty;
        public ItemTypes Type { get; set; } = null;
    }
}