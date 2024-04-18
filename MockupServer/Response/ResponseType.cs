using System.ComponentModel;

namespace MockupServer.Response
{
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
        Redirect
    }
}
