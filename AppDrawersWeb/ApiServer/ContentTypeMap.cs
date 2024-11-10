using System;
using System.Collections.Generic;

namespace AppDrawers.ApiServer
{
    public static class ContentTypeMap
    {
        private static Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".json", ContentTypes.Json },
            { ".js", ContentTypes.JavaScript },
            { ".mjs", ContentTypes.JavaScript },
            { ".html", ContentTypes.Html },
            { ".css", ContentTypes.Css },
            { ".txt", ContentTypes.Text },
            { ".ico", ContentTypes.Icon },
            { ".png", ContentTypes.Png },
            { ".jpg", ContentTypes.Jpg },
            { ".webp", ContentTypes.Webp },
            { ".svg", ContentTypes.Svg },
            { ".pdf", ContentTypes.Pdf },
        };

        public static string GetContentType(string extension)
        {
            if (!extension.StartsWith("."))
            {
                extension = $".{extension}";
            }

            if (map.ContainsKey(extension))
            {
                return map[extension];
            }

            return ContentTypes.Binary;
        }
    }
}