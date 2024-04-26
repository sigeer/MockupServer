using Microsoft.Extensions.Logging;
using MockupServer.Models;
using MockupServer.Response;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Xml;
using Utility.Extensions;

namespace MockupServer.Storage
{
    public class DataService
    {
        readonly IFreeSql _freeSql;
        readonly ILogger<DataService> _logger;

        public DataService(IFreeSql freeSql, ILogger<DataService> logger)
        {
            _freeSql = freeSql;
            _logger = logger;
        }

        public async Task<List<Domain>> GetDomainList()
        {
            return await _freeSql.Select<Domain>().ToListAsync();
        }

        public async Task<Domain> InsertOrUpdateDomain(Domain model)
        {
            await _freeSql.InsertOrUpdate<Domain>().SetSource(model).ExecuteAffrowsAsync();
            return model;
        }

        public async Task<GlobalCache> GetGlobal()
        {
            var model = await _freeSql.Select<GlobalCache>().ToOneAsync();
            if (model == null)
            {
                model = new GlobalCache();
                await _freeSql.Insert<GlobalCache>(model).ExecuteAffrowsAsync();
            }
            return model;
        }

        public async Task<GlobalCache> SaveGlobal(GlobalCache model)
        {
            var ds = await _freeSql.Update<GlobalCache>().WhereIf(true, x => true).Set(x => x.ListenerPort, model.ListenerPort).ExecuteAffrowsAsync();
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
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        static internal HttpResponseMessage GenerateResponseMessage(ResponseType type, string? content)
        {
            if (content == null)
                return new HttpResponseMessage(HttpStatusCode.NoContent);

            HttpContent fakeContent;
            switch (type)
            {
                case ResponseType.Empty:
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                case ResponseType.JsonObject:
                    var fakeContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
                    {
                        CharSet = "utf-8"
                    };
                    // fakeContent = new ObjectContent<object>(JsonConvert.DeserializeObject(data.ResponseData ?? "{}") ?? new object { }, new JsonMediaTypeFormatter(), fakeContentType);
                    fakeContent = new StringContent(content, fakeContentType);
                    break;
                case ResponseType.Text:
                    var textContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Plain)
                    {
                        CharSet = "utf-8"
                    };
                    fakeContent = new StringContent(content, textContentType);
                    break;
                case ResponseType.File:
                    fakeContent = new StreamContent(File.OpenRead(content));
                    fakeContent.Headers.ContentType = new MediaTypeHeaderValue(content.GetMimeTypeByFileName());
                    break;
                case ResponseType.XmlObject:
                    var xmlContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Xml)
                    {
                        CharSet = "utf-8"
                    };
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(content);

                    fakeContent = new StringContent(xmlDoc.InnerXml, xmlContentType);
                    break;
                case ResponseType.Redirect:
                    var msg = new HttpResponseMessage(HttpStatusCode.Redirect);
                    msg.Headers.Location = new Uri(content);
                    return msg;
                default:
                    var errorMsg = $"不正常的ResponseType={type}";
                    throw new Exception(errorMsg);
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = fakeContent };
        }

        public async Task InserOrUpdateRecord(string url, string method, int type, string res)
        {
            var model = new MockupObject(url, method, type, res);

            var sameItem = await _freeSql.Select<MockupObject>().Where<MockupObject>(x => x.RequestUrl == model.RequestUrl && x.Method == model.Method).ToOneAsync();
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
            kw = kw.Replace("?", "\\?").Replace("&", "\\&");
            var total = await _freeSql.Select<MockupObject>().Where(x => x.RequestUrl.Contains(kw)).ToListAsync();
            return total;
        }
    }
}
