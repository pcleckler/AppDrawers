using System;

namespace AppDrawers.ApiServer
{
    public class UriIndex
    {
        public const string ChangeDirectoryCommand = "changeDirectory";
        public const string DisplayMenuCommand = "displayMenu";
        public const string GetDirectoryContentsCommand = "getDirectoryContents";
        public const string HideMenuCommand = "hideMenu";

        public UriIndex(string listenPrefix)
        {
            this.ListenPrefix = listenPrefix;

            this.DefaultDocument = new Uri($"{this.ListenPrefix}index.html");
        }

        public Uri DefaultDocument { get; private set; } = null;
        public string ListenPrefix { get; private set; } = string.Empty;
    }
}