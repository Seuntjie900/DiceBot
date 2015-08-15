
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading; 
using System.Security.Cryptography;

namespace DiceBot
{
    public class BB : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = true;
        string username = "";
        
        DateTime lastupdate = new DateTime();
        Random R = new Random();
        public BB(cDiceBot Parent)
        {
            maxRoll = 99.99;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = false;
            BetURL = "https://betterbets.io/api/bet/id?=";
            Thread t = new Thread(GetBalanceThread);
            t.Start();
            this.Parent = Parent;
            Name = "BetterBets";
            Tip = true;
            TipUsingName = true;
            SiteURL = "https://betterbets.io/?ref=1301492";

        }

        
        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (accesstoken != "" && (DateTime.Now - lastupdate).TotalSeconds > 60)
                    {
                        HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://betterbets.io/api/user?accessToken=" + accesstoken);
                        if (Prox != null)
                            betrequest.Proxy = Prox;
                        betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                        HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                        string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();

                        bbStats tmpu = json.JsonDeserialize<bbStats>(sEmitResponse2);
                        balance = tmpu.balance; //i assume
                        bets = tmpu.total_bets;
                        wagered = tmpu.total_wagered;
                        profit = tmpu.total_profit;
                        wins = tmpu.total_wins;
                        losses = bets - losses;
                        Parent.updateBalance((decimal)(balance));
                        Parent.updateBets(bets);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered );
                        Parent.updateWins(wins);
                        lastupdate = DateTime.Now;
                        
                    }
                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
        }

        public override bool Register(string Username, string Password)
        {

            if (System.Windows.Forms.MessageBox.Show("Unfortunately DiceBot cannot register new users at betterbets.io. Want to open the page now?", "Register", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://betterbets.io/?ref=1301492");
            }

            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {
            this.username = Username;
                            this.accesstoken = Password;
            try
            {
                if (accesstoken != "" )
                    {
                        HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://betterbets.io/api/user?accessToken=" + accesstoken);
                        betrequest.Method = "GET";
                        if (Prox != null)
                            betrequest.Proxy = Prox;
                        betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                        HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                        string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                    
                        bbStats tmpu = json.JsonDeserialize<bbStats>(sEmitResponse2);
                        if (tmpu.error != 1)
                        {
                            balance = tmpu.balance; //i assume
                            bets = tmpu.total_bets;
                            wagered = tmpu.total_wagered;
                            profit = tmpu.total_profit;
                            wins = tmpu.total_wins;
                            losses = bets - losses;
                            Parent.updateBalance((decimal)(balance));
                            Parent.updateBets(bets);
                            Parent.updateLosses(losses);
                            Parent.updateProfit(profit);
                            Parent.updateWagered(wagered);
                            Parent.updateWins(wins);
                            lastupdate = DateTime.Now;
                            getDepositAddress();
                            
                            finishedlogin(true);
                            return;
                        }
                    }
                    
                }
            
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    
                    
                }
                finishedlogin(false);
                return;
            }
            finishedlogin(false);
        }
        string next = "";
        void placebetthread()
        {
            try
            {

                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://betterbets.io/api/betDice/");//?"+string.Format("accessToken={3}&wager={0:0.00000000}&chance={1}&direction={2}", (amount), chance.ToString("0.00"), High ? "1" : "0", accesstoken));
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.Method = "POST";
                double tmpchance = High ? 99.99 - chance : chance;
                string post = string.Format("accessToken={3}&wager={0:0.00000000}&chance={1}&direction={2}", (amount), chance.ToString("0.00"), High ? "1" : "0", accesstoken);
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                bbResult tmp = json.JsonDeserialize<bbResult>(sEmitResponse);
                
                
                next = tmp.nextServerSeed;
                lastupdate = DateTime.Now;
                balance = tmp.balance;
                bets++;
                if (tmp.win == 1)
                    wins++;
                else losses++;
                
                wagered = (tmp.wager);
                profit = tmp.profit;
                Bet tmp2 = tmp.toBet();
                tmp2.serverhash = next;
                next = tmp.nextServerSeed;
                
                FinishedBet(tmp2);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                }
                if (e.Message.Contains("429") || e.Message.Contains("502"))
                {
                    Thread .Sleep(200);
                    placebetthread();
                }
                

            }
        }

        protected override void internalPlaceBet(bool High)
        {
            this.High = High;
            new Thread(placebetthread).Start();
        }

       
        public override void ResetSeed()
        {
            if ((DateTime.Now - LastSeedReset).TotalSeconds>90)
            {
                try
                {
                    LastSeedReset = DateTime.Now;
                    Parent.updateStatus("Resetting Seed");
                    HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://betterbets.io/api/seed");
                    if (Prox != null)
                        betrequest.Proxy = Prox;
                    betrequest.Method = "POST";
                    string post = string.Format("AccessToken={1}&seed={0}",R.Next(0, Int32.MaxValue), accesstoken);
                    betrequest.ContentLength = post.Length;
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                    {

                        writer.Write(post);
                    }
                    HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {

                        string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                        Parent.updateStatus(sEmitResponse);
                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(2000);
                            ResetSeed();
                        }
                    }
                }
            }
            else
            {
                Parent.updateStatus("Too soon to reset seed. Delaying reset.");
            }
            

        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

      

       
        public override bool ReadyToBet()
        {
            return true;
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            return false;
        }

        public override double GetLucky(string server, string client, int nonce)
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
            string msg = client + "-" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }
        new public static double sGetLucky(string server, string client, int nonce)
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
            string msg = client + "-" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                {
                    lucky %= 10000;
                    return lucky / 100;

                }
            }
            return 0;
        }

        public string getDepositAddress()
        {
            return "";
            HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://betterbets.io/api/depositAddress?accessToken=" + accesstoken);
            if (Prox != null)
                betrequest.Proxy = Prox;
            betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
            string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
            PRCDepost tmp = json.JsonDeserialize<PRCDepost>(sEmitResponse2);
            return tmp.Address;
        }

        public override void Disconnect()
        {
            ispd = false;
            accesstoken = "";
        }

        public override void SendTip(string User, double amount)
        {
            

            try
            {
                string post = "accessToken="+ accesstoken +"&uname=" + User+ "&amount=" + (amount * 100000000.0).ToString("");


                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://betterbets.io/api/api/tip");
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.Method = "POST";

                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    
                }
            }
        }

        void GetRollThread(object _BetID)
        {
            
            
        }

        public override void GetSeed(long BetID)
        {
            GettingSeed = true;
            Thread GetSeedThread = new Thread(new ParameterizedThreadStart(GetRollThread));
            GetSeedThread.Start(BetID);
            //GetRollThread(BetID);
        }

        public override void SendChatMessage(string Message)
        {
        
        }

    }

    public class bbResult
    {
        public int error { get; set; }
        public int win { get; set; }
        public double balanceOrig { get; set; }
        public double balance { get; set; }
        public double profit { get; set; }
        public int lfNotified { get; set; }
        public int lfActive { get; set; }
        public double lfMaxBetAmt { get; set; }
        public double lfMaturityPercent { get; set; }
        public double lfActivePercent { get; set; }
        public double version { get; set; }
        public double maintenance { get; set; }
        public int happyHour { get; set; }
        public int direction { get; set; }
        public double wager { get; set; }
        public double target { get; set; }
        public double result { get; set; }
        public int clientSeed { get; set; }
        public string serverSeed { get; set; }
        public string nextServerSeed { get; set; }
        public long betId { get; set; }

        public Bet toBet()
        {
            Bet tmp = new Bet { 
                Amount = (decimal)wager,
                date = DateTime.Now,
                Profit = (decimal)profit,
                Roll = (decimal)result,
                high = direction == 1,
                Chance = (decimal)target,
                clientseed = clientSeed.ToString(),
                serverseed = serverSeed,
                Id=betId
            };

            return tmp;
        }
    }

        public class bbTip
        {
            public int error { get; set; }
            public double balance { get; set; }
            public double version { get; set; }
            public int maintenance { get; set; }
            public int happyHour { get; set; }
        }

        public class bbStats
        {
            public int error { get; set; }
            public int id { get; set; }
            public double balance { get; set; }
            public string alias { get; set; }

            public int clientseed { get; set; }
            public int client_seed_sequence { get; set; }
            public string server_seed { get; set; }
            public int total_bets { get; set; }
            public double total_wagered { get; set; }
            public int total_wins { get; set; }
            public double total_profit { get; set; }
            
            
        }
    public class bbSeed
    {
        public int newSeed { get; set; }
    }
    public class bbdeposit
    {
        public string deposit_address { get; set; }
    }
    
}
