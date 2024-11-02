using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AppDrawers
{
    internal static class Extensions
    {
        private const int FO_DELETE = 3;
        private const int FOF_ALLOWUNDO = 0x40; // Send to the Recycle Bin
        private const int FOF_NOCONFIRMATION = 0x10; // Don't prompt the user

        private static Dictionary<string, string> EnvVariableCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, Image> ImageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static ToolStripItem AssignImage(this ToolStripItem tsi, string iconFilePath, int iconIndex)
        {
            try
            {
                if (iconFilePath.Length < 1)
                {
                    return tsi;
                }

                var imageCacheKey = $"{iconFilePath}:{iconIndex}";

                if (ImageCache.ContainsKey(imageCacheKey))
                {
                    tsi.Image = ImageCache[imageCacheKey];
                }
                else
                {
                    var icon = IconExtractor.GetIcon(iconFilePath.ExpandEnvironmentVariables(), iconIndex);

                    if (icon != null)
                    {
                        tsi.Image = icon.ToBitmap();

                        ImageCache.Add(imageCacheKey, tsi.Image);
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return tsi;
        }

        public static string ExpandEnvironmentVariables(this string value)
        {
            // Cache Environment variables
            if (EnvVariableCache.Count < 1)
            {
                var envDict = Environment.GetEnvironmentVariables();

                foreach (string variableName in envDict.Keys)
                {
                    if (!EnvVariableCache.ContainsKey(variableName))
                    {
                        EnvVariableCache.Add(variableName, (string)envDict[variableName]);
                    }
                }
            }

            // Extract variables names from the string and substitute environment variable contents if available
            var sb = new StringBuilder();

            var inVariable = false;

            var token = new StringBuilder();

            foreach (var c in value.ToCharArray())
            {
                if (c == '%')
                {
                    if (inVariable)
                    {
                        var variableName = token.ToString();

                        if (EnvVariableCache.ContainsKey(variableName))
                        {
                            sb.Append(EnvVariableCache[variableName]);
                        }
                        else
                        {
                            sb.Append($"%{variableName}%");
                        }

                        token.Clear();

                        inVariable = false;
                    }
                    else
                    {
                        sb.Append(token);

                        token.Clear();

                        inVariable = true;
                    }
                }
                else
                {
                    token.Append(c);
                }
            }

            if (inVariable)
            {
                sb.Append("%");
            }

            sb.Append(token);

            return sb.ToString();
        }

        public static void Recycle(this string filename, bool promptUser = false)
        {
            var fileInfo = new FileInfo(filename);

            fileInfo.Recycle(promptUser);
        }

        public static void Recycle(this FileInfo fileInfo, bool promptUser = false)
        {
            if (fileInfo.Exists)
            {
                short flags = FOF_ALLOWUNDO;

                if (!promptUser)
                {
                    flags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
                }

                SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
                {
                    wFunc = FO_DELETE,
                    pFrom = fileInfo.FullName + '\0' + '\0', // Double-null termination
                    fFlags = flags
                };

                SHFileOperation(ref fileOp);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }
    }
}