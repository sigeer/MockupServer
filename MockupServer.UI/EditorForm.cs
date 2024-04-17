using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MockupServer.UI
{
    public partial class EditorForm : Form
    {
        private TextEditorControl textEditorControl;
        public EventHandler<string>? OnSubmit;
        public EditorForm(string? content)
        {
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
    }
}
