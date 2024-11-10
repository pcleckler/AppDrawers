using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace AppDrawers.GUI
{
    public class AppContextMenu : ContextMenuStrip
    {
        public AppContextMenu() : base()
        {
            AddMenuItem(new ToolStripMenuItem() { Text = "About" }).Click += cmsAbout;
            AddMenuItem(new ToolStripSeparator());
            AddMenuItem(new ToolStripMenuItem() { Text = "Refresh Content" }).Click += cmsRefreshContent;
            AddMenuItem(new ToolStripMenuItem() { Text = "Open Developer Tools" }).Click += cmsOpenDevTools;
            AddMenuItem(new ToolStripSeparator());
            AddMenuItem(new ToolStripMenuItem() { Text = "Exit" }).Click += cmsExit;
        }

        private ToolStripItem AddMenuItem(ToolStripItem menuItem)
        {
            this.Items.Add(menuItem);

            return menuItem;
        }

        private void cmsAbout(object sender, EventArgs e)
        {
            var appName = nameof(AppDrawers);

            MessageBox.Show(
                $"{appName}\n\n{Encoding.UTF8.GetString(Properties.Resources.LICENSE)}",
                nameof(AppDrawers),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void cmsExit(object sender, EventArgs e)
        {
            try
            {
                if (this.Tag is NotifyIcon notifyIcon)
                {
                    notifyIcon.Visible = false;
                }

                Application.Exit();

                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to terminate the application.\n\n{ex.GetExceptionMessageTree()}", nameof(AppDrawers), MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void cmsOpenDevTools(object sender, EventArgs e)
        {
            if (this.Tag is NotifyIcon notifyIcon && notifyIcon.Tag is WebForm webForm)
            {
                webForm.OpenDevToolsWindow();
            }
        }

        private void cmsRefreshContent(object sender, EventArgs e)
        {
            if (this.Tag is NotifyIcon notifyIcon && notifyIcon.Tag is WebForm webForm)
            {
                webForm.RefreshWebView();
            }
        }
    }
}