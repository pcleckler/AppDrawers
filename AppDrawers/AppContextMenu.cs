using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace AppDrawers
{
    public class AppContextMenu : ContextMenuStrip
    {
        public AppContextMenu() : base()
        {
            AddMenuItem(new ToolStripMenuItem() { Text = "About" }).Click += cmsAbout;
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
    }
}