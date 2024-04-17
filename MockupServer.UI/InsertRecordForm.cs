using MockupServer.Models;
using MockupServer.Response;
using MockupServer.Storage;
using Newtonsoft.Json;

namespace MockupServer.UI
{
    public partial class InsertRecordForm : Form
    {
        readonly DataService _dataService;
        readonly MockupObject model;
        public InsertRecordForm(MockupObject defaultModel, DataService dataService)
        {
            _dataService = dataService;
            model = defaultModel;
            InitializeComponent();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtUrl.Text) || !TxtUrl.Text.StartsWith('/') || TxtUrl.Text.EndsWith('/'))
            {
                MessageBox.Show("url要以/开头，结尾不能带/");
                return;
            }

            if (ComboBox_Type.SelectedValue == null)
            {
                MessageBox.Show("请选择返回类型");
                return;
            }
            if ((ResponseType)ComboBox_Type.SelectedValue == ResponseType.JsonObject)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<object>(model.ResponseData ?? "");
                    if (obj == null)
                    {
                        MessageBox.Show("body应当是一个json对象");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            await _dataService.InserOrUpdateRecord(TxtUrl.Text, ComboBox_Method.Text, (int)ComboBox_Type.SelectedValue, model.ResponseData);
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void InsertRecordForm_Load(object sender, EventArgs e)
        {
            TxtUrl.Text = model.RequestUrl;

            ComboBox_Type.DataSource = new List<KeyValuePair<string, int>>()
            {
                new KeyValuePair<string, int>("空", (int)ResponseType.Empty),
                new KeyValuePair<string, int>("Json对象", (int)ResponseType.JsonObject),
                new KeyValuePair<string, int>("文本", (int)ResponseType.Text),
                new KeyValuePair<string, int>("文件", (int)ResponseType.File),
                new KeyValuePair<string, int>("Xml对象", (int)ResponseType.XmlObject),
            };
            ComboBox_Type.ValueMember = "Value";
            ComboBox_Type.DisplayMember = "Key";

            ComboBox_Method.SelectedItem = model.Method;
            ComboBox_Type.SelectedValue = model.ResponseType;
        }

        private void Btn_Edit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = new EditorForm(model.ResponseData, (ResponseType)ComboBox_Type.SelectedValue);
            form.OnSubmit += (obj, content) =>
            {
                model.ResponseData = content;
            };
            form.ShowDialog();
        }
    }
}
