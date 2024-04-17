namespace MockupServer.UI
{
    partial class CachedRecordForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewMain = new DataGridView();
            MenuData = new ContextMenuStrip(components);
            MenuEdit = new ToolStripMenuItem();
            MenuDelete = new ToolStripMenuItem();
            label1 = new Label();
            TxtFilter = new TextBox();
            BtnSearch = new Button();
            BtnAdd = new Button();
            ((System.ComponentModel.ISupportInitialize)DataGridViewMain).BeginInit();
            MenuData.SuspendLayout();
            SuspendLayout();
            // 
            // DataGridViewMain
            // 
            DataGridViewMain.AllowUserToAddRows = false;
            DataGridViewMain.AllowUserToDeleteRows = false;
            DataGridViewMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridViewMain.Location = new Point(12, 47);
            DataGridViewMain.Name = "DataGridViewMain";
            DataGridViewMain.ReadOnly = true;
            DataGridViewMain.RowTemplate.Height = 25;
            DataGridViewMain.Size = new Size(776, 359);
            DataGridViewMain.TabIndex = 0;
            DataGridViewMain.CellFormatting += DataGridViewMain_CellFormatting;
            // 
            // MenuData
            // 
            MenuData.Items.AddRange(new ToolStripItem[] { MenuEdit, MenuDelete });
            MenuData.Name = "MenuData";
            MenuData.Size = new Size(101, 48);
            // 
            // MenuEdit
            // 
            MenuEdit.Name = "MenuEdit";
            MenuEdit.Size = new Size(100, 22);
            MenuEdit.Text = "编辑";
            MenuEdit.Click += MenuEdit_Click;
            // 
            // MenuDelete
            // 
            MenuDelete.Name = "MenuDelete";
            MenuDelete.Size = new Size(100, 22);
            MenuDelete.Text = "删除";
            MenuDelete.Click += MenuDelete_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 21);
            label1.Name = "label1";
            label1.Size = new Size(32, 17);
            label1.TabIndex = 1;
            label1.Text = "筛选";
            // 
            // TxtFilter
            // 
            TxtFilter.Location = new Point(50, 18);
            TxtFilter.Name = "TxtFilter";
            TxtFilter.PlaceholderText = "支持正则表达式";
            TxtFilter.Size = new Size(243, 23);
            TxtFilter.TabIndex = 2;
            // 
            // BtnSearch
            // 
            BtnSearch.Location = new Point(299, 18);
            BtnSearch.Name = "BtnSearch";
            BtnSearch.Size = new Size(75, 23);
            BtnSearch.TabIndex = 3;
            BtnSearch.Text = "查询";
            BtnSearch.UseVisualStyleBackColor = true;
            BtnSearch.Click += BtnSearch_Click;
            // 
            // BtnAdd
            // 
            BtnAdd.Location = new Point(380, 18);
            BtnAdd.Name = "BtnAdd";
            BtnAdd.Size = new Size(75, 23);
            BtnAdd.TabIndex = 4;
            BtnAdd.Text = "新增";
            BtnAdd.UseVisualStyleBackColor = true;
            BtnAdd.Click += BtnAdd_Click;
            // 
            // CachedRecordForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 420);
            Controls.Add(BtnAdd);
            Controls.Add(BtnSearch);
            Controls.Add(TxtFilter);
            Controls.Add(label1);
            Controls.Add(DataGridViewMain);
            Name = "CachedRecordForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "模拟数据 - 管理";
            Load += CachedRecordForm_Load;
            ((System.ComponentModel.ISupportInitialize)DataGridViewMain).EndInit();
            MenuData.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView DataGridViewMain;
        private Label label1;
        private TextBox TxtFilter;
        private Button BtnSearch;
        private Button BtnAdd;
        private ContextMenuStrip MenuData;
        private ToolStripMenuItem MenuEdit;
        private ToolStripMenuItem MenuDelete;
    }
}