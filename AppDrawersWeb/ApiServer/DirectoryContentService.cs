using AppDrawers.JavaScriptMessages;
using AppDrawers.Shortcuts;
using System;
using System.Collections.Generic;
using System.IO;

namespace AppDrawers.ApiServer
{
    public class DirectoryContentService
    {
        public static int MaxDepth = 10;

        public static MessageWrapper GetContent(string directory)
        {
            return (new MessageWrapper()
            {
                Type = "DirectoryContents",
                Data = CollectItems(directory),
            });
        }

        private static void CollectFiles(List<ContentItem> contents, string directory)
        {
            var validFileExtensions = ItemTypes.GetExtensionMatchRegEx();

            foreach (var filename in Directory.GetFiles(directory))
            {
                var fileInfo = new FileInfo(filename);

                if (!validFileExtensions.IsMatch(fileInfo.Extension))
                {
                    continue;
                }

                var itemType = ItemTypes.GetValueByExtension(fileInfo.Extension);

                var fileItem = new ContentItem()
                {
                    Text = Path.GetFileNameWithoutExtension(fileInfo.Name),
                    Type = itemType,
                    Target = fileInfo.FullName,
                };

                string iconFilePath = itemType.DefaultIcon;
                int iconIndex = itemType.DefaultIconIndex;

                if (itemType == ItemTypes.Shortcut)
                {
                    var shortcutInfo = new ShortcutInfo(filename);

                    fileItem.Target = shortcutInfo;

                    iconFilePath = shortcutInfo.IconFilePath;
                    iconIndex = shortcutInfo.IconIndex;
                }

                fileItem.ImageUri = ImageCaching.GetImage(iconFilePath, iconIndex)?.ConvertToDataUri();

                contents.Add(fileItem);
            }
        }

        private static List<ContentItem> CollectItems(string directory)
        {
            var contents = new List<ContentItem>();

            CollectSubdirectories(contents, directory);

            CollectFiles(contents, directory);

            return contents;
        }

        private static void CollectSubdirectories(List<ContentItem> contents, string directory)
        {
            try
            {
                foreach (var subdirectory in Directory.GetDirectories(directory))
                {
                    var dirInfo = new DirectoryInfo(subdirectory);

                    if (dirInfo.Name.StartsWith(".") || dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        continue;
                    }

                    var directoryItem = new ContentItem()
                    {
                        Text = dirInfo.Name,
                        Type = ItemTypes.Directory,
                        Target = dirInfo.FullName,

                        ImageUri = ImageCaching.GetImage(
                            ItemTypes.Directory.DefaultIcon,
                            ItemTypes.Directory.DefaultIconIndex)?.ConvertToDataUri(),
                    };

                    contents.Add(directoryItem);
                }
            }
            catch (Exception ex)
            {
                // Ignore
            }
        }
    }
}