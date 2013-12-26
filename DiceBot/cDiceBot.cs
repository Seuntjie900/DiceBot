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
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using WMPLib;
namespace DiceBot
{
    
    public partial class cDiceBot : Form
    {

        #region Variables
        Random rand = new Random();
        double StartBalance = 0;        
        double Lastbet = 0;
        double MinBet = 0;
        double Multiplier = 0;
        double WinMultiplier = 0;
        double Limit = 0;
        double Amount = 0;
        double loading = 0;
        double loadammount = 2;
        double LargestBet = 0;
        double LargestWin = 0;
        double LargestLoss = 0;
        double LowerLimit = 0;
        double Devider = 0;
        double WinDevider = 0;
        double Chance = 0;
        double avgloss = 0;
        double avgwin = 0;
        double avgstreak = 0;
        double currentprofit = 0;
        double profit = 0;
        int numwinstreasks = 0;
        int numlosesreaks = 0;
        int numstreaks = 0;
        int Wins = 0;
        int Losses = 0;
        int Winstreak = 0;
        int BestStreak = 0;
        int WorstStreak = 0;
        int BestStreak2 = 0;
        int WorstStreak2 = 0;
        int BestStreak3 = 0;
        int WorstStreak3 = 0;
        int Losestreak = 0;
        int timecounter = 0;
        int waiter = 0;
        int iMultiplyCounter = 0;
        int MaxMultiplies = 0;
        int WinMaxMultiplies = 0;
        int Devidecounter = 0;
        int WinDevidecounter = 0;
        int SoundStreakCount = 15;
        int restartcounter = 0;
        int reversebets = 0;
        int laststreaklose = 0;
        int laststreakwin = 0;
        bool stop = true;
        bool withdraw = false;
        bool invest = false;
        bool reset = false;
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
        bool high = true;
        private bool withdrew;
        DateTime dtStarted = new DateTime();
        DateTime dtLastBet = new DateTime();
        TimeSpan TotalTime = new TimeSpan(0, 0, 0);
        public string username = "";
        public string password = "";        
        public string Botname = "";
        public Email Emails { get; set; }
        Simulation lastsim;
        string ching = "";
        string salarm = "";
        #endregion

        #region Auto Invest Divest Vars
        decimal PrincipleInvest = 0.085m;
        decimal SitelowestProfits = 9999999999999;
        DateTime sitelowrecorded = new DateTime();
        decimal SitelowesProfits2 = 9999999999999;
        DateTime sitelowrecorded2 = new DateTime();
        decimal SitelowestProfits3 = 9999999999999;
        DateTime sitelowrecorded3 = new DateTime();
        DateTime lastprofit = new DateTime();
        decimal MyProfit = 0;
        DateTime lastinvest = DateTime.Now;
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

            System.Net.IWebProxy defproxy = System.Net.WebRequest.GetSystemWebProxy();
            Uri proxy = defproxy.GetProxy(new Uri("http://just-dice.com"));
            if (proxy.AbsoluteUri != new Uri("http://just-dice.com").AbsoluteUri)
            {
                Skybound.Gecko.GeckoPreferences.User["network.proxy.http"] = proxy.Host;
                Skybound.Gecko.GeckoPreferences.User["network.proxy.http_port"] = proxy.Port;
                Skybound.Gecko.GeckoPreferences.User["network.proxy.ssl"] = proxy.Host;
                Skybound.Gecko.GeckoPreferences.User["network.proxy.ssl_port"] = proxy.Port;
                Skybound.Gecko.GeckoPreferences.User["network.proxy.type"] = 1;
                Skybound.Gecko.GeckoPreferences.User["signon.autologin.proxy"] = false;
                //0 – Direct connection, no proxy. (Default)
                //1 – Manual proxy configuration.
                //2 – Proxy auto-configuration (PAC).
                //4 – Auto-detect proxy settings.
                //5 – Use system proxy settings (Default in Linux).     
            }

            #region tooltip Texts
            ToolTip tt = new ToolTip();
            tt.SetToolTip(lblZigZag1, "After every n bets/wins/losses \n(as specified to the right), \nthe bot will switch from \nbetting high to low or vica verca");
            tt.SetToolTip(lblLowLimit,
                "When your balance goes below this\n" +
                "value, the bot will stop playing.\n" +
                "actions specified below");

            tt.SetToolTip(lblLimit,
                    "When your balance reaches this\n"+
                    "value, the bot will do one of \n" +
                    "the actions specified below.");
                    
                tt.SetToolTip(lblLowLimit,
                "When your balance goes below this\n" +
                "value, the bot will stop playing.\n" +
                "actions specified below");
                tt.SetToolTip( lblAction,
                "The selected action will occur when\n" +
                "your balance goes above the limit as\n" +
                "specified above"); 
                tt.SetToolTip( lblAmount,
                "The amount that will be invested\n" +
                "or deposited when the limit is reached"); 
                tt.SetToolTip( lblAddress,
                "Btc Address that the funds get" +
                "withdrawn to\n"); 
                tt.SetToolTip( lblMinBet,
                "This is the first bet to be placed,\n" +
                "upon win, bet will reset to this value\n"); 
                tt.SetToolTip( lblChance,
                "Chance of winning, as entered into \n" +
                "the site\n");
                tt.SetToolTip( lblMultiplier,
                "Upon a loss, the bet will be \n" +
                "multiplied by this value. See\n" +
                "Max multiplies and After as well"); 
            tt.SetToolTip( lblMaxMultiplier,
                "In a losing streak, the bet will\n" +
                "will be multiplied untill the streak\n" +
                "reaches "+txtMaxMultiply.Text +" bets. The following bets\n"+
                "will be with the same amount"); 
                tt.SetToolTip( lblAfter,
                "with every " + txtNBets.Text + " losses in a row,\n" +
                "the muliplier will be multiplied with\n" +
                txtDevider.Text+". The idea is to decrease the size\n"+
                "the multiplier, keep the value between\n"+
                "0.9 and 0.5. Minimum Multiplier is 1"); 
                tt.SetToolTip( lblAfter2,
            "with every " + txtNBets.Text + " losses in a row,\n" +
            "the muliplier will be multiplied with\n" +
            txtDevider.Text + ". The idea is to decrease the size\n" +
            "the multiplier, keep the value between\n" +
            "0.9 and 0.5. Minimum Multiplier is 1"); 
                tt.SetToolTip( lblDevider,
            "with every " + txtNBets.Text + " losses in a row,\n" +
            "the muliplier will be multiplied with\n" +
            txtDevider.Text + ". The idea is to decrease the size\n" +
            "the multiplier, keep the value between\n" +
            "0.9 and 0.5. Minimum Multiplier is 1"); 
                


            #endregion


           
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
            if (txtSecretURL.Text == "")
            {
                gckBrowser.Navigate("http://just-dice.com/");
            }
            else
            {

                gckBrowser.Navigate(txtSecretURL.Text);
            }
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
            
            if (StartBalance != -1)
            {
                profit = (PreviousBalance - StartBalance);

            }
            else
            {

            }
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

            decimal profpB = 0;
            if (Wins+Losses >0)
                profpB = (decimal)profit / (decimal)(Wins + Losses);
            decimal betsps = (decimal)(Wins + Losses) / (decimal)(((TotalTime.Hours + curtime.Hours) * 60 * 60) + ((TotalTime.Minutes + curtime.Minutes) * 60) + (TotalTime.Minutes + curtime.Seconds));
            decimal profph = 0;
            if (profpB>0)
                profph = (profpB / betsps) * (decimal)60.0 * (decimal)60.0;
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
            lblAvgWinStreak.Text = avgwin.ToString("0.000000");
            lblAvgLoseStreak.Text = avgloss.ToString("0.000000");
            lblAvgStreak.Text = avgstreak.ToString("0.000000");
            if (avgstreak > 0)
                lblAvgStreak.ForeColor = Color.Green;
            else lblAvgStreak.ForeColor = Color.Red;
            lbl3Best.Text = BestStreak.ToString() + "\n" + BestStreak2.ToString() + "\n" + BestStreak3.ToString();
            lbl3Worst.Text = WorstStreak.ToString() + "\n" + WorstStreak2.ToString() + "\n" + WorstStreak2.ToString();
            lblLastStreakLose.Text = laststreaklose.ToString();
            lblLastStreakWin.Text = laststreakwin.ToString();
            lblLargestBet.Text = LargestBet.ToString("0.00000000");
            lblLargestLoss.Text = LargestLoss.ToString("0.00000000");
            lblLargestWin.Text = LargestWin.ToString("0.00000000");
        }

        int maxbets()//Start
        {
            if (PreviousBalance != 0)
            {
                if (rdbConstant.Checked)
                    return maxbetsconstant();
                else if (rdbDevider.Checked)
                    return maxbetsVariable();
                else if (rdbMaxMultiplier.Checked)
                    return maxbetsMaxMultiplies();
                else if (rdbReduce.Checked) 
                return maxbetsChangeOnce();
            }
            return 0;
        }

        int maxbetsconstant()
        {
            double total = 0;
            int bets = 0;
            double curbet = MinBet;
            double Multiplier = dparse(txtMultiplier.Text);
            while (total < PreviousBalance)
            {
                if (bets > 0)
                    curbet *= Multiplier;
                bets++;
                total += curbet;
                if (bets > 500)
                    return -500;  
            }
            return bets;
        }

        int maxbetsVariable()
        {
            double total = 0;
            int bets = 0;
            double curbet = MinBet;
            int n = Devidecounter;
            double dmultiplier = dparse(txtMultiplier.Text);;
            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    if (bets % Devidecounter == 0)
                        dmultiplier *= Devider;

                    curbet *= dmultiplier;
                }
                bets++;
                total += curbet;
                if (bets > 500)
                    return -500;
            }
            return bets;
        }

        int maxbetsMaxMultiplies()
        {
            double total = 0;
            int bets = 0;
            double curbet = MinBet;
            int n = Devidecounter;
            double dmultiplier = dparse(txtMultiplier.Text); ; ;
            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    if (bets > MaxMultiplies)
                        dmultiplier = 1;

                    curbet *= dmultiplier;
                }
                bets++;
                total += curbet;
                if (bets > 500)
                    return -500;
            }
            return bets;
        }

        int maxbetsChangeOnce()
        {
            double total = 0;
            int bets = 0;
            double curbet = MinBet;
            int n = Devidecounter;
            double dmultiplier = dparse(txtMultiplier.Text); ; ;
            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    if (bets == Devidecounter)
                        dmultiplier *= Devider;

                    curbet *= dmultiplier;
                }
                bets++;
                total += curbet;
                if (bets > 500)
                    return -500;
            }
            return bets;
        }
        
        //depricated this asshole. i hate recursion, don't know why the hell i ever used this
        //  this is a BAD piece of code taht doesn't work right. i keep it only to remind myself never to program like that again
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
            new StreakTable(MinBet, Multiplier, Devider, Devidecounter, MaxMultiplies, mode, Chance).Show();
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
                
                double dBalance = dparse(sBalance);
                return dBalance;
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
                if (high)
                gckBrowser.Navigate("javascript:clicked_action_bet_hi()");
                else
                    gckBrowser.Navigate("javascript:clicked_action_bet_lo()");
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
                try
                {
                    if (Sound && SoundWithdraw)
                    {
                        if (ching == "")
                        {
                            (new SoundPlayer(@"media\withdraw.wav")).Play();
                        }
                        else
                        {
                            if (ching.Substring(ching.LastIndexOf(".")).ToLower() == "mp3")
                            {
                                WindowsMediaPlayer player = new WindowsMediaPlayer();
                                player.URL = ching;
                                player.controls.play();
                            }
                            else
                            {
                                (new SoundPlayer(ching)).Play();
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to play CHING, pelase make sure file exists");
                }
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
                try
                {
                    if (Sound && SoundWithdraw)
                    {
                        if (ching == "")
                        {
                            (new SoundPlayer(@"media\withdraw.wav")).Play();
                        }
                        else
                        {
                            if (ching.Substring(ching.LastIndexOf(".")).ToLower() == "mp3")
                            {
                                WindowsMediaPlayer player = new WindowsMediaPlayer();
                                player.URL = ching;
                                player.controls.play();
                            }
                            else
                            {
                                (new SoundPlayer(ching)).Play();
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to play CHING, pelase make sure file exists");
                }
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
                Emails.SendInvest(Amount, Getbalance(), dparse(invested));
                StartBalance -= Amount;
                Start();
            }

            waiter++;
        }

        void ResetSeed()
        {
            if (waiter == 20)
            {
                gckBrowser.Navigate("javascript:clicked_action_random()");
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
                waiter = 0; reset = false;
                withdrew = true;
                Start();
            }
            waiter++;
        }

        void Start()
        {
            rtbDonate.Text = "Please feel free to donate. \t\tBtc:  1EHPYeVGkquij8eMRQqwyb5bjpooyyfgn5 \t\tLtc: LQvMRbyuuSVsvXA3mQQM3zXT53hb34CEzy";
            Winstreak = 0;
            Losestreak = 0;
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
            bool valid = true;
            if (chkBotSpeed.Checked)
            {
                if ((DateAndTime.Now - dtLastBet).Ticks < new TimeSpan(0, 0, 0, 0, (int)((decimal)1000.0 / nudBotSpeed.Value)).Ticks)
                {
                    valid = false;
                }
            }
            double dBalance = PreviousBalance;
            if (valid)
                dBalance = Getbalance();

                if (dBalance != PreviousBalance && dBalance != -1 || withdrew)
                {
                    if (PreviousBalance == 0)
                        StartBalance = dBalance;
                    PreviousBalance = dBalance;
                }
                else if (dBalance == PreviousBalance && dBalance != -1 || withdrew)
                {
                    if ((DateTime.Now - dtLastBet).Seconds + ((DateTime.Now - dtLastBet).Minutes * 60) > 120 && !stop)
                    {

                        gckBrowser.Reload();
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
                if (reset)
                {
                    ResetSeed();
                }
            
            
        }

        void DoBet(double dBalance)
        {
            

            if (!stop && !gckBrowser.IsBusy && !(withdraw || invest ||reset))
            {
                double betresult = dBalance - PreviousBalance;
                if (betresult > 0)
                {
                    if (LargestWin < betresult)
                        LargestWin = betresult;
                }
                else if (betresult < 0)
                {
                    if (LargestLoss < -betresult)
                        LargestLoss = -betresult;
                }

                if (LargestBet < Lastbet)
                    LargestBet = Lastbet;

                //if its a win
                if (dBalance > PreviousBalance && !(withdraw || invest || reset))
                {
                    

                    if (PreviousBalance != 0)
                    {


                        if (rdbWinMaxMultiplier.Checked && Winstreak >= WinMaxMultiplies)
                        {
                            WinMultiplier = 1;
                        }
                        else if (rdbWinDevider.Checked && Winstreak % WinDevidecounter == 0 && Winstreak > 0)
                        {
                            WinMultiplier *= WinDevider;
                        }
                        else if (rdbWinReduce.Checked && Winstreak == WinDevidecounter && Winstreak > 0)
                        {
                            WinMultiplier *= WinDevider;
                        }
                        if (Winstreak == 0)
                        {
                            currentprofit = 0;
                        }
                        currentprofit += (Lastbet*(99/Chance))-Lastbet;
                        Lastbet *= WinMultiplier;
                        if (Winstreak == 0)
                        {
                            Lastbet = MinBet;
                            
                        }
                        Wins++;
                        Winstreak++;
                        if (chkResetBetWins.Checked && Winstreak % nudResetWins.Value == 0)
                        {
                            Lastbet = MinBet;
                        }
                        if (chkChangeWinStreak.Checked && (Winstreak == nudChangeWinStreak.Value))
                        {
                            Lastbet = (double)nudChangeWinStreakTo.Value;
                        }
                        if (Winstreak >= nudLastStreakWin.Value)
                            laststreakwin = Winstreak;

                        if (currentprofit > ((double)nudStopWinBtcStreak.Value) && chkStopWinBtcStreak.Checked)
                        {
                            Stop();
                        }
                        if (Winstreak >= nudStopWinStreak.Value && chkStopWinStreak.Checked)
                        {
                            Stop();
                        }
                        if (profit >= (double)nudStopWinBtc.Value && chkStopWinBtc.Checked)
                        {
                            Stop();
                        }
                        if (Losestreak != 0)
                        {
                            double avglosecalc = avgloss * numlosesreaks;
                            avglosecalc += Losestreak;
                            avglosecalc /= ++numlosesreaks;
                            avgloss = avglosecalc;
                            double avgbetcalc = avgstreak * numstreaks;
                            avgbetcalc -= Losestreak;
                            avgbetcalc /= ++numstreaks;
                            avgstreak = avgbetcalc;
                            if (Losestreak > WorstStreak3)
                            {
                                WorstStreak3 = Losestreak;
                                if (Losestreak > WorstStreak2)
                                {
                                    WorstStreak3 = WorstStreak2;
                                    WorstStreak2 = Losestreak;
                                    if (Losestreak > WorstStreak)
                                    {
                                        WorstStreak2 = WorstStreak;
                                        WorstStreak = Losestreak;
                                    }
                                }
                            }
                        }
                        Losestreak = 0;
                        
                        
                        
                    }
                    /*else
                    {
                        dPreviousBalance = dBalance;
                    }*/
                    //Lastbet = MinBet;
                    if (stoponwin)
                    {
                        Stop();
                    }
                    iMultiplyCounter = 0;

                    Multiplier = dparse(txtMultiplier.Text);

                    if (chkReverse.Checked)
                    {
                        if (rdbReverseWins.Checked && Winstreak % reversebets == 0)
                        {
                            high = !high;
                        }
                    }
                }

                    //if its a loss
                else if (dBalance < PreviousBalance && !(withdraw || invest || reset))
                {
                    //do i use this line?
                    iMultiplyCounter++;

                    //stop multiplying if at max or if it goes below 1
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
                        //adjust multiplier according to devider
                    else if (rdbReduce.Checked && Losestreak == Devidecounter && Losestreak > 0)
                    {
                        Multiplier *= Devider;
                    }
                    //reset current profit when switching from a winning streak to a losing streak
                    if (Losestreak == 0)
                    {
                        currentprofit = 0;
                    }

                    //adjust profit
                    currentprofit -= Lastbet;
                    //set new bet size
                    Lastbet *= Multiplier;
                    
                    //increase losses and losestreak
                    Losses++;
                    Losestreak++;
                    
                    //reset bet to minimum if applicable
                    if (chkResetBetLoss.Checked && Losestreak % nudResetBetLoss.Value == 0)
                    {
                        Lastbet = MinBet;
                    }
                    //update last losing streak if it is above the specified value to show in the stats
                    if (Losestreak >= nudLastStreakLose.Value)
                        laststreaklose = Losestreak;

                    //switch high low if applied in the zig zag tab
                    if (chkReverse.Checked)
                    {
                        if (rdbReverseLoss.Checked && Losestreak % reversebets == 0)
                        {
                            high = !high;
                        }
                    }

                    //change bet after a certain losing streak
                    if (chkChangeLoseStreak.Checked && (Losestreak == nudChangeLoseStreak.Value))
                    {
                        Lastbet = (double)nudChangeLoseStreakTo.Value;
                    }
                    
                    
                    //stop conditions:
                    //stop if lose streak is higher than specified
                    if (Losestreak >= nudStopLossStreak.Value && chkStopLossStreak.Checked)
                    {
                        Stop();
                    }

                    //stop if current profit drops below specified value/ loss is larger than specified value
                    if (currentprofit <= (0.0 - (double)nudStopLossBtcStreal.Value) && chkStopLossBtcStreak.Checked)
                    {
                        Stop();
                    }

                    // stop if total profit/total loss is below/above certain value
                    if (profit <= 0.0-(double)nudStopLossBtc.Value && chkStopLossBtc.Checked)
                    {
                        Stop();
                    }

                    //when switching from win streak to lose streak, calculate some stats
                    if (Winstreak != 0)
                    {
                        double avgwincalc = avgwin * numwinstreasks;
                        avgwincalc += Winstreak;
                        avgwincalc /= ++numwinstreasks;
                        avgwin = avgwincalc;
                        double avgbetcalc = avgstreak * numstreaks;
                        avgbetcalc += Winstreak;
                        avgbetcalc /= ++numstreaks;
                        avgstreak = avgbetcalc;
                        if (Winstreak > BestStreak3)
                        {
                            BestStreak3 = Winstreak;
                            if (Winstreak > BestStreak2)
                            {
                                BestStreak3 = BestStreak2;
                                BestStreak2 = Winstreak;
                                if (Winstreak > BestStreak)
                                {
                                    BestStreak2 = BestStreak;
                                    BestStreak = Winstreak;
                                }
                            }
                        }
                    }

                    //reset win streak
                    Winstreak = 0;

                    //sounds
                    if (Sound && SoundStreak && Losestreak > SoundStreakCount)
                        playalarm();
                    //email
                    if (Emails.Streak && Losestreak > Emails.StreakSize)
                        Emails.SendStreak(Losestreak, Emails.StreakSize, dBalance);

                    
                    //update worst streaks
                    if (Losestreak > WorstStreak)
                        WorstStreak = Losestreak;

                    //reset win multplier
                    WinMultiplier = dparse(txtWinMultiplier.Text);

                }
                if (chkReverse.Checked)
                {
                    if (rdbReverseBets.Checked && (Wins+Losses) % reversebets == 0 )
                    {
                        high = !high;
                    }
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
                        playalarm();
                    TrayIcon.BalloonTipText = "DiceBot has Stopped Betting\nThe next bet will will have put your Balance below your lower limit";

                    if (Emails.Lower)
                        Emails.SendLowLimit(dBalance, LowerLimit, Lastbet);
                }



                if ( Wins!=0 && Losses!=0 && chkResetSeed.Checked)
                {
                    if ( ((rdbResetSeedBets.Checked && (Wins+Losses) % nudResetSeed.Value == 0) ||
                       (rdbResetSeedWins.Checked && Wins % nudResetSeed.Value == 0 && Losestreak==0)||
                       (rdbResetSeedLosses.Checked && Losses % nudResetSeed.Value == 0 && Winstreak == 0)) && !withdrew)
                    {
                        waiter = 0;
                        reset = true;
                        ResetSeed();
                    }
                    
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

        void playalarm()
        {
            try
            {
                
                    if (salarm == "")
                    {
                        (new SoundPlayer(@"media\alarm.wav")).Play();
                    }
                    else
                    {
                        int ext = salarm.LastIndexOf(".")+1;
                        if (salarm.Substring(ext).ToLower() == "mp3")
                        {
                            WindowsMediaPlayer player = new WindowsMediaPlayer();
                            player.URL = salarm;
                            player.controls.play();
                        }
                        else
                        {
                            (new SoundPlayer(salarm)).Play();
                        }
                    }
                
            }
            catch
            {
                MessageBox.Show("Failed to play Alarm, pelase make sure file exists");
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
            if ((sender as Button).Name.ToUpper().Contains("HIGH"))
                high=true;
            else
                high = false;
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
            
            if (chkStopOnWin.Checked)
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
                    sw.WriteLine("SaveVersion|" + "2");
                    sw.WriteLine("Amount|" + txtAmount.Text);
                    sw.WriteLine("Limit|"+txtLimit.Text);
                    if (chkLimit.Checked)
                        sw.WriteLine("LimitEnabled|1");
                    else
                        sw.WriteLine("LimitEnabled|0");
                     sw.WriteLine("LowerLimit|"+txtLowerLimit.Text);
                    sw.Write("LowerLimitEnabled|");
                    if (chkLowerLimit.Checked)
                        sw.WriteLine("1");
                    else
                        sw.WriteLine("0");

                    sw.WriteLine("MinBet|"+txtMinBet.Text);
                    sw.WriteLine("Multiplier|"+txtMultiplier.Text);
                    sw.WriteLine("URL|"+txtSecretURL.Text);
                    sw.WriteLine("To|"+txtTo.Text);
                    sw.Write("OnStop|");
                    if (rdbInvest.Checked)
                    {
                        sw.WriteLine("0");
                    }
                    else if (rdbStop.Checked)
                    {
                        sw.WriteLine("1");
                    }
                    else
                    {
                        sw.WriteLine("2");
                    }
                    sw.Write("StopOnWin|");
                    if (chkStopOnWin.Checked)
                        sw.WriteLine("1");
                    else
                        sw.WriteLine("0");
                    
                    sw.WriteLine("Chance|"+txtChance.Text);
                    sw.WriteLine("MaxMultiply|"+txtMaxMultiply.Text);
                    sw.WriteLine("NBets|"+txtNBets.Text);
                    sw.WriteLine("Devider|"+ txtDevider.Text);
                    sw.Write("MultiplierMode|");
                    if (rdbMaxMultiplier.Checked)
                        sw.WriteLine("0");
                    else if (rdbDevider.Checked)
                        sw.WriteLine("1");
                    else if (rdbConstant.Checked)
                        sw.WriteLine("2");
                    else sw.WriteLine("3");
                    sw.Write("ReverseEnabled|");
                    if (chkReverse.Checked)
                        sw.WriteLine("1");
                    else sw.WriteLine("0");
                    sw.Write("ReverseMode|");
                    if (rdbReverseBets.Checked)
                        sw.WriteLine("0");
                    else if (rdbReverseLoss.Checked)
                        sw.WriteLine("1");
                    else if (rdbReverseWins.Checked)
                        sw.WriteLine("2");
                    sw.WriteLine("ReverseOn|"+NudReverse.Value.ToString("00"));

                    sw.WriteLine("LastStreakWin|" + nudLastStreakWin.Value.ToString("00"));
                    sw.WriteLine("LastStreakLose|"+nudLastStreakLose.Value.ToString("00"));
                    
                    sw.Write("ResetBetLossEnabled|");
                    if (chkResetBetLoss.Checked)
                    {
                        sw.WriteLine("1");
                    }
                    else
                    {
                        sw.WriteLine("0");
                    }
                    sw.WriteLine("ResetBetLossValue|"+nudResetBetLoss.Value.ToString());

                    sw.Write("ResetBetWinsEnabled|");
                    if (chkResetBetWins.Checked)
                        sw.WriteLine("1");
                    else
                        sw.WriteLine("0");
                    sw.WriteLine("ResetWinsValue|"+nudResetWins.Value.ToString());
                    sw.WriteLine("WinMultiplier|" + txtWinMultiplier.Text);
                    sw.WriteLine("WinMaxMultiplies|" + txtWinMaxMultiplies.Text);
                    sw.WriteLine("WinNBets|" + txtWinNBets.Text);
                    sw.WriteLine("WinDevider|" + txtWinDevider.Text);
                    sw.Write("WinMultiplyMode|");
                    if (rdbWinConstant.Checked)
                        sw.WriteLine("0");
                    else if (rdbWinDevider.Checked)
                        sw.WriteLine("1");
                    else if (rdbWinMaxMultiplier.Checked)
                        sw.WriteLine("2");
                    else if (rdbWinReduce.Checked)
                        sw.WriteLine("3");

                    string msg = "";
                    if (chkBotSpeed.Checked)
                        msg = "1";
                    else msg = "0";
                    sw.WriteLine("BotSpeedEnabled|"+msg);
                    sw.WriteLine("BotSpeedValue|" + nudBotSpeed.Value.ToString());
                    if (chkResetSeed.Checked)
                        msg = "1";
                    else msg= "0";
                    sw.WriteLine("ResetSeedEnabled|" + msg);
                    
                    if (rdbResetSeedBets.Checked)
                        msg = "0";
                    else if (rdbResetSeedWins.Checked)
                        msg = "1";
                    else if (rdbResetSeedLosses.Checked)
                        msg = "2";
                    sw.WriteLine("ResetSeedMode|"+msg);
                        sw.WriteLine("ResetSeedValue|"+nudResetSeed.Value.ToString());
                    
                    if (chkStopLossStreak.Checked) msg = "1";
                    else msg="0";
                    sw.WriteLine("StopAfterLoseStreakEnabled|"+msg);                    
                    sw.WriteLine("StopAfterLoseStreakValue|"+nudStopLossStreak.Value.ToString());
                    
                    if ( chkStopLossBtcStreak.Checked ) msg = "1";
                    else msg="0";
                    sw.WriteLine("StopAfterLoseStreakBtcEnabled|"+msg);
                    sw.WriteLine("StopAfterLoseStreakBtcValue|"+nudStopLossBtcStreal.Value.ToString());
                    
                    if (  chkStopLossBtc.Checked ) msg = "1";
                    else msg="0";
                    sw.WriteLine("StopAfterLoseBtcEnabled|"+msg);
                    sw.WriteLine("StopAfterLoseBtcValue|"+nudStopLossBtc.Value.ToString());
                    
                    
                    
                    if ( chkChangeLoseStreak.Checked ) msg = "1";
                    else msg="0";
                    sw.WriteLine("ChangeAfterLoseStreakEnabled|"+msg);
                    sw.WriteLine("ChangeAfterLoseStreakSize|"+nudChangeLoseStreak.Value.ToString());
                    sw.WriteLine("ChangeAfterLoseStreakTo|"+nudChangeLoseStreakTo.Value.ToString());


                    if (chkStopWinStreak.Checked) msg = "1";
                    else msg = "0";
                    sw.WriteLine("StopAfterWinStreakEnabled|" + msg);
                    sw.WriteLine("StopAfterWinStreakValue|" + nudStopWinStreak.Value.ToString());
                    if (chkStopWinBtcStreak.Checked) msg = "1";
                    else msg = "0";
                    sw.WriteLine("StopAfterWinStreakBtcEnabled|" + msg);
                    sw.WriteLine("StopAfterWinStreakBtcValue|" + nudStopWinBtcStreak.Value.ToString());
                    if (chkStopWinBtc.Checked) msg = "1";
                    else msg = "0";
                    sw.WriteLine("StopAfterWinBtcEnabled|" + msg);
                    sw.WriteLine("StopAfterWinBtcValue|" + nudStopWinBtc.Value.ToString());

                    if (chkChangeWinStreak.Checked) msg = "1";
                    else msg = "0";
                    sw.WriteLine("ChangeAfterWinStreakEnabled|" + msg);
                    sw.WriteLine("ChangeAfterWinStreakSize|" + nudChangeWinStreak.Value.ToString());
                    sw.WriteLine("ChangeAfterWinStreakTo|" + nudChangeWinStreakTo.Value.ToString());
                    
                    #region old save, Not applicable
                    /*string msg = "";
                    msg += txtAmount.Text + ";";
                    msg += txtLimit.Text + ";";
                    if (chkLimit.Checked)
                        msg += "1;";
                    else
                        msg += "0;";
                    msg += txtLowerLimit.Text + ";";
                    if (chkLowerLimit.Checked)
                        msg += "1;";
                    else
                        msg += "0;";
                    msg += txtMinBet.Text + ";";
                    msg += txtMultiplier.Text + ";";
                    msg += txtSecretURL.Text + ";";
                    msg += txtTo.Text + ";";
                    if (rdbInvest.Checked)
                    {
                        msg += "0;";
                    }
                    else if (rdbStop.Checked)
                    {
                        msg += "1;";
                    }
                    else
                    {
                        msg += "2;";
                    }


                    if (chkOnWin.Checked)
                        msg += "1;";
                    else
                        msg += "0;";
                    sw.WriteLine(msg);
                    msg = "";
                    msg += txtChance.Text + ";";
                    msg += txtMaxMultiply.Text + ";";
                    msg += txtNBets.Text + ";";
                    msg += txtDevider.Text + ";";
                    if (rdbMaxMultiplier.Checked)
                        msg += "0;";
                    else if (rdbDevider.Checked)
                        msg += "1";
                    else if (rdbConstant.Checked)
                        msg += "2";
                    else msg += "3";
                    sw.WriteLine(msg);
                    msg = "";
                    if (chkReverse.Checked)
                        msg += 1 + ";";
                    else msg += 0 + ";";
                    if (rdbReverseBets.Checked)
                        msg += "0";
                    else if (rdbReverseLoss.Checked)
                        msg += "1";
                    else if (rdbReverseWins.Checked)
                        msg += "2";
                    msg += ";";
                    msg += NudReverse.Value.ToString("00");
                    msg += ";";
                    msg += nudLastStreakWin.Value.ToString("00") + ";" + nudLastStreakLose.Value.ToString("00");
                    sw.WriteLine(msg);
                    msg = "";
                    if (chkResetBetLoss.Checked)
                    {
                        msg += "1;";
                    }
                    else
                    {
                        msg+="0;";
                    }
                    msg += nudResetBetLoss.Value.ToString()+";";
                    if (chkResetBetWins.Checked)
                        msg += "1;";
                    else
                        msg += "0;";
                    msg += nudResetWins.Value.ToString() + ";";
                    msg += txtWinMultiplier.Text + ";"+txtWinMaxMultiplies.Text+";"+txtWinNBets.Text+";"+txtWinDevider.Text+";";
                    if (rdbWinConstant.Checked)
                        msg+="0";
                    else if (rdbWinDevider.Checked)
                        msg+="1";
                    else if (rdbWinMaxMultiplier.Checked)
                        msg+="2";
                    else if (rdbWinReduce.Checked)
                        msg+="3";
                    msg+=";";
                    if (chkBotSpeed.Checked)
                        msg += "1";
                    else msg += "0";
                    msg += ";";
                    if (chkResetSeed.Checked)
                        msg += "1";
                    else msg += "0";
                    msg += ";";
                    if (rdbResetSeedBets.Checked)
                        msg += "0";
                    else if (rdbResetSeedWins.Checked)
                        msg += "1";
                    else if (rdbResetSeedLosses.Checked)
                        msg += "2";
                    msg += ";";
                        msg+= nudResetSeed.Value.ToString();
                    sw.WriteLine(msg);*/
                    #endregion
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

        bool oldLoad(string File)
        {
            using (StreamReader sw = new StreamReader(File))
                {
                    string msg = sw.ReadLine();
                    string[] values = msg.Split(',');
                    if (msg.Contains(";"))
                        values = msg.Split(';');
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
                        chkStopOnWin.Checked = true;
                    }
                    else
                        chkStopOnWin.Checked = false;
                    if (!sw.EndOfStream)
                    {
                        msg = sw.ReadLine();
                        values = msg.Split(',');
                        if (msg.Contains(";"))
                            values = msg.Split(';');
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
                    if (!sw.EndOfStream)
                    {
                        msg = sw.ReadLine();
                        values = msg.Split(',');
                        if (msg.Contains(";"))
                            values = msg.Split(';');
                        i = 0;                        
                        if (values[i++] == "1")
                            chkReverse.Checked = true;
                        else
                            chkReverse.Checked = false;                        
                        string cur = values[i++];
                        if (cur == "0")
                            rdbReverseBets.Checked = true;
                        else if (cur == "1")
                            rdbReverseLoss.Checked = true;
                        else if (cur == "2")
                            rdbReverseWins.Checked = true;
                        NudReverse.Value = (decimal)dparse(values[i++]);
                        if (values.Length > i)
                        {
                            nudLastStreakWin.Value = (decimal)dparse(values[i++]);
                            nudLastStreakLose.Value = (decimal)dparse(values[i++]);
                        }
                    }
                    if (!sw.EndOfStream)
                    {
                        msg = sw.ReadLine();
                        values = msg.Split(',');
                        if (msg.Contains(";"))
                            values = msg.Split(';');
                        i = 0;
                        
                        chkResetBetLoss.Checked =(values[i++] == "1");
                        nudResetBetLoss.Value = (decimal)dparse(values[i++]);
                        chkResetBetWins.Checked = (values[i++] == "1");
                        nudResetWins.Value = (decimal)dparse(values[i++]);
                        
                        txtWinMultiplier.Text = values[i++];
                        txtWinMaxMultiplies.Text = values[i++];
                        txtWinNBets.Text  = values[i++];
                        txtWinDevider.Text = values[i++];
                        string cur = values[i++];
                        rdbWinConstant.Checked = (cur == "0");
                        rdbWinDevider.Checked = (cur == "1");
                        rdbWinMaxMultiplier.Checked = (cur == "2");
                        rdbWinReduce.Checked = (cur == "3");
                        /*if (chkBotSpeed.Checked)
                            msg += "1";
                        else msg += "0";
                        msg += ";";
                        if (chkResetSeed.Checked)
                            msg += "1";
                        else msg += "0";
                        msg += ";";
                        if (rdbResetSeedBets.Checked)
                            msg += "0";
                        else if (rdbResetSeedWins.Checked)
                            msg += "1";
                        else if (rdbResetSeedLosses.Checked)
                            msg += "2";*/
                        chkBotSpeed.Checked = (values[i++] == "1");
                        chkResetSeed.Checked = (values[i++] == "1");
                        cur = values[i++];
                        rdbResetSeedBets.Checked = (cur == "0");
                        rdbResetSeedWins.Checked = (cur == "1");
                        rdbResetSeedLosses.Checked = (cur == "2");
                        if (values.Length >= i + 1)
                        {
                            nudResetSeed.Value = iparse(values[i++]);
                        }
                    }
                }
            variabledisable();
            return true;
        }

        string getvalue(List<SavedItem> list, string item)
        {
            foreach (SavedItem cur in list)
            {
                if (cur.Name.ToUpper() == item.ToUpper())
                {
                    return cur.Value;
                }

            }
            return "0-0-0";
        }

        bool load(string File)
        {
            try
            {
                string header = "";
                using (StreamReader sr = new StreamReader(File))
                {
                    header = sr.ReadLine();
                }
                               
                //if load file is not of version 2 or above, do old load
                if (!header.ToUpper().Contains("VERSION"))
                {
                    return oldLoad(File);
                }
                //else do normal load
                else
                {
                    List<SavedItem> saveditems = new List<SavedItem>();
                    using (StreamReader sr = new StreamReader(File))
                    {                        
                        while (!sr.EndOfStream)
                        {
                            string[] s = sr.ReadLine().Split('|');
                            saveditems.Add(new SavedItem(s[0],s[1]));
                        }
                    }
                    txtAmount.Text=getvalue( saveditems, "Amount");
                    txtLimit.Text = getvalue(saveditems, "Limit");
                    chkLimit.Checked= (getvalue(saveditems, "LimitEnabled") == "1");
                    txtLowerLimit.Text = getvalue(saveditems, "LowerLimit");
                    chkLowerLimit.Checked = (getvalue(saveditems, "LowerLimitEnabled") == "1");
                    txtMinBet.Text = getvalue(saveditems, "MinBet");
                    txtMultiplier.Text = getvalue(saveditems, "Multiplier");
                    txtSecretURL.Text = getvalue(saveditems, "URL");
                    txtTo.Text = getvalue(saveditems, "To");
                    string temp = getvalue(saveditems, "OnStop");
                    rdbInvest.Checked = (temp == "0");
                    rdbStop.Checked = (temp == "1");
                    rdbWithdraw.Checked = (temp == "2");
                    chkStopOnWin.Checked = ("1"==getvalue(saveditems, "StopOnWin"));
                    txtChance.Text = getvalue(saveditems, "Chance");
                    txtMaxMultiply.Text = getvalue(saveditems, "MaxMultiply");
                    txtNBets.Text = getvalue(saveditems, "NBets");
                    txtDevider.Text = getvalue(saveditems, "devider");
                    temp = getvalue(saveditems, "MultiplierMode");
                    rdbMaxMultiplier.Checked = (temp == "0");
                    rdbDevider.Checked = (temp == "1");
                    rdbConstant.Checked = (temp == "2");
                    rdbReduce.Checked = (temp=="3");
                    chkReverse.Checked = ("1"==getvalue(saveditems, "ReverseEnabled"));
                    temp = getvalue(saveditems, "ReverseMode");
                    rdbReverseBets.Checked = (temp == "0");
                    rdbReverseLoss.Checked = (temp == "1");
                    rdbReverseWins.Checked = (temp == "2");
                    NudReverse.Value = (decimal)dparse(getvalue(saveditems, "ReverseOn"));
                    nudLastStreakWin.Value = (decimal)dparse(getvalue(saveditems, "LastStreakWin"));
                    nudLastStreakLose.Value = (decimal)dparse(getvalue(saveditems, "LastStreakLose"));
                    chkResetBetLoss.Checked = ("1"==getvalue(saveditems, "ResetBetLossEnabled"));
                    nudResetBetLoss.Value = (decimal)dparse(getvalue(saveditems, "ResetBetLossValue"));
                    chkResetBetWins.Checked = ("1"==getvalue(saveditems, "ResetBetWinsEnabled"));
                    nudResetWins.Value = (decimal)dparse(getvalue(saveditems, "ResetWinsValue"));
                    txtWinMultiplier.Text = getvalue(saveditems, "WinMultiplier");
                    txtWinMaxMultiplies.Text = getvalue(saveditems, "WinMaxMultiplies");
                    txtWinNBets.Text = getvalue(saveditems, "WinNBets");
                    txtWinDevider.Text = getvalue(saveditems, "WinDevider");
                    temp = getvalue(saveditems, "WinMultiplyMode");
                    rdbWinConstant.Checked = ("0" == temp);
                    rdbWinDevider.Checked = ("1" == temp);
                    rdbWinMaxMultiplier.Checked = ("2" == temp);
                    rdbWinReduce.Checked = ("3" == temp);
                    chkBotSpeed.Checked = ("1"==getvalue(saveditems, "BotSpeedEnabled"));
                    nudBotSpeed.Value = (decimal)dparse(getvalue(saveditems, "BotSpeedValue"));
                    chkResetSeed.Checked = ("1"==getvalue(saveditems, "ResetSeedEnabled"));
                    temp = getvalue(saveditems, "ResetSeedMode");
                    rdbResetSeedBets.Checked = ("0" == temp);
                    rdbResetSeedWins.Checked = ("1" == temp);
                    rdbResetSeedLosses.Checked = ("2" == temp);
                    nudResetSeed.Value=(decimal)dparse(getvalue(saveditems, "ResetSeedValue"));

                    chkStopLossStreak.Checked = ("1" == getvalue(saveditems, "StopAfterLoseStreakEnabled"));
                    nudStopLossStreak.Value = (decimal)dparse(getvalue(saveditems, "StopAfterLoseStreakValue"));
                    chkStopLossBtcStreak.Checked = ("1" == getvalue(saveditems, "StopAfterLoseStreakBtcEnabled"));
                    nudStopLossBtcStreal.Value = (decimal)dparse(getvalue(saveditems, "StopAfterLoseStreakBtcValue"));
                    chkStopLossBtc.Checked = ("1" == getvalue(saveditems, "StopAfterLoseBtcEnabled"));
                    nudStopLossBtc.Value = (decimal)dparse(getvalue(saveditems, "StopAfterLoseBtcValue"));

                    chkChangeLoseStreak.Checked = ("1" == getvalue(saveditems, "ChangeAfterLoseStreakEnabled"));
                    nudChangeLoseStreak.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterLoseStreakSize"));
                    nudChangeLoseStreakTo.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterLoseStreakTo"));


                    chkStopWinStreak.Checked = ("1" == getvalue(saveditems, "StopAfterWinStreakEnabled"));
                    nudStopWinStreak.Value = (decimal)dparse(getvalue(saveditems, "StopAfterWinStreakValue"));
                    chkStopWinBtcStreak.Checked = ("1" == getvalue(saveditems, "StopAfterWinStreakBtcEnabled"));
                    nudStopWinBtcStreak.Value = (decimal)dparse(getvalue(saveditems, "StopAfterWinStreakBtcValue"));
                    chkStopWinBtc.Checked = ("1" == getvalue(saveditems, "StopAfterWinBtcEnabled"));
                    nudStopWinBtc.Value = (decimal)dparse(getvalue(saveditems, "StopAfterWinBtcValue"));

                    chkChangeWinStreak.Checked = ("1" == getvalue(saveditems, "ChangeAfterWinStreakEnabled"));
                    nudChangeWinStreak.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterWInStreakSize"));
                    nudChangeWinStreakTo.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterWInStreakTo"));
               
                

                }

                
                
                variabledisable();
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool loadsettingsold()
        {
            using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
            {
                //NCPuser,ncppass,autologin
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
                autologin = login;
                password = spass;
                username = suser;
                autostart = start;
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
                    autologin = login;
                    password = spass;
                    username = suser;
                    autostart = start;
                }

                info = sr.ReadLine();
                string[] values = info.Split(',');
                if (info.Contains(";"))
                    values = info.Split(';');
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
                Emails.StreakSize = iparse(values[j++]);
                if (values.Count() == j)
                {
                    Emails.SMTP = values[j++];
                }

                info = sr.ReadLine();
                values = info.Split(',');
                j = 0;
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
                SoundStreakCount = iparse(values[j++]);

            }
            return true;
        }

        public void loadsettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
                {

                    string test = sr.ReadLine();
                    if (test != "new")
                        loadsettingsold();
                    else
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
                        autologin = login;
                        password = spass;
                        username = suser;
                        autostart = start;
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
                            autologin = login;
                            password = spass;
                            username = suser;
                            autostart = start;
                        }
                        List<SavedItem> saveditems = new List<SavedItem>();
                        while (!sr.EndOfStream)
                        {
                            string[] temp = sr.ReadLine().Split('|');
                            saveditems.Add(new SavedItem(temp[0],temp[1]));
                        }
                        //string msg = "";
                        tray = ("1"==getvalue(saveditems, "Tray"));
                        Botname = getvalue(saveditems, "BotName");
                        Emails.Enable = ("1"==getvalue(saveditems, "enableEmail"));
                        Emails.emailaddress = getvalue(saveditems, "emailaddress");
                        Emails.Withdraw = ("1"==getvalue(saveditems, "emailwithdraw"));
                        Emails.Lower = ("1"==getvalue(saveditems, "emaillow"));
                        Emails.Streak = ("1" == getvalue(saveditems, "emailstreak"));
                        Emails.StreakSize = iparse(getvalue(saveditems, "emailstreakval"));
                        Emails.SMTP = getvalue(saveditems, "SMTP");

                        SoundWithdraw = ("1" ==getvalue(saveditems, "CoinEnabled"));
                        ching= txtPathChing.Text = getvalue(saveditems, "CoinPath");
                        Sound = ("1"==getvalue(saveditems, "AlarmEnabled"));
                        SoundLow = ("1" == getvalue(saveditems, "AlarmLowEnabled"));
                        SoundStreak = ("1" == getvalue(saveditems, "AlarmStreakEnabled"));
                        SoundStreakCount =iparse(getvalue(saveditems, "AlarmStreakValue"));
                        salarm= txtPathAlarm.Text = getvalue(saveditems, "AlarmPath");
                        nudEmailStreak.Value = (decimal)Emails.StreakSize;
                        nudSoundStreak.Value = SoundStreakCount;


                    }

                }

                chkAlarm.Checked = Sound;
                chkEmail.Checked = Emails.Enable;
                chkEmailLowLimit.Checked = Emails.Lower;
                chkEmailStreak.Checked = Emails.Streak;
                chkEmailWithdraw.Checked = Emails.Withdraw;
                chkJDAutoLogin.Checked = autologin;
                chkJDAutoStart.Checked = autostart;
                
                chkSoundLowLimit.Checked = SoundLow;
                chkSoundStreak.Checked = SoundStreak;
                chkSoundWithdraw.Checked = SoundWithdraw;
                chkTray.Checked = tray;
                txtBot.Text = Text;
                txtEmail.Text = Emails.emailaddress;
                txtJDPass.Text = password;
                txtJDUser.Text = username;
                
                
            }

            catch
            {

            }
        }

        void writesettings()
        {
            using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
            {
                sw.WriteLine("new");
                //sw.WriteLine("User|" + txtJDUser.Text);
                //sw.WriteLine("Seed|"+txtJDPass.Text);
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

                ////tray,botname,enableemail,emailaddress,emailwithdraw,emailinvest,emaillow,emailstreak,emailstreakval
                string msg = "";
                msg = (chkTray.Checked) ? "1" : "0";                
                sw.WriteLine("tray|"+msg);
                sw.WriteLine("botmname|"+txtBot.Text);
                msg = (chkEmail.Checked) ? "1" : "0";  
                sw.WriteLine("enableemail|"+msg);
                sw.WriteLine("emailaddress|"+txtEmail.Text);
                msg = (chkEmailWithdraw.Checked) ? "1" : "0";  
                sw.WriteLine("emailwithdraw|"+msg);
                msg = (chkEmailLowLimit.Checked) ? "1" : "0";  
                sw.WriteLine("emaillow|"+msg);
                msg = (chkEmailStreak.Checked) ? "1" : "0";  
                sw.WriteLine("emailstreak|"+msg);
                sw.WriteLine("emailstreakval|"+nudEmailStreak.Value.ToString());
                sw.WriteLine("SMTP|" + Emails.SMTP);
                

                ////soundcoin,soundalarm,soundlower,soundstrea,soundstreakvalue
               
                msg = (chkSoundWithdraw.Checked) ? "1" : "0";
                sw.WriteLine("CoinEnabled|" + msg);
                sw.WriteLine("CoinPath|" + txtPathChing.Text);
                msg = (chkAlarm.Checked) ? "1" : "0";
                sw.WriteLine("AlarmEnabled|" + msg);
                msg = (chkSoundLowLimit.Checked) ? "1" : "0";
                sw.WriteLine("AlarmLowEnabled|" + msg);
                msg = (chkSoundStreak.Checked) ? "1" : "0";
                sw.WriteLine("AlarmStreakEnabled|" + msg);

                sw.WriteLine("AlarmStreakValue|" + nudSoundStreak.Value.ToString());
                sw.WriteLine("AlarmPath|" + txtPathAlarm.Text);
                



                #region old Save
                ////JDuser,JDPass,AutoLogin,AutoStart
                //string temp2 = txtJDUser.Text + "," + txtJDPass.Text + ",";
                //if (chkJDAutoLogin.Checked)
                //    temp2 += "1,";
                //else temp2 += "0";
                //if (chkJDAutoStart.Checked)
                //    temp2 += "1,";
                //else temp2 += "0";
                //string jdline = "";

                //foreach (char c in temp2)
                //{
                //    jdline += ((int)c).ToString() + " ";
                //}
                //sw.WriteLine(jdline);

                ////tray,botname,enableemail,emailaddress,emailwithdraw,emailinvest,emaillow,emailstreak,emailstreakval
                //string temp3 = "";
                //if (chkTray.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //temp3 += txtBot.Text + ",";
                //if (chkEmail.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //temp3 += txtEmail.Text + ",";
                //if (chkEmailWithdraw.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //if (chkEmailLowLimit.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //if (chkEmailStreak.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //temp3 += nudEmailStreak.Value.ToString();
                //temp3 += "," + Emails.SMTP;
                //sw.WriteLine(temp3);

                ////soundcoin,soundalarm,soundlower,soundstrea,soundstreakvalue
                //temp3 = "";
                //if (chkSoundWithdraw.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //if (chkAlarm.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //if (chkSoundLowLimit.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //if (chkSoundStreak.Checked)
                //    temp3 += "1,";
                //else temp3 += "0,";
                //temp3 += nudSoundStreak.Value.ToString();
                //sw.WriteLine(temp3);
                #endregion
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

        private void variabledisable()
        {
            //set active controlls for on loss multiplier
            if (rdbConstant.Checked)
            {
                txtMaxMultiply.Enabled = false;
                txtNBets.Enabled = false;
                txtDevider.Enabled = false;
            }
            if (rdbDevider.Checked || rdbReduce.Checked)
            {
                txtMaxMultiply.Enabled = false;
                txtNBets.Enabled = true;
                txtDevider.Enabled = true;
            }
            if (rdbMaxMultiplier.Checked)
            {
                txtMaxMultiply.Enabled = true;
                txtNBets.Enabled = false;
                txtDevider.Enabled = false;
            }

            //set active controlls for on win multiplier
            if (rdbWinConstant.Checked)
            {
                txtWinMaxMultiplies.Enabled = false;
                txtWinNBets.Enabled = false;
                txtWinDevider.Enabled = false;
            }
            if (rdbWinDevider.Checked || rdbWinReduce.Checked)
            {
                txtWinMaxMultiplies.Enabled = false;
                txtWinNBets.Enabled = true;
                txtWinDevider.Enabled = true;
            }
            if (rdbWinMaxMultiplier.Checked)
            {
                txtWinMaxMultiplies.Enabled = true;
                txtWinNBets.Enabled = false;
                txtWinDevider.Enabled = false;
            }
        }

        private void txtChance_Leave(object sender, EventArgs e)
        {
            testInputs();
            try
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_chance").DomObject);
                gie.Value = Chance.ToString().Replace(',', '.');
            }
            catch
            {

            }
        }

        double dparse(string text)
        {
            double number = -1;
            string test = "0.000001";
            double dtest = 0;
            if (double.TryParse(test, out dtest))
            {
                if (dtest != 0.000001)
                {
                    text = text.Replace(".", ",");
                }
                else
                {
                    text = text.Replace(",", ".");
                }
            }
            else
            {
                text = text.Replace(",", ".");
            }

            //text = text.Replace(",", ".");
            if (!double.TryParse(text, out number))
            {
                //text = text.Replace(".", ",");
                if (!double.TryParse(text, out number))
                    return -1;
            }

            /*if (!double.TryParse(text, out number))
            {
                if (text.Contains("."))
                    text = text.Replace('.', ',');
                else if (text.Contains(","))
                    text = text.Replace(',', '.');
                if (!double.TryParse(text, out number))
                    number=-1;
            }*/
            
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
            WinMultiplier = dparse(txtWinMultiplier.Text);
            if (WinMultiplier == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Multiplier Field\n";
            }
            WinMaxMultiplies = iparse(txtWinMaxMultiplies.Text);
            if (WinMaxMultiplies == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Max Multplies Field\n";
            }
            WinDevidecounter = iparse(txtWinNBets.Text);
            if (WinDevidecounter == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the After n bets Field\n";
            }
            WinDevider = dparse(txtWinDevider.Text);
            if (WinDevider == -1)
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

            variabledisable();
            
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

        /*private void btnSettings_Click(object sender, EventArgs e)
        {
            new cSettings(this).Show();
        }*/

        #region auto invest divest
        bool divesting = false;
        bool divesting2 = false;

        private void tmrCheckInvest_Tick(object sender, EventArgs e)
        {
            #region invest hour

            if (chkHour.Checked)
            {
                if ((lastinvest - DateTime.Now).Hours > nudHours.Value)
                {
                    if (waiter == 0)
                    {
                        gckBrowser.Navigate("javascript:socket.emit('invest',csrf,'" + ((decimal)Getbalance() * (decimal)nudInvestHour.Value / (decimal)100.0).ToString() + "',0)");
                    }
                    if (waiter == 2)
                    {
                        gckBrowser.Navigate("http://Just-dice.com");
                        lastinvest = DateTime.Now;
                        waiter = -1;
                    }
                    waiter++;
                }
            }
#endregion
            #region divest profit
            if (chkDivestProf.Checked)
            {
                string sBalance="999999";
                try
                {
                    GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("investment")[0].DomObject);
                     sBalance = gieBalance.InnerHtml;
                }
                    catch
                {
                    
                }
                    decimal curinvested = 0;
                    if (decimal.TryParse(sBalance, out curinvested) || divesting)
                    {
                        if ((PrincipleInvest / curinvested) < 1 || divesting)
                        {
                            if ((PrincipleInvest / curinvested * 100) > nudProfitPer.Value || divesting)
                            {
                                if (waiter == 0)
                                {
                                    gckBrowser.Navigate("javascript:socket.emit('divest',csrf,'" + ((curinvested * nudProfitPer.Value) / 100).ToString() + "',0)");
                                    divesting = true;
                                }
                                else if (waiter == 2)
                                {
                                    gckBrowser.Navigate("http://Just-dice.com");
                                    waiter = -1;
                                    divesting = false;
                                }
                                waiter++;
                            }
                        }
                    }
            }
            #endregion

            #region site profit
            if (chkSiteProfit.Checked)
            {
                string sBalance = "s";
                decimal NewProfit = 0;
                try
                {
                    GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("sprofitraw")[0].DomObject);
                    sBalance = gieBalance.InnerHtml;

                }
                catch
                {

                }
                if (decimal.TryParse(sBalance, out NewProfit) || divesting2)
                {
                    decimal oldprofit = SitelowestProfits;

                        if (oldprofit < 0 && NewProfit < 0)
                        {
                            if (oldprofit / NewProfit < 1)
                            {
                                if ((oldprofit / NewProfit) * 100m > nudTotalDivestPer.Value)
                                {

                                    if (waiter == 0)
                                    {
                                        gckBrowser.Navigate("javascript:socket.emit(\"divest\",csrf,\"all\",divest_code.val()");
                                        divesting2 = true;
                                    }


                                }
                            }
                            
                        }
                        if (oldprofit > 0 && NewProfit > 0)
                        {
                            if (NewProfit / oldprofit < 1)
                            {
                                if ((NewProfit / oldprofit) * 100m > nudTotalDivestPer.Value)
                                {

                                    if (waiter == 0)
                                    {
                                        gckBrowser.Navigate("javascript:socket.emit(\"divest\",csrf,\"all\",divest_code.val()");
                                        divesting2 = true;
                                    }


                                }
                            }
                        }

                        if (waiter == 2 && divesting2)
                        {
                            gckBrowser.Navigate("http://Just-dice.com");
                            waiter = -1;
                            divesting = false;
                        }
                    if (divesting2)
                        waiter++;
                    
                }
            }
            if ((DateTime.Now - lastprofit).Hours >= 1)
            {
                string sBalance = "s";
                decimal NewProfit = 0;
                try
                {
                    GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("sprofitraw")[0].DomObject);
                    sBalance = gieBalance.InnerHtml;
                }
                catch
                {

                }
                if (decimal.TryParse(sBalance, out NewProfit))
                {
                    if (NewProfit < SitelowestProfits)
                    {
                        SitelowestProfits3 = SitelowesProfits2;
                        sitelowrecorded3 = sitelowrecorded2;
                        SitelowesProfits2 = SitelowestProfits;
                        sitelowrecorded2 = sitelowrecorded;
                        SitelowestProfits = NewProfit;
                        sitelowrecorded = DateTime.Now;

                    }
                }
            }
            #endregion

        }

        private void txtPrince_Leave(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtPrince.Text, out PrincipleInvest))
            {
                MessageBox.Show("Principle not a valid number");
            }
        }
        #endregion


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            reversebets = (int)NudReverse.Value;
            NudReverse.Value = reversebets;
        }

        private void tabPage5_Click(object sender, EventArgs e)
        {

        }

        #region Simulate and bet generator

        Simulation runsim()
        {
            
           
            double dMultiplier = Multiplier;
            double startMultiplier = Multiplier;
            int numbets = (int)nudSimNumBets.Value;
            int bets = 1;
            int wins = 0;
            int losses = 0;
            int winstreak = 0;
            int losstreak = 0;
            int largestwinstreak = 0;
            int largestlostreak = 0;
            int MaxMultiplies = this.MaxMultiplies;
            double devider = this.Devider;
            double devidercounter = this.Devidecounter;
            double balance = (double)nudSimBalance.Value;
            double lastbet = MinBet;
            double minbet = MinBet;
            double profit = 0;
            string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ._";
            string server = "";

            for (int i = 0; i < 64; i++)
            {
                server += (chars[rand.Next(0, chars.Length)]);
            }
            string client = "";
            for (int i = 0; i < 24; i++)
            {
                client += rand.Next(0, 10).ToString();
            }
            
            string sserver = "";
            foreach (byte b in server)
            {
                sserver += Convert.ToChar(b);
            }
            
            Simulation sim = new Simulation(balance.ToString("0.00000000"), bets.ToString(), sserver, client);
            while (bets <= numbets)
            {
                //string columns = "Bet Number,LuckyNumber,Chance,Roll,Result,Wagered,Profit,Balance,Total Profit";
                string betstring = bets+",";
                double number = getlucky(server, client, bets);
                betstring += number.ToString() + ","+Chance.ToString()+",";
                bool win = false;
                if (high)
                    betstring += ">" + (100 - Chance) + ",";
                else
                    betstring += "<" + Chance+",";
                if (high && number > 100 - Chance)
                {
                    win = true;
                }
                else if (!high && number < Chance)
                {
                    win = true;
                }

                if (win)
                {
                    betstring += "win,";
                    wins++;
                    winstreak++;
                    if (largestwinstreak < winstreak)
                        largestwinstreak = winstreak;
                    losstreak = 0;
                    dMultiplier = startMultiplier;
                    profit += (lastbet * 99 / Chance) - lastbet;
                    betstring += lastbet + ",";
                    betstring += (lastbet * 99 / Chance) - lastbet+",";
                    balance += (lastbet * 99 / Chance) - lastbet;
                    lastbet = minbet;
                    dMultiplier = startMultiplier;
                    if (chkReverse.Checked && rdbReverseWins.Checked && winstreak % reversebets == 0)
                        high = !high;
                    if (chkChangeWinStreak.Checked && (winstreak == nudChangeWinStreak.Value))
                    {
                        lastbet = (double)nudChangeWinStreakTo.Value;
                    }

                }
                else
                {
                    betstring += "lose,";
                    losses++;
                    losstreak++;
                    if (largestlostreak < losstreak)
                        largestlostreak = losstreak;
                    winstreak = 0;
                    betstring += lastbet + ",";
                    betstring += - lastbet + ",";
                    profit -= lastbet;
                    balance -= lastbet;
                    if (rdbMaxMultiplier.Checked && losstreak == MaxMultiplies)
                    {
                        dMultiplier = 1;
                    }
                    if (rdbDevider.Checked && losstreak % devidercounter == 0)
                    {
                        dMultiplier *= devider;
                    }
                    if (rdbReduce.Checked && losstreak == devidercounter)
                    {
                        dMultiplier *= devider;
                    }
                    if (chkChangeLoseStreak.Checked && (losstreak == nudChangeLoseStreak.Value))
                    {
                        lastbet = (double)nudChangeLoseStreakTo.Value;
                    }

                    lastbet *= dMultiplier;
                    if (lastbet > balance)
                        break;
                    if (chkReverse.Checked && rdbReverseLoss.Checked && losstreak % reversebets == 0)
                        high = !high;
                }
                betstring += balance + ",";
                betstring += profit;
                sim.bets.Add(betstring);
                bets++;
                if (chkReverse.Checked && rdbReverseBets.Checked && bets % reversebets == 0)
                    high = !high;
            }

            lblSimLosses.Text = losses.ToString();
            lblSimProfit.Text = profit.ToString("0.00000000");
            lblSimWins.Text = wins.ToString();
            lblSimEndBalance.Text = balance.ToString("0.00000000");
            lblSimLoseStreak.Text = largestlostreak.ToString();
            lblSimWinStreak.Text = largestwinstreak.ToString();
            
            return sim;

        }

        private void btnSim_Click(object sender, EventArgs e)
        {
            lastsim = runsim();
        }

        private double getlucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();
            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = nonce.ToString() + ":" + client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i+=charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);
                
                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }

        private void btnExportSim_Click(object sender, EventArgs e)
        {
            SaveFileDialog svdExportSim = new SaveFileDialog();
            svdExportSim.DefaultExt = "csv";
            svdExportSim.AddExtension = true;
            if (svdExportSim.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                try
                {
                    if (lastsim == null)
                    {
                        lastsim = runsim();
                    }
                    using (StreamWriter sw = new StreamWriter(svdExportSim.FileName))
                    {
                        foreach (string s in lastsim.bets)
                        {
                            sw.WriteLine(s);
                        }
                    }
                    MessageBox.Show("exported to " + svdExportSim.FileName);
                }
                catch
                {
                    MessageBox.Show("Failed exporting to " + svdExportSim.FileName);
                }
            }
        }

        private void btnGenerateBets_Click(object sender, EventArgs e)
        {
            if (txtClientSeed.Text != "" && txtServerSeed.Text != "")
            {
                List<string> Betlist = new List<string>();
                string headers = "betnumber,luckynumber,,Please note, This algorithm is still in testing, some Numbers Might be wrong.\n,,,Check the alternative roll verifier";
                Betlist.Add(headers);
                byte[] server = new byte[64];
                
                for (decimal i = nudGenBetsStart.Value; i < nudGenBetsStart.Value + nudGenBetsAmount.Value; i++)
                {
                    string curstring = i.ToString() + "," + getlucky(i + ":" + txtServerSeed.Text + ":" + i, txtClientSeed.Text, (int)i).ToString();
                    Betlist.Add(curstring);
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter("LuckyNum-" + txtClientSeed.Text + ".csv"))
                    {
                        foreach (string s in Betlist)
                        {
                            sw.WriteLine(s);
                        }
                    }
                    MessageBox.Show("Saved bets to: " + "LuckyNum-" + txtClientSeed.Text + ".csv");
                }
                catch
                {
                    MessageBox.Show("Failed saving bets to: " + "LuckyNum-" + txtClientSeed.Text + ".csv");
                }
            }
            else
            {
                MessageBox.Show("Please enter a server seed and a client seed");
            }
        }

        #endregion

        private void btnResetStats_Click(object sender, EventArgs e)
        {
            Wins = 0;
            Losses = 0;
            StartBalance = Getbalance();
            Winstreak = Losestreak = BestStreak = laststreaklose = laststreakwin = WorstStreak = BestStreak2 = WorstStreak3 = BestStreak3 = WorstStreak3 = numstreaks = numwinstreasks = numlosesreaks = 0;
            avgloss = avgstreak = LargestBet = LargestLoss = LargestWin = avgwin = 0.0;
            UpdateStats();
        }

        private void btnSaveUser_Click(object sender, EventArgs e)
        {
            writesettings();
            loadsettings();
        }

        
        private void nudBotSpeed_ValueChanged(object sender, EventArgs e)
        {
            if (nudBotSpeed.Value != (decimal)0.0)
            {
                lblTimeBetween.Text = (1 / nudBotSpeed.Value).ToString("##0.0000") + "Seconds";
            }
        }

        private void btnSMTP_Click_1(object sender, EventArgs e)
        {
            string smtp = Interaction.InputBox("Enter new smtp server address", "SMTP", "smtp.secrueserver.net");
            Emails.SMTP = smtp;
            
        }

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

        

        

    }

    

    
}
