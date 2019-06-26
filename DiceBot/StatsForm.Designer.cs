namespace DiceBot
{
    partial class StatsForm
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
            this.stats1 = new DiceBot.Stats();
            this.SuspendLayout();
            // 
            // stats1
            // 
            this.stats1.Location = new System.Drawing.Point(0, 0);
            this.stats1.Name = "stats1";
            this.stats1.ShowHideButtons = true;
            this.stats1.Size = new System.Drawing.Size(486, 270);
            this.stats1.TabIndex = 0;
            // 
            // StatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 268);
            this.Controls.Add(this.stats1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "StatsForm";
            this.Text = "Stats";
            this.ResumeLayout(false);

        }

        #endregion

        public Stats stats1;
    }
}