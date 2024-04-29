using Microsoft.AspNetCore.Http;
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
                return await _httpClient.SendAsync(httpRequestMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }


        public async Task<HttpResponseMessage?> GetResponseMessage(HttpContext context)
        {
            try
            {
                var relativeUrl = $"{context.Request.Path}{context.Request.QueryString}";
                HttpResponseMessage? data = await _dataService.GetDataFromCache(context.Request.Method, relativeUrl);
                if (data != null)
                    return data;

                if (data == null)
                {
                    // 代理模式
                    if (context.Request.Host.Host != "localhost" && context.Request.Host.Port != State.Port)
                    {
                        var requestMessage = await RequestBody.GetHttpRequestMessage(context);
                        data = await GetDataFromRemote(requestMessage);
                    }
                }

                if (data == null)
                {
                    var domainList = await _dataService.GetDomainList();
                    var effectiveData = domainList.Where(x =>
                                            relativeUrl.StartsWith(x.UrlPrefix, StringComparison.OrdinalIgnoreCase)
                                            && !string.IsNullOrWhiteSpace(x.Target)
                                       ).OrderByDescending(x => x.UrlPrefix.Length)
                                        .ToArray();
                    foreach (var item in effectiveData)
                    {
                        var newUri = new Uri($"{item.Target}{relativeUrl}");
                        var requestMessage = await RequestBody.GetHttpRequestMessage(context, newUri);
                        data = await GetDataFromRemote(requestMessage);

                        if (data != null)
                            break;
                    }
                }
                data ??= new HttpResponseMessage(HttpStatusCode.NotFound);
                return data;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}
