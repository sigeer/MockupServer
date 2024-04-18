namespace MockupServer.UI
{
    partial class DataEditForm
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
            label1 = new Label();
            TxtUrl = new TextBox();
            label2 = new Label();
            BtnSave = new Button();
            BtnCancel = new Button();
            label3 = new Label();
            ComboBox_Type = new ComboBox();
            label4 = new Label();
            ComboBox_Method = new ComboBox();
            Btn_Edit = new LinkLabel();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 26);
            label1.Name = "label1";
            label1.Size = new Size(25, 17);
            label1.TabIndex = 0;
            label1.Text = "Url";
            // 
            // TxtUrl
            // 
            TxtUrl.Location = new Point(75, 23);
            TxtUrl.Name = "TxtUrl";
            TxtUrl.Size = new Size(181, 23);
            TxtUrl.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 117);
            label2.Name = "label2";
            label2.Size = new Size(56, 17);
            label2.TabIndex = 2;
            label2.Text = "返回数据";
            // 
            // BtnSave
            // 
            BtnSave.Location = new Point(181, 147);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(75, 23);
            BtnSave.TabIndex = 4;
            BtnSave.Text = "保存";
            BtnSave.UseVisualStyleBackColor = true;
            BtnSave.Click += BtnSave_Click;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(100, 147);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "取消";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 86);
            label3.Name = "label3";
            label3.Size = new Size(56, 17);
            label3.TabIndex = 6;
            label3.Text = "返回类型";
            // 
            // ComboBox_Type
            // 
            ComboBox_Type.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBox_Type.FormattingEnabled = true;
            ComboBox_Type.Location = new Point(75, 83);
            ComboBox_Type.Name = "ComboBox_Type";
            ComboBox_Type.Size = new Size(180, 25);
            ComboBox_Type.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 55);
            label4.Name = "label4";
            label4.Size = new Size(56, 17);
            label4.TabIndex = 8;
            label4.Text = "请求方式";
            // 
            // ComboBox_Method
            // 
            ComboBox_Method.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBox_Method.FormattingEnabled = true;
            ComboBox_Method.Items.AddRange(new object[] { "GET", "POST", "PUT", "DELETE" });
            ComboBox_Method.Location = new Point(75, 52);
            ComboBox_Method.Name = "ComboBox_Method";
            ComboBox_Method.Size = new Size(181, 25);
            ComboBox_Method.TabIndex = 9;
            // 
            // Btn_Edit
            // 
            Btn_Edit.AutoSize = true;
            Btn_Edit.Location = new Point(75, 117);
            Btn_Edit.Name = "Btn_Edit";
            Btn_Edit.Size = new Size(56, 17);
            Btn_Edit.TabIndex = 11;
            Btn_Edit.TabStop = true;
            Btn_Edit.Text = "编辑内容";
            Btn_Edit.LinkClicked += Btn_Edit_LinkClicked;
            // 
            // DataEditForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(291, 182);
            Controls.Add(Btn_Edit);
            Controls.Add(ComboBox_Method);
            Controls.Add(label4);
            Controls.Add(ComboBox_Type);
            Controls.Add(label3);
            Controls.Add(BtnCancel);
            Controls.Add(BtnSave);
            Controls.Add(label2);
            Controls.Add(TxtUrl);
            Controls.Add(label1);
            Name = "DataEditForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "模拟数据 - 编辑";
            Load += InsertRecordForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox TxtUrl;
        private Label label2;
        private Button BtnSave;
        private Button BtnCancel;
        private Label label3;
        private ComboBox ComboBox_Type;
        private Label label4;
        private ComboBox ComboBox_Method;
        private LinkLabel Btn_Edit;
    }
}