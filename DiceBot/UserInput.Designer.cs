namespace DiceBot
{
    partial class UserInput
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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.nudNumInput = new System.Windows.Forms.NumericUpDown();
            this.txtTextInput = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumInput)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(12, 9);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(40, 13);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "Prompt";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(291, 82);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // nudNumInput
            // 
            this.nudNumInput.Location = new System.Drawing.Point(12, 56);
            this.nudNumInput.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
            this.nudNumInput.Minimum = new decimal(new int[] {
            1215752192,
            23,
            0,
            -2147483648});
            this.nudNumInput.Name = "nudNumInput";
            this.nudNumInput.Size = new System.Drawing.Size(435, 20);
            this.nudNumInput.TabIndex = 2;
            // 
            // txtTextInput
            // 
            this.txtTextInput.Location = new System.Drawing.Point(12, 56);
            this.txtTextInput.Name = "txtTextInput";
            this.txtTextInput.Size = new System.Drawing.Size(435, 20);
            this.txtTextInput.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(372, 82);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.button2_Click);
            // 
            // UserInput
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 115);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.nudNumInput);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.txtTextInput);
            this.Name = "UserInput";
            this.Text = "UserInput";
            ((System.ComponentModel.ISupportInitialize)(this.nudNumInput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown nudNumInput;
        private System.Windows.Forms.TextBox txtTextInput;
        private System.Windows.Forms.Button btnOk;
    }
}