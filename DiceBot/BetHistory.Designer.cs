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
            this.cmbJumpTo = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.cmbViewPerPage = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnView = new System.Windows.Forms.Button();
            this.dtpUntill = new System.Windows.Forms.DateTimePicker();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbView = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvBets = new System.Windows.Forms.DataGridView();
            this.pnlSearch = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblSearchDateUntill = new System.Windows.Forms.Label();
            this.lblSearchDateFrom = new System.Windows.Forms.Label();
            this.dtpSearchUntil = new System.Windows.Forms.DateTimePicker();
            this.dtpSearchFrom = new System.Windows.Forms.DateTimePicker();
            this.rdbDateRange = new System.Windows.Forms.RadioButton();
            this.rdbDateAll = new System.Windows.Forms.RadioButton();
            this.btnSearch = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rdbVerifiedAll = new System.Windows.Forms.RadioButton();
            this.rdbNotVerified = new System.Windows.Forms.RadioButton();
            this.rdbVerified = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdbHighAll = new System.Windows.Forms.RadioButton();
            this.rdbLow = new System.Windows.Forms.RadioButton();
            this.rdbHigh = new System.Windows.Forms.RadioButton();
            this.chkServerHash = new System.Windows.Forms.CheckBox();
            this.chkServerSeed = new System.Windows.Forms.CheckBox();
            this.chkClientSeed = new System.Windows.Forms.CheckBox();
            this.chkProfit = new System.Windows.Forms.CheckBox();
            this.chkRoll = new System.Windows.Forms.CheckBox();
            this.chkStake = new System.Windows.Forms.CheckBox();
            this.chkChance = new System.Windows.Forms.CheckBox();
            this.chkBetId = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
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
            this.pnlSearch.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.betBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbJumpTo);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.btnNext);
            this.panel1.Controls.Add(this.btnPrevious);
            this.panel1.Controls.Add(this.cmbViewPerPage);
            this.panel1.Controls.Add(this.label4);
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
            this.panel1.Size = new System.Drawing.Size(1438, 56);
            this.panel1.TabIndex = 0;
            // 
            // cmbJumpTo
            // 
            this.cmbJumpTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbJumpTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJumpTo.FormattingEnabled = true;
            this.cmbJumpTo.Location = new System.Drawing.Point(1368, 22);
            this.cmbJumpTo.Name = "cmbJumpTo";
            this.cmbJumpTo.Size = new System.Drawing.Size(58, 21);
            this.cmbJumpTo.TabIndex = 12;
            this.cmbJumpTo.SelectedIndexChanged += new System.EventHandler(this.cmbJumpTo_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1317, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Jump To:";
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(1236, 21);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 10;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrevious.Location = new System.Drawing.Point(1155, 21);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(75, 23);
            this.btnPrevious.TabIndex = 9;
            this.btnPrevious.Text = "Previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // cmbViewPerPage
            // 
            this.cmbViewPerPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbViewPerPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbViewPerPage.FormattingEnabled = true;
            this.cmbViewPerPage.Items.AddRange(new object[] {
            "10",
            "25",
            "50",
            "100",
            "250",
            "500"});
            this.cmbViewPerPage.Location = new System.Drawing.Point(1101, 22);
            this.cmbViewPerPage.Name = "cmbViewPerPage";
            this.cmbViewPerPage.Size = new System.Drawing.Size(48, 21);
            this.cmbViewPerPage.TabIndex = 8;
            this.cmbViewPerPage.SelectedIndexChanged += new System.EventHandler(this.cmbViewPerPage_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1015, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "View Per Page:";
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(806, 21);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(75, 23);
            this.btnView.TabIndex = 6;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Visible = false;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // dtpUntill
            // 
            this.dtpUntill.Location = new System.Drawing.Point(586, 22);
            this.dtpUntill.Name = "dtpUntill";
            this.dtpUntill.Size = new System.Drawing.Size(200, 20);
            this.dtpUntill.TabIndex = 5;
            this.dtpUntill.Visible = false;
            // 
            // dtpFrom
            // 
            this.dtpFrom.Location = new System.Drawing.Point(309, 22);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(200, 20);
            this.dtpFrom.TabIndex = 4;
            this.dtpFrom.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(547, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Untill:";
            this.label3.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(270, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "From:";
            this.label2.Visible = false;
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
            "Custom",
            "Search"});
            this.cmbView.Location = new System.Drawing.Point(101, 22);
            this.cmbView.Name = "cmbView";
            this.cmbView.Size = new System.Drawing.Size(121, 21);
            this.cmbView.TabIndex = 1;
            this.cmbView.SelectedIndexChanged += new System.EventHandler(this.cmbView_SelectedIndexChanged);
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
            // dgvBets
            // 
            this.dgvBets.AllowUserToAddRows = false;
            this.dgvBets.AllowUserToDeleteRows = false;
            this.dgvBets.AllowUserToOrderColumns = true;
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
            this.dgvBets.Location = new System.Drawing.Point(0, 235);
            this.dgvBets.Name = "dgvBets";
            this.dgvBets.ReadOnly = true;
            this.dgvBets.Size = new System.Drawing.Size(1438, 178);
            this.dgvBets.TabIndex = 1;
            // 
            // pnlSearch
            // 
            this.pnlSearch.Controls.Add(this.groupBox3);
            this.pnlSearch.Controls.Add(this.btnSearch);
            this.pnlSearch.Controls.Add(this.textBox1);
            this.pnlSearch.Controls.Add(this.label7);
            this.pnlSearch.Controls.Add(this.groupBox2);
            this.pnlSearch.Controls.Add(this.groupBox4);
            this.pnlSearch.Controls.Add(this.chkServerHash);
            this.pnlSearch.Controls.Add(this.chkServerSeed);
            this.pnlSearch.Controls.Add(this.chkClientSeed);
            this.pnlSearch.Controls.Add(this.chkProfit);
            this.pnlSearch.Controls.Add(this.chkRoll);
            this.pnlSearch.Controls.Add(this.chkStake);
            this.pnlSearch.Controls.Add(this.chkChance);
            this.pnlSearch.Controls.Add(this.chkBetId);
            this.pnlSearch.Controls.Add(this.label6);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 56);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(1438, 179);
            this.pnlSearch.TabIndex = 2;
            this.pnlSearch.TabStop = false;
            this.pnlSearch.Text = "Search";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblSearchDateUntill);
            this.groupBox3.Controls.Add(this.lblSearchDateFrom);
            this.groupBox3.Controls.Add(this.dtpSearchUntil);
            this.groupBox3.Controls.Add(this.dtpSearchFrom);
            this.groupBox3.Controls.Add(this.rdbDateRange);
            this.groupBox3.Controls.Add(this.rdbDateAll);
            this.groupBox3.Location = new System.Drawing.Point(850, 18);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(380, 65);
            this.groupBox3.TabIndex = 31;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Date";
            // 
            // lblSearchDateUntill
            // 
            this.lblSearchDateUntill.AutoSize = true;
            this.lblSearchDateUntill.Location = new System.Drawing.Point(126, 43);
            this.lblSearchDateUntill.Name = "lblSearchDateUntill";
            this.lblSearchDateUntill.Size = new System.Drawing.Size(29, 13);
            this.lblSearchDateUntill.TabIndex = 5;
            this.lblSearchDateUntill.Text = "And:";
            this.lblSearchDateUntill.Visible = false;
            // 
            // lblSearchDateFrom
            // 
            this.lblSearchDateFrom.AutoSize = true;
            this.lblSearchDateFrom.Location = new System.Drawing.Point(103, 21);
            this.lblSearchDateFrom.Name = "lblSearchDateFrom";
            this.lblSearchDateFrom.Size = new System.Drawing.Size(52, 13);
            this.lblSearchDateFrom.TabIndex = 4;
            this.lblSearchDateFrom.Text = "Between:";
            this.lblSearchDateFrom.Visible = false;
            // 
            // dtpSearchUntil
            // 
            this.dtpSearchUntil.Location = new System.Drawing.Point(161, 39);
            this.dtpSearchUntil.Name = "dtpSearchUntil";
            this.dtpSearchUntil.Size = new System.Drawing.Size(200, 20);
            this.dtpSearchUntil.TabIndex = 3;
            this.dtpSearchUntil.Visible = false;
            // 
            // dtpSearchFrom
            // 
            this.dtpSearchFrom.Location = new System.Drawing.Point(161, 17);
            this.dtpSearchFrom.Name = "dtpSearchFrom";
            this.dtpSearchFrom.Size = new System.Drawing.Size(200, 20);
            this.dtpSearchFrom.TabIndex = 2;
            this.dtpSearchFrom.Visible = false;
            // 
            // rdbDateRange
            // 
            this.rdbDateRange.AutoSize = true;
            this.rdbDateRange.Location = new System.Drawing.Point(6, 41);
            this.rdbDateRange.Name = "rdbDateRange";
            this.rdbDateRange.Size = new System.Drawing.Size(83, 17);
            this.rdbDateRange.TabIndex = 1;
            this.rdbDateRange.Text = "Date Range";
            this.rdbDateRange.UseVisualStyleBackColor = true;
            this.rdbDateRange.CheckedChanged += new System.EventHandler(this.rdbDateRange_CheckedChanged);
            // 
            // rdbDateAll
            // 
            this.rdbDateAll.AutoSize = true;
            this.rdbDateAll.Checked = true;
            this.rdbDateAll.Location = new System.Drawing.Point(6, 19);
            this.rdbDateAll.Name = "rdbDateAll";
            this.rdbDateAll.Size = new System.Drawing.Size(36, 17);
            this.rdbDateAll.TabIndex = 0;
            this.rdbDateAll.TabStop = true;
            this.rdbDateAll.Text = "All";
            this.rdbDateAll.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(585, 57);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 30;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click_1);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(94, 59);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(482, 20);
            this.textBox1.TabIndex = 29;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 28;
            this.label7.Text = "Search For:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rdbVerifiedAll);
            this.groupBox2.Controls.Add(this.rdbNotVerified);
            this.groupBox2.Controls.Add(this.rdbVerified);
            this.groupBox2.Location = new System.Drawing.Point(680, 91);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(157, 65);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Verified";
            // 
            // rdbVerifiedAll
            // 
            this.rdbVerifiedAll.AutoSize = true;
            this.rdbVerifiedAll.Checked = true;
            this.rdbVerifiedAll.Location = new System.Drawing.Point(110, 19);
            this.rdbVerifiedAll.Name = "rdbVerifiedAll";
            this.rdbVerifiedAll.Size = new System.Drawing.Size(36, 17);
            this.rdbVerifiedAll.TabIndex = 2;
            this.rdbVerifiedAll.TabStop = true;
            this.rdbVerifiedAll.Text = "All";
            this.rdbVerifiedAll.UseVisualStyleBackColor = true;
            // 
            // rdbNotVerified
            // 
            this.rdbNotVerified.AutoSize = true;
            this.rdbNotVerified.Location = new System.Drawing.Point(6, 42);
            this.rdbNotVerified.Name = "rdbNotVerified";
            this.rdbNotVerified.Size = new System.Drawing.Size(104, 17);
            this.rdbNotVerified.TabIndex = 1;
            this.rdbNotVerified.Text = "Not Verified Only";
            this.rdbNotVerified.UseVisualStyleBackColor = true;
            // 
            // rdbVerified
            // 
            this.rdbVerified.AutoSize = true;
            this.rdbVerified.Location = new System.Drawing.Point(6, 19);
            this.rdbVerified.Name = "rdbVerified";
            this.rdbVerified.Size = new System.Drawing.Size(84, 17);
            this.rdbVerified.TabIndex = 0;
            this.rdbVerified.Text = "Verified Only";
            this.rdbVerified.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rdbHighAll);
            this.groupBox4.Controls.Add(this.rdbLow);
            this.groupBox4.Controls.Add(this.rdbHigh);
            this.groupBox4.Location = new System.Drawing.Point(680, 18);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(157, 65);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "High";
            // 
            // rdbHighAll
            // 
            this.rdbHighAll.AutoSize = true;
            this.rdbHighAll.Checked = true;
            this.rdbHighAll.Location = new System.Drawing.Point(97, 19);
            this.rdbHighAll.Name = "rdbHighAll";
            this.rdbHighAll.Size = new System.Drawing.Size(36, 17);
            this.rdbHighAll.TabIndex = 2;
            this.rdbHighAll.TabStop = true;
            this.rdbHighAll.Text = "All";
            this.rdbHighAll.UseVisualStyleBackColor = true;
            // 
            // rdbLow
            // 
            this.rdbLow.AutoSize = true;
            this.rdbLow.Location = new System.Drawing.Point(6, 42);
            this.rdbLow.Name = "rdbLow";
            this.rdbLow.Size = new System.Drawing.Size(69, 17);
            this.rdbLow.TabIndex = 1;
            this.rdbLow.Text = "Low Only";
            this.rdbLow.UseVisualStyleBackColor = true;
            // 
            // rdbHigh
            // 
            this.rdbHigh.AutoSize = true;
            this.rdbHigh.Location = new System.Drawing.Point(6, 19);
            this.rdbHigh.Name = "rdbHigh";
            this.rdbHigh.Size = new System.Drawing.Size(71, 17);
            this.rdbHigh.TabIndex = 0;
            this.rdbHigh.Text = "High Only";
            this.rdbHigh.UseVisualStyleBackColor = true;
            // 
            // chkServerHash
            // 
            this.chkServerHash.AutoSize = true;
            this.chkServerHash.Location = new System.Drawing.Point(585, 24);
            this.chkServerHash.Name = "chkServerHash";
            this.chkServerHash.Size = new System.Drawing.Size(85, 17);
            this.chkServerHash.TabIndex = 25;
            this.chkServerHash.Text = "Server Hash";
            this.chkServerHash.UseVisualStyleBackColor = true;
            // 
            // chkServerSeed
            // 
            this.chkServerSeed.AutoSize = true;
            this.chkServerSeed.Location = new System.Drawing.Point(491, 24);
            this.chkServerSeed.Name = "chkServerSeed";
            this.chkServerSeed.Size = new System.Drawing.Size(85, 17);
            this.chkServerSeed.TabIndex = 24;
            this.chkServerSeed.Text = "Server Seed";
            this.chkServerSeed.UseVisualStyleBackColor = true;
            // 
            // chkClientSeed
            // 
            this.chkClientSeed.AutoSize = true;
            this.chkClientSeed.Location = new System.Drawing.Point(402, 24);
            this.chkClientSeed.Name = "chkClientSeed";
            this.chkClientSeed.Size = new System.Drawing.Size(80, 17);
            this.chkClientSeed.TabIndex = 23;
            this.chkClientSeed.Text = "Client Seed";
            this.chkClientSeed.UseVisualStyleBackColor = true;
            // 
            // chkProfit
            // 
            this.chkProfit.AutoSize = true;
            this.chkProfit.Location = new System.Drawing.Point(343, 24);
            this.chkProfit.Name = "chkProfit";
            this.chkProfit.Size = new System.Drawing.Size(50, 17);
            this.chkProfit.TabIndex = 22;
            this.chkProfit.Text = "Profit";
            this.chkProfit.UseVisualStyleBackColor = true;
            // 
            // chkRoll
            // 
            this.chkRoll.AutoSize = true;
            this.chkRoll.Location = new System.Drawing.Point(290, 24);
            this.chkRoll.Name = "chkRoll";
            this.chkRoll.Size = new System.Drawing.Size(44, 17);
            this.chkRoll.TabIndex = 21;
            this.chkRoll.Text = "Roll";
            this.chkRoll.UseVisualStyleBackColor = true;
            // 
            // chkStake
            // 
            this.chkStake.AutoSize = true;
            this.chkStake.Location = new System.Drawing.Point(227, 24);
            this.chkStake.Name = "chkStake";
            this.chkStake.Size = new System.Drawing.Size(54, 17);
            this.chkStake.TabIndex = 20;
            this.chkStake.Text = "Stake";
            this.chkStake.UseVisualStyleBackColor = true;
            // 
            // chkChance
            // 
            this.chkChance.AutoSize = true;
            this.chkChance.Location = new System.Drawing.Point(155, 24);
            this.chkChance.Name = "chkChance";
            this.chkChance.Size = new System.Drawing.Size(63, 17);
            this.chkChance.TabIndex = 19;
            this.chkChance.Text = "Chance";
            this.chkChance.UseVisualStyleBackColor = true;
            // 
            // chkBetId
            // 
            this.chkBetId.AutoSize = true;
            this.chkBetId.Location = new System.Drawing.Point(94, 24);
            this.chkBetId.Name = "chkBetId";
            this.chkBetId.Size = new System.Drawing.Size(56, 17);
            this.chkBetId.TabIndex = 18;
            this.chkBetId.Text = "Bet ID";
            this.chkBetId.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Search in:";
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Bet ID";
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.DataPropertyName = "date";
            this.dateDataGridViewTextBoxColumn.HeaderText = "Date";
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            this.dateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // highDataGridViewCheckBoxColumn
            // 
            this.highDataGridViewCheckBoxColumn.DataPropertyName = "high";
            this.highDataGridViewCheckBoxColumn.HeaderText = "high";
            this.highDataGridViewCheckBoxColumn.Name = "highDataGridViewCheckBoxColumn";
            this.highDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // chanceDataGridViewTextBoxColumn
            // 
            this.chanceDataGridViewTextBoxColumn.DataPropertyName = "Chance";
            this.chanceDataGridViewTextBoxColumn.HeaderText = "Chance";
            this.chanceDataGridViewTextBoxColumn.Name = "chanceDataGridViewTextBoxColumn";
            this.chanceDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // amountDataGridViewTextBoxColumn
            // 
            this.amountDataGridViewTextBoxColumn.DataPropertyName = "Amount";
            this.amountDataGridViewTextBoxColumn.HeaderText = "Stake";
            this.amountDataGridViewTextBoxColumn.Name = "amountDataGridViewTextBoxColumn";
            this.amountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // rollDataGridViewTextBoxColumn
            // 
            this.rollDataGridViewTextBoxColumn.DataPropertyName = "Roll";
            this.rollDataGridViewTextBoxColumn.HeaderText = "Roll";
            this.rollDataGridViewTextBoxColumn.Name = "rollDataGridViewTextBoxColumn";
            this.rollDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // profitDataGridViewTextBoxColumn
            // 
            this.profitDataGridViewTextBoxColumn.DataPropertyName = "Profit";
            this.profitDataGridViewTextBoxColumn.HeaderText = "Profit";
            this.profitDataGridViewTextBoxColumn.Name = "profitDataGridViewTextBoxColumn";
            this.profitDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // verifiedDataGridViewCheckBoxColumn
            // 
            this.verifiedDataGridViewCheckBoxColumn.DataPropertyName = "Verified";
            this.verifiedDataGridViewCheckBoxColumn.HeaderText = "Verified";
            this.verifiedDataGridViewCheckBoxColumn.Name = "verifiedDataGridViewCheckBoxColumn";
            this.verifiedDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // nonceDataGridViewTextBoxColumn
            // 
            this.nonceDataGridViewTextBoxColumn.DataPropertyName = "nonce";
            this.nonceDataGridViewTextBoxColumn.HeaderText = "nonce";
            this.nonceDataGridViewTextBoxColumn.Name = "nonceDataGridViewTextBoxColumn";
            this.nonceDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // clientseedDataGridViewTextBoxColumn
            // 
            this.clientseedDataGridViewTextBoxColumn.DataPropertyName = "clientseed";
            this.clientseedDataGridViewTextBoxColumn.HeaderText = "clientseed";
            this.clientseedDataGridViewTextBoxColumn.Name = "clientseedDataGridViewTextBoxColumn";
            this.clientseedDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // serverseedDataGridViewTextBoxColumn
            // 
            this.serverseedDataGridViewTextBoxColumn.DataPropertyName = "serverseed";
            this.serverseedDataGridViewTextBoxColumn.HeaderText = "serverseed";
            this.serverseedDataGridViewTextBoxColumn.Name = "serverseedDataGridViewTextBoxColumn";
            this.serverseedDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // serverhashDataGridViewTextBoxColumn
            // 
            this.serverhashDataGridViewTextBoxColumn.DataPropertyName = "serverhash";
            this.serverhashDataGridViewTextBoxColumn.HeaderText = "serverhash";
            this.serverhashDataGridViewTextBoxColumn.Name = "serverhashDataGridViewTextBoxColumn";
            this.serverhashDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // betBindingSource
            // 
            this.betBindingSource.DataSource = typeof(DiceBot.Bet);
            // 
            // BetHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1438, 413);
            this.Controls.Add(this.dgvBets);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.panel1);
            this.Name = "BetHistory";
            this.Text = "Bet History";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBets)).EndInit();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
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
        private System.Windows.Forms.BindingSource betBindingSource;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.ComboBox cmbViewPerPage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbJumpTo;
        private System.Windows.Forms.GroupBox pnlSearch;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblSearchDateUntill;
        private System.Windows.Forms.Label lblSearchDateFrom;
        private System.Windows.Forms.DateTimePicker dtpSearchUntil;
        private System.Windows.Forms.DateTimePicker dtpSearchFrom;
        private System.Windows.Forms.RadioButton rdbDateRange;
        private System.Windows.Forms.RadioButton rdbDateAll;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rdbVerifiedAll;
        private System.Windows.Forms.RadioButton rdbNotVerified;
        private System.Windows.Forms.RadioButton rdbVerified;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rdbHighAll;
        private System.Windows.Forms.RadioButton rdbLow;
        private System.Windows.Forms.RadioButton rdbHigh;
        private System.Windows.Forms.CheckBox chkServerHash;
        private System.Windows.Forms.CheckBox chkServerSeed;
        private System.Windows.Forms.CheckBox chkClientSeed;
        private System.Windows.Forms.CheckBox chkProfit;
        private System.Windows.Forms.CheckBox chkRoll;
        private System.Windows.Forms.CheckBox chkStake;
        private System.Windows.Forms.CheckBox chkChance;
        private System.Windows.Forms.CheckBox chkBetId;
        private System.Windows.Forms.Label label6;
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
    }
}