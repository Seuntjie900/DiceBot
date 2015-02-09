using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        Control[] ControlsToDisable;
        private const string vers = "2.5.5";
        DateTime OpenTime = DateTime.UtcNow;
        Random r = new Random();
        Graph LiveGraph;
        #region Variables
        Random rand = new Random();
        bool retriedbet = false;
        double StartBalance = 0;        
        double Lastbet = 0;
        double MinBet = 0;
        double Multiplier = 0;
        double WinMultiplier = 0;
        double Limit = 0;
        double Amount = 0;
        
        
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
        double luck = 0;
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
        
        int iMultiplyCounter = 0;
        int MaxMultiplies = 0;
        int WinMaxMultiplies = 0;
        int Devidecounter = 0;
        int WinDevidecounter = 0;
        int SoundStreakCount = 15;
        int restartcounter = 0;
        
        int laststreaklose = 0;
        int laststreakwin = 0;
        int Currency = 0;
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
        bool starthigh = true;
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

        //labouchere
        List<double> LabList = new List<double>();
        #endregion

        DiceSite CurrentSite;
        private double dPreviousBalance;
        
        void populateFiboNacci()
        {
            decimal Previous = 0;
            decimal Current = (decimal)MinBet ;
            for (int i =0; i<100; i++)
            {
                lstFibonacci.Items.Add(string.Format("{0}. {1}", i, Current));
                decimal tmp = Current;
                Current += Previous;
                Previous = tmp;
            }
        }

        public double PreviousBalance
        {
            get { return dPreviousBalance; }
            set 
            {
               
                dPreviousBalance = value; 
            }
        }

        delegate void dDobet(bool Win, double Profit);
        public void GetBetResult(double Balance, bool win, double Profit)
        {
            PreviousBalance = (double)Balance;
            if (InvokeRequired)
            {
                Invoke(new dDobet(DoBet),win, Profit);
            }
            else
            DoBet(win, (double)Profit);
            if (!RunningSimulation)
            {
                AddChartPoint(profit);
            }
        }

        delegate void dAddChartPoint(double Profit);
        void AddChartPoint(double Profit)
        {
            if (InvokeRequired)
            {
                Invoke(new dAddChartPoint(AddChartPoint), profit);
            }
            else
            {
                chrtEmbeddedLiveChart.Series[0].Points.AddY(profit + Profit);
            }
        }

        void EnableNotLoggedInControls(bool Enabled)
        { 
            foreach (Control c in ControlsToDisable)
            {
                c.Enabled = Enabled;
            }
            if (Enabled)
            { 
                btnRegister.Enabled = false;
                btnLogIn.Text = "Logout";
            }
            else
            {
                btnRegister.Enabled = true;
                btnLogIn.Text = "Log In";
            }
        }

        public cDiceBot()
        {
            sqlite_helper.CheckDBS();
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
                
            }
            InitializeComponent();
            
            ControlsToDisable = new Control[] { btnApiBetHigh, btnApiBetLow, btnWithdraw, btnInvest, btnTip, btnStartHigh, btnStartLow };
            EnableNotLoggedInControls(false);
            cmbSettingMode.SelectedIndex = 0;
            chrtEmbeddedLiveChart.Series[0].Points.AddXY(0, 0);
            chrtEmbeddedLiveChart.ChartAreas[0].AxisX.Minimum = 0;
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
                "reaches "+nudMaxMultiplies.Value.ToString("0.00000") +" bets. The following bets\n"+
                "will be with the same amount"); 
                tt.SetToolTip( lblAfter,
                "with every " + nudNbets.Value + " losses in a row,\n" +
                "the muliplier will be multiplied with\n" +
                nudDevider.Value.ToString("0.00000")+". The idea is to decrease the size\n"+
                "the multiplier, keep the value between\n"+
                "0.9 and 0.5. Minimum Multiplier is 1"); 
                tt.SetToolTip( lblAfter2,
            "with every " + nudNbets.Value.ToString() + " losses in a row,\n" +
            "the muliplier will be multiplied with\n" +
            nudDevider.Value.ToString("0.00000") + ". The idea is to decrease the size\n" +
            "the multiplier, keep the value between\n" +
            "0.9 and 0.5. Minimum Multiplier is 1"); 
                tt.SetToolTip( lblDevider,
            "with every " + nudNbets.Value.ToString() + " losses in a row,\n" +
            "the muliplier will be multiplied with\n" +
            nudDevider.Value.ToString("0.00000") + ". The idea is to decrease the size\n" +
            "the multiplier, keep the value between\n" +
            "0.9 and 0.5. Minimum Multiplier is 1"); 
                


            #endregion

           
            
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings"))
            {
                if (MessageBox.Show("Dice Bot has detected that there are no default settings saved on this computer."+
                    "If this is the first time you are running Dice Bot, it is highly recommended you see the begginners guide"+
                    "\n\nGo to Beginners Guide now?", "Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Process.Start("https://bitcointalk.org/index.php?topic=391870");
                }
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
            switch (cmbSite.SelectedIndex)
            {
                case 0: CurrentSite = new JD(this); break;
                case 1: CurrentSite = new PD(this)/*new PRC()*/; break;
                case 2: CurrentSite = new PD(this)/*new D999()*/; break;
                //case 3: CurrentSite = new PRC2(); break;
                case 3: CurrentSite = new PD(this)/*new SafeDice()*/; break;
                case 4: CurrentSite = new PD(this); break;
            }
            if (cmbSite.SelectedIndex==-1)
            {
                cmbSite.SelectedIndex=4;
            }
            Thread tGetVers = new Thread(new ThreadStart(getversion));
            tGetVers.Start();
            populateFiboNacci();
        }

        //check if the current version of the bot is the latest version available
        void getversion()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://seuntjie.com/Dicebot/vs.html");
                HttpWebResponse EmitResponse = (HttpWebResponse)request.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                string[] ss = sEmitResponse.Split('|');
                if (ss[0]!=vers)
                {
                    string newfeatures = ss.Length>1?"New features include: "+ss[1]:"";
                    if (MessageBox.Show("A new version of DiceBot is available. "+newfeatures+" \n\nDo you want to go to the download page now?","Update Available", MessageBoxButtons.YesNo)== System.Windows.Forms.DialogResult.Yes)
                    {
                        Process.Start("http://sourceforge.net/projects/seuntjiejddb");
                    }
                }
            }
            catch
            {

            }
        }

               
        private void Form1_Load(object sender, EventArgs e)
        {
            
            testInputs();
        }

        //Statistics
        //includes - 
        //updatestats();
        //maxbets();//start
        //maxbets();//recursive
        //btnStreakTable_Click()
        #region Statistics
        delegate void dUpdateStats();
        private void UpdateStats()
        {
            if (InvokeRequired)
            {
                Invoke(new dUpdateStats(UpdateStats));
                return;
            }
            else
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
                if (Wins + Losses > 0)
                    profpB = (decimal)profit / (decimal)(Wins + Losses);
                decimal betsps =0;
                if (curtime.TotalSeconds>0)
                    betsps = (decimal)(Wins + Losses) / (decimal)(curtime.TotalSeconds);
                decimal profph = 0;
                if (profpB > 0 && betsps>0)
                    profph = (profpB / betsps) * (decimal)60.0 * (decimal)60.0;
                lblProfpb.Text = profpB.ToString("0.00000000");
                lblProfitph.Text = (profpB * (decimal)60.0 * (decimal)60.0).ToString("0.00000000");
                lblProfit24.Text = (profpB * (decimal)60.0 * (decimal)60.0 * (decimal)24.0).ToString("0.00000000");

                int imaxbets = maxbets();
                if (imaxbets == -500)
                    lblMaxBets.Text = "500+";
                else
                    lblMaxBets.Text = imaxbets.ToString();
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
                if (Losses != 0)
                {
                    lblLuck.Text = luck.ToString("00.00") + "%";
                }
            }
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
        bool convert = false;
        int maxbetsconstant()
        {
            double total = 0;
            int bets = 0;
            double curbet = MinBet;
            
            double Multiplier = (double)(nudMultiplier.Value);

            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    curbet *= Multiplier;
                }
                if (bets == nudChangeLoseStreak.Value && chkChangeLoseStreak.Checked)
                {
                    curbet = (double)nudChangeLoseStreakTo.Value;
                }
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
            double dmultiplier = (double)(nudMultiplier.Value);
            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    if (bets % Devidecounter == 0)
                        dmultiplier *= Devider;

                    curbet *= dmultiplier;
                }
                if (bets == nudChangeLoseStreak.Value && chkChangeLoseStreak.Checked)
                {
                    curbet = (double)nudChangeLoseStreakTo.Value;
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
            double dmultiplier = (double)(nudMultiplier.Value);
            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    if (bets > MaxMultiplies)
                        dmultiplier = 1;

                    curbet *= dmultiplier;
                }
                if (bets == nudChangeLoseStreak.Value && chkChangeLoseStreak.Checked)
                {
                    curbet = (double)nudChangeLoseStreakTo.Value;
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
            double dmultiplier = (double)(nudMultiplier.Value);
            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    if (bets == Devidecounter)
                        dmultiplier *= Devider;

                    curbet *= dmultiplier;
                }
                if (bets == nudChangeLoseStreak.Value && chkChangeLoseStreak.Checked)
                {
                    curbet = (double)nudChangeLoseStreakTo.Value;
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

        private void CalculateLuck(bool win)
        {
            decimal lucktotal = (decimal)luck * (decimal)((Wins + Losses) - 1);
            if (win)
                lucktotal += (decimal)((decimal)100 / (decimal)Chance)*(decimal)100;
            double tmp = (double)(lucktotal / (decimal)(Wins + Losses));
            luck = tmp;
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
            bool success = false;
            double dBalance = Getbalance( out success);
            stop = true;
            TotalTime += (DateTime.Now - dtStarted);
            if (RunningSimulation)
            {
                Updatetext(lblSimLosses, Losses.ToString());
                Updatetext(lblSimProfit, profit.ToString("0.00000000"));
                Updatetext(lblSimWins, Wins.ToString());
                Updatetext(lblSimEndBalance, PreviousBalance.ToString("0.00000000"));
                Updatetext(lblSimLoseStreak, WorstStreak.ToString());
                Updatetext(lblSimWinStreak, BestStreak.ToString());
                using (StreamWriter sw = File.AppendText(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim"))
                {
                    foreach (string tmpbet in tempsim.bets)
                    {
                        sw.WriteLine(tmpbet);
                    }
                    tempsim.bets.Clear();
                }
                //return tempsim;
                //RunningSimulation = false;
                PreviousBalance=tmpbalance;
                Wins=  tmpwins ;
                Losses = tmplosses;
                StartBalance = tmpStartBalance ;
            }
        }

        double Getbalance(out bool success)
        {
            try
            {
                string sBalance = "";
                //JD
                sBalance = CurrentSite.GetbalanceValue();
               
                double dBalance = dparse(sBalance, ref convert);
                if (convert)
                {
                    success = true;
                    return dBalance;
                }
                else
                {
                    success = false;
                    return -1;
                }
            }
            catch
            {
                success = false;
                return -1;
            }
        }

        
        void PlaceBet()
        {
            try
            {
                //JD
                //CurrentSite.SetChance(txtChance.Text, gckBrowser);
                /*if (CurrentSite is PD)
                    if ((CurrentSite as PD).chance == 0)
                        CurrentSite.SetChance(Chance.ToString(), gckBrowser);*/
                /*if (CurrentSite is SafeDice)
                    SafeDiceCounter++;
                if (!(CurrentSite is SafeDice) || SafeDiceCounter == 1)*/
                {
                    CurrentSite.SetAmount(Lastbet);
                }
                if (!CurrentSite.ReadyToBet())
                    return;
                
                //if (!(CurrentSite is SafeDice) || SafeDiceCounter >= 2)
                {
                    CurrentSite.PlaceBet(high);
                    
                    dtLastBet = DateTime.Now;
                    EnableTimer(tmBet, false);
                }
                
                
            }
            catch
            {

            }
        }

        void Withdraw()
        {
            if (CurrentSite.AutoWithdraw)
                if (CurrentSite.Withdraw((double)(nudAmount.Value), txtTo.Text))
                {

                    //withdraw = false;
                    TrayIcon.BalloonTipText = "Withdraw " + nudAmount.Value + " Complete\nRestarting Bets";
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
                    //withdrew = true;
                    Emails.SendWithdraw(Amount, PreviousBalance - Amount, txtTo.Text);
                    StartBalance -= Amount;
                    //Start(true);
                }
        }

        void Invest()
        {

            if (CurrentSite.AutoInvest)
            {
                if (CurrentSite.Invest((double)(nudAmount.Value)))
                {
                    //invest = false;
                    TrayIcon.BalloonTipText = "Invest " + nudAmount.Value + "Complete\nRestarting Bets";
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
                    
                    //withdrew = true;
                    bool success = false;
                    Emails.SendInvest(Amount, Getbalance(out success), dparse("-0", ref convert));
                    StartBalance -= Amount;
                    //Start(true);
                }

            }
        }

        void ResetSeed()
        {
            if (CurrentSite.ChangeSeed)
            {

                CurrentSite.ResetSeed();

            }
        }

        void Start(bool Continue)
        {
            if (!Continue)
            {
                Winstreak = 0;
                Losestreak = 0;
                save();

                stoponwin = false;
                Chance = (double)nudChance.Value;
                CurrentSite.SetChance(Chance.ToString("0.00000000"));

                dtStarted = DateTime.Now;
            }
            if (testInputs())
            {
                stop = false;
                if (rdbLabEnable.Checked)
                {
                    LabList = new List<double>();
                    string[] lines = GetLabList();
                    foreach (string s in lines)
                    {
                        LabList.Add(dparse(s,ref convert));
                    }
                }
                if (!Continue)
                {
                    Lastbet = MinBet;
                    if (rdbLabEnable.Checked)
                    {
                        if (LabList.Count == 1)
                            Lastbet = LabList[0];
                        else
                            Lastbet = LabList[0] + LabList[LabList.Count - 1];
                    }
                    mutawaprev = (double)nudChangeWinStreakTo.Value / (double)nudMutawaMultiplier.Value;
                }
                if (RunningSimulation)
                {
                    setInterval(tmBet, 1);
                    Simbet();
                }
                else
                {
                    setInterval(tmBet, 100);
                    //PlaceBet();
                    EnableTimer(tmBet, true);
                }
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
            if (!RunningSimulation)
            {
                double dBalance = PreviousBalance;

                if (valid)
                {
                    bool success = false;
                    dBalance = Getbalance(out success);

                }
                if ((dBalance != PreviousBalance && convert || withdrew) && dBalance > 0)
                {
                    if (PreviousBalance == 0)
                        StartBalance = dBalance;
                    PreviousBalance = dBalance;

                    try
                    {
                        string bets = "";
                        double dbets = 0;
                        string myprofit = "";
                        double dprof = 0;
                        bets = CurrentSite.GetTotalBets().Replace(",", "");
                        dbets = dparse(bets, ref convert);
                        myprofit = CurrentSite.GetMyProfit().Replace(",", "");
                        dprof = dparse(myprofit, ref convert);

                      
                    }
                    catch
                    {

                    }

                }
                else if (dBalance == PreviousBalance && convert || withdrew)
                {
                    if ((DateTime.Now - dtLastBet).TotalSeconds > 30 && !stop)
                    {
                        if (/*(CurrentSite is PRC || CurrentSite is SafeDice) &&*/ !retriedbet)
                        {
                            retriedbet = true;
                            //PlaceBet();
                            EnableTimer(tmBet, true);

                        }
                    }
                    if ((DateTime.Now - dtLastBet).TotalSeconds > 120 && !stop)
                    {

                        
                            dtLastBet = DateTime.Now;
                            restartcounter = 0;
                        

                    }
                    
                    if (restartcounter > 50 && restartcounter < 51 && !stop)
                    {
                        Start(true);
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
        }

        double mutawaprev = 0;
        bool trazelmultiply = false;
        int trazelwin = 0;
        
        void Labouchere(bool Win)
        {
            if (Win)
            {
                if (rdbLabEnable.Checked)
                {
                    if (chkReverseLab.Checked)
                    {
                        if (LabList.Count == 1)
                            LabList.Add(LabList[0]);
                        else
                            LabList.Add(LabList[0] + LabList[LabList.Count - 1]);
                    }
                    else if (LabList.Count > 1)
                    {
                        LabList.RemoveAt(0);
                        LabList.RemoveAt(LabList.Count - 1);
                        if (LabList.Count == 0)
                        {
                            if (rdbLabStop.Checked)
                                Stop();
                            else
                            {
                                string[] ss = GetLabList();
                                LabList = new List<double>();
                                foreach (string s in ss)
                                {
                                    LabList.Add(dparse(s, ref convert));
                                }
                                if (LabList.Count == 1)
                                    Lastbet = LabList[0];
                                else if (LabList.Count > 1)
                                    Lastbet = LabList[0] + LabList[LabList.Count - 1];
                            }
                        }

                    }
                    else
                    {
                        if (rdbLabStop.Checked)
                            Stop();
                        else
                        {
                            string[] ss = GetLabList();
                            LabList = new List<double>();
                            foreach (string s in ss)
                            {
                                LabList.Add(dparse(s, ref convert));
                            }
                            if (LabList.Count == 1)
                                Lastbet = LabList[0];
                            else if (LabList.Count > 1)
                                Lastbet = LabList[0] + LabList[LabList.Count - 1];
                        }
                    }
                }

                
            }
            else
            {
                //do laboucghere logic
                if (rdbLabEnable.Checked)
                {
                    if (!chkReverseLab.Checked)
                    {
                        if (LabList.Count == 1)
                            LabList.Add(LabList[0]);
                        else
                            LabList.Add(LabList[0] + LabList[LabList.Count - 1]);
                    }
                    else
                    {
                        if (LabList.Count > 1)
                        {
                            LabList.RemoveAt(0);
                            LabList.RemoveAt(LabList.Count - 1);
                            if (LabList.Count == 0)
                                Stop();
                        }
                        else
                        {
                            if (rdbLabStop.Checked)
                                Stop();
                            else
                            {
                                string[] ss = GetLabList();
                                LabList = new List<double>();
                                foreach (string s in ss)
                                {
                                    LabList.Add(dparse(s, ref convert));
                                }
                                if (LabList.Count == 1)
                                    Lastbet = LabList[0];
                                else if (LabList.Count > 1)
                                    Lastbet = LabList[0] + LabList[LabList.Count - 1];
                            }
                        }
                    }
                }


                //end labouchere logic
            }
            
                if (LabList.Count == 1)
                    Lastbet = LabList[0];
                else if (LabList.Count > 1)
                    Lastbet = LabList[0] + LabList[LabList.Count - 1];
                else
                {
                    if (rdbLabStop.Checked)
                        Stop();
                    else
                    {
                        string[] ss = GetLabList();
                        LabList = new List<double>();
                        foreach (string s in ss)
                        {
                            LabList.Add(dparse(s, ref convert));
                        }
                        if (LabList.Count == 1)
                            Lastbet = LabList[0];
                        else if (LabList.Count > 1)
                            Lastbet = LabList[0] + LabList[LabList.Count - 1];
                    }

                
            }

        }

        void martingale(bool Win)
        {
            if (Win)
            {
                if (rdbWinMaxMultiplier.Checked && Winstreak >= WinMaxMultiplies)
                {
                    WinMultiplier = 1;
                }
                else if (rdbWinDevider.Checked && Winstreak % WinDevidecounter == 1 && Winstreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                else if (rdbWinReduce.Checked && Winstreak == WinDevidecounter && Winstreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                Lastbet *= WinMultiplier;
                if (Winstreak == 1)
                {
                    if (!chkMK.Checked)
                    {
                        Lastbet = MinBet;
                    }
                    try
                    {
                        Chance = (double)(nudChance.Value);
                        if (!RunningSimulation)
                            CurrentSite.SetChance(Chance.ToString().Replace(',', '.'));
                    }
                    catch
                    {

                    }
                }
                if (chkTrazel.Checked)
                {
                    high = starthigh;
                }
                if (chkMK.Checked)
                {
                    if (double.Parse((Lastbet - (double)nudMKDecrement.Value).ToString("0.00000000"), System.Globalization.CultureInfo.InvariantCulture) > 0)
                    {
                        Lastbet -= (double)nudMKDecrement.Value;
                    }
                }
                if (chkTrazel.Checked && trazelwin % (double)nudTrazelWin.Value == 0 && trazelwin != 0)
                {
                    Lastbet = (double)nudtrazelwinto.Value;
                    trazelwin = -1;
                    trazelmultiply = true;
                    high = !starthigh;
                }
                else
                {
                    if (chkTrazel.Checked)
                    {
                        Lastbet = MinBet;
                        trazelmultiply = false;
                    }
                }

                if (chkResetBetWins.Checked && Winstreak % nudResetWins.Value == 0)
                {
                    Lastbet = MinBet;
                }
                if (chkChangeWinStreak.Checked && (Winstreak == nudChangeWinStreak.Value))
                {
                    Lastbet = (double)nudChangeWinStreakTo.Value;
                }
                if (checkBox1.Checked)
                {
                    if (Winstreak == nudMutawaWins.Value)
                        Lastbet = mutawaprev *= (double)nudMutawaMultiplier.Value;
                    if (Winstreak == nudMutawaWins.Value + 1)
                    {
                        Lastbet = MinBet;
                        mutawaprev = (double)nudChangeWinStreakTo.Value / (double)nudMutawaMultiplier.Value;
                    }

                }
                if (chkChangeChanceWin.Checked && (Winstreak == nudChangeChanceWinStreak.Value))
                {
                    try
                    {
                        Chance = (double)nudChangeChanceWinTo.Value;
                        if (!RunningSimulation)
                            CurrentSite.SetChance(nudChangeChanceWinTo.Value.ToString().Replace(',', '.'));

                    }
                    catch
                    {

                    }
                }
                        

            }
            else
            {
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
                if (chkTrazel.Checked && trazelmultiply)
                {
                    Multiplier = (double)nudTrazelMultiplier.Value;
                }
                if (chkTrazel.Checked)
                {
                    high = starthigh;
                }
                if (chkTrazel.Checked && Losestreak + 1 >= (double)NudTrazelLose.Value && !trazelmultiply)
                {
                    Lastbet = (double)nudtrazelloseto.Value;
                    trazelmultiply = true;
                    high = !starthigh;
                }
                if (trazelmultiply)
                {
                    trazelwin = -1;

                }
                else
                {
                    trazelwin = 0;
                }
                //set new bet size
                Lastbet *= Multiplier;
                if (chkMK.Checked)
                {
                    Lastbet += (double)nudMKIncrement.Value;
                }
                if (checkBox1.Checked)
                {
                    Lastbet = MinBet;
                }

                //reset bet to minimum if applicable
                if (chkResetBetLoss.Checked && Losestreak % nudResetBetLoss.Value == 0)
                {
                    Lastbet = MinBet;
                }
                //change bet after a certain losing streak
                if (chkChangeLoseStreak.Checked && (Losestreak == nudChangeLoseStreak.Value))
                {
                    Lastbet = (double)nudChangeLoseStreakTo.Value;
                }
            }
        }
        int FibonacciLevel = 0;
        void Fibonacci(bool Win)
        {
            if (Win)
            {
                if (rdbFiboWinIncrement.Checked)
                {
                    FibonacciLevel += (int)nudFiboWinIncrement.Value;
                }
                else if (rdbFiboWinReset.Checked)
                {
                    FibonacciLevel = 0;
                }
                else
                {
                    FibonacciLevel = 0;
                    Stop();
                }
            }
            else
            {
                if (rdbFiboLossIncrement.Checked)
                {
                    FibonacciLevel += (int)nudFiboLossIncrement.Value;
                }
                else if (rdbFiboLossReset.Checked)
                {
                    FibonacciLevel = 0;
                }
                else
                {
                    FibonacciLevel = 0;
                    Stop();
                }
            }
            if (FibonacciLevel < 0)
                FibonacciLevel = 0;
            
            if (FibonacciLevel>= (int)nudFiboLeve.Value & chkFiboLevel.Checked)
            {
                if (rdbFiboLevelReset.Checked)
                    FibonacciLevel = 0;
                else
                {
                    FibonacciLevel = 0;
                    Stop();
                }
            }
            Lastbet = double.Parse(lstFibonacci.Items[FibonacciLevel].ToString().Substring(lstFibonacci.Items[FibonacciLevel].ToString().IndexOf(" ")+1));
        }

        void Alembert(bool Win)
        {
            if (Win)
            {
                
                if ((Winstreak) % (nudAlembertStretchWin.Value +1) == 0)
                {
                    Lastbet += (double)nudAlembertIncrementWin.Value;
                }
            }
            else
            {
                if ((Losestreak) % (nudAlembertStretchLoss.Value + 1) == 0)
                {
                    Lastbet += (double)nudAlembertIncrementLoss.Value;
                }
            }
            if (Lastbet < MinBet)
                Lastbet = MinBet;
        }

        int presetLevel = 0;
        void PresetList(bool Win)
        {
            if (Win)
            {
                if (rdbPresetWinStep.Checked)
                {
                    presetLevel += (int)nudPresetWinStep.Value;
                }
                else if (rdbPresetWinReset.Checked)
                {
                    presetLevel = 0;
                }
                else
                {
                    presetLevel = 0; 
                    Stop();
                }
            }
            else
            {
                if (rdbPresetLossStep.Checked)
                {
                    presetLevel += (int)nudPresetLossStep.Value;
                }
                else if (rdbPresetLossReset.Checked)
                {
                    presetLevel = 0;
                }
                else
                {
                    presetLevel = 0;
                    Stop();
                }
            }
            if (presetLevel < 0)
                presetLevel = 0;
            if (presetLevel > rtbPresetList.Lines.Length-1)
            {
                if (rdbPresetEndStop.Checked)
                {
                    Stop();
                }
                else if (rdbPresetEndStep.Checked)
                {
                    while (presetLevel > rtbPresetList.Lines.Length - 1)
                    {
                        presetLevel -= (int)nudPresetEndStep.Value;
                    }
                }
                else
                {
                    presetLevel = 0;
                }
            }
            double Betval = -1;
            if (presetLevel < rtbPresetList.Lines.Length)
            {
                if (double.TryParse(rtbPresetList.Lines[presetLevel], out Betval))
                {
                    Lastbet = Betval;
                }
                else
                {
                    Stop();
                    MessageBox.Show("Invalid bet in list. Please make sure there is only one bet per line and no other charachters or letters in the list.");
                }
            }
        }

        public void DoBet(bool Win, double profit)
        {
            retriedbet = false;
            if (!stop && !reset)
            {
                //double betresult = dBalance - PreviousBalance;
                if (Win)
                {
                    if (LargestWin < profit)
                        LargestWin = profit;
                }
                else if (Win)
                {
                    if (LargestLoss < profit)
                        LargestLoss = profit;
                }

                if (LargestBet < Lastbet)
                    LargestBet = Lastbet;

                //if its a win
                if (Win && !(reset))
                {

                    if (PreviousBalance != 0)
                    {
                       
                        if (Winstreak == 0)
                        {
                            currentprofit = 0;
                        }                        
                        
                        currentprofit += (Lastbet*(99/Chance))-Lastbet;
                        
                        
                        Wins++;
                        Winstreak++;
                        trazelwin++;
                        CalculateLuck(true);

                        
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
                    
                    if (stoponwin)
                    {
                        Stop();
                    }
                    iMultiplyCounter = 0;                    
                    Multiplier = (double)(nudMultiplier.Value);
                    if (chkReverse.Checked)
                    {
                        if (rdbReverseWins.Checked && Winstreak % NudReverse.Value == 0)
                        {
                            high = !high;
                        }
                    }
                }

                    //if its a loss
                else if (!Win && !(reset))
                {
                    
                    //do i use this line?
                    iMultiplyCounter++;

                    //reset current profit when switching from a winning streak to a losing streak
                    if (Losestreak == 0)
                    {
                        currentprofit = 0;
                    }
                    
                    //adjust profit
                    currentprofit -= Lastbet;
                    
                    //increase losses and losestreak
                    Losses++;
                    Losestreak++;
                    
                    CalculateLuck(false);
                    
                    //update last losing streak if it is above the specified value to show in the stats
                    if (Losestreak >= nudLastStreakLose.Value)
                        laststreaklose = Losestreak;

                    //switch high low if applied in the zig zag tab
                    if (chkReverse.Checked)
                    {
                        if (rdbReverseLoss.Checked && Losestreak % NudReverse.Value == 0)
                        {
                            high = !high;
                        }
                    }

                   

                    /*if (chkTrazel.Checked && Lastbet > (double)NudTrazelLose.Value)
                    {
                        Multiplier = (double)nudTrazelMultiplier.Value;
                    }*/
                    //change chance after a certain losing streak
                    if (chkChangeChanceLose.Checked && (Losestreak == nudChangeChanceLoseStreak.Value))
                    {
                        try
                        {
                            Chance = (double)nudChangeChanceLoseTo.Value;
                            if (!RunningSimulation)
                                CurrentSite.SetChance(nudChangeChanceLoseTo.Value.ToString().Replace(',', '.'));
                            
                            
                        }
                        catch
                        {

                        }
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
                        /*if (chkTrazel.Checked && Winstreak>(double)nudTrazelWin.Value)
                        {
                            Multiplier = (double)nudTrazelMultiplier.Value;
                        }*/
                    }

                    //reset win streak
                    Winstreak = 0;

                    //sounds
                    if (!RunningSimulation)
                    if (Sound && SoundStreak && Losestreak > SoundStreakCount)
                        playalarm();
                    //email
                    if (!RunningSimulation)
                    if (Emails.Streak && Losestreak > Emails.StreakSize)
                        Emails.SendStreak(Losestreak, Emails.StreakSize, dPreviousBalance);

                    
                    //update worst streaks
                    if (!RunningSimulation)
                    if (Losestreak > WorstStreak)
                        WorstStreak = Losestreak;

                    //reset win multplier
                    WinMultiplier = (double)(nudWinMultiplier.Value);

                }
                if (chkReverse.Checked)
                {
                    if (rdbReverseBets.Checked && (Wins+Losses) % NudReverse.Value == 0 )
                    {
                        high = !high;
                    }
                }
                if (!RunningSimulation)
                if (dPreviousBalance > Limit && chkLimit.Checked)
                {

                    if (rdbStop.Checked)
                    {
                        Stop();
                    }
                    else if (rdbWithdraw.Checked)
                    {
                        Withdraw();

                    }
                    else if (rdbInvest.Checked)
                    {
                        Invest();

                    }
                }
                if (!RunningSimulation)
                if (dPreviousBalance - Lastbet < LowerLimit && chkLowerLimit.Checked)
                {
                    TrayIcon.BalloonTipText = "Balance lower than " + nudLowerLimit.Value + "\nStopping Bets...";
                    TrayIcon.ShowBalloonTip(1000);
                    Stop();
                    if (Sound && SoundLow)
                        playalarm();
                    TrayIcon.BalloonTipText = "DiceBot has Stopped Betting\nThe next bet will will have put your Balance below your lower limit";

                    if (Emails.Lower)
                        Emails.SendLowLimit(dPreviousBalance, LowerLimit, Lastbet);
                }


                if (!RunningSimulation)
                if ( Wins!=0 && Losses!=0 && chkResetSeed.Checked)
                {
                    if ( ((rdbResetSeedBets.Checked && (Wins+Losses) % nudResetSeed.Value == 0) ||
                       (rdbResetSeedWins.Checked && Wins % nudResetSeed.Value == 0 && Losestreak==0)||
                       (rdbResetSeedLosses.Checked && Losses % nudResetSeed.Value == 0 && Winstreak == 0)) && !withdrew)
                    {
                        
                        //reset = true;
                        ResetSeed();
                    }
                    
                }
                if (rdbMartingale.Checked)
                {
                    martingale(Win);
                }
                else if (rdbLabEnable.Checked)
                {
                    Labouchere(Win);
                }
                else if (rdbFibonacci.Checked)
                {
                    Fibonacci(Win);
                }
                else if (rdbAlembert.Checked)
                {
                    Alembert(Win);
                }
                else if (rdbPreset.Checked)
                {
                    PresetList(Win);
                }
                if (chkPercentage.Checked)
                {
                    Lastbet = (double)(nudPercentage.Value / (decimal)100.0) * dPreviousBalance;
                }
               
                if (RunningSimulation && (Wins + Losses > nudSimNumBets.Value || Lastbet>PreviousBalance))
                {
                    Stop();
                }
                
                if (!(stop ||reset || withdraw ||invest))
                {
                    //tmBet.Enabled = true;
                    EnableTimer(tmBet, true);
                    try
                    {
                        //if (!RunningSimulation)
                        UpdateStats();
                    }
                    catch
                    {

                    }
                    withdrew = false;
                }


            }
            if (RunningSimulation && stop)
                RunningSimulation = false;

        }

    delegate void dEnableTimer(System.Windows.Forms.Timer tmr, bool enabled);
    void EnableTimer(System.Windows.Forms.Timer tmr, bool enabled)
    {
        if (InvokeRequired)
        {
            Invoke(new dEnableTimer(EnableTimer), tmr, enabled);
            return;
        }
        else
        {
            tmr.Enabled = enabled;
        }

    }
    delegate void dSetTimerInterval(System.Windows.Forms.Timer tmr, int Interval);
    void setInterval(System.Windows.Forms.Timer tmr, int Interval)
    {
        if (InvokeRequired)
        {
            Invoke(new dSetTimerInterval(setInterval), tmr, Interval);
            return;
        }
        else
        {
            tmr.Interval = Interval;
        }

    }

        delegate string[] dGetLabList();
        string[] GetLabList()
        {
            if (InvokeRequired)
            {
                return (string[])Invoke(new dGetLabList(GetLabList));
                
            }
            else
            {
                return rtbBets.Lines;
            }
        }

        bool RunningSimulation = false;
        private void tmBet_Tick(object sender, EventArgs e)
        {
            try
            {
                if (RunningSimulation)
                {
                    Simbet();
                }
                else
                if (CurrentSite.ReadyToBet())
                {
                    Thread.Sleep(100);
                    PlaceBet();
                }
               
            }
            catch
            {

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
                Start(false);
                running = true;
            }
        }

        #endregion
        

        protected override void OnClosing(CancelEventArgs e)
        {
            if (CurrentSite != null)
            {
                CurrentSite.Disconnect();
            }
            save();
            if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim"))
            {
                File.Delete(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim");
            }
            if (File.Exists("currentprofitbet.txt"))
            {
                File.Delete("currentprofitbet.txt");
            }
            if (File.Exists("currentprofittime.txt"))
            {
                File.Delete("currentprofittime.txt");
             }
            string[] files = Directory.GetFiles(".");
            foreach (string F in files)
            {
                if (F.StartsWith(".\\tmp_"))
                {
                    File.Delete(F);
                }
            }
            base.OnClosing(e);
            Application.Exit();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
           
           
            if ((sender as Button).Name.ToUpper().Contains("HIGH"))
            {
                starthigh = high = true;

            }
            else
            {
                starthigh = high = false;
            }
            Start(false);
        }

        #region Login

        void Login()
        {
            if (CurrentSite.AutoLogin)
                loggedin = CurrentSite.Login(username, password);

        }
        
        #endregion



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
            
            Stop();
        }

        #region Save and load settings
        void save()
        {
            save(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings");
            savepersonal();
        }

        void savepersonal()
        {
            using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings3"))
            {
                sw.WriteLine("Amount|" + nudAmount.Value);
                sw.WriteLine("Limit|" + nudLimit.Value);
                if (chkLimit.Checked)
                    sw.WriteLine("LimitEnabled|1");
                else
                    sw.WriteLine("LimitEnabled|0");
                sw.WriteLine("LowerLimit|" + nudLowerLimit.Value);
                sw.Write("LowerLimitEnabled|");
                if (chkLowerLimit.Checked)
                    sw.WriteLine("1");
                else
                    sw.WriteLine("0");
                
                sw.WriteLine("To|" + txtTo.Text);
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
                
                sw.WriteLine("LastStreakWin|" + nudLastStreakWin.Value.ToString("00"));
                sw.WriteLine("LastStreakLose|" + nudLastStreakLose.Value.ToString("00"));
                string msg = "";
                if (chkBotSpeed.Checked)
                    msg = "1";
                else msg = "0";
                sw.WriteLine("BotSpeedEnabled|" + msg);
                sw.WriteLine("BotSpeedValue|" + nudBotSpeed.Value.ToString());
                if (chkResetSeed.Checked)
                    msg = "1";
                else msg = "0";
                sw.WriteLine("ResetSeedEnabled|" + msg);

                if (rdbResetSeedBets.Checked)
                    msg = "0";
                else if (rdbResetSeedWins.Checked)
                    msg = "1";
                else if (rdbResetSeedLosses.Checked)
                    msg = "2";
                sw.WriteLine("ResetSeedMode|" + msg);
                sw.WriteLine("ResetSeedValue|" + nudResetSeed.Value.ToString());
                sw.WriteLine("QuickSwitchFolder|" + txtQuickSwitch.Text);
                sw.WriteLine("SettingsMode|" + cmbSettingMode.SelectedIndex);
                sw.WriteLine("Site|" + cmbSite.SelectedIndex);
                
            }
        }
        
        void save(string file)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
               
                try
                {
                    sw.WriteLine("SaveVersion|" + "2");
                    sw.WriteLine("MinBet|"+nudMinBet.Value);
                    sw.WriteLine("Multiplier|"+nudMultiplier.Value);
                    sw.WriteLine("Chance|"+nudChance.Value);
                    sw.WriteLine("MaxMultiply|"+nudMaxMultiplies.Value);
                    sw.WriteLine("NBets|"+nudNbets.Value);
                    sw.WriteLine("Devider|"+ nudDevider.Value);
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
                    sw.WriteLine("WinMultiplier|" + nudWinMultiplier.Value);
                    sw.WriteLine("WinMaxMultiplies|" + nudWinMaxMultiplies.Value);
                    sw.WriteLine("WinNBets|" + nudWinNBets.Value);
                    sw.WriteLine("WinDevider|" + nudWinDevider.Value);
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
                    
                    sw.WriteLine("ChangeChanceAfterLoseStreakEnabled|" + ((chkChangeChanceLose.Checked)?"1":"0"));
                    sw.WriteLine("ChangeChanceAfterLoseStreakSize|" + nudChangeChanceLoseStreak.Value.ToString("00"));
                    sw.WriteLine("ChangeChanceAfterLoseStreakValue|" + nudChangeChanceLoseTo.Value.ToString());

                    sw.WriteLine("ChangeChanceAfterWinStreakEnabled|" + ((chkChangeChanceWin.Checked) ? "1" : "0"));
                    sw.WriteLine("ChangeChanceAfterWinStreakSize|" + nudChangeChanceWinStreak.Value.ToString("00"));
                    sw.WriteLine("ChangeChanceAfterWinStreakValue|" + nudChangeChanceWinTo.Value.ToString());
                    sw.WriteLine("MutawaMultiplier|" + nudMutawaMultiplier.Value.ToString());
                    sw.WriteLine("MutawaWins|" + nudMutawaWins.Value.ToString());
                    sw.WriteLine("MutawaEnabled|"+(checkBox1.Checked?"1":"0"));
                    

                    sw.WriteLine("TrazalWin|" + nudTrazelWin.Value.ToString());
                    sw.WriteLine("TrazalWinTo|" + nudtrazelwinto.Value.ToString());
                    sw.WriteLine("TrazalLose|" + NudTrazelLose.Value.ToString("00"));
                    sw.WriteLine("TrazalLoseTo|" + nudtrazelloseto.Value.ToString());
                    sw.WriteLine("TrazelMultiPlier|" + nudTrazelMultiplier.Value.ToString());
                    sw.WriteLine("TrazelEnabled|" + (chkTrazel.Checked ? "1" : "0"));

                    sw.WriteLine("MKIncrement|" + nudMKIncrement.Value.ToString());
                    sw.WriteLine("MKDecrement|" + nudMKDecrement.Value.ToString());
                    sw.WriteLine("MKEnabled|" + (chkMK.Checked ? "1" : "0"));

                    sw.WriteLine("LabEnabled|" + (rdbLabEnable.Checked ? "1" : "0"));
                    sw.WriteLine("LabReverse|" + (chkReverseLab.Checked ? "1" : "0"));
                    string labtmp = "";
                    foreach (string s in rtbBets.Lines)
                    {
                        if (labtmp != "")
                            labtmp += "?";
                        labtmp += s;
                    }
                    sw.WriteLine("LabValues|" + labtmp);

                    //rdbLabRestart.Checked = ("1" == getvalue(saveditems, "LabComplete"));
                    //rdbLabStop.Checked = ("2" == getvalue(saveditems, "LabComplete"));
                    sw.WriteLine("LabComplete|" + (rdbLabStop.Checked ? "2" : "1"));

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
                    nudAmount.Value = decimal.Parse(values[i++]);
                    nudLimit.Value = decimal.Parse(values[i++]);
                    if (values[i++] == "1")
                        chkLimit.Checked = true;
                    else
                        chkLimit.Checked = false;
                    nudLowerLimit.Value = decimal.Parse(values[i++]);
                    if (values[i++] == "1")
                        chkLowerLimit.Checked = true;
                    else
                        chkLowerLimit.Checked = false;
                    nudMinBet.Value = decimal.Parse(values[i++]);
                    nudMultiplier.Value = decimal.Parse(values[i++]);
                    
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
                        //chkStopOnWin.Checked = true;
                    }
                    else
                    { 
                        //chkStopOnWin.Checked = false;
                    }
                    if (!sw.EndOfStream)
                    {
                        msg = sw.ReadLine();
                        values = msg.Split(',');
                        if (msg.Contains(";"))
                            values = msg.Split(';');
                        i = 0;
                        nudChance.Value = decimal.Parse(values[i++]);
                        nudMaxMultiplies.Value = decimal.Parse(values[i++]);
                        nudNbets.Value = decimal.Parse(values[i++]);
                        nudDevider.Value = decimal.Parse(values[i++]);
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
                        NudReverse.Value = (decimal)dparse(values[i++], ref convert);
                        if (values.Length > i)
                        {
                            nudLastStreakWin.Value = (decimal)dparse(values[i++], ref convert);
                            nudLastStreakLose.Value = (decimal)dparse(values[i++], ref convert);
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
                        nudResetBetLoss.Value = (decimal)dparse(values[i++], ref convert);
                        chkResetBetWins.Checked = (values[i++] == "1");
                        nudResetWins.Value = (decimal)dparse(values[i++], ref convert);

                        nudWinMultiplier.Value = decimal.Parse(values[i++]);
                        nudWinMaxMultiplies.Value = decimal.Parse(values[i++]);
                        nudWinNBets.Value = decimal.Parse(values[i++]);
                        nudWinDevider.Value = decimal.Parse(values[i++]);
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
                    if (cur.Name == "ReverseOn")
                    {

                    }
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
                    if (System.IO.File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings3"))
                    {
                        using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings3"))
                        {
                            while (!sr.EndOfStream)
                            {
                                string[] s = sr.ReadLine().Split('|');
                                saveditems.Add(new SavedItem(s[0], s[1]));
                            }
                        }
                    }
                    nudAmount.Value=decimal.Parse(getvalue( saveditems, "Amount"));
                    nudLimit.Value = decimal.Parse(getvalue(saveditems, "Limit"));
                    chkLimit.Checked= (getvalue(saveditems, "LimitEnabled") == "1");
                    nudLowerLimit.Value = decimal.Parse(getvalue(saveditems, "LowerLimit"));
                    chkLowerLimit.Checked = (getvalue(saveditems, "LowerLimitEnabled") == "1");
                    nudMinBet.Value = decimal.Parse(getvalue(saveditems, "MinBet"));
                    nudMultiplier.Value = decimal.Parse(getvalue(saveditems, "Multiplier"));
                    
                    txtTo.Text = getvalue(saveditems, "To");
                    string temp = getvalue(saveditems, "OnStop");
                    rdbInvest.Checked = (temp == "0");
                    rdbStop.Checked = (temp == "1");
                    rdbWithdraw.Checked = (temp == "2");
                    //chkStopOnWin.Checked = ("1"==getvalue(saveditems, "StopOnWin"));
                    nudChance.Value = decimal.Parse(getvalue(saveditems, "Chance"));
                    nudMaxMultiplies.Value = decimal.Parse(getvalue(saveditems, "MaxMultiply"));
                    nudNbets.Value = decimal.Parse(getvalue(saveditems, "NBets"));
                    nudDevider.Value = decimal.Parse(getvalue(saveditems, "devider"));
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
                    NudReverse.Value = (decimal)dparse(getvalue(saveditems, "ReverseOn"), ref convert);
                    nudLastStreakWin.Value = (decimal)dparse(getvalue(saveditems, "LastStreakWin"), ref convert);
                    nudLastStreakLose.Value = (decimal)dparse(getvalue(saveditems, "LastStreakLose"), ref convert);
                    chkResetBetLoss.Checked = ("1"==getvalue(saveditems, "ResetBetLossEnabled"));
                    nudResetBetLoss.Value = (decimal)dparse(getvalue(saveditems, "ResetBetLossValue"), ref convert);
                    chkResetBetWins.Checked = ("1"==getvalue(saveditems, "ResetBetWinsEnabled"));
                    nudResetWins.Value = (decimal)dparse(getvalue(saveditems, "ResetWinsValue"), ref convert);
                    nudWinMultiplier.Value = decimal.Parse(getvalue(saveditems, "WinMultiplier"));
                    nudWinMaxMultiplies.Value = decimal.Parse(getvalue(saveditems, "WinMaxMultiplies"));
                    nudWinNBets.Value = decimal.Parse(getvalue(saveditems, "WinNBets"));
                    nudWinDevider.Value = decimal.Parse(getvalue(saveditems, "WinDevider"));
                    temp = getvalue(saveditems, "WinMultiplyMode");
                    rdbWinConstant.Checked = ("0" == temp);
                    rdbWinDevider.Checked = ("1" == temp);
                    rdbWinMaxMultiplier.Checked = ("2" == temp);
                    rdbWinReduce.Checked = ("3" == temp);
                    chkBotSpeed.Checked = ("1"==getvalue(saveditems, "BotSpeedEnabled"));
                    nudBotSpeed.Value = (decimal)dparse(getvalue(saveditems, "BotSpeedValue"), ref convert);
                    chkResetSeed.Checked = ("1"==getvalue(saveditems, "ResetSeedEnabled"));
                    temp = getvalue(saveditems, "ResetSeedMode");
                    rdbResetSeedBets.Checked = ("0" == temp);
                    rdbResetSeedWins.Checked = ("1" == temp);
                    rdbResetSeedLosses.Checked = ("2" == temp);
                    nudResetSeed.Value = (decimal)dparse(getvalue(saveditems, "ResetSeedValue"), ref convert);

                    chkStopLossStreak.Checked = ("1" == getvalue(saveditems, "StopAfterLoseStreakEnabled"));
                    nudStopLossStreak.Value = (decimal)dparse(getvalue(saveditems, "StopAfterLoseStreakValue"), ref convert);
                    chkStopLossBtcStreak.Checked = ("1" == getvalue(saveditems, "StopAfterLoseStreakBtcEnabled"));
                    nudStopLossBtcStreal.Value = (decimal)dparse(getvalue(saveditems, "StopAfterLoseStreakBtcValue"), ref convert);
                    chkStopLossBtc.Checked = ("1" == getvalue(saveditems, "StopAfterLoseBtcEnabled"));
                    nudStopLossBtc.Value = (decimal)dparse(getvalue(saveditems, "StopAfterLoseBtcValue"), ref convert);

                    chkChangeLoseStreak.Checked = ("1" == getvalue(saveditems, "ChangeAfterLoseStreakEnabled"));
                    nudChangeLoseStreak.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterLoseStreakSize"), ref convert);
                    nudChangeLoseStreakTo.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterLoseStreakTo"), ref convert);


                    chkStopWinStreak.Checked = ("1" == getvalue(saveditems, "StopAfterWinStreakEnabled"));
                    nudStopWinStreak.Value = (decimal)dparse(getvalue(saveditems, "StopAfterWinStreakValue"), ref convert);
                    chkStopWinBtcStreak.Checked = ("1" == getvalue(saveditems, "StopAfterWinStreakBtcEnabled"));
                    nudStopWinBtcStreak.Value = (decimal)dparse(getvalue(saveditems, "StopAfterWinStreakBtcValue"), ref convert);
                    chkStopWinBtc.Checked = ("1" == getvalue(saveditems, "StopAfterWinBtcEnabled"));
                    nudStopWinBtc.Value = (decimal)dparse(getvalue(saveditems, "StopAfterWinBtcValue"), ref convert);

                    chkChangeWinStreak.Checked = ("1" == getvalue(saveditems, "ChangeAfterWinStreakEnabled"));
                    nudChangeWinStreak.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterWInStreakSize"), ref convert);
                    nudChangeWinStreakTo.Value = (decimal)dparse(getvalue(saveditems, "ChangeAfterWInStreakTo"), ref convert);

                    chkChangeChanceLose.Checked = ("1" == getvalue(saveditems, "ChangeChanceAfterLoseStreakEnabled"));
                    nudChangeChanceLoseStreak.Value = (decimal)dparse(getvalue(saveditems, "ChangeChanceAfterLoseStreakSize"), ref convert);
                    nudChangeChanceLoseTo.Value = (decimal)dparse(getvalue(saveditems, "ChangeChanceAfterLoseStreakValue"), ref convert);

                    chkChangeChanceWin.Checked = ("1" == getvalue(saveditems, "ChangeChanceAfterWinStreakEnabled"));
                    nudChangeChanceWinStreak.Value = (decimal)dparse(getvalue(saveditems, "ChangeChanceAfterWinStreakSize"), ref convert);
                    nudChangeChanceWinTo.Value = (decimal)dparse(getvalue(saveditems, "ChangeChanceAfterWinStreakValue"), ref convert);
                    /*
                    nudMutawaMultiplier.Value = (decimal)dparse(getvalue(saveditems, "MutawaMultiplier"), ref convert);
                    nudMutawaWins.Value = (decimal)dparse(getvalue(saveditems, "MutawaWins"), ref convert);
                    checkBox1.Checked = ("1" == getvalue(saveditems, "MutawaEnabled"));*/
                    
                    /*nudTrazelWin.Value = (decimal)dparse(getvalue(saveditems, "TrazalWin"), ref convert);
                    nudtrazelwinto.Value = (decimal)dparse(getvalue(saveditems, "TrazalWinTo"), ref convert);
                    NudTrazelLose.Value = (decimal)dparse(getvalue(saveditems, "TrazalLose"), ref convert);
                    nudtrazelloseto.Value = (decimal)dparse(getvalue(saveditems, "TrazalLoseTo"), ref convert);
                    nudTrazelMultiplier.Value = (decimal)dparse(getvalue(saveditems, "TrazelMultiPlier"), ref convert);
                    chkTrazel.Checked = ("1" == getvalue(saveditems, "TrazelEnabled"));*/

                  

                    nudMKIncrement.Value = (decimal)dparse(getvalue(saveditems, "MKIncrement"), ref convert);
                    nudMKDecrement.Value = (decimal)dparse(getvalue(saveditems, "MKDecrement"), ref convert);
                    chkMK.Checked = ("1" == getvalue(saveditems, "MKEnabled"));
                    txtQuickSwitch.Text = getvalue(saveditems, "QuickSwitchFolder");
                    if (txtQuickSwitch.Text!="")
                    {
                        btnStratRefresh_Click(btnStratRefresh, new EventArgs() );
                    }

                    rdbLabEnable.Checked = ("1" == getvalue(saveditems, "LabEnabled"));
                    chkReverseLab.Checked = ("1" == getvalue(saveditems, "LabReverse"));

                   string[] tmp =getvalue(saveditems, "LabValues").Split('?');
                    if (tmp.Length>0)
                    { 
                        if (tmp[0]!="0-0-0")
                            rtbBets.Lines = getvalue(saveditems, "LabValues").Split('?');
                    }
                    rdbLabRestart.Checked = ("1" == getvalue(saveditems, "LabComplete"));
                    rdbLabStop.Checked = ("2" == getvalue(saveditems, "LabComplete"));

                    cmbSite.SelectedIndex = int.Parse(getvalue(saveditems, "Site"));
                    cmbSettingMode.SelectedIndex = int.Parse(getvalue(saveditems, "SettingsMode"));
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
                sw.WriteLine("botname|"+txtBot.Text);
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
                nudMaxMultiplies.Enabled = false;
                nudNbets.Enabled = false;
                nudDevider.Enabled = false;
            }
            if (rdbDevider.Checked || rdbReduce.Checked)
            {
                nudMaxMultiplies.Enabled = false;
                nudNbets.Enabled = true;
                nudDevider.Enabled = true;
            }
            if (rdbMaxMultiplier.Checked)
            {
                nudMaxMultiplies.Enabled = true;
                nudNbets.Enabled = false;
                nudDevider.Enabled = false;
            }

            //set active controlls for on win multiplier
            if (rdbWinConstant.Checked)
            {
                nudWinMaxMultiplies.Enabled = false;
                nudWinNBets.Enabled = false;
                nudWinDevider.Enabled = false;
            }
            if (rdbWinDevider.Checked || rdbWinReduce.Checked)
            {
                nudWinMaxMultiplies.Enabled = false;
                nudWinNBets.Enabled = true;
                nudWinDevider.Enabled = true;
            }
            if (rdbWinMaxMultiplier.Checked)
            {
                nudWinMaxMultiplies.Enabled = true;
                nudWinNBets.Enabled = false;
                nudWinDevider.Enabled = false;
            }
        }

        private void txtChance_Leave(object sender, EventArgs e)
        {
            testInputs();
            try
            {
                CurrentSite.SetChance(Chance.ToString().Replace(",", "."));
                                
            }
            catch
            {

            }
        }

        double dparse(string text,ref bool success)
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
                text = text.Replace(".", ",");
            }

            //text = text.Replace(",", ".");
            if (!double.TryParse(text, out number))
            {
                //text = text.Replace(".", ",");
                if (!double.TryParse(text, out number))
                {
                    success = false;
                    return -1;
                    
                }
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
            success = true;
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
            populateFiboNacci();
            string sMessage = "";
            bool valid = true;
            //double d = double.Parse(txtLimit.Text.Replace('.',','));
            /*if (!double.TryParse(txtLimit.Text, out Limit))
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }*/
            Limit = (double)(nudLimit.Value);
            if (Limit == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }
            LowerLimit = (double)(nudLowerLimit.Value);
            if (LowerLimit == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }
            Amount = (double)(nudAmount.Value);
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
            MinBet = (double)(nudMinBet.Value);
            if (MinBet==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Minimum Bet Field\n";
            }
            Chance = (double)(nudChance.Value);
            if (Chance == -1)
            {
                valid = false;
                sMessage += "Please enter a valid % in the Chance Field (Without the % sign)";
            }
            else
            {

            }
            Multiplier = (double)(nudMultiplier.Value);
            if (Multiplier == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Multiplier Field\n";
            }
            MaxMultiplies= (int)(nudMaxMultiplies.Value);
            if (MaxMultiplies==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Max Multplies Field\n";
            }
            Devidecounter = (int)( nudNbets.Value);
            if (Devidecounter==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the After n bets Field\n";
            }
            Devider = (double)(nudDevider.Value);
            if (Devider == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Times Multiplier By Field\n";
            }
            WinMultiplier = (int)(nudWinMultiplier.Value);
            if (WinMultiplier == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Multiplier Field\n";
            }
            WinMaxMultiplies = (int)(nudWinMaxMultiplies.Value);
            if (WinMaxMultiplies == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Max Multplies Field\n";
            }
            WinDevidecounter = (int)(nudWinNBets.Value);
            if (WinDevidecounter == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the After n bets Field\n";
            }
            WinDevider = (double)(nudWinDevider.Value);
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
                Start(false);
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

        
        #region Simulate and bet generator
        Simulation tempsim;
        
        Thread simthread;
        string server = "";
        string client = "";
        double tmpbalance = 0;
        int tmpwins = 0;
        int tmplosses = 0;
        double tmpStartBalance = 0;
        void runsim()
        {
            tmpbalance = PreviousBalance;
            tmpwins = Wins;
            tmplosses = Losses;
            tmpStartBalance = StartBalance;
            StartBalance = dPreviousBalance = (double)nudSimBalance.Value;
            Wins = Losses = 0;
            
            
            
            string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ._";
            server = "";

            for (int i = 0; i < 64; i++)
            {
                server += (chars[rand.Next(0, chars.Length)]);
            }
            client = "";
            for (int i = 0; i < 24; i++)
            {
                client += rand.Next(0, 10).ToString();
            }

            string sserver = "";
            foreach (byte b in server)
            {
                sserver += Convert.ToChar(b);
            }
            
            tempsim = new Simulation(dPreviousBalance.ToString("0.00000000"), (Wins+Losses).ToString(), sserver, client);
            RunningSimulation = true;
            stop = false;
            Lastbet = MinBet;
            Start(false);
            
        }

        void Simbet()
        {
            dtLastBet = DateTime.Now;
            EnableTimer(tmBet, false);
            if (Wins + Losses <= nudSimNumBets.Value)
            {
                string betstring = (Wins + Losses).ToString() + ",";
                double number = CurrentSite.GetLucky(server, client, Wins + Losses);
                betstring += number.ToString() + "," + Chance.ToString() + ",";
                bool win = false;
                if (high)
                    betstring += ">" + (100 - Chance) + ",";
                else
                    betstring += "<" + Chance + ",";
                if (high && number > 100 - Chance)
                {
                    win = true;
                }
                else if (!high && number < Chance)
                {
                    win = true;
                }
                double betProfit = 0;
                if (win)
                {
                    betstring += "win,";
                    betstring += Lastbet + ",";
                    betProfit = (Lastbet * 99 / Chance) - Lastbet;
                    betstring += betProfit  + ",";
                    

                }
                else
                {

                    betstring += "lose,";
                    betstring += Lastbet + ",";
                    betProfit = -Lastbet ;
                    betstring +=  betProfit +",";
                    
                }
                this.PreviousBalance = dPreviousBalance + betProfit;
                betstring += PreviousBalance + ",";
                betstring += profit;
                tempsim.bets.Add(betstring);
                int bets = Wins + Losses;
                if (bets % 1000 == 0)
                {
                    Updatetext(lblSimProgress, ((double)bets / (double)nudSimNumBets.Value * 100.00).ToString("00.00") + "%");
                }
                if (bets % 10000 == 0)
                {
                    using (StreamWriter sw = File.AppendText(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim"))
                    {
                        foreach (string tmpbet in tempsim.bets)
                        {
                            sw.WriteLine(tmpbet);
                        }

                    }
                    tempsim.bets.Clear();
                }

                GetBetResult(PreviousBalance, win, betProfit);
            }
            else
                Stop();
            
        }

        delegate void DelAlterMsgLog(Control TextBox, string Text);
        public static void Updatetext(Control TextBox, string Text)
        {
            if (TextBox.InvokeRequired)
            {
                DelAlterMsgLog del = new DelAlterMsgLog(Updatetext);
                TextBox.Invoke(del, TextBox, Text);
            }
            else
            {
                TextBox.Text = Text;
            }
        }

        private void btnSim_Click(object sender, EventArgs e)
        {
            if (! stop)
            {
                MessageBox.Show("Please stop the bot before running a simulation.");
            }
            else
            { 
                bool go = true;
                if (nudSimNumBets.Value >= 1000000)
                {
                    go = (MessageBox.Show("To keep RAM usage to a minimum, "+
                                            "\nthe sim data is temporarily stored on your"+
                                            "\nlocal C: drive. This file can become very large," +
                                            "\nApproximately 80MB per 1M bets. This file is"+
                                            "\ndeleted when the bot is closed normally.\n\nContinue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes);
                }
                if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim"))
                {
                    File.Delete(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim");
                }
                tmrSimulation.Enabled = true;
                lblSimRun.Text = "Running Simulation, Please Wait";
                lblSimRun.ForeColor = Color.Red;
                simthread = new Thread(new ThreadStart(runsim));
                simthread.Start();
            }
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
                        MessageBox.Show("Please run a simulation first");
                    }
                    else
                    {
                       
                        File.Copy(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\tempsim", svdExportSim.FileName);
                        
                        MessageBox.Show("exported to " + svdExportSim.FileName);
                    }
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
                    string curstring = i.ToString() + "," + CurrentSite.GetLucky(txtServerSeed.Text, txtClientSeed.Text, (int)i).ToString();
                    Betlist.Add(curstring);
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter("LuckyNum-" + DateTime.Now.ToShortDateString().Replace("/","-") + ".csv"))
                    {
                        foreach (string s in Betlist)
                        {
                            sw.WriteLine(s);
                        }
                    }
                    MessageBox.Show("Saved bets to: " + "LuckyNum-" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".csv");
                }
                catch
                {
                    MessageBox.Show("Failed saving bets to: " + "LuckyNum-" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".csv");
                }
            }
            else
            {
                MessageBox.Show("Please enter a server seed and a client seed");
            }
        }

        private void tmrSimulation_Tick(object sender, EventArgs e)
        {
            if (!RunningSimulation)
            {
                tmrSimulation.Enabled = false;
                lblSimRun.ForeColor = Color.Green;
                lblSimRun.Text = "Simulation Completed";
                lastsim = tempsim;
            }
            /*if (simthread != null)
            {
                if (!simthread.IsAlive)
                {
                    
                }
            }*/
        }
        #endregion

        private void btnResetStats_Click(object sender, EventArgs e)
        {
            Wins = 0;
            Losses = 0;
            bool success = false;
            double tmp = Getbalance(out success);
            if (success)
                StartBalance = tmp;
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

       

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://bitcointalk.org/index.php?topic=391870");
        }


        

        #region charts
        //button for generating random charts - for testing purposes
        private void button1_Click(object sender, EventArgs e)
        {
            List<Bet> tmpBets = new List<Bet>();
            double previous = 0;
            for (int i = 0; i < r.Next(1000, 100000); i++)
            {
                
                int tmp = r.Next(0, 10);
                if (tmp % 2 == 0)
                {
                    previous -= tmp;
                }
                else
                    previous += tmp;
                tmpBets.Add(new Bet { Id = i, Profit = (decimal)previous });
            }

            Graph g = new Graph(tmpBets.ToArray());
            g.Show();
        }

    

        #region generate charts


        
        
        private void btnChartBetID_Click(object sender, EventArgs e)
        {
            Graph g = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, (long)nudGraphStartBetID.Value));
            g.Show();
        }

        private void btnChartTimeRange_Click(object sender, EventArgs e)
        {
            Graph g = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, dtpStart.Value, dtpEnd.Value));
            g.Show();
        }
        private void btnGraphProfitBets_Click(object sender, EventArgs e)
        {
            bool created = false;
            if (LiveGraph != null)
            {
                if (LiveGraph.IsDisposed)
                {
                    LiveGraph = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, OpenTime, DateTime.Now.AddYears(1)));
                    LiveGraph.Show();
                    created = true;
                }
            }
            else
            {
                LiveGraph = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, OpenTime, DateTime.Now.AddYears(1)));
                LiveGraph.Show();
                created = true;
            }
            if (!created)
                MessageBox.Show("Live chart is already open. Please close the current live chart window before opening a new one.");
            //currentprofitbet();
        }

        private void btnGraphProfitTime_Click(object sender, EventArgs e)
        {
            Graph g = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, OpenTime, OpenTime.AddYears(1)));
            g.Show();
        }

        private void btnChartAllTimeProfitBets_Click(object sender, EventArgs e)
        {
            bool created = false;
            if (LiveGraph != null)
            {
                if (LiveGraph.IsDisposed)
                {
                    LiveGraph = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name));
                    LiveGraph.Show();
                    created = true;
                }
            }
            else
            {
                LiveGraph = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name));
                LiveGraph.Show();
                created = true;
            }
            if (!created)
                MessageBox.Show("Live chart is already open. Please close the current live chart window before opening a new one.");
            
        }

        private void btnChartAllTimeProfitTime_Click(object sender, EventArgs e)
        {
            Graph g = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name));
            g.Show();
        }


        #endregion
        #endregion


        private void btnStopOnWin_Click(object sender, EventArgs e)
        {
            stoponwin = true;
        }

        private void cmbSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableNotLoggedInControls(false);
            if (CurrentSite!=null)
            {
                CurrentSite.Disconnect();
            }
            if (CurrentSite is PD)
            {
                (CurrentSite as PD).ispd = false;
            }
            switch (cmbSite.SelectedIndex)
            {
                case 0: CurrentSite = new JD(this); break;

                /*case 1: CurrentSite = new PRC(); if (!(url.StartsWith("pocketrocketscasino.eu") || url.StartsWith("pocketrocketscasino.eu"))) { gckBrowser.Navigate("https://pocketrocketscasino.eu/ref/357"); } pnlApiInfo.Visible = false; gckBrowser.Visible = true; gckBrowser.Dock = DockStyle.Fill; break;//############
                    //############################################
                    //############################################

                case 2: CurrentSite = new D999(); if (!(url.StartsWith("999dice.com") || url.StartsWith("www.999dice.com"))) { gckBrowser.Navigate("www.999dice.com/?20073598"); } pnlApiInfo.Visible = false; gckBrowser.Visible = true; gckBrowser.Dock = DockStyle.Fill; break;

                //case 3: CurrentSite = new PRC2(); if (!(url.StartsWith("") )) { gckBrowser.Navigate(""); } break;
                case 3: CurrentSite = new SafeDice(); if (!(url.StartsWith("safedice.com"))) { gckBrowser.Navigate("safedice.com/?r=1050"); } pnlApiInfo.Visible = false; gckBrowser.Visible = true; gckBrowser.Dock = DockStyle.Fill; break;
                */case 4: CurrentSite = new PD(this); break;
                
            }
            rdbInvest.Enabled = CurrentSite.AutoInvest;
            if (!rdbInvest.Enabled)
                rdbInvest.Checked = false;
            rdbWithdraw.Enabled = CurrentSite.AutoWithdraw;
            if (!rdbWithdraw.Enabled)
                rdbWithdraw.Checked = false;
            
        }

        private void btnBrowseStratFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            if (fdb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtQuickSwitch.Text = fdb.SelectedPath;
            }
        }

        private void btnStratRefresh_Click(object sender, EventArgs e)
        {
            lsbStrats.Items.Clear();
            cmbStrat.Items.Clear();
            if (Directory.Exists(txtQuickSwitch.Text))
            {
                foreach (string x in Directory.GetFiles(txtQuickSwitch.Text))
                {
                    using (StreamReader sr = new StreamReader(x))
                    {
                        string tmptxt = sr.ReadLine();
                        if (tmptxt.StartsWith("SaveVersion"))
                        {
                            lsbStrats.Items.Add( new FileInfo(x).Name );
                            cmbStrat.Items.Add(new FileInfo(x).Name);
                        }
                    }
                }
            }
        }

        private void cmbStrat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(txtQuickSwitch.Text+"\\"+cmbStrat.SelectedItem.ToString()))
            {
                load(txtQuickSwitch.Text + "\\" + cmbStrat.SelectedItem.ToString());
            }
        }

        private void cmbStrat_Click(object sender, EventArgs e)
        {
            if (cmbStrat.Items.Count<1)
            {
                MessageBox.Show("Theres nothing here! You probably still need to specify a folder for this feature to work.\n\n"+
                    "Go to the Advanced Bet Settings tab, then click the browse button below the 'Quick Switch Folder' text box.\n"+
                    "Select a folder with some exported strategies in and click refresh. The usable files will be identified and the strategies loaded. You can now switch between them using the drop down menu.");
            }
        }

        
        private void btnBrowseLab_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdLab = new OpenFileDialog();
            if (ofdLab.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(ofdLab.FileName))
                try
                {
                    string s = File.ReadAllText(ofdLab.FileName);
                    string[] ss = s.Split('\n');
                    foreach (string sss in ss)
                    {
                        dparse(sss, ref convert);
                        if (!convert)
                            break;
                    }
                    if (convert)
                    {
                        rtbBets.Text = s;
                        
                    }
                    else
                    {
                        MessageBox.Show("Invalid bets file. Please make sure there are only bets in the file, 1 per line. NO other characters are permitted.");
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid bets file. Please make sure there are only bets in the file, 1 per line. NO other characters are permitted.");
                }
            }
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        #region api controlls
        ///multithread updates for api based sites
        delegate void dupdateControll(object value);

        public void updateBalance(object Balance)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateBalance), Balance);
            }
            else
            {
                lblApiBalance.Text = decimal.Parse(Balance.ToString(), System.Globalization.CultureInfo.InvariantCulture).ToString("0.00000000");
            }
        }

        public void updateDeposit(object Address)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateDeposit), Address);
            }
            else
            {
                txtApiAddress.Text = Address.ToString();
            }
        }

        public void updateWins(object Wins)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateWins), Wins);
            }
            else
            {
                lblApiWins.Text = (Wins).ToString();
            }
        }

        public void updateLosses(object Wins)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateLosses), Wins);
            }
            else
            {
                lblApiLosses.Text = (Wins).ToString();
            }
        }

        public void updateBets(object Bets)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateBets), Bets);
            }
            else
            {
                lblApiBets.Text = (Bets).ToString();
            }
        }

        public void updateProfit(object _Profit)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateProfit), _Profit);
            }
            else
            {

                decimal Profit = 0;
                Profit = Convert.ToDecimal(_Profit);
               
                 lblApiProfit.Text = ((decimal)Profit).ToString("0.00000000");
                if ((decimal)Profit==0)
                {
                    lblApiProfit.ForeColor = Color.Blue;
                }
                else if ((decimal)Profit > 0)
                {
                    lblApiProfit.ForeColor = Color.Green;
                }
                else 
                {
                    lblApiProfit.ForeColor = Color.Red;
                }
            }
        }

        public void updateWagered(object _Wagered)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateWagered), _Wagered);
            }
            else
            {
                decimal Wagered = 0;
                Wagered = Convert.ToDecimal(_Wagered);
                lblApiWagered.Text = (Wagered).ToString("0.00000000");
            }
        }

        public void updateBet(object _Bet)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateBet), _Bet);
            }
            else
            {
                decimal Bet = 0;
                Bet = Convert.ToDecimal(_Bet);
                nudApiBet.Value = (decimal)Bet;
            }
        }

        public void updateChance(object _Chance)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateChance), _Chance);
            }
            else
            {
                decimal Chance = 0;
                Chance = Convert.ToDecimal(_Chance);
                nudApiChance.Value = ((decimal)Chance);
            }
        }

        public void updatePayout(object _Payout)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updatePayout), _Payout);
            }
            else
            {
                decimal Payout = 0;
                Payout = Convert.ToDecimal(_Payout);
                nudApiPayout.Value = (decimal)Payout;
            }
        }

        public void updateBetProfit(object _BetProfit)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateBetProfit), _BetProfit);
            }
            else
            {
                decimal BetProfit = 0;
                BetProfit = Convert.ToDecimal(_BetProfit);
                lblApiBetProfit.Text = ((decimal)BetProfit).ToString("0.00000000");
            }
        }

        public void updateStatus(object Status)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateStatus), Status);
            }
            else
            {
                lblStatus.Text = Status.ToString();
            }
        }
        List<Bet> BetsToShow = new List<Bet>();

        public void AddBet(object Bet)
        {
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(AddBet), Bet);
            }
            else
            {
                if (LiveGraph != null)
                    if (!LiveGraph.IsDisposed)
                        LiveGraph.AddBet(Bet as Bet);
                sqlite_helper.AddBet(Bet as Bet, CurrentSite.Name);
                BetsToShow.Insert(0, (Bet)Bet);
                if (BetsToShow.Count>100)
                {
                    BetsToShow.RemoveAt(BetsToShow.Count - 1);
                }
                BindingSource bs = new BindingSource();
                bs.DataSource = BetsToShow;
                dataGridView1.DataSource = bs;
                foreach (DataGridViewRow Myrow in dataGridView1.Rows)
                {           
                    if (Myrow.Cells[6].Value != null)
                    {
                        if (((bool)Myrow.Cells[3].Value ? (decimal)Myrow.Cells[5].Value < 100m - (decimal)(Myrow.Cells[4].Value) : (decimal)Myrow.Cells[5].Value > (decimal)(Myrow.Cells[4].Value)))
                        {
                            Myrow.DefaultCellStyle.BackColor = Color.Pink;
                        }
                        else
                        {
                            Myrow.DefaultCellStyle.BackColor = Color.LightGreen;
                        }
                    }
                }
                
            }
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "Log In")
            {
                if (CurrentSite.Login(txtApiUsername.Text, txtApiPassword.Text, txtApi2fa.Text))
                {
                    EnableNotLoggedInControls(true);

                }
            }
            else
            {
                if (CurrentSite!=null)
                {
                    Stop();
                    CurrentSite.Disconnect();
                    EnableNotLoggedInControls(false);
                }
            }
        }


        #endregion

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (CurrentSite.Register(txtApiUsername.Text, txtApiPassword.Text))
            {
                EnableNotLoggedInControls(true);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==0)
            {

                string url = CurrentSite.BetURL+ dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                Process.Start(url);
            }
        }

        /// <summary>
        /// place single bet, HIGH
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            CurrentSite.SetAmount((double)nudApiBet.Value);
            CurrentSite.SetChance(nudApiChance.Value.ToString("0.00000000"));
            CurrentSite.PlaceBet(true);
        }

        /// <summary>
        /// place single bet, Low
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            CurrentSite.SetAmount((double)nudApiBet.Value);
            CurrentSite.SetChance(nudApiChance.Value.ToString("0.00000000"));
            CurrentSite.PlaceBet(false);
        }

        private void nudApiBet_ValueChanged(object sender, EventArgs e)
        {
            if ((sender as NumericUpDown).Name == "nudApiBet")
            {

                lblApiBetProfit.Text = ((nudApiBet.Value * nudApiPayout.Value) - nudApiBet.Value).ToString("0.00000000"); 
            }
            else if ((sender as NumericUpDown).Name == "nudApiChance")
            {
                decimal payout = (100m - CurrentSite.edge) / (nudApiChance.Value);
                if (nudApiPayout.Value != payout)
                    nudApiPayout.Value = payout;
                lblApiBetProfit.Text = ((nudApiBet.Value * payout) - nudApiBet.Value).ToString("0.00000000"); 
            }
            else if ((sender as NumericUpDown).Name == "nudApiPayout")
            {
                decimal chance = (100m - CurrentSite.edge) / (nudApiPayout.Value);
                if (nudApiChance.Value != chance)
                    nudApiChance.Value = chance;
                lblApiBetProfit.Text = ((nudApiBet.Value * nudApiPayout.Value) - nudApiBet.Value).ToString("0.00000000"); 
            }
        }

        //settings mode combobox
        List<TabPage> Tabs = new List<TabPage>();
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           /* if (cmbSettingMode.SelectedIndex==0 && tcBetSettings.TabCount>3)
            {
                Tabs.Add(tcBetSettings.TabPages[3]);
                tcBetSettings.TabPages.RemoveAt(3);
                Tabs.Add(tcBetSettings.TabPages[3]);
                tcBetSettings.TabPages.RemoveAt(3);
                Tabs.Add(tcBetSettings.TabPages[3]);
                tcBetSettings.TabPages.RemoveAt(3);
                Tabs.Add(tcBetSettings.TabPages[3]);
                tcBetSettings.TabPages.RemoveAt(3);
                pnlMartingaleLossAdvanced.Visible = false;
                pnlMartingaleWinAdvanced.Visible = false;
                pnlAdvancedAdvanced.Visible = false;
                label82.Visible = false;
                cmbStrat.Visible = false;
                
            }
            else if (cmbSettingMode.SelectedIndex==1)
            {
                for (int i = 0; i < Tabs.Count; i++)
                {
                    tcBetSettings.TabPages.Add(Tabs[i]);
                    Tabs.RemoveAt(i--);
                    pnlMartingaleLossAdvanced.Visible = true;
                    pnlMartingaleWinAdvanced.Visible = true;
                    pnlAdvancedAdvanced.Visible = true;
                    label82.Visible = true;
                    cmbStrat.Visible = true;
                }
            }*/
                
        }

        private void btnBetHistory_Click(object sender, EventArgs e)
        {
            BetHistory tmp = new BetHistory(CurrentSite.Name);
            tmp.Show();
        }

        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            if (CurrentSite.AutoWithdraw)
            {
                string Response = Interaction.InputBox("Amount to withdraw: ", "Withdraw", "0", -1, -1);
                double tmpAmount = 0;
                if (double.TryParse(Response, out tmpAmount))
                {
                    string Address = Interaction.InputBox("Bitcoin Address: ", "Withdraw", "", -1, -1);
                    System.Text.RegularExpressions.Regex txt = null;

                    txt = new System.Text.RegularExpressions.Regex(@"^[13][a-km-zA-HJ-NP-Z0-9]{26,33}$");

                    bool valid = txt.IsMatch(Address);
                    if (valid)
                    {

                        CurrentSite.Withdraw(tmpAmount, "");
                    }
                    else

                        MessageBox.Show("Invalid Address");
                }
                else
                {
                    MessageBox.Show("Input not a valid number");
                }
            }
        }

        //will invest at default kelly for multikelly sites with a default, 0.5% for multikelly sites that have no default.
        private void btnInvest_Click(object sender, EventArgs e)
        {
            if (CurrentSite.AutoInvest)
            {
                string Response = Interaction.InputBox("Amount to invest: ", "Invest", "0", -1, -1);
                double tmpAmount = 0;
                if (double.TryParse(Response, out tmpAmount))
                {
                    CurrentSite.Invest(tmpAmount);
                    
                }
                else
                {
                    MessageBox.Show("Input not a valid number");
                }
            }
        }

        private void btnTip_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature has not yet been implemented.");
        }

        private void rdbPreset_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton tmp = sender as RadioButton;
            if (tmp != rdbAlembert)
            {
                if (rdbAlembert.Checked && tmp.Checked)
                    rdbAlembert.Checked = false;
            }
            if (tmp != rdbMartingale)
            {
                if (rdbMartingale.Checked && tmp.Checked)
                    rdbMartingale.Checked = false;
            }
            if (tmp != rdbLabEnable)
            {
                if (rdbLabEnable.Checked && tmp.Checked)
                    rdbLabEnable.Checked = false;
            }
            if (tmp !=rdbFibonacci)
            {
                if (rdbFibonacci.Checked && tmp.Checked)
                    rdbFibonacci.Checked = false;
            }
            if (tmp != rdbPreset)
            {
                if (rdbPreset.Checked && tmp.Checked)
                    rdbPreset.Checked = false;
            }
        }

        //Browse for preset list of bets
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdLab = new OpenFileDialog();
            if (ofdLab.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(ofdLab.FileName))
                    try
                    {
                        string s = File.ReadAllText(ofdLab.FileName);
                        string[] ss = s.Split('\n');
                        foreach (string sss in ss)
                        {
                            dparse(sss, ref convert);
                            if (!convert)
                                break;
                        }
                        if (convert)
                        {
                            
                            rtbPresetList.Text = s;

                        }
                        else
                        {
                            MessageBox.Show("Invalid bets file. Please make sure there are only bets in the file, 1 per line. NO other characters are permitted.");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Invalid bets file. Please make sure there are only bets in the file, 1 per line. NO other characters are permitted.");
                    }
            }
        }

        

        

        

        

    }

}
