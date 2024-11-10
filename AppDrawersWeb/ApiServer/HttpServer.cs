using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace AppDrawers.ApiServer
{
    public class HttpServer
    {
        private HttpListener listener = null;
        private Dictionary<Regex, ResponseHandler> routes = null;

        public HttpServer(List<string> prefixes, Dictionary<Regex, ResponseHandler> routes)
        {
            this.listener = new HttpListener();

            this.routes = routes;

            foreach (var prefix in prefixes)
            {
                this.listener.Prefixes.Add(prefix);
            }
        }

        public delegate AppResponse ResponseHandler(HttpListenerContext context);

        public void Start()
        {
            this.listener.Start();

            while (this.listener.IsListening)
            {
                try
                {
                    this.listener.BeginGetContext(this.HandleRequest, null).AsyncWaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Server listening exception.", ex);
                }
            }
        }

        public void Stop()
        {
            try { this.listener.Close(); } catch { /* Ignore */ }

            try { this.listener.Stop(); } catch { /* Ignore */ }
        }

        private void HandleRequest(IAsyncResult ar)
        {
            HttpListenerContext context = null;

            try
            {
                context = this.listener.EndGetContext(ar);

                foreach (var routeRegEx in this.routes.Keys)
                {
                    if (routeRegEx.IsMatch(context.Request.RawUrl))
                    {
                        var handler = this.routes[routeRegEx];

                        var response = handler.Invoke(context);

                        context.Response.ContentType = response.ContentType ?? "text/plain";

                        if (response.Buffer != null)
                        {
                            context.Response.ContentLength64 = response.Buffer.Length;
                            context.Response.OutputStream.Write(response.Buffer, 0, response.Buffer.Length);
                        }

                        context.Response.OutputStream.Close();

                        context.Response.StatusCode = (int)response.StatusCode;

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
                }
            }
        }
    }
}