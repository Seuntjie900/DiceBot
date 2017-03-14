
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
    public class Bitvest : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public static string[] cCurrencies = new string[2] { "Bitcoins", "Tokens" };
        
        public Bitvest(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://bitvest.io/results?game=dice&query=";

            this.Currencies = cCurrencies;
            Currency = "Bitcoins";
            this.Parent = Parent;
            Name = "Bitvest";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://bitvest.io?r=46534";
            
        }
        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }
        Random R = new Random();
        string RandomSeed()
        {
            string s = "";
            string chars = "0123456789abcdef";
            while (s.Length<20)
            {
                s += chars[R.Next(0, chars.Length)];
            }

            return s;
        }
        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 10||ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                        pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                        pairs.Add(new KeyValuePair<string, string>("k", "0"));
                        pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                        pairs.Add(new KeyValuePair<string, string>("u", "0"));
                        pairs.Add(new KeyValuePair<string, string>("self_only", "1"));

                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        string sEmitResponse = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;

                        ForceUpdateStats = false;
                        bitvestLoginBase tmpbase = json.JsonDeserialize<bitvestLoginBase>(sEmitResponse.Replace("-", "_"));
                        if (tmpbase!=null)
                        {
                            if (tmpbase.data!=null)
                            {
                                if (Currency.ToLower() == "bitcoins")
                                {
                                    balance = decimal.Parse(tmpbase.data.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }
                                else
                                {
                                    balance = decimal.Parse(tmpbase.data.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }
                                Parent.updateBalance(balance);
                            }
                        }
                        
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
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://bitvest.io") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("r", "46534"));
                
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("create.php", Content).Result.Content.ReadAsStringAsync().Result;
                bitvestLoginBase tmpbase = json.JsonDeserialize<bitvestLoginBase>(sEmitResponse.Replace("-", "_"));
                accesstoken = tmpbase.data.session_token;
                secret = tmpbase.account.secret;

                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                pairs.Add(new KeyValuePair<string, string>("k", "0"));
                pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                pairs.Add(new KeyValuePair<string, string>("u", "0"));
                //pairs.Add(new KeyValuePair<string, string>("self_only", "1"));

                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;


                tmpbase = json.JsonDeserialize<bitvestLoginBase>(sEmitResponse.Replace("-", "_"));
                accesstoken = tmpbase.data.session_token;
                //tmplogin = tmpblogin.data;

                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("act", "convert"));
                pairs.Add(new KeyValuePair<string, string>("c", "9999999"));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("password_conf", Password));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                tmpbase = json.JsonDeserialize<bitvestLoginBase>(sEmitResponse);
                if (!tmpbase.success)
                {
                    Parent.updateStatus(tmpbase.msg);
                    finishedlogin(false);
                    return false;
                }
                accesstoken = tmpbase.data.session_token;
                secret = tmpbase.account.secret;
                if (accesstoken == "")
                    return false;
                else
                {
                    bitvestLoginBase tmpblogin = tmpbase;
                    bitvestLogin tmplogin = tmpblogin.data;
                    if (Currency.ToLower() == "bitcoins")
                    {
                        balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    accesstoken = tmplogin.session_token;
                    secret = tmpblogin.account.secret;
                    wagered = 0;
                    profit = 0;
                    bets = 0;
                    wins = 0;
                    losses = 0;
                    Parent.updateBalance(balance);
                    Parent.updateWagered(wagered);
                    Parent.updateProfit(profit);
                    Parent.updateBets(bets);
                    Parent.updateWins(wins);
                    Parent.updateLosses(losses);
                    Parent.updateDeposit(tmpblogin.account.address);
                    lastupdate = DateTime.Now;
                    ispd = true;
                    pw = Password;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    lasthash = tmpblogin.server_hash;
                    this.Tip = false;
                    finishedlogin(true);
                    return true;


                }

            }
            catch (Exception e)
            {
                Parent.updateStatus(e.Message);
                finishedlogin(false); return false;
            }

            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {
            //accept-encoding:gzip, deflate,
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://bitvest.io/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            
            try
            {
                string resp = "";// Client.GetStringAsync("").Result;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("type","secret"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                resp = Client.PostAsync("https://bitvest.io/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                bitvestLoginBase tmpblogin = json.JsonDeserialize<bitvestLoginBase>(resp.Replace("-", "_"));
                bitvestLogin tmplogin = tmpblogin.data;
                secret = tmpblogin.account.secret;
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                pairs.Add(new KeyValuePair<string, string>("k", "0"));
                pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                pairs.Add(new KeyValuePair<string, string>("u", "0"));
                //pairs.Add(new KeyValuePair<string, string>("self_only", "1"));
                Content = new FormUrlEncodedContent(pairs);
                resp = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;


                 tmpblogin = json.JsonDeserialize<bitvestLoginBase>(resp.Replace("-", "_"));
                 tmplogin = tmpblogin.data;
                if (tmplogin.session_token!=null)
                {
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("username", Username));
                    pairs.Add(new KeyValuePair<string, string>("password", Password));
                    pairs.Add(new KeyValuePair<string, string>("tfa", otp));
                    pairs.Add(new KeyValuePair<string, string>("token", tmplogin.session_token));
                    //pairs.Add(new KeyValuePair<string, string>("c", "secret"));
                    pairs.Add(new KeyValuePair<string, string>("secret", secret));
                    Content = new FormUrlEncodedContent(pairs);
                    resp = Client.PostAsync("https://bitvest.io/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                    tmpblogin = json.JsonDeserialize<bitvestLoginBase>(resp.Replace("-", "_"));
                    tmplogin = tmpblogin.data;
                    if (Currency.ToLower()=="bitcoins")
                    {
                        balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo );
                    }
                    else
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    accesstoken = tmplogin.session_token;
                    secret = tmpblogin.account.secret;
                    wagered = decimal.Parse(tmplogin.self_total_bet_dice, System.Globalization.NumberFormatInfo.InvariantInfo);
                    profit = decimal.Parse(tmplogin.self_total_won_dice, System.Globalization.NumberFormatInfo.InvariantInfo);
                    bets = int.Parse(tmplogin.self_total_bets_dice.Replace(",",""), System.Globalization.NumberFormatInfo.InvariantInfo);
                    wins = 0;
                    losses = 0;
                    Parent.updateBalance(balance);
                    Parent.updateWagered(wagered);
                    Parent.updateProfit(profit);
                    Parent.updateBets(bets);
                    Parent.updateWins(wins);
                    Parent.updateLosses(losses);
                    Parent.updateDeposit(tmpblogin.account.address);
                    lastupdate = DateTime.Now;
                    ispd = true;
                    pw = Password;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    lasthash = tmpblogin.server_hash;
                    this.Tip = tmpblogin.tip.enabled;
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
                finishedlogin(false);
                return;
            }
            catch (Exception e)
            {
                finishedlogin(false);
                return;
            }
            finishedlogin(false);
        }

        int retrycount = 0;
        DateTime Lastbet = DateTime.Now;
        string secret = "";

        string lasthash = "";
        void placebetthread(object bet)
        {
            
            try
            {
                PlaceBetObj tmp5 = bet as PlaceBetObj;
                decimal amount = tmp5.Amount;
                decimal chance = tmp5.Chance;
                bool High = tmp5.High;
                
                decimal tmpchance = High ?maxRoll - chance+0.0001m : chance-0.0001m;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                string seed = RandomSeed();
                pairs.Add(new KeyValuePair<string, string>("bet", (amount).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("target", tmpchance.ToString("0.0000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("side", High ? "high" : "low"));
                pairs.Add(new KeyValuePair<string, string>("act", "play_dice"));
                pairs.Add(new KeyValuePair<string, string>("currency", Currency));
                pairs.Add(new KeyValuePair<string, string>("secret",secret));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("user_seed",seed));
                pairs.Add(new KeyValuePair<string, string>("v", "65535"));
                

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                Lastbet = DateTime.Now;
                try
                {
                    bitvestbet tmp = json.JsonDeserialize<bitvestbet>(sEmitResponse.Replace("f-", "f_").Replace("n-","n_"));
                    if (tmp.success)
                    {
                        Bet resbet = new Bet
                        {
                            Amount = tmp5.Amount,
                            date = DateTime.Now,
                            Chance = tmp5.Chance,
                            high = tmp5.High,
                            clientseed = seed,
                            serverhash = lasthash,
                            serverseed = tmp.server_seed,
                            Roll = tmp.game_result.roll,
                            Profit = tmp.game_result.win == 0 ? -tmp5.Amount : tmp.game_result.win - tmp5.Amount,
                            nonce = -1,
                            Id = tmp.game_id.ToString(),
                            Currency = Currency

                        };
                        bets++;
                        lasthash = tmp.server_hash;
                        bool Win = (((bool)High ? (decimal)tmp.game_result.roll > (decimal)maxRoll - (decimal)(chance) : (decimal)tmp.game_result.roll < (decimal)(chance)));
                        if (Win)
                            wins++;
                        else losses++;
                        wagered += amount;
                        profit += resbet.Profit;
                        balance = decimal.Parse(Currency.ToLower()=="bitcoins"?tmp.data.balance:tmp.data.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                        /*tmp.bet.client = tmp.user.client;
                        tmp.bet.serverhash = tmp.user.server;
                        lastupdate = DateTime.Now;
                        balance = tmp.user.balance / 100000000.0m; //i assume
                        bets = tmp.user.bets;
                        wins = tmp.user.wins;
                        losses = tmp.user.losses;
                        wagered = (decimal)(tmp.user.wagered / 100000000m);
                        profit = (decimal)(tmp.user.profit / 100000000m);
                        */
                        FinishedBet(resbet);
                        retrycount = 0;
                    }
                    else
                    {
                        Parent.updateStatus(tmp.msg);
                        if (tmp.msg.ToLower() == "bet rate limit exceeded")
                        {
                            Parent.updateStatus(tmp.msg+". Retrying in a second;");
                            Thread.Sleep(1000);
                            placebetthread(bet);
                            return;
                        }
                    }
                }
                catch
                {
                    Parent.updateStatus(sEmitResponse);
                }
            }
            catch (AggregateException e)
            {
                if (retrycount++ < 3)
                {
                    Thread.Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance));
                    return;
                }
                if (e.InnerException.Message.Contains("429") || e.InnerException.Message.Contains("502"))
                {
                    Thread .Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance));
                }
                

            }
            catch (Exception e2)
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
            
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

      

       
        public override bool ReadyToBet()
        {
            return true;
        }
        string pw = "";
        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                
                Thread.Sleep(500);
                 List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                 pairs.Add(new KeyValuePair<string, string>("quantity", (Amount).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("address", Address));
                pairs.Add(new KeyValuePair<string, string>("act", "withdraw"));
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("password", pw));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("tfa", ""));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            SHA512 betgenerator = SHA512.Create();
            int charstouse = 5;
            string source = server + "|" + client;
            byte[] hash = betgenerator.ComputeHash(Encoding.UTF8.GetBytes(source));

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000m;
            }
            return 0;
        }
        new public static decimal sGetLucky(string server, string client, int nonce)
        {
            SHA512 betgenerator = SHA512.Create();
            int charstouse = 5;
            string source = server + "|" + client;
            byte[] hash = betgenerator.ComputeHash(Encoding.UTF8.GetBytes(source));

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000m;
            }
            return 0;
        }

        public string getDepositAddress()
        {
            try
            {
                 string sEmitResponse = Client.GetStringAsync("deposit?api_key=" + accesstoken).Result;
                pdDeposit tmpa = json.JsonDeserialize<pdDeposit>(sEmitResponse);
                return tmpa.address;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("429"))
                    {
                        Thread.Sleep(1500);
                        return getDepositAddress();
                    }
                    
                }
                return "";
            }
        }

        public override void Disconnect()
        {
            ispd = false;
            if (accesstoken!="")
            try
            {
                    string sEmitResponse = Client.GetStringAsync("logout?api_key=" + accesstoken).Result;
                    accesstoken = "";
            }
            catch
            {

            }
        }

        public override void Donate(decimal Amount)
        {
            if (Currency.ToLower()=="bitcoins")
                SendTip("seuntjie", Amount);
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("message", string.Format("/tip {0} {1}", User, amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo))));
                pairs.Add(new KeyValuePair<string, string>("act", "chat"));
                pairs.Add(new KeyValuePair<string, string>("room", "1"));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                
                return (sEmitResponse.Contains("true"));
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    
                }
            }
            return false;
        }


        DateTime lastchat = DateTime.UtcNow;



        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }
    }
    public class bitvestLoginBase
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public bitvestLogin data { get; set; }
        public bitvestAccount account { get; set; }
        public string server_hash { get; set; }
        public bitvesttip tip { get; set; }
    }
    public class bitvestLogin
    {
        
        public string balance { get; set; }
        public string token_balance { get; set; }
        public string self_username { get; set; }
        public string self_user_id { get; set; }
        public string self_ref_count { get; set; }
        public string self_ref_total_profit { get; set; }
        public string self_total_bet_dice { get; set; }
        public string self_total_won_dice { get; set; }
        public string self_total_bets_dice { get; set; }
        public string session_token { get; set; }
        
    }
    public class bitvesttip
    {
        public bool enabled { get; set; }
    }
    public class bitvestAccount
    {
        public string type { get; set; }
        public string address { get; set; }
        public string secret { get; set; }

    }

    public class bitvestbet
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public bitvestbetdata data { get; set; }
        public bitvestgameresult game_result { get; set; }
        public long game_id { get; set; }
        public string result { get; set; }
        public string server_seed { get; set; }
        public string server_hash { get; set; }
    }
    public class bitvestbetdata
    {
        public string balance { get; set; }
        public string pending { get; set; }
        public string token_balance { get; set; }
        public string self_username { get; set; }
        public string self_user_id { get; set; }
        

    }
    public class bitvestgameresult
    {
        public decimal roll { get; set; }
        public decimal win { get; set; }
        public decimal total_bet { get; set; }
        public decimal multiplier { get; set; }
    }
}
