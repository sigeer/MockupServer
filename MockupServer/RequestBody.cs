using Microsoft.AspNetCore.Http;

namespace MockupServer
{
    public static class RequestBody
    {
        /// <summary>
        /// 基于已缓冲的请求体构造代理请求，避免重复读取原始请求流
        /// </summary>
        public static HttpRequestMessage CreateProxyRequest(HttpRequest request, Uri newUri, byte[]? body)
        {
            var requestMsg = new HttpRequestMessage(new HttpMethod(request.Method), newUri)
            {
                Version = request.Protocol switch
                {
                    "HTTP/2" => System.Net.HttpVersion.Version20,
                    _ => System.Net.HttpVersion.Version11,
                },
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
            };

            if (body != null && body.Length > 0)
                requestMsg.Content = new ByteArrayContent(body);

            foreach (var h in request.Headers)
            {
                var values = h.Value.ToArray();
                if (!requestMsg.Headers.TryAddWithoutValidation(h.Key, values))
                    requestMsg.Content?.Headers.TryAddWithoutValidation(h.Key, values);
            }

            return requestMsg;
        }
    }
}
