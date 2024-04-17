using Microsoft.Extensions.Logging;
using MockupServer.Storage;

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


        public async Task<HttpResponseMessage?> GetResponseMessage(HttpRequestMessage httpRequestMessage)
        {
            try
            {
                var data = await _dataService.GetDataFromCache(httpRequestMessage);
                if (data == null)
                    data = await GetDataFromRemote(httpRequestMessage);
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
