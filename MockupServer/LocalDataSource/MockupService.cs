using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using MockupServer.Storage;
using System.Net;

namespace MockupServer.LocalDataSource
{
    public class MockupService
    {
        readonly ILogger<MockupService> _logger;
        readonly HttpClient _httpClient;
        readonly DataService _dataService;

        public MockupService(DataService dataService, HttpClient httpClient, ILogger<MockupService> logger)
        {
            _dataService = dataService;
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<HttpResponseMessage?> GetDataFromRemote(HttpRequestMessage httpRequestMessage)
        {
            try
            {
                return await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "代理请求失败：{Url}", httpRequestMessage.RequestUri);
                return null;
            }
        }

        /// <summary>
        /// 规范化域名前缀：去除尾部'/'后补一个'/'，保证相对路径计算从'/'开始且不会误匹配更长域名
        /// </summary>
        static string NormalizePrefix(string urlPrefix)
        {
            return urlPrefix.TrimEnd('/') + "/";
        }

        public async Task<HttpResponseMessage> GetResponseMessage(HttpContext context)
        {
            try
            {
                var totalUrl = context.Request.GetDisplayUrl();
                var domainList = await _dataService.GetDomainList();
                var effectiveData = domainList
                    .Select(x => new { Item = x, Prefix = NormalizePrefix(x.UrlPrefix) })
                    .Where(x => totalUrl.StartsWith(x.Prefix, StringComparison.OrdinalIgnoreCase)
                                && !string.IsNullOrWhiteSpace(x.Item.Target))
                    .OrderByDescending(x => x.Prefix.Length)
                    .ToArray();

                if (effectiveData.Length == 0)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);

                // 缓冲请求体，供多个候选域名代理时重复读取
                byte[]? bodyBytes = null;
                if (context.Request.ContentLength > 0)
                {
                    context.Request.EnableBuffering();
                    using var ms = new MemoryStream();
                    await context.Request.Body.CopyToAsync(ms, context.RequestAborted);
                    bodyBytes = ms.ToArray();
                }

                var proxyFailed = false;
                foreach (var item in effectiveData)
                {
                    // 前缀已规范化为以'/'结尾，Length-1 使相对路径以'/'开头
                    var relativeUrl = totalUrl.Substring(item.Prefix.Length - 1);

                    var data = await _dataService.GetDataFromCache(context.Request.Method, relativeUrl);
                    if (data != null)
                        return data;

                    var newUri = new Uri($"{item.Item.Target.TrimEnd('/')}{relativeUrl}");
                    using var requestMessage = RequestBody.CreateProxyRequest(context.Request, newUri, bodyBytes);
                    data = await GetDataFromRemote(requestMessage);

                    if (data != null)
                        return data;

                    proxyFailed = true;
                }

                // 匹配到域名但全部代理失败，与"无路由"区分
                if (proxyFailed)
                    return new HttpResponseMessage(HttpStatusCode.BadGateway);

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理请求失败：{Url}", context.Request.GetDisplayUrl());
                throw;
            }
        }
    }
}
