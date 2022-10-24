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
            this.components = new System.ComponentModel.Container();
            this.DataGridViewMain = new System.Windows.Forms.DataGridView();
            this.MenuData = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtFilter = new System.Windows.Forms.TextBox();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.BtnAdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewMain)).BeginInit();
            this.MenuData.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataGridViewMain
            // 
            this.DataGridViewMain.AllowUserToAddRows = false;
            this.DataGridViewMain.AllowUserToDeleteRows = false;
            this.DataGridViewMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridViewMain.Location = new System.Drawing.Point(12, 47);
            this.DataGridViewMain.Name = "DataGridViewMain";
            this.DataGridViewMain.ReadOnly = true;
            this.DataGridViewMain.RowTemplate.Height = 25;
            this.DataGridViewMain.Size = new System.Drawing.Size(776, 359);
            this.DataGridViewMain.TabIndex = 0;
            // 
            // MenuData
            // 
            this.MenuData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEdit,
            this.MenuDelete});
            this.MenuData.Name = "MenuData";
            this.MenuData.Size = new System.Drawing.Size(101, 48);
            // 
            // MenuEdit
            // 
            this.MenuEdit.Name = "MenuEdit";
            this.MenuEdit.Size = new System.Drawing.Size(100, 22);
            this.MenuEdit.Text = "编辑";
            this.MenuEdit.Click += new System.EventHandler(this.MenuEdit_Click);
            // 
            // MenuDelete
            // 
            this.MenuDelete.Name = "MenuDelete";
            this.MenuDelete.Size = new System.Drawing.Size(100, 22);
            this.MenuDelete.Text = "删除";
            this.MenuDelete.Click += new System.EventHandler(this.MenuDelete_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "筛选";
            // 
            // TxtFilter
            // 
            this.TxtFilter.Location = new System.Drawing.Point(50, 18);
            this.TxtFilter.Name = "TxtFilter";
            this.TxtFilter.PlaceholderText = "支持正则表达式";
            this.TxtFilter.Size = new System.Drawing.Size(243, 23);
            this.TxtFilter.TabIndex = 2;
            // 
            // BtnSearch
            // 
            this.BtnSearch.Location = new System.Drawing.Point(299, 18);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(75, 23);
            this.BtnSearch.TabIndex = 3;
            this.BtnSearch.Text = "查询";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // BtnAdd
            // 
            this.BtnAdd.Location = new System.Drawing.Point(380, 18);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(75, 23);
            this.BtnAdd.TabIndex = 4;
            this.BtnAdd.Text = "新增";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // CachedRecordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 420);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.BtnSearch);
            this.Controls.Add(this.TxtFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DataGridViewMain);
            this.Name = "CachedRecordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CachedRecordForm";
            this.Load += new System.EventHandler(this.CachedRecordForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewMain)).EndInit();
            this.MenuData.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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