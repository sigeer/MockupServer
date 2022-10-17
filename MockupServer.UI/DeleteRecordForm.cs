using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockupServer.Configs;
using MockupServer.LocalDataSource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MockupServer.UI
{
    public partial class DeleteRecordForm : Form
    {
        readonly IHost _webApp;
        public DeleteRecordForm(IHost host)
        {
            _webApp = host;
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            var service = _webApp.Services.GetService<MockupService>();
            if (service != null)
            {
                var option = _webApp.Services.GetService<ServerOptions>()!;
                await service.DeleteRecordAsync(option.OriginalServiceUrl, TxtUrl.Text);
                Close();
            }
        }
    }
}
