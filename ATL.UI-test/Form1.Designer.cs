namespace ATL.UI_test
{
    partial class Form1
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
            this.GoAsyncProgressBtn = new System.Windows.Forms.Button();
            this.ProgressLbl = new System.Windows.Forms.Label();
            this.GoSyncBtn = new System.Windows.Forms.Button();
            this.GoAsyncBtn = new System.Windows.Forms.Button();
            this.GoSyncProgressBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // GoAsyncProgressBtn
            // 
            this.GoAsyncProgressBtn.Location = new System.Drawing.Point(12, 56);
            this.GoAsyncProgressBtn.Name = "GoAsyncProgressBtn";
            this.GoAsyncProgressBtn.Size = new System.Drawing.Size(136, 23);
            this.GoAsyncProgressBtn.TabIndex = 0;
            this.GoAsyncProgressBtn.Text = "ASYNC / PROGRESS";
            this.GoAsyncProgressBtn.UseVisualStyleBackColor = true;
            this.GoAsyncProgressBtn.Click += new System.EventHandler(this.GoAsyncProgressBtn_Click);
            // 
            // ProgressLbl
            // 
            this.ProgressLbl.AutoSize = true;
            this.ProgressLbl.Location = new System.Drawing.Point(12, 152);
            this.ProgressLbl.Name = "ProgressLbl";
            this.ProgressLbl.Size = new System.Drawing.Size(55, 13);
            this.ProgressLbl.TabIndex = 1;
            this.ProgressLbl.Text = "progress%";
            this.ProgressLbl.Visible = false;
            // 
            // GoSyncBtn
            // 
            this.GoSyncBtn.Location = new System.Drawing.Point(12, 24);
            this.GoSyncBtn.Name = "GoSyncBtn";
            this.GoSyncBtn.Size = new System.Drawing.Size(136, 23);
            this.GoSyncBtn.TabIndex = 2;
            this.GoSyncBtn.Text = "SYNC";
            this.GoSyncBtn.UseVisualStyleBackColor = true;
            this.GoSyncBtn.Click += new System.EventHandler(this.GoSyncBtn_Click);
            // 
            // GoAsyncBtn
            // 
            this.GoAsyncBtn.Location = new System.Drawing.Point(12, 82);
            this.GoAsyncBtn.Name = "GoAsyncBtn";
            this.GoAsyncBtn.Size = new System.Drawing.Size(136, 23);
            this.GoAsyncBtn.TabIndex = 3;
            this.GoAsyncBtn.Text = "ASYNC";
            this.GoAsyncBtn.UseVisualStyleBackColor = true;
            this.GoAsyncBtn.Click += new System.EventHandler(this.GoAsyncBtn_Click);
            // 
            // GoSyncProgressBtn
            // 
            this.GoSyncProgressBtn.Location = new System.Drawing.Point(15, 126);
            this.GoSyncProgressBtn.Name = "GoSyncProgressBtn";
            this.GoSyncProgressBtn.Size = new System.Drawing.Size(136, 23);
            this.GoSyncProgressBtn.TabIndex = 4;
            this.GoSyncProgressBtn.Text = "SYNC / PROGRESS";
            this.GoSyncProgressBtn.UseVisualStyleBackColor = true;
            this.GoSyncProgressBtn.Click += new System.EventHandler(this.GoSyncProgressBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(226, 216);
            this.Controls.Add(this.GoSyncProgressBtn);
            this.Controls.Add(this.GoAsyncBtn);
            this.Controls.Add(this.GoSyncBtn);
            this.Controls.Add(this.ProgressLbl);
            this.Controls.Add(this.GoAsyncProgressBtn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GoAsyncProgressBtn;
        private System.Windows.Forms.Label ProgressLbl;
        private System.Windows.Forms.Button GoSyncBtn;
        private System.Windows.Forms.Button GoAsyncBtn;
        private System.Windows.Forms.Button GoSyncProgressBtn;
    }
}

