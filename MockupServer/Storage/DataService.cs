using Microsoft.Extensions.Logging;
using MockupServer.Models;
using MockupServer.Response;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Xml;
using Utility.Extensions;

namespace MockupServer.Storage
{
    public class DataService
    {
        static readonly TimeSpan DomainCacheTtl = TimeSpan.FromSeconds(3);
        static readonly object DomainCacheLock = new();
        static List<Domain>? _domainCache;
        static long _domainCacheTicks;

        readonly IFreeSql _freeSql;
        readonly ILogger<DataService> _logger;

        public DataService(IFreeSql freeSql, ILogger<DataService> logger)
        {
            _freeSql = freeSql;
            _logger = logger;
        }

        public async Task<List<Domain>> GetDomainList()
        {
            lock (DomainCacheLock)
            {
                if (_domainCache != null && Environment.TickCount64 - _domainCacheTicks < DomainCacheTtl.TotalMilliseconds)
                    return _domainCache;
            }

            var list = await _freeSql.Select<Domain>().ToListAsync();
            lock (DomainCacheLock)
            {
                _domainCache = list;
                _domainCacheTicks = Environment.TickCount64;
            }
            return list;
        }

        static void InvalidateDomainCache()
        {
            lock (DomainCacheLock)
            {
                _domainCache = null;
            }
        }

        public async Task<Domain> InsertOrUpdateDomain(Domain model)
        {
            await _freeSql.InsertOrUpdate<Domain>().SetSource(model).ExecuteAffrowsAsync();
            InvalidateDomainCache();
            return model;
        }

        public async Task DeleteDomain(string urlPrefix)
        {
            await _freeSql.Delete<Domain>().Where(x => x.UrlPrefix == urlPrefix).ExecuteAffrowsAsync();
            InvalidateDomainCache();
        }

        public async Task<GlobalCache> GetGlobal()
        {
            var model = await _freeSql.Select<GlobalCache>().ToOneAsync();
            if (model == null)
            {
                model = new GlobalCache();
                await _freeSql.Insert<GlobalCache>(model).ExecuteAffrowsAsync();
                // 插入后重查，降低并发下拿到未落盘对象/重复行的概率
                model = await _freeSql.Select<GlobalCache>().ToOneAsync() ?? model;
            }
            return model;
        }

        public async Task<GlobalCache> SaveGlobal(GlobalCache model)
        {
            // 表语义为单行配置，无主键，更新全部行
            await _freeSql.Update<GlobalCache>().Where(x => true).Set(x => x.ListenerPort, model.ListenerPort).ExecuteAffrowsAsync();
            return model;
        }

        public async Task<HttpResponseMessage?> GetDataFromCache(string method, string relativeUrl)
        {
            var data = await _freeSql.Select<MockupObject>().Where(x => x.Method == method && x.RequestUrl == relativeUrl).ToOneAsync();
            if (data == null)
                return null;

            try
            {
                return GenerateResponseMessage((ResponseType)data.ResponseType, data.ResponseData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成Mock响应失败：{Method} {Url}", method, relativeUrl);
                return null;
            }
        }

        static MediaTypeHeaderValue CreateUtf8MediaType(string mediaType)
        {
            return new MediaTypeHeaderValue(mediaType)
            {
                CharSet = "utf-8"
            };
        }

        static HttpContent CreateUtf8Content(string mediaType, string content)
        {
            return new StringContent(content, CreateUtf8MediaType(mediaType));
        }

        internal HttpResponseMessage GenerateResponseMessage(ResponseType type, string? content)
        {
            if (content == null)
                return new HttpResponseMessage(HttpStatusCode.NoContent);

            HttpContent fakeContent;
            switch (type)
            {
                case ResponseType.Empty:
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                case ResponseType.JsonObject:
                    fakeContent = CreateUtf8Content(MediaTypeNames.Application.Json, content);
                    break;
                case ResponseType.Text:
                    fakeContent = CreateUtf8Content(MediaTypeNames.Text.Plain, content);
                    break;
                case ResponseType.File:
                    if (!File.Exists(content))
                        return new HttpResponseMessage(HttpStatusCode.NotFound);

                    fakeContent = new StreamContent(File.OpenRead(content));
                    fakeContent.Headers.ContentType = new MediaTypeHeaderValue(content.GetMimeTypeByFileName());
                    break;
                case ResponseType.XmlObject:
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(content);

                    fakeContent = CreateUtf8Content(MediaTypeNames.Application.Xml, xmlDoc.InnerXml);
                    break;
                case ResponseType.Redirect:
                    var msg = new HttpResponseMessage(HttpStatusCode.Redirect);
                    msg.Headers.Location = new Uri(content);
                    return msg;
                case ResponseType.RandomJsonDataFromCSharp:
                    var obj = new CSharpUtility(_logger).GenerateObjectByCSharpClass(content);
                    fakeContent = CreateUtf8Content(MediaTypeNames.Application.Json, JsonConvert.SerializeObject(obj));
                    break;
                default:
                    throw new Exception($"不正常的ResponseType={type}");
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = fakeContent };
        }

        public async Task InsertOrUpdateRecord(string url, string method, int type, string? res)
        {
            var model = new MockupObject(url, method, type, res);

            var sameItem = await _freeSql.Select<MockupObject>()
                .Where(x => x.RequestUrl == model.RequestUrl && x.Method == model.Method)
                .ToOneAsync();
            if (sameItem == null)
                await _freeSql.Insert<MockupObject>(model).ExecuteAffrowsAsync();
            else
                await _freeSql.Update<MockupObject>().Where(x => x.RequestUrl == model.RequestUrl && x.Method == model.Method)
                    .Set(x => x.ResponseData, model.ResponseData)
                    .Set(x => x.ResponseType, model.ResponseType).ExecuteAffrowsAsync();
        }

        public async Task DeleteRecordAsync(string url, string method)
        {
            await _freeSql.Delete<MockupObject>().Where(x => x.RequestUrl == url && x.Method == method).ExecuteAffrowsAsync();
        }

        public async Task<List<MockupObject>> GetDataList(string kw)
        {
            var query = _freeSql.Select<MockupObject>();
            if (!string.IsNullOrEmpty(kw))
                query = query.Where(x => x.RequestUrl.Contains(kw));

            var total = await query.ToListAsync();
            total.Reverse();
            return total;
        }
    }
}
