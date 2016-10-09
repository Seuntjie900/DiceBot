
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Http;

namespace DiceBot
{
    public class WD : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        
        DateTime lastupdate = new DateTime();
        RandomNumberGenerator R = new System.Security.Cryptography.RNGCryptoServiceProvider();
        public static string[] cCurrencies = new string[2] { "btc", "cj" };
        string actualcur = "btc";
        public WD(cDiceBot Parent)
        {
            NonceBased = false;
            Currency = "btc";
            register = false;
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = false;
            BetURL = "https://wealthydice.com/api/bet/id?=";
            this.Parent = Parent;
            Name = "BetterBets";
            Tip = true;
            TipUsingName = true;
            SiteURL = "https://wealthydice.com/?ref=62";
            Currencies = new string[2] { "btc", "cj" };
            

        }

        protected override void CurrencyChanged()
    {
        actualcur = Currency == "btc" ? Currency : "rbs";
            try
            {
                if (accesstoken != "")
                {
                    lastupdate = DateTime.Now;

                    string s1 = "user?accessToken=" + accesstoken + "&coin=" + actualcur;
                    string s = Client.GetStringAsync(s1).Result;
                    bbStats tmpu = json.JsonDeserialize<bbStats>(s);
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


                }
            }
            catch
            { }
        }
        HttpClientHandler ClientHandlr;// = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip };;
        HttpClient Client;
        void GetBalanceThread()
        {
            
                while (ispd)
                {
                    try
                    {
                        if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60||ForceUpdateStats))
                        {
                            lastupdate = DateTime.Now;
                            string s1 = "user?accessToken=" + accesstoken+"&coin="+actualcur;                    
                            string s = Client.GetStringAsync(s1).Result;
                            bbStats tmpu = json.JsonDeserialize<bbStats>(s);
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
                        }
                    }
                    catch
                    {
                            
                   }
                    Thread.Sleep(1000);
            }
           
        }

        public override bool Register(string Username, string Password)
        {

            if (System.Windows.Forms.MessageBox.Show("Unfortunately DiceBot cannot register new users at wealthydice.com. Want to open the page now?", "Register", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://wealthydice.com/?ref=1301492");
            }

            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {

            ClientHandlr = new HttpClientHandler()
            {
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = (IWebProxy)this.Prox,
                UseProxy = this.Prox != null
            };
            Client = new HttpClient(ClientHandlr) { BaseAddress= new Uri("https://wealthydice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            lastupdate = DateTime.Now;
            this.username = Username;
                            this.accesstoken = Password;
            try
            {
                if (accesstoken != "" )
                    {
                    string s1 = "user?accessToken=" + accesstoken+"&coin="+actualcur;
                    try
                    {
                        string s = Client.GetStringAsync(s1).Result;

                        bbStats tmpu = json.JsonDeserialize<bbStats>(s);
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
                            Thread t = new Thread(GetBalanceThread);
                            ispd = true;
                            t.Start();
                            
                            finishedlogin(true);
                            return;
                        }
                        else
                        {
                            finishedlogin(false);
                            return;
                        }
                    }
                        catch (AggregateException e)
                    {

                    }
                    catch (Exception e)
                    {
                        finishedlogin(false);
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
        int retrycount = 0;
        void placebetthread(object BetObj)
        {
            try
            {
                PlaceBetObj tmp9 = BetObj as PlaceBetObj;
                bool High = tmp9.High;
                decimal amount = tmp9.Amount;
                decimal chance = tmp9.Chance;
                byte[] bytes = new byte[4];
                R.GetBytes(bytes);
                long client = (long)BitConverter.ToUInt32(bytes, 0);
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("accessToken", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("wager", amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("chance", chance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("direction", High ? "1" : "0"));
                pairs.Add(new KeyValuePair<string, string>("coin", Currency));
                pairs.Add(new KeyValuePair<string, string>("clientSeed", client.ToString()));
                R.GetBytes(bytes);
                client = (long)BitConverter.ToUInt32(bytes, 0);
                pairs.Add(new KeyValuePair<string, string>("clientSeedNext", client.ToString()));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("betDice/", Content))
                {
                    while (!response.IsCompleted)
                        Thread.Sleep(100);
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (retrycount++ < 3)
                        {
                            placebetthread(new PlaceBetObj(High, amount, chance));
                            return;
                        }
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            placebetthread(new PlaceBetObj(High, amount, chance));
                            return;
                        }
                    }
                }

                bbResult tmp = json.JsonDeserialize<bbResult>(responseData);

                if (tmp.error != 1)
                {
                    next = tmp.nextServerSeed;
                    lastupdate = DateTime.Now;
                    balance = tmp.balance;
                    bets++;
                    if (tmp.win == 1)
                        wins++;
                    else losses++;

                    wagered += (tmp.wager);
                    profit += tmp.profit;


                    Bet tmp2 = tmp.toBet();
                    tmp2.date = DateTime.Now;
                    tmp2.serverhash = next;
                    next = tmp.nextServerSeed;

                    FinishedBet(tmp2);
                    retrycount = 0;
                }
                else
                {
                    Parent.updateStatus("An error has occured! Betting has stopped for your safety.");
                }
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
                    Thread.Sleep(200);
                    placebetthread(new PlaceBetObj(High, amount, chance));
                }


            }
            catch (Exception e)
            {

            }
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            this.High = High;
            new Thread(new ParameterizedThreadStart(placebetthread)).Start(new PlaceBetObj(High, amount, chance));
        }

       
        public override void ResetSeed()
        {
            if ((DateTime.Now - LastSeedReset).TotalSeconds>90)
            {
                try
                {
                    LastSeedReset = DateTime.Now;
                    Parent.updateStatus("Resetting Seed");
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("accessToken", accesstoken));
                    pairs.Add(new KeyValuePair<string, string>("seed", amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                    pairs.Add(new KeyValuePair<string, string>("coin", actualcur));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string responseData = "";
                    using (var response = Client.PostAsync("seed/", Content))
                    {
                        try
                        {
                            responseData = response.Result.Content.ReadAsStringAsync().Result;
                        }
                        catch (AggregateException e)
                        {
                            if (e.InnerException.Message.Contains("ssl"))
                            {
                                ResetSeed();
                                return;
                            }
                        }
                    }
                    
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

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            return false;
        }

        public override decimal GetLucky(string server, string client, int nonce)
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

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }
        new public static decimal sGetLucky(string server, string client, int nonce)
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

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
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
            try
            {
                string s = Client.GetStringAsync("depositAddress?accessToken=" + accesstoken + "&coin=" +actualcur).Result;
                PRCDepost tmp = json.JsonDeserialize<PRCDepost>(s);
                return tmp.Address;
            }
            catch (AggregateException e)
            {
                return "";
            }
        }

        public override void Disconnect()
        {
            ispd = false;
            accesstoken = "";
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            

            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("accessToken", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("uname", User));
                pairs.Add(new KeyValuePair<string, string>("coin", actualcur));
                pairs.Add(new KeyValuePair<string, string>("amount", (amount * 100000000.0m).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("tip/", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                        return (responseData.Contains("success"));
                        
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            return SendTip(User , amount);
                            
                        }
                    }
                }
                /*string post = "accessToken="+ accesstoken +"&uname=" + User+ "&amount=" + (amount * 100000000.0).ToString("");


                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://wealthydice.com/api/api/tip");
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
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    return false;
                }
            }
            return false;
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

    /*public class bbResult
    {
        public int error { get; set; }
        public int win { get; set; }
        public decimal balanceOrig { get; set; }
        public decimal balance { get; set; }
        public decimal profit { get; set; }
        public int lfNotified { get; set; }
        public int lfActive { get; set; }
        public decimal lfMaxBetAmt { get; set; }
        public decimal lfMaturityPercent { get; set; }
        public decimal lfActivePercent { get; set; }
        public decimal version { get; set; }
        public decimal maintenance { get; set; }
        public int happyHour { get; set; }
        public int direction { get; set; }
        public decimal wager { get; set; }
        public decimal target { get; set; }
        public decimal result { get; set; }
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
                
                clientseed = clientSeed.ToString(),
                serverseed = serverSeed,
                Id=betId
            };

            tmp.Chance = tmp.high ? 99.99m - (decimal)target : (decimal)target;

            return tmp;
        }
    }

        public class bbTip
        {
            public int error { get; set; }
            public decimal balance { get; set; }
            public decimal version { get; set; }
            public int maintenance { get; set; }
            public int happyHour { get; set; }
        }

        public class bbStats
        {
            public int error { get; set; }
            public int id { get; set; }
            public decimal balance { get; set; }
            public string alias { get; set; }

            public int clientseed { get; set; }
            public int client_seed_sequence { get; set; }
            public string server_seed { get; set; }
            public int total_bets { get; set; }
            public decimal total_wagered { get; set; }
            public int total_wins { get; set; }
            public decimal total_profit { get; set; }
            
            
        }
    public class bbSeed
    {
        public int newSeed { get; set; }
    }
    public class bbdeposit
    {
        public string deposit_address { get; set; }
    }*/
    
}
    