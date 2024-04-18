using MockupServer.Models;
using MockupServer.Response;
using MockupServer.Storage;
using Newtonsoft.Json;

namespace MockupServer.UI
{
    public partial class DataEditForm : Form
    {
        readonly DataService _dataService;
        readonly MockupObject model;
        public DataEditForm(MockupObject defaultModel, DataService dataService)
        {
            _dataService = dataService;
            model = defaultModel;
            InitializeComponent();
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUrl.Text))
            {
                Alert.Show("请输入Url");
                return;
            }

            if (!TxtUrl.Text.StartsWith('/') && !TxtUrl.Text.StartsWith("http"))
            {
                Alert.Show("Url要以\"/\"，或者\"http\"开头");
                return;
            }

            if (TxtUrl.Text.EndsWith('/'))
            {
                Alert.Show("Url不能以\"/\"结尾");
                return;
            }

            if (ComboBox_Type.SelectedValue == null)
            {
                Alert.Show("请选择返回类型");
                return;
            }
            if ((ResponseType)ComboBox_Type.SelectedValue == ResponseType.JsonObject)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<object>(model.ResponseData ?? "");
                    if (obj == null)
                    {
                        Alert.Show("body应当是一个json对象");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Alert.Show(ex.Message);
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
            var selectedType = (ResponseType)ComboBox_Type.SelectedValue;
            if (selectedType == ResponseType.File)
            {
                var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    model.ResponseData = ofd.FileName;
                }
                return;
            }
            var form = new EditorForm(model.ResponseData, selectedType);
            form.OnSubmit += (obj, content) =>
            {
                model.ResponseData = content;
            };
            form.ShowDialog();
        }
    }
}
