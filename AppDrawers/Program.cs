using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppDrawers
{
    internal class Program
    {
        private const string appName = nameof(AppDrawers);

        private static int port = 9393;

        // Define a Tuple with contentType and buffer set to null as a simple acknowledgement response
        private static (string, byte[]) SimpleAck = (null, null);

        private static Uri uriArg = null;

        private static bool EvaluateArguments(string[] args)
        {
            var state = string.Empty;

            foreach (var arg in args)
            {
                if (Uri.TryCreate(arg, UriKind.Absolute, out var tempUri))
                {
                    uriArg = tempUri;
                }
                else if (arg.Equals("/port", StringComparison.OrdinalIgnoreCase))
                {
                    state = "port";
                }
                else if (arg.Equals("/?") || arg.Equals("/help", StringComparison.OrdinalIgnoreCase))
                {
                    Usage();
                    return false;
                }
                else
                {
                    if (state == "port" && int.TryParse(arg, out int tempPort))
                    {
                        port = tempPort;
                        state = string.Empty;
                    }
                }
            }

            return true;
        }

        [STAThread]
        private static async Task Main(string[] args)
        {
            try
            {
                // Handle the command line
                if (!EvaluateArguments(args))
                {
                    Usage();
                    return;
                }

                if (uriArg != null)
                {
                    await RequestDirectoryAsync();

                    return;
                }

                // Create a NotifyIcon
                StaThread.Start(() =>
                {
                    var notifyIcon = new NotifyIcon()
                    {
                        Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                        Text = appName,
                        Visible = true,
                        ContextMenuStrip = new AppContextMenu(),
                    };

                    notifyIcon.ContextMenuStrip.Tag = notifyIcon;

                    // Process Application Events
                    Application.Run();
                });

                // Initialize AppDrawer
                AppDrawer.Initialize();

                // Begin waiting for display requests
                (new HttpServer(

                    new List<string>()
                    {
                        $"http://localhost:{port}/{appName}/"
                    },
                    new Dictionary<Regex, HttpServer.ResponseHandler>()
                    {
                        { new Regex("^.*/directory.*$"), ShowDirectory },
                    },
                    new Logger($"{appName}-{port}-{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log")

                )).Start();

                // Have the current thread wait while events are processed
                while (true)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Main program error. Terminating.\n\n{ex.GetExceptionMessageTree()}", appName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private static async Task RequestDirectoryAsync()
        {
            try
            {
                HttpClient client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                });

                var response = await client.GetAsync(uriArg);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed. The server returned a '{response.StatusCode}' response (status code {(int)response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while attempting to request the specified directory.\n\n{ex.GetExceptionMessageTree()}\n\n Is the {appName} service running?", appName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private static (string, byte[]) ShowDirectory(HttpListenerContext context)
        {
            var directory = context.Request.QueryString["dir"];

            var clipping = context.Request.QueryString["clipping"];

            AppDrawer.ShowMenu(directory, clipping);

            return SimpleAck;
        }

        private static void Usage()
        {
            MessageBox.Show($"Usage: {appName} [/port <port>]\n\nwhere <port> is the HTTP port that the program should listen on.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}