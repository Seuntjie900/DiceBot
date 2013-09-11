using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic;

namespace DiceBot
{
    public partial class cSettings : Form
    {
        cDiceBot Owner;
        public cSettings(cDiceBot owner)
        {
            InitializeComponent();
            Owner = owner;

            chkAlarm.Checked = owner.Sound;
            chkEmail.Checked = owner.Emails.Enable;
            chkEmailLowLimit.Checked = owner.Emails.Lower;
            chkEmailStreak.Checked = owner.Emails.Streak;
            chkEmailWithdraw.Checked = owner.Emails.Withdraw;
            chkJDAutoLogin.Checked = owner.autologin;
            chkJDAutoStart.Checked = owner.autostart;
            chkNCPAutoLogin.Checked = owner.NCPAutoLogin;
            chkSoundLowLimit.Checked = owner.SoundLow;
            chkSoundStreak.Checked = owner.SoundStreak;
            chkSoundWithdraw.Checked = owner.SoundWithdraw;
            chkTray.Checked = owner.tray;
            txtBot.Text = owner.Text;
            txtEmail.Text = owner.Emails.emailaddress;
            txtJDPass.Text = owner.password;
            txtJDUser.Text = owner.username;
            txtNCPPass.Text = owner.NCPpassword;
            txtNCPUser.Text = owner.NCPusername;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            /*Owner.Botname = txtBot.Text;
            Owner.username = txtJDUser.Text;
            Owner.password = txtJDPass.Text;
            Owner.autologin = chkJDAutoLogin.Checked;
            Owner.autostart = chkJDAutoStart.Checked;
            Owner.tray = chkTray.Checked;
            Email emails = new Email(txtBot.Text, txtEmail.Text);
            if (chkEmail.Checked)
            {
                if (chkEmailWithdraw.Checked) emails.Withdraw = true; else emails.Withdraw = false;
                if (chkEmailLowLimit.Checked) emails.Lower = true; else emails.Lower = false;
                if (chkEmailStreak.Checked) emails.Streak = true; else emails.Streak = false;
                emails.StreakSize = (int)nudEmailStreak.Value;
            }
            else
            {
                emails.Lower = false;
                emails.Streak = false;
                emails.Withdraw = false;
                emails.StreakSize=(int)nudEmailStreak.Value;
            }
            Owner.Emails = emails;*/

            writesettings();
            Owner.loadsettings();
            base.OnClosing(e);
        }

        void writesettings()
        {
            using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
            {
                
                //NCPuser,ncppass,autologin
                string temp1 = txtNCPUser.Text+","+txtNCPPass.Text+",";
                if (chkNCPAutoLogin.Checked)
                    temp1 += "1";
                else temp1 += "0";
                string ncpline = "";

                foreach (char c in temp1)
                {
                    ncpline += ((int)c).ToString()+" ";
                }
                sw.WriteLine(ncpline);

                //JDuser,JDPass,AutoLogin,AutoStart
                string temp2 = txtJDUser.Text + "," + txtJDPass.Text + ",";
                if (chkJDAutoLogin.Checked)
                    temp2 += "1,";
                else temp2 += "0";
                if (chkJDAutoStart.Checked)
                    temp2 += "1,";
                else temp2 += "0";
                string jdline = "";

                foreach (char c in temp2)
                {
                    jdline += ((int)c).ToString() + " ";
                }
                sw.WriteLine(jdline);

                //tray,botname,enableemail,emailaddress,emailwithdraw,emailinvest,emaillow,emailstreak,emailstreakval
                string temp3 = "";
                if (chkTray.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                temp3 += txtBot.Text + ",";
                if (chkEmail.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                temp3 += txtEmail.Text + ",";
                if (chkEmailWithdraw.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                if (chkEmailLowLimit.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                if (chkEmailStreak.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                temp3 += nudEmailStreak.Value.ToString();
                temp3 += "," + Owner.Emails.SMTP;
                sw.WriteLine(temp3);

                //soundcoin,soundalarm,soundlower,soundstrea,soundstreakvalue
                temp3 = "";
                if (chkSoundWithdraw.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                if (chkAlarm.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                if (chkSoundLowLimit.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                if (chkSoundStreak.Checked)
                    temp3 += "1,";
                else temp3 += "0,";
                temp3 += nudSoundStreak.Value.ToString();
                sw.WriteLine(temp3);
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSMTP_Click(object sender, EventArgs e)
        {
            string smtp = Interaction.InputBox("Enter new smtp server address", "SMTP", "smtp.secrueserver.net");
            Owner.Emails.SMTP = smtp;
        }

        
    }
}
