using MockupServer.Models;
using MockupServer.Response;
using MockupServer.Storage;
using Utility.Extensions;

namespace MockupServer.UI
{
    public partial class CachedRecordForm : Form
    {
        readonly DataService _coreService;
        public CachedRecordForm(DataService dataService)
        {
            _coreService = dataService;
            InitializeComponent();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadDataGridView();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new InsertRecordForm(MockupObject.Empty(), _coreService);
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
                HeaderText = "请求方式",
                Name = nameof(MockupObject.Method),
                Width = 200,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "请求类型",
                Name = nameof(MockupObject.ResponseType),
                Width = 200,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "返回数据",
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
                var total = await _coreService.GetDataList(TxtFilter.Text);

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
                            Value = item.Method,
                            ValueType = typeof(string),
                        });
                        row.Cells.Add(new DataGridViewTextBoxCell()
                        {
                            Value = item.ResponseType,
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
            var url = selectedRow.Cells[0].Value.ToString();
            var method = selectedRow.Cells[1].Value.ToString();
            var type = (int)selectedRow.Cells[2].Value;
            var responseData = selectedRow.Cells[3].Value.ToString();
            new InsertRecordForm(new MockupObject(url, method, type, responseData), _coreService).ShowDialog();
            LoadDataGridView();
        }

        private async void MenuDelete_Click(object sender, EventArgs e)
        {
            var selectedRow = DataGridViewMain.Rows[DataGridViewMain.SelectedCells[0].RowIndex];
            await _coreService.DeleteRecordAsync(selectedRow.Cells[0].Value.ToString(), selectedRow.Cells[1].Value.ToString());
            LoadDataGridView();
        }

        private void DataGridViewMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var val = DataGridViewMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 2)
            {
                e.Value = ((ResponseType)val).GetDescription();
            }
        }
    }
}
