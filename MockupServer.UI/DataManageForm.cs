using MockupServer.Models;
using MockupServer.Response;
using MockupServer.Storage;
using Utility.Extensions;

namespace MockupServer.UI
{
    public partial class DataManageForm : Form
    {
        readonly DataService _coreService;
        public DataManageForm(DataService dataService)
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
            using var form = new DataEditForm(MockupObject.Empty(), _coreService);
            form.ShowDialog();
            LoadDataGridView();
        }

        private void DataManageForm_Load(object sender, EventArgs e)
        {
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "Url",
                Name = nameof(MockupObject.RequestUrl),
                Width = 500,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "请求方式",
                Name = nameof(MockupObject.Method),
                Width = 100,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            DataGridViewMain.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "返回类型",
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            MenuItem_CopyUrl.Visible = State.IsRunning;
        }

        private bool TryGetSelectedRow(out DataGridViewRow? selectedRow)
        {
            selectedRow = null;
            if (DataGridViewMain.SelectedCells.Count == 0)
                return false;

            var rowIndex = DataGridViewMain.SelectedCells[0].RowIndex;
            if (rowIndex < 0 || rowIndex >= DataGridViewMain.Rows.Count)
                return false;

            selectedRow = DataGridViewMain.Rows[rowIndex];
            return true;
        }

        private void LoadDataGridView()
        {
            var filter = TxtFilter.Text;
            Task.Run(async () =>
            {
                var total = await _coreService.GetDataList(filter);

                if (IsHandleCreated && !IsDisposed)
                {
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
                }
            });

        }

        private void MenuEdit_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRow(out var selectedRow) || selectedRow == null)
                return;

            var url = selectedRow.Cells[0].Value?.ToString();
            var method = selectedRow.Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(method))
                return;

            var type = Convert.ToInt32(selectedRow.Cells[2].Value);
            var responseData = selectedRow.Cells[3].Value?.ToString();
            using var form = new DataEditForm(new MockupObject(url, method, type, responseData), _coreService);
            form.ShowDialog();
            LoadDataGridView();
        }

        private async void MenuDelete_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRow(out var selectedRow) || selectedRow == null)
                return;

            var url = selectedRow.Cells[0].Value?.ToString();
            var method = selectedRow.Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(method))
                return;

            try
            {
                await _coreService.DeleteRecordAsync(url, method);
            }
            catch (Exception ex)
            {
                Alert.Show($"删除失败：{ex.Message}");
            }
            LoadDataGridView();
        }

        private async void MenuItem_CopyUrl_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRow(out var selectedRow) || selectedRow == null)
                return;

            var uri = selectedRow.Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(uri))
                return;

            try
            {
                var global = await _coreService.GetGlobal();
                Clipboard.SetText($"http://localhost:{global.ListenerPort}{uri}");
            }
            catch (Exception ex)
            {
                Alert.Show($"复制失败：{ex.Message}");
            }
        }

        private void DataGridViewMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2)
                return;

            var val = DataGridViewMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (val != null)
                e.Value = ((ResponseType)Convert.ToInt32(val)).GetDescription();
        }
    }
}
