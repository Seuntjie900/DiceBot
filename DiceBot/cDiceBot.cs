using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Skybound.Gecko;
using Skybound;
using Skybound.Gecko.DOM;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using System.IO;
using System.Resources;
using System.Net;
using System.Media;
namespace DiceBot
{
    
    public partial class cDiceBot : Form
    {

        #region Variables
        double StartBalance = 0;        
        double Lastbet = 0;
        double MinBet = 0;
        double Multiplier = 0;
        double Limit = 0;
        double Amount = 0;
        double loading = 0;
        double loadammount = 2;
        double BiggestBest = 0;
        double LowerLimit = 0;
        double Devider = 0;
        double Chance = 0;
        int Wins = 0;
        int Losses = 0;
        int Winstreak = 0;
        int BestStreak = 0;
        int WorstStreak = 0;
        int Losestreak = 0;
        int timecounter = 0;
        int waiter = 0;
        int iMultiplyCounter = 0;
        int MaxMultiplies = 0;
        int Devidecounter = 0;
        int SoundStreakCount = 15;
        int restartcounter = 0;
        bool stop = true;
        bool withdraw = false;
        bool invest = false;
        bool running = false;
        bool stoponwin = false;
        bool loggedin = false;
        public bool tray = false;
        public bool Sound = true;
        public bool SoundWithdraw=true;
        public bool SoundLow = true;
        public bool SoundStreak = false;
        public bool autologin = false;
        public bool autostart = false;
        public bool NCPAutoLogin = false;
        private bool withdrew;
        DateTime dtStarted = new DateTime();
        DateTime dtLastBet = new DateTime();
        TimeSpan TotalTime = new TimeSpan(0, 0, 0);
        public string username = "";
        public string password = "";
        public string NCPusername = "";
        public string NCPpassword = "";
        public string Botname = "";
        public Email Emails { get; set; }
        #endregion

        private double dPreviousBalance;
        

        public double PreviousBalance
        {
            get { return dPreviousBalance; }
            set 
            {
                DoBet(value);
                dPreviousBalance = value; 
            }
        }

        public cDiceBot()
        {
            
            InitializeComponent();

            
            

            #region tooltip events
            lblLowLimit.MouseEnter += Labels_MouseEnter;
            lblAction.MouseEnter += Labels_MouseEnter;
            lblAmount.MouseEnter += Labels_MouseEnter;
            lblAddress.MouseEnter += Labels_MouseEnter;
            lblMinBet.MouseEnter += Labels_MouseEnter;
            lblMultiplier.MouseEnter += Labels_MouseEnter;
            lblMaxMultiplier.MouseEnter += Labels_MouseEnter;
            lblAfter.MouseEnter += Labels_MouseEnter;
            lblAfter2.MouseEnter += Labels_MouseEnter;
            lblBets.MouseEnter += Labels_MouseEnter;
            lblDevider.MouseEnter += Labels_MouseEnter;

            lblLowLimit.MouseLeave += Labels_MouseLeave;
            lblAction.MouseLeave += Labels_MouseLeave;
            lblAmount.MouseLeave += Labels_MouseLeave;
            lblAddress.MouseLeave += Labels_MouseLeave;
            lblMinBet.MouseLeave += Labels_MouseLeave;
            lblMultiplier.MouseLeave += Labels_MouseLeave;
            lblMaxMultiplier.MouseLeave += Labels_MouseLeave;
            lblAfter.MouseLeave += Labels_MouseLeave;
            lblAfter2.MouseLeave += Labels_MouseLeave;
            lblBets.MouseLeave += Labels_MouseLeave;
            lblDevider.MouseLeave += Labels_MouseLeave;
            #endregion


            lblTip.Location = new Point(999, 999);
            Xpcom.Initialize(Environment.CurrentDirectory + "\\xulrunner\\");
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings"))
            {
                try
                {

                    Directory.CreateDirectory(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2");
                }
                catch
                {

                }
            }
            else
            {
                if (Emails == null)
                    Emails = new Email("", "");
                load();
                loadsettings();
                
            }

            tmStop.Enabled = true;
            
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            gckBrowser.Navigate("http://just-dice.com/");
            testInputs();
        }
        //Statistics
        //includes - 
        //updatestats();
        //maxbets();//start
        //maxbets();//recursive
        //btnStreakTable_Click()
        #region Statistics
        private void UpdateStats()
        {
            lblLoseStreak.Text = WorstStreak.ToString();
            lblLosses.Text = Losses.ToString();
            double profit = (PreviousBalance - StartBalance);
            lblProfit.Text = profit.ToString("0.00000000");
            lblBalance.Text = PreviousBalance.ToString("0.00000000");
            if ((PreviousBalance - StartBalance) < 0)
            {
                lblProfit.ForeColor = Color.Red;

            }
            else
            {
                lblProfit.ForeColor = Color.Green;
            }
            if (Winstreak == 0)
            {
                lblCustreak.Text = Losestreak.ToString();
                lblCustreak.ForeColor = Color.Red;
            }
            else
            {
                lblCustreak.Text = Winstreak.ToString();
                lblCustreak.ForeColor = Color.Green;

            }

            lblWins.Text = Wins.ToString();
            lblWinStreak.Text = BestStreak.ToString();
            TimeSpan curtime = DateTime.Now - dtStarted;
            lblBets.Text = (Wins + Losses).ToString();

            decimal profpB = (decimal)profit / (decimal)(Wins + Losses);
            decimal betsps = (decimal)(Wins + Losses) / (decimal)(((TotalTime.Hours + curtime.Hours) * 60 * 60) + ((TotalTime.Minutes + curtime.Minutes) * 60) + (TotalTime.Minutes + curtime.Seconds));
            decimal profph = (profpB / betsps) * (decimal)60.0 * (decimal)60.0;
            lblProfpb.Text = profpB.ToString("0.00000000");
            lblProfitph.Text = (profpB * (decimal)60.0 * (decimal)60.0).ToString("0.00000000");
            lblProfit24.Text = (profpB * (decimal)60.0 * (decimal)60.0 * (decimal)24.0).ToString("0.00000000");
                
            int imaxbets = maxbets();
            if (imaxbets == -500)
                lblMaxBets.Text = "500+";
            else
                lblMaxBets.Text = imaxbets.ToString() ;
            if (imaxbets > 20)
            {
                lblMaxBets.ForeColor = Color.Blue;
            }
            else if (imaxbets > 15)
            {
                lblMaxBets.ForeColor = Color.Green;
            }
            else if (imaxbets > 10)
            {
                lblMaxBets.ForeColor = Color.DarkOrange;
            }
            else
            {
                lblMaxBets.ForeColor = Color.Red;
            }
                
        }

        int maxbets()//Start
        {
            if (PreviousBalance != 0)
            {
                return maxbets(PreviousBalance, 0, MinBet, Multiplier);
            }
            return 0;
        }

        int maxbets(double balance, int betnr, double betsize, double multiplier)//Recursive
        {
            
            double dMultiplier = multiplier;
            double dbalance = balance;
            if (chkLowerLimit.Checked)
                dbalance -= LowerLimit;
            if (dbalance > betsize)
            {
                if (rdbDevider.Checked && betnr % Devidecounter == 0 && betnr > 0)
                {
                    dMultiplier *= Devider;
                    if (Multiplier < 1)
                        Multiplier = 1;
                }
                if ( rdbReduce.Checked && betnr == Devidecounter && betnr > 0)
                {
                    dMultiplier *= Devider;
                }
                else if (rdbMaxMultiplier.Checked && betnr > MaxMultiplies)
                {
                    dMultiplier = 1;
                }
                if (betnr < 501)
                {
                    betnr = maxbets(balance - betsize, ++betnr, betsize * dMultiplier, dMultiplier);
                }
                else
                    return -500;
            }
            else
            {
                betnr--;
            }

            return betnr;
        }        

        private void btnStreakTable_Click(object sender, EventArgs e)
        {
            int mode = 0;
            if (rdbConstant.Checked)
                mode = 0;
            else if (rdbDevider.Checked)
                mode = 1;
            else if (rdbMaxMultiplier.Checked)
                mode = 2;
            else if (rdbReduce.Checked)
                mode = 3;
            new StreakTable(MinBet, Multiplier, Devider, Devidecounter, MaxMultiplies, mode).Show();
        }

        #endregion

        
        //Core Program
        //includes -
        //Stop()
        //Getbalance()
        //Withdraw()
        //Invest()
        //Start()
        //tmBetting_Tick()
        //dobet()
        //placebet()
        //tmstop_tick
        #region Core Program

        private void Stop()
        {
            //tmBetting.Enabled = false;
            double dBalance = Getbalance();
            stop = true;
            TotalTime += (DateTime.Now - dtStarted);
        }

        double Getbalance()
        {
            try
            {
                GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_balance").DomObject);
                string sBalance = gieBalance.Value;
                double test = 0;
                if (double.TryParse("0,01", out test))
                {
                    sBalance.Replace(".", ",");
                }
                else
                {
                    sBalance.Replace(",", ".");
                }
                double dBalance = -1;
                if (double.TryParse(sBalance, out dBalance))
                {
                    return dBalance;
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        void PlaceBet()
        {
            try
            {
                GeckoInputElement gieChance = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_chance").DomObject);
                gieChance.TextContent = txtChance.Text;
                GeckoInputElement gieBet = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_bet").DomObject);
                gieBet.Value = Lastbet.ToString("0.00000000").Replace(',', '.');
                gckBrowser.Navigate("javascript:clicked_action_bet_hi()");
                dtLastBet = DateTime.Now;
            }
            catch
            {

            }
        }

        void Withdraw()
        {

            if (waiter == 0)
            {
                gckBrowser.Navigate("javascript:clicked_action_withdraw();");
            }
            if (waiter == 11)
            {
                GeckoInputElement gieAddress = new GeckoInputElement(gckBrowser.Document.GetElementById("wd_address").DomObject);
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("wd_amount").DomObject);
                gieAddress.Value = txtTo.Text;
                gieAmount.Value = txtAmount.Text;

            }
            if (waiter == 20)
            {
                gckBrowser.Navigate("javascript:socket.emit('withdraw',csrf,'" + txtTo.Text + "','" + txtAmount.Text + "',0)");
            }

            if (waiter == 30)
            {

                if (txtSecretURL.Text != "")
                {
                    gckBrowser.Navigate(txtSecretURL.Text);
                }
                else
                {
                    gckBrowser.Navigate("http://just-dice.com");
                }
            }
            if (waiter == 100)
            {
                waiter = -1; withdraw = false;
                TrayIcon.BalloonTipText = "Withdraw " + txtAmount.Text + "Complete\nRestarting Bets";
                TrayIcon.ShowBalloonTip(1000);
                if (Sound && SoundWithdraw)
                    (new SoundPlayer(@"media\withdraw.wav")).Play();
                withdrew = true;
                Emails.SendWithdraw(Amount, PreviousBalance-Amount, txtTo.Text);
                StartBalance -= Amount;
                Start();
            }

            waiter++;
        }

        void Invest()
        {

            if (waiter == 0)
            {
                //gckBrowser.Navigate("javascript:socket.emit('invest',csrf,'0.01',0)");

                /*foreach (GeckoElement ge in gckBrowser.Document.GetElementsByTagName("a"))
                {
                    if (ge.TextContent.Contains("invest"))
                    {
                        GeckoLinkElement eglink = new GeckoLinkElement(ge.DomObject);
                        eglink.Id = "hello";
                        gckBrowser.Update();
                        GeckoElement ge2 = gckBrowser.Document.GetElementById("hello");
                    }
                }*/
                /*GeckoElement gec = gckBrowser.Document.GetElementById("linkinvest");
                if (txtSecretURL.Text!="")
                {
                    gckBrowser.Navigate("javascript:window.location.href = '"+txtSecretURL.Text+"/#invest'");
                }
                else
                    gckBrowser.Navigate("javascript:window.location.href = 'http://just-dice.com/#invest'");*/


            }
            if (waiter == 5)
            {
                gckBrowser.Navigate("javascript:clicked_action_invest();");
            }
            if (waiter == 15)
            {
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("invest_input").DomObject);
                gieAmount.Value = txtAmount.Text;
            }
            if (waiter == 20)
            {
                gckBrowser.Navigate("javascript:socket.emit('invest',csrf,'" + txtAmount.Text + "',0)");
            }
            if (waiter == 30)
            {

                if (txtSecretURL.Text != "")
                {
                    gckBrowser.Navigate(txtSecretURL.Text);
                }
                else
                {
                    gckBrowser.Navigate("http://just-dice.com");
                }
            }
            if (waiter == 100)
            {
                waiter = 0; invest = false;
                TrayIcon.BalloonTipText = "Invest " + txtAmount.Text + "Complete\nRestarting Bets";
                TrayIcon.ShowBalloonTip(1000);
                if (Sound && SoundWithdraw)
                    (new SoundPlayer(@"media\withdraw.wav")).Play();
                string invested = "";
                try
                {
                    GeckoElement ge = (GeckoElement)gckBrowser.Document.GetElementsByClassName("investment")[0];
                    invested = ge.InnerHtml;
                }
                catch
                {

                }
                withdrew = true;
                Emails.SendInvest(Amount, Getbalance(), double.Parse(invested));
                StartBalance -= Amount;
                Start();
            }

            waiter++;
        }

        void Start()
        {
            rtbDonate.Text += "Please feel free to donate. \t\tBtc:  1EHPYeVGkquij8eMRQqwyb5bjpooyyfgn5 \t\tLtc: LQvMRbyuuSVsvXA3mQQM3zXT53hb34CEzy";

            save();

            stoponwin = false;
            //stop = false;

            dtStarted = DateTime.Now;

            if (testInputs())
            {
                stop = false;
                Lastbet = MinBet;
                PlaceBet();
            }
        }

        private void tmBetting_Tick(object sender, EventArgs e)
        {
            double dBalance = Getbalance();
            if (dBalance != PreviousBalance && dBalance != -1 || withdrew)
            {
                if (PreviousBalance == 0)
                    StartBalance = dBalance;
                PreviousBalance = dBalance;
            }
            else if (dBalance == PreviousBalance && dBalance != -1 || withdrew)
            {
                if ((DateTime.Now - dtLastBet).Seconds + ((DateTime.Now - dtLastBet).Minutes * 60) > 120)
                {
                    
                    gckBrowser.Refresh();
                    dtLastBet = DateTime.Now;
                    restartcounter = 0;
                }
                if (restartcounter == 30 && !stop)
                {
                    Start();
                }
                if (restartcounter < 50)
                {
                    restartcounter++;
                }
            }
            if (withdraw)
            {
                Withdraw();
            }
            if (invest)
            {
                Invest();
            }
        }

        void DoBet(double dBalance)
        {
            if (!stop && !gckBrowser.IsBusy && !(withdraw || invest))
            {

                if (dBalance > PreviousBalance && !(withdraw || invest))
                {
                    if (PreviousBalance != 0)
                    {
                        Wins++;
                        Winstreak++;
                        Losestreak = 0;
                        if (Winstreak > BestStreak)
                            BestStreak = Winstreak;
                    }
                    /*else
                    {
                        dPreviousBalance = dBalance;
                    }*/
                    Lastbet = MinBet;
                    if (stoponwin)
                    {
                        Stop();
                    }
                    iMultiplyCounter = 0;
                    Multiplier = double.Parse(txtMultiplier.Text);
                }
                else if (dBalance < PreviousBalance && !(withdraw || invest))
                {
                    iMultiplyCounter++;
                    if (rdbMaxMultiplier.Checked && Losestreak >= MaxMultiplies)
                    {
                        Multiplier = 1;
                    }
                    else if (rdbDevider.Checked && Losestreak % Devidecounter == 0 && Losestreak > 0)
                    {
                        Multiplier *= Devider;
                        if (Multiplier < 1)
                            Multiplier = 1;
                    }
                    else if (rdbReduce.Checked && Losestreak == Devidecounter && Losestreak > 0)
                    {
                        Multiplier *= Devider;
                    }
                    Lastbet *= Multiplier;
                    if (Lastbet > BiggestBest)
                        BiggestBest = Lastbet;
                    Losses++;
                    Losestreak++;
                    Winstreak = 0;
                    if (Sound && SoundStreak && Losestreak > SoundStreakCount)
                        new SoundPlayer("Media\\alarm.wav").Play();

                    if (Emails.Streak && Losestreak > Emails.StreakSize)
                        Emails.SendStreak(Losestreak, Emails.StreakSize, dBalance);

                    

                    if (Losestreak > WorstStreak)
                        WorstStreak = Losestreak;

                   
                }

                if (dBalance > Limit && chkLimit.Checked)
                {

                    if (rdbStop.Checked)
                    {
                        Stop();
                    }
                    else if (rdbWithdraw.Checked)
                    {
                        withdraw = true;
                        waiter = 0;

                    }
                    else if (rdbInvest.Checked)
                    {
                        waiter = 0;
                        invest = true;

                    }
                }
                if (dBalance - Lastbet < LowerLimit && chkLowerLimit.Checked)
                {
                    TrayIcon.BalloonTipText = "Balance lower than " + txtLowerLimit + "\nStopping Bets...";
                    TrayIcon.ShowBalloonTip(1000);
                    Stop();
                    if (Sound && SoundLow)
                        new SoundPlayer("Media\\alarm.wav").Play();
                    TrayIcon.BalloonTipText = "DiceBot has Stopped Betting\nThe next bet will will have put your Balance below your lower limit";

                    if (Emails.Lower)
                        Emails.SendLowLimit(dBalance, LowerLimit, Lastbet);
                }

                if ((dBalance != PreviousBalance || withdrew) && !stop)
                {
                    PlaceBet();
                    dPreviousBalance = dBalance;
                    try
                    {
                        UpdateStats();
                    }
                    catch
                    {

                    }
                    withdrew = false;
                }


            }

        }

        private void tmStop_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                Stop();
            }
            if (gckBrowser.IsBusy)
            {
                pbLoading.Visible = true;
                loading += loadammount;

                if (loading >= 100 || loading <= 0)
                {
                    loadammount = -loadammount;
                    if (loading < 0)
                        loading = 0;
                    if (loading > 100)
                        loading = 100;
                }
                pbLoading.Value = (int)loading;
            }
            else
            {
                pbLoading.Visible = false;
                pbLoading.Value = 0;
                loading = 0;
            }
            if (!stop && timecounter > 10)
            {
                lblTime.Text = (TotalTime + (DateTime.Now - dtStarted)).ToString(@"hh\:mm\:ss");
                timecounter = 0;
            }
            if (!loggedin)
                Login();
            timecounter++;
            if (autostart && !running)
            {
                Start();
                running = true;
            }
        }

        #endregion
        

        protected override void OnClosing(CancelEventArgs e)
        {
            save();
            base.OnClosing(e);
            Application.Exit();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        #region Login

        void Login()
        {
            try
            {
                GeckoInputElement gieUser = new GeckoInputElement(gckBrowser.Document.GetElementById("username").DomObject);
                GeckoInputElement giePass = new GeckoInputElement(gckBrowser.Document.GetElementsByName("password")[0].DomObject);
                
                GeckoInputElement gieSubmit = null;
                foreach (GeckoElement gie in gckBrowser.Document.GetElementsByTagName("input"))
                {
                    if (gie.GetAttribute("type") == "submit")
                    {
                        gieSubmit = new GeckoInputElement(gie.DomObject);
                        break;
                    }
                }
                
                gieUser.Value = username;
                giePass.Value = password;
                if (autologin)
                    gieSubmit.click();
                loggedin = true;
            }
            catch
            {
                loggedin = false;
            }

        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            GeckoInputElement gieUser = new GeckoInputElement(gckBrowser.Document.GetElementById("username").DomObject);
            GeckoInputElement giePass = new GeckoInputElement(gckBrowser.Document.GetElementById("password").DomObject);
            GeckoInputElement gieSubmit = new GeckoInputElement(gckBrowser.Document.GetElementsByTagName("submit")[0].DomObject);
            gieUser.Value = username;
            giePass.Value = password;
            gieSubmit.click();
            GeckoElement gksubmit = gckBrowser.Document.Body;
        }
        #endregion


        private void btnGo_Click(object sender, EventArgs e)
        {
            if (txtSecretURL.Text != "")
            {
                gckBrowser.Navigate(txtSecretURL.Text);
            }
        }

        private void rdbInvest_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbInvest.Checked)
            {
                MessageBox.Show("Please select the Invest page, otherwise invest will fail");
            }
        }

        //stop button pressed
        private void btnStop_Click(object sender, EventArgs e)
        {
            
            if (chkOnWin.Checked)
            {
                stoponwin = true;
            }
            else
            Stop();
        }

        #region Save and load settings
        void save()
        {
            save(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings");
        }

        void save(string file)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                try
                {
                    string msg = "";
                    msg += txtAmount.Text + ",";
                    msg += txtLimit.Text + ",";
                    if (chkLimit.Checked)
                        msg += "1,";
                    else
                        msg += "0,";
                    msg += txtLowerLimit.Text + ",";
                    if (chkLowerLimit.Checked)
                        msg += "1,";
                    else
                        msg += "0,";
                    msg += txtMinBet.Text + ",";
                    msg += txtMultiplier.Text + ",";
                    msg += txtSecretURL.Text + ",";
                    msg += txtTo.Text + ",";
                    if (rdbInvest.Checked)
                    {
                        msg += "0,";
                    }
                    else if (rdbStop.Checked)
                    {
                        msg += "1,";
                    }
                    else
                    {
                        msg += "2,";
                    }


                    if (chkOnWin.Checked)
                        msg += "1,";
                    else
                        msg += "0,";
                    sw.WriteLine(msg);
                    msg = "";
                    msg += txtChance.Text + ",";
                    msg += txtMaxMultiply.Text + ",";
                    msg += txtNBets.Text + ",";
                    msg += txtDevider.Text + ",";
                    if (rdbMaxMultiplier.Checked)
                        msg += "0,";
                    else if (rdbDevider.Checked)
                        msg += "1,";
                    else if (rdbConstant.Checked)
                        msg += "2";
                    else msg += "3";
                    sw.WriteLine(msg);



                }
                catch
                {

                }
            }
        }
        bool load()
        {
            return (load(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings"));
        }

        bool load(string File)
        {
            try
            {
                using (StreamReader sw = new StreamReader(File))
                {
                    string msg = sw.ReadLine();
                    string[] values = msg.Split(',');
                    int i = 0;
                    txtAmount.Text = values[i++];
                    txtLimit.Text = values[i++];
                    if (values[i++] == "1")
                        chkLimit.Checked = true;
                    else
                        chkLimit.Checked = false;
                    txtLowerLimit.Text = values[i++];
                    if (values[i++] == "1")
                        chkLowerLimit.Checked = true;
                    else
                        chkLowerLimit.Checked = false;
                    txtMinBet.Text = values[i++];
                    txtMultiplier.Text = values[i++];
                    txtSecretURL.Text = values[i++];
                    txtTo.Text = values[i++];
                    string action = values[i++];
                    if (action == "0")
                    {
                        rdbInvest.Checked = true;
                    }
                    else if (action == "1")
                    {
                        rdbStop.Checked = true;
                    }
                    else
                        rdbWithdraw.Checked = true;


                    if (values[i++] == "1")
                    {
                        chkOnWin.Checked = true;
                    }
                    else
                        chkOnWin.Checked = false;
                    if (!sw.EndOfStream)
                    {
                        msg = sw.ReadLine();
                        values = msg.Split(',');
                        i = 0;
                        txtChance.Text = values[i++];
                        txtMaxMultiply.Text = values[i++];
                        txtNBets.Text = values[i++];
                        txtDevider.Text = values[i++];
                        string s = values[i++];
                        if (s == "0")
                            rdbMaxMultiplier.Checked = true;
                        else if (s == "1")
                            rdbDevider.Checked = true;
                        else if (s == "2")
                            rdbConstant.Checked = true;
                        else if (s == "3")
                            rdbReduce.Checked = true;

                    }

                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void loadsettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
                {
                    //NCPuser,ncppass,autologin
                    string info = sr.ReadLine();
                    string[] chars = info.Split(' ');
                    string suser = "";
                    string spass = "";
                    bool login = false;
                    int word = 0;                    
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
                                }
                        }
                    }
                    NCPAutoLogin = login;
                    NCPpassword = spass;
                    NCPusername = suser;

                    info = sr.ReadLine();
                    chars = info.Split(' ');
                    suser = "";
                    spass = "";
                    bool start = false;
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
                    autologin = login;
                    password = spass;
                    username = suser;
                    autostart = start;


                    info = sr.ReadLine();
                    string[] values = info.Split(',');
                    int j = 0;
                    tray = (values[j++] == "1");
                    Botname = values[j++];
                    if (values[j++] != "1")
                    {
                        Emails.Enable = false;
                        Emails.emailaddress = values[j++];
                        Emails.Lower = Emails.Streak = Emails.Withdraw = (values[j] == "1");
                        j += 3;
                    }
                    else
                    {
                        Emails.Enable = true;
                        Emails.emailaddress = values[j++];
                        Emails.Withdraw = ((values[j++] == "1"));
                        Emails.Lower = ((values[j++] == "1"));
                        Emails.Streak = ((values[j++] == "1"));
                    }
                    Emails.StreakSize = int.Parse(values[j++]);
                    if (values.Count() == j)
                    {
                        Emails.SMTP = values[j++];
                    }

                    info = sr.ReadLine();
                    values = info.Split(',');
                    j=0;
                    SoundWithdraw = (values[j++] == "1");
                    if (values[j++] != "1")
                    {
                        Sound = false;
                       SoundLow = SoundStreak = (values[j] == "1");
                       j += 2;
                    }
                    else
                    {
                        Sound = true;
                        SoundLow = ((values[j++] == "1"));
                        SoundStreak = ((values[j++] == "1"));
                    }
                    SoundStreakCount = int.Parse(values[j++]);

                }

                
            }

            catch
            {

            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            bool valid = true;
            DialogResult d = MessageBox.Show("Importing a new profile will override default bet settings. Do you want to export current settings before importing?", "Import", MessageBoxButtons.YesNoCancel);
            if (d == System.Windows.Forms.DialogResult.No)
            {
                if (ofdImport.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    load(ofdImport.FileName);
                    valid = true;
                }
                else
                    valid = false;
            }
            else if (valid)
                if (d == System.Windows.Forms.DialogResult.Yes && valid)
                {
                    valid = false;
                    if (ofdExport.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        save(ofdExport.FileName);
                        valid = true;
                    }
                    if (valid)
                        if (ofdImport.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            load(ofdImport.FileName);
                        }
                }
                else if (d == System.Windows.Forms.DialogResult.Cancel)
                {


                }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (ofdExport.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                save(ofdExport.FileName);

            }
        }
        #endregion

        #region test and parse variables
        private void txtChance_Leave(object sender, EventArgs e)
        {
            testInputs();
            try
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_chance").DomObject);
                gie.Value = Chance.ToString();
            }
            catch
            {

            }
        }

        double dparse(string text)
        {
            double number = -1;
            if (!double.TryParse(text, out number))
            {
                if (text.Contains("."))
                    text = text.Replace('.', ',');
                else if (text.Contains(","))
                    text = text.Replace(',', '.');
                if (!double.TryParse(text, out number))
                    number=-1;
            }
            return number;
        }
        int iparse(string text)
        {
            int number = -1;
            if (!int.TryParse(text, out number))
            {
                if (text.Contains("."))
                    text = text.Replace('.', ',');
                else if (text.Contains(","))
                    text = text.Replace(',', '.');
                if (!int.TryParse(text, out number))
                    number = -1;
            }
            return number;
        }

        bool testInputs()
        {
            string sMessage = "";
            bool valid = true;
            //double d = double.Parse(txtLimit.Text.Replace('.',','));
            /*if (!double.TryParse(txtLimit.Text, out Limit))
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }*/
            Limit = dparse(txtLimit.Text);
            if (Limit == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }
            LowerLimit = dparse(txtLowerLimit.Text);
            if (LowerLimit == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }
            Amount = dparse(txtAmount.Text);
            if (Amount==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Amount Field\n";
            }
            if (txtTo.Text == "")
            {
                valid = false;
                sMessage += "Please enter a valid Address in the Address Field\n";
            }
            MinBet = dparse(txtMinBet.Text);
            if (MinBet==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Minimum Bet Field\n";
            }
            Chance = dparse(txtChance.Text);
            if (Chance == -1)
            {
                valid = false;
                sMessage += "Please enter a valid % in the Chance Field (Without the % sign)";
            }
            else
            {

            }
            Multiplier= dparse(txtMultiplier.Text);
            if (Multiplier == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Multiplier Field\n";
            }
            MaxMultiplies= iparse(txtMaxMultiply.Text);
            if (MaxMultiplies==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Max Multplies Field\n";
            }
            Devidecounter = iparse(txtNBets.Text);
            if (Devidecounter==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the After n bets Field\n";
            }
            Devider = dparse(txtDevider.Text);
            if (Devider == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Times Multiplier By Field\n";
            }
            if (!valid)
                MessageBox.Show(sMessage);
            return valid;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            int max =  maxbets();
            if (max == -500)
                lblMaxBets.Text = "500+";
            else
                lblMaxBets.Text = max.ToString();
        }
        #endregion

        #region tooltip
        private void Labels_MouseEnter(object sender, EventArgs e)
        {
            Label owner = sender as Label;
            string message = "";
            
            switch (owner.Name)
            {
                case "lblLimit": message = 
                    "When your balance reaches this\n"+
                    "value, the bot will do one of \n" +
                    "the actions specified below.";  
                    break;
                case "lblLowLimit": message =
                "When your balance goes below this\n" +
                "value, the bot will stop playing.\n" +
                "actions specified below"; break;
                case "lblAction": message =
                "The selected action will occur when\n" +
                "your balance goes above the limit as\n" +
                "specified above"; break;
                case "Amount": message =
                "The amount that will be invested\n" +
                "or deposited when the limit is reached"; break;
                case "lblAddress": message =
                "Btc Address that the funds get" +
                "withdrawn to\n"; break;
                case "lblMinBet": message =
                "This is the first bet to be placed,\n" +
                "upon win, bet will reset to this value\n"; break;
                case "lblChance": message =
                "Chance of winning, as entered into \n" +
                "the site\n"; break;
                case "lblMultiplier": message =
                "Upon a loss, the bet will be \n" +
                "multiplied by this value. See\n" +
                "Max multiplies and After as well"; break;
                case "lblMaxMultiplier": message =
                "In a losing streak, the bet will\n" +
                "will be multiplied untill the streak\n" +
                "reaches "+txtMaxMultiply.Text +" bets. The following bets\n"+
                "will be with the same amount"; break;
                case "lblAfter": message =
                "with every " + txtNBets.Text + " losses in a row,\n" +
                "the muliplier will be multiplied with\n" +
                txtDevider.Text+". The idea is to decrease the size\n"+
                "the multiplier, keep the value between\n"+
                "0.9 and 0.5. Minimum Multiplier is 1"; break;
                case "lblAfter2": message =
            "with every " + txtNBets.Text + " losses in a row,\n" +
            "the muliplier will be multiplied with\n" +
            txtDevider.Text + ". The idea is to decrease the size\n" +
            "the multiplier, keep the value between\n" +
            "0.9 and 0.5. Minimum Multiplier is 1"; break;
                case "lblDevider": message =
            "with every " + txtNBets.Text + " losses in a row,\n" +
            "the muliplier will be multiplied with\n" +
            txtDevider.Text + ". The idea is to decrease the size\n" +
            "the multiplier, keep the value between\n" +
            "0.9 and 0.5. Minimum Multiplier is 1"; break;
                
                default: break;
            }
            showtip(message, owner);            
        }

        private void Labels_MouseLeave(object sender, EventArgs e)
        {
            hidetip();
        }

        void showtip(string Message, Label Owner)
        {
            lblTip.Visible = true;
            Point position = new Point(Owner.Location.X + Owner.Width, Owner.Location.Y + Owner.Height);
            if (Owner.Parent!=Settings)
            {
                position = new Point(Owner.Parent.Location.X + position.X, Owner.Parent.Location.Y + position.Y);
            }
            lblTip.Location = position;
            lblTip.Text = Message;
            lblTip.BringToFront();
        }

        void hidetip()
        {
            lblTip.Location = new Point(999, 999);
            lblTip.Text = "";
        }
        #endregion

        private void btnAbout_Click(object sender, EventArgs e)
        {
            new About().Show();
        }

        #region Tray Icon and popups

        private void cDiceBot_Resize(object sender, EventArgs e)
        {
            if (tray)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    
                    TrayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                    TrayIcon.BalloonTipText = "DiceBot is still running";
                    TrayIcon.BalloonTipTitle = "DiceBot";
                    TrayIcon.BalloonTipIcon = ToolTipIcon.None;
                    TrayIcon.ShowBalloonTip(500);
                    this.Hide();
                }
            }
        }

        void menuitemclick(object sender, EventArgs e)
        {
            string name = (sender as ToolStripDropDownItem).Text;

            if (name.ToLower() == "show")
            {
                this.Show();
                this.WindowState = FormWindowState.Maximized;
                this.BringToFront();
            }
            else if (name.ToLower() == "start")
            {
                Start();
            }
            else if (name.ToLower() == "stop")
            {
                Stop();
            }
            else if (name.ToLower() == "close")
            {
                if (MessageBox.Show("Are you sure you want to close DiceBot?", "Close DeciBot", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
        }

        private void TrayIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string curstreak = "";
            Thread.Sleep(200);
            if (Winstreak == 0)
            {
                curstreak = Losestreak.ToString() + " Losses";
            }
            else
            {
                curstreak = Winstreak.ToString() + "Wins";
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right  )
            {

                this.Show();
                this.WindowState = FormWindowState.Maximized;
                this.BringToFront();
                /*TrayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                TrayIcon.ContextMenuStrip.Items.Add("Show",null, menuitemclick);
                TrayIcon.ContextMenuStrip.Items.Add("Start", null, menuitemclick);
                TrayIcon.ContextMenuStrip.Items.Add("Stop", null, menuitemclick);
                TrayIcon.ContextMenuStrip.Items.Add("Close", null, menuitemclick);
                TrayIcon.ContextMenuStrip.Items.Add("cancel", null);
                TrayIcon.ContextMenuStrip.Show(System.Windows.Forms.Cursor.Position);*/
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                TrayIcon.BalloonTipTitle = "DiceBot";
                TrayIcon.BalloonTipText = string.Format("Balance: {0:0.00000000}\n Profit: {1:0.00000000}\nCurrent Streak: {2}\nWorst Streak: {3}\nTime running: ", PreviousBalance, PreviousBalance - StartBalance, curstreak, WorstStreak) + (TotalTime + (DateTime.Now - dtStarted)).ToString(@"hh\:mm\:ss");
                TrayIcon.BalloonTipIcon = ToolTipIcon.None;
                TrayIcon.ShowBalloonTip(800);
            }
            /*else
            {
                TrayIcon.BalloonTipTitle = "DiceBot";
                TrayIcon.BalloonTipText = string.Format("Balance: {0:0.00000000}\n Profit: {1:0.00000000}\nCurrent Streak: {2}\nWorst Streak: {3}\nTime running: ", PreviousBalance, PreviousBalance - StartBalance, curstreak, WorstStreak) + (TotalTime + (DateTime.Now - dtStarted)).ToString(@"hh\:mm\:ss");
                TrayIcon.BalloonTipIcon = ToolTipIcon.None;
                TrayIcon.ShowBalloonTip(10);
            }*/
            
        }

        private void TrayIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();
        }


        #endregion

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            new cSettings(this).Show();
        }



        
        
    }

    

    
}
