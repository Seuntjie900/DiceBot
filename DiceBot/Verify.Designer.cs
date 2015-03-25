namespace DiceBot
{
    partial class Verify
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
            this.btnGenerateBets = new System.Windows.Forms.Button();
            this.nudGenBetsAmount = new System.Windows.Forms.NumericUpDown();
            this.label28 = new System.Windows.Forms.Label();
            this.nudGenBetsStart = new System.Windows.Forms.NumericUpDown();
            this.label27 = new System.Windows.Forms.Label();
            this.txtClientSeed = new System.Windows.Forms.TextBox();
            this.txtServerSeed = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudGenBetsAmount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGenBetsStart)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGenerateBets
            // 
            this.btnGenerateBets.Location = new System.Drawing.Point(87, 127);
            this.btnGenerateBets.Name = "btnGenerateBets";
            this.btnGenerateBets.Size = new System.Drawing.Size(165, 23);
            this.btnGenerateBets.TabIndex = 17;
            this.btnGenerateBets.Text = "Generate Bets";
            this.btnGenerateBets.UseVisualStyleBackColor = true;
            this.btnGenerateBets.Click += new System.EventHandler(this.btnGenerateBets_Click);
            // 
            // nudGenBetsAmount
            // 
            this.nudGenBetsAmount.Location = new System.Drawing.Point(259, 98);
            this.nudGenBetsAmount.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudGenBetsAmount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGenBetsAmount.Name = "nudGenBetsAmount";
            this.nudGenBetsAmount.Size = new System.Drawing.Size(76, 20);
            this.nudGenBetsAmount.TabIndex = 16;
            this.nudGenBetsAmount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(168, 102);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(81, 13);
            this.label28.TabIndex = 15;
            this.label28.Text = "Amount of bets:";
            // 
            // nudGenBetsStart
            // 
            this.nudGenBetsStart.Location = new System.Drawing.Point(87, 98);
            this.nudGenBetsStart.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nudGenBetsStart.Name = "nudGenBetsStart";
            this.nudGenBetsStart.Size = new System.Drawing.Size(75, 20);
            this.nudGenBetsStart.TabIndex = 14;
            this.nudGenBetsStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(24, 102);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(58, 13);
            this.label27.TabIndex = 13;
            this.label27.Text = "Start From:";
            // 
            // txtClientSeed
            // 
            this.txtClientSeed.Location = new System.Drawing.Point(87, 73);
            this.txtClientSeed.Name = "txtClientSeed";
            this.txtClientSeed.Size = new System.Drawing.Size(356, 20);
            this.txtClientSeed.TabIndex = 12;
            // 
            // txtServerSeed
            // 
            this.txtServerSeed.Location = new System.Drawing.Point(87, 9);
            this.txtServerSeed.Multiline = true;
            this.txtServerSeed.Name = "txtServerSeed";
            this.txtServerSeed.Size = new System.Drawing.Size(356, 58);
            this.txtServerSeed.TabIndex = 10;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(17, 76);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(64, 13);
            this.label26.TabIndex = 11;
            this.label26.Text = "Client Seed:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(12, 9);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(69, 13);
            this.label21.TabIndex = 9;
            this.label21.Text = "Server Seed:";
            // 
            // Verify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 171);
            this.Controls.Add(this.btnGenerateBets);
            this.Controls.Add(this.nudGenBetsAmount);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.nudGenBetsStart);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.txtClientSeed);
            this.Controls.Add(this.txtServerSeed);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.label21);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Verify";
            this.Text = "Verify Rolls";
            ((System.ComponentModel.ISupportInitialize)(this.nudGenBetsAmount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGenBetsStart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenerateBets;
        private System.Windows.Forms.NumericUpDown nudGenBetsAmount;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.NumericUpDown nudGenBetsStart;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox txtClientSeed;
        private System.Windows.Forms.TextBox txtServerSeed;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label21;
    }
}