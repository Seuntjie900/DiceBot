namespace DiceBot
{
    partial class BetHistory
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgvBets = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbView = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.dtpUntill = new System.Windows.Forms.DateTimePicker();
            this.btnView = new System.Windows.Forms.Button();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.highDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.chanceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.amountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rollDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.profitDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.verifiedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.nonceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clientseedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverseedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverhashDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.betBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.betBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnView);
            this.panel1.Controls.Add(this.dtpUntill);
            this.panel1.Controls.Add(this.dtpFrom);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cmbView);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(906, 56);
            this.panel1.TabIndex = 0;
            // 
            // dgvBets
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvBets.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvBets.AutoGenerateColumns = false;
            this.dgvBets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idDataGridViewTextBoxColumn,
            this.dateDataGridViewTextBoxColumn,
            this.highDataGridViewCheckBoxColumn,
            this.chanceDataGridViewTextBoxColumn,
            this.amountDataGridViewTextBoxColumn,
            this.rollDataGridViewTextBoxColumn,
            this.profitDataGridViewTextBoxColumn,
            this.verifiedDataGridViewCheckBoxColumn,
            this.nonceDataGridViewTextBoxColumn,
            this.clientseedDataGridViewTextBoxColumn,
            this.serverseedDataGridViewTextBoxColumn,
            this.serverhashDataGridViewTextBoxColumn});
            this.dgvBets.DataSource = this.betBindingSource;
            this.dgvBets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBets.Location = new System.Drawing.Point(0, 56);
            this.dgvBets.Name = "dgvBets";
            this.dgvBets.Size = new System.Drawing.Size(906, 357);
            this.dgvBets.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "View Bets From:";
            // 
            // cmbView
            // 
            this.cmbView.FormattingEnabled = true;
            this.cmbView.Items.AddRange(new object[] {
            "Today",
            "This Week",
            "This Month",
            "This Year",
            "The Beginning of time",
            "Custom"});
            this.cmbView.Location = new System.Drawing.Point(101, 19);
            this.cmbView.Name = "cmbView";
            this.cmbView.Size = new System.Drawing.Size(121, 21);
            this.cmbView.TabIndex = 1;
            this.cmbView.SelectedIndexChanged += new System.EventHandler(this.cmbView_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(270, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "From:";
            this.label2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(547, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Untill:";
            this.label3.Visible = false;
            // 
            // dtpFrom
            // 
            this.dtpFrom.Location = new System.Drawing.Point(309, 19);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(200, 20);
            this.dtpFrom.TabIndex = 4;
            this.dtpFrom.Visible = false;
            // 
            // dtpUntill
            // 
            this.dtpUntill.Location = new System.Drawing.Point(586, 19);
            this.dtpUntill.Name = "dtpUntill";
            this.dtpUntill.Size = new System.Drawing.Size(200, 20);
            this.dtpUntill.TabIndex = 5;
            this.dtpUntill.Visible = false;
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(806, 17);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(75, 23);
            this.btnView.TabIndex = 6;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Visible = false;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Bet ID";
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.DataPropertyName = "date";
            this.dateDataGridViewTextBoxColumn.HeaderText = "Date";
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            // 
            // highDataGridViewCheckBoxColumn
            // 
            this.highDataGridViewCheckBoxColumn.DataPropertyName = "high";
            this.highDataGridViewCheckBoxColumn.HeaderText = "high";
            this.highDataGridViewCheckBoxColumn.Name = "highDataGridViewCheckBoxColumn";
            // 
            // chanceDataGridViewTextBoxColumn
            // 
            this.chanceDataGridViewTextBoxColumn.DataPropertyName = "Chance";
            this.chanceDataGridViewTextBoxColumn.HeaderText = "Chance";
            this.chanceDataGridViewTextBoxColumn.Name = "chanceDataGridViewTextBoxColumn";
            // 
            // amountDataGridViewTextBoxColumn
            // 
            this.amountDataGridViewTextBoxColumn.DataPropertyName = "Amount";
            this.amountDataGridViewTextBoxColumn.HeaderText = "Stake";
            this.amountDataGridViewTextBoxColumn.Name = "amountDataGridViewTextBoxColumn";
            // 
            // rollDataGridViewTextBoxColumn
            // 
            this.rollDataGridViewTextBoxColumn.DataPropertyName = "Roll";
            this.rollDataGridViewTextBoxColumn.HeaderText = "Roll";
            this.rollDataGridViewTextBoxColumn.Name = "rollDataGridViewTextBoxColumn";
            // 
            // profitDataGridViewTextBoxColumn
            // 
            this.profitDataGridViewTextBoxColumn.DataPropertyName = "Profit";
            this.profitDataGridViewTextBoxColumn.HeaderText = "Profit";
            this.profitDataGridViewTextBoxColumn.Name = "profitDataGridViewTextBoxColumn";
            // 
            // verifiedDataGridViewCheckBoxColumn
            // 
            this.verifiedDataGridViewCheckBoxColumn.DataPropertyName = "Verified";
            this.verifiedDataGridViewCheckBoxColumn.HeaderText = "Verified";
            this.verifiedDataGridViewCheckBoxColumn.Name = "verifiedDataGridViewCheckBoxColumn";
            // 
            // nonceDataGridViewTextBoxColumn
            // 
            this.nonceDataGridViewTextBoxColumn.DataPropertyName = "nonce";
            this.nonceDataGridViewTextBoxColumn.HeaderText = "nonce";
            this.nonceDataGridViewTextBoxColumn.Name = "nonceDataGridViewTextBoxColumn";
            // 
            // clientseedDataGridViewTextBoxColumn
            // 
            this.clientseedDataGridViewTextBoxColumn.DataPropertyName = "clientseed";
            this.clientseedDataGridViewTextBoxColumn.HeaderText = "clientseed";
            this.clientseedDataGridViewTextBoxColumn.Name = "clientseedDataGridViewTextBoxColumn";
            // 
            // serverseedDataGridViewTextBoxColumn
            // 
            this.serverseedDataGridViewTextBoxColumn.DataPropertyName = "serverseed";
            this.serverseedDataGridViewTextBoxColumn.HeaderText = "serverseed";
            this.serverseedDataGridViewTextBoxColumn.Name = "serverseedDataGridViewTextBoxColumn";
            // 
            // serverhashDataGridViewTextBoxColumn
            // 
            this.serverhashDataGridViewTextBoxColumn.DataPropertyName = "serverhash";
            this.serverhashDataGridViewTextBoxColumn.HeaderText = "serverhash";
            this.serverhashDataGridViewTextBoxColumn.Name = "serverhashDataGridViewTextBoxColumn";
            // 
            // betBindingSource
            // 
            this.betBindingSource.DataSource = typeof(DiceBot.Bet);
            // 
            // BetHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 413);
            this.Controls.Add(this.dgvBets);
            this.Controls.Add(this.panel1);
            this.Name = "BetHistory";
            this.Text = "Bet History";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.betBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvBets;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.DateTimePicker dtpUntill;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn highDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn chanceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn amountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rollDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn profitDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn verifiedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nonceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn clientseedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverseedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverhashDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource betBindingSource;
    }
}