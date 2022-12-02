using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockupServer.Configs;
using Newtonsoft.Json;

namespace MockupServer.UI
{
    public partial class Form1 : Form
    {
        ILogger<Form1> _logger;
        IHost? _webApp;
        public Form1()
        {
            InitializeComponent();
            SetFormStatus(false);
        }

        private void BtnMockupDataForm_Click(object sender, EventArgs e)
        {
            if (_webApp != null)
            {
                var form = new CachedRecordForm( _webApp);
                form.ShowDialog();
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtHost.Text) 
                || TxtHost.Text.EndsWith("/") 
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
                    BeginInvoke(() =>
                    {
                        BtnStart.Enabled = false;
                    });
                    var options = await SaveAndReadOption();
                    _webApp = WebServerService.Create(options.Port, options);
                    _logger = _webApp.Services.GetService<ILogger<Form1>>()!;
                    await _webApp.StartAsync();
                    BeginInvoke(() =>
                    {
                        MessageBox.Show("服务器已启动");
                        SetFormStatus(true);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    _webApp!.Dispose();
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
                OriginalServiceUrl = TxtHost.Text,
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
                    if (configs!= null)
                    {
                        BeginInvoke(() =>
                        {
                            TxtPort.Text = configs.Port.ToString();
                            TxtHost.Text = configs.OriginalServiceUrl;
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
            TxtHost.Enabled = !isStarted;

            BtnMockupDataForm.Enabled = isStarted;
        }

        private void TxtHost_Leave(object sender, EventArgs e)
        {
            //GerateUrlPrefix();
        }

        private void BtnStartRecord_Click(object sender, EventArgs e)
        {
            var nowState = State.ToggleRecord();
            BtnStartRecord.Text = nowState ? "停止录制" : "开始录制";
        }

        //private void GerateUrlPrefix()
        //{
        //    var indexStart = TxtHost.Text.IndexOf('/');
        //    if (indexStart > 0)
        //        TxtUrlPrefix.Text = TxtHost.Text.Substring(indexStart, TxtHost.Text.Length - indexStart);
        //}
    }
}