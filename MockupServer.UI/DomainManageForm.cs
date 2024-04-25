using MockupServer.Models;
using MockupServer.Storage;
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
    public partial class DomainManageForm : Form
    {
        readonly DataService _dataService;
        public DomainManageForm(DataService dataService)
        {
            _dataService = dataService;

            InitializeComponent();
        }

        private void Btn_Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DomainManageForm_Load(object sender, EventArgs e)
        {
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "请求方式",
                Name = nameof(Domain.UrlPrefix),
                Width = 200,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "目标",
                Name = nameof(Domain.Target),
                Width = 200,
                CellTemplate = new DataGridViewTextBoxCell(),
            });

            LoadGridData();
        }

        private void LoadGridData()
        {
            Task.Run(async () =>
            {
                var list = await _dataService.GetDomainList();

                BeginInvoke(() =>
                {
                    DataGridViewMain.Rows.Clear();
                    list.ForEach(item =>
                    {
                        var row = new DataGridViewRow();
                        row.Cells.Add(new DataGridViewTextBoxCell()
                        {
                            Value = item.UrlPrefix,
                            ValueType = typeof(string),
                        });
                        row.Cells.Add(new DataGridViewTextBoxCell()
                        {
                            Value = item.Target,
                            ValueType = typeof(string),
                        });
                        DataGridViewMain.Rows.Add(row);
                        row.Cells[0].ReadOnly = true;
                    });
                });
            });

        }

        private async void DataGridViewMain_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = DataGridViewMain.Rows[e.RowIndex];

            var urlPrefix = row.Cells[0].Value?.ToString();
            var target = row.Cells[1].Value?.ToString();
            if (string.IsNullOrWhiteSpace(urlPrefix) || string.IsNullOrWhiteSpace(target))
            {
                return;
            }

            var model = new Domain(urlPrefix, target);
            await _dataService.InsertOrUpdateDomain(model);
        }
    }
}
