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
using SharpLua;
using WMPLib;
using System.Globalization;
using System.Reflection;

namespace DiceBot
{
    
    public partial class cDiceBot : Form
    {
        #region saving and loading strat vars
        Dictionary<string, Control> SaveNames = new Dictionary<string, Control>();
        Dictionary<string, Control> PSaveNames = new Dictionary<string, Control>();
        #endregion

        //Version number to test against site
        private const string vers = "3.2.3";


        Control[] ControlsToDisable;
        
        DateTime OpenTime = DateTime.Now;
        Random r = new Random();
        Graph LiveGraph;
        Stats StatsWindows = new Stats();
        Simulate SimWindow;
        
        #region Variables
        public int logging = 0;
        Random rand = new Random();
        bool retriedbet = false;
        decimal StartBalance = 0;        
        decimal Lastbet = -1;
        decimal MinBet = 0;
        decimal Multiplier = 0;
        decimal WinMultiplier = 0;
        decimal Limit = 0;
        decimal Amount = 0;
        
        decimal LargestBet = 0;
        decimal LargestWin = 0;
        decimal LargestLoss = 0;
        decimal LowerLimit = 0;
        decimal Devider = 0;
        decimal WinDevider = 0;
        decimal Chance = 0;
        decimal avgloss = 0;
        decimal avgwin = 0;
        decimal avgstreak = 0;
        decimal currentprofit = 0;
        decimal profit = 0;
        decimal luck = 0;
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
        
        bool stop = true;
        bool withdraw = false;
        bool invest = false;
        bool reset = false;
        bool running = false;
        bool stoponwin = false;

        #region settings vars
        public bool tray = false;
        public bool Sound = true;
        public bool SoundWithdraw=true;
        public bool SoundLow = true;
        public bool SoundStreak = false;
        public bool autologin = false;
        public bool autostart = false;
        public string username = "";
        public string password = "";
        public string Botname = "";
        public Email Emails { get; set; }
        bool autoseeds = true;
        string ching = "";
        string salarm = "";
        bool startupMessage = true;
        int donateMode = 2;
        decimal donatePercentage = 1;
        #endregion


        bool high = true;
        bool starthigh = true;
        private bool withdrew;
        DateTime dtStarted = new DateTime();
        DateTime dtLastBet = new DateTime();
        TimeSpan TotalTime = new TimeSpan(0, 0, 0);
        Simulation lastsim;

        //labouchere
        List<decimal> LabList = new List<decimal>();
        #endregion

        DiceSite CurrentSite;
        private decimal dPreviousBalance;
        delegate void dpopFibonacci();
        void populateFiboNacci()
        {
            DumpLog("Populating Fibonacci",7);
            if (InvokeRequired)
            {
                Invoke(new dpopFibonacci(populateFiboNacci));
                return;
            }
            else
            {
            decimal Previous = 0;
            decimal Current = (decimal)MinBet ;
            lstFibonacci.Items.Clear();
            for (int i =0; i<100; i++)
            {
                lstFibonacci.Items.Add(string.Format("{0}. {1}", i, Current));
                decimal tmp = Current;
                Current += Previous;
                Previous = tmp;
            }
            }
        }

        public decimal PreviousBalance
        {
            get { return dPreviousBalance; }
            set 
            {
               
                dPreviousBalance = value; 
            }
        }

        decimal Chartprofit = 0;
        delegate void dDobet(Bet bet);
        public void GetBetResult(decimal Balance, Bet bet)
        {
            DumpLog("received bet result: Balance: "+Balance+", Bet:"+json.JsonSerializer<Bet>(bet) , 6);
            if (logging>2)
            using (StreamWriter sw = File.AppendText("log.txt"))
            {
                sw.WriteLine(json.JsonSerializer<Bet>(bet));
            }
            PreviousBalance = (decimal)Balance;
            profit += (decimal)bet.Profit;
            Chartprofit += (decimal)bet.Profit;
            if (!RunningSimulation)
            {
                AddChartPoint(profit);
            }
            if (InvokeRequired)
            {
                Invoke(new dDobet(DoBet),bet);
            }
            else
                DoBet(bet);
            
            
            
        }

        delegate void dAddChartPoint(decimal Profit);
        void AddChartPoint(decimal Profit)
        {

            if (InvokeRequired)
            {
                Invoke(new dAddChartPoint(AddChartPoint), profit);
            }
            else
            {
                
                if (chrtEmbeddedLiveChart.Enabled)
                chrtEmbeddedLiveChart.Series[0].Points.AddY(Chartprofit);
            }
        }

        void EnableNotLoggedInControls(bool Enabled)
        {
            DumpLog("Funushed logging in. Result: " + Enabled, 6);
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
        
        public cDiceBot(string[] args)
        {
            
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture =  new CultureInfo("en-US");
            sqlite_helper.CheckDBS();
            InitializeComponent();
            tsmiVersion.Text = "Version "+vers;
            foreach (string s in args)
            {
                if (s.StartsWith("log="))
                {
                    string loglev = s.Substring(s.IndexOf("=") + 1);
                    int.TryParse(loglev, out LogLevel);
                }
            }
            DumpLog("starting bot "+ vers, 6);
            PopulateSaveNames();
            WriteConsole("Starting Dicebot " + vers);
            PopoutChat.SendMessage += PopoutChat_SendMessage;
            SimWindow = new Simulate(this);
            StatsWindows.btnResetStats.Click += btnResetStats_Click;
            
            ControlsToDisable = new Control[] { btnApiBetHigh, btnApiBetLow, btnWithdraw, btnInvest, btnTip, btnStartHigh, btnStartLow, btnStartHigh2, btnStartLow2, btnMPWithdraw, btnMPDeposit };
            EnableNotLoggedInControls(false);
            basicToolStripMenuItem.Checked = true;
            chrtEmbeddedLiveChart.Series[0].Points.AddXY(0, 0);
            chrtEmbeddedLiveChart.ChartAreas[0].AxisX.Minimum = 0;
            #region tooltip Texts
            ToolTip tt = new ToolTip();
            tt.SetToolTip(gbZigZag , "After every n bets/wins/losses \n(as specified to the right), \nthe bot will switch from \nbetting high to low or vica verca");
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
                primeDiceToolStripMenuItem.Checked = true;

                bool frst = true;
            foreach (string s in dice999.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem{ Text=s};

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                diceToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;
                
                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;
                
            }
            foreach (string s in dice999.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                dogeToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }

            foreach (string s in SafeDice.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                safediceToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }

            foreach (string s in bitdice.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                bitDiceToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            foreach (string s in CoinMillions.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                coinMillionsToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            foreach (string s in BB.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                betterbetsToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            foreach (string s in WD.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                wealthyDiceToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            foreach (string s in FortuneJack.cCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                fortuneJackToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            foreach (string s in cryptogames.sCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                cryptoGamesToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            foreach (string s in Bitsler.sCurrencies)
            {
                ToolStripMenuItem tmpItem = new ToolStripMenuItem { Text = s };

                if (frst)
                {
                    tmpItem.Checked = true;
                    frst = false;
                }

                bitslerToolStripMenuItem.DropDown.Items.Add(tmpItem);
                tmpItem.Click += btcToolStripMenuItem_Click;

                tmpItem.CheckedChanged += btcToolStripMenuItem_CheckedChanged;

            }
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings"))
            {
                if (MessageBox.Show("Dice Bot has detected that there are no default settings saved on this computer."+
                    "If this is the first time you are running Dice Bot, it is highly recommended you see the begginners guide"+
                    "\n\nGo to Beginners Guide now?", "Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Process.Start("https://bot.seuntjie.com/GettingStarted.aspx");
                }
                try
                {

                    Directory.CreateDirectory(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2");
                }
                catch (Exception e)
                {
                    DumpLog(e.Message, 1);
                    DumpLog(e.StackTrace, 2);
                }
            }
            else
            {
                if (Emails == null)
                    Emails = new Email("", "");
                load();
                loadsettings();
                if (txtQuickSwitch.Text!="")
                    btnStratRefresh_Click(btnStratRefresh, new EventArgs());
            }
           
            tmStop.Enabled = true;
            
            Thread tGetVers = new Thread(new ThreadStart(getversion));
            tGetVers.Start();
            populateFiboNacci();
            
            if (autologin)
            {
                CurrentSite.FinishedLogin -= CurrentSite_FinishedLogin;
                CurrentSite.FinishedLogin += CurrentSite_FinishedLogin;
                CurrentSite.Login(username, password , txtApi2fa.Text);
                
            }
            Lua.RegisterFunction("withdraw",this, new dWithdraw(luaWithdraw).Method);
            Lua.RegisterFunction("invest", this, new dInvest(luainvest).Method);
            Lua.RegisterFunction("tip", this, new dtip(luatip).Method);
            Lua.RegisterFunction("stop", this, new dStop(luaStop).Method);
            Lua.RegisterFunction("resetseed", this, new dResetSeed(luaResetSeed).Method);
            Lua.RegisterFunction("print", this, new dWriteConsole(WriteConsole).Method);
            Lua.RegisterFunction("getHistory", this, new dluagethistory(luagethistory).Method);
            Lua.RegisterFunction("getHistoryByDate", this, new dluagethistory2(luagethistory).Method);
            Lua.RegisterFunction("getHistoryByQuery", this, new dQueryHistory(QueryHistory).Method);
            Lua.RegisterFunction("runsim",this, new dRunsim(runsim).Method);
            Lua.RegisterFunction("martingale", this, new dStrat(LuaMartingale).Method);
            Lua.RegisterFunction("labouchere", this, new dStrat(LuaLabouchere).Method);
            Lua.RegisterFunction("fibonacci", this, new dStrat(LuaFibonacci).Method);
            Lua.RegisterFunction("dalembert", this, new dStrat(LuaDAlember).Method);
            Lua.RegisterFunction("presetlist", this, new dStrat(LuaPreset).Method);
            Lua.RegisterFunction("resetstats", this, new dResetStats(resetstats).Method);
            Lua.RegisterFunction("setvalueint", this, new dSetValue(LuaSetValue).Method);
            Lua.RegisterFunction("setvaluestring", this, new dSetValue1(LuaSetValue).Method);
            Lua.RegisterFunction("setvaluedecimal", this, new dSetValue2(LuaSetValue).Method);
            Lua.RegisterFunction("setvaluebool", this, new dSetValue3(LuaSetValue).Method);
            Lua.RegisterFunction("getvalue", this, new dGetValue(LuaGetValue).Method);
            Lua.RegisterFunction("loadstrategy", this, new dLoadStrat(LuaLoadStrat).Method);
            Lua.RegisterFunction("read", this, new dGetInput(GetInputForLua).Method);
            Lua.RegisterFunction("readadv", this, new dGetInputWithParams(GetInputWithParams).Method);
            Lua.RegisterFunction("alarm", this, new dPlayAlarm(playalarm).Method);
            Lua.RegisterFunction("ching", this, new dPlayChing(PlayChing).Method);
            Lua.RegisterFunction("resetbuiltin", this, new dPlayChing(Reset).Method);
            DumpLog("constructor done", 8);
        }
        void luaStop()
        {
            Stop("Lua stop command issued");
        }

        delegate bool dLoadStrat(string File);
        bool LuaLoadStrat(string File)
        {
            return load(File, false);
        }
        delegate object dGetInput(string prompt, int type);
        /*
            0= bool
            1= int
            2= decimal
            3= string
        */
        bool WaitForInput = false;
        object GetInputForLua(string prompt, int type)
        {
            WaitForInput = true;
            DumpLog("getting user input for lua script", 7);
            UserInput tmp = new UserInput();
            DialogResult tmpRes = tmp.ShowDialog(prompt, type);
            WaitForInput = false;
            return tmp.Value;
        }
        delegate object dGetInputWithParams(string prompt,int type,string userinputext,string btncanceltext,string btnoktext);
        /*
            0= bool
            1= int
            2= decimal
            3= string
        */
        public object GetInputWithParams(string prompt, int type, string userinputext, string btncanceltext, string btnoktext)
        {
            WaitForInput = true;
            DumpLog("getting advanced user input for lua script", 7);
            UserInput tmp = new UserInput();
            DialogResult tmpRes = tmp.ShowDialog(prompt, type, userinputext, btncanceltext, btnoktext);
            WaitForInput = false;
            return tmp.Value;
        }
        delegate void dSetValue(string Name, int Value);
        void LuaSetValue(string Name, int Value)
        {
            SetValue(Name, Value, false);
        }
        delegate void dSetValue1(string Name, string Value);
        void LuaSetValue(string Name, string Value)
        {
            SetValue(Name, Value, false);
        }
        delegate void dSetValue2(string Name, decimal Value);
        void LuaSetValue(string Name, decimal Value)
        {
            SetValue(Name, Value, false);
        }
        delegate void dSetValue3(string Name, bool Value);
        void LuaSetValue(string Name, bool Value)
        {
            SetValue(Name, Value, false);
        }
        delegate object dGetValue(string Name);
        object LuaGetValue(string Name)
        {
            return getValue(Name, false);
        }

        delegate decimal dStrat(bool Win);
        decimal LuaMartingale(bool Win)
        {
            decimal tmpNext = Lastbet;
            martingale(Win);
            decimal tmpval = Lastbet;
            Lastbet = tmpNext;
            return tmpval;
        }

        decimal LuaFibonacci(bool Win)
        {
            decimal tmpNext = Lastbet;
            Fibonacci(Win);
            decimal tmpval = Lastbet;
            Lastbet = tmpNext;
            return tmpval;
        }

        decimal LuaLabouchere(bool Win)
        {
            decimal tmpNext = Lastbet;
            Labouchere(Win);
            decimal tmpval = Lastbet;
            Lastbet = tmpNext;
            return tmpval;
        }

        decimal LuaDAlember(bool Win)
        {
            decimal tmpNext = Lastbet;
            Alembert(Win);
            decimal tmpval = Lastbet;
            Lastbet = tmpNext;
            return tmpval;
        }

        decimal LuaPreset(bool Win)
        {
            decimal tmpNext = Lastbet;
            PresetList(Win);
            decimal tmpval = Lastbet;
            Lastbet = tmpNext;
            return tmpval;
        }



        delegate Bet[] dQueryHistory(string Query);
        Bet[] QueryHistory(string Query)
        {
            return sqlite_helper.GetHistoryByQuery(Query);
        }

        delegate void dResetStats();
       

        delegate void dRunsim(decimal startingabalance, int bets);
        void runsim(decimal startingbalance, int bets)
        {
            if (stop)
            {
                LuaRuntime.SetLua(Lua);
                GetLuaVars();
                LuaRuntime.Run(richTextBox3.Text);
                SimWindow.nudSimBalance.Value = (decimal)startingbalance;
                SimWindow.nudSimNumBets.Value = (decimal)bets;
               WriteConsole("Running " + bets + " bets Simulation with starting balance of " + startingbalance);
               btnSim_Click(SimWindow.btnSim, new EventArgs());
            }
            else
            {
                WriteConsole("Bot currently betting. Please stop betting before running simulation.");
            }
        }

        void luaResetSeed()
        {
            WriteConsole("Resetting Seed!");
            if (CurrentSite.ChangeSeed)
                CurrentSite.ResetSeed();
        }
        void luaWithdraw(decimal amount, string address)
        {
            WriteConsole("Withdrawing " +amount + " to " + address);
            if (CurrentSite.AutoWithdraw)
                CurrentSite.Withdraw(amount, address);
        }

        void luainvest(decimal amount)
        {
            WriteConsole("investing " + amount);
            if (CurrentSite.AutoInvest)
                CurrentSite.Invest(amount);
        }
        void luatip(string username, decimal amount)
        {
            WriteConsole("Tipping "+ amount + " to "+username);
            if (CurrentSite.Tip)
                CurrentSite.SendTip(username, amount);
        }

        delegate Bet[] dluagethistory();
        Bet[] luagethistory()
        {
            return sqlite_helper.GetBetHistory(CurrentSite.Name);
        }
        delegate Bet[] dluagethistory2(string From, string untill);
        Bet[] luagethistory(string From, string untill)
        {
            try
            {
                return sqlite_helper.GetBetHistory(CurrentSite.Name, DateTime.Parse(From, System.Globalization.DateTimeFormatInfo.InvariantInfo), DateTime.Parse(untill, System.Globalization.DateTimeFormatInfo.InvariantInfo));
            }
            catch (Exception e)
            {
                WriteConsole(e.Message);
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
                Stop("Lua exception");
                return null;
            }
        }
        void CurrentSite_FinishedLogin(bool LoggedIn)
        {
            if (InvokeRequired)
                Invoke(new DiceSite.dFinishedLogin(CurrentSite_FinishedLogin), LoggedIn);
            else
            {
                if (LoggedIn)
                {
                    txtApiPassword.Text = "";
                    EnableNotLoggedInControls(true);
                    MessageBox.Show("Successfully Logged in or registered.");
                    updateStatus("Logged in.");
                    if (autostart)
                    {
                        Start(false);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to log in or register new account!", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    updateStatus("Disconnected");
                }

            }
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
                        Process.Start("http://bot.seuntjie.com/botpage.aspx");
                    }
                }
                else
                {
                    if (startupMessage)
                    {
                        if (ss.Length>=3)
                        {
                            string Message = ss[2];
                            string Link = "";
                            if (ss.Length>=4)
                            {
                                Link = ss[3];
                            }
                            
                            
                            ShowStartup(Message, Link);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
            }
        }

        delegate void dShowStartup(string Message, string Link);
        void ShowStartup(string Message, string Link)
        {
            if (InvokeRequired)
            {
                Invoke(new dShowStartup(ShowStartup), Message, Link);
                return;

            }
            else
            {
                Startup tmp = new Startup();
                tmp.FormClosing += Tmp_FormClosing;
                tmp.Show(Message, Link);
            }
        }

        private void Tmp_FormClosing(object sender, FormClosingEventArgs e)
        {
            startupMessage = !(sender as Startup).chkDontShow.Checked;
            if (!startupMessage)
            {
                DiceBot.Settings tmpSet = new Settings(this);
                tmpSet.chkStartup.Checked = startupMessage;
                writesettings(tmpSet);
                tmpSet.Dispose();
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
                lblLosses2.Text = Losses.ToString();
                lblLosses2.Text = Losses.ToString();
                lblProfit2.Text = profit.ToString("0.00000000");
                if (Winstreak == 0)
                {
                    lblCustreak2.Text = Losestreak.ToString();
                    lblCustreak2.ForeColor =  Color.Red;
                }
                else
                {
                    lblCustreak2.Text =  Winstreak.ToString();
                    lblCustreak2.ForeColor =  Color.Green;

                }
                lblWins2.Text = Wins.ToString();
                if (StatsWindows!= null)
                {
                    if (!StatsWindows.IsDisposed)
                    {
                        StatsWindows.lblLoseStreak.Text = WorstStreak.ToString();
                        lblLosses2.Text = StatsWindows.lblLosses.Text = Losses.ToString();


                        lblProfit2.Text = StatsWindows.lblProfit.Text = profit.ToString("0.00000000");
                        StatsWindows.lblBalance.Text = PreviousBalance.ToString("0.00000000");
                        if (profit < 0)
                        {
                            StatsWindows.lblProfit.ForeColor = Color.Red;

                        }
                        else
                        {
                            StatsWindows.lblProfit.ForeColor = Color.Green;

                        }
                        if (profit > 0.001m)
                        {
                            donateToolStripMenuItem.ForeColor = Color.Green;
                            donateToolStripMenuItem.BackColor = Color.LightBlue;
                        }
                        if (Winstreak == 0)
                        {
                            lblCustreak2.Text = StatsWindows.lblCustreak.Text = Losestreak.ToString();
                            lblCustreak2.ForeColor = StatsWindows.lblCustreak.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblCustreak2.Text = StatsWindows.lblCustreak.Text = Winstreak.ToString();
                            lblCustreak2.ForeColor = StatsWindows.lblCustreak.ForeColor = Color.Green;

                        }

                        lblWins2.Text = StatsWindows.lblWins.Text = Wins.ToString();
                        StatsWindows.lblWinStreak.Text = BestStreak.ToString();
                        //
                        TimeSpan curtime = TimeSpan.Parse(StatsWindows.lblTime.Text);
                        //TimeSpan curtime = DateTime.Now - dtStarted;
                        lblBets2.Text = StatsWindows.lblBets.Text = (Wins + Losses).ToString();
                        decimal profpB = 0;
                        if (Wins + Losses > 0)
                            profpB = (decimal)profit / (decimal)(Wins + Losses);
                        decimal betsps = 0;

                        if (curtime.TotalSeconds > 0.0)
                            betsps = (decimal)(Wins + Losses) / (decimal)(curtime.TotalSeconds);
                        decimal profph = 0;
                        if (profpB > 0 && betsps > 0)
                            profph = (profpB * betsps) * 60.0m * 60.0m;
                        StatsWindows.lblProfpb.Text = profpB.ToString("0.00000000");
                        StatsWindows.lblProfitph.Text = (profph.ToString("0.00000000"));
                        StatsWindows.lblProfit24.Text = (profph* 24.0m).ToString("0.00000000");

                        int imaxbets = maxbets();
                        if (imaxbets == -500)
                            StatsWindows.lblMaxBets.Text = "500+";
                        else
                            StatsWindows.lblMaxBets.Text = imaxbets.ToString();
                        if (imaxbets > 20)
                        {
                            StatsWindows.lblMaxBets.ForeColor = Color.Blue;
                        }
                        else if (imaxbets > 15)
                        {
                            StatsWindows.lblMaxBets.ForeColor = Color.Green;
                        }
                        else if (imaxbets > 10)
                        {
                            StatsWindows.lblMaxBets.ForeColor = Color.DarkOrange;
                        }
                        else
                        {
                            StatsWindows.lblMaxBets.ForeColor = Color.Red;
                        }
                        StatsWindows.lblAvgWinStreak.Text = avgwin.ToString("0.000000");
                        StatsWindows.lblAvgLoseStreak.Text = avgloss.ToString("0.000000");
                        StatsWindows.lblAvgStreak.Text = avgstreak.ToString("0.000000");
                        if (avgstreak > 0)
                            StatsWindows.lblAvgStreak.ForeColor = Color.Green;
                        else StatsWindows.lblAvgStreak.ForeColor = Color.Red;
                        StatsWindows.lbl3Best.Text = BestStreak.ToString() + "\n" + BestStreak2.ToString() + "\n" + BestStreak3.ToString();
                        StatsWindows.lbl3Worst.Text = WorstStreak.ToString() + "\n" + WorstStreak2.ToString() + "\n" + WorstStreak3.ToString();
                        StatsWindows.lblLastStreakLose.Text = laststreaklose.ToString();
                        StatsWindows.lblLastStreakWin.Text = laststreakwin.ToString();
                        StatsWindows.lblLargestBet.Text = LargestBet.ToString("0.00000000");
                        StatsWindows.lblLargestLoss.Text = LargestLoss.ToString("0.00000000");
                        StatsWindows.lblLargestWin.Text = LargestWin.ToString("0.00000000");
                        if (Losses != 0)
                        {
                            StatsWindows.lblLuck.Text = luck.ToString("00.00") + "%";
                        }
                    }
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
            decimal total = 0;
            int bets = 0;
            decimal curbet = MinBet;
            
            decimal Multiplier = (decimal)(nudMultiplier.Value);

            while (total < PreviousBalance)
            {
                if (bets > 0)
                {
                    curbet *= Multiplier;
                }
                if (bets == nudChangeLoseStreak.Value && chkChangeLoseStreak.Checked)
                {
                    curbet = (decimal)nudChangeLoseStreakTo.Value;
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
            decimal total = 0;
            int bets = 0;
            decimal curbet = MinBet;
            int n = Devidecounter;
            decimal dmultiplier = (decimal)(nudMultiplier.Value);
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
                    curbet = (decimal)nudChangeLoseStreakTo.Value;
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
            decimal total = 0;
            int bets = 0;
            decimal curbet = MinBet;
            int n = Devidecounter;
            decimal dmultiplier = (decimal)(nudMultiplier.Value);
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
                    curbet = (decimal)nudChangeLoseStreakTo.Value;
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
            decimal total = 0;
            int bets = 0;
            decimal curbet = MinBet;
            int n = Devidecounter;
            decimal dmultiplier = (decimal)(nudMultiplier.Value);
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
                    curbet = (decimal)nudChangeLoseStreakTo.Value;
                }
                bets++;
                total += curbet;
                if (bets > 500)
                    return -500;
            }
            return bets;
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
            decimal tmp = (decimal)(lucktotal / (decimal)(Wins + Losses));
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

        private void Stop(string Reason)
        {
            updateStatus(Reason+", stopping");
            TrayIcon.BalloonTipText = Reason + ", stopping";
            TrayIcon.ShowBalloonTip(1000);
            //tmBetting.Enabled = false;
            WriteConsole("Betting Stopped!");
            decimal dBalance = CurrentSite.balance;
            stop = true;
            TotalTime += (DateTime.Now - dtStarted);
            if (RunningSimulation)
            {
                WriteConsole(string.Format("Simulation finished. Bets:{0} Wins:{1} Losses:{2} Balance:{3} Profit:{4} Worst Streak:{5} Best Streak:{6}", 
                    Losses+Wins, Wins, Losses, PreviousBalance, profit, WorstStreak, BestStreak ));
                Updatetext(SimWindow.lblSimLosses, Losses.ToString());
                Updatetext(SimWindow.lblSimProfit, profit.ToString("0.00000000"));
                Updatetext(SimWindow.lblSimWins, Wins.ToString());
                Updatetext(SimWindow.lblSimEndBalance, PreviousBalance.ToString("0.00000000"));
                Updatetext(SimWindow.lblSimLoseStreak, WorstStreak.ToString());
                Updatetext(SimWindow.lblSimWinStreak, BestStreak.ToString());
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
                profit = tmpprofit;
            }
        }

        bool ResetBet = false;
      private void Reset()
        {
            
                reset = true;
                if (rdbMartingale.Checked)
                {
                    Lastbet = MinBet;
                }
                else if (rdbLabEnable.Checked)
                {
                    string[] ss = GetLabList();
                    LabList = new List<decimal>();
                    foreach (string s in ss)
                    {
                        decimal tmpval = dparse(s, ref convert);
                        if (convert)
                            LabList.Add(tmpval);
                        else
                        {
                            MessageBox.Show("Could not parse number: " + s + ". Please remove it from the list. (This could be an empty newline character)");
                        }
                    }
                    if (LabList.Count == 1)
                        Lastbet = LabList[0];
                    else if (LabList.Count > 1)
                        Lastbet = LabList[0] + LabList[LabList.Count - 1];
                }
                else if (rdbFibonacci.Checked)
                {
                    FibonacciLevel = 0;
                    Lastbet = decimal.Parse(lstFibonacci.Items[FibonacciLevel].ToString().Substring(lstFibonacci.Items[FibonacciLevel].ToString().IndexOf(" ") + 1));
                }
                else if (rdbAlembert.Checked)
                {
                    Lastbet = MinBet;
                }
                else if (rdbPreset.Checked)
                {
                    presetLevel = 0;
                    decimal Betval = -1;
                    if (presetLevel < rtbPresetList.Lines.Length)
                    {
                        SetPresetValues(presetLevel);
                    }
                }
            
        }
        
        void PlaceBet()
        {
            try
            {
                
                CurrentSite.amount=(Lastbet);
                
                if (!CurrentSite.ReadyToBet())
                    return;

                CurrentSite.chance = Chance;
                CurrentSite.PlaceBet(high,Lastbet, Chance);
                    
                dtLastBet = DateTime.Now;
                EnableTimer(tmBet, false);
                
            }
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
            }
        }

        delegate void dPlayChing();
        void PlayChing()
        {
            try
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
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
                MessageBox.Show("Failed to play CHING, pelase make sure file exists");
            }
        }

        void Withdraw()
        {
            
            if (CurrentSite.AutoWithdraw)
                if (CurrentSite.Withdraw((decimal)(nudAmount.Value), txtTo.Text))
                {

                    
                    TrayIcon.BalloonTipText = "Withdraw " + nudAmount.Value + " Complete\nRestarting Bets";
                    TrayIcon.ShowBalloonTip(1000);
                    try
                    {
                        if (Sound && SoundWithdraw)
                        {
                            PlayChing();
                        }
                    }
                    catch (Exception e)
                    {
                        DumpLog(e.Message, 1);
                        DumpLog(e.StackTrace, 2);
                        MessageBox.Show("Failed to play CHING, pelase make sure file exists");
                    }
                    
                    Emails.SendWithdraw(Amount, PreviousBalance - Amount, txtTo.Text);
                    StartBalance -= Amount;
                    //Start(true);
                }
        }

        void Invest()
        {

            if (CurrentSite.AutoInvest)
            {
                if (CurrentSite.Invest((decimal)(nudAmount.Value)))
                {
                    //invest = false;
                    TrayIcon.BalloonTipText = "Invest " + nudAmount.Value + "Complete\nRestarting Bets";
                    TrayIcon.ShowBalloonTip(1000);
                    try
                    {
                        if (Sound && SoundWithdraw)
                        {
                            PlayChing();
                        }
                    }
                    catch (Exception e)
                    {
                        DumpLog(e.Message, 1);
                        DumpLog(e.StackTrace, 2);
                        MessageBox.Show("Failed to play CHING, pelase make sure file exists");
                    }
                    
                    Emails.SendInvest(Amount, CurrentSite.balance, dparse("-0", ref convert));
                    StartBalance -= Amount;
                    
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
                if (!programmerToolStripMenuItem.Checked)
                Chance = (decimal)nudChance.Value;
                CurrentSite.chance =(Chance);

                dtStarted = DateTime.Now;
            }
            if (testInputs())
            {
                if (!programmerToolStripMenuItem.Checked)
                    Reset();
                reset = false;
                stop = false;
                if (rdbLabEnable.Checked)
                {
                    LabList = new List<decimal>();
                    string[] lines = GetLabList();
                    foreach (string s in lines)
                    {
                        decimal tmpval = dparse(s, ref convert);
                        if (convert)
                            LabList.Add(tmpval);
                        else
                        {
                            MessageBox.Show("Could not parse number: " + s + ". Please remove it from the list. (This could be an empty newline character)");Stop("Invalid bet in Labouchere list");return;
                        }
                    }
                }
                if (!Continue)
                {
                    if (!programmerToolStripMenuItem.Checked)
                    {
                        Lastbet = MinBet;
                        if (!programmerToolStripMenuItem.Checked)
                        Chance = (decimal)nudChance.Value;
                    }
                    else
                    {
                        if (Lastbet <0)
                        {
                            WriteConsole("Please set starting bet using nextbet = x.xxxxxxxx");
                        }
                        if (Chance==0)
                        {

                            WriteConsole("Please set starting chance using chance = yy.yyyy");
                        }
                    }
                    if (rdbLabEnable.Checked)
                    {
                        if (LabList.Count > 0)
                        {
                            if (LabList.Count == 1)
                                Lastbet = LabList[0];
                            else
                                Lastbet = LabList[0] + LabList[LabList.Count - 1];
                        }
                        else
                        {
                            MessageBox.Show("Please enter a list of bets into the bet box on the labouchere tab, 1 bet per line.");
                            Stop("No bets in labouchere bet list");
                            return;
                        }
                    }
                    if (nudMutawaMultiplier.Value != 0)
                    {
                        mutawaprev = (decimal)nudChangeWinStreakTo.Value / (decimal)nudMutawaMultiplier.Value;
                    }
                }
                if (RunningSimulation)
                {
                    setInterval(tmBet, 1);
                    Simbet();
                }
                else
                {
                    setInterval(tmBet, 10);
                    EnableTimer(tmBet, true);
                }
            }
        }


        private void tmBetting_Tick(object sender, EventArgs e)
        {
            
            if (!RunningSimulation)
            {
                decimal dBalance = PreviousBalance;
                if (CurrentSite != null)
                dBalance = CurrentSite.balance;
                if ((dBalance != PreviousBalance && convert || withdrew) && dBalance > 0)
                {
                    if (PreviousBalance == 0)
                        StartBalance = dBalance;
                    PreviousBalance = dBalance;

                }
                else if (dBalance == PreviousBalance && convert || withdrew)
                {
                    if ((DateTime.Now - dtLastBet).TotalSeconds > 30 && !stop)
                    {
                        if (!retriedbet && !WaitForInput)
                        {
                            retriedbet = true;
                            EnableTimer(tmBet, true);

                        }
                    }
                    if ((DateTime.Now - dtLastBet).TotalSeconds > 120 && !stop && !WaitForInput)
                    {

                        
                            dtLastBet = DateTime.Now;
                            restartcounter = 0;
                        

                    }

                    if (restartcounter > 50 && restartcounter < 51 && !stop && !WaitForInput)
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
                /*if (reset)
                {
                    ResetSeed();
                }*/


            }
        }

        decimal mutawaprev = 0;
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
                            {
                                Stop("End of labouchere list reached");
                                
                            }
                            else
                            {
                                Reset();
                            }
                        }

                    }
                    else
                    {
                        if (rdbLabStop.Checked)
                        {
                            Stop("End of labouchere list reached");
                            
                        }
                        else
                        {
                            string[] ss = GetLabList();
                            LabList = new List<decimal>();
                            foreach (string s in ss)
                            {
                                decimal tmpval = dparse(s, ref convert);
                                if (convert)
                                    LabList.Add(tmpval);
                                else
                                {
                                    MessageBox.Show("Could not parse number: " + s + ". Please remove it from the list. (This could be an empty newline character)"); Stop("Invalid bet in Labouchere list"); return;
                                }
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
                            
                        }
                        else
                        {
                            if (rdbLabStop.Checked)
                            {
                                Stop("Stopping: End of labouchere list reached.");
                                
                            }
                            else
                            {
                                string[] ss = GetLabList();
                                LabList = new List<decimal>();
                                foreach (string s in ss)
                                {
                                    decimal tmpval = dparse(s, ref convert);
                                    if (convert)
                                        LabList.Add(tmpval);
                                    else
                                    {
                                        MessageBox.Show("Could not parse number: " + s + ". Please remove it from the list. (This could be an empty newline character)"); Stop("Invalid bet in Labouchere list"); return;
                                    }
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
                    {
                        Stop("Stopping: End of labouchere list reached.");
                        
                    }
                    else
                    {
                        string[] ss = GetLabList();
                        LabList = new List<decimal>();
                        foreach (string s in ss)
                        {
                            decimal tmpval = dparse(s, ref convert);
                            if (convert)
                                LabList.Add(tmpval);
                            else
                            {
                                MessageBox.Show("Could not parse number: " + s + ". Please remove it from the list. (This could be an empty newline character)"); Stop("Invalid bet in Labouchere list"); return;
                            }
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
                if (Winstreak% (int)nudStretchWin.Value == 0)
                    Lastbet *= WinMultiplier;
                if (Winstreak == 1)
                {
                    if(chkFirstResetWin.Checked && !chkMK.Checked)
                    {
                        Lastbet = MinBet;
                    }
                    try
                    {
                        Chance = (decimal)(nudChance.Value);
                        if (!RunningSimulation)
                            CurrentSite.chance =(Chance);
                    }
                    catch (Exception e)
                    {
                        DumpLog(e.Message, 1);
                        DumpLog(e.StackTrace, 2);
                    }
                }
                if (chkTrazel.Checked)
                {
                    high = starthigh;
                }
                if (chkMK.Checked)
                {
                    if (decimal.Parse((Lastbet - (decimal)nudMKDecrement.Value).ToString("0.00000000"), System.Globalization.CultureInfo.InvariantCulture) > 0)
                    {
                        Lastbet -= (decimal)nudMKDecrement.Value;
                    }
                }
                if (chkTrazel.Checked && trazelwin % (decimal)nudTrazelWin.Value == 0 && trazelwin != 0)
                {
                    Lastbet = (decimal)nudtrazelwinto.Value;
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

                
                if (chkChangeWinStreak.Checked && (Winstreak == nudChangeWinStreak.Value))
                {
                    Lastbet = (decimal)nudChangeWinStreakTo.Value;
                }
                if (checkBox1.Checked)
                {
                    if (Winstreak == nudMutawaWins.Value)
                        Lastbet = mutawaprev *= (decimal)nudMutawaMultiplier.Value;
                    if (Winstreak == nudMutawaWins.Value + 1)
                    {
                        Lastbet = MinBet;
                        mutawaprev = (decimal)nudChangeWinStreakTo.Value / (decimal)nudMutawaMultiplier.Value;
                    }

                }
                if (chkChangeChanceWin.Checked && (Winstreak == nudChangeChanceWinStreak.Value))
                {
                    try
                    {
                        Chance = (decimal)nudChangeChanceWinTo.Value;
                        if (!RunningSimulation)
                            CurrentSite.chance = ((decimal)nudChangeChanceWinTo.Value);

                    }
                    catch (Exception e)
                    {
                        DumpLog(e.Message, 1);
                        DumpLog(e.StackTrace, 2);
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
                    Multiplier = (decimal)nudTrazelMultiplier.Value;
                }
                if (chkTrazel.Checked)
                {
                    high = starthigh;
                }
                if (chkTrazel.Checked && Losestreak + 1 >= (decimal)NudTrazelLose.Value && !trazelmultiply)
                {
                    Lastbet = (decimal)nudtrazelloseto.Value;
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
                if (Losestreak % (int)nudStretchLoss.Value == 0)
                    Lastbet *= Multiplier;
                if (Losestreak == 1)
                {
                    if (chkFirstResetLoss.Checked)
                    {
                        Lastbet = MinBet;
                    }
                }
                if (chkMK.Checked)
                {
                    Lastbet += (decimal)nudMKIncrement.Value;
                }
                if (checkBox1.Checked)
                {
                    Lastbet = MinBet;
                }

               
                //change bet after a certain losing streak
                if (chkChangeLoseStreak.Checked && (Losestreak == nudChangeLoseStreak.Value))
                {
                    Lastbet = (decimal)nudChangeLoseStreakTo.Value;
                }

                //change chance after a certain losing streak
                if (chkChangeChanceLose.Checked && (Losestreak == nudChangeChanceLoseStreak.Value))
                {
                    try
                    {
                        Chance = (decimal)nudChangeChanceLoseTo.Value;
                        if (!RunningSimulation)
                            CurrentSite.chance = (decimal)(nudChangeChanceLoseTo.Value);


                    }
                    catch (Exception e)
                    {
                        DumpLog(e.Message, 1);
                        DumpLog(e.StackTrace, 2);
                    }
                }
            }
            if (chkPercentage.Checked)
            {
                Lastbet = (decimal)(nudPercentage.Value / (decimal)100.0) * dPreviousBalance;
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
                    Stop("Fibonacci bet won.");
                    
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
                    Stop("Fibonacci bet lost.");
                    
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
                    Stop("Fibonacci level " + (int)nudFiboLeve.Value + ".");
                    
                }
            }
            Lastbet = decimal.Parse(lstFibonacci.Items[FibonacciLevel].ToString().Substring(lstFibonacci.Items[FibonacciLevel].ToString().IndexOf(" ")+1));
        }

        void Alembert(bool Win)
        {
            if (Win)
            {
                
                if ((Winstreak) % (nudAlembertStretchWin.Value +1) == 0)
                {
                    Lastbet += (decimal)nudAlembertIncrementWin.Value;
                }
            }
            else
            {
                if ((Losestreak) % (nudAlembertStretchLoss.Value + 1) == 0)
                {
                    Lastbet += (decimal)nudAlembertIncrementLoss.Value;
                }
            }
            if (Lastbet < MinBet)
                Lastbet = MinBet;
        }

        int presetLevel = 0;
        void SetPresetValues(int Level)
        {
            decimal Betval = -1;
            string[] Vars = null;
            if (rtbPresetList.Lines[Level].Contains("-"))
            {
                Vars = rtbPresetList.Lines[Level].Split('-');
            }
            else if (rtbPresetList.Lines[Level].Contains("/"))
            {
                Vars = rtbPresetList.Lines[Level].Split('/');
            }
            else if (rtbPresetList.Lines[Level].Contains("\\"))
            {
                Vars = rtbPresetList.Lines[Level].Split('\\');
            }
            else 
            {
                Vars = rtbPresetList.Lines[Level].Split('&');
            }

            if (decimal.TryParse(Vars[0], out Betval))
            {
                Lastbet = Betval;

                if (Vars.Length >= 2)
                {
                    decimal chance = -1;
                    if (decimal.TryParse(Vars[1], out chance))
                    {
                        Chance = chance;
                    }
                    else
                    {
                        if (Vars[1].ToLower() == "low" || Vars[1].ToLower() == "lo")
                            high = false;
                        else if (Vars[1].ToLower() == "high" || Vars[1].ToLower() == "hi")
                        {
                            high = true;
                        }
                    }
                    if (Vars.Length >= 3)
                    {
                        if (decimal.TryParse(Vars[2], out chance))
                        {
                            Chance = chance;
                        }
                        else
                        {
                            if (Vars[2].ToLower() == "low" || Vars[2].ToLower() == "lo")
                                high = false;
                            else if (Vars[2].ToLower() == "high" || Vars[2].ToLower() == "hi")
                            {
                                high = true;
                            }
                        }
                    }
                }
            }
           
            else
            {
                Stop("Invalid bet inpreset list");
            }
        }
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
                    Stop("Preset List bet won.");
                    
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
                    Stop("Preset List bet lost.");
                    
                }
            }
            if (presetLevel < 0)
                presetLevel = 0;
            if (presetLevel > rtbPresetList.Lines.Length-1)
            {
                if (rdbPresetEndStop.Checked)
                {
                    Stop("End of preset list reached.");
                    
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
            
            if (presetLevel < rtbPresetList.Lines.Length)
            {
                SetPresetValues(presetLevel);
            }
        }

        decimal ProfitSinceLastReset = 0;
        decimal StreakProfitSinceLastReset = 0;
        decimal StreakLossSinceLastReset = 0;
        int betsAtLastReset = 0;
        int lossesAtLastReset = 0;
        int winsAtLastReset = 0;
        bool EnableReset = false;
        bool EnableProgZigZag = false;
        public void DoBet(Bet bet)
        {
            bool Win = (((bool)bet.high ? (decimal)bet.Roll> (decimal)CurrentSite.maxRoll - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
            if (!Win)
            {

            }
            decimal profit = (decimal)bet.Profit;
            retriedbet = false;
            if (!stop)
            {
                if (Win)
                {
                    if (LargestWin < profit)
                        LargestWin = profit;
                }
                else
                {
                    if (LargestLoss < -profit)
                        LargestLoss = -profit;
                }

                if (LargestBet < Lastbet)
                    LargestBet = Lastbet;

                //if its a win
                if (Win)
                {

                    if (PreviousBalance != 0)
                    {
                       
                        if (Winstreak == 0)
                        {
                            currentprofit = 0;
                            StreakProfitSinceLastReset = 0;
                            StreakLossSinceLastReset = 0;
                        }                        
                        
                        currentprofit += profit;
                        ProfitSinceLastReset += profit;
                        StreakProfitSinceLastReset += profit;
                        
                        
                        Wins++;
                        Winstreak++;
                        trazelwin++;
                        CalculateLuck(true);

                        if (StatsWindows != null)
                            if (!StatsWindows.IsDisposed)
                            {
                                if (Winstreak >= StatsWindows.nudLastStreakWin.Value)
                                    laststreakwin = Winstreak;
                            }
                        
                        if (!programmerToolStripMenuItem.Checked || EnableReset)
                        {
                            if (chkResetBetWins.Checked && Winstreak % nudResetWins.Value == 0)
                            {
                                Reset();
                            }
                            if (currentprofit >= ((decimal)nudStopWinBtcStreak.Value) && chkStopWinBtcStreak.Checked)
                            {
                                Stop("Made " + currentprofit + " profit in a row");
                                
                            }
                            if (Winstreak >= nudStopWinStreak.Value && chkStopWinStreak.Checked)
                            {
                                Stop("Won "+ Winstreak + " bets in a row");
                                
                            }
                            if (this.profit >= (decimal)nudStopWinBtc.Value && chkStopWinBtc.Checked)
                            {
                                Stop("Made " + this.profit + " profit");
                                
                            }
                            if (StreakProfitSinceLastReset >= (decimal)nudResetBtcStreakProfit.Value && chkResetBtcStreakProfit.Checked)
                            {
                                Reset();
                                StreakProfitSinceLastReset = 0;
                            }
                            if (ProfitSinceLastReset> (decimal)nudResetBtcProfit.Value && chkResetBtcProfit.Checked)
                            {
                                Reset();
                                ProfitSinceLastReset = 0;
                            }
                            if (Wins >= nudStopWins.Value && chkStopWins.Checked)
                            {
                                Stop("More than " + nudStopLosses.Value + " Wins (" + Wins + "). Reset stats before restarting bot");
                            }
                            if (Wins - winsAtLastReset >= nudResetWins2.Value && chkResetWins.Checked)
                            {
                                Reset();
                                winsAtLastReset = Wins;
                            }
                        }
                        if (Losestreak != 0)
                        {
                            decimal avglosecalc = avgloss * numlosesreaks;
                            avglosecalc += Losestreak;
                            avglosecalc /= ++numlosesreaks;
                            avgloss = avglosecalc;
                            decimal avgbetcalc = avgstreak * numstreaks;
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
                        Stop("Stop on win clicked, bet won");
                        
                    }
                    iMultiplyCounter = 0;                    
                    Multiplier = (decimal)(nudMultiplier.Value);
                    if ((!programmerToolStripMenuItem.Checked) || EnableProgZigZag)
                    {
                        if (chkZigZagWins.Checked && Wins% (int)nudZigZagWins.Value==0 && Wins!=0)
                        {
                             
                                high = !high;
                            
                        }
                        if (chkZigZagWinsStreak.Checked && Winstreak % (int)nudZigZagWinsStreak.Value ==0 && Winstreak!=0)
                        {
                            high = !high;
                        }
                    }
                }

                    //if its a loss
                else if (!Win)
                {
                    
                    //do i use this line?
                    iMultiplyCounter++;

                    //reset current profit when switching from a winning streak to a losing streak
                    if (Losestreak == 0)
                    {
                        currentprofit = 0;
                        StreakProfitSinceLastReset = 0;
                        StreakLossSinceLastReset = 0;
                    }
                    
                    //adjust profit
                    currentprofit -= Lastbet;
                    ProfitSinceLastReset -= Lastbet;
                    
                    StreakLossSinceLastReset -= Lastbet;
                    //increase losses and losestreak
                    Losses++;
                    Losestreak++;
                    
                    CalculateLuck(false);
                    
                    //update last losing streak if it is above the specified value to show in the stats
                    if (StatsWindows != null)
                        if (!StatsWindows.IsDisposed)
                            if (Losestreak >= StatsWindows.nudLastStreakLose.Value)
                                laststreaklose = Losestreak;

                    //switch high low if applied in the zig zag tab
                    if ((!programmerToolStripMenuItem.Checked) || EnableProgZigZag)
                    {
                        if (chkZigZagLoss.Checked && Losses % (int)nudZigZagLoss.Value == 0 && Losses != 0)
                        {

                            high = !high;

                        }
                        if (chkZigZagLossStreak.Checked && Losestreak % (int)nudZigZagLossStreak.Value == 0 && Losestreak != 0)
                        {
                            high = !high;
                        }
                    }

                   

                    if (!programmerToolStripMenuItem.Checked || EnableReset)
                    {
                        if (chkResetBetLoss.Checked && Losestreak %nudResetBetLoss.Value == 0)
                        {
                            Reset();
                        }
                        //stop conditions:
                        //stop if lose streak is higher than specified
                        if (Losestreak >= nudStopLossStreak.Value && chkStopLossStreak.Checked)
                        {
                            Stop("Lost " +Losestreak + "bets in a row");
                        }

                        //stop if current profit drops below specified value/ loss is larger than specified value
                        if (currentprofit <= (0.0m - (decimal)nudStopLossBtcStreal.Value) && chkStopLossBtcStreak.Checked)
                        {
                            Stop("Lost " + currentprofit+ " " + CurrentSite.Currency+" in a row");
                        }

                        // stop if total profit/total loss is below/above certain value
                        if (this.profit <= 0.0m - (decimal)nudStopLossBtc.Value && chkStopLossBtc.Checked)
                        {
                            Stop("Lost " + this.profit + " " + CurrentSite.Currency);
                        }
                        if (StreakLossSinceLastReset <= -(decimal)nudResetBtcStreakLoss.Value && chkResetBtcStreakLoss.Checked)
                        {
                            Reset();
                            StreakLossSinceLastReset = 0;
                        }
                        if (ProfitSinceLastReset < -(decimal)nudResetBtcLoss.Value && chkResetBtcLoss.Checked)
                        {
                            Reset();
                            ProfitSinceLastReset = 0;
                        }
                        if (Losses>= nudStopLosses.Value && chkStopLosses.Checked)
                        {
                            Stop("More than " + nudStopLosses.Value + " losses ("+Losses+"). Reset stats before restarting bot");
                        }
                        if (Losses-lossesAtLastReset >= nudResetLosses.Value && chkResetLosses.Checked)
                        {
                            Reset();
                            lossesAtLastReset = Losses;
                        }
                    }
                    //when switching from win streak to lose streak, calculate some stats
                    if (Winstreak != 0)
                    {
                        decimal avgwincalc = avgwin * numwinstreasks;
                        avgwincalc += Winstreak;
                        avgwincalc /= ++numwinstreasks;
                        avgwin = avgwincalc;
                        decimal avgbetcalc = avgstreak * numstreaks;
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
                    if (!RunningSimulation)
                    if (Sound && SoundStreak && Losestreak > SoundStreakCount)
                        playalarm();
                    //email
                    if (!RunningSimulation && Emails!=null)
                    if (Emails.Streak && Losestreak > Emails.StreakSize)
                        Emails.SendStreak(Losestreak, Emails.StreakSize, dPreviousBalance);

                    
                    //update worst streaks
                    /*if (!RunningSimulation)
                    if (Losestreak > WorstStreak)
                        WorstStreak = Losestreak;*/

                    //reset win multplier
                    WinMultiplier = (decimal)(nudWinMultiplier.Value);

                }

                if (Wins + Losses >= nudStopBets.Value && chkStopBets.Checked)
                {
                    Stop("Bets exeeding " + nudStopBets.Value +"(stop after x bets)");
                }
                if ( Wins+Losses-betsAtLastReset >= nudResetBets.Value && chkResetBets.Checked)
                {
                    Reset();
                    betsAtLastReset = Wins + Losses;
                }
                TimeSpan curtime = DateTime.Now - dtStarted;

                if ((decimal)curtime.TotalHours >= nudStopTimeH.Value && curtime.Minutes >= (decimal)nudStopTimeM.Value && curtime.Seconds >= (decimal)nudStopTimeS.Value && chkStopTime.Checked)
                {
                    Stop(string.Format("Time exeeding {0}:{1}:{2}", nudStopTimeH.Value, nudStopTimeM.Value, nudStopTimeS.Value));
                }
                
                if (chkZigZagBets.Checked && (!programmerToolStripMenuItem.Checked || EnableProgZigZag ))
                {
                    if ((Wins+Losses) % (int)nudZigZagBets.Value == 0 && (Wins+Losses)!=0 )
                    {
                        high = !high;
                    }
                }
                if (!RunningSimulation)
                if (dPreviousBalance >= Limit && chkLimit.Checked && (!programmerToolStripMenuItem.Checked))
                {

                    if (rdbStop.Checked)
                    {
                        Stop("Balance larger than "+Limit+" (limit)");
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
                if (dPreviousBalance - Lastbet <= LowerLimit && chkLowerLimit.Checked &&(!programmerToolStripMenuItem.Checked))
                {
                    //TrayIcon.BalloonTipText = "Balance lower than " + nudLowerLimit.Value + "\nStopping Bets...";
                    TrayIcon.ShowBalloonTip(1000);
                    Stop("Balance lower than " + nudLowerLimit.Value);
                    if (Sound && SoundLow)
                        playalarm();
                    TrayIcon.BalloonTipText = "DiceBot has Stopped Betting\nThe next bet will will have put your Balance below your lower limit";

                    if (Emails.Lower)
                        Emails.SendLowLimit(dPreviousBalance, LowerLimit, Lastbet);
                }


                if (!RunningSimulation)
                if ( Wins!=0 && Losses!=0 && chkResetSeed.Checked && (!programmerToolStripMenuItem.Checked))
                {
                    if ( ((rdbResetSeedBets.Checked && (Wins+Losses) % nudResetSeed.Value == 0) ||
                       (rdbResetSeedWins.Checked && Wins % nudResetSeed.Value == 0 && Losestreak==0)||
                       (rdbResetSeedLosses.Checked && Losses % nudResetSeed.Value == 0 && Winstreak == 0)) && !withdrew)
                    {
                        
                   
                        ResetSeed();
                    }
                    
                }

                try
                {
                   if (!RunningSimulation)
                    UpdateStats();
                }
                catch (Exception e)
                {
                    DumpLog(e.Message, 1);
                    DumpLog(e.StackTrace, 2);
                }
                if (RunningSimulation && (Wins + Losses > numSimBets || Lastbet > PreviousBalance))
                {
                    Stop("Simulation complete");
                }
                
                if (!(stop || withdraw ||invest))
                {
                    if (programmerToolStripMenuItem.Checked)
                    {
                        parseScript(bet);
                    }
                    else if (!reset)
                    {
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
                    }
                    if (chkMinBet.Checked && (!programmerToolStripMenuItem.Checked || EnableReset) && Lastbet< (decimal)nudMinumumBet.Value)
                    {
                        Lastbet = (decimal)nudMinumumBet.Value;

                    }
                    if (chkMaxBet.Checked && (!programmerToolStripMenuItem.Checked || EnableReset) && Lastbet > (decimal)nudMaximumBet.Value)
                    {
                        Lastbet = (decimal)nudMaximumBet.Value;
                    }
                    if (RunningSimulation && Lastbet > dPreviousBalance)
                    {
                        Stop("Simulation complete");
                    }
                    if (!stop)
                    {
                        if (!RunningSimulation)
                        WriteConsole("Betting " + Lastbet + " at " + Chance +"% chance to win, "+ (high?"high":"low"));
                        EnableTimer(tmBet, true);

                        withdrew = false;
                    }
                }


            }
            if (RunningSimulation && stop)
            {
                RunningSimulation = false;
            }
            reset = false;
        }

        System.Collections.ArrayList Vars = new System.Collections.ArrayList();
        private void parseScript(Bet bet)
        {

            try
            {
                bool Win = (((bool)bet.high ? (decimal)bet.Roll> (decimal)CurrentSite.maxRoll - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
            
                SetLuaVars();
                Lua["win"] = Win;
                Lua["currentprofit"] = ((decimal)(bet.Profit * 100000000m)) / 100000000.0m;
                Lua["lastBet"] = bet;
                LuaRuntime.SetLua(Lua);
                LuaRuntime.Run("dobet()");
                GetLuaVars();
            }
            catch (LuaException e)
            {
                Stop("Lua error!");
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
                WriteConsole(e.Message);
            }
            catch (Exception e)
            {
                Stop("An error has occurred parsing the lua script.");
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
                WriteConsole(e.Message);
            }
                        
        }

        
        void WriteConsole(string Message)
        {

            if (InvokeRequired)
            {
                Invoke(new dWriteConsole(WriteConsole), Message);
                return;
            }
            else
            {
                if (rtbConsole.Lines.Length > 1000)
                {
                    List<string> lines = new List<string>(rtbConsole.Lines);
                    while (lines.Count > 950)
                    {
                        lines.RemoveAt(0);
                    }
                    rtbConsole.Lines = lines.ToArray();
                }
                rtbConsole.AppendText(Message + "\r\n");
            }
        }
        delegate void dWriteConsole(string Message);
        delegate void dWithdraw(decimal Amount, string Address);
        delegate void dInvest(decimal Amount);
        delegate void dtip(string username, decimal amount);
        delegate void dStop();
        delegate void dResetSeed();
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
                bool valid = true;
                if (chkBotSpeed.Checked)
                {
                    if ((DateAndTime.Now - dtLastBet).Ticks < new TimeSpan(0, 0, 0, 0, (int)((decimal)1000.0 / nudBotSpeed.Value)).Ticks)
                    {
                        valid = false;
                    }
                }

                if (RunningSimulation)
                {
                    Simbet();
                }
                else
                if (CurrentSite.ReadyToBet() && valid)
                {
                    if (!stop)
                        PlaceBet();
                    EnableTimer(tmBet, false);
                }
               
            }
            catch (Exception ex)
            {
                DumpLog(ex.Message, 1);
                DumpLog(ex.StackTrace, 2);
            }

        }

        delegate void dPlayAlarm();
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
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
                MessageBox.Show("Failed to play Alarm, pelase make sure file exists");
            }
        }

        private void tmStop_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                Stop("Emergency stop keys detected");
            }
           
            if (!stop && timecounter > 10)
            {
                if (StatsWindows!=null)
                {
                    if (!StatsWindows.IsDisposed)
                    {
                        StatsWindows.lblTime.Text = (TotalTime + (DateTime.Now - dtStarted)).ToString(@"hh\:mm\:ss");
                    }
                }
                
                timecounter = 0;
            }
            timecounter++;
            /*if (autostart && !running)
            {
                Start(false);
                running = true;
            }*/
        }

        #endregion
        

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((CurrentSite.AutoWithdraw || CurrentSite.Tip) && profit>0)
            {
                if (donateMode == 1)
                {

                }
                else if (donateMode == 2)
                {
                    DonateBox tmp = new DonateBox();
                    if (tmp.ShowDialog(profit, CurrentSite.Currency, donatePercentage) == DialogResult.Yes)
                    {
                        CurrentSite.Donate(tmp.amount);
                        Thread.Sleep(200);
                    }
                    donateMode = (tmp.radioButton3.Checked ? 3 : tmp.radioButton2.Checked ? 1 : 2);
                    donatePercentage = (decimal)tmp.numericUpDown1.Value;
                }
                else if (donateMode==3)
                {
                    CurrentSite.Donate((donatePercentage / 100.0m) * profit);
                }
            }
            Stop("");
            if (CurrentSite != null)
            {
                CurrentSite.Disconnect();
            }
            save();
            Settings tmpSet = new DiceBot.Settings(this);
            tmpSet.loadsettings();
            tmpSet.nudDonatePercentage.Value = (decimal)donatePercentage;
            tmpSet.rdbDonateAuto.Checked = donateMode == 3;
            tmpSet.rdbDonateDefault.Checked = donateMode == 2;
            tmpSet.rdbDonateDont.Checked = donateMode == 1;
            writesettings(tmpSet);
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

        //stop button pressed
        private void btnStop_Click(object sender, EventArgs e)
        {
            
            Stop("stop button clicked");
        }

        #region Save and load settings
        delegate void dsave();
        void save()
        {
            if (InvokeRequired)
            {
                Invoke( new  dsave(save));
                return;
            }
            save(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings");
            if (programmerToolStripMenuItem.Checked)
            {
                try
                {
                    File.WriteAllText("TempCodeBackup.txt", richTextBox3.Text);
                }
                catch (Exception e)
                {
                    DumpLog(e.Message, 1);
                    DumpLog(e.StackTrace, 2);
                }
            }
            savepersonal();
        }

        void savepersonal()
        {
            using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings3"))
            {
                for (int i = 0; i < PSaveNames.Count; i++)
                {
                    sw.WriteLine(PSaveNames.Keys.ToArray<string>()[i] + "|" + Convert.ToString(getValue(PSaveNames.Keys.ToArray<string>()[i], true)));
                }
                sw.Close();
                sw.Dispose();
                return;
                    

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
                
                sw.WriteLine("LastStreakWin|" + StatsWindows.nudLastStreakWin.Value.ToString("00"));
                sw.WriteLine("LastStreakLose|" + StatsWindows.nudLastStreakLose.Value.ToString("00"));
                string msg = "";
                if (chkBotSpeed.Checked)
                    msg = "1";
                else msg = "0";
                sw.WriteLine("BotSpeedEnabled|" + msg);
                sw.WriteLine("BotSpeedValue|" + nudBotSpeed.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
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
                sw.WriteLine("ResetSeedValue|" + nudResetSeed.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                sw.WriteLine("QuickSwitchFolder|" + txtQuickSwitch.Text);
                sw.WriteLine("SettingsMode|" + (basicToolStripMenuItem.Checked?"0":advancedToolStripMenuItem.Checked?"1":"2"));
                sw.WriteLine("Site|" + (justDiceToolStripMenuItem.Checked?"0":primeDiceToolStripMenuItem.Checked?"1":pocketRocketsCasinoToolStripMenuItem.Checked?"2": diceToolStripMenuItem.Checked?"3":safediceToolStripMenuItem.Checked?"4":daDiceToolStripMenuItem.Checked?"5":rollinIOToolStripMenuItem.Checked?"6":bitDiceToolStripMenuItem.Checked?"7":betterbetsToolStripMenuItem.Checked?"8":moneyPotToolStripMenuItem.Checked?"9":"1"));
            }
        }
        
        delegate void dSave(string file);

        void save(string file)
        {
            if (InvokeRequired)
            {
                Invoke(new dSave(save), file);
                return;
            }
            using (StreamWriter sw = new StreamWriter(file))
            {
               
                try
                {
                    sw.WriteLine("SaveVersion|" + "3");
                    for (int i = 0; i < SaveNames.Count; i++ )
                    {
                        object t = getValue(SaveNames.Keys.ToArray<string>()[i], false);
                        if (t is string[])
                        {
                            string t2 = "";
                            foreach (string s in t as string[])
                            {
                                if (t2.Length > 0)
                                    t2 += "?";
                                t2 += s;
                            }
                            t = t2;
                        }
                        sw.WriteLine(SaveNames.Keys.ToArray<string>()[i]+"|"+Convert.ToString(t));
                    }
                        sw.Close();
                    sw.Dispose();
                    return;
                    
                    
                    
                }
                catch (Exception e)
                {
                    DumpLog(e.Message, 1);
                    DumpLog(e.StackTrace, 2);
                }
            }
        }
        bool load()
        {
            
            return (load(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\settings", true));
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
                        {}
                            //chkReverse.Checked = true;
                        else
                        { }
                            //chkReverse.Checked = false;                        
                        string cur = values[i++];
                        if (cur == "0")
                        {}
                            //rdbReverseBets.Checked = true;
                        else if (cur == "1")
                        {}//rdbReverseLoss.Checked = true;
                        else if (cur == "2")
                        { }// rdbReverseWins.Checked = true;
                        decimal tmpval = (decimal)dparse(values[i++], ref convert);
                        if (values.Length > i)
                        {
                            StatsWindows.nudLastStreakWin.Value = (decimal)dparse(values[i++], ref convert);
                            StatsWindows.nudLastStreakLose.Value = (decimal)dparse(values[i++], ref convert);
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

        public string getvalue(List<SavedItem> list, string item)
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

        bool load(string File, bool Settings)
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
                            saveditems.Add(new SavedItem(s[0], s[1]));
                        }
                    }
                    if (Settings)
                    {
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
                    }
                    bool safe = true;
                    string errors = "";
                    foreach (SavedItem t in saveditems)
                    {
                        try
                        {
                            SetValue(t.Name, t.Value, false);
                        }
                        catch (Exception e)
                        {
                            DumpLog(e.Message, 1);
                            DumpLog(e.StackTrace, 2);
                            errors += t.Name + ", ";
                            safe = false;
                        }
                    }
                    foreach (SavedItem t in saveditems)
                    {
                        try
                        {
                            SetValue(t.Name, t.Value, true);
                        }
                        catch (Exception e)
                        {
                            DumpLog(e.Message, 1);
                            DumpLog(e.StackTrace, 2);
                            errors += t.Name + ", ";
                            safe = false;
                        }
                    }
                    variabledisable();
                    if (!safe)
                    {
                        MessageBox.Show("There was a problem loading the following settings: " + errors);
                    }
                    return true;
                }
                                   
            }
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
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
                        ching= getvalue(saveditems, "CoinPath");
                        Sound = ("1"==getvalue(saveditems, "AlarmEnabled"));
                        SoundLow = ("1" == getvalue(saveditems, "AlarmLowEnabled"));
                        SoundStreak = ("1" == getvalue(saveditems, "AlarmStreakEnabled"));
                        SoundStreakCount =iparse(getvalue(saveditems, "AlarmStreakValue"));
                        salarm= getvalue(saveditems, "AlarmPath");
                        Emails.StreakSize = (int)Emails.StreakSize;
                        autoseeds = getvalue(saveditems, "AutoGetSeed") != "0";
                        maxRows = iparse(getvalue(saveditems, "NumLiveBets"));
                        maxRows = maxRows <= 0 ? 1 : maxRows;
                        startupMessage = (getvalue(saveditems, "StartupMessage") == "1" || getvalue(saveditems, "StartupMessage") == "-1");
                        donatePercentage = dparse(getvalue(saveditems, "DonatePercentage"), ref convert);
                        donateMode = iparse(getvalue(saveditems, "DonateMode"));
                        if (donatePercentage == -1) donatePercentage = 1;
                        if (donateMode == -1) donateMode = 2;


                    }

                }

                
                
            }

            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
            }
        }

        void writesettings(Settings TmpSet)
        {
            using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
            {
                sw.WriteLine("new");
                string temp2 = TmpSet.txtJDUser.Text + "," + TmpSet.txtJDPass.Text + ",";
                if (TmpSet.chkJDAutoLogin.Checked)
                    temp2 += "1,";
                else temp2 += "0";
                if (TmpSet.chkJDAutoStart.Checked)
                    temp2 += "1,";
                else temp2 += "0";
                string jdline = "";

                foreach (char c in temp2)
                {
                    jdline += ((int)c).ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " ";
                }
                sw.WriteLine(jdline);

                ////tray,botname,enableemail,emailaddress,emailwithdraw,emailinvest,emaillow,emailstreak,emailstreakval
                string msg = "";
                msg = (TmpSet.chkTray.Checked) ? "1" : "0";                
                sw.WriteLine("tray|"+msg);
                sw.WriteLine("botname|" + TmpSet.txtBot.Text);
                msg = (TmpSet.chkEmail.Checked) ? "1" : "0";  
                sw.WriteLine("enableemail|"+msg);
                sw.WriteLine("emailaddress|" + TmpSet.txtEmail.Text);
                msg = (TmpSet.chkEmailWithdraw.Checked) ? "1" : "0";  
                sw.WriteLine("emailwithdraw|"+msg);
                msg = (TmpSet.chkEmailLowLimit.Checked) ? "1" : "0";  
                sw.WriteLine("emaillow|"+msg);
                msg = (TmpSet.chkEmailStreak.Checked) ? "1" : "0";  
                sw.WriteLine("emailstreak|"+msg);
                sw.WriteLine("emailstreakval|" + TmpSet.nudEmailStreak.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                if (Emails == null)
                {
                    Emails = new Email("","");
                    Emails.SMTP = "emails11.secureserver.net";
                }
                sw.WriteLine("SMTP|" + Emails.SMTP);
                

                ////soundcoin,soundalarm,soundlower,soundstrea,soundstreakvalue

                msg = (TmpSet.chkSoundWithdraw.Checked) ? "1" : "0";
                sw.WriteLine("CoinEnabled|" + msg);
                sw.WriteLine("CoinPath|" + TmpSet.txtPathChing.Text);
                msg = (TmpSet.chkAlarm.Checked) ? "1" : "0";
                sw.WriteLine("AlarmEnabled|" + msg);
                msg = (TmpSet.chkSoundLowLimit.Checked) ? "1" : "0";
                sw.WriteLine("AlarmLowEnabled|" + msg);
                msg = (TmpSet.chkSoundStreak.Checked) ? "1" : "0";
                sw.WriteLine("AlarmStreakEnabled|" + msg);

                sw.WriteLine("AlarmStreakValue|" + TmpSet.nudSoundStreak.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                sw.WriteLine("AlarmPath|" + TmpSet.txtPathAlarm.Text);

                sw.WriteLine("AutoGetSeed|"+ (autoseeds?"1":"0"));
                sw.WriteLine("NumLiveBets|" + TmpSet.nudLiveBetsNum.Value);

                sw.WriteLine("DonatePercentage|" +TmpSet.nudDonatePercentage.Value );
                sw.WriteLine("StartupMessage|" + (TmpSet.chkStartup.Checked?"1":"0"));
                sw.WriteLine("DonateMode|"+ (TmpSet.rdbDonateDont.Checked?"1":TmpSet.rdbDonateDefault.Checked?"2":"3"));

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
                    load(ofdImport.FileName, false);
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
                            load(ofdImport.FileName, false);
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
            if ((sender as Control).Name == "nudMultiplier")
            {
                if ((sender as NumericUpDown).Value != nudMutliplier2.Value)
                    nudMutliplier2.Value = (sender as NumericUpDown).Value;
            }
            if ((sender as Control).Name == "nudWinMultiplier")
            {
                if ((sender as NumericUpDown).Value != nudWinMultiplier2.Value)
                    nudWinMultiplier2.Value = (sender as NumericUpDown).Value;
            }
            if ((sender as Control).Name == "nudMinBet")
            {
                if ((sender as NumericUpDown).Value != nudMinbet2.Value)
                    nudMinbet2.Value = (sender as NumericUpDown).Value;
            }
            if ((sender as Control).Name == "nudChance")
            {
                if ((sender as NumericUpDown).Value != nudChance2.Value)
                    nudChance2.Value = (sender as NumericUpDown).Value;
            }

            testInputs();
            try
            {
                CurrentSite.chance =  (Chance);
                                
            }
            catch (Exception ex)
            {
                DumpLog(ex.Message, 1);
                DumpLog(ex.StackTrace, 2);
            }
        }

        public decimal dparse(string text,ref bool success)
        {
            decimal number = -1;
            string test = "0.000001";
            decimal dtest = 0;
            if (decimal.TryParse(test, out dtest))
            {
                if (dtest != 0.000001m)
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

            
            if (!decimal.TryParse(text, out number))
            {
                
                if (!decimal.TryParse(text, out number))
                {
                    success = false;
                    return -1;
                    
                }
            }
            success = true;
            return number;
        }
        public int iparse(string text)
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
            Limit = (decimal)(nudLimit.Value);
            if (Limit == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }
            LowerLimit = (decimal)(nudLowerLimit.Value);
            if (LowerLimit == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Limit Field\n";
            }
            Amount = (decimal)(nudAmount.Value);
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
            MinBet = (decimal)(nudMinBet.Value);
            if (MinBet==-1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Minimum Bet Field\n";
            }
            if (!programmerToolStripMenuItem.Checked)
            Chance = (decimal)(nudChance.Value);
            if (Chance == -1)
            {
                valid = false;
                sMessage += "Please enter a valid % in the Chance Field (Without the % sign)";
            }
            else
            {

            }
            Multiplier = (decimal)(nudMultiplier.Value);
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
            Devider = (decimal)(nudDevider.Value);
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
            WinDevider = (decimal)(nudWinDevider.Value);
            if (WinDevider == -1)
            {
                valid = false;
                sMessage += "Please enter a valid number in the Times Multiplier By Field\n";
            }
            populateFiboNacci();
            if (!valid)
                MessageBox.Show(sMessage);
            return valid;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            int max =  maxbets();
            if (max == -500)
                StatsWindows.lblMaxBets.Text = "500+";
            else
                StatsWindows.lblMaxBets.Text = max.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

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
                Stop("stop button clicked");
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
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                TrayIcon.BalloonTipTitle = "DiceBot";
                TrayIcon.BalloonTipText = string.Format("Balance: {0:0.00000000}\n Profit: {1:0.00000000}\nCurrent Streak: {2}\nWorst Streak: {3}\nTime running: ", PreviousBalance, PreviousBalance - StartBalance, curstreak, WorstStreak) + (TotalTime + (DateTime.Now - dtStarted)).ToString(@"hh\:mm\:ss");
                TrayIcon.BalloonTipIcon = ToolTipIcon.None;
                TrayIcon.ShowBalloonTip(800);
            }
            
        }

        private void TrayIcon_MousedecimalClick(object sender, System.Windows.Forms.MouseEventArgs e)
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
        decimal tmpbalance = 0;
        int tmpwins = 0;
        int tmplosses = 0;
        decimal tmpprofit = 0;
        decimal tmpStartBalance = 0;
        int numSimBets = 0;
        void runsim()
        {
            numSimBets = (int)SimWindow.nudSimNumBets.Value;
            tmpbalance = PreviousBalance;
            tmpwins = Wins;
            tmplosses = Losses;
            tmpStartBalance = StartBalance;
            tmpprofit = profit;
            StartBalance = dPreviousBalance = (decimal)SimWindow.nudSimBalance.Value;
            Wins = Losses = 0;
            profit = 0;
            
            
            
            string chars = "0123456789abcdef";
            if (! (CurrentSite is dice999))
            {
                chars += "ghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ._";
            }
            server = "";

            for (int i = 0; i < 64; i++)
            {
                server += (chars[rand.Next(0, chars.Length)]);
            }
            client = "";
            if (CurrentSite is dice999)
            {
                client = rand.Next(0, int.MaxValue).ToString();
            }
            else
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
            if (programmerToolStripMenuItem.Checked)
                Lastbet = Lastbet;
            else
                Lastbet = MinBet;
            Start(false);
            
        }

        void Simbet()
         {
            dtLastBet = DateTime.Now;
            EnableTimer(tmBet, false);
            Bet tmp = new Bet();
            if (Wins + Losses < numSimBets)
            {
                string betstring = (Wins + Losses).ToString() + ",";
                if (!CurrentSite.NonceBased)
                {
                    string chars = "0123456789abcdef";
                    if (!(CurrentSite is dice999))
                    {
                        chars += "ghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ._";
                    }
                    server = "";

                    for (int i = 0; i < 64; i++)
                    {
                        server += (chars[rand.Next(0, chars.Length)]);
                    }
                    client = "";
                    if (CurrentSite is dice999)
                    {
                        client = rand.Next(0, int.MaxValue).ToString();
                    }
                    else
                        for (int i = 0; i < 24; i++)
                        {
                            client += rand.Next(0, 10).ToString();
                        }

                    string sserver = "";
                    foreach (byte b in server)
                    {
                        sserver += Convert.ToChar(b);
                    }
                    this.server = sserver;
                }
                decimal number = CurrentSite.GetLucky(server, client, Wins + Losses);
                tmp.Roll = (decimal)number;
                tmp.Chance = (decimal)Chance;
                tmp.Amount = (decimal)Lastbet;
                tmp.high = high;
                tmp.date = DateTime.Now;
                
                betstring += number.ToString() + "," + Chance.ToString() + ",";
                bool win = false;
                if (high)
                    betstring += ">" + (CurrentSite.maxRoll - Chance) + ",";
                else
                    betstring += "<" + Chance + ",";
                if (high && number > CurrentSite.maxRoll - Chance)
                {
                    win = true;
                }
                else if (!high && number < Chance)
                {
                    win = true;
                }
                decimal betProfit = 0;
                if (win)
                {
                    betstring += "win,";
                    betstring += Lastbet + ",";
                    betProfit = (Lastbet * 99 / Chance) - Lastbet;
                    betstring += betProfit  + ",";
                    tmp.Profit = (decimal)betProfit;    

                }
                else
                {

                    betstring += "lose,";
                    betstring += Lastbet + ",";
                    betProfit = -Lastbet ;
                    betstring +=  betProfit +",";
                    tmp.Profit = (decimal)betProfit;
                }
                this.PreviousBalance = dPreviousBalance + betProfit;
                betstring += PreviousBalance + ",";
                betstring += profit;
                tempsim.bets.Add(betstring);
                int bets = Wins + Losses;
                if (bets % 1000 == 0)
                {
                    Updatetext(SimWindow.lblSimProgress, ((decimal)bets / (decimal)numSimBets * 100.00m).ToString("00.00") + "%");
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

                GetBetResult(PreviousBalance, tmp);
            }
            else
                Stop("Simulation Complete");
            
            
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

        public void btnSim_Click(object sender, EventArgs e)
        {
            if (! stop)
            {
                MessageBox.Show("Please stop the bot before running a simulation.");
            }
            else
            { 
                bool go = true;
                if (SimWindow.nudSimNumBets.Value >= 1000000)
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
                SimWindow.lblSimRun.Text = "Running Simulation, Please Wait";
                SimWindow.lblSimRun.ForeColor = Color.Red;
                simthread = new Thread(new ThreadStart(runsim));
                simthread.Start();
            }
        }

        

        public void btnExportSim_Click(object sender, EventArgs e)
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
                catch (Exception ex)
                {
                    DumpLog(ex.Message, 1);
                    DumpLog(ex.StackTrace, 2);
                    MessageBox.Show("Failed exporting to " + svdExportSim.FileName);
                }
            }
        }

        public void GenerateBets_Click(string ClientSeed, string ServerSeed, long StartValue, long Amount)
        {
            if (ClientSeed != "" && ServerSeed != "")
            {
                List<string> Betlist = new List<string>();
                string headers = "betnumber,luckynumber,,Please note, This algorithm is still in testing, some Numbers Might be wrong.\n,,,Check the alternative roll verifier";
                Betlist.Add(headers);
                byte[] server = new byte[64];

                for (decimal i = StartValue; i < StartValue + Amount; i++)
                {
                    string curstring = i.ToString() + "," + CurrentSite.GetLucky(ServerSeed, ClientSeed, (int)i).ToString();
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
                catch (Exception e)
                {
                    DumpLog(e.Message, 1);
                    DumpLog(e.StackTrace, 2);
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
                SimWindow.lblSimRun.ForeColor = Color.Green;
                SimWindow.lblSimRun.Text = "Simulation Completed";
                lastsim = tempsim;
            }
        }
        #endregion

        void resetstats()
        {
            Wins = 0;
            Losses = 0;
            bool success = false;
            profit = 0;
            decimal tmp = CurrentSite.balance;
            if (success)
                StartBalance = tmp;
            Winstreak = Losestreak = BestStreak = WorstStreak = laststreaklose = laststreakwin =   BestStreak2 = WorstStreak2 = BestStreak3 = WorstStreak3 = numstreaks = numwinstreasks = numlosesreaks = 0;
            avgloss = avgstreak = LargestBet = LargestLoss = LargestWin = avgwin = 0.0m;
            TotalTime += (DateTime.Now - dtStarted);
            dtStarted = DateTime.Now;
            UpdateStats();
        }

        private void btnResetStats_Click(object sender, EventArgs e)
        {
            resetstats();
        }

        private void btnSaveUser_Click(object sender, EventArgs e)
        {
            Settings tmpSet = new DiceBot.Settings(this);
            if (tmpSet.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                writesettings( tmpSet );
                loadsettings();
            }
        }

        
        private void nudBotSpeed_ValueChanged(object sender, EventArgs e)
        {
            if (nudBotSpeed.Value != (decimal)0.0)
            {
                lblTimeBetween.Text = (1 / nudBotSpeed.Value).ToString("##0.0000") + "Seconds";
            }
        }



        

       

       

        #region charts
        //button for generating random charts - for testing purposes
        private void button1_Click(object sender, EventArgs e)
        {
            List<Bet> tmpBets = new List<Bet>();
            decimal previous = 0;
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
            Custom_Chart tmp = new Custom_Chart();
            if (tmp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (tmp.ChartType == 1)
                {
                    Graph g = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, tmp.StartID));
                    g.Show();
                }
                else
                {
                    Graph g = new Graph(sqlite_helper.GetBetForCharts(CurrentSite.Name, tmp.StartDate , tmp.EndDate));
                    g.Show();
                }
            }

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
            
            
        }

        private void btnBrowseStratFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            if (fdb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtQuickSwitch.Text = fdb.SelectedPath;
            }
            btnStratRefresh_Click(btnStratRefresh, new EventArgs());
        }

        private void btnStratRefresh_Click(object sender, EventArgs e)
        {
            lsbStrats.Items.Clear();
            cmbStrat.Items.Clear();
            if (Directory.Exists(txtQuickSwitch.Text))
            {
                foreach (string x in Directory.GetFiles(txtQuickSwitch.Text))
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(x))
                        {
                            string tmptxt = sr.ReadLine();
                            if (tmptxt.StartsWith("SaveVersion"))
                            {
                                lsbStrats.Items.Add(new FileInfo(x).Name);
                                cmbStrat.Items.Add(new FileInfo(x).Name);
                            }
                        }
                    }
                    catch { };
                }
            }
        }

        private void cmbStrat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(txtQuickSwitch.Text+"\\"+cmbStrat.SelectedItem.ToString()))
            {
                load(txtQuickSwitch.Text + "\\" + cmbStrat.SelectedItem.ToString(), false);
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
                catch (Exception ex)
                {
                    DumpLog(ex.Message, 1);
                    DumpLog(ex.StackTrace, 2);
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
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            
            if (InvokeRequired)
            {
                Invoke(new dupdateControll(updateBalance), Balance);
            }
            else
            {
                
                lblApiBalance.Text = (Balance is decimal?(decimal)(Balance):(decimal)(decimal)Balance).ToString("0.00000000");
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

        fChat PopoutChat = new fChat("");
        
        public void AddChat(object Message)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new dupdateControll(AddChat), Message);

                }
                else
                {
                    if (PopoutChat != null)
                    {
                        if (!PopoutChat.IsDisposed)
                        {
                            PopoutChat.GotMessage((string)Message);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
            }
        }


        //List<Bet> BetsToShow = new List<Bet>();
        int maxRows = 100;
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
                dataGridView1.DataBindings.Clear();
                Bet _Bet = (Bet as Bet);
                if (logging > 2)
                using (StreamWriter sw = File.AppendText("log.txt"))
                {
                    sw.WriteLine(json.JsonSerializer<Bet>(_Bet));
                }
                dataGridView1.Rows.Insert(0, _Bet.Id, _Bet.date, _Bet.Amount, _Bet.high, _Bet.Chance, _Bet.Roll,_Bet.Profit,_Bet.nonce );
                if ( dataGridView1.Rows.Count >0 )
                {
                    
                    if (dataGridView1.Rows[0].Cells[6].Value != null)
                    {
                        
                            if (!((bool)_Bet.high ? (decimal)_Bet.Roll > (decimal)CurrentSite.maxRoll - (decimal)(_Bet.Chance) : (decimal)_Bet.Roll < (decimal)(_Bet.Chance)))
                            {
                                if (_Bet.Chance<=50)
                                {
                                    if (
                                        (decimal)_Bet.Roll < (decimal)CurrentSite.maxRoll - (decimal)(_Bet.Chance) && 
                                        (decimal)_Bet.Roll > (decimal)(_Bet.Chance))
                                    {
                                        dataGridView1.Rows[0].Cells[5].Style.BackColor = Color.LightGray;
                                    }
                                    else
                                        dataGridView1.Rows[0].Cells[5].Style.BackColor = Color.Pink;
                                }
                                
                                dataGridView1.Rows[0].DefaultCellStyle.BackColor = Color.Pink;
                            }
                            else
                            {
                                if (_Bet.Chance > 50)
                                {
                                    
                                    if ((decimal)_Bet.Roll > (decimal)CurrentSite.maxRoll - (decimal)(_Bet.Chance) && 
                                        (decimal)_Bet.Roll < (decimal)(_Bet.Chance))
                                    {
                                        dataGridView1.Rows[0].Cells[5].Style.BackColor = Color.Gold;
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[0].Cells[5].Style.BackColor = Color.LightGreen;
                                    }
                                }
                                
                                    dataGridView1.Rows[0].DefaultCellStyle.BackColor = Color.LightGreen;
                                if (_Bet.Profit < 0)
                                {

                                }
                            }
                        
                    }
                }
                while (dataGridView1.Rows.Count > maxRows && dataGridView1.Rows.Count>0)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);
                }
                
            }
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {



            if ((sender as Button).Text == "Log In")
            {
                string curcur = CurrentSite.Currency;
                switch (CurrentSite.GetType().Name)
                {
                    case "JD": CurrentSite = new JD(this); break;
                    case "PRC": CurrentSite = new PRC(this); break;
                    case "BB": CurrentSite = new BB(this); break;
                    case "WD": CurrentSite = new WD(this); break;
                    case "bitdice": CurrentSite = new bitdice(this); break;
                    case "Coinichiwa": CurrentSite = new Coinichiwa(this); break;
                    case "CoinMillions": CurrentSite = new CoinMillions(this); break;
                    case "cryptogames": CurrentSite = new cryptogames(this); break;
                    case "dadice": CurrentSite = new dadice(this); break;
                    case "dice999": CurrentSite = new dice999(this, (CurrentSite as dice999).doge999); break;
                    case "doge999": CurrentSite = new dice999(this, true); break;
                    case "FortuneJack": CurrentSite = new FortuneJack(this); break;
                    case "MagicalDice": CurrentSite = new MagicalDice(this); break;
                    case "MoneroDice": CurrentSite = new MoneroDice(this); break;
                    case "moneypot": CurrentSite = new moneypot(this); break;
                    case "PD": CurrentSite = new PD(this); break;
                    case "rollin": CurrentSite = new rollin(this); break;
                    case "SafeDice": CurrentSite = new SafeDice(this); break;
                    case "SatoshiDice": CurrentSite = new SatoshiDice(this); break;
                }
                if (UseProxy)
                    CurrentSite.SetProxy(proxHost, proxport, proxUser, proxPass);
                CurrentSite.Currency = curcur;
                CurrentSite.FinishedLogin -= CurrentSite_FinishedLogin;
                CurrentSite.FinishedLogin +=CurrentSite_FinishedLogin;
                
                CurrentSite.Login(txtApiUsername.Text, txtApiPassword.Text, txtApi2fa.Text);
                
            }
            else
            {
                if (CurrentSite!=null)
                {
                    Stop("Logging out of site");
                    CurrentSite.Disconnect();
                    string curcur = CurrentSite.Currency;
                    switch (CurrentSite.GetType().Name)
                    {
                        case "JD": CurrentSite = new JD(this); break;
                        case "PRC": CurrentSite = new PRC(this); break;
                        case "BB": CurrentSite = new BB(this); break;
                        case "WD": CurrentSite = new WD(this); break;
                        case "bitdice": CurrentSite = new bitdice(this); break;
                        case "Coinichiwa": CurrentSite = new Coinichiwa(this); break;
                        case "CoinMillions": CurrentSite = new CoinMillions(this); break;
                        case "cryptogames": CurrentSite = new cryptogames(this); break;
                        case "dice999": CurrentSite = new dice999(this, false); break;
                        case "doge999": CurrentSite = new dice999(this, true); break;
                        case "FortuneJack": CurrentSite = new FortuneJack(this); break;
                        case "MagicalDice": CurrentSite = new MagicalDice(this); break;
                        case "MoneroDice": CurrentSite = new MoneroDice(this); break;
                        case "moneypot": CurrentSite = new moneypot(this); break;
                        case "PD": CurrentSite = new PD(this); break;
                        case "rollin": CurrentSite = new rollin(this); break;
                        case "SafeDice": CurrentSite = new SafeDice(this); break;
                        case "SatoshiDice": CurrentSite = new SatoshiDice(this); break;
                    }
                    if (UseProxy)
                        CurrentSite.SetProxy(proxHost, proxport, proxUser, proxPass);
                    CurrentSite.Currency = curcur;
                    EnableNotLoggedInControls(false);
                }
            }
            txtApi2fa.Text = "";
        }


        #endregion

        private void btnRegister_Click(object sender, EventArgs e)
        {
                
            if (CurrentSite.register)
            {
                ConfirmPassword Conf = new ConfirmPassword();
                bool Valid = false;

                if (Conf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Valid = Conf.Password == txtApiPassword.Text;
                }
                if (Valid)
                {
                    CurrentSite.FinishedLogin -= CurrentSite_FinishedLogin;
                    CurrentSite.FinishedLogin += CurrentSite_FinishedLogin;
            
                    if (CurrentSite.Register(txtApiUsername.Text, txtApiPassword.Text))
                    {
                        EnableNotLoggedInControls(true);
                    }
                }
                else
                {
                    MessageBox.Show("Passwords do not match!.");
                }
            }
            else
            {
                if (MessageBox.Show(string.Format("It looks like {0} does not allow registration through the API. Would you like to open {0} in your browser to register an account?", CurrentSite.Name)) == DialogResult.OK)
                {
                    Process.Start(CurrentSite.SiteURL);
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==0 && e.RowIndex>=0)
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
            CurrentSite.amount = ((decimal)nudApiBet.Value);
            CurrentSite.chance = (decimal)(nudApiChance.Value);
            CurrentSite.PlaceBet(true, (decimal)nudApiBet.Value, (decimal)(nudApiChance.Value));
        }

        /// <summary>
        /// place single bet, Low
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            CurrentSite.amount =((decimal)nudApiBet.Value);
            CurrentSite.chance = (decimal)(nudApiChance.Value);
            CurrentSite.PlaceBet(false, (decimal)nudApiBet.Value,(decimal)(nudApiChance.Value));
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
        bool ViewedAdvanced = false;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Checked)
            {
                if ((sender as ToolStripMenuItem).Name == "basicToolStripMenuItem")
                {
                    pnlProgrammer.Visible = pnlAdvanced.Visible = false;
                    pnlBasic.Visible = true;
                    scMain.SplitterDistance = (scMain.Width - pnlBasic.Width) - 3;
                    if (ViewedAdvanced)
                        MessageBox.Show("Please note: Settings set in the advanced mode are still be active.");
                }
                else if ((sender as ToolStripMenuItem).Name == "advancedToolStripMenuItem")
                {
                    ViewedAdvanced = true;
                    pnlAdvanced.Visible = true;
                    pnlProgrammer.Visible = pnlBasic.Visible = false;
                    scMain.SplitterDistance = (scMain.Width - pnlAdvanced.Width) - 3;
                }
                else if ((sender as ToolStripMenuItem).Name == "programmerToolStripMenuItem")
                {
                    ViewedAdvanced = true;
                    pnlProgrammer.Visible = true;
                    pnlAdvanced.Visible = pnlBasic.Visible = false;
                    scMain.SplitterDistance = (scMain.Width - pnlProgrammer.Width) - 3;

                }
            }
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
                string Response = Interaction.InputBox("Amount to withdraw: ", "Withdraw", "0.00000000", -1, -1);
                decimal tmpAmount = 0;
                if (decimal.TryParse(Response, out tmpAmount))
                {
                    string Address = Interaction.InputBox("Bitcoin Address: ", "Withdraw", "", -1, -1);
                    /*System.Text.RegularExpressions.Regex txt = null;

                    txt = new System.Text.RegularExpressions.Regex(@"^[13][a-km-zA-HJ-NP-Z0-9]{26,33}$");

                    bool valid = txt.IsMatch(Address);
                    if (valid)*/
                    {

                        CurrentSite.Withdraw(tmpAmount, Address);
                    }
                    /*else

                        MessageBox.Show("Invalid Address");*/
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
                string Response = Interaction.InputBox("Amount to invest: ", "Invest", "0.00000000", -1, -1);
                decimal tmpAmount = 0;
                if (decimal.TryParse(Response, out tmpAmount))
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
            if (CurrentSite.Tip)
            {
                string User = Interaction.InputBox((CurrentSite.TipUsingName?"Username":"User ID")+" of user to tip:", "Tip", "",-1,-1 );
                if (!CurrentSite.TipUsingName)
                {
                    int ID = 0;
                    if (!int.TryParse(User, out ID))
                    {
                        updateStatus("Invalid UserID");
                        return;
                    }
                }
                string Amount = Interaction.InputBox("Amount to tip: ", "Tip", "0.00000000", -1,-1);
                decimal tmpAmount = 0;
                if (decimal.TryParse(Amount, out tmpAmount))
                {
                    CurrentSite.SendTip(User, tmpAmount);

                }
                else
                {
                    MessageBox.Show("Input not a valid number");
                }
            }
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
                {
                    rdbMartingale.Checked = false;
                }
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
            if (tmp == rdbMartingale)
            {
                if (tmp.Checked)
                {
                    gbCustom.Enabled = true;
                    gbCustom.Text = "";
                }
                else
                {
                    gbCustom.Enabled = false;
                    gbCustom.Text = "These settings can only be used with martingale";
                }
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
                    catch (Exception ex)
                    {
                        DumpLog(ex.Message, 1);
                        DumpLog(ex.StackTrace, 2);
                        MessageBox.Show("Invalid bets file. Please make sure there are only bets in the file, 1 per line. NO other characters are permitted.");
                    }
            }
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            chrtEmbeddedLiveChart.Enabled = !chrtEmbeddedLiveChart.Enabled;
            if (!chrtEmbeddedLiveChart.Enabled)
                btnDisable.Text = "Start Chart";
            else
                btnDisable.Text = "Stop Chart";
        }

        private void btnChartReset_Click(object sender, EventArgs e)
        {
            Chartprofit = 0;
            chrtEmbeddedLiveChart.Series[0].Points.Clear();
            chrtEmbeddedLiveChart.Series[0].Points.AddXY(0, 0);
            
        }

        private void btnHideLive_Click(object sender, EventArgs e)
        {
            chrtEmbeddedLiveChart.Visible = !chrtEmbeddedLiveChart.Visible;
            if (chartToolStripMenuItem.Checked != chrtEmbeddedLiveChart.Visible)
                chartToolStripMenuItem.Checked = chrtEmbeddedLiveChart.Visible;
            if (chrtEmbeddedLiveChart.Visible)
            {
                btnHideLive.Text = "Hide Chart";
                splitContainer1.SplitterDistance = 250;
            }
            else
            {
                btnHideLive.Text = "Show Chart";
                splitContainer1.SplitterDistance = 25;
            }
        }

        private void txtApiUsername_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogIn_Click(btnLogIn, new EventArgs());
            }
        }

        
        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (nudMutliplier2.Value != nudMultiplier.Value)
                nudMultiplier.Value = nudMutliplier2.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (nudWinMultiplier2.Value != nudWinMultiplier.Value)
                nudWinMultiplier.Value = nudWinMultiplier2.Value;
        }

        private void nudChance2_ValueChanged(object sender, EventArgs e)
        {
            if (nudChance2.Value != nudChance.Value)
                nudChance.Value = nudChance2.Value;
        }

        private void nudMinbet2_ValueChanged(object sender, EventArgs e)
        {
            if (nudMinBet.Value != nudMinbet2.Value)
                nudMinBet.Value = nudMinbet2.Value;
        }

        DateTime LastMissingCheck = DateTime.Now;
        public void btnGetSeeds_Click(object sender, EventArgs e)
        {
            if (running)
            {
                MessageBox.Show("Please stop the bot before looking for missing seeds. This is an extremely expensive query to run and can cause other functions to stall or break.");
            }
            else
            { 
                GetMissingSeeds();
            }
        }
        
        void GetMissingSeeds()
        {
            LastMissingCheck = DateTime.Now;
            BetIDs = sqlite_helper.GetMissingSeedIDs(CurrentSite.Name);
        }
        List<long> BetIDs = new List<long>();
        
        private void tmrMissingSeeds_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now - LastMissingCheck).TotalMinutes > 5)
                {
                    GetMissingSeeds();
                }
                if (BetIDs.Count > 0 && !CurrentSite.GettingSeed)
                {
                    long tmp = BetIDs[0];
                    BetIDs.RemoveAt(0);
                    CurrentSite.GetSeed(tmp);

                }
            }
            catch { }
        }

        private void ChatSend_Click(string Message)
        {
            if (Message!="")
            {
                CurrentSite.SendChatMessage(Message);
                
            }
        }
  

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        LuaInterface Lua = LuaRuntime.GetLua();
        //LuaContext Lua = new LuaContext();
        private void richTextBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            
            
        }
        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void chartToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chartToolStripMenuItem.Checked != chrtEmbeddedLiveChart.Visible)
                btnHideLive_Click(btnHideLive, new EventArgs());
            
        }

        private void viewToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void loginPanelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            gbLogin.Visible = loginPanelToolStripMenuItem.Checked;
            
        }

        private void manualBettingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
                gbManualBet.Visible = manualBettingToolStripMenuItem.Checked;
            
        }

        private void statsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (statsToolStripMenuItem.Checked)
            {
                StatsWindows.Show();
            }
            else
            {
                statsToolStripMenuItem.Visible = false;
                
            }
        }

        

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void beginnersGuidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://bot.seuntjie.com/gettingstarted.aspxex");
        }

        private void justDiceToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            EnableNotLoggedInControls(false);
            if (CurrentSite != null)
            {
                CurrentSite.Disconnect();
            }
            if (CurrentSite is PD)
            {
                (CurrentSite as PD).ispd = false;
            }
            if ((sender as ToolStripMenuItem).Checked)
            {
                foreach (ToolStripMenuItem t in siteToolStripMenuItem.DropDownItems)
                {
                    if (t == sender as ToolStripMenuItem)
                    {
                        t.Checked = true;
                    }
                    else
                    {
                        t.Checked = false;
                    }
                }
                switch ((sender as ToolStripMenuItem).Name)
                {
                    case "justDiceToolStripMenuItem": CurrentSite = new JD(this); siteToolStripMenuItem.Text = "Site " + "(JD)"; break;
                    case "pocketRocketsCasinoToolStripMenuItem": CurrentSite = new PRC(this); siteToolStripMenuItem.Text = "Site " + "(BK)"; break;                    
                    case "diceToolStripMenuItem": CurrentSite = new dice999(this,false); siteToolStripMenuItem.Text = "Site " + "(999D)"; break;
                    case "dogeToolStripMenuItem": CurrentSite = new dice999(this, true); siteToolStripMenuItem.Text = "Site " + "(999D)"; break;
                    case "primeDiceToolStripMenuItem": CurrentSite = new PD(this); siteToolStripMenuItem.Text = "Site " + "(PD)"; break;
                    case "safediceToolStripMenuItem": CurrentSite = new SafeDice(this); siteToolStripMenuItem.Text = "Site (SD)"; break;
                    case "daDiceToolStripMenuItem": CurrentSite = new dadice(this); siteToolStripMenuItem.Text = "Site (DAD)"; break;
                    case "rollinIOToolStripMenuItem": CurrentSite = new rollin(this); siteToolStripMenuItem.Text = "Site (RIO)"; break;
                    case "bitDiceToolStripMenuItem": CurrentSite = new bitdice(this); siteToolStripMenuItem.Text = "Site (BD)"; break;
                    case "betterbetsToolStripMenuItem": CurrentSite = new BB(this); siteToolStripMenuItem.Text = "Site (BB)"; break;
                    case "wealthyDiceToolStripMenuItem": CurrentSite = new WD(this); siteToolStripMenuItem.Text = "Site (WD)"; break;
                    case "moneyPotToolStripMenuItem" : CurrentSite = new moneypot(this); siteToolStripMenuItem.Text = "Site (MP)"; break;
                       
                    case "coinMillionsToolStripMenuItem": CurrentSite = new CoinMillions(this); siteToolStripMenuItem.Text = "Site(CM)"; break;
                    case "magicalDiceToolStripMenuItem": CurrentSite = new MagicalDice(this); siteToolStripMenuItem.Text = "Site(MD)"; break;
                    //case "investdiceToolStripMenuItem": CurrentSite = new InvestDice(this); siteToolStripMenuItem.Text = "Site(ID)"; break;
                    case "coinichiwaToolStripMenuItem": CurrentSite = new Coinichiwa(this); siteToolStripMenuItem.Text = "Site(CW)"; break;
                    case "moneroDiceToolStripMenuItem": CurrentSite=new MoneroDice(this); siteToolStripMenuItem.Text="Site (MonD)"; break;
                    case "fortuneJackToolStripMenuItem" : CurrentSite = new FortuneJack(this); siteToolStripMenuItem.Text = "Site (FJ)"; break;
                    case "cryptoGamesToolStripMenuItem" : CurrentSite = new cryptogames(this); siteToolStripMenuItem.Text = "Site (CG)"; break;
                    case "bitslerToolStripMenuItem" : CurrentSite = new Bitsler(this); siteToolStripMenuItem.Text = "Site (BS)"; break;
                    case "satoshiDiceToolStripMenuItem" : CurrentSite = new SatoshiDice(this); siteToolStripMenuItem.Text = "Site (SatD)"; break;
                }
                if (CurrentSite is WD|| CurrentSite is PD || CurrentSite is dadice || CurrentSite is CoinMillions || CurrentSite is Coinichiwa || CurrentSite is cryptogames)
                {
                    lblPass.Text = "API key:";
                    lblUsername.Text = "Username:";
                }
                else if (CurrentSite is BB || CurrentSite is moneypot) 
                {
                    lblPass.Text = "API Token:";

                    lblUsername.Text = "Username:";
                }
                else if (CurrentSite is MoneroDice)
                {
                    lblPass.Text = "Private Key:";
                    lblUsername.Text = "Public Key:";
                }
                else if (CurrentSite is SatoshiDice)
                {
                    lblUsername.Text = "Email";
                    lblPass.Text = "Password:";
                }
                else
                {
                    lblPass.Text = "Password:";
                    lblUsername.Text = "Username:";
                }
                if (CurrentSite is moneypot)
                {
                    btnMPDeposit.Visible = btnMPWithdraw.Visible = true;
                }
                else
                {
                    btnMPDeposit.Visible = btnMPWithdraw.Visible = false;
                }
                rdbInvest.Enabled = CurrentSite.AutoInvest;
                if (!rdbInvest.Enabled)
                    rdbInvest.Checked = false;
                rdbWithdraw.Enabled = CurrentSite.AutoWithdraw;
                if (!rdbWithdraw.Enabled)
                    rdbWithdraw.Checked = false;
                if (UseProxy)
                    CurrentSite.SetProxy(proxHost, proxport, proxUser, proxPass);
            }
            
        }

        private void justDiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem t in siteToolStripMenuItem.DropDownItems)
            {
                if (t == sender as ToolStripMenuItem)
                {
                    t.Checked = true;
                }
                /*else
                {
                    t.Checked = false;
                }*/
            }
        }

        private void basicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem t in settingsModeToolStripMenuItem.DropDownItems)
            {
                t.Checked = t == sender as ToolStripMenuItem;
                
            }
        }

        private void btcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tmp = (sender as ToolStripMenuItem);
            if (!((ToolStripMenuItem)tmp.OwnerItem).Checked)
            {
                ((ToolStripMenuItem)tmp.OwnerItem).Checked = true;
            }
            
            foreach (ToolStripMenuItem t in  (tmp.Owner ).Items)
            {
                t.Checked = t == sender as ToolStripMenuItem;
            }
        }

        private void btcToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Checked)
            {
                CurrentSite.Currency = (sender as ToolStripMenuItem).Text.ToLower();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        void SetLuaVars()
        {
            try
            {
                //Lua.clear();
                Lua["balance"] = PreviousBalance ;                
                Lua["profit"] = this.profit;
                Lua["currentstreak"] = (Winstreak > 0) ? Winstreak : -Losestreak;
                Lua["previousbet"] = Lastbet;
                Lua["nextbet"] = Lastbet;
                Lua["chance"] = Chance;
                Lua["bethigh"] = high;
                Lua["bets"] = Wins + Losses;
                Lua["wins"] = Wins;
                Lua["losses"] = Losses;
                Lua["currencies"] = CurrentSite.Currencies;
                Lua["currency"] = CurrentSite.Currency;                
                Lua["enablersc"] = EnableReset;
                Lua["enablezz"] = EnableProgZigZag;
            }
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
                Stop("LUA ERROR!!");
                WriteConsole("LUA ERROR!!");
                WriteConsole(e.Message);
            }
        }
        void GetLuaVars()
        {

            try
            {
                Lastbet = (decimal)(double)Lua["nextbet"];
                Chance = (decimal)(double)Lua["chance"];
                high = (bool)Lua["bethigh"];
                CurrentSite.amount = Lastbet;
                CurrentSite.chance = Chance;
                if (CurrentSite.Currency != (string)Lua["currency"])
                    CurrentSite.Currency = (string)Lua["currency"];
                EnableReset = (bool)Lua["enablersc"];
                EnableProgZigZag = (bool)Lua["enablezz"];
            }
            catch (Exception e)
            {
                DumpLog(e.Message, 1);
                DumpLog(e.StackTrace, 2);
            }
        }
        int LCindex = 0;
        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {

                SetLuaVars();
                LCindex = 0;
                LastCommands.Add(txtConsoleIn.Text);
                if (LastCommands.Count>26)
                { LastCommands.RemoveAt(0); }
                WriteConsole(txtConsoleIn.Text);
                if (txtConsoleIn.Text.ToLower() == "start()")
                {
                    LuaRuntime.SetLua(Lua);
                    try
                    {
                        SetLuaVars();
                        LuaRuntime.Run(richTextBox3.Text);
                        GetLuaVars();
                        Start(false);
                    }
                    catch (Exception ex)
                    {
                        WriteConsole("LUA ERROR!!");
                        WriteConsole(ex.Message);
                        DumpLog(ex.Message, 1);
                        DumpLog(ex.StackTrace, 2);
                    }
                    
                }
                
                else
                {
                    try
                    {
                        LuaRuntime.SetLua(Lua);
                        LuaRuntime.Run(txtConsoleIn.Text);
                    }
                    catch (Exception ex)
                    {
                        WriteConsole("LUA ERROR!!");
                        WriteConsole(ex.Message);
                        DumpLog(ex.Message, 1);
                        DumpLog(ex.StackTrace, 2);
                    }
                }
                
                txtConsoleIn.Text = "";
                GetLuaVars();
            }
            if (e.KeyCode == Keys.Up)
            {
                if (LCindex < LastCommands.Count)
                    LCindex++;
                if (LastCommands.Count>0)
                txtConsoleIn.Text = LastCommands[LastCommands.Count - LCindex];

            }
            if (e.KeyCode == Keys.Down)
            {
                if (LCindex >0)
                    LCindex--;
                if (LCindex <=0)
                {
                    txtConsoleIn.Text = "";
                }
                else if (LastCommands.Count > 0)
                txtConsoleIn.Text = LastCommands[LastCommands.Count - LCindex];
                

            }
        }
        List<string> LastCommands = new List<string>();
        private void txtConsoleIn_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtConsoleIn.Text = "";
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Process.Start("http://bot.seuntjie.com/ProgrammerMode.html");
        }

        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://github.com/seuntie900/DiceBot");
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        
        private void button4_Click_1(object sender, EventArgs e)
        {
            if (PopoutChat == null)
            {
                PopoutChat = new fChat("");
                PopoutChat.SendMessage+=PopoutChat_SendMessage;
                PopoutChat.Show();
            }
            else if (PopoutChat.IsDisposed)
            {
                PopoutChat = new fChat("");
                PopoutChat.SendMessage += PopoutChat_SendMessage;
                PopoutChat.Show();
            }
            else
            {
                PopoutChat.Show();
                
            }
            
            
        }

        void PopoutChat_SendMessage(string Message)
        {
            
            ChatSend_Click(Message);
        }

        private void donateToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Donate tmp = new Donate(CurrentSite.Name);
            tmp.Show();
        }

        string proxUser = "", proxPass = "", proxHost ="";
        int proxport = 0;
        bool UseProxy = false;
        private void proxySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiceBot.Proxy Prox = new Proxy();
            if (Prox.ShowDialog() == DialogResult.OK)
            {
                UseProxy = Prox.chkProxy.Checked;
                proxUser = Prox.txtUsername.Text;
                proxPass = Prox.txtPassword.Text;
                proxHost = Prox.txtHost.Text;
                proxport = (int)Prox.nudPort.Value;
                if (UseProxy)
                    CurrentSite.SetProxy(proxHost, proxport, proxUser, proxPass);
            }
        }

        private void luckyNumberVerifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Verify tmp = new Verify(this);
            tmp.ShowDialog();
            
            

        }

        private void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        
        private void statsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StatsWindows.Show();
        }

        private void simulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SimWindow.Show();
        }

        private void pnlAdvancedAdvanced_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void btnHelpMartingale_Click(object sender, EventArgs e)
        {
            Process.Start("http://bot.seuntjie.com/martingale.html");
        }

        private void btnHelpLabouchere_Click(object sender, EventArgs e)
        {
            Process.Start("http://bot.seuntjie.com/labouchere.html");
        }

        private void btnHelpFibonacci_Click(object sender, EventArgs e)
        {
            Process.Start("http://bot.seuntjie.com/fibonacci.html");
        }

        private void btnHelpAlembert_Click(object sender, EventArgs e)
        {
            Process.Start("http://bot.seuntjie.com/alembert.html");
        }

        private void btnHelpPreset_Click(object sender, EventArgs e)
        {
            Process.Start("http://bot.seuntjie.com/presetlist.html");
        }

        private void btnOpenCode_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdtmp = new OpenFileDialog();
            if (ofdtmp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(ofdtmp.FileName))
                    {
                        string tmp = sr.ReadToEnd();
                        richTextBox3.Text = tmp;
                    }
                }
                catch (Exception ex)
                {
                    DumpLog(ex.Message, 1);
                    DumpLog(ex.StackTrace, 2);
                    MessageBox.Show("Invalid file!");
                }
            }
        }

        private void btnCodeSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog svdtmp = new SaveFileDialog();
            if (svdtmp.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                try
                {
                    File.WriteAllText(svdtmp.FileName, richTextBox3.Text);
                    MessageBox.Show("Saved!");
                }
                catch (Exception ex)
                {
                    DumpLog(ex.Message, 1);
                    DumpLog(ex.StackTrace, 2);
                    MessageBox.Show("Could not save code to file.");
                }
            }
        }


        object getValue(string key, bool Private)
        {
            if ((SaveNames.ContainsKey(key) && !Private) || (Private && PSaveNames.ContainsKey(key)))
            {
                Control c = !Private ? SaveNames[key] : PSaveNames[key];
                if (c == null)
                {
                    if ( key == "SettingsMode")
                        return (basicToolStripMenuItem.Checked ? 0 : advancedToolStripMenuItem.Checked ? 1 : 2);
                    if (key == "Site")
                        return (justDiceToolStripMenuItem.Checked ? 0 : 
                            primeDiceToolStripMenuItem.Checked ? 1 : 
                            pocketRocketsCasinoToolStripMenuItem.Checked ? 2 : 
                            diceToolStripMenuItem.Checked ? 3 : 
                            safediceToolStripMenuItem.Checked ? 4 : 
                            daDiceToolStripMenuItem.Checked ? 5 : 
                            rollinIOToolStripMenuItem.Checked ? 6 : 
                            bitDiceToolStripMenuItem.Checked ? 7: 
                            betterbetsToolStripMenuItem.Checked?8:
                            moneyPotToolStripMenuItem.Checked?9:
                            coinMillionsToolStripMenuItem.Checked ? 10 :
                            magicalDiceToolStripMenuItem.Checked ? 11 :
                            fortuneJackToolStripMenuItem.Checked? 12:
                            cryptoGamesToolStripMenuItem.Checked?13:
                            bitslerToolStripMenuItem.Checked?14:
                            dogeToolStripMenuItem.Checked?15:
                            wealthyDiceToolStripMenuItem.Checked?16:
                            satoshiDiceToolStripMenuItem.Checked?17:
                            1);
                }
                else if (c is TextBox)
                    return (c as TextBox).Text;
                else if (c is NumericUpDown)
                    return (c as NumericUpDown).Value;
                else if (c is RadioButton)
                {
                    if (key == "MultiplierMode")
                    return rdbMaxMultiplier.Checked? "0" : rdbDevider.Checked ?"1": rdbConstant.Checked ?"2":"3";
                    
                    if (key == "WinMultiplyMode")
                    {
                        return rdbWinConstant.Checked? "0": rdbWinDevider.Checked ? "1":
                        rdbWinMaxMultiplier.Checked ? "2":"3";

                    }
                    if (key == "LabComplete")
                    {
                        return rdbLabRestart.Checked ? "1":"2";
                    }
                    if (key == "Strategy")
                    {
                        return rdbMartingale.Checked ?"0":
                        rdbLabEnable.Checked ?"1":
                        rdbFibonacci.Checked ?"2":
                        rdbAlembert.Checked ?"3":"4";
                    }
                    if (key == "FibonacciLoss")
                    {
                        return rdbFiboLossIncrement.Checked ?"0":
                            rdbFiboLossReset.Checked ?"1":"2";
                    }
                    if (key == "FibonacciWin")
                    {
                        return rdbFiboWinIncrement.Checked ?"0":
                        rdbFiboWinReset.Checked ?"1":"2";
                    }
                    if (key == "FibonacciLevel")
                    {
                       return  rdbFiboLevelStop.Checked ?"0":"1";

                    }
                    if (key == "PresetEnd")
                    {
                        return rdbPresetEndReset.Checked ?"0":
                        rdbPresetEndStep.Checked ?"1":"2";
                    }
                    if (key == "PresetLoss")
                    {
                        return rdbPresetLossReset.Checked ?"0":
                        rdbPresetLossStep.Checked ?"1":"2";
                    }
                    if (key == "PresetWin")
                    {
                        return rdbPresetWinReset.Checked ?"0":
                        rdbPresetWinStep.Checked ?"1":"2";
                    }
                    if (key == "OnStop")
                    {
                        return rdbInvest.Checked?"0": rdbStop.Checked?"1":"2";
                    }
                    if (key == "ResetSeedMode")
                    {
                        return rdbResetSeedBets.Checked ? "0":rdbResetSeedWins.Checked? "1":"2";
                    }
                }
                else if (c is CheckBox)
                    return (c as CheckBox).Checked;
                else if (c is RichTextBox)
                {
                    
                    return (c as RichTextBox).Lines;
                }

                return null;
            }
            else
            {
                return null;
            }
        }
        void SetValue(string Key, int value, bool Private)
        {
            if ((SaveNames.ContainsKey(Key) && !Private) || (Private && PSaveNames.ContainsKey(Key)))
            {
                Control c = !Private ? SaveNames[Key] : PSaveNames[Key];
                if (c == null)
                {

                    //sw.WriteLine("SettingsMode|" + (basicToolStripMenuItem.Checked ? "0" : advancedToolStripMenuItem.Checked ? "1" : "2"));
                    //sw.WriteLine("Site|" + (justDiceToolStripMenuItem.Checked ? "0" : primeDiceToolStripMenuItem.Checked ? "1" : pocketRocketsCasinoToolStripMenuItem.Checked ? "2" : diceToolStripMenuItem.Checked ? "3" : safediceToolStripMenuItem.Checked ? "4" : daDiceToolStripMenuItem.Checked ? "5" : rollinIOToolStripMenuItem.Checked ? "6" : bitDiceToolStripMenuItem.Checked ? "7" : "1"));
                    if (Key == "Site")
                    {
                        justDiceToolStripMenuItem.Checked = value == 0;
                        primeDiceToolStripMenuItem.Checked = value == 1;
                        pocketRocketsCasinoToolStripMenuItem.Checked = value == 2;
                        diceToolStripMenuItem.Checked = value == 3;
                        safediceToolStripMenuItem.Checked = value == 4;
                        daDiceToolStripMenuItem.Checked = value == 5;
                        rollinIOToolStripMenuItem.Checked = value == 6;
                        bitDiceToolStripMenuItem.Checked = value == 7;
                        betterbetsToolStripMenuItem.Checked = value == 8;
                        moneyPotToolStripMenuItem.Checked = value == 9;
                        coinMillionsToolStripMenuItem.Checked = value == 10;
                        magicalDiceToolStripMenuItem.Checked = value == 11;
                        fortuneJackToolStripMenuItem.Checked = value == 12;
                        cryptoGamesToolStripMenuItem.Checked = value == 13;
                        bitslerToolStripMenuItem.Checked = value == 14;
                        dogeToolStripMenuItem.Checked = value == 15;
                        wealthyDiceToolStripMenuItem.Checked = value == 16;
                        satoshiDiceToolStripMenuItem.Checked = value == 17;
                        if (value > 17)
                        {
                            primeDiceToolStripMenuItem.Checked = true; ;
                        }

                    }
                    else if (Key == "SettingsMode")
                    {

                        basicToolStripMenuItem.Checked = value == 0;
                        advancedToolStripMenuItem.Checked = value == 1;
                        programmerToolStripMenuItem.Checked = value == 2;
                    }
                }
                else if (c is NumericUpDown)
                    (c as NumericUpDown).Value = Convert.ToDecimal(value);
                else if (c is RadioButton)
                {
                    if (Key == "MultiplierMode")
                    {
                        
                        rdbMaxMultiplier.Checked = value==0;                            
                        rdbDevider.Checked = value ==1;
                        rdbConstant.Checked = value == 2;                            
                        rdbReduce.Checked = value == 3;
                    }
                    if (Key == "WinMultiplyMode")
                    {
                        rdbWinConstant.Checked = value == 0;
                        rdbWinDevider.Checked = value == 1;                            
                        rdbWinMaxMultiplier.Checked= value == 2;
                        rdbWinReduce.Checked = value == 3;
                            
                    }
                    if ( Key == "LabComplete")
                    {
                        rdbLabRestart.Checked = value==1;
                        rdbLabStop.Checked = value== 2;
                    }
                    if (Key == "Strategy")
                    {
                        rdbMartingale.Checked = value == 0;
                        rdbLabEnable.Checked = value == 1;
                        rdbFibonacci.Checked = value == 2;
                        rdbAlembert.Checked = value == 3;
                        rdbPreset.Checked = value == 4;
                    }
                    if (Key == "FibonacciLoss")
                    {
                        rdbFiboLossIncrement.Checked =value== 0;
                        rdbFiboLossReset.Checked = value==1;
                        rdbFiboLossStop.Checked = value== 2;
                    }
                    if (Key == "FibonacciWin")
                    {
                        rdbFiboWinIncrement.Checked = value == 0;
                        rdbFiboWinReset.Checked = value == 1;
                        rdbFiboWinStop.Checked = value == 2;
                    }
                    if (Key == "FibonacciLevel")
                    {
                       rdbFiboLevelStop.Checked = value == 0;
                       rdbFiboLevelReset.Checked = value == 1;
                        
                    }
                    if (Key == "PresetEnd")
                    {
                        rdbPresetEndReset.Checked = value == 0;
                        rdbPresetEndStep.Checked = value == 1;
                        rdbPresetEndStop.Checked = value == 2;
                    }
                    if (Key == "PresetLoss")
                    {
                        rdbPresetLossReset.Checked = value == 0;
                        rdbPresetLossStep.Checked = value == 1;
                        rdbPresetLossStop.Checked = value == 2;
                    }
                    if (Key == "PresetWin")
                    {
                        rdbPresetWinReset.Checked = value == 0;
                        rdbPresetWinStep.Checked = value == 1;
                        rdbPresetWinStop.Checked = value == 2;
                    }
                    if (Key == "OnStop")
                    {
                        rdbInvest.Checked = value == 0;
                        rdbStop.Checked = value == 1;
                        rdbWithdraw.Checked = value == 2;
                    }
                    if (Key == "ResetSeedMode")
                    {
                        rdbResetSeedBets.Checked = value == 0;
                        rdbResetSeedWins.Checked = value == 1;
                        rdbResetSeedLosses.Checked = value == 2;
                    }
                }
                else if (c is CheckBox)
                    (c as CheckBox).Checked = value == 1;
            }
        }
        void SetValue(string Key, string value, bool Private)
        {
            if ((SaveNames.ContainsKey(Key) && !Private) || (Private && PSaveNames.ContainsKey(Key)))
            {

                Control c = !Private ? SaveNames[Key] : PSaveNames[Key];
                if (c == null)
                {

                    //sw.WriteLine("SettingsMode|" + (basicToolStripMenuItem.Checked ? "0" : advancedToolStripMenuItem.Checked ? "1" : "2"));
                    //sw.WriteLine("Site|" + (justDiceToolStripMenuItem.Checked ? "0" : primeDiceToolStripMenuItem.Checked ? "1" : pocketRocketsCasinoToolStripMenuItem.Checked ? "2" : diceToolStripMenuItem.Checked ? "3" : safediceToolStripMenuItem.Checked ? "4" : daDiceToolStripMenuItem.Checked ? "5" : rollinIOToolStripMenuItem.Checked ? "6" : bitDiceToolStripMenuItem.Checked ? "7" : "1"));
                    if (Key == "Site")
                    {
                        justDiceToolStripMenuItem.Checked = value == "0";
                        primeDiceToolStripMenuItem.Checked = value == "1";
                        pocketRocketsCasinoToolStripMenuItem.Checked = value == "2";
                        diceToolStripMenuItem.Checked = value == "3";
                        safediceToolStripMenuItem.Checked = value == "4";
                        daDiceToolStripMenuItem.Checked = value == "5";
                        rollinIOToolStripMenuItem.Checked = value == "6";
                        bitDiceToolStripMenuItem.Checked = value == "7";
                        betterbetsToolStripMenuItem.Checked = value == "8";
                        moneyPotToolStripMenuItem.Checked = value == "9";
                        coinMillionsToolStripMenuItem.Checked = value == "10";
                        magicalDiceToolStripMenuItem.Checked = value == "11";
                        fortuneJackToolStripMenuItem.Checked = value == "12";
                        cryptoGamesToolStripMenuItem.Checked = value == "13";
                        bitslerToolStripMenuItem.Checked = value == "14";
                        dogeToolStripMenuItem.Checked = value == "15";
                        wealthyDiceToolStripMenuItem.Checked = value == "16";
                        satoshiDiceToolStripMenuItem.Checked = value == "17";
                    }
                    else if (Key == "SettingsMode")
                    {
                        
                        basicToolStripMenuItem.Checked = value == "0";
                        advancedToolStripMenuItem.Checked = value == "1";
                        programmerToolStripMenuItem.Checked = value == "2";
                    }
                }
                else if (c is TextBox)
                    (c as TextBox).Text = value;
                else if (c is NumericUpDown)
                    (c as NumericUpDown).Value = Convert.ToDecimal(value);
                else if (c is RadioButton)
                {
                    if (Key == "MultiplierMode")
                    {
                        
                        rdbMaxMultiplier.Checked = value=="0";                            
                        rdbDevider.Checked = value =="1";
                        rdbConstant.Checked = value == "2";                            
                        rdbReduce.Checked = value == "3";
                    }
                    if (Key == "WinMultiplyMode")
                    {
                        rdbWinConstant.Checked = value == "0";
                        rdbWinDevider.Checked = value == "1";                            
                        rdbWinMaxMultiplier.Checked= value == "2";
                        rdbWinReduce.Checked = value == "3";
                            
                    }
                    if ( Key == "LabComplete")
                    {
                        rdbLabRestart.Checked = value=="1";
                        rdbLabStop.Checked = value== "2";
                    }
                    if (Key == "Strategy")
                    {
                        rdbMartingale.Checked = value =="0";
                        rdbLabEnable.Checked = value == "1";
                        rdbFibonacci.Checked = value == "2";
                        rdbAlembert.Checked = value == "3";
                        rdbPreset.Checked = value == "4";
                    }
                    if (Key == "FibonacciLoss")
                    {
                        rdbFiboLossIncrement.Checked =value== "0";
                        rdbFiboLossReset.Checked = value=="1";
                        rdbFiboLossStop.Checked = value== "2";
                    }
                    if (Key == "FibonacciWin")
                    {
                        rdbFiboWinIncrement.Checked = value == "0";
                        rdbFiboWinReset.Checked = value == "1";
                        rdbFiboWinStop.Checked = value == "2";
                    }
                    if (Key == "FibonacciLevel")
                    {
                       rdbFiboLevelStop.Checked = value == "0";
                       rdbFiboLevelReset.Checked = value == "1";
                        
                    }
                    if (Key == "PresetEnd")
                    {
                        rdbPresetEndReset.Checked = value == "0";
                        rdbPresetEndStep.Checked = value == "1";
                        rdbPresetEndStop.Checked = value == "2";
                    }
                    if (Key == "PresetLoss")
                    {
                        rdbPresetLossReset.Checked = value == "0";
                        rdbPresetLossStep.Checked = value == "1";
                        rdbPresetLossStop.Checked = value == "2";
                    }
                    if (Key == "PresetWin")
                    {
                        rdbPresetWinReset.Checked = value == "0";
                        rdbPresetWinStep.Checked = value == "1";
                        rdbPresetWinStop.Checked = value == "2";
                    }
                    if (Key=="OnStop")
                    {
                        rdbInvest.Checked= value =="0";
                        rdbStop.Checked = value == "1";
                        rdbWithdraw.Checked = value == "2";
                    }
                    if (Key == "ResetSeedMode")
                    {
                        rdbResetSeedBets.Checked = value =="0";
                        rdbResetSeedWins.Checked = value == "1";
                        rdbResetSeedLosses.Checked = value == "2";
                    }

                
                }
                else if (c is CheckBox)
                    (c as CheckBox).Checked = value == "1" || value == "True";
                else if (c is RichTextBox)
                    (c as RichTextBox).Lines = value.Split('?');
                
            }
        }
        void SetValue(string Key, decimal value, bool Private)
        {
            if ((SaveNames.ContainsKey(Key) && !Private) || (Private && PSaveNames.ContainsKey(Key)))
            {
                Control c = !Private ? SaveNames[Key] : PSaveNames[Key];
                if (c is NumericUpDown)
                    (c as NumericUpDown).Value = Convert.ToDecimal(value);
            }
        }
        void SetValue(string Key, bool value, bool Private)
        {
            if ((SaveNames.ContainsKey(Key) && !Private) || (Private && PSaveNames.ContainsKey(Key)))
            {
                Control c = !Private? SaveNames[Key]: PSaveNames[Key];
                if (c is CheckBox)
                    (c as CheckBox).Checked = value;
            }
        }

        void PopulateSaveNames()
        {
            SaveNames.Add("MinBet", nudMinBet);
            SaveNames.Add("Multiplier", nudMultiplier);
            SaveNames.Add("Chance", nudChance);
            SaveNames.Add("MaxMultiply", nudMaxMultiplies);
            SaveNames.Add("NBets", nudNbets);
            SaveNames.Add("Devider", nudDevider);
            SaveNames.Add("MultiplierMode", rdbMaxMultiplier);
            SaveNames.Add("ResetBetLossEnabled", chkResetBetLoss);
            SaveNames.Add("ResetBetLossValue", nudResetBetLoss);
            SaveNames.Add("ResetBetWinsEnabled", chkResetBetWins);
            SaveNames.Add("ResetWinsValue", nudResetWins);
            SaveNames.Add("WinMultiplier", nudWinMultiplier);
            SaveNames.Add("WinMaxMultiplies", nudWinMaxMultiplies);
            SaveNames.Add("WinNBets", nudWinNBets);
            SaveNames.Add("WinDevider", nudWinDevider);
            SaveNames.Add("WinMultiplyMode", rdbWinConstant);
            SaveNames.Add("StopAfterLoseStreakEnabled", chkStopLossStreak);
            SaveNames.Add("StopAfterLoseStreakValue", nudStopLossStreak);
            SaveNames.Add("StopAfterLoseStreakBtcEnabled", chkStopLossBtcStreak);
            SaveNames.Add("StopAfterLoseStreakBtcValue", nudStopLossBtcStreal);
            SaveNames.Add("StopAfterLoseBtcEnabled", chkStopLossBtc);
            SaveNames.Add("StopAfterLoseBtcValue", nudStopLossBtc);
            
            /*sw.Write("MultiplierMode|");
            if (rdbMaxMultiplier.Checked)
                sw.WriteLine("0");
            else if (rdbDevider.Checked)
                sw.WriteLine("1");
            else if (rdbConstant.Checked)
                sw.WriteLine("2");
            else sw.WriteLine("3");*/            
            /*sw.Write("WinMultiplyMode|");
            if (rdbWinConstant.Checked)
                sw.WriteLine("0");
            else if (rdbWinDevider.Checked)
                sw.WriteLine("1");
            else if (rdbWinMaxMultiplier.Checked)
                sw.WriteLine("2");
            else if (rdbWinReduce.Checked)
                sw.WriteLine("3");*/

            SaveNames.Add("ChangeAfterLoseStreakEnabled", chkChangeLoseStreak);
            SaveNames.Add("ChangeAfterLoseStreakSize", nudChangeLoseStreak);
            SaveNames.Add("ChangeAfterLoseStreakTo", nudChangeLoseStreakTo);
            SaveNames.Add("StopAfterWinStreakEnabled", chkStopWinStreak);
            SaveNames.Add("StopAfterWinStreakValue", nudStopWinStreak);
            SaveNames.Add("StopAfterWinStreakBtcEnabled", chkStopWinBtcStreak);
            SaveNames.Add("StopAfterWinStreakBtcValue", nudStopWinBtcStreak);
            SaveNames.Add("StopAfterWinBtcEnabled", chkStopWinBtc);
            SaveNames.Add("StopAfterWinBtcValue", nudStopWinBtc);
            SaveNames.Add("ChangeAfterWinStreakEnabled", chkChangeWinStreak);
            SaveNames.Add("ChangeAfterWinStreakSize", nudChangeWinStreak);
            SaveNames.Add("ChangeAfterWinStreakTo", nudChangeWinStreakTo);

            SaveNames.Add("ChangeChanceAfterLoseStreakEnabled", chkChangeChanceLose);
            SaveNames.Add("ChangeChanceAfterLoseStreakSize", nudChangeChanceLoseStreak);
            SaveNames.Add("ChangeChanceAfterLoseStreakValue", nudChangeChanceLoseTo);
            SaveNames.Add("ChangeChanceAfterWinStreakEnabled", chkChangeChanceWin);
            SaveNames.Add("ChangeChanceAfterWinStreakSize", nudChangeChanceWinStreak);
            SaveNames.Add("ChangeChanceAfterWinStreakValue", nudChangeChanceWinTo);
            SaveNames.Add("MutawaMultiplier", nudMutawaMultiplier);
            SaveNames.Add("MutawaWins", nudMutawaWins);
            SaveNames.Add("MutawaEnabled", checkBox1);

            SaveNames.Add("TrazalWin", nudTrazelWin);
            SaveNames.Add("TrazalWinTo", nudtrazelwinto);
            SaveNames.Add("TrazalLose", NudTrazelLose);
            SaveNames.Add("TrazalLoseTo", nudtrazelloseto);
            SaveNames.Add("TrazelMultiPlier", nudTrazelMultiplier);
            SaveNames.Add("TrazelEnabled", chkTrazel);
            SaveNames.Add("MKIncrement", nudMKIncrement);
            SaveNames.Add("MKDecrement", nudMKDecrement);
            SaveNames.Add("MKEnabled", chkMK);

            SaveNames.Add("LabReverse", chkReverseLab);
            SaveNames.Add("LabValues", rtbBets);
            SaveNames.Add("LabComplete", rdbLabStop );

            SaveNames.Add("Strategy", rdbMartingale);
            
            SaveNames.Add("FibonacciLoss", rdbFiboLossIncrement);
            SaveNames.Add("FibonacciWin", rdbFiboWinIncrement);
            SaveNames.Add("FibonacciLevel", rdbFiboLevelStop);
            SaveNames.Add("FibonacciLevelEnabled", chkFiboLevel);
            SaveNames.Add("FibonacciLossSteps", nudFiboLossIncrement);
            SaveNames.Add("FibonacciWinSteps", nudFiboWinIncrement);
            SaveNames.Add("FibonnaciLevelSteps", nudFiboLeve);
            
            SaveNames.Add("dAlembertLossIncrement", nudAlembertIncrementLoss);
            SaveNames.Add("dAlembertLossStretch", nudAlembertStretchLoss);
            SaveNames.Add("dAlembertWinIncrement", nudAlembertIncrementWin);
            SaveNames.Add("dAlembertWinStretch", nudAlembertStretchWin);
            
            SaveNames.Add("PresetValues", rtbPresetList);
            SaveNames.Add("PresetEnd", rdbPresetEndReset);
            SaveNames.Add("PresetEndStep", nudPresetEndStep);
            SaveNames.Add("PresetLoss", rdbPresetLossReset);
            SaveNames.Add("PresetLossStep", nudPresetLossStep);
            SaveNames.Add("PresetWin", rdbPresetWinReset);
            SaveNames.Add("PresetWinStep", nudPresetWinStep);

            SaveNames.Add("ReverseWin", chkZigZagWins);
            SaveNames.Add("ReverseWinStreak", chkZigZagWinsStreak);
            SaveNames.Add("ReverseLoss", chkZigZagLoss);
            SaveNames.Add("ReverseLossStreak", chkZigZagLossStreak);
            SaveNames.Add("ReverseBet", chkZigZagBets);
            SaveNames.Add("ReverseWinValue",nudZigZagWins );
            SaveNames.Add("ReverseWinStreakValue", nudZigZagWinsStreak);
            SaveNames.Add("ReverseLossValue", nudZigZagLoss);
            SaveNames.Add("ReverseLossStreakValue",nudZigZagLossStreak );
            SaveNames.Add("ReverseBetValue", nudZigZagBets);
            
            SaveNames.Add("ResetBtcStreakLoss", chkResetBtcStreakLoss);
            SaveNames.Add("ResetBtcStreakLossValue", nudResetBtcStreakLoss);
            SaveNames.Add("ResetBtcLoss",chkResetBtcLoss );
            SaveNames.Add("ResetBtcLossValue", nudResetBtcLoss);

            SaveNames.Add("ResetBtcStreakProfit",chkResetBtcStreakProfit );
            SaveNames.Add("ResetBtcStreakProfitValue", nudResetBtcStreakProfit);
            SaveNames.Add("ResetBtcProfit", chkResetBtcProfit);
            SaveNames.Add("ResetBtcProfitValue", nudResetBtcProfit);

            SaveNames.Add("FirstResetLoss", chkFirstResetLoss);
            SaveNames.Add("FirstResetWin", chkFirstResetWin);
            
            SaveNames.Add("MartingaleStretchLoss", nudStretchLoss);
            SaveNames.Add("MartingaleStretchWin", nudStretchWin);
            SaveNames.Add("EnableMaximumBet", chkMaxBet);
            SaveNames.Add("EnableMinumumBet", chkMinBet);
            SaveNames.Add("MaximumBet",nudMaximumBet);
            SaveNames.Add("MinumumBet", nudMinumumBet);

            SaveNames.Add("StopBetsEnable", chkStopBets);
            SaveNames.Add("StopBetsValue", nudStopBets);
            SaveNames.Add("ResetBetsEnable", chkResetBets);
            SaveNames.Add("ResetBetsValue", nudResetBets);
            SaveNames.Add("StopTimeEnable", chkStopTime);
            SaveNames.Add("StopTimeHour", nudStopTimeH);
            SaveNames.Add("StopTimeMinute", nudStopTimeM);
            SaveNames.Add("StopTimeSecond", nudStopTimeS);

            SaveNames.Add("StopLossesEnable", chkStopLosses);
            SaveNames.Add("StopLossesValue", nudStopLosses);
            SaveNames.Add("ResetLossesEnable", chkResetLosses);
            SaveNames.Add("ResetLossesValue", nudResetLosses);

            SaveNames.Add("StopWinsEnable", chkStopWins);
            SaveNames.Add("StopWinsValue", nudStopWins);
            SaveNames.Add("ResetWinsEnable", chkResetWins);
            SaveNames.Add("ResetWinsValue2", nudResetWins2);
            
            
            PSaveNames.Add("Amount", nudAmount);
            PSaveNames.Add("Limit", nudLimit);
            PSaveNames.Add("LimitEnabled", chkLimit);
            PSaveNames.Add("LowerLimit",nudLowerLimit );
            PSaveNames.Add("LowerLimitEnabled", chkLowerLimit);
            PSaveNames.Add("To", txtTo);
            PSaveNames.Add("OnStop", rdbInvest);
                        
            PSaveNames.Add("LastStreakWin", StatsWindows.nudLastStreakWin);
            PSaveNames.Add("LastStreakLose", StatsWindows.nudLastStreakLose);
            PSaveNames.Add("BotSpeedEnabled", chkBotSpeed);
            PSaveNames.Add("BotSpeedValue", nudBotSpeed);
            PSaveNames.Add("ResetSeedEnabled", chkResetSeed);
            PSaveNames.Add("ResetSeedMode", rdbResetSeedBets);

            PSaveNames.Add("ResetSeedValue",nudResetSeed );
            PSaveNames.Add("QuickSwitchFolder", txtQuickSwitch);
            PSaveNames.Add("SettingsMode", null);
            PSaveNames.Add("Site", null);
            PSaveNames.Add("QuickSwitch", txtQuickSwitch);
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (CurrentSite is moneypot)
            {
                (CurrentSite as moneypot).ShowMPDeposit();
                
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            Process.Start(CurrentSite.SiteURL);
        }

        private void btnWithdrawAlt_Click(object sender, EventArgs e)
        {
            SimpleSwap tmp = new SimpleSwap(ExchangeType.withdraw, CurrentSite.Currency);
            tmp.Withdraw += Tmp_Withdraw;
            tmp.Show();
        }

        private void Tmp_Withdraw(string Address, decimal Amount)
        {
            CurrentSite.Withdraw(Amount, Address);
        }

        private void btnMPWithdraw_Click(object sender, EventArgs e)
        {
            if (CurrentSite is moneypot)
            {
                (CurrentSite as moneypot).ShowMPWithdraw();

            }
        }

        private void btnDepositAlt_Click(object sender, EventArgs e)
        {
            SimpleSwap tmp = new SimpleSwap(ExchangeType.deposit, CurrentSite.Currency, txtApiAddress.Text);
            tmp.Show();
        }

        private void lbVariables_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        int LogLevel = 0;
        public void DumpLog(string Message, int Level)
        {
            if (Message!=null)
            {
                if (Level<=LogLevel)
                {
                    try
                    {
                        using (StreamWriter SW = File.AppendText("DICEBOTLOG.txt"))
                        {
                            SW.WriteLine(Message);
                        }
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }
        }

        private void frequentlyAskedQuestionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://bot.seuntjie.com/faqs.aspx");
        }

        private void seedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SeedInput().Show();
        }
        
    }
}
