namespace DiceBot
{
    partial class SimpleSwap
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
            this.label1 = new System.Windows.Forms.Label();
            this.cbFrom = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbTo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtReceiving = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.nudSending = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.txtRate = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtReveice = new System.Windows.Forms.TextBox();
            this.btnExchange = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnProgress = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtDeposit = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnQuote = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudSending)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Exchange From:";
            // 
            // cbFrom
            // 
            this.cbFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFrom.FormattingEnabled = true;
            this.cbFrom.Items.AddRange(new object[] {
            "Btc",
            "Doge",
            "Clam",
            "Ltc",
            "Dash"});
            this.cbFrom.Location = new System.Drawing.Point(15, 25);
            this.cbFrom.Name = "cbFrom";
            this.cbFrom.Size = new System.Drawing.Size(121, 21);
            this.cbFrom.TabIndex = 1;
            this.cbFrom.SelectedIndexChanged += new System.EventHandler(this.cbFrom_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(192, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Exchange To:";
            // 
            // cbTo
            // 
            this.cbTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTo.FormattingEnabled = true;
            this.cbTo.Items.AddRange(new object[] {
            "Btc",
            "Doge",
            "Clam",
            "Ltc",
            "Dash"});
            this.cbTo.Location = new System.Drawing.Point(195, 25);
            this.cbTo.Name = "cbTo";
            this.cbTo.Size = new System.Drawing.Size(146, 21);
            this.cbTo.TabIndex = 3;
            this.cbTo.SelectedIndexChanged += new System.EventHandler(this.cbTo_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(392, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Receiving Address:";
            // 
            // txtReceiving
            // 
            this.txtReceiving.Location = new System.Drawing.Point(395, 25);
            this.txtReceiving.Name = "txtReceiving";
            this.txtReceiving.Size = new System.Drawing.Size(260, 20);
            this.txtReceiving.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(142, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "How much are you sending?";
            // 
            // nudSending
            // 
            this.nudSending.DecimalPlaces = 8;
            this.nudSending.Location = new System.Drawing.Point(15, 86);
            this.nudSending.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.nudSending.Name = "nudSending";
            this.nudSending.Size = new System.Drawing.Size(120, 20);
            this.nudSending.TabIndex = 7;
            this.nudSending.ValueChanged += new System.EventHandler(this.nudSending_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(192, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Current Rate";
            // 
            // txtRate
            // 
            this.txtRate.Location = new System.Drawing.Point(195, 86);
            this.txtRate.Name = "txtRate";
            this.txtRate.ReadOnly = true;
            this.txtRate.Size = new System.Drawing.Size(146, 20);
            this.txtRate.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(392, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "You will receive: (*)";
            // 
            // txtReveice
            // 
            this.txtReveice.Location = new System.Drawing.Point(395, 86);
            this.txtReveice.Name = "txtReveice";
            this.txtReveice.ReadOnly = true;
            this.txtReveice.Size = new System.Drawing.Size(167, 20);
            this.txtReveice.TabIndex = 11;
            // 
            // btnExchange
            // 
            this.btnExchange.Location = new System.Drawing.Point(580, 86);
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(75, 23);
            this.btnExchange.TabIndex = 12;
            this.btnExchange.Text = "Exchange";
            this.btnExchange.UseVisualStyleBackColor = true;
            this.btnExchange.Click += new System.EventHandler(this.btnExchange_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnProgress);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.txtDeposit);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Location = new System.Drawing.Point(17, 140);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(638, 34);
            this.panel1.TabIndex = 13;
            this.panel1.Visible = false;
            // 
            // btnProgress
            // 
            this.btnProgress.Location = new System.Drawing.Point(560, 6);
            this.btnProgress.Name = "btnProgress";
            this.btnProgress.Size = new System.Drawing.Size(75, 23);
            this.btnProgress.TabIndex = 3;
            this.btnProgress.Text = "Progress";
            this.btnProgress.UseVisualStyleBackColor = true;
            this.btnProgress.Click += new System.EventHandler(this.btnProgress_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(375, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(179, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "View the progress of your exchange:";
            // 
            // txtDeposit
            // 
            this.txtDeposit.Location = new System.Drawing.Point(126, 6);
            this.txtDeposit.Name = "txtDeposit";
            this.txtDeposit.Size = new System.Drawing.Size(198, 20);
            this.txtDeposit.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Deposit to this address:";
            // 
            // btnQuote
            // 
            this.btnQuote.Location = new System.Drawing.Point(580, 51);
            this.btnQuote.Name = "btnQuote";
            this.btnQuote.Size = new System.Drawing.Size(75, 23);
            this.btnQuote.TabIndex = 15;
            this.btnQuote.Text = "Get Quote";
            this.btnQuote.UseVisualStyleBackColor = true;
            this.btnQuote.Click += new System.EventHandler(this.btnQuote_Click);
            // 
            // SimpleSwap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 191);
            this.Controls.Add(this.btnQuote);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnExchange);
            this.Controls.Add(this.txtReveice);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtRate);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.nudSending);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtReceiving);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbTo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbFrom);
            this.Controls.Add(this.label1);
            this.Name = "SimpleSwap";
            this.Text = "SimpleSwap";
            ((System.ComponentModel.ISupportInitialize)(this.nudSending)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbFrom;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbTo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtReceiving;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudSending;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtReveice;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnProgress;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtDeposit;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnQuote;
    }
}