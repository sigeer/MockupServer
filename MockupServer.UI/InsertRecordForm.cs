using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockupServer.Configs;
using MockupServer.LocalDataSource;
using Newtonsoft.Json;

namespace MockupServer.UI
{
    public partial class InsertRecordForm : Form
    {
        readonly IHost _webApp;
        readonly MockupObject model;
        public InsertRecordForm(MockupObject defaultModel, IHost webApp)
        {
            _webApp = webApp;
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
            var service = _webApp.Services.GetService<MockupService>()!;
            var option = _webApp.Services.GetService<ServerOptions>()!;
            try
            {
                var obj = JsonConvert.DeserializeObject<object>(TxtBody.Text ?? "");
                if (obj == null)
                {
                    MessageBox.Show("body应当是一个json对象");
                    return;
                }
                await service.InserRecord(option.OriginalServiceUrl, TxtUrl.Text, TxtBody.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void InsertRecordForm_Load(object sender, EventArgs e)
        {
            TxtUrl.Text = model.RequestUrl;
            TxtBody.Text = model.ResponseData;
        }
    }
}
