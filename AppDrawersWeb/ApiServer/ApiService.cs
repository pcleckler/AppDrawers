using AppDrawers.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AppDrawers.ApiServer
{
    public class ApiService
    {
        private readonly string WebRoot = @"D:\Drive\Programming2\C#\AppDrawers\AppDrawersWeb\GUI\www";// @".\www";
        private AppResponse SimpleAck = new AppResponse() { StatusCode = HttpStatusCode.OK };
        private WebForm webForm;

        public ApiService(UriIndex uriIndex, WebForm webForm)
        {
            this.webForm = webForm;

            StaThread.Start(() =>
            {
                (new HttpServer(

                    new List<string>()
                    {
                        uriIndex.ListenPrefix
                    },
                    new Dictionary<Regex, HttpServer.ResponseHandler>()
                    {
                        { new Regex($@"^.*/{UriIndex.GetDirectoryContentsCommand}[\?]*.*$"), GetDirectoryContents },
                        { new Regex($@"^.*/{UriIndex.DisplayMenuCommand}$"), DisplayMenu },
                        { new Regex($@"^.*/{UriIndex.HideMenuCommand}$"), HideMenu },
                        { new Regex($@"^.*/{UriIndex.ChangeDirectoryCommand}[\?]*.*$"), ChangeDirectory },
                        { new Regex(@"^.*$"), ServeStaticFiles },
                    }

                )).Start();
            });
        }

        private AppResponse ChangeDirectory(HttpListenerContext context)
        {
            var directory = context.Request.QueryString["dir"];

            var clipping = context.Request.QueryString["clipping"];

            this.webForm.Invoke(new Action(() =>
            {
                this.webForm.ChangeDirectory(directory, clipping);
            }));

            return this.SimpleAck;
        }

        private AppResponse DisplayMenu(HttpListenerContext context)
        {
            this.webForm.Invoke(new Action(() =>
            {
                this.webForm.DisplayMenu();
            }));

            return SimpleAck;
        }

        private AppResponse GetDirectoryContents(HttpListenerContext context)
        {
            var directory = context.Request.QueryString["dir"];

            var contents = DirectoryContentService.GetContent(directory);

            return new AppResponse()
            {
                Buffer = Encoding.UTF8.GetBytes(contents.ToJson()),
                ContentType = ContentTypes.Json,
            };
        }

        private AppResponse HideMenu(HttpListenerContext context)
        {
            this.webForm.Invoke(new Action(() =>
            {
                this.webForm.HideMenu();
            }));

            return SimpleAck;
        }

        private AppResponse ServeStaticFiles(HttpListenerContext context)
        {
            string fullPath = UrlToLocalPath(this.WebRoot, context.Request.RawUrl, string.Empty);

            if (!fullPath.StartsWith(this.WebRoot, StringComparison.OrdinalIgnoreCase))
            {
                return new AppResponse()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                };
            }

            // File not found?
            if (!File.Exists(fullPath))
            {
                return new AppResponse()
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            return new AppResponse()
            {
                Buffer = File.ReadAllBytes(fullPath),
                ContentType = ContentTypeMap.GetContentType(Path.GetExtension(fullPath))
            };
        }

        private string UrlToLocalPath(string webRoot, string urlString, string appRoot, string defaultFilename = "index.html")
        {
            if (urlString == null || webRoot == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(urlString))
            {
                return string.Empty;
            }

            // Decode the URL and remove leading slashes
            string relativePath = Uri.UnescapeDataString(urlString).TrimStart('/');

            // Remove the appRoot, if present
            if (relativePath.ToLower().StartsWith(appRoot.ToLower()))
            {
                relativePath = relativePath.Substring(appRoot.Length).TrimStart('/');
            }

            if (relativePath.Length < 1)
            {
                relativePath = defaultFilename;
            }

            // Combine the root directory with the decoded relative path
            string localPath = Path.Combine(webRoot, relativePath);

            // Resolve the full path and verify it starts with the root directory path
            string fullPath = Path.GetFullPath(localPath);

            return fullPath;
        }
    }
}