using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace AppDrawers
{
    public class IconExtractor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        public static Icon GetIcon(string filename, int iconIndex)
        {
            var lgIconArray = new IntPtr[1];
            var smIconArray = new IntPtr[1];

            var iconCount = ExtractIconEx(filename, iconIndex, lgIconArray, smIconArray, 1);

            if (iconCount > 0 && smIconArray[0] != IntPtr.Zero)
            {
                var icon = Icon.FromHandle(smIconArray[0]);

                return icon;
            }

            if (File.Exists(filename))
            {
                return Icon.ExtractAssociatedIcon(filename);
            }

            return null;
        }
    }
}