using FreeSql.DataAnnotations;

namespace MockupServer.Models
{
    [Table(Name = "mockupobject")]
    public class MockupObject
    {
        public string RequestUrl { get; set; } = null!;
        public string Method { get; set; } = "GET";

        /// <summary>
        /// 0. Empty
        /// 1. Json
        /// 2. Text
        /// 3. File
        /// 4. Xml
        /// </summary>
        public int ResponseType { get; set; } = 1;
        /// <summary>
        /// Empty: null
        /// Json, JsonString
        /// Text: String,
        /// File: FilePath,
        /// Xml: XmlString
        /// </summary>
        public string? ResponseData { get; set; }
        /// <summary>
        /// ResponseData为null，或者UseRandomGenerate为true时，返回随机数据
        /// </summary>
        // public bool UseRandomGenerate { get; set; } = false;

        public MockupObject(string requestUrl, string method, int responseType, string? responseData)        {
            RequestUrl = requestUrl;
            Method = method;
            ResponseType = responseType;
            ResponseData = responseData;
        }

        private MockupObject() { }
        public static MockupObject Empty()
        {
            return new MockupObject();
        }

    }
}
