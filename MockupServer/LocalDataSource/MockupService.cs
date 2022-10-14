using Microsoft.Extensions.Configuration;
using MockupServer.Configs;
using MockupServer.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace MockupServer.LocalDataSource
{
    public class MockupService
    {
        readonly IMongoDatabase _db;
        readonly HttpClientPool _pool;

        public MockupService(IMongoClient client, HttpClientPool httpClientPool, IConfiguration configuration)
        {
            _db = client.GetDatabase(configuration["DataBase"] ?? ServerSettings.DefaultDataBase);
            _pool = httpClientPool;
        }

        public async Task<object?> GetObject(string url, string relativeUrl, Dictionary<string, string> headers)
        {
            var httpClient = _pool.GetHttpClient();
            try
            {
                var remoteData = await httpClient.HttpGetCore(url, headers);

                if (remoteData.IsSuccessStatusCode)
                {
                    var data = await remoteData.Content.ReadFromJsonAsync<object>();
                    return data;
                }
                else
                {
                    var table = _db.GetCollection<MockupObject>(DateTime.Today.ToString("yyyyMM"));
                    var data = (await table.FindAsync(x => x.RequestUrl == relativeUrl)).FirstOrDefault();
                    if (data != null)
                        return data.ResponseData;
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                _pool.Return(httpClient);
            }
        }

        public async Task<object?> PostObject(string url, string relativeUrl, string? postBodyStr, Dictionary<string, string> headers)
        {
            var httpClient = _pool.GetHttpClient();
            try
            {
                var remoteData = await httpClient.HttpPostCore(url, JsonConvert.DeserializeObject<object>(postBodyStr), headers);
                if (remoteData.IsSuccessStatusCode)
                {
                    var data = await remoteData.Content.ReadFromJsonAsync<object>();
                    return data;
                }
                else
                {
                    var table = _db.GetCollection<MockupObject>(DateTime.Today.ToString("yyyyMM"));
                    var data = (await table.FindAsync(x => x.RequestUrl == relativeUrl)).FirstOrDefault();
                    if (data != null)
                        return data.ResponseData;
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                _pool.Return(httpClient);
            }
        }


        public async Task<HttpResponseMessage?> SendObject(HttpRequestMessage httpRequestMessage, string relativeUrl)
        {
            var httpClient = _pool.GetHttpClient();
            try
            {
                var remoteData = await httpClient.HttpSendCore(httpRequestMessage);
                if (remoteData.IsSuccessStatusCode)
                {
                    var data = remoteData;
                    return data;
                }
                else
                {
                    var table = _db.GetCollection<MockupObject>(DateTime.Today.ToString("yyyyMM"));
                    var data = (await table.FindAsync(x => x.RequestUrl == relativeUrl)).FirstOrDefault();
                    if (data != null)
                    {
                        var fakeContent = JsonContent.Create(data.ResponseData);
                        var fakeContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        fakeContentType.CharSet = "utf-8";
                        fakeContent.Headers.ContentType = fakeContentType;
                        var fakeData = new HttpResponseMessage() { Content = fakeContent };
                        return fakeData;

                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                _pool.Return(httpClient);
            }
        }

        public async Task InserRecord(string url, object res)
        {
            var table = _db.GetCollection<MockupObject>(DateTime.Today.ToString("yyyyMM"));
            await table.InsertOneAsync(new MockupObject { RequestUrl = url, ResponseData = res });
        }
    }
}
