using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AppDrawers
{
    public class MenuItemTargetTypes
    {
        public static readonly MenuItemTargetTypes CaptureClip = new MenuItemTargetTypes("CaptureClip", string.Empty, string.Empty, 0);

        public static readonly MenuItemTargetTypes Clip = new MenuItemTargetTypes(
            "Clip",
            ".Clip",
            @"%SystemRoot%\System32\SHELL32.dll",
            260);

        public static readonly MenuItemTargetTypes DateFormat = new MenuItemTargetTypes(
            "DateFormat",
            ".DateFormat",
            @"%SystemRoot%\System32\SHELL32.dll",
            167);

        public static readonly MenuItemTargetTypes Directory = new MenuItemTargetTypes(
            "Directory",
            string.Empty,
            @"%SystemRoot%\System32\SHELL32.dll",
            3);

        public static readonly MenuItemTargetTypes NewFolder = new MenuItemTargetTypes("NewFolder", string.Empty, string.Empty, 0);
        public static readonly MenuItemTargetTypes NewShortcut = new MenuItemTargetTypes("NewShortcut", string.Empty, string.Empty, 0);
        public static readonly MenuItemTargetTypes OpenFolder = new MenuItemTargetTypes("OpenFolder", string.Empty, string.Empty, 0);
        public static readonly MenuItemTargetTypes Shortcut = new MenuItemTargetTypes("Shortcut", ".lnk", string.Empty, 0);

        private MenuItemTargetTypes(string type, string extension, string defaultIcon, int defaultIconIndex)
        {
            this.Type = type;
            this.Extension = extension;
            this.DefaultIcon = defaultIcon;
            this.DefaultIconIndex = defaultIconIndex;
        }

        public static List<MenuItemTargetTypes> Values { get; } = new List<MenuItemTargetTypes>()
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

        public static MenuItemTargetTypes GetValueByExtension(string extension)
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