using FreeSql.DataAnnotations;

namespace MockupServer.Models
{
    [Table(Name = "mockupobject")]
    public class MockupObject
    {
        public string RequestUrl { get; set; } = null!;
        public string Method { get; set; } = "GET";

        /// <summary>
        /// 对应 <see cref="ResponseType"/>：0. Empty 1. JsonObject 2. Text 3. File 4. XmlObject 5. Redirect 6. RandomJsonDataFromCSharp
        /// </summary>
        public int ResponseType { get; set; } = 1;
        /// <summary>
        /// Empty: null
        /// JsonObject, Text: 字符串内容
        /// File: 文件路径
        /// XmlObject: Xml字符串
        /// Redirect: 跳转地址
        /// RandomJsonDataFromCSharp: C#类定义代码
        /// </summary>
        public string? ResponseData { get; set; }

        public MockupObject(string requestUrl, string method, int responseType, string? responseData)
        {
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
