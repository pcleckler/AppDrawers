using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppDrawers
{
    internal class Program
    {
        private const string appName = nameof(AppDrawers);

        private static string appUrl;
        private static Logger logger;

        private static int port = 9393;

        // Define a Tuple with contentType and buffer set to null as a simple acknowledgement response
        private static (string, byte[]) SimpleAck = (null, null);

        private static Uri uriArg = null;

        private static void EvaluateArguments(string[] args)
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
                    return;
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
        }

        [STAThread]
        private static async Task Main(string[] args)
        {
            try
            {
                // Initialized
                EvaluateArguments(args);

                appUrl = $"http://localhost:{port}/{appName}/";

                if (uriArg != null)
                {
                    await RequestDirectoryAsync();

                    return;
                }

                logger = new Logger($"{appName}-{port}-{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log");

                AppDrawer.Initialize();

                // Define listening URL prefixes
                var prefixes = new List<string>() { appUrl };

                // Define possible route handlers
                var routes = new Dictionary<Regex, HttpServer.ResponseHandler>()
                {
                    { new Regex("^.*/quit$"), Quit },
                    { new Regex("^.*/check.*$"), context => SimpleAck },
                    { new Regex("^.*/directory.*$"), ShowDirectory },
                };

                var server = new HttpServer(prefixes, routes, logger);

                server.Start();

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

        private static (string, byte[]) Quit(HttpListenerContext context)
        {
            Process.GetCurrentProcess().Kill();

            return SimpleAck;
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

            Quit(null);
        }
    }
}