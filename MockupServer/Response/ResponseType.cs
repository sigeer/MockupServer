using System.ComponentModel;

namespace MockupServer.Response
{
    /// <summary>
    /// 更新后，需要相应的修改<see cref="MockupServer.Storage.DataService.GenerateResponseMessage(ResponseType, string?)"/>
    /// </summary>
    public enum ResponseType
    {
        [Description("空")]
        Empty,
        [Description("Json对象")]
        JsonObject,
        [Description("文本")]
        Text,
        [Description("文件")]
        File,
        [Description("Xml对象")]
        XmlObject,
        [Description("转发")]
        Redirect,
        [Description("Json对象-从C#随机生成")]
        RandomJsonDataFromCSharp,
    }
}
