using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockupServer.LocalDataSource;
using Serilog;
using Serilog.Events;

namespace MockupServer
{
    public static class RequestBody
    {
        public static async Task<HttpRequestMessage> GetHttpRequestMessage(HttpContext context, Uri? newUri = null)
        {
            var requestMsg = new HttpRequestMessageFeature(context).HttpRequestMessage;
            if (newUri != null)
                requestMsg.RequestUri = newUri;
            return await CloneHttpRequestMessageAsync(requestMsg);
        }
        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            HttpRequestMessage copyOfRequest = new HttpRequestMessage(request.Method, request.RequestUri);

            // Copy the request's content (via a MemoryStream) into the cloned object
            var ms = new MemoryStream();
            if (request.Content != null)
            {
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                copyOfRequest.Content = new StreamContent(ms);

                // Copy the content headers
                if (request.Content.Headers != null)
                {
                    foreach (var h in request.Content.Headers)
                    {
                        copyOfRequest.Content.Headers.Add(h.Key, h.Value);
                    }
                }
            }

            copyOfRequest.Version = request.Version;

            foreach (KeyValuePair<string, object> prop in request.Options)
            {
                copyOfRequest.Options.TryAdd(prop.Key, prop.Value);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                copyOfRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return copyOfRequest;
        }
    }
}
