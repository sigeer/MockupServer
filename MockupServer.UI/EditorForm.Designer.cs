namespace MockupServer.UI
{
    partial class EditorForm
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
            Btn_Submit = new Button();
            Btn_Cancel = new Button();
            Panel_Editor = new Panel();
            SuspendLayout();
            // 
            // Btn_Submit
            // 
            Btn_Submit.Location = new Point(713, 415);
            Btn_Submit.Name = "Btn_Submit";
            Btn_Submit.Size = new Size(75, 23);
            Btn_Submit.TabIndex = 0;
            Btn_Submit.Text = "确定";
            Btn_Submit.UseVisualStyleBackColor = true;
            Btn_Submit.Click += Btn_Submit_Click;
            // 
            // Btn_Cancel
            // 
            Btn_Cancel.Location = new Point(632, 415);
            Btn_Cancel.Name = "Btn_Cancel";
            Btn_Cancel.Size = new Size(75, 23);
            Btn_Cancel.TabIndex = 1;
            Btn_Cancel.Text = "取消";
            Btn_Cancel.UseVisualStyleBackColor = true;
            Btn_Cancel.Click += Btn_Cancel_Click;
            // 
            // Panel_Editor
            // 
            Panel_Editor.Location = new Point(0, 0);
            Panel_Editor.Name = "Panel_Editor";
            Panel_Editor.Size = new Size(802, 409);
            Panel_Editor.TabIndex = 2;
            // 
            // EditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Panel_Editor);
            Controls.Add(Btn_Cancel);
            Controls.Add(Btn_Submit);
            Name = "EditorForm";
            Text = "编辑器";
            ResumeLayout(false);
        }

        #endregion

        private Button Btn_Submit;
        private Button Btn_Cancel;
        private Panel Panel_Editor;
    }
}