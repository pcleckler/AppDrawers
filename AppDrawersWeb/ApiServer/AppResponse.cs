using System;
using System.Net;

namespace AppDrawers.ApiServer
{
    public class AppResponse
    {
        public byte[] Buffer { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "text/plain";
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    }
}