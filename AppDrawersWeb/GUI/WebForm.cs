using AppDrawers.ApiServer;
using AppDrawers.JavaScriptMessages;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AppDrawers.GUI
{
    public partial class WebForm : Form
    {
        private bool closeRequested = false;
        private bool firstOpen = true;
        private UriIndex uriIndex;

        public WebForm(UriIndex uriIndex)
        {
            InitializeComponent();

            this.uriIndex = uriIndex;

            this.BackColor = Color.Red; // Only color that appears to support this behavior?
            this.webView.BackColor = this.BackColor;
            this.webView.DefaultBackgroundColor = Color.Transparent;

            this.TransparencyKey = this.BackColor;

            this.FormBorderStyle = FormBorderStyle.None;

            var notifyIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text = $"{Constants.AppName} - {this.uriIndex.ListenPrefix}", //port != Constants.DefaultPort ? $"{Constants.AppName}:{port}" : "",
                Visible = true,
                ContextMenuStrip = new AppContextMenu(),
                Tag = this,
            };

            notifyIcon.ContextMenuStrip.Tag = notifyIcon;
        }

        public async void ChangeDirectory(string directory, string clipping)
        {
            var clientPoint = this.PointToClient(Cursor.Position);

            var message = new MessageWrapper()
            {
                Type = nameof(DirectoryChanged),
                Data = new DirectoryChanged()
                {
                    Directory = directory,
                    Clipping = clipping == "true",
                    CursorX = clientPoint.X + 20,
                    CursorY = clientPoint.Y + 20,
                }
            };

            var javaScript = $"{Constants.ProcessNativeMessageFunction}({message.ToJson()})";

            await webView.CoreWebView2.ExecuteScriptAsync(javaScript);
        }

        public void Dismiss()
        {
            this.WindowState = FormWindowState.Normal;

            this.Hide();
        }

        public void DisplayMenu()
        {
            var workingArea = Screen.GetWorkingArea(Cursor.Position);

            this.Left = workingArea.Left;
            this.Width = workingArea.Width;
            this.Top = workingArea.Top;
            this.Height = workingArea.Height;

            this.WindowState = FormWindowState.Maximized;

            this.Show();
            this.Activate();
        }

        public void HideMenu()
        {
            this.Hide();
        }

        public void OpenDevToolsWindow()
        {
            this.webView.CoreWebView2.OpenDevToolsWindow();
        }

        public void RefreshWebView()
        {
            this.webView.CoreWebView2.Navigate(this.uriIndex.DefaultDocument.ToString());
        }

        private async void InitializeWebViewAsync()
        {
            await this.webView.EnsureCoreWebView2Async(null);

            this.webView.CoreWebView2.Navigate(this.uriIndex.DefaultDocument.ToString());  // Set your URL here
        }

        private void WebForm_Activated(object sender, EventArgs e)
        {
            if (this.firstOpen)
            {
                this.Dismiss();

                this.firstOpen = false;
            }
        }

        private void WebForm_Deactivate(object sender, EventArgs e)
        {
            this.Dismiss();
        }

        private void WebForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !this.closeRequested;

            if (e.Cancel)
            {
                this.Dismiss();
            }
        }

        private void WebForm_Load(object sender, EventArgs e)
        {
            this.InitializeWebViewAsync();
        }

        private void webView_Click(object sender, EventArgs e)
        {
            this.Dismiss();
        }
    }
}