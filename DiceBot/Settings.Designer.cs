namespace DiceBot
{
    partial class Settings
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
            this.groupBox19 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nudLiveBetsNum = new System.Windows.Forms.NumericUpDown();
            this.lblSeedFound = new System.Windows.Forms.Label();
            this.lblSeedProgress = new System.Windows.Forms.Label();
            this.btnGetSeeds = new System.Windows.Forms.Button();
            this.chkAutoSeeds = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label120 = new System.Windows.Forms.Label();
            this.label119 = new System.Windows.Forms.Label();
            this.btnBrowseAlarm = new System.Windows.Forms.Button();
            this.txtPathAlarm = new System.Windows.Forms.TextBox();
            this.btnBrowseChing = new System.Windows.Forms.Button();
            this.txtPathChing = new System.Windows.Forms.TextBox();
            this.nudSoundStreak = new System.Windows.Forms.NumericUpDown();
            this.chkSoundStreak = new System.Windows.Forms.CheckBox();
            this.chkSoundLowLimit = new System.Windows.Forms.CheckBox();
            this.chkAlarm = new System.Windows.Forms.CheckBox();
            this.chkSoundWithdraw = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSMTP = new System.Windows.Forms.Button();
            this.nudEmailStreak = new System.Windows.Forms.NumericUpDown();
            this.chkEmailStreak = new System.Windows.Forms.CheckBox();
            this.chkEmailLowLimit = new System.Windows.Forms.CheckBox();
            this.label48 = new System.Windows.Forms.Label();
            this.chkEmailWithdraw = new System.Windows.Forms.CheckBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.label49 = new System.Windows.Forms.Label();
            this.chkEmail = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label43 = new System.Windows.Forms.Label();
            this.txtJDUser = new System.Windows.Forms.TextBox();
            this.txtJDPass = new System.Windows.Forms.TextBox();
            this.chkJDAutoLogin = new System.Windows.Forms.CheckBox();
            this.chkJDAutoStart = new System.Windows.Forms.CheckBox();
            this.label40 = new System.Windows.Forms.Label();
            this.txtBot = new System.Windows.Forms.TextBox();
            this.label50 = new System.Windows.Forms.Label();
            this.chkTray = new System.Windows.Forms.CheckBox();
            this.btnSaveUser = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nudDonatePercentage = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.rdbDonateAuto = new System.Windows.Forms.RadioButton();
            this.rdbDonateDefault = new System.Windows.Forms.RadioButton();
            this.rdbDonateDont = new System.Windows.Forms.RadioButton();
            this.chkStartup = new System.Windows.Forms.CheckBox();
            this.groupBox19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLiveBetsNum)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoundStreak)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEmailStreak)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDonatePercentage)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.label1);
            this.groupBox19.Controls.Add(this.nudLiveBetsNum);
            this.groupBox19.Controls.Add(this.lblSeedFound);
            this.groupBox19.Controls.Add(this.lblSeedProgress);
            this.groupBox19.Controls.Add(this.btnGetSeeds);
            this.groupBox19.Controls.Add(this.chkAutoSeeds);
            this.groupBox19.Location = new System.Drawing.Point(442, 44);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Size = new System.Drawing.Size(421, 87);
            this.groupBox19.TabIndex = 62;
            this.groupBox19.TabStop = false;
            this.groupBox19.Text = "Bets";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Number of bets to show in the live bet panel:";
            // 
            // nudLiveBetsNum
            // 
            this.nudLiveBetsNum.Location = new System.Drawing.Point(229, 42);
            this.nudLiveBetsNum.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudLiveBetsNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLiveBetsNum.Name = "nudLiveBetsNum";
            this.nudLiveBetsNum.Size = new System.Drawing.Size(54, 20);
            this.nudLiveBetsNum.TabIndex = 4;
            this.nudLiveBetsNum.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudLiveBetsNum.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // lblSeedFound
            // 
            this.lblSeedFound.AutoSize = true;
            this.lblSeedFound.Location = new System.Drawing.Point(101, 51);
            this.lblSeedFound.Name = "lblSeedFound";
            this.lblSeedFound.Size = new System.Drawing.Size(0, 13);
            this.lblSeedFound.TabIndex = 3;
            // 
            // lblSeedProgress
            // 
            this.lblSeedProgress.AutoSize = true;
            this.lblSeedProgress.Location = new System.Drawing.Point(278, 51);
            this.lblSeedProgress.Name = "lblSeedProgress";
            this.lblSeedProgress.Size = new System.Drawing.Size(0, 13);
            this.lblSeedProgress.TabIndex = 2;
            // 
            // btnGetSeeds
            // 
            this.btnGetSeeds.Location = new System.Drawing.Point(281, 15);
            this.btnGetSeeds.Name = "btnGetSeeds";
            this.btnGetSeeds.Size = new System.Drawing.Size(130, 23);
            this.btnGetSeeds.TabIndex = 1;
            this.btnGetSeeds.Text = "Look for seeds now";
            this.btnGetSeeds.UseVisualStyleBackColor = true;
            this.btnGetSeeds.Click += new System.EventHandler(this.btnGetSeeds_Click);
            // 
            // chkAutoSeeds
            // 
            this.chkAutoSeeds.AutoSize = true;
            this.chkAutoSeeds.Location = new System.Drawing.Point(4, 19);
            this.chkAutoSeeds.Name = "chkAutoSeeds";
            this.chkAutoSeeds.Size = new System.Drawing.Size(242, 17);
            this.chkAutoSeeds.TabIndex = 0;
            this.chkAutoSeeds.Text = "Automatically try to get server seed of old bets";
            this.chkAutoSeeds.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label120);
            this.groupBox4.Controls.Add(this.label119);
            this.groupBox4.Controls.Add(this.btnBrowseAlarm);
            this.groupBox4.Controls.Add(this.txtPathAlarm);
            this.groupBox4.Controls.Add(this.btnBrowseChing);
            this.groupBox4.Controls.Add(this.txtPathChing);
            this.groupBox4.Controls.Add(this.nudSoundStreak);
            this.groupBox4.Controls.Add(this.chkSoundStreak);
            this.groupBox4.Controls.Add(this.chkSoundLowLimit);
            this.groupBox4.Controls.Add(this.chkAlarm);
            this.groupBox4.Controls.Add(this.chkSoundWithdraw);
            this.groupBox4.Location = new System.Drawing.Point(13, 280);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(423, 157);
            this.groupBox4.TabIndex = 61;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Sounds";
            // 
            // label120
            // 
            this.label120.AutoSize = true;
            this.label120.Location = new System.Drawing.Point(33, 123);
            this.label120.Name = "label120";
            this.label120.Size = new System.Drawing.Size(55, 13);
            this.label120.TabIndex = 14;
            this.label120.Text = "Alarm File:";
            // 
            // label119
            // 
            this.label119.AutoSize = true;
            this.label119.Location = new System.Drawing.Point(20, 46);
            this.label119.Name = "label119";
            this.label119.Size = new System.Drawing.Size(68, 13);
            this.label119.TabIndex = 13;
            this.label119.Text = "Kachink File:";
            // 
            // btnBrowseAlarm
            // 
            this.btnBrowseAlarm.Location = new System.Drawing.Point(283, 118);
            this.btnBrowseAlarm.Name = "btnBrowseAlarm";
            this.btnBrowseAlarm.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseAlarm.TabIndex = 12;
            this.btnBrowseAlarm.Text = "Browse";
            this.btnBrowseAlarm.UseVisualStyleBackColor = true;
            this.btnBrowseAlarm.Click += new System.EventHandler(this.btnBrowseAlarm_Click);
            // 
            // txtPathAlarm
            // 
            this.txtPathAlarm.Location = new System.Drawing.Point(94, 120);
            this.txtPathAlarm.Name = "txtPathAlarm";
            this.txtPathAlarm.Size = new System.Drawing.Size(183, 20);
            this.txtPathAlarm.TabIndex = 11;
            // 
            // btnBrowseChing
            // 
            this.btnBrowseChing.Location = new System.Drawing.Point(283, 41);
            this.btnBrowseChing.Name = "btnBrowseChing";
            this.btnBrowseChing.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseChing.TabIndex = 10;
            this.btnBrowseChing.Text = "Browse";
            this.btnBrowseChing.UseVisualStyleBackColor = true;
            this.btnBrowseChing.Click += new System.EventHandler(this.btnBrowseChing_Click_1);
            // 
            // txtPathChing
            // 
            this.txtPathChing.Location = new System.Drawing.Point(94, 43);
            this.txtPathChing.Name = "txtPathChing";
            this.txtPathChing.Size = new System.Drawing.Size(183, 20);
            this.txtPathChing.TabIndex = 9;
            // 
            // nudSoundStreak
            // 
            this.nudSoundStreak.Location = new System.Drawing.Point(329, 94);
            this.nudSoundStreak.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudSoundStreak.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudSoundStreak.Name = "nudSoundStreak";
            this.nudSoundStreak.Size = new System.Drawing.Size(35, 20);
            this.nudSoundStreak.TabIndex = 8;
            this.nudSoundStreak.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // chkSoundStreak
            // 
            this.chkSoundStreak.AutoSize = true;
            this.chkSoundStreak.Location = new System.Drawing.Point(202, 97);
            this.chkSoundStreak.Name = "chkSoundStreak";
            this.chkSoundStreak.Size = new System.Drawing.Size(127, 17);
            this.chkSoundStreak.TabIndex = 3;
            this.chkSoundStreak.Text = "Losing Streak above:";
            this.chkSoundStreak.UseVisualStyleBackColor = true;
            // 
            // chkSoundLowLimit
            // 
            this.chkSoundLowLimit.AutoSize = true;
            this.chkSoundLowLimit.Location = new System.Drawing.Point(29, 97);
            this.chkSoundLowLimit.Name = "chkSoundLowLimit";
            this.chkSoundLowLimit.Size = new System.Drawing.Size(121, 17);
            this.chkSoundLowLimit.TabIndex = 2;
            this.chkSoundLowLimit.Text = "Lower Limit reached";
            this.chkSoundLowLimit.UseVisualStyleBackColor = true;
            // 
            // chkAlarm
            // 
            this.chkAlarm.AutoSize = true;
            this.chkAlarm.Location = new System.Drawing.Point(6, 74);
            this.chkAlarm.Name = "chkAlarm";
            this.chkAlarm.Size = new System.Drawing.Size(103, 17);
            this.chkAlarm.TabIndex = 1;
            this.chkAlarm.Text = "Sound alarm for:";
            this.chkAlarm.UseVisualStyleBackColor = true;
            // 
            // chkSoundWithdraw
            // 
            this.chkSoundWithdraw.AutoSize = true;
            this.chkSoundWithdraw.Location = new System.Drawing.Point(7, 20);
            this.chkSoundWithdraw.Name = "chkSoundWithdraw";
            this.chkSoundWithdraw.Size = new System.Drawing.Size(189, 17);
            this.chkSoundWithdraw.TabIndex = 0;
            this.chkSoundWithdraw.Text = "Play KACHINK on withdraw/invest";
            this.chkSoundWithdraw.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSMTP);
            this.groupBox2.Controls.Add(this.nudEmailStreak);
            this.groupBox2.Controls.Add(this.chkEmailStreak);
            this.groupBox2.Controls.Add(this.chkEmailLowLimit);
            this.groupBox2.Controls.Add(this.label48);
            this.groupBox2.Controls.Add(this.chkEmailWithdraw);
            this.groupBox2.Controls.Add(this.txtEmail);
            this.groupBox2.Controls.Add(this.label49);
            this.groupBox2.Controls.Add(this.chkEmail);
            this.groupBox2.Location = new System.Drawing.Point(13, 136);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(423, 138);
            this.groupBox2.TabIndex = 60;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Email Notifications";
            // 
            // btnSMTP
            // 
            this.btnSMTP.Location = new System.Drawing.Point(210, 12);
            this.btnSMTP.Name = "btnSMTP";
            this.btnSMTP.Size = new System.Drawing.Size(75, 23);
            this.btnSMTP.TabIndex = 8;
            this.btnSMTP.Text = "Edit SMTP";
            this.btnSMTP.UseVisualStyleBackColor = true;
            this.btnSMTP.Click += new System.EventHandler(this.btnSMTP_Click);
            // 
            // nudEmailStreak
            // 
            this.nudEmailStreak.Location = new System.Drawing.Point(141, 109);
            this.nudEmailStreak.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudEmailStreak.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudEmailStreak.Name = "nudEmailStreak";
            this.nudEmailStreak.Size = new System.Drawing.Size(35, 20);
            this.nudEmailStreak.TabIndex = 7;
            this.nudEmailStreak.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // chkEmailStreak
            // 
            this.chkEmailStreak.AutoSize = true;
            this.chkEmailStreak.Location = new System.Drawing.Point(6, 110);
            this.chkEmailStreak.Name = "chkEmailStreak";
            this.chkEmailStreak.Size = new System.Drawing.Size(128, 17);
            this.chkEmailStreak.TabIndex = 6;
            this.chkEmailStreak.Text = "Losing Streak Above:";
            this.chkEmailStreak.UseVisualStyleBackColor = true;
            // 
            // chkEmailLowLimit
            // 
            this.chkEmailLowLimit.AutoSize = true;
            this.chkEmailLowLimit.Location = new System.Drawing.Point(141, 87);
            this.chkEmailLowLimit.Name = "chkEmailLowLimit";
            this.chkEmailLowLimit.Size = new System.Drawing.Size(117, 17);
            this.chkEmailLowLimit.TabIndex = 5;
            this.chkEmailLowLimit.Text = "Low Limit Reached";
            this.chkEmailLowLimit.UseVisualStyleBackColor = true;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(6, 68);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(126, 13);
            this.label48.TabIndex = 4;
            this.label48.Text = "Receive Notifications for:";
            // 
            // chkEmailWithdraw
            // 
            this.chkEmailWithdraw.AutoSize = true;
            this.chkEmailWithdraw.Location = new System.Drawing.Point(6, 87);
            this.chkEmailWithdraw.Name = "chkEmailWithdraw";
            this.chkEmailWithdraw.Size = new System.Drawing.Size(105, 17);
            this.chkEmailWithdraw.TabIndex = 3;
            this.chkEmailWithdraw.Text = "Withdraw/Invest";
            this.chkEmailWithdraw.UseVisualStyleBackColor = true;
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(48, 41);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(237, 20);
            this.txtEmail.TabIndex = 2;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(6, 44);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(35, 13);
            this.label49.TabIndex = 1;
            this.label49.Text = "Email:";
            // 
            // chkEmail
            // 
            this.chkEmail.AutoSize = true;
            this.chkEmail.Location = new System.Drawing.Point(6, 20);
            this.chkEmail.Name = "chkEmail";
            this.chkEmail.Size = new System.Drawing.Size(65, 17);
            this.chkEmail.TabIndex = 0;
            this.chkEmail.Text = "Enabled";
            this.chkEmail.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label43);
            this.groupBox1.Controls.Add(this.txtJDUser);
            this.groupBox1.Controls.Add(this.txtJDPass);
            this.groupBox1.Controls.Add(this.chkJDAutoLogin);
            this.groupBox1.Controls.Add(this.chkJDAutoStart);
            this.groupBox1.Controls.Add(this.label40);
            this.groupBox1.Location = new System.Drawing.Point(12, 44);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 86);
            this.groupBox1.TabIndex = 59;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Startup Settings";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(6, 28);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(58, 13);
            this.label43.TabIndex = 28;
            this.label43.Text = "Username:";
            // 
            // txtJDUser
            // 
            this.txtJDUser.Location = new System.Drawing.Point(78, 22);
            this.txtJDUser.Name = "txtJDUser";
            this.txtJDUser.Size = new System.Drawing.Size(107, 20);
            this.txtJDUser.TabIndex = 19;
            // 
            // txtJDPass
            // 
            this.txtJDPass.Location = new System.Drawing.Point(78, 48);
            this.txtJDPass.Name = "txtJDPass";
            this.txtJDPass.PasswordChar = '*';
            this.txtJDPass.Size = new System.Drawing.Size(107, 20);
            this.txtJDPass.TabIndex = 20;
            // 
            // chkJDAutoLogin
            // 
            this.chkJDAutoLogin.AutoSize = true;
            this.chkJDAutoLogin.Location = new System.Drawing.Point(213, 24);
            this.chkJDAutoLogin.Name = "chkJDAutoLogin";
            this.chkJDAutoLogin.Size = new System.Drawing.Size(77, 17);
            this.chkJDAutoLogin.TabIndex = 24;
            this.chkJDAutoLogin.Text = "Auto Login";
            this.chkJDAutoLogin.UseVisualStyleBackColor = true;
            // 
            // chkJDAutoStart
            // 
            this.chkJDAutoStart.AutoSize = true;
            this.chkJDAutoStart.Location = new System.Drawing.Point(213, 50);
            this.chkJDAutoStart.Name = "chkJDAutoStart";
            this.chkJDAutoStart.Size = new System.Drawing.Size(73, 17);
            this.chkJDAutoStart.TabIndex = 25;
            this.chkJDAutoStart.Text = "Auto Start";
            this.chkJDAutoStart.UseVisualStyleBackColor = true;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(6, 51);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(56, 13);
            this.label40.TabIndex = 29;
            this.label40.Text = "Password:";
            // 
            // txtBot
            // 
            this.txtBot.Location = new System.Drawing.Point(203, 9);
            this.txtBot.Name = "txtBot";
            this.txtBot.Size = new System.Drawing.Size(120, 20);
            this.txtBot.TabIndex = 58;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(140, 12);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(57, 13);
            this.label50.TabIndex = 57;
            this.label50.Text = "Bot Name:";
            // 
            // chkTray
            // 
            this.chkTray.AutoSize = true;
            this.chkTray.Location = new System.Drawing.Point(12, 12);
            this.chkTray.Name = "chkTray";
            this.chkTray.Size = new System.Drawing.Size(98, 17);
            this.chkTray.TabIndex = 56;
            this.chkTray.Text = "Minimize to tray";
            this.chkTray.UseVisualStyleBackColor = true;
            // 
            // btnSaveUser
            // 
            this.btnSaveUser.Location = new System.Drawing.Point(759, 414);
            this.btnSaveUser.Name = "btnSaveUser";
            this.btnSaveUser.Size = new System.Drawing.Size(104, 23);
            this.btnSaveUser.TabIndex = 55;
            this.btnSaveUser.Text = "Save Settings";
            this.btnSaveUser.UseVisualStyleBackColor = true;
            this.btnSaveUser.Click += new System.EventHandler(this.btnSaveUser_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.nudDonatePercentage);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.rdbDonateAuto);
            this.groupBox3.Controls.Add(this.rdbDonateDefault);
            this.groupBox3.Controls.Add(this.rdbDonateDont);
            this.groupBox3.Location = new System.Drawing.Point(442, 137);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(421, 126);
            this.groupBox3.TabIndex = 63;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Donate";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(229, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "%";
            // 
            // nudDonatePercentage
            // 
            this.nudDonatePercentage.DecimalPlaces = 3;
            this.nudDonatePercentage.Location = new System.Drawing.Point(126, 93);
            this.nudDonatePercentage.Name = "nudDonatePercentage";
            this.nudDonatePercentage.Size = new System.Drawing.Size(97, 20);
            this.nudDonatePercentage.TabIndex = 4;
            this.nudDonatePercentage.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Donation Percentage:";
            // 
            // rdbDonateAuto
            // 
            this.rdbDonateAuto.AutoSize = true;
            this.rdbDonateAuto.Location = new System.Drawing.Point(13, 64);
            this.rdbDonateAuto.Name = "rdbDonateAuto";
            this.rdbDonateAuto.Size = new System.Drawing.Size(300, 17);
            this.rdbDonateAuto.TabIndex = 2;
            this.rdbDonateAuto.TabStop = true;
            this.rdbDonateAuto.Text = "Automatically donate this percentage when closing the bot";
            this.rdbDonateAuto.UseVisualStyleBackColor = true;
            // 
            // rdbDonateDefault
            // 
            this.rdbDonateDefault.AutoSize = true;
            this.rdbDonateDefault.Checked = true;
            this.rdbDonateDefault.Location = new System.Drawing.Point(13, 41);
            this.rdbDonateDefault.Name = "rdbDonateDefault";
            this.rdbDonateDefault.Size = new System.Drawing.Size(311, 17);
            this.rdbDonateDefault.TabIndex = 1;
            this.rdbDonateDefault.TabStop = true;
            this.rdbDonateDefault.Text = "Use this default percentage when showing the donate dialog";
            this.rdbDonateDefault.UseVisualStyleBackColor = true;
            // 
            // rdbDonateDont
            // 
            this.rdbDonateDont.AutoSize = true;
            this.rdbDonateDont.Location = new System.Drawing.Point(13, 18);
            this.rdbDonateDont.Name = "rdbDonateDont";
            this.rdbDonateDont.Size = new System.Drawing.Size(273, 17);
            this.rdbDonateDont.TabIndex = 0;
            this.rdbDonateDont.Text = "Do not Show the donate dialog when closign the bot";
            this.rdbDonateDont.UseVisualStyleBackColor = true;
            // 
            // chkStartup
            // 
            this.chkStartup.AutoSize = true;
            this.chkStartup.Checked = true;
            this.chkStartup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStartup.Location = new System.Drawing.Point(442, 12);
            this.chkStartup.Name = "chkStartup";
            this.chkStartup.Size = new System.Drawing.Size(133, 17);
            this.chkStartup.TabIndex = 64;
            this.chkStartup.Text = "Show startup message";
            this.chkStartup.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 450);
            this.Controls.Add(this.chkStartup);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox19);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtBot);
            this.Controls.Add(this.label50);
            this.Controls.Add(this.chkTray);
            this.Controls.Add(this.btnSaveUser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Settings";
            this.Text = "Settings";
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLiveBetsNum)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoundStreak)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEmailStreak)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDonatePercentage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox19;
        private System.Windows.Forms.Label lblSeedFound;
        private System.Windows.Forms.Label lblSeedProgress;
        private System.Windows.Forms.Button btnGetSeeds;
        public System.Windows.Forms.CheckBox chkAutoSeeds;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label120;
        private System.Windows.Forms.Label label119;
        private System.Windows.Forms.Button btnBrowseAlarm;
        public System.Windows.Forms.TextBox txtPathAlarm;
        private System.Windows.Forms.Button btnBrowseChing;
        public System.Windows.Forms.TextBox txtPathChing;
        public System.Windows.Forms.NumericUpDown nudSoundStreak;
        public System.Windows.Forms.CheckBox chkSoundStreak;
        public System.Windows.Forms.CheckBox chkSoundLowLimit;
        public System.Windows.Forms.CheckBox chkAlarm;
        public System.Windows.Forms.CheckBox chkSoundWithdraw;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSMTP;
        public System.Windows.Forms.NumericUpDown nudEmailStreak;
        public System.Windows.Forms.CheckBox chkEmailStreak;
        public System.Windows.Forms.CheckBox chkEmailLowLimit;
        private System.Windows.Forms.Label label48;
        public System.Windows.Forms.CheckBox chkEmailWithdraw;
        public System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label label49;
        public System.Windows.Forms.CheckBox chkEmail;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label43;
        public System.Windows.Forms.TextBox txtJDUser;
        public System.Windows.Forms.TextBox txtJDPass;
        public System.Windows.Forms.CheckBox chkJDAutoLogin;
        public System.Windows.Forms.CheckBox chkJDAutoStart;
        private System.Windows.Forms.Label label40;
        public System.Windows.Forms.TextBox txtBot;
        private System.Windows.Forms.Label label50;
        public System.Windows.Forms.CheckBox chkTray;
        private System.Windows.Forms.Button btnSaveUser;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown nudLiveBetsNum;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.NumericUpDown nudDonatePercentage;
        public System.Windows.Forms.RadioButton rdbDonateAuto;
        public System.Windows.Forms.RadioButton rdbDonateDefault;
        public System.Windows.Forms.RadioButton rdbDonateDont;
        public System.Windows.Forms.CheckBox chkStartup;
    }
}