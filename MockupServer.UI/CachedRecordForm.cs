using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockupServer.Configs;
using MockupServer.LocalDataSource;
using MockupServer.Models;
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
    public partial class CachedRecordForm : Form
    {
        readonly IHost _webApp;
        readonly MockupService _coreService;
        readonly ServerOptions _options;
        public CachedRecordForm(IHost host)
        {
            _webApp = host;
            _coreService = _webApp.Services.GetService<MockupService>()!;
            _options = _webApp.Services.GetService<ServerOptions>()!;
            InitializeComponent();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadDataGridView();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new InsertRecordForm(new MockupObject(), _webApp);
            form.ShowDialog();
            LoadDataGridView();
        }

        private void CachedRecordForm_Load(object sender, EventArgs e)
        {
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "Url",
                Name = nameof(MockupObject.RequestUrl),
                Width = 200,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "Response Body",
                Name = nameof(MockupObject.ResponseData),
                Width = 400,
                CellTemplate = new DataGridViewTextBoxCell()
            });
            LoadDataGridView();
        }

        private void LoadDataGridView()
        {
            Task.Run(async () =>
            {
                var total = await _coreService.GetDataList(_options.OriginalServiceUrl, TxtFilter.Text);

                BeginInvoke(() =>
                {
                    DataGridViewMain.Rows.Clear();
                    total.ForEach(item =>
                    {
                        var row = new DataGridViewRow();
                        row.Cells.Add(new DataGridViewTextBoxCell()
                        {
                            Value = item.RequestUrl,
                            ValueType = typeof(string),
                        });
                        row.Cells.Add(new DataGridViewTextBoxCell()
                        {
                            Value = item.ResponseData,
                            ValueType = typeof(string),
                        });
                        row.ContextMenuStrip = MenuData;
                        DataGridViewMain.Rows.Add(row);
                    });
                });
            });

        }

        private void MenuEdit_Click(object sender, EventArgs e)
        {
            var selectedRow = DataGridViewMain.Rows[DataGridViewMain.SelectedCells[0].RowIndex];
            new InsertRecordForm(new MockupObject { RequestUrl = selectedRow.Cells[0].Value.ToString(), ResponseData = selectedRow.Cells[1].Value.ToString() }, _webApp).ShowDialog();
            LoadDataGridView();
        }

        private async void MenuDelete_Click(object sender, EventArgs e)
        {
            var selectedRow = DataGridViewMain.Rows[DataGridViewMain.SelectedCells[0].RowIndex];
            await _coreService.DeleteRecordAsync(_options.OriginalServiceUrl, selectedRow.Cells[0].Value.ToString());
            LoadDataGridView();
        }
    }
}
