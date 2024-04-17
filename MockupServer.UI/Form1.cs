using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockupServer.Configs;
using MockupServer.Storage;
using Newtonsoft.Json;

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
            var form = new CachedRecordForm(_dataService);
            form.ShowDialog();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtServer.Text)
                || TxtServer.Text.EndsWith("/")
                || string.IsNullOrEmpty(TxtPort.Text)
                || !int.TryParse(TxtPort.Text, out int port))
            {
                MessageBox.Show("请完成所有内容");
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
                    await _webApp.StartAsync();
                    State.IsRunning = true;
                    BeginInvoke(() =>
                    {
                        MessageBox.Show("服务器已启动");
                        SetFormStatus(true);
                    });
                }
                catch (Exception ex)
                {
                    Invoke(() =>
                    {
                        BtnStart.Enabled = true;
                    });
                    _logger.LogError(ex.ToString());
                    _webApp!.Dispose();
                    State.IsRunning = false;
                    var err = ex.ToString();
                    MessageBox.Show(err.Length >= 200 ? err.Substring(0, 200) : err);
                }
            });

        }

        private async Task<ServerOptions> SaveAndReadOption()
        {
            int.TryParse(TxtPort.Text, out int port);
            var option = new ServerOptions()
            {
                OriginalServiceUrl = TxtServer.Text,
                Port = port,
            };
            await File.WriteAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServerSettings.SettingFileName), JsonConvert.SerializeObject(option));
            return option;
        }

        private async void BtnStop_Click(object sender, EventArgs e)
        {
            if (_webApp != null)
            {
                await _webApp.StopAsync();
                _webApp.Dispose();
                State.IsRunning = false;
                MessageBox.Show("服务器已停止");
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
            var settingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServerSettings.SettingFileName);
            if (File.Exists(settingFile))
            {
                var configStr = await File.ReadAllTextAsync(settingFile);
                if (!string.IsNullOrEmpty(configStr))
                {
                    var configs = JsonConvert.DeserializeObject<ServerOptions>(configStr);
                    if (configs != null)
                    {
                        BeginInvoke(() =>
                        {
                            TxtPort.Text = configs.Port.ToString();
                            TxtServer.Text = configs.OriginalServiceUrl;
                        });
                    }
                }
            }
        }

        private void SetFormStatus(bool isStarted)
        {
            BtnStart.Enabled = !isStarted;
            BtnStop.Enabled = isStarted;

            TxtPort.Enabled = !isStarted;
            TxtServer.Enabled = !isStarted;
        }

        private void TxtHost_Leave(object sender, EventArgs e)
        {
            //GerateUrlPrefix();
        }

        //private void GerateUrlPrefix()
        //{
        //    var indexStart = TxtHost.Text.IndexOf('/');
        //    if (indexStart > 0)
        //        TxtUrlPrefix.Text = TxtHost.Text.Substring(indexStart, TxtHost.Text.Length - indexStart);
        //}
    }
}