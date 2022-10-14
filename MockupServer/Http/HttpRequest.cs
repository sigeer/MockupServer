using System.Net.Http.Json;

namespace MockupServer.Http
{
    public static class HttpRequest
    {
        private static async Task<HttpResponseMessage> HttpPostCore(string url, object data,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                return await httpClient.HttpPostCore(url, data, headers, cancellationToken);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostCore(this HttpClient httpClient, string url, object data,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            if (headers != null)
            {
                if (!headers.Keys.Any(x => x.ToLower() == "content-type"))
                    headers.Add("Content-Type", "application/json");
                foreach (var key in headers.Keys)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, headers[key]);
                }
            }
            return await httpClient.PostAsync(url, data == null ? null : JsonContent.Create(data), cancellationToken);
        }

        public static async Task<string> PostAsync(string url, object data,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var res = await HttpPostCore(url, data, headers, cancellationToken);

            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<T?> PostAsync<T>(string url, object data,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default) where T : class
        {
            var res = await HttpPostCore(url, data, headers, cancellationToken);

            return await res.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<HttpResponseMessage> HttpGetCore(this HttpClient httpClient, string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            if (headers != null)
            {
                if (!headers.Keys.Any(x => x.ToLower() == "content-type"))
                    headers.Add("Content-Type", "application/json");
                foreach (var key in headers.Keys)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, headers[key]);
                }
            }
            return await httpClient.GetAsync(url, cancellationToken);
        }

        private static async Task<HttpResponseMessage> HttpGetCore(string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                return await httpClient.HttpGetCore(url, headers, cancellationToken);
            }
        }

        public static async Task<string> GetAsync(string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var res = await HttpGetCore(url, headers, cancellationToken);
            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<T?> GetAsync<T>(string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default) where T : class
        {
            var res = await HttpGetCore(url, headers, cancellationToken);
            return await res.Content.ReadFromJsonAsync<T>();
        }

        private static async Task<HttpResponseMessage> HttpSendCore(HttpRequestMessage requestMessage,
    CancellationToken cancellationToken = default)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                return await httpClient.HttpSendCore(requestMessage, cancellationToken);
            }
        }

        public static async Task<HttpResponseMessage> HttpSendCore(this HttpClient httpClient, HttpRequestMessage requestMessage,
            CancellationToken cancellationToken = default)
        {
            return await httpClient.SendAsync(requestMessage, cancellationToken);
        }

        public static async Task<string> SendAsync(HttpRequestMessage requestMessage,
    CancellationToken cancellationToken = default)
        {
            var res = await HttpSendCore(requestMessage, cancellationToken);
            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<T?> SendAsync<T>(HttpRequestMessage requestMessage,
            CancellationToken cancellationToken = default) where T : class
        {
            var res = await HttpSendCore(requestMessage, cancellationToken);
            return await res.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<HttpResponseMessage> PostRawAsync(string url, object data,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var res = await HttpPostCore(url, data, headers, cancellationToken);
            return res;
        }
        public static async Task<HttpResponseMessage> GetRawAsync(string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var res = await HttpGetCore(url, headers, cancellationToken);
            return res;
        }
    }
}
