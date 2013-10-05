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
        double Limit = 0;
        double Amount = 0;
        double loading = 0;
        double loadammount = 2;
        double LargestBet = 0;
        double LargestWin = 0;
        double LargestLoss = 0;
        double LowerLimit = 0;
        double Devider = 0;
        double Chance = 0;
        double avgloss = 0;
        double avgwin = 0;
        double avgstreak = 0;
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
        int Devidecounter = 0;
        int SoundStreakCount = 15;
        int restartcounter = 0;
        int reversebets = 0;
        int laststreaklose = 0;
        int laststreakwin = 0;
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
        bool high = true;
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
        Simulation lastsim;
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
            double Multiplier = double.Parse(txtMultiplier.Text);
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
            double dmultiplier = double.Parse(txtMultiplier.Text);;
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
            double dmultiplier = double.Parse(txtMultiplier.Text); ; ;
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
            double dmultiplier = double.Parse(txtMultiplier.Text); ; ;
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
                if (dBalance > PreviousBalance && !(withdraw || invest))
                {
                    

                    if (PreviousBalance != 0)
                    {
                        Wins++;
                        Winstreak++;
                        if (Winstreak >= nudLastStreakWin.Value)
                            laststreakwin = Winstreak;
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
                    Lastbet = MinBet;
                    if (stoponwin)
                    {
                        Stop();
                    }
                    iMultiplyCounter = 0;
                    Multiplier = double.Parse(txtMultiplier.Text);
                    if (chkReverse.Checked)
                    {
                        if (rdbReverseWins.Checked && Winstreak % reversebets == 0)
                        {
                            high = !high;
                        }
                    }
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
                    
                    Losses++;
                    Losestreak++;
                    if (Losestreak >= nudLastStreakLose.Value)
                        laststreaklose = Losestreak;
                    if (chkReverse.Checked)
                    {
                        if (rdbReverseLoss.Checked && Losestreak % reversebets == 0)
                        {
                            high = !high;
                        }
                    }
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
                    Winstreak = 0;
                    if (Sound && SoundStreak && Losestreak > SoundStreakCount)
                        new SoundPlayer("Media\\alarm.wav").Play();

                    if (Emails.Streak && Losestreak > Emails.StreakSize)
                        Emails.SendStreak(Losestreak, Emails.StreakSize, dBalance);

                    

                    if (Losestreak > WorstStreak)
                        WorstStreak = Losestreak;

                   

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
                    msg = "";
                    if (chkReverse.Checked)
                        msg += 1 + ",";
                    else msg += 0 + ",";
                    if (rdbReverseBets.Checked)
                        msg += "0";
                    else if (rdbReverseLoss.Checked)
                        msg += "1";
                    else if (rdbReverseWins.Checked)
                        msg += "2";
                    msg += ",";
                    msg += NudReverse.Value.ToString("00");
                    msg += ",";
                    msg += nudLastStreakWin.Value.ToString("00") + "," + nudLastStreakLose.Value.ToString("00");
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
                    if (!sw.EndOfStream)
                    {
                        msg = sw.ReadLine();
                        values = msg.Split(',');
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
                        NudReverse.Value = decimal.Parse(values[i++]);
                        if (values.Length > i)
                        {
                            nudLastStreakWin.Value = decimal.Parse(values[i++]);
                            nudLastStreakLose.Value = decimal.Parse(values[i++]);
                        }
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

                chkAlarm.Checked = Sound;
                chkEmail.Checked = Emails.Enable;
                chkEmailLowLimit.Checked = Emails.Lower;
                chkEmailStreak.Checked = Emails.Streak;
                chkEmailWithdraw.Checked = Emails.Withdraw;
                chkJDAutoLogin.Checked = autologin;
                chkJDAutoStart.Checked = autostart;
                chkNCPAutoLogin.Checked = NCPAutoLogin;
                chkSoundLowLimit.Checked = SoundLow;
                chkSoundStreak.Checked = SoundStreak;
                chkSoundWithdraw.Checked = SoundWithdraw;
                chkTray.Checked = tray;
                txtBot.Text = Text;
                txtEmail.Text = Emails.emailaddress;
                txtJDPass.Text = password;
                txtJDUser.Text = username;
                txtNCPPass.Text = NCPpassword;
                txtNCPUser.Text = NCPusername;
                
            }

            catch
            {

            }
        }

        void writesettings()
        {
            using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
            {

                //NCPuser,ncppass,autologin
                string temp1 = txtNCPUser.Text + "," + txtNCPPass.Text + ",";
                if (chkNCPAutoLogin.Checked)
                    temp1 += "1";
                else temp1 += "0";
                string ncpline = "";

                foreach (char c in temp1)
                {
                    ncpline += ((int)c).ToString() + " ";
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
                temp3 += "," + Emails.SMTP;
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
        }

        private void btnSMTP_Click(object sender, EventArgs e)
        {
            string smtp = Interaction.InputBox("Enter new smtp server address", "SMTP", "smtp.secrueserver.net");
            Emails.SMTP = smtp;
        }

    }

    

    
}
