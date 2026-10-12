using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockupServer.Models;
using MockupServer.Storage;
using Serilog;

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
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                _webApp?.StopServerAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关闭时停止服务器失败");
            }
            finally
            {
                _webApp = null;
                Log.CloseAndFlush();
            }
        }

        private void BtnMockupDataForm_Click(object sender, EventArgs e)
        {
            using var form = new DataManageForm(_dataService);
            form.ShowDialog();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtPort.Text)
                || !int.TryParse(TxtPort.Text, out var port))
            {
                Alert.Show("请完成所有内容");
                return;
            }

            // UI 线程先禁用按钮，防止双击启动多个 host
            BtnStart.Enabled = false;

            Task.Run(async () =>
            {
                IHost? host = null;
                try
                {
                    _global = await SaveAndReadOption(port);

                    host = WebServerService.Create(_global);
                    await host.StartServerAsync();
                    BeginInvoke(() =>
                    {
                        _webApp = host;
                        Alert.Show("服务器已启动");
                        SetFormStatus(true);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "启动服务器失败");
                    if (host != null)
                    {
                        try { await host.StopServerAsync(); } catch { /* 启动失败时尽力停止 */ }
                    }
                    if (IsHandleCreated && !IsDisposed)
                    {
                        BeginInvoke(() =>
                        {
                            SetFormStatus(false);
                            Alert.Show(ex.ToString());
                        });
                    }
                }
            });
        }

        private async Task<GlobalCache> SaveAndReadOption(int port)
        {
            _global ??= await _dataService.GetGlobal();
            _global.ListenerPort = port;
            await _dataService.SaveGlobal(_global);
            return _global;
        }

        private async void BtnStop_Click(object sender, EventArgs e)
        {
            if (_webApp == null)
                return;

            try
            {
                var host = _webApp;
                _webApp = null;
                await host.StopServerAsync();
                Alert.Show("服务器已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止服务器失败");
                Alert.Show(ex.ToString());
            }
            finally
            {
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

            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(() =>
                {
                    TxtPort.Text = _global.ListenerPort.ToString();
                });
            }
        }

        private void SetFormStatus(bool isStarted)
        {
            BtnStart.Enabled = !isStarted;
            BtnStop.Enabled = isStarted;

            TxtPort.Enabled = !isStarted;
            Label_ServerInfo.Text = isStarted ? $"http://localhost:{_global?.ListenerPort}" : "";
        }

        private void Btn_DomainManage_Click(object sender, EventArgs e)
        {
            using var form = new DomainManageForm(_dataService);
            form.ShowDialog();
        }
    }
}
