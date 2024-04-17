using ICSharpCode.TextEditor;
using MockupServer.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace MockupServer.UI
{
    public partial class EditorForm : Form
    {
        readonly ResponseType _type;
        private TextEditorControl textEditorControl;
        public EventHandler<string>? OnSubmit;
        public EditorForm(string? content, ResponseType type)
        {
            _type = type;

            InitializeComponent();

            // 创建 TextEditor 控件实例
            textEditorControl = new TextEditorControl();
            textEditorControl.Dock = DockStyle.Fill;
            textEditorControl.Encoding = System.Text.Encoding.UTF8;
            textEditorControl.Font = new System.Drawing.Font("Consolas", 10);
            // 将 TextEditor 控件添加到窗体中
            Panel_Editor.Controls.Add(textEditorControl);

            textEditorControl.Document.FoldingManager.FoldingStrategy = new MingFolding();
            textEditorControl.TextChanged += TextEditor_TextChanged;
            textEditorControl.Text = content;

            if (_type == ResponseType.JsonObject)
            {
                Btn_Formatter.Text = "格式化（Json）";
                Btn_Formatter.Visible = true;
            }
            else if (_type == ResponseType.XmlObject)
            {
                Btn_Formatter.Text = "格式化（Xml）";
                Btn_Formatter.Visible = true;
            }
            else
            {
                Btn_Formatter.Visible = false;
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Btn_Submit_Click(object sender, EventArgs e)
        {
            OnSubmit?.Invoke(sender, textEditorControl.Text);
            Close();
        }

        private void TextEditor_TextChanged(object? sender, EventArgs e)
        {
            textEditorControl.Document.FoldingManager.UpdateFoldings(null, null);
        }

        private void Btn_Formatter_Click(object sender, EventArgs e)
        {
            if (_type == ResponseType.JsonObject)
            {
                try
                {
                    // 解析 JSON 字符串为 JObject 对象
                    JObject jsonObject = JObject.Parse(textEditorControl.Text);

                    // 设置 JSON 序列化选项
                    var settings = new JsonSerializerSettings
                    {
                        // 设置缩进
                        Formatting = Newtonsoft.Json.Formatting.Indented
                    };

                    // 将 JObject 对象序列化为缩进后的 JSON 字符串
                    textEditorControl.Text = JsonConvert.SerializeObject(jsonObject, settings);
                }
                catch 
                {
                }
            }
            if (_type == ResponseType.XmlObject)
            {
                try
                {
                    // 加载 XML 字符串到 XmlDocument 对象
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(textEditorControl.Text);

                    // 设置 XML 序列化选项
                    var settings = new XmlWriterSettings
                    {
                        // 设置缩进
                        Indent = true,
                        OmitXmlDeclaration = true,
                        IndentChars = "   " // 设置缩进字符为四个空格
                    };

                    // 使用 XmlWriter 将 XmlDocument 对象写入到内存流中
                    using (var stringWriter = new System.IO.StringWriter())
                    {
                        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                        {
                            xmlDoc.WriteTo(xmlWriter);
                        }

                        // 获取缩进后的 XML 字符串
                        textEditorControl.Text = stringWriter.ToString();
                    }
                }
                catch
                {
                }
            }
        }
    }
}
