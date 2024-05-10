using MockupServer.Models;
using MockupServer.Storage;

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
                HeaderText = "域",
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

            if (!urlPrefix.EndsWith('/') && !target.EndsWith('/'))
            {
                // 避免出现 /abcdef 被 /abc 域处理的情况
                MessageBox.Show("需要以/结尾");
                return;
            }

            var model = new Domain(urlPrefix, target);
            await _dataService.InsertOrUpdateDomain(model);
        }

        private async void DataGridViewMain_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            var urlPrefix = e.Row.Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(urlPrefix))
                return;

            await _dataService.DeleteDomain(urlPrefix);
        }
    }
}
