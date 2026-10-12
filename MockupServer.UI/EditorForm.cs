using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using MockupServer.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace MockupServer.UI
{
    public partial class EditorForm : Form
    {
        static bool _highlightingRegistered;
        readonly ResponseType _type;
        private TextEditorControl textEditorControl;
        public EventHandler<string>? OnSubmit;
        public EditorForm(string? content, ResponseType type)
        {
            _type = type;

            InitializeComponent();
            KeyPreview = true;

            // 创建 TextEditor 控件实例
            textEditorControl = new TextEditorControl();
            textEditorControl.Dock = DockStyle.Fill;
            textEditorControl.Encoding = System.Text.Encoding.UTF8;
            // 将 TextEditor 控件添加到窗体中
            Panel_Editor.Controls.Add(textEditorControl);

            textEditorControl.Document.FoldingManager.FoldingStrategy = new MingFolding();
            textEditorControl.TextChanged += TextEditor_TextChanged;
            textEditorControl.Text = content;

            InitializeHighLighting();

            if (_type == ResponseType.JsonObject)
            {
                Btn_Formatter.Text = "格式化（Json）";
                Btn_Formatter.Visible = true;
                textEditorControl.SetHighlighting("JSON");
            }
            else if (_type == ResponseType.XmlObject)
            {
                Btn_Formatter.Text = "格式化（Xml）";
                Btn_Formatter.Visible = true;
                textEditorControl.SetHighlighting("XML");
            }
            else if (_type == ResponseType.RandomJsonDataFromCSharp)
            {
                if (string.IsNullOrEmpty(textEditorControl.Text?.Trim()))
                {
                    textEditorControl.Text = GetDefaultCodeForRandom();
                }
                Btn_Formatter.Text = "重置";
                Btn_Formatter.Visible = true;
                textEditorControl.SetHighlighting("C#");
            }
            else
            {
                Btn_Formatter.Visible = false;
            }
        }

        private void InitializeHighLighting()
        {
            if (_highlightingRegistered)
                return;

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HighLighting");
            if (Directory.Exists(path))
            {
                var fsmp = new FileSyntaxModeProvider(path);
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmp);
                _highlightingRegistered = true;
                // 更多高亮配置：https://github.com/xv/ICSharpCode.TextEditor-Lexers/tree/master/Syntax
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

        private string GetDefaultCodeForRandom()
        {
            return """
                    // [Default(1)] 设置默认值，不会再使用随机值
                    // [Range(1, 100)] 设置数字类型的随机取值范围，数组长度的取值范围
                    // 这个类只能修改data的类型，如果类名与你定义的类名重复，需要修改你定义的类名
                    public class Root
                    {
                        // 将YourClass替换成想要返回的类型（非系统类型须此处定义），最终返回的内容将会是data而不是{data: xxx}
                        // 不能修改data的名称
                        public YourClass data { get; set; }
                        // 添加其他字段属性将没有意义，不会参与读取
                    }
                    """ + Environment.NewLine + Environment.NewLine;
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
                catch (Exception ex)
                {
                    Alert.Show($"Json格式化失败：{ex.Message}");
                }
            }
            else if (_type == ResponseType.XmlObject)
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
                        IndentChars = "    " // 设置缩进字符为四个空格
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
                catch (Exception ex)
                {
                    Alert.Show($"Xml格式化失败：{ex.Message}");
                }
            }
            else if (_type == ResponseType.RandomJsonDataFromCSharp)
            {
                textEditorControl.Text = GetDefaultCodeForRandom();
            }
        }

        private void EditorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.S)
                {
                    OnSubmit?.Invoke(sender, textEditorControl.Text);
                    Close();
                }
            }
        }
    }
}
