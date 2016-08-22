namespace DiceBot
{
    partial class Startup
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
            this.rtbNews = new System.Windows.Forms.RichTextBox();
            this.btnSite = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.chkDontShow = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // rtbNews
            // 
            this.rtbNews.Location = new System.Drawing.Point(12, 12);
            this.rtbNews.Name = "rtbNews";
            this.rtbNews.Size = new System.Drawing.Size(260, 206);
            this.rtbNews.TabIndex = 0;
            this.rtbNews.Text = "";
            // 
            // btnSite
            // 
            this.btnSite.Location = new System.Drawing.Point(12, 224);
            this.btnSite.Name = "btnSite";
            this.btnSite.Size = new System.Drawing.Size(75, 23);
            this.btnSite.TabIndex = 1;
            this.btnSite.Text = "View Page";
            this.btnSite.UseVisualStyleBackColor = true;
            this.btnSite.Visible = false;
            this.btnSite.Click += new System.EventHandler(this.btnSite_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(197, 224);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // chkDontShow
            // 
            this.chkDontShow.AutoSize = true;
            this.chkDontShow.Location = new System.Drawing.Point(143, 253);
            this.chkDontShow.Name = "chkDontShow";
            this.chkDontShow.Size = new System.Drawing.Size(129, 17);
            this.chkDontShow.TabIndex = 4;
            this.chkDontShow.Text = "Don\'t Show this again";
            this.chkDontShow.UseVisualStyleBackColor = true;
            // 
            // Startup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 278);
            this.Controls.Add(this.chkDontShow);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSite);
            this.Controls.Add(this.rtbNews);
            this.Name = "Startup";
            this.Text = "Daily News/Tip";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbNews;
        private System.Windows.Forms.Button btnSite;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.CheckBox chkDontShow;
    }
}