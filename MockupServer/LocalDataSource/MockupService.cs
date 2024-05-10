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
                var totalUrl = context.Request.GetDisplayUrl();
                var domainList = await _dataService.GetDomainList();
                var effectiveData = domainList.Where(x =>
                                        totalUrl.StartsWith(x.UrlPrefix, StringComparison.OrdinalIgnoreCase)
                                        && !string.IsNullOrWhiteSpace(x.Target)
                                   ).OrderByDescending(x => x.UrlPrefix.Length)
                                    .ToArray();

                if (effectiveData.Length == 0)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);

                foreach (var item in effectiveData)
                {
                    var relativeUrl = totalUrl.Substring(item.UrlPrefix.Length - 1);

                    var data = await _dataService.GetDataFromCache(context.Request.Method, relativeUrl);
                    if (data != null)
                        return data;

                    var newUri = new Uri($"{item.Target.TrimEnd('/')}{relativeUrl}");
                    var requestMessage = await RequestBody.GetHttpRequestMessage(context, newUri);
                    data = await GetDataFromRemote(requestMessage);

                    if (data != null)
                        return data;
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}
