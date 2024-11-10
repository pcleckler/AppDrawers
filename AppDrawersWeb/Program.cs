using AppDrawers.ApiServer;
using AppDrawers.GUI;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace AppDrawers
{
    internal static class Program
    {
        private static ApiService apiServer;

        private static int port = Constants.DefaultPort;

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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // Handle the command line
            if (!EvaluateArguments(args))
            {
                Usage();
                return;
            }

            if (uriArg != null)
            {
                RequestDirectory();

                return;
            }

            // Prep for WinForms
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            // Provide URIs for components
            var uriIndex = new UriIndex($"http://localhost:{port}/{Constants.AppName}/");

            var webForm = new WebForm(uriIndex);

            // Start an ApiServer
            apiServer = new ApiService(uriIndex, webForm);

            // Open WebForm
            Application.Run(webForm);
        }

        private static void RequestDirectory()
        {
            StaThread.Start(async () =>
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
                    MessageBox.Show($"An error occurred while attempting to request the specified directory.\n\n{ex.GetExceptionMessageTree()}\n\n Is the {Constants.AppName} service running?", Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            });
        }

        private static void Usage()
        {
            MessageBox.Show($"Usage: {Constants.AppName} [/port <port>]\n\nwhere <port> is the HTTP port that the program should listen on.", Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}