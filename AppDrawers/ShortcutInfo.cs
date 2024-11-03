using Lnk.ShellItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AppDrawers
{
    public class ShortcutInfo
    {
        public ShortcutInfo(string filename)
        {
            const string appRootGuid = "9f4c2855-9f79-4b39-a8d0-e1d42de1d5f3";
            const string assetGuid = "86d40b4d-9069-443c-819a-2a54090dccec";

            var link = Lnk.Lnk.LoadFile(filename);

            var targetPath = System.IO.Path.Combine(link.LocalPath ?? string.Empty, link.CommonPath ?? string.Empty);

            var iconFilePath = link.IconLocation ?? string.Empty;
            var iconIndex = 0;

            // If not given any image information, dig into the link for details
            if (targetPath.Length < 1 && iconFilePath.Length < 1)
            {
                var appRoot = string.Empty;
                var appPictures = new List<string>();

                bool isProgramFile(string value)
                {
                    return System.IO.Path.GetExtension(value).Equals(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(value.ExpandEnvironmentVariables());
                }

                foreach (var targetId in link.TargetIDs)
                {
                    if (targetId is ShellBag0X00 sb0)
                    {
                        foreach (var sheet in sb0.PropertyStore.Sheets)
                        {
                            if (sheet.GUID.ToString().Equals(appRootGuid, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var pName in sheet.PropertyNames.Keys)
                                {
                                    var pValue = sheet.PropertyNames[pName];

                                    if (Directory.Exists(pValue))
                                    {
                                        appRoot = pValue;
                                    }
                                    else if (isProgramFile(pValue))
                                    {
                                        iconFilePath = pValue;
                                    }
                                }
                            }
                            else if (sheet.GUID.ToString().Equals(assetGuid, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var pName in sheet.PropertyNames.Keys)
                                {
                                    var partialFilename = sheet.PropertyNames[pName];

                                    var extension = System.IO.Path.GetExtension(partialFilename);

                                    if (extension.Length > 0)
                                    {
                                        appPictures.Add(partialFilename);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var pName in sheet.PropertyNames.Keys)
                                {
                                    var pValue = sheet.PropertyNames[pName];

                                    if (isProgramFile(pValue))
                                    {
                                        iconFilePath = pValue;
                                    }
                                }
                            }
                        }
                    }
                }

                // If no image has been located, but there are executables in the appRoot, use the first one found.
                if (iconFilePath.Length < 1 && appRoot.Length > 0)
                {
                    foreach (var programFilename in Directory.GetFiles(appRoot, "*.exe"))
                    {
                        iconFilePath = programFilename;
                        break;
                    }
                }

                // If no image has been located, but there an application manifest exists, attempt to locate a logo element.
                if (iconFilePath.Length < 1 && appRoot.Length > 0)
                {
                    var appManifest = System.IO.Path.Combine(appRoot, "AppxManifest.xml");

                    if (File.Exists(appManifest))
                    {
                        var manifest = new XmlDocument();

                        manifest.Load(appManifest);

                        var logoFilename = manifest.GetElementsByTagName("Logo")?[0].InnerText ?? string.Empty;

                        if (logoFilename.Length > 0)
                        {
                            logoFilename = System.IO.Path.Combine(appRoot, logoFilename);
                        }

                        if (File.Exists(logoFilename))
                        {
                            iconFilePath = logoFilename;
                        }
                    }
                }

                // If the application install directory (appRoot) was located and a set of picture files retrieved, pick an image to display
                if (iconFilePath.Length < 1 && appRoot.Length > 0 && appPictures.Count > 0)
                {
                    foreach (var appPicture in appPictures)
                    {
                        var appPictureDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(appRoot, appPicture));
                        var appPictureBaseName = System.IO.Path.GetFileNameWithoutExtension(appPicture).ToLower();
                        var appPictureExtension = System.IO.Path.GetExtension(appPicture).ToLower();
                        var appPictureFullName = System.IO.Path.Combine(appRoot, appPicture);

                        if (File.Exists(appPictureFullName))
                        {
                            iconFilePath = appPictureFullName;
                            break;
                        }
                        else
                        {
                            foreach (var imageFilename in Directory.GetFiles(appPictureDirectory))
                            {
                                var imageBaseName = System.IO.Path.GetFileNameWithoutExtension(imageFilename).ToLower();
                                var imageExtension = System.IO.Path.GetExtension(imageFilename).ToLower();

                                if (imageBaseName.StartsWith(appPictureBaseName) && imageExtension.Equals(appPictureExtension))
                                {
                                    iconFilePath = System.IO.Path.Combine(appRoot, imageFilename);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else // Use traditional shortcut icon info
            {
                var parts = iconFilePath.Split(',');

                iconFilePath = parts[0];

                if (iconFilePath.Length < 1)
                {
                    iconFilePath = targetPath;
                }

                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out int tempIconIndex);

                    iconIndex = tempIconIndex;
                }
                else
                {
                    iconIndex = link.Header.IconIndex;
                }
            }

            this.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
            this.Path = targetPath ?? string.Empty;
            this.IconFilePath = iconFilePath ?? string.Empty;
            this.IconIndex = iconIndex;
            this.WorkingDirectory = link.WorkingDirectory ?? string.Empty;
            this.Comments = link.Name ?? string.Empty;
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