namespace MockupServer.UI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            TxtPort = new TextBox();
            BtnStart = new Button();
            BtnStop = new Button();
            label3 = new Label();
            BtnMockupDataForm = new Button();
            Btn_DomainManage = new Button();
            Label_ServerInfo = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(24, 15);
            label1.Name = "label1";
            label1.Size = new Size(32, 17);
            label1.TabIndex = 0;
            label1.Text = "端口";
            // 
            // TxtPort
            // 
            TxtPort.Location = new Point(62, 12);
            TxtPort.Name = "TxtPort";
            TxtPort.Size = new Size(75, 23);
            TxtPort.TabIndex = 1;
            // 
            // BtnStart
            // 
            BtnStart.Location = new Point(12, 86);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(125, 23);
            BtnStart.TabIndex = 4;
            BtnStart.Text = "开始";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // BtnStop
            // 
            BtnStop.Location = new Point(165, 86);
            BtnStop.Name = "BtnStop";
            BtnStop.Size = new Size(131, 23);
            BtnStop.TabIndex = 5;
            BtnStop.Text = "停止";
            BtnStop.UseVisualStyleBackColor = true;
            BtnStop.Click += BtnStop_Click;
            // 
            // label3
            // 
            label3.Location = new Point(12, 121);
            label3.Name = "label3";
            label3.Size = new Size(284, 44);
            label3.TabIndex = 6;
            label3.Text = "请求会优先使用模拟数据，如果没有则转发至目标服务器。";
            // 
            // BtnMockupDataForm
            // 
            BtnMockupDataForm.Location = new Point(12, 168);
            BtnMockupDataForm.Name = "BtnMockupDataForm";
            BtnMockupDataForm.Size = new Size(284, 23);
            BtnMockupDataForm.TabIndex = 7;
            BtnMockupDataForm.Text = "模拟数据表";
            BtnMockupDataForm.UseVisualStyleBackColor = true;
            BtnMockupDataForm.Click += BtnMockupDataForm_Click;
            // 
            // Btn_DomainManage
            // 
            Btn_DomainManage.Location = new Point(12, 49);
            Btn_DomainManage.Name = "Btn_DomainManage";
            Btn_DomainManage.Size = new Size(284, 23);
            Btn_DomainManage.TabIndex = 9;
            Btn_DomainManage.Text = "设置域";
            Btn_DomainManage.UseVisualStyleBackColor = true;
            Btn_DomainManage.Click += Btn_DomainManage_Click;
            // 
            // Label_ServerInfo
            // 
            Label_ServerInfo.AutoSize = true;
            Label_ServerInfo.Location = new Point(143, 15);
            Label_ServerInfo.Name = "Label_ServerInfo";
            Label_ServerInfo.Size = new Size(0, 17);
            Label_ServerInfo.TabIndex = 10;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(317, 207);
            Controls.Add(Label_ServerInfo);
            Controls.Add(Btn_DomainManage);
            Controls.Add(BtnMockupDataForm);
            Controls.Add(label3);
            Controls.Add(BtnStop);
            Controls.Add(BtnStart);
            Controls.Add(TxtPort);
            Controls.Add(label1);
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "测试数据服务器";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox TxtPort;
        private Button BtnStart;
        private Button BtnStop;
        private Label label3;
        private Button BtnMockupDataForm;
        private Button Btn_DomainManage;
        private Label Label_ServerInfo;
    }
}