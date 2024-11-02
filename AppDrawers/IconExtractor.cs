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
                var shInfo = new SHFILEINFO();

                shInfo.szDisplayName = new string((char)0, 260);
                shInfo.szTypeName = new string((char)0, 80);

                SHGetFileInfo(filename, 0, ref shInfo, (uint)Marshal.SizeOf(shInfo), 0x100 | 0x0);

                return Icon.FromHandle(shInfo.hIcon);
            }

            return null;
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        public struct SHFILEINFO
        {
            public int dwAttributes;
            public IntPtr hIcon;
            public int iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
    }
}