namespace MockupServer.UI
{
    partial class DomainManageForm
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
            DataGridViewMain = new DataGridView();
            Btn_Close = new Button();
            ((System.ComponentModel.ISupportInitialize)DataGridViewMain).BeginInit();
            SuspendLayout();
            // 
            // DataGridViewMain
            // 
            DataGridViewMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridViewMain.Location = new Point(12, 12);
            DataGridViewMain.Name = "DataGridViewMain";
            DataGridViewMain.RowTemplate.Height = 25;
            DataGridViewMain.Size = new Size(558, 256);
            DataGridViewMain.TabIndex = 0;
            DataGridViewMain.CellEndEdit += DataGridViewMain_CellEndEdit;
            // 
            // Btn_Close
            // 
            Btn_Close.Location = new Point(495, 274);
            Btn_Close.Name = "Btn_Close";
            Btn_Close.Size = new Size(75, 23);
            Btn_Close.TabIndex = 1;
            Btn_Close.Text = "关闭";
            Btn_Close.UseVisualStyleBackColor = true;
            Btn_Close.Click += Btn_Close_Click;
            // 
            // DomainManageForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(582, 309);
            Controls.Add(Btn_Close);
            Controls.Add(DataGridViewMain);
            Name = "DomainManageForm";
            Text = "设置域";
            Load += DomainManageForm_Load;
            ((System.ComponentModel.ISupportInitialize)DataGridViewMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView DataGridViewMain;
        private Button Btn_Close;
    }
}