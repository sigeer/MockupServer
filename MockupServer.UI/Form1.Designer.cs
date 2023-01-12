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
            this.label1 = new System.Windows.Forms.Label();
            this.TxtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtServer = new System.Windows.Forms.TextBox();
            this.BtnStart = new System.Windows.Forms.Button();
            this.BtnStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.BtnMockupDataForm = new System.Windows.Forms.Button();
            this.BtnStartRecord = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "本服务器使用的端口";
            // 
            // TxtPort
            // 
            this.TxtPort.Location = new System.Drawing.Point(134, 12);
            this.TxtPort.Name = "TxtPort";
            this.TxtPort.Size = new System.Drawing.Size(162, 23);
            this.TxtPort.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "目标服务器";
            // 
            // TxtServer
            // 
            this.TxtServer.Location = new System.Drawing.Point(86, 47);
            this.TxtServer.Name = "TxtServer";
            this.TxtServer.Size = new System.Drawing.Size(210, 23);
            this.TxtServer.TabIndex = 3;
            this.TxtServer.Leave += new System.EventHandler(this.TxtHost_Leave);
            // 
            // BtnStart
            // 
            this.BtnStart.Location = new System.Drawing.Point(12, 86);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(125, 23);
            this.BtnStart.TabIndex = 4;
            this.BtnStart.Text = "开始";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // BtnStop
            // 
            this.BtnStop.Location = new System.Drawing.Point(165, 86);
            this.BtnStop.Name = "BtnStop";
            this.BtnStop.Size = new System.Drawing.Size(131, 23);
            this.BtnStop.TabIndex = 5;
            this.BtnStop.Text = "停止";
            this.BtnStop.UseVisualStyleBackColor = true;
            this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(284, 44);
            this.label3.TabIndex = 6;
            this.label3.Text = "请求会优先使用假数据，如果没有则转发至目标服务器。";
            // 
            // BtnMockupDataForm
            // 
            this.BtnMockupDataForm.Location = new System.Drawing.Point(12, 209);
            this.BtnMockupDataForm.Name = "BtnMockupDataForm";
            this.BtnMockupDataForm.Size = new System.Drawing.Size(284, 23);
            this.BtnMockupDataForm.TabIndex = 7;
            this.BtnMockupDataForm.Text = "模拟数据表";
            this.BtnMockupDataForm.UseVisualStyleBackColor = true;
            this.BtnMockupDataForm.Click += new System.EventHandler(this.BtnMockupDataForm_Click);
            // 
            // BtnStartRecord
            // 
            this.BtnStartRecord.Location = new System.Drawing.Point(12, 124);
            this.BtnStartRecord.Name = "BtnStartRecord";
            this.BtnStartRecord.Size = new System.Drawing.Size(284, 23);
            this.BtnStartRecord.TabIndex = 8;
            this.BtnStartRecord.Text = "开始录制";
            this.BtnStartRecord.UseVisualStyleBackColor = true;
            this.BtnStartRecord.Click += new System.EventHandler(this.BtnStartRecord_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 245);
            this.Controls.Add(this.BtnStartRecord);
            this.Controls.Add(this.BtnMockupDataForm);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnStop);
            this.Controls.Add(this.BtnStart);
            this.Controls.Add(this.TxtServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TxtPort);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "测试数据服务器";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox TxtPort;
        private Label label2;
        private TextBox TxtServer;
        private Button BtnStart;
        private Button BtnStop;
        private Label label3;
        private Button BtnMockupDataForm;
        private Button BtnStartRecord;
    }
}