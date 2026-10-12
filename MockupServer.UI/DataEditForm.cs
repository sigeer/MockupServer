using MockupServer.Models;
using MockupServer.Response;
using MockupServer.Storage;
using Newtonsoft.Json;
using Utility.Extensions;

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
            try
            {
                if (string.IsNullOrWhiteSpace(TxtUrl.Text))
                {
                    Alert.Show("请输入Url");
                    return;
                }

                if (!TxtUrl.Text.StartsWith('/'))
                {
                    Alert.Show("Url要以\"/\"开头");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ComboBox_Method.Text))
                {
                    Alert.Show("请选择请求方式");
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
                await _dataService.InsertOrUpdateRecord(TxtUrl.Text, ComboBox_Method.Text, (int)ComboBox_Type.SelectedValue, model.ResponseData);
                Close();
            }
            catch (Exception ex)
            {
                Alert.Show($"保存失败：{ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DataEditForm_Load(object sender, EventArgs e)
        {
            TxtUrl.Text = model.RequestUrl;

            var typeList = Enum.GetValues<ResponseType>();

            // 需要先设置Member， 再设置DataSouce --设置ValueMember，DataSource都会触发SelectedValueChanged
            ComboBox_Type.ValueMember = "Value";
            ComboBox_Type.DisplayMember = "Key";
            ComboBox_Type.DataSource = typeList.Select(x => new KeyValuePair<string, int>(x.GetDescription(), (int)x)).ToList();
            ComboBox_Type.SelectedValue = model.ResponseType;

            ComboBox_Method.SelectedItem = model.Method;
            SetBtnEditText();
        }

        private void Btn_Edit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var selectedType = (ResponseType)ComboBox_Type.SelectedValue!;
            if (selectedType == ResponseType.File)
            {
                using var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    model.ResponseData = ofd.FileName;
                }
                return;
            }
            using var form = new EditorForm(model.ResponseData, selectedType);
            form.OnSubmit += (obj, content) =>
            {
                model.ResponseData = content;
            };
            form.ShowDialog();
        }

        private void ComboBox_Type_SelectedValueChanged(object sender, EventArgs e)
        {
            SetBtnEditText();
        }

        private void SetBtnEditText()
        {
            if (ComboBox_Type.SelectedValue == null)
                return;

            if ((ResponseType)ComboBox_Type.SelectedValue! == ResponseType.File)
            {
                Btn_Edit.Text = "选择文件";
            }
            else
            {
                Btn_Edit.Text = "编辑内容";
            }
        }

    }
}
