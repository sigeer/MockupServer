﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MockupServer.Configs;
using MockupServer.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net.Http.Formatting;
using System.Text;

namespace MockupServer.LocalDataSource
{
    public class MockupService
    {
        readonly IMongoDatabase _db;
        readonly HttpClientPool _pool;
        readonly ILogger<MockupService> _logger;

        public MockupService(IMongoClient client, HttpClientPool httpClientPool, IConfiguration configuration, ILogger<MockupService> logger)
        {
            _db = client.GetDatabase(configuration["DataBase"] ?? ServerSettings.DefaultDataBase);
            _pool = httpClientPool;
            _logger = logger;
        }

        public async Task<HttpResponseMessage?> SendObject(HttpRequestMessage httpRequestMessage, string originalHost, string relativeUrl)
        {
            try
            {
                var table = _db.GetCollection<MockupObject>(originalHost);
                var data = (await table.FindAsync(x => x.RequestUrl == relativeUrl.ToLower())).FirstOrDefault();
                if (data != null)
                {
                    _logger.LogInformation($"{relativeUrl} read from MongoDB");
                    var fakeContent = new ObjectContent<object>(JsonConvert.DeserializeObject(data.ResponseData), new JsonMediaTypeFormatter(), "application/json");
                    var fakeContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    fakeContentType.CharSet = "utf-8";
                    fakeContent.Headers.ContentType = fakeContentType;
                    var fakeData = new HttpResponseMessage() { Content = fakeContent, StatusCode = System.Net.HttpStatusCode.OK };
                    return fakeData;
                }
                else
                {
                    var httpClient = _pool.GetHttpClient();

                    try
                    {
                        var remoteData = await httpClient.HttpSendCore(httpRequestMessage);
                        if (remoteData.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"{relativeUrl} read from {httpRequestMessage.RequestUri.ToString()}");
                            return remoteData;
                        }
                        return null;
                    }
                    finally
                    {
                        _pool.Return(httpClient);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }

        }

        public async Task InserRecord(string collection, string url, string res)
        {
            var table = _db.GetCollection<MockupObject>(collection);
            await table.InsertOneAsync(new MockupObject { RequestUrl = url.ToLower(), ResponseData = res });
        }

        public async Task DeleteRecordAsync(string collection, string url)
        {
            var table = _db.GetCollection<MockupObject>(collection);
            await table.DeleteManyAsync(x => x.RequestUrl == url.ToLower());
        }

        public async Task<List<MockupObject>> GetDataList(string collection, string kw)
        {
            var table = _db.GetCollection<MockupObject>(collection);
            var total = await (await table.FindAsync(x => x.RequestUrl.Contains(kw))).ToListAsync();
            return total;
        }
    }
}
