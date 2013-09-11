namespace DiceBot
{
    partial class StreakTable
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
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCacl = new System.Windows.Forms.Button();
            this.rdbConstant = new System.Windows.Forms.RadioButton();
            this.rdbVariable = new System.Windows.Forms.RadioButton();
            this.rdbMax = new System.Windows.Forms.RadioButton();
            this.nudMaxMultiply = new System.Windows.Forms.NumericUpDown();
            this.txtNBets = new System.Windows.Forms.TextBox();
            this.txtDevider = new System.Windows.Forms.TextBox();
            this.txtMultiplier = new System.Windows.Forms.TextBox();
            this.txtMinBet = new System.Windows.Forms.TextBox();
            this.rdbOnce = new System.Windows.Forms.RadioButton();
            this.txtReturn = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.betNrDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.betAmountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalWageredDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.returnonwinDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.profitDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cBetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxMultiply)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cBetBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.betNrDataGridViewTextBoxColumn,
            this.betAmountDataGridViewTextBoxColumn,
            this.totalWageredDataGridViewTextBoxColumn,
            this.returnonwinDataGridViewTextBoxColumn,
            this.profitDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.cBetBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 91);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(611, 234);
            this.dataGridView1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.txtReturn);
            this.panel1.Controls.Add(this.rdbOnce);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnCacl);
            this.panel1.Controls.Add(this.rdbConstant);
            this.panel1.Controls.Add(this.rdbVariable);
            this.panel1.Controls.Add(this.rdbMax);
            this.panel1.Controls.Add(this.nudMaxMultiply);
            this.panel1.Controls.Add(this.txtNBets);
            this.panel1.Controls.Add(this.txtDevider);
            this.panel1.Controls.Add(this.txtMultiplier);
            this.panel1.Controls.Add(this.txtMinBet);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(611, 91);
            this.panel1.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(345, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Maximum Multiplies:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(161, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Every N Bets:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(186, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Devider:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Multiplier:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Start Bet:";
            // 
            // btnCacl
            // 
            this.btnCacl.Location = new System.Drawing.Point(517, 11);
            this.btnCacl.Name = "btnCacl";
            this.btnCacl.Size = new System.Drawing.Size(61, 23);
            this.btnCacl.TabIndex = 8;
            this.btnCacl.Text = "Calculate";
            this.btnCacl.UseVisualStyleBackColor = true;
            this.btnCacl.Click += new System.EventHandler(this.btnCacl_Click);
            // 
            // rdbConstant
            // 
            this.rdbConstant.AutoSize = true;
            this.rdbConstant.Location = new System.Drawing.Point(345, 68);
            this.rdbConstant.Name = "rdbConstant";
            this.rdbConstant.Size = new System.Drawing.Size(67, 17);
            this.rdbConstant.TabIndex = 7;
            this.rdbConstant.TabStop = true;
            this.rdbConstant.Text = "Constant";
            this.rdbConstant.UseVisualStyleBackColor = true;
            // 
            // rdbVariable
            // 
            this.rdbVariable.AutoSize = true;
            this.rdbVariable.Location = new System.Drawing.Point(205, 68);
            this.rdbVariable.Name = "rdbVariable";
            this.rdbVariable.Size = new System.Drawing.Size(63, 17);
            this.rdbVariable.TabIndex = 6;
            this.rdbVariable.TabStop = true;
            this.rdbVariable.Text = "Variable";
            this.rdbVariable.UseVisualStyleBackColor = true;
            // 
            // rdbMax
            // 
            this.rdbMax.AutoSize = true;
            this.rdbMax.Location = new System.Drawing.Point(62, 68);
            this.rdbMax.Name = "rdbMax";
            this.rdbMax.Size = new System.Drawing.Size(45, 17);
            this.rdbMax.TabIndex = 5;
            this.rdbMax.TabStop = true;
            this.rdbMax.Text = "Max";
            this.rdbMax.UseVisualStyleBackColor = true;
            // 
            // nudMaxMultiply
            // 
            this.nudMaxMultiply.Location = new System.Drawing.Point(447, 14);
            this.nudMaxMultiply.Name = "nudMaxMultiply";
            this.nudMaxMultiply.Size = new System.Drawing.Size(45, 20);
            this.nudMaxMultiply.TabIndex = 4;
            // 
            // txtNBets
            // 
            this.txtNBets.Location = new System.Drawing.Point(239, 36);
            this.txtNBets.Name = "txtNBets";
            this.txtNBets.Size = new System.Drawing.Size(100, 20);
            this.txtNBets.TabIndex = 3;
            // 
            // txtDevider
            // 
            this.txtDevider.Location = new System.Drawing.Point(239, 13);
            this.txtDevider.Name = "txtDevider";
            this.txtDevider.Size = new System.Drawing.Size(100, 20);
            this.txtDevider.TabIndex = 2;
            // 
            // txtMultiplier
            // 
            this.txtMultiplier.Location = new System.Drawing.Point(62, 36);
            this.txtMultiplier.Name = "txtMultiplier";
            this.txtMultiplier.Size = new System.Drawing.Size(93, 20);
            this.txtMultiplier.TabIndex = 1;
            // 
            // txtMinBet
            // 
            this.txtMinBet.Location = new System.Drawing.Point(62, 13);
            this.txtMinBet.Name = "txtMinBet";
            this.txtMinBet.Size = new System.Drawing.Size(93, 20);
            this.txtMinBet.TabIndex = 0;
            // 
            // rdbOnce
            // 
            this.rdbOnce.AutoSize = true;
            this.rdbOnce.Location = new System.Drawing.Point(457, 68);
            this.rdbOnce.Name = "rdbOnce";
            this.rdbOnce.Size = new System.Drawing.Size(91, 17);
            this.rdbOnce.TabIndex = 14;
            this.rdbOnce.TabStop = true;
            this.rdbOnce.Text = "Change Once";
            this.rdbOnce.UseVisualStyleBackColor = true;
            // 
            // txtReturn
            // 
            this.txtReturn.Location = new System.Drawing.Point(447, 36);
            this.txtReturn.Name = "txtReturn";
            this.txtReturn.Size = new System.Drawing.Size(100, 20);
            this.txtReturn.TabIndex = 15;
            this.txtReturn.Text = "1.98";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(365, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Return on win:";
            // 
            // betNrDataGridViewTextBoxColumn
            // 
            this.betNrDataGridViewTextBoxColumn.DataPropertyName = "BetNr";
            this.betNrDataGridViewTextBoxColumn.HeaderText = "BetNr";
            this.betNrDataGridViewTextBoxColumn.Name = "betNrDataGridViewTextBoxColumn";
            // 
            // betAmountDataGridViewTextBoxColumn
            // 
            this.betAmountDataGridViewTextBoxColumn.DataPropertyName = "Bet_Amount";
            this.betAmountDataGridViewTextBoxColumn.HeaderText = "Bet_Amount";
            this.betAmountDataGridViewTextBoxColumn.Name = "betAmountDataGridViewTextBoxColumn";
            // 
            // totalWageredDataGridViewTextBoxColumn
            // 
            this.totalWageredDataGridViewTextBoxColumn.DataPropertyName = "Total_Wagered";
            this.totalWageredDataGridViewTextBoxColumn.HeaderText = "Total_Wagered";
            this.totalWageredDataGridViewTextBoxColumn.Name = "totalWageredDataGridViewTextBoxColumn";
            // 
            // returnonwinDataGridViewTextBoxColumn
            // 
            this.returnonwinDataGridViewTextBoxColumn.DataPropertyName = "Return_on_win";
            this.returnonwinDataGridViewTextBoxColumn.HeaderText = "Return_on_win";
            this.returnonwinDataGridViewTextBoxColumn.Name = "returnonwinDataGridViewTextBoxColumn";
            // 
            // profitDataGridViewTextBoxColumn
            // 
            this.profitDataGridViewTextBoxColumn.DataPropertyName = "Profit";
            this.profitDataGridViewTextBoxColumn.HeaderText = "Profit";
            this.profitDataGridViewTextBoxColumn.Name = "profitDataGridViewTextBoxColumn";
            // 
            // cBetBindingSource
            // 
            this.cBetBindingSource.DataSource = typeof(DiceBot.cBet);
            // 
            // StreakTable
            // 
            this.AcceptButton = this.btnCacl;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 325);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.Name = "StreakTable";
            this.Text = "StreakTable";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxMultiply)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cBetBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn betNrDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn betAmountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalWageredDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn returnonwinDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn profitDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource cBetBindingSource;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rdbConstant;
        private System.Windows.Forms.RadioButton rdbVariable;
        private System.Windows.Forms.RadioButton rdbMax;
        private System.Windows.Forms.NumericUpDown nudMaxMultiply;
        private System.Windows.Forms.TextBox txtNBets;
        private System.Windows.Forms.TextBox txtDevider;
        private System.Windows.Forms.TextBox txtMultiplier;
        private System.Windows.Forms.TextBox txtMinBet;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCacl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtReturn;
        private System.Windows.Forms.RadioButton rdbOnce;
    }
}