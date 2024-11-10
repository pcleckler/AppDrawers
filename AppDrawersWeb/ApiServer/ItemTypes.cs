using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AppDrawers.ApiServer
{
    public class ItemTypes
    {
        public static readonly ItemTypes CaptureClip = new ItemTypes("CaptureClip", string.Empty, string.Empty, 0);

        public static readonly ItemTypes Clip = new ItemTypes(
            "Clip",
            ".Clip",
            @"%SystemRoot%\System32\SHELL32.dll",
            260);

        public static readonly ItemTypes DateFormat = new ItemTypes(
            "DateFormat",
            ".DateFormat",
            @"%SystemRoot%\System32\SHELL32.dll",
            167);

        public static readonly ItemTypes Directory = new ItemTypes(
            "Directory",
            string.Empty,
            @"%SystemRoot%\System32\SHELL32.dll",
            3);

        public static readonly ItemTypes NewFolder = new ItemTypes("NewFolder", string.Empty, string.Empty, 0);
        public static readonly ItemTypes NewShortcut = new ItemTypes("NewShortcut", string.Empty, string.Empty, 0);
        public static readonly ItemTypes OpenFolder = new ItemTypes("OpenFolder", string.Empty, string.Empty, 0);
        public static readonly ItemTypes Shortcut = new ItemTypes("Shortcut", ".lnk", string.Empty, 0);

        private ItemTypes(string type, string extension, string defaultIcon, int defaultIconIndex)
        {
            this.Type = type;
            this.Extension = extension;
            this.DefaultIcon = defaultIcon;
            this.DefaultIconIndex = defaultIconIndex;
        }

        public static List<ItemTypes> Values { get; } = new List<ItemTypes>()
        {
            NewFolder,
            OpenFolder,
            Directory,
            Shortcut,
            Clip,
            DateFormat,
            CaptureClip,
            NewShortcut
        };

        public string DefaultIcon { get; } = string.Empty;
        public int DefaultIconIndex { get; } = 0;
        public string Extension { get; } = string.Empty;
        public string Type { get; } = string.Empty;

        public static Regex GetExtensionMatchRegEx()
        {
            var sb = new StringBuilder();

            foreach (var type in Values)
            {
                if (type.Extension.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("|");
                    }

                    sb.Append($@"^.*\{type.Extension}$");
                }
            }

            return new Regex(sb.ToString());
        }

        public static ItemTypes GetValueByExtension(string extension)
        {
            foreach (var type in Values)
            {
                if (type.Extension.Equals(extension, System.StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }

            return null;
        }
    }
}