namespace SGSclient
{
    partial class SGSClient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SGSClient));
            this.btnSend = new System.Windows.Forms.Button();
            this.txtChatBox = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.lstChatters = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.AccessibleDescription = null;
            this.btnSend.AccessibleName = null;
            resources.ApplyResources(this.btnSend, "btnSend");
            this.btnSend.BackgroundImage = null;
            this.btnSend.Font = null;
            this.btnSend.Name = "btnSend";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtChatBox
            // 
            this.txtChatBox.AccessibleDescription = null;
            this.txtChatBox.AccessibleName = null;
            resources.ApplyResources(this.txtChatBox, "txtChatBox");
            this.txtChatBox.BackColor = System.Drawing.SystemColors.Window;
            this.txtChatBox.BackgroundImage = null;
            this.txtChatBox.Font = null;
            this.txtChatBox.Name = "txtChatBox";
            this.txtChatBox.ReadOnly = true;
            // 
            // txtMessage
            // 
            this.txtMessage.AccessibleDescription = null;
            this.txtMessage.AccessibleName = null;
            resources.ApplyResources(this.txtMessage, "txtMessage");
            this.txtMessage.BackgroundImage = null;
            this.txtMessage.Font = null;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.TextChanged += new System.EventHandler(this.txtMessage_TextChanged);
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // lstChatters
            // 
            this.lstChatters.AccessibleDescription = null;
            this.lstChatters.AccessibleName = null;
            resources.ApplyResources(this.lstChatters, "lstChatters");
            this.lstChatters.BackgroundImage = null;
            this.lstChatters.Font = null;
            this.lstChatters.FormattingEnabled = true;
            this.lstChatters.Name = "lstChatters";
            // 
            // SGSClient
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.lstChatters);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtChatBox);
            this.Controls.Add(this.btnSend);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = null;
            this.MaximizeBox = false;
            this.Name = "SGSClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SGSClient_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtChatBox;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.ListBox lstChatters;
    }
}

