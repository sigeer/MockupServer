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
        GlobalCache? _global;
        public Form1(ILogger<Form1> logger, DataService dataService)
        {
            _logger = logger;
            _dataService = dataService;

            InitializeComponent();
            SetFormStatus(false);
            _logger.LogInformation("程序已启动");
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
                    _global = await SaveAndReadOption();

                    _webApp = WebServerService.Create(_global);
                    await _webApp.StartServerAsync();
                    BeginInvoke(() =>
                    {
                        Alert.Show("服务器已启动");
                        SetFormStatus(true);
                    });
                }
                catch (Exception ex)
                {
                    Invoke(() =>
                    {
                        SetFormStatus(false);
                    });
                    _logger.LogError(ex.ToString());
                    await _webApp.StopServerAysnc();
                    Alert.Show(ex.ToString());
                }
            });

        }

        private async Task<GlobalCache> SaveAndReadOption()
        {
            _global ??= await _dataService.GetGlobal();
            if (int.TryParse(TxtPort.Text, out int port))
            {
                _global.ListenerPort = port;
                await _dataService.SaveGlobal(_global);
            }
            return _global;
        }

        private async void BtnStop_Click(object sender, EventArgs e)
        {
            if (_webApp != null)
            {
                await _webApp.StopServerAysnc();
                _webApp = null;
                Alert.Show("服务器已停止");
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
            _global = await _dataService.GetGlobal();

            BeginInvoke(() =>
            {
                TxtPort.Text = _global.ListenerPort.ToString();
            });
        }

        private void SetFormStatus(bool isStarted)
        {
            BtnStart.Enabled = !isStarted;
            BtnStop.Enabled = isStarted;

            TxtPort.Enabled = !isStarted;
            Label_ServerInfo.Text = isStarted ? $"http://localhost:{_global?.ListenerPort}": "";
            // TxtServer.Enabled = !isStarted;
        }

        private void Btn_DomainManage_Click(object sender, EventArgs e)
        {
            new DomainManageForm(_dataService).ShowDialog();
        }
    }
}