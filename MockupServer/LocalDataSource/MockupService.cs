﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MockupServer.Http;
using MockupServer.Models;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace MockupServer.LocalDataSource
{
    public class MockupService
    {
        readonly IFreeSql _freeSql;
        readonly HttpClientPool _pool;
        readonly ILogger<MockupService> _logger;
        readonly HttpClientHandler httpclientHandler;

        public MockupService(IFreeSql freeSql, HttpClientPool httpClientPool, IConfiguration configuration, ILogger<MockupService> logger)
        {
            _freeSql = freeSql;
            _pool = httpClientPool;
            _logger = logger;
            httpclientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true,
                UseCookies = true
            };
        }

        private async Task<HttpResponseMessage?> GetDataFromRemote(HttpRequestMessage httpRequestMessage, string relativeUrl)
        {
            var httpClient = _pool.GetHttpClient(httpclientHandler);

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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
            finally
            {
                _pool.Return(httpClient);
            }
        }

        private async Task<HttpResponseMessage?> GetDataFromCache(string relativeUrl)
        {
            //relativeUrl = relativeUrl.Replace("?", "\\?").Replace("&", "\\&");
            var data = await _freeSql.Select<MockupObject>().Where<MockupObject>(x => x.RequestUrl == relativeUrl).ToOneAsync();
            if (data == null)
                return null;

            var fakeContent = new ObjectContent<object>(JsonConvert.DeserializeObject(data.ResponseData ?? "{}") ?? new object { }, new JsonMediaTypeFormatter(), "application/json");
            var fakeContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };
            fakeContent.Headers.ContentType = fakeContentType;
            var fakeData = new HttpResponseMessage() { Content = fakeContent, StatusCode = System.Net.HttpStatusCode.OK };
            return fakeData;
        }

        public async Task<HttpResponseMessage?> SendObject(HttpRequestMessage httpRequestMessage, string relativeUrl)
        {
            try
            {
                var remote = await GetDataFromCache(relativeUrl);
                if (remote != null)
                {
                    return remote;
                }
                else
                {
                    var remoteData = await GetDataFromRemote(httpRequestMessage, relativeUrl);
                    if (State.IsRecording 
                        && remoteData != null 
                        && remoteData.IsSuccessStatusCode 
                        && IsTextMediaType(remoteData.Content.Headers.ContentType?.MediaType))
                    {
                        //record
                        await InserOrUpdateRecord(relativeUrl, await remoteData.Content.ReadAsStringAsync());
                    }
                    return remoteData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        public bool IsTextMediaType(string? mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
                return false;
            return mediaType.StartsWith("text/") || new string[] { "application/json" , "application/xml" }.Contains(mediaType.ToLower());
        }

        public async Task InserOrUpdateRecord(string url, string res)
        {
            await _freeSql.InsertOrUpdate<MockupObject>().SetSource(new MockupObject { RequestUrl = url, ResponseData = res }).ExecuteAffrowsAsync();
        }

        public async Task DeleteRecordAsync(string url)
        {
            await _freeSql.Delete<MockupObject>().Where(x => x.RequestUrl == url).ExecuteAffrowsAsync();
        }

        public async Task<List<MockupObject>> GetDataList(string kw)
        {
            kw = kw.Replace("?", "\\?").Replace("&", "\\&");
            var total = await _freeSql.Select<MockupObject>().Where(x => x.RequestUrl.Contains(kw)).ToListAsync();
            return total;
        }
    }
}
