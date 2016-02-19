using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Http;

namespace DiceBotCore.Sites
{
    class BetterBets:DiceSiteBase
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool isbb = true;
        string username = "";
        DateTime lastupdate = new DateTime();
        Random R = new Random();
        HttpClientHandler ClientHandlr;// = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip };;
        HttpClient Client;

        BetterBets()
        {
            CanRegister = false;
            Currencies = new string[2] {"btc","rbs" };
            Currency = 1;
            MaxRoll = 99.99;
            Edge = 1;
            SiteURL = "https://betterbets.io/?ref=1701";
            AutoWithdraw = false;
            AutoInvest = false;
            CanTip = true;
            TipUsingName = true;
            SiteName = "BetterBets";
            SiteAbbreviation = "BB";
            BetURL = "https://betterbets.io/api/bet/?id={0}";
            CanGetSeed = false;
            GettingSeed = false;
            CanVerify = true;
            CanChat = false;
            CanChangeSeed = true;
            CanSetClientSeed = true;

        }

        int RetryCount = 0;
        string next = "";
        protected override void _PlaceBet(double Amount, double Chance, bool High)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("accessToken", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("wager", Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("chance", Chance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("direction", High ? "1" : "0"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("betDice/", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (RetryCount++ < 3)
                        {
                            _PlaceBet(Amount, Chance, High);
                            return;
                        }
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            _PlaceBet(Amount, Chance, High);
                            return;
                        }
                    }
                }

                bbResult tmp = Helpers.json.JsonDeserialize<bbResult>(responseData);
                if (tmp.error != 1)
                {
                    next = tmp.nextServerSeed;
                    lastupdate = DateTime.Now;
                    Stats.Balance = tmp.balance;
                    Stats.bets++;
                    if (tmp.win == 1)
                        Stats.wins++;
                    else Stats.losses++;

                    Stats.Wagered += (tmp.wager);
                    Stats.Profit += tmp.profit;


                    Bet tmp2 = tmp.toBet();
                    tmp2.ServerHash = next;
                    next = tmp.nextServerSeed;

                    callBetFinished(tmp2);
                    RetryCount = 0;
                }
                else
                {
                    ////Parent.updateStatus("An error has occured! Betting has stopped for your safety.");
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    ////Parent.updateStatus(sEmitResponse);
                }
                if (e.Message.Contains("429") || e.Message.Contains("502"))
                {
                    Thread.Sleep(200);
                    _PlaceBet(Amount, Chance, High);
                }


            }
            catch (Exception e)
            {

            }
        }

        protected override bool _Login(string Username, string Password, string TFA)
        {
            ClientHandlr = new HttpClientHandler() { UseCookies = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://betterbets.io/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            lastupdate = DateTime.Now;
            this.username = Username;
            this.accesstoken = Password;
            try
            {
                if (accesstoken != "")
                {
                    string s1 = "user?accessToken=" + accesstoken;
                    try
                    {
                        string s = Client.GetStringAsync(s1).Result;

                        bbStats tmpu = Helpers.json.JsonDeserialize<bbStats>(s);
                        if (tmpu.error != 1)
                        {
                            this.Stats = new SiteStats { Balance=tmpu.balance, bets=tmpu.total_bets, wins=tmpu.total_wins, losses=tmpu.total_bets-tmpu.total_wins, Profit=tmpu.total_profit, Wagered=tmpu.total_wagered };
                            /*balance = tmpu.balance; //i assume
                            bets = tmpu.total_bets;
                            wagered = tmpu.total_wagered;
                            profit = tmpu.total_profit;
                            wins = tmpu.total_wins;
                            losses = bets - losses;*/
                            //Parent.updateBalance((decimal)(balance));
                            //Parent.updateBets(bets);
                            //Parent.updateLosses(losses);
                            //Parent.updateProfit(profit);
                            //Parent.updateWagered(wagered);
                            //Parent.updateWins(wins);
                            lastupdate = DateTime.Now;
                            //getDepositAddress();

                            
                            return true;
                        }
                        else
                        {
                            
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        
                        return false;
                    }

                }

            }

            catch (WebException e)
            {
                if (e.Response != null)
                {



                }
                return (false);
                
            }
            return false;
        }

        protected override void _Disconnect()
        {
            isbb = false;
            accesstoken = "";
            username = "";
            Stats = new SiteStats();
        }

        public override void SetProxy(Helpers.ProxyDetails ProxyInfo)
        {
            throw new NotImplementedException();
        }

        protected override void _UpdateStats()
        {
            if (accesstoken != "" && (DateTime.Now - lastupdate).TotalSeconds > 60)
            {
                lastupdate = DateTime.Now;
                string s = Client.GetAsync(new Uri("user?accessToken=" + accesstoken)).Result.RequestMessage.Content.ReadAsStringAsync().Result;
                bbStats tmpu = Helpers.json.JsonDeserialize<bbStats>(s);
                this.Stats.Balance = tmpu.balance; //i assume
                this.Stats.bets = tmpu.total_bets;
                this.Stats.Wagered = tmpu.total_wagered;
                this.Stats.Profit = tmpu.total_profit;
                this.Stats.wins = tmpu.total_wins;
                this.Stats.losses = this.Stats.bets - this.Stats.wins;
            }
        }

        void GetBalanceThread()
        {
            try
            {
                while (isbb)
                {
                    if (accesstoken != "" && (DateTime.Now - lastupdate).TotalSeconds > 60)
                        UpdateStats();
                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
        }

        string customSeed = "";
        protected override void _ResetSeed()
        {
            if ((DateTime.Now - LastSeedReset).TotalSeconds > 90)
            {
                try
                {
                    string tseed = R.Next(0, int.MaxValue).ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (customSeed!=null && customSeed!= "")
                    {
                        tseed = customSeed;
                    }
                    LastSeedReset = DateTime.Now;
                    ////Parent.updateStatus("Resetting Seed");
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("accessToken", accesstoken));
                    pairs.Add(new KeyValuePair<string, string>("seed", tseed));
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
                                _ResetSeed();
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
                        //Parent.updateStatus(sEmitResponse);
                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(2000);
                            _ResetSeed();
                        }
                    }
                }
            }
            else
            {
                //Parent.updateStatus("Too soon to reset seed. Delaying reset.");
            }

        }

        protected override void _SetClientSeed(string ClientSeed)
        {
            customSeed = ClientSeed;
        }

        protected override double _GetLucky(string Hash, string ServerSeed, string ClientSeed, int Nonce)
        {
            string serverhash = Helpers.Hash.SHA256(ServerSeed);

            int matchash = serverhash.Equals(Hash)?1:-1;
            int charstouse = 5;
            string msg = ClientSeed + "-" + Nonce.ToString();            
            string hash = Helpers.Hash.HMAC512(msg, ServerSeed);
            for (int i = 0; i < hash.Length; i += charstouse)
            {

                string s = hash.Substring(i, charstouse);

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return matchash*( lucky / 10000.0);
            }
            return int.MaxValue;
        }

        protected override void _SendTip(string Username, double Amount)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("accessToken", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("uname", Username));
                pairs.Add(new KeyValuePair<string, string>("amount", (Amount * 100000000.0).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("tip/", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            SendTip(Username, Amount);
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
                    //Parent.updateStatus(sEmitResponse);

                }
            }
        }

        public string getDepositAddress()
        {
            try
            {
                string s = Client.GetStringAsync("depositAddress?accessToken=" + accesstoken).Result;
                bbDeposit tmp = Helpers.json.JsonDeserialize<bbDeposit>(s);
                return tmp.Address;
            }
            catch (AggregateException e)
            {
                return "";
            }
        }
    }
    public class bbDeposit
    {
        public bool Success { get; set; }
        public string Address { get; set; }
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
            Bet tmp = new Bet
            {
                Amount = wager,
                Date = DateTime.Now,
                Profit = profit,
                Roll = result,
                High = direction == 1,

                ClientSeed = clientSeed.ToString(),
                ServerSeed = serverSeed,
                ID = betId
            };

            tmp.Chance = tmp.High ? 99.99 - target : target;

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
