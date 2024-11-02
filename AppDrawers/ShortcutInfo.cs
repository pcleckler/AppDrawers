using System;

namespace AppDrawers
{
    public class ShortcutInfo
    {
        public ShortcutInfo(string filename)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");

            dynamic shell = Activator.CreateInstance(shellType);

            var shortcut = shell.CreateShortCut(filename);

            var parts = shortcut.IconLocation.Split(',');

            var iconFilePath = parts[0];

            if (iconFilePath.Length < 1)
            {
                iconFilePath = shortcut.TargetPath;
            }

            this.IconIndex = 0;

            if (parts.Length > 1)
            {
                int.TryParse(parts[1], out int tempIconIndex);

                this.IconIndex = tempIconIndex;
            }

            this.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
            this.Path = shortcut.TargetPath;
            this.IconFilePath = iconFilePath;
            this.WorkingDirectory = shortcut.WorkingDirectory;
            this.Comments = shortcut.Description;
            this.ShortcutPath = filename;
        }

        public string Comments { get; }
        public string IconFilePath { get; }
        public int IconIndex { get; }
        public string Name { get; }
        public string Path { get; }
        public string ShortcutPath { get; }
        public string WorkingDirectory { get; }
    }
}