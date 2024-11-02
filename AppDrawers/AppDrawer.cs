using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppDrawers
{
    public class AppDrawer
    {
        private const string StubMenu = "(None)";

        private static ContextMenuStrip cms = null;

        private static ToolStripMenuItem cmsHeader = null;

        private static Dictionary<MenuItemTargetTypes, Action<ToolStripMenuItem, MenuItemTarget>> menuItemActions = new Dictionary<MenuItemTargetTypes, Action<ToolStripMenuItem, MenuItemTarget>>()
        {
            { MenuItemTargetTypes.OpenFolder, OpenFolder },
            { MenuItemTargetTypes.CaptureClip, CaptureClip },
            { MenuItemTargetTypes.Clip, OpenClip },
            { MenuItemTargetTypes.DateFormat, OpenDateFormat },
            { MenuItemTargetTypes.NewShortcut, CreateNewShortcut },
            { MenuItemTargetTypes.Shortcut, OpenShortcut },
        };

        public static void Initialize()
        {
            cms = new ContextMenuStrip();

            cmsHeader = (ToolStripMenuItem)AddMenuItem(cms, new ToolStripMenuItem()
            {
                Enabled = false
            });

            AddMenuItem(cms, new ToolStripSeparator());
            AddMenuItem(cms, new ToolStripMenuItem() { Text = "Open" }).Click += CmsOpen;
            AddMenuItem(cms, new ToolStripMenuItem() { Text = "Rename" }).Click += CmsRename;
            AddMenuItem(cms, new ToolStripMenuItem() { Text = "Duplicate" }).Click += CmsDuplicate;
            AddMenuItem(cms, new ToolStripSeparator());
            AddMenuItem(cms, new ToolStripMenuItem() { Text = "Delete" }).Click += CmsDelete;
            AddMenuItem(cms, new ToolStripSeparator());
            AddMenuItem(cms, new ToolStripMenuItem() { Text = "Properties" }).Click += cmsProperties;
        }

        public static void ShowMenu(string directory, string clipping)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            var menu = new ContextMenuStrip()
            {
                BackColor = Color.FromArgb(255, 176, 176, 176)
            };

            FillMenu(menu, directory, clipping);

            SetForegroundWindow(new HandleRef(menu, menu.Handle));

            menu.Show(Cursor.Position);

            while (menu.Visible)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }

        private static ToolStripItem AddMenuItem(object targetMenu, ToolStripItem menuItem)
        {
            if (targetMenu is ContextMenuStrip cms)
            {
                cms.Items.Add(menuItem);
            }
            else if (targetMenu is ToolStripMenuItem tsmi)
            {
                tsmi.DropDownItems.Add(menuItem);
            }

            return menuItem;
        }

        private static void CaptureClip(ToolStripMenuItem tsmi, MenuItemTarget mitt)
        {
            if (mitt.Target is DirectoryInfo dirinfo)
            {
                StaThread.Start(() =>
                {
                    string text = Clipboard.GetText();

                    if (text != null && text.Length > 0)
                    {
                        var extension = MenuItemTargetTypes.Clip.Extension;

                        var sfd = new SaveFileDialog()
                        {
                            Title = "Save Clip As...",
                            Filter = $"Clips (*{extension})|*{extension}",
                            DefaultExt = extension.Substring(1),
                            FileName = $"{text}{extension}",
                            InitialDirectory = dirinfo.FullName
                        };

                        var result = sfd.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, text);
                        }
                    }
                });
            }
        }

        private static void CmsDelete(object sender, EventArgs e)
        {
            if (GetCmsFileInfo() is FileInfo fileInfo)
            {
                if (DialogResult.Yes == MessageBox.Show(
                    $"Are you sure you want to delte '{fileInfo.FullName}'?",
                    "Delete Item?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation))
                {
                    fileInfo.Recycle(true);
                }
            }
        }

        private static void CmsDuplicate(object sender, EventArgs e)
        {
            if (GetCmsFileInfo() is FileInfo fileInfo)
            {
                StaThread.Start(() =>
                {
                    var label = fileInfo.Extension.Equals(MenuItemTargetTypes.Shortcut.Extension, StringComparison.OrdinalIgnoreCase) ? "Shortcut" : $"{fileInfo.Extension.Substring(1).ToUpper()} File";

                    var itemBaseFilename = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                    var duplicateFilename = Path.Combine(fileInfo.DirectoryName, $"{itemBaseFilename} (2){fileInfo.Extension}");

                    for (int i = 3; i < 100; i++)
                    {
                        if (!File.Exists(duplicateFilename))
                        {
                            break;
                        }

                        duplicateFilename = Path.Combine(fileInfo.DirectoryName, $"{itemBaseFilename} ({i}){fileInfo.Extension}");
                    }

                    var sfd = new SaveFileDialog()
                    {
                        Title = $"Save Duplicate Of '{itemBaseFilename}' As...",
                        Filter = $"{label} (*{fileInfo.Extension}|*{fileInfo.Extension}",
                        FileName = Path.GetFileName(duplicateFilename),
                        DefaultExt = fileInfo.Extension.Substring(1),
                        InitialDirectory = fileInfo.DirectoryName,
                    };

                    var result = sfd.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            File.Copy(fileInfo.FullName, sfd.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Failed to duplicate fiel.\n\n{ex.Message}",
                                "Duplicate",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                        }
                    }
                });
            }
        }

        private static void CmsOpen(object sender, EventArgs e)
        {
            if (cms.Tag is MenuItemContextMenuContext context)
            {
                OpenMenuItem(context.ToolStripMenuItem, e);
            }
        }

        private static void cmsProperties(object sender, EventArgs e)
        {
            if (GetCmsFileInfo() is FileInfo fileInfo)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = fileInfo.FullName,
                    Verb = "properties",
                });
            }
        }

        private static void CmsRename(object sender, EventArgs e)
        {
            if (GetCmsFileInfo() is FileInfo fileInfo)
            {
                StaThread.Start(() =>
                {
                    var label = fileInfo.Extension.Equals(MenuItemTargetTypes.Shortcut.Extension, StringComparison.OrdinalIgnoreCase) ? "Shortcut" : $"{fileInfo.Extension.Substring(1).ToUpper()} File";

                    var itemBaseFilename = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                    var sfd = new SaveFileDialog()
                    {
                        Title = $"Rename '{itemBaseFilename}' As...",
                        Filter = $"{label} (*{fileInfo.Extension}|*{fileInfo.Extension}",
                        FileName = itemBaseFilename,
                        DefaultExt = fileInfo.Extension.Substring(1),
                        InitialDirectory = fileInfo.DirectoryName,
                    };

                    var result = sfd.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            File.Move(fileInfo.FullName, sfd.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Failed to rename file.\n\n{ex.Message}",
                                "Rename",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                        }
                    }
                });
            }
        }

        private static void CreateNewShortcut(ToolStripMenuItem tsmi, MenuItemTarget mit)
        {
            var shortcutFilename = string.Empty;

            if (mit.Target is DirectoryInfo dirInfo)
            {
                try
                {
                    shortcutFilename = Path.Combine(dirInfo.FullName, "File.tmp");

                    File.WriteAllText(shortcutFilename, "");

                    var p = Process.Start("rundll32.exe", $"appwiz.cpl,NewLinkHere {shortcutFilename}");

                    while (!p.HasExited)
                    {
                        Application.DoEvents();
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create the shortcut.\n\n{ex.Message}", "Create New Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                finally
                {
                    if (shortcutFilename.Length > 0 && File.Exists(shortcutFilename))
                    {
                        try
                        {
                            File.Delete(shortcutFilename);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to delete the temporary shortcut file '{shortcutFilename}'. This file may need to be deleted manually.\n\n{ex.Message}", "Create New Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
            }
        }

        private static void FillMenu(object menu, string directory, string clipping)
        {
            // Directories
            foreach (var subdirectory in Directory.GetDirectories(directory))
            {
                var dirInfo = new DirectoryInfo(subdirectory);

                if (dirInfo.Name.StartsWith(".") || dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    continue;
                }

                var dirMenuItem = new ToolStripMenuItem()
                {
                    Text = dirInfo.Name,
                    Tag = new MenuItemTarget
                    {
                        Type = MenuItemTargetTypes.Directory,
                        Target = dirInfo,
                    }
                };

                if (dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0)
                {
                    var submenuItem = new ToolStripMenuItem()
                    {
                        Text = StubMenu,
                        Enabled = false,
                    };

                    dirMenuItem.DropDownItems.Add(submenuItem);

                    dirMenuItem.DropDownOpening += (sender, e) =>
                    {
                        if (sender is ToolStripMenuItem tsmi)
                        {
                            if (tsmi.DropDownItems.Count == 1 && tsmi.DropDownItems[0].Text == StubMenu)
                            {
                                tsmi.DropDownItems.Clear();

                                FillMenu(tsmi, dirInfo.FullName, clipping);
                            }
                        }
                    };
                }

                AddMenuItem(menu, dirMenuItem).AssignImage(
                    MenuItemTargetTypes.Directory.DefaultIcon,
                    MenuItemTargetTypes.Directory.DefaultIconIndex);
            }

            // Files
            var validFileExtensions = MenuItemTargetTypes.GetExtensionMatchRegEx();

            foreach (var filename in Directory.GetFiles(directory))
            {
                var fileInfo = new FileInfo(filename);

                if (!validFileExtensions.IsMatch(fileInfo.Extension))
                {
                    continue;
                }

                object target = fileInfo;

                var mitt = MenuItemTargetTypes.GetValueByExtension(fileInfo.Extension);

                string iconFilePath = mitt.DefaultIcon;
                int iconIndex = mitt.DefaultIconIndex;

                if (mitt == MenuItemTargetTypes.Shortcut)
                {
                    var shortcutInfo = new ShortcutInfo(filename);

                    target = shortcutInfo;

                    iconFilePath = shortcutInfo.IconFilePath;
                    iconIndex = shortcutInfo.IconIndex;
                }

                AddMenuItem(menu, new ToolStripMenuItem()
                {
                    Text = Path.GetFileNameWithoutExtension(fileInfo.Name),
                    Tag = new MenuItemTarget()
                    {
                        Type = mitt,
                        Target = target,
                    }
                })
                .AssignImage(iconFilePath, iconIndex)
                .MouseUp += OpenMenuItem;
            }

            // OpenFolder
            AddMenuItem(menu, new ToolStripSeparator());

            AddMenuItem(menu, new ToolStripMenuItem()
            {
                Text = "Open Folder",
                Tag = new MenuItemTarget()
                {
                    Type = MenuItemTargetTypes.OpenFolder,
                    Target = new DirectoryInfo(directory),
                },
            }).Click += OpenMenuItem;

            // Capture Clip
            if (clipping == "true")
            {
                AddMenuItem(menu, new ToolStripMenuItem()
                {
                    Text = "Capture Clip",
                    Tag = new MenuItemTarget()
                    {
                        Type = MenuItemTargetTypes.CaptureClip,
                        Target = new DirectoryInfo(directory),
                    }
                }).Click += OpenMenuItem;
            }
            else
            {
                // New Shortcut
                AddMenuItem(menu, new ToolStripMenuItem()
                {
                    Text = "New Shortcut",
                    Tag = new MenuItemTarget()
                    {
                        Type = MenuItemTargetTypes.NewShortcut,
                        Target = new DirectoryInfo(directory),
                    }
                }).Click += OpenMenuItem;
            }
        }

        private static FileInfo GetCmsFileInfo()
        {
            if (cms.Tag is MenuItemContextMenuContext context)
            {
                if (context.MenuItemTarget.Target is ShortcutInfo shortcutInfo)
                {
                    return new FileInfo(shortcutInfo.ShortcutPath);
                }
                else if (context.MenuItemTarget.Target is FileInfo targetFileInfo)
                {
                    return targetFileInfo;
                }
            }

            throw new Exception("Failed to obtain file inforamtion structure.");
        }

        private static void OpenClip(ToolStripMenuItem tsmi, MenuItemTarget mit)
        {
            if (mit.Target is FileInfo fileInfo)
            {
                StaThread.Start(() =>
                {
                    Clipboard.SetText(File.ReadAllText(fileInfo.FullName));
                });
            }
        }

        private static void OpenDateFormat(ToolStripMenuItem tsmi, MenuItemTarget mit)
        {
            if (mit.Target is FileInfo fileInfo)
            {
                StaThread.Start(() =>
                {
                    Clipboard.SetText(DateTime.Now.ToString(File.ReadAllText(fileInfo.FullName)));
                });
            }
        }

        private static void OpenFolder(ToolStripMenuItem tsmi, MenuItemTarget mit)
        {
            if (mit.Target is DirectoryInfo dirInfo)
            {
                try
                {
                    Process.Start("explorer.exe", dirInfo.FullName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open the folder.\n\n{ex.Message}", "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private static void OpenMenuItem(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem tsmi))
            {
                return;
            }

            if (!(tsmi.Tag is MenuItemTarget mit))
            {
                return;
            }

            if (e is MouseEventArgs me && me.Button == MouseButtons.Right)
            {
                cms.Tag = new MenuItemContextMenuContext()
                {
                    ToolStripMenuItem = tsmi,
                    MenuItemTarget = mit,
                };

                cmsHeader.Text = tsmi.Text;
                cmsHeader.Image = tsmi.Image;

                SetForegroundWindow(new HandleRef(cms, cms.Handle));

                cms.Show(Cursor.Position);

                while (cms.Visible)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }

                return;
            }

            if (menuItemActions.ContainsKey(mit.Type))
            {
                menuItemActions[mit.Type].Invoke(tsmi, mit);
            }
        }

        private static void OpenShortcut(ToolStripMenuItem tsmi, MenuItemTarget mit)
        {
            if (mit.Target is ShortcutInfo shortcutInfo)
            {
                try
                {
                    Process.Start(shortcutInfo.ShortcutPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to execute the shortcut.\n\n{ex.Message}", "Open Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(HandleRef hWnd);
    }
}