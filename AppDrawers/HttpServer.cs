using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace AppDrawers
{
    public class HttpServer
    {
        private HttpListener listener = null;
        private Logger logger = null;
        private Dictionary<Regex, ResponseHandler> routes = null;

        public HttpServer(List<string> prefixes, Dictionary<Regex, ResponseHandler> routes, Logger logger)
        {
            this.logger = logger;

            this.listener = new HttpListener();

            this.routes = routes;

            foreach (var prefix in prefixes)
            {
                this.listener.Prefixes.Add(prefix);
            }
        }

        public delegate (string contentType, byte[] buffer) ResponseHandler(HttpListenerContext context);

        public void Start()
        {
            this.listener.Start();

            foreach (var prefix in this.listener.Prefixes)
            {
                this.logger.LogMessage($"Listening on {prefix}");
            }

            while (this.listener.IsListening)
            {
                try
                {
                    this.listener.BeginGetContext(this.HandleRequest, null).AsyncWaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    this.logger.LogMessage($"Server listening exception: {ex.GetExceptionMessageTree()}");
                    this.logger.LogMessage(ex.StackTrace);
                }
            }
        }

        public void Stop()
        {
            try { this.listener.Close(); } catch (Exception ex) { this.logger.LogMessage($"Failed to close server communication services. {ex.GetExceptionMessageTree()}"); }

            try { this.listener.Stop(); } catch (Exception ex) { this.logger.LogMessage($"Failed to stop server process. {ex.GetExceptionMessageTree()}"); }
        }

        private void HandleRequest(IAsyncResult ar)
        {
            HttpListenerContext context = null;

            try
            {
                context = this.listener.EndGetContext(ar);

                this.logger.LogMessage($"URL requested: {context.Request.Url.LocalPath}");

                foreach (var routeRegEx in this.routes.Keys)
                {
                    if (routeRegEx.IsMatch(context.Request.RawUrl))
                    {
                        var handler = this.routes[routeRegEx];

                        var response = handler.Invoke(context);

                        context.Response.ContentType = response.contentType ?? "text/plain";

                        if (response.buffer != null)
                        {
                            context.Response.ContentLength64 = response.buffer.Length;
                            context.Response.OutputStream.Write(response.buffer, 0, response.buffer.Length);
                        }

                        context.Response.OutputStream.Close();

                        context.Response.StatusCode = (int)HttpStatusCode.OK;

                        context.Response.Close();

                        return;
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.Close();

                    this.logger.LogMessage($"Server exception: {ex.GetExceptionMessageTree()}");
                    this.logger.LogMessage(ex.StackTrace);
                }
            }
        }
    }
}