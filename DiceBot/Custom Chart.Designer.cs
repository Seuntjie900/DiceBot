namespace DiceBot
{
    partial class Custom_Chart
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
            this.btnChartTimeRange = new System.Windows.Forms.Button();
            this.btnChartBetID = new System.Windows.Forms.Button();
            this.nudGraphStartBetID = new System.Windows.Forms.NumericUpDown();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.label96 = new System.Windows.Forms.Label();
            this.label95 = new System.Windows.Forms.Label();
            this.label94 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphStartBetID)).BeginInit();
            this.SuspendLayout();
            // 
            // btnChartTimeRange
            // 
            this.btnChartTimeRange.Location = new System.Drawing.Point(327, 89);
            this.btnChartTimeRange.Name = "btnChartTimeRange";
            this.btnChartTimeRange.Size = new System.Drawing.Size(75, 44);
            this.btnChartTimeRange.TabIndex = 89;
            this.btnChartTimeRange.Text = "Draw Chart";
            this.btnChartTimeRange.UseVisualStyleBackColor = true;
            this.btnChartTimeRange.Click += new System.EventHandler(this.btnChartTimeRange_Click);
            // 
            // btnChartBetID
            // 
            this.btnChartBetID.Location = new System.Drawing.Point(327, 17);
            this.btnChartBetID.Name = "btnChartBetID";
            this.btnChartBetID.Size = new System.Drawing.Size(75, 44);
            this.btnChartBetID.TabIndex = 88;
            this.btnChartBetID.Text = "Draw Chart";
            this.btnChartBetID.UseVisualStyleBackColor = true;
            this.btnChartBetID.Click += new System.EventHandler(this.btnChartBetID_Click);
            // 
            // nudGraphStartBetID
            // 
            this.nudGraphStartBetID.Location = new System.Drawing.Point(121, 23);
            this.nudGraphStartBetID.Maximum = new decimal(new int[] {
            276447232,
            23283,
            0,
            0});
            this.nudGraphStartBetID.Name = "nudGraphStartBetID";
            this.nudGraphStartBetID.Size = new System.Drawing.Size(200, 20);
            this.nudGraphStartBetID.TabIndex = 87;
            // 
            // dtpEnd
            // 
            this.dtpEnd.Location = new System.Drawing.Point(121, 115);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(200, 20);
            this.dtpEnd.TabIndex = 86;
            // 
            // dtpStart
            // 
            this.dtpStart.Location = new System.Drawing.Point(121, 89);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(200, 20);
            this.dtpStart.TabIndex = 85;
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.Location = new System.Drawing.Point(86, 121);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(29, 13);
            this.label96.TabIndex = 84;
            this.label96.Text = "And:";
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.Location = new System.Drawing.Point(25, 95);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(90, 13);
            this.label95.TabIndex = 83;
            this.label95.Text = "All Bets Between:";
            // 
            // label94
            // 
            this.label94.AutoSize = true;
            this.label94.Location = new System.Drawing.Point(8, 25);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(107, 13);
            this.label94.TabIndex = 82;
            this.label94.Text = "All bets Above bet id:";
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(327, 160);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 42);
            this.button1.TabIndex = 81;
            this.button1.Text = "Random Graph";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            // 
            // Custom_Chart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 153);
            this.Controls.Add(this.btnChartTimeRange);
            this.Controls.Add(this.btnChartBetID);
            this.Controls.Add(this.nudGraphStartBetID);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(this.label96);
            this.Controls.Add(this.label95);
            this.Controls.Add(this.label94);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Custom_Chart";
            this.Text = "Custom Chart";
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphStartBetID)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnChartTimeRange;
        private System.Windows.Forms.Button btnChartBetID;
        private System.Windows.Forms.NumericUpDown nudGraphStartBetID;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.Label label95;
        private System.Windows.Forms.Label label94;
        private System.Windows.Forms.Button button1;
    }
}