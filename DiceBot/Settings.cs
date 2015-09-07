using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DiceBot
{
    public partial class Settings : Form
    {
        new cDiceBot Parent;
        public Settings(cDiceBot Parent)
        {
            InitializeComponent();
            this.Parent = Parent;
            loadsettings();
        }
        
        
        private void btnSaveUser_Click(object sender, EventArgs e)
        {
            this.DialogResult =  System.Windows.Forms.DialogResult.OK;
        }
        public string ching { get; set; }
        public string salarm { get; set; }
        private void btnBrowseChing_Click(object sender, EventArgs e)
        {
            bool ching = ((sender as Button).Name == "btnBrowseChing");
            OpenFileDialog ofdSound = new OpenFileDialog();
            ofdSound.CheckFileExists = true;
            ofdSound.Filter = "MP3|*.mp3|Wave Files|*.wav";
            if (ofdSound.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ching)
                {
                    this.ching = ofdSound.FileName;
                    txtPathChing.Text = this.ching;
                }
                else
                {
                    salarm = ofdSound.FileName;
                    txtPathAlarm.Text = salarm;
                }
            }
        }

        public void loadsettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
                {

                    string test = sr.ReadLine();
                    if (test == "new")
                        
                    
                    {
                        string info = sr.ReadLine();
                        string[] chars = info.Split(' ');
                        string suser = "";
                        string spass = "";
                        bool login = false;
                        bool start = false;
                        int word = 0;
                        for (int i = 0; i < chars.Length; i++)
                        {
                            int num = 0;
                            if (int.TryParse(chars[i], out num))
                            {
                                if ((char)num == ',' || (char)num == ';')
                                    word++;
                                else
                                    switch (word)
                                    {
                                        case 0: suser += (char)num; break;
                                        case 1: spass += (char)num; break;
                                        case 2: if ((char)num == '1') login = true; else login = false; break;
                                        case 3: if ((char)num == '1') start = true; else start = false; break;
                                    }
                            }
                        }
                        chkJDAutoLogin.Checked = login;
                        txtJDPass.Text = spass;
                        txtJDUser.Text = suser;
                        chkJDAutoStart.Checked = start;
                        if (word == 2)
                        {

                            info = sr.ReadLine();
                            chars = info.Split(' ');
                            suser = "";
                            spass = "";

                            login = false;
                            word = 0;
                            for (int i = 0; i < chars.Length; i++)
                            {
                                int num = 0;
                                if (int.TryParse(chars[i], out num))
                                {
                                    if ((char)num == ',')
                                        word++;
                                    else
                                        switch (word)
                                        {
                                            case 0: suser += (char)num; break;
                                            case 1: spass += (char)num; break;
                                            case 2: if ((char)num == '1') login = true; else login = false; break;
                                            case 3: if ((char)num == '1') start = true; else start = false; break;
                                        }
                                }
                            }
                            chkJDAutoLogin.Checked = login;
                            txtJDPass.Text = spass;
                            txtJDUser.Text = suser;
                            chkJDAutoStart.Checked = start;
                        }
                        List<SavedItem> saveditems = new List<SavedItem>();
                        while (!sr.EndOfStream)
                        {
                            string[] temp = sr.ReadLine().Split('|');
                            saveditems.Add(new SavedItem(temp[0],temp[1]));
                        }
                        //string msg = "";
                        
                        chkTray.Checked  = ("1"==Parent.getvalue(saveditems, "Tray"));
                        txtBot.Text = Parent.getvalue(saveditems, "BotName");
                        chkEmail.Checked = ("1" == Parent.getvalue(saveditems, "enableEmail"));
                        txtEmail.Text = Parent.getvalue(saveditems, "emailaddress");
                        chkEmailWithdraw.Checked = ("1" == Parent.getvalue(saveditems, "emailwithdraw"));
                        chkEmailLowLimit.Checked = ("1" == Parent.getvalue(saveditems, "emaillow"));
                        chkEmailStreak.Checked = ("1" == Parent.getvalue(saveditems, "emailstreak"));
                        nudEmailStreak.Value = (decimal)Parent.iparse(Parent.getvalue(saveditems, "emailstreakval"));
                        Parent.Emails.SMTP = Parent.getvalue(saveditems, "SMTP");

                        chkSoundWithdraw.Checked = ("1" == Parent.getvalue(saveditems, "CoinEnabled"));
                        txtPathChing.Text = Parent.getvalue(saveditems, "CoinPath");
                        chkAlarm.Checked = ("1" == Parent.getvalue(saveditems, "AlarmEnabled"));
                        chkSoundLowLimit.Checked = ("1" == Parent.getvalue(saveditems, "AlarmLowEnabled"));
                        chkSoundStreak.Checked = ("1" == Parent.getvalue(saveditems, "AlarmStreakEnabled"));
                        nudSoundStreak.Value = (decimal)Parent.iparse(Parent.getvalue(saveditems, "AlarmStreakValue"));
                        txtPathAlarm.Text = Parent.getvalue(saveditems, "AlarmPath");
                        //Emails.StreakSize = (int)Emails.StreakSize;
                        chkAutoSeeds.Checked = Parent.getvalue(saveditems, "AutoGetSeed") != "0";
                        nudLiveBetsNum.Value = (decimal)Parent.iparse(Parent.getvalue(saveditems, "NumLiveBets"));
                        bool convert = false;
                        nudDonatePercentage.Value = (decimal)Parent.dparse(Parent.getvalue(saveditems, "DonatePercentage"), ref convert);
                        chkStartup.Checked = Parent.getvalue(saveditems, "StartupMessage")=="1";
                        string tmp = Parent.getvalue(saveditems, "DonateMode");
                        rdbDonateDont.Checked = tmp == "1";
                        rdbDonateDefault.Checked = tmp == "2";
                        rdbDonateAuto.Checked = tmp == "3";
                    }

                }

                
                /*chkAlarm.Checked = Sound;
                chkEmail.Checked = Emails.Enable;
                chkEmailLowLimit.Checked = Emails.Lower;
                chkEmailStreak.Checked = Emails.Streak;
                chkEmailWithdraw.Checked = Emails.Withdraw;
                
                
                
                chkSoundLowLimit.Checked = SoundLow;
                chkSoundStreak.Checked = SoundStreak;
                chkSoundWithdraw.Checked = SoundWithdraw;
                chkTray.Checked = tray;
                txtBot.Text = Text;
                txtEmail.Text = Emails.emailaddress;
                txtJDPass.Text = password;
                txtJDUser.Text = username;
                */
                
            }

            catch
            {

            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (nudLiveBetsNum.Value>500)
            {
                MessageBox.Show("Warning! Bot may become slow and unresponsive if too many bets are displayed.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSMTP_Click(object sender, EventArgs e)
        {
            string smtp = Interaction.InputBox("Enter new smtp server address", "SMTP", "smtp.secrueserver.net");
            Parent.Emails.SMTP = smtp;
        }

        private void btnBrowseChing_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofdChing = new OpenFileDialog();
            ofdChing.Filter = "sound Files (*.mp3,*.wav)|*.mp3;*.wav";
            if (ofdChing.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofdChing.FileName))
                {
                    txtPathChing.Text = ofdChing.FileName;
                }
            }
        }

        private void btnBrowseAlarm_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdChing = new OpenFileDialog();
            ofdChing.Filter = "sound Files (*.mp3,*.wav)|*.mp3;*.wav";
            if (ofdChing.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofdChing.FileName))
                {
                    txtPathAlarm.Text = ofdChing.FileName;
                }
            }
        }

        private void btnGetSeeds_Click(object sender, EventArgs e)
        {
            Parent.btnGetSeeds_Click(btnGetSeeds, e);
        }
    }
}
