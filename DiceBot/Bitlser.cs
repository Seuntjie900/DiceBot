using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class Bitsler:DiceSite
    {
        bool IsBitsler = false;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        public static string[] sCurrencies = new string[] { "btc", "ltc", "doge", "burst", "dash", "zec", "bch" };
        HttpClientHandler ClientHandlr;
        
        public Bitsler(cDiceBot Parent)
        {
            _MFAText = "API Key";
            ShowXtra = true;
            

            Currencies = sCurrencies;
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://www.bitsler.com/?ref=seuntjie/";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "Bitsler";
            Tip =false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://www.bitsler.com/?ref=seuntjie";
            register = false;
            AutoUpdate = true;
        }
        void GetBalanceThread()
        {
            while (IsBitsler)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats)
                {
                    lastupdate = DateTime.Now;
                    try
                    {
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        string sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                        bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                        if (bsstatsbase != null)
                            if (bsstatsbase._return != null)
                                if (bsstatsbase._return.success == "true")
                                {
                                    switch (Currency.ToLower())
                                    {
                                        case "btc": balance = bsstatsbase._return.btc_balance;
                                            profit = bsstatsbase._return.btc_profit;
                                            wagered = bsstatsbase._return.btc_wagered; break;
                                        case "ltc": balance = bsstatsbase._return.ltc_balance;
                                            profit = bsstatsbase._return.ltc_profit;
                                            wagered = bsstatsbase._return.ltc_wagered; break;
                                        case "doge": balance = bsstatsbase._return.doge_balance;
                                            profit = bsstatsbase._return.doge_profit;
                                            wagered = bsstatsbase._return.doge_wagered; break;
                                        case "eth": balance = bsstatsbase._return.eth_balance;
                                            profit = bsstatsbase._return.eth_profit;
                                            wagered = bsstatsbase._return.eth_wagered; break;
                                        case "burst":
                                            balance = bsstatsbase._return.burst_balance;
                                            profit = bsstatsbase._return.burst_profit;
                                            wagered = bsstatsbase._return.burst_wagered; break;
                                        case "dash":
                                            balance = bsstatsbase._return.dash_balance;
                                            profit = bsstatsbase._return.dash_profit;
                                            wagered = bsstatsbase._return.dash_wagered; break;
                                        case "zac":
                                            balance = bsstatsbase._return.zec_balance;
                                            profit = bsstatsbase._return.zec_profit;
                                            wagered = bsstatsbase._return.zec_wagered; break;
                                        case "bch":
                                            balance = bsstatsbase._return.bch_balance;
                                            profit = bsstatsbase._return.bch_profit;
                                            wagered = bsstatsbase._return.bch_wagered; break;

                                    }
                                    bets = int.Parse(bsstatsbase._return.bets);
                                    wins = int.Parse(bsstatsbase._return.wins);
                                    losses = int.Parse(bsstatsbase._return.losses);

                                    Parent.updateBalance(balance);
                                    Parent.updateBets(bets);
                                    Parent.updateLosses(losses);
                                    Parent.updateProfit(profit);
                                    Parent.updateWagered(wagered);
                                    Parent.updateWins(wins);
                                }
                                else
                                {
                                    if (bsstatsbase._return.value != null)
                                    {

                                        Parent.updateStatus(bsstatsbase._return.value);

                                    }
                                }
                    }
                    catch { }
                }
                Thread.Sleep(1000);
            }
        }

        protected override void CurrencyChanged()
        {
            base.CurrencyChanged();
            lastupdate = DateTime.Now;
            if (accesstoken != "" && IsBitsler)
            {
                try
                {
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                    bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    if (bsstatsbase != null)
                        if (bsstatsbase._return != null)
                            if (bsstatsbase._return.success == "true")
                            {
                                switch (Currency.ToLower())
                                {
                                    case "btc": balance = bsstatsbase._return.btc_balance;
                                        profit = bsstatsbase._return.btc_profit;
                                        wagered = bsstatsbase._return.btc_wagered; break;
                                    case "ltc": balance = bsstatsbase._return.ltc_balance;
                                        profit = bsstatsbase._return.ltc_profit;
                                        wagered = bsstatsbase._return.ltc_wagered; break;
                                    case "doge": balance = bsstatsbase._return.doge_balance;
                                        profit = bsstatsbase._return.doge_profit;
                                        wagered = bsstatsbase._return.doge_wagered; break;
                                    case "eth": balance = bsstatsbase._return.eth_balance;
                                        profit = bsstatsbase._return.eth_profit;
                                        wagered = bsstatsbase._return.eth_wagered; break;
                                    case "burst":
                                        balance = bsstatsbase._return.burst_balance;
                                        profit = bsstatsbase._return.burst_profit;
                                        wagered = bsstatsbase._return.burst_wagered; break;
                                    case "dash":
                                        balance = bsstatsbase._return.dash_balance;
                                        profit = bsstatsbase._return.dash_profit;
                                        wagered = bsstatsbase._return.dash_wagered; break;
                                    case "zac":
                                        balance = bsstatsbase._return.zec_balance;
                                        profit = bsstatsbase._return.zec_profit;
                                        wagered = bsstatsbase._return.zec_wagered; break;
                                    case "bch":
                                        balance = bsstatsbase._return.bch_balance;
                                        profit = bsstatsbase._return.bch_profit;
                                        wagered = bsstatsbase._return.bch_wagered; break;
                                }
                                bets = int.Parse(bsstatsbase._return.bets);
                                wins = int.Parse(bsstatsbase._return.wins);
                                losses = int.Parse(bsstatsbase._return.losses);

                                Parent.updateBalance(balance);
                                Parent.updateBets(bets);
                                Parent.updateLosses(losses);
                                Parent.updateProfit(profit);
                                Parent.updateWagered(wagered);
                                Parent.updateWins(wins);
                            }
                            else
                            {
                                if (bsstatsbase._return.value != null)
                                {

                                    Parent.updateStatus(bsstatsbase._return.value);
                                    
                                }
                            }
                }
                catch { }
            }
        }

        void PlaceBetThread(object BetObj)
        {
            try
            {
                
                PlaceBetObj tmpob = BetObj as PlaceBetObj;
                LastBetAmount = (double)tmpob.Amount;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                /*access_token
type:dice
amount:0.00000001
condition:< or >
game:49.5
devise:btc*/
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("type", "dice"));
                pairs.Add(new KeyValuePair<string, string>("amount", tmpob.Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("condition", tmpob.High?">":"<"));
                pairs.Add(new KeyValuePair<string, string>("game", !tmpob.High ? tmpob.Chance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) : (maxRoll - tmpob.Chance).ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("devise", Currency));
                pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpmsg = Client.PostAsync("bet", Content).Result;
                string sEmitResponse = tmpmsg.Content.ReadAsStringAsync().Result;
                bsBetBase bsbase = null;
                try
                {
                    bsbase = json.JsonDeserialize<bsBetBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                }
                catch (Exception e)
                {

                }
                
                if (bsbase!=null)
                    if (bsbase._return!=null)
                        if (bsbase._return.success == "true")
                        {
                            balance = decimal.Parse(bsbase._return.new_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                            lastupdate = DateTime.Now;
                            Bet tmp = bsbase._return.ToBet();
                            tmp.Guid = tmpob.Guid;
                            profit += (decimal)tmp.Profit;
                            wagered += (decimal)tmp.Amount;
                            tmp.date = DateTime.Now;
                            bool win = false;
                            if ((tmp.Roll > 99.99m - tmp.Chance && tmp.high) || (tmp.Roll < tmp.Chance && !tmp.high))
                            {
                                win = true;
                            }
                            //set win
                            if (win)
                                wins++;
                            else
                                losses++;
                            bets++;
                            LastBetAmount = (double)tmpob.Amount;
                            LastBet = DateTime.Now;
                            FinishedBet(tmp);
                            return;
                        }
                        else
                        {
                            if (bsbase._return.value != null)
                            {
                                if (bsbase._return.value.Contains("Bet in progress, please wait few seconds and retry."))
                                {
                                    Parent.updateStatus("Bet in progress. You need to log in with your browser and place a bet manually to fix this.");
                                }
                                else
                                {
                                    Parent.updateStatus(bsbase._return.value);
                                }
                            }
                        }               
                //

            }
            catch (AggregateException e)
            {
                Parent.updateStatus("An Unknown error has ocurred.");
            }
            catch (Exception e)
            {
                Parent.updateStatus("An Unknown error has ocurred.");
            }
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            System.Threading.Thread tBetThread = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            tBetThread.Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        Random R = new Random();
        DateTime LastReset = new DateTime();
        public override void ResetSeed()
        {
            //Just wanted to test if this works. It doesn't. Will work with the bitsler team to
            //expand functionality in the future.
            Thread.Sleep(100);
            try
            {
                if ((DateTime.Now - LastReset).TotalMinutes >= 3)
                {
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    pairs.Add(new KeyValuePair<string, string>("username", username));
                    pairs.Add(new KeyValuePair<string, string>("seed_client", R.Next(0, int.MaxValue).ToString()));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("change-seeds", Content).Result.Content.ReadAsStringAsync().Result;
                    bsResetSeedBase bsbase = json.JsonDeserialize<bsResetSeedBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    //sqlite_helper.InsertSeed(bsbase._return.last_seeds_revealed.seed_server_hashed, bsbase._return.last_seeds_revealed.seed_server_revealed);
                    sqlite_helper.InsertSeed(bsbase._return.last_seeds_revealed.seed_server, bsbase._return.last_seeds_revealed.seed_server_revealed);
                }
                else
                {
                    Parent.updateStatus("Too soon to update seed.");
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
            catch
            {
                Parent.updateStatus("Too soon to update seed.");
            }
            Thread.Sleep(51);
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            string error = "";
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitsler.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "DiceBot");

            try
            {
                string actual2fa = "";
                if (twofa.Contains("&"))
                {
                    string[] pars = twofa.Split('&');
                    actual2fa = pars[1];
                    twofa = pars[0];
                }
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                if (!string.IsNullOrWhiteSpace(actual2fa))
                {
                    pairs.Add(new KeyValuePair<string, string>("twofactor", actual2fa));
                }
                pairs.Add(new KeyValuePair<string, string>("api_key", twofa));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpresp = Client.PostAsync("login", Content).Result;

                byte[] bytes = tmpresp.Content.ReadAsByteArrayAsync().Result;
                string sEmitResponse = tmpresp.Content.ReadAsStringAsync().Result;
                
                //getuserstats 
                bsloginbase bsbase = json.JsonDeserialize<bsloginbase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                
                if (bsbase!=null)
                    if (bsbase._return!=null)
                        if (bsbase._return.success=="true")
                        {
                            accesstoken = bsbase._return.access_token;
                            IsBitsler = true;
                            lastupdate = DateTime.Now;

                            
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                            bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                            if (bsstatsbase != null)
                                if (bsstatsbase._return != null)
                                    if (bsstatsbase._return.success == "true")
                                    {
                                        switch (Currency.ToLower())
                                        {
                                            case "btc": balance = bsstatsbase._return.btc_balance;
                                                profit = bsstatsbase._return.btc_profit;
                                                wagered = bsstatsbase._return.btc_wagered; break;
                                            case "ltc": balance = bsstatsbase._return.ltc_balance;
                                                profit = bsstatsbase._return.ltc_profit;
                                                wagered = bsstatsbase._return.ltc_wagered; break;
                                            case "doge": balance = bsstatsbase._return.doge_balance;
                                                profit = bsstatsbase._return.doge_profit;
                                                wagered = bsstatsbase._return.doge_wagered; break;
                                            case "eth": balance = bsstatsbase._return.eth_balance;
                                                profit = bsstatsbase._return.eth_profit;
                                                wagered = bsstatsbase._return.eth_wagered; break;
                                            case "burst":
                                                balance = bsstatsbase._return.burst_balance;
                                                profit = bsstatsbase._return.burst_profit;
                                                wagered = bsstatsbase._return.burst_wagered; break;
                                            case "dash":
                                                balance = bsstatsbase._return.dash_balance;
                                                profit = bsstatsbase._return.dash_profit;
                                                wagered = bsstatsbase._return.dash_wagered; break;
                                            case "zac":
                                                balance = bsstatsbase._return.zec_balance;
                                                profit = bsstatsbase._return.zec_profit;
                                                wagered = bsstatsbase._return.zec_wagered; break;
                                            case "bch":
                                                balance = bsstatsbase._return.bch_balance;
                                                profit = bsstatsbase._return.bch_profit;
                                                wagered = bsstatsbase._return.bch_wagered; break;
                                        }
                                        bets = int.Parse(bsstatsbase._return.bets==null?"0": bsstatsbase._return.bets);
                                        wins = int.Parse(bsstatsbase._return.wins == null ? "0" : bsstatsbase._return.wins);
                                        losses = int.Parse(bsstatsbase._return.losses == null ? "0" : bsstatsbase._return.losses);

                                        Parent.updateBalance(balance);
                                        Parent.updateBets(bets);
                                        Parent.updateLosses(losses);
                                        Parent.updateProfit(profit);
                                        Parent.updateWagered(wagered);
                                        Parent.updateWins(wins);
                                        this.username = Username;
                                    }
                                    else
                                    {
                                        if (bsstatsbase._return.value != null)
                                        {

                                            Parent.updateStatus(bsstatsbase._return.value);

                                        }
                                    }
                            
                            
                            IsBitsler = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            finishedlogin(true);
                            return;
                        }
                        else
                        {
                            if (bsbase._return.value != null)
                                Parent.updateStatus(bsbase._return.value);
                        }

            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
            }
            finishedlogin(false);
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }
        DateTime LastBet = DateTime.Now;
        double LastBetAmount = 0;
        public override bool ReadyToBet()
        {
            //return true;

            int type_delay=0;
            
            if (Currency.ToLower() == "btc") {
                if (LastBetAmount<= 0.00000005|| (double)amount <= 0.00000005 )
                    type_delay = 1;
                else 
                    type_delay = 2;
            }
            else if (Currency.ToLower() == "eth")
            {
                if (LastBetAmount<= 0.00000250|| (double)amount <= 0.00000250)
                    type_delay = 1;
                else 
                    type_delay = 2;
               
            }
            else if (Currency.ToLower() == "ltc")
            {
                if (LastBetAmount<= 0.00001000 || (double)amount <= 0.00001000)
                    type_delay = 1;
                else 
                    type_delay = 2;
                
            }
            else if (Currency.ToLower() == "doge")
            {
                if (LastBetAmount<= 5.00000000 || (double)amount <= 5.00000000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "burst")
            {
                if (LastBetAmount <= 5.00000000 || (double)amount <= 5.00000000)
                    type_delay = 1;
                else 
                    type_delay = 2;
                
            }
            else if (Currency.ToLower() == "bch")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "dash")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "zec")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            int delay = 0;
            if (type_delay == 1)
                delay = 300;
            else if (type_delay == 2)
                delay = 200;            
            else
                delay = 200;

            return (DateTime.Now - LastBet).TotalMilliseconds > delay;
        }

        public override void Disconnect()
        {
            IsBitsler = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public static decimal sGetLucky(string server, string client, int nonce)
        {
            SHA1 betgenerator = SHA1.Create();
            string Seed = server + "-" + client + "-" + nonce;
            byte[] serverb = new byte[Seed.Length];

            for (int i = 0; i < Seed.Length; i++)
            {
                serverb[i] = Convert.ToByte(Seed[i]);
            }
            decimal Lucky = 0;
            do
            {
                serverb = betgenerator.ComputeHash(serverb.ToArray());
                StringBuilder hex = new StringBuilder(serverb.Length * 2);
                foreach (byte b in serverb)
                    hex.AppendFormat("{0:x2}", b);

                string s = hex.ToString().Substring(0, 8);
                Lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            } while (Lucky > 4294960000);
            Lucky = (Lucky % 10000.0m) / 100.0m;
            if (Lucky < 0)
                return -Lucky;
            return Lucky;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            
            SHA1 betgenerator = SHA1.Create();
            string Seed = server+"-"+client+"-"+nonce;
            byte[] serverb = new byte[Seed.Length];

            for (int i = 0; i < Seed.Length; i++)
            {
                serverb[i] = Convert.ToByte(Seed[i]);
            }
            decimal Lucky = 0;
            do
            {
                serverb = betgenerator.ComputeHash(serverb.ToArray());
                StringBuilder hex = new StringBuilder(serverb.Length * 2);
                foreach (byte b in serverb)
                    hex.AppendFormat("{0:x2}", b);

                string s = hex.ToString().Substring(0, 8);
                Lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            } while (Lucky > 4294960000);
            Lucky = (Lucky % 10000.0m) / 100.0m;
            if (Lucky < 0)
                return -Lucky;
            return Lucky;
            /*
            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + client + ":" + nonce.ToString();
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
            }*/
            return 0;
        }
    }

    public class bsLogin
    {
        public string success { get; set; }
        public string value { get; set; }
        public string access_token { get; set; }
    }
    public class bsloginbase
    {
        public bsLogin _return { get; set; }
    }
    //"{\"return\":{\"success\":\"true\",\"balance\":1.0e-5,\"wagered\":0,\"profit\":0,\"bets\":\"0\",\"wins\":\"0\",\"losses\":\"0\"}}"
    //{"return":{"success":"true","bets":0,"wins":0,"losses":0,"btc_profit":"0.00000000","btc_wagered":"0.00000000","btc_balance":"0.00000000","eth_profit":"0.00000000","eth_wagered":"0.00000000","eth_balance":"0.00000000","ltc_profit":"0.00000000","ltc_wagered":"0.00000000","ltc_balance":"0.00000000","bch_profit":"0.00000000","bch_wagered":"0.00000000","bch_balance":"0.00000000","doge_profit":"0.00000000","doge_wagered":"0.00000000","doge_balance":"0.00000000","dash_profit":"0.00000000","dash_wagered":"0.00000000","dash_balance":"0.00000000","zec_profit":"0.00000000","zec_wagered":"0.00000000","zec_balance":"0.00000000","burst_profit":"0.00000000","burst_wagered":"0.00000000","burst_balance":"0.00000000"}}
    public class bsStats
    {
        public string success { get; set; }
        public string value { get; set; }
        public decimal btc_balance { get; set; }
        public decimal btc_wagered { get; set; }
        public decimal btc_profit { get; set; }
        public string bets { get; set; }
        public decimal ltc_balance { get; set; }
        public decimal ltc_wagered { get; set; }
        public decimal ltc_profit { get; set; }

        public decimal doge_balance { get; set; }
        public decimal doge_wagered { get; set; }
        public decimal doge_profit { get; set; }
        public decimal eth_balance { get; set; }
        public decimal eth_wagered { get; set; }
        public decimal eth_profit { get; set; }
        public decimal burst_balance { get; set; }
        public decimal burst_wagered { get; set; }
        public decimal zec_profit { get; set; }
        public decimal zec_balance { get; set; }
        public decimal zec_wagered { get; set; }
        public decimal bch_profit { get; set; }
        public decimal bch_balance { get; set; }
        public decimal bch_wagered { get; set; }
        public decimal dash_profit { get; set; }
        public decimal dash_balance { get; set; }
        public decimal dash_wagered { get; set; }
        public decimal burst_profit { get; set; }
        public string wins { get; set; }
        public string losses { get; set; }
    }
    public class bsStatsBase
    {
        public bsStats _return { get; set; }
    }
    public class bsBetBase
    {
        public bsBet _return { get; set; }
    }
    public class bsBet
    {
        public string success { get; set; }
        public string value { get; set; }
        public string username { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string devise { get; set; }
        public long ts { get; set; }
        public string time { get; set; }
        public string amount { get; set; }
        public decimal roll_number { get; set; }
        public string condition { get; set; }
        public string game { get; set; }
        public decimal payout { get; set; }
        public string winning_chance { get; set; }
        public string amount_return { get; set; }
        public string new_balance { get; set; }
        public string _event { get; set; }
        public string server_seed { get; set; }
        public string client_seed { get; set; }
        public long nonce { get; set; }

        public Bet ToBet()
        {
            Bet tmp = new Bet
            {
                Amount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                date = json.ToDateTime2(ts.ToString()),
                Id = id,
                Profit = decimal.Parse(amount_return, System.Globalization.NumberFormatInfo.InvariantInfo),
                Roll = (decimal)roll_number,
                high = condition == ">",
                Chance = decimal.Parse(winning_chance, System.Globalization.NumberFormatInfo.InvariantInfo),
                nonce = nonce,
                serverhash = server_seed,
                clientseed = client_seed                
            };
            return tmp;
        }
    }
    public class bsResetSeedBase
    {
        public bsResetSeed _return { get; set; }
    }
    public class bsResetSeed
    {
        public string seed_server_hashed { get; set; }
        public string seed_server { get; set; }
        public string seed_client { get; set; }
        public string nonce { get; set; }
        public string seed_server_revealed { get; set; }
        public bsResetSeed last_seeds_revealed { get; set; }
    }

}
