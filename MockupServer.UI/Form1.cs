using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockupServer.Models;
using MockupServer.Storage;

namespace MockupServer.UI
{
    public partial class Form1 : Form
    {
        ILogger<Form1> _logger;
        IHost? _webApp;
        DataService _dataService;
        public Form1(ILogger<Form1> logger, DataService dataService)
        {
            _logger = logger;
            _dataService = dataService;

            InitializeComponent();
            SetFormStatus(false);
        }

        private void BtnMockupDataForm_Click(object sender, EventArgs e)
        {
            var form = new DataManageForm(_dataService);
            form.ShowDialog();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtPort.Text)
                || !int.TryParse(TxtPort.Text, out int port))
            {
                Alert.Show("请完成所有内容");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    Invoke(() =>
                    {
                        BtnStart.Enabled = false;
                    });
                    var options = await SaveAndReadOption();

                    _webApp = WebServerService.Create(options);
                    await _webApp.StartServerAsync();
                    BeginInvoke(() =>
                    {
                        Alert.Show("服务器已启动");
                        Label_ServerInfo.Text = $"http://localhost:{options.ListenerPort}";
                        SetFormStatus(true);
                    });
                }
                catch (Exception ex)
                {
                    Invoke(() =>
                    {
                        BtnStart.Enabled = true;
                        Label_ServerInfo.Text = "";
                    });
                    _logger.LogError(ex.ToString());
                    await _webApp.StopServerAysnc();
                    Label_ServerInfo.Text = "";
                    Alert.Show(ex.ToString());
                }
            });

        }

        private async Task<GlobalCache> SaveAndReadOption()
        {
            if (int.TryParse(TxtPort.Text, out int port))
            {
                var model = new GlobalCache()
                {
                    ListenerPort = port
                };
                return await _dataService.SaveGlobal(model);
            }
            return new GlobalCache();
        }

        private async void BtnStop_Click(object sender, EventArgs e)
        {
            if (_webApp != null)
            {
                await _webApp.StopServerAysnc();
                _webApp = null;
                Alert.Show("服务器已停止");
                Label_ServerInfo.Text = "";
                SetFormStatus(false);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                await ReadCacheData();
            });
        }

        private async Task ReadCacheData()
        {
            var global = await _dataService.GetGlobal();

            BeginInvoke(() =>
            {
                TxtPort.Text = global.ListenerPort.ToString();
            });
        }

        private void SetFormStatus(bool isStarted)
        {
            BtnStart.Enabled = !isStarted;
            BtnStop.Enabled = isStarted;

            TxtPort.Enabled = !isStarted;
            // TxtServer.Enabled = !isStarted;
        }

        private void Btn_DomainManage_Click(object sender, EventArgs e)
        {
            new DomainManageForm(_dataService).ShowDialog();
        }
    }
}