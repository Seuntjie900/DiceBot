namespace DiceBot
{
    partial class cSettings
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
            this.chkTray = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBot = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSMTP = new System.Windows.Forms.Button();
            this.nudEmailStreak = new System.Windows.Forms.NumericUpDown();
            this.chkEmailStreak = new System.Windows.Forms.CheckBox();
            this.chkEmailLowLimit = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkEmailWithdraw = new System.Windows.Forms.CheckBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkEmail = new System.Windows.Forms.CheckBox();
            this.grpbxUser = new System.Windows.Forms.GroupBox();
            this.txtNCPUser = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkJDAutoStart = new System.Windows.Forms.CheckBox();
            this.chkJDAutoLogin = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkNCPAutoLogin = new System.Windows.Forms.CheckBox();
            this.txtJDPass = new System.Windows.Forms.TextBox();
            this.txtJDUser = new System.Windows.Forms.TextBox();
            this.txtNCPPass = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.nudSoundStreak = new System.Windows.Forms.NumericUpDown();
            this.chkSoundStreak = new System.Windows.Forms.CheckBox();
            this.chkSoundLowLimit = new System.Windows.Forms.CheckBox();
            this.chkAlarm = new System.Windows.Forms.CheckBox();
            this.chkSoundWithdraw = new System.Windows.Forms.CheckBox();
            this.btnDone = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEmailStreak)).BeginInit();
            this.grpbxUser.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoundStreak)).BeginInit();
            this.SuspendLayout();
            // 
            // chkTray
            // 
            this.chkTray.AutoSize = true;
            this.chkTray.Location = new System.Drawing.Point(6, 19);
            this.chkTray.Name = "chkTray";
            this.chkTray.Size = new System.Drawing.Size(98, 17);
            this.chkTray.TabIndex = 41;
            this.chkTray.Text = "Minimize to tray";
            this.chkTray.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBot);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chkTray);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 48);
            this.groupBox1.TabIndex = 42;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // txtBot
            // 
            this.txtBot.Location = new System.Drawing.Point(283, 17);
            this.txtBot.Name = "txtBot";
            this.txtBot.Size = new System.Drawing.Size(120, 20);
            this.txtBot.TabIndex = 43;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(220, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 42;
            this.label3.Text = "Bot Name:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSMTP);
            this.groupBox2.Controls.Add(this.nudEmailStreak);
            this.groupBox2.Controls.Add(this.chkEmailStreak);
            this.groupBox2.Controls.Add(this.chkEmailLowLimit);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.chkEmailWithdraw);
            this.groupBox2.Controls.Add(this.txtEmail);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.chkEmail);
            this.groupBox2.Location = new System.Drawing.Point(12, 66);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 181);
            this.groupBox2.TabIndex = 43;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Email Notifications";
            // 
            // btnSMTP
            // 
            this.btnSMTP.Location = new System.Drawing.Point(117, 16);
            this.btnSMTP.Name = "btnSMTP";
            this.btnSMTP.Size = new System.Drawing.Size(75, 23);
            this.btnSMTP.TabIndex = 8;
            this.btnSMTP.Text = "Edit SMTP";
            this.btnSMTP.UseVisualStyleBackColor = true;
            this.btnSMTP.Click += new System.EventHandler(this.btnSMTP_Click);
            // 
            // nudEmailStreak
            // 
            this.nudEmailStreak.Location = new System.Drawing.Point(141, 134);
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
            this.chkEmailStreak.Location = new System.Drawing.Point(6, 134);
            this.chkEmailStreak.Name = "chkEmailStreak";
            this.chkEmailStreak.Size = new System.Drawing.Size(128, 17);
            this.chkEmailStreak.TabIndex = 6;
            this.chkEmailStreak.Text = "Losing Streak Above:";
            this.chkEmailStreak.UseVisualStyleBackColor = true;
            // 
            // chkEmailLowLimit
            // 
            this.chkEmailLowLimit.AutoSize = true;
            this.chkEmailLowLimit.Location = new System.Drawing.Point(6, 110);
            this.chkEmailLowLimit.Name = "chkEmailLowLimit";
            this.chkEmailLowLimit.Size = new System.Drawing.Size(117, 17);
            this.chkEmailLowLimit.TabIndex = 5;
            this.chkEmailLowLimit.Text = "Low Limit Reached";
            this.chkEmailLowLimit.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Receive Notifications for:";
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
            this.txtEmail.Size = new System.Drawing.Size(146, 20);
            this.txtEmail.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Email:";
            // 
            // chkEmail
            // 
            this.chkEmail.AutoSize = true;
            this.chkEmail.Location = new System.Drawing.Point(7, 20);
            this.chkEmail.Name = "chkEmail";
            this.chkEmail.Size = new System.Drawing.Size(65, 17);
            this.chkEmail.TabIndex = 0;
            this.chkEmail.Text = "Enabled";
            this.chkEmail.UseVisualStyleBackColor = true;
            // 
            // grpbxUser
            // 
            this.grpbxUser.Controls.Add(this.txtNCPUser);
            this.grpbxUser.Controls.Add(this.label9);
            this.grpbxUser.Controls.Add(this.label8);
            this.grpbxUser.Controls.Add(this.label7);
            this.grpbxUser.Controls.Add(this.label6);
            this.grpbxUser.Controls.Add(this.chkJDAutoStart);
            this.grpbxUser.Controls.Add(this.chkJDAutoLogin);
            this.grpbxUser.Controls.Add(this.label5);
            this.grpbxUser.Controls.Add(this.label4);
            this.grpbxUser.Controls.Add(this.chkNCPAutoLogin);
            this.grpbxUser.Controls.Add(this.txtJDPass);
            this.grpbxUser.Controls.Add(this.txtJDUser);
            this.grpbxUser.Controls.Add(this.txtNCPPass);
            this.grpbxUser.Location = new System.Drawing.Point(221, 66);
            this.grpbxUser.Name = "grpbxUser";
            this.grpbxUser.Size = new System.Drawing.Size(200, 291);
            this.grpbxUser.TabIndex = 44;
            this.grpbxUser.TabStop = false;
            this.grpbxUser.Text = "User Settings";
            this.grpbxUser.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // txtNCPUser
            // 
            this.txtNCPUser.Location = new System.Drawing.Point(81, 46);
            this.txtNCPUser.Name = "txtNCPUser";
            this.txtNCPUser.Size = new System.Drawing.Size(100, 20);
            this.txtNCPUser.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 182);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Password:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 159);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Username:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 75);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Password:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Username:";
            // 
            // chkJDAutoStart
            // 
            this.chkJDAutoStart.AutoSize = true;
            this.chkJDAutoStart.Location = new System.Drawing.Point(43, 233);
            this.chkJDAutoStart.Name = "chkJDAutoStart";
            this.chkJDAutoStart.Size = new System.Drawing.Size(73, 17);
            this.chkJDAutoStart.TabIndex = 12;
            this.chkJDAutoStart.Text = "Auto Start";
            this.chkJDAutoStart.UseVisualStyleBackColor = true;
            // 
            // chkJDAutoLogin
            // 
            this.chkJDAutoLogin.AutoSize = true;
            this.chkJDAutoLogin.Location = new System.Drawing.Point(44, 210);
            this.chkJDAutoLogin.Name = "chkJDAutoLogin";
            this.chkJDAutoLogin.Size = new System.Drawing.Size(77, 17);
            this.chkJDAutoLogin.TabIndex = 11;
            this.chkJDAutoLogin.Text = "Auto Login";
            this.chkJDAutoLogin.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Just-Dice Settings";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "NetCode Pool Settings";
            // 
            // chkNCPAutoLogin
            // 
            this.chkNCPAutoLogin.AutoSize = true;
            this.chkNCPAutoLogin.Location = new System.Drawing.Point(44, 98);
            this.chkNCPAutoLogin.Name = "chkNCPAutoLogin";
            this.chkNCPAutoLogin.Size = new System.Drawing.Size(77, 17);
            this.chkNCPAutoLogin.TabIndex = 8;
            this.chkNCPAutoLogin.Text = "Auto Login";
            this.chkNCPAutoLogin.UseVisualStyleBackColor = true;
            // 
            // txtJDPass
            // 
            this.txtJDPass.Location = new System.Drawing.Point(81, 179);
            this.txtJDPass.Name = "txtJDPass";
            this.txtJDPass.PasswordChar = '*';
            this.txtJDPass.Size = new System.Drawing.Size(100, 20);
            this.txtJDPass.TabIndex = 7;
            // 
            // txtJDUser
            // 
            this.txtJDUser.Location = new System.Drawing.Point(81, 153);
            this.txtJDUser.Name = "txtJDUser";
            this.txtJDUser.Size = new System.Drawing.Size(100, 20);
            this.txtJDUser.TabIndex = 5;
            // 
            // txtNCPPass
            // 
            this.txtNCPPass.Location = new System.Drawing.Point(81, 72);
            this.txtNCPPass.Name = "txtNCPPass";
            this.txtNCPPass.PasswordChar = '*';
            this.txtNCPPass.Size = new System.Drawing.Size(100, 20);
            this.txtNCPPass.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.nudSoundStreak);
            this.groupBox4.Controls.Add(this.chkSoundStreak);
            this.groupBox4.Controls.Add(this.chkSoundLowLimit);
            this.groupBox4.Controls.Add(this.chkAlarm);
            this.groupBox4.Controls.Add(this.chkSoundWithdraw);
            this.groupBox4.Location = new System.Drawing.Point(12, 233);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(200, 124);
            this.groupBox4.TabIndex = 45;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Sounds";
            // 
            // nudSoundStreak
            // 
            this.nudSoundStreak.Location = new System.Drawing.Point(157, 80);
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
            this.chkSoundStreak.Location = new System.Drawing.Point(30, 83);
            this.chkSoundStreak.Name = "chkSoundStreak";
            this.chkSoundStreak.Size = new System.Drawing.Size(127, 17);
            this.chkSoundStreak.TabIndex = 3;
            this.chkSoundStreak.Text = "Losing Streak above:";
            this.chkSoundStreak.UseVisualStyleBackColor = true;
            // 
            // chkSoundLowLimit
            // 
            this.chkSoundLowLimit.AutoSize = true;
            this.chkSoundLowLimit.Location = new System.Drawing.Point(30, 67);
            this.chkSoundLowLimit.Name = "chkSoundLowLimit";
            this.chkSoundLowLimit.Size = new System.Drawing.Size(121, 17);
            this.chkSoundLowLimit.TabIndex = 2;
            this.chkSoundLowLimit.Text = "Lower Limit reached";
            this.chkSoundLowLimit.UseVisualStyleBackColor = true;
            // 
            // chkAlarm
            // 
            this.chkAlarm.AutoSize = true;
            this.chkAlarm.Location = new System.Drawing.Point(7, 44);
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
            // btnDone
            // 
            this.btnDone.Location = new System.Drawing.Point(343, 363);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(75, 23);
            this.btnDone.TabIndex = 46;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // cSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 391);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.grpbxUser);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "cSettings";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudEmailStreak)).EndInit();
            this.grpbxUser.ResumeLayout(false);
            this.grpbxUser.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoundStreak)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkTray;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtBot;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown nudEmailStreak;
        private System.Windows.Forms.CheckBox chkEmailStreak;
        private System.Windows.Forms.CheckBox chkEmailLowLimit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkEmailWithdraw;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkEmail;
        private System.Windows.Forms.GroupBox grpbxUser;
        private System.Windows.Forms.TextBox txtNCPPass;
        private System.Windows.Forms.CheckBox chkNCPAutoLogin;
        private System.Windows.Forms.TextBox txtJDPass;
        private System.Windows.Forms.TextBox txtJDUser;
        private System.Windows.Forms.CheckBox chkJDAutoStart;
        private System.Windows.Forms.CheckBox chkJDAutoLogin;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown nudSoundStreak;
        private System.Windows.Forms.CheckBox chkSoundStreak;
        private System.Windows.Forms.CheckBox chkSoundLowLimit;
        private System.Windows.Forms.CheckBox chkAlarm;
        private System.Windows.Forms.CheckBox chkSoundWithdraw;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtNCPUser;
        private System.Windows.Forms.Button btnSMTP;
    }
}