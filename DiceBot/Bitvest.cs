
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
        public static string[] cCurrencies = new string[] { "bitcoins", "tokens","litecoins","ethers" };
        
        public Bitvest(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://bitvest.io/results?game=dice&query=";

            this.Currencies = cCurrencies;
            Currency = "bitcoins";
            this.Parent = Parent;
            Name = "Bitvest";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://bitvest.io?r=46534";
            NonceBased = false;
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
            
            while (ispd)
            {
                try
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 10 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        ForceUpdateStats = false;
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                        pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                        pairs.Add(new KeyValuePair<string, string>("k", "0"));
                        pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                        pairs.Add(new KeyValuePair<string, string>("u", "0"));
                        pairs.Add(new KeyValuePair<string, string>("self_only", "1"));

                        HttpResponseMessage resp1 = Client.GetAsync("").Result;
                        string s1 = "";
                        if (resp1.IsSuccessStatusCode)
                        {
                            s1 = resp1.Content.ReadAsStringAsync().Result;
                            //Parent.DumpLog("BE login 2.1", 7);
                        }
                        else
                        {
                            //Parent.DumpLog("BE login 2.2", 7);
                            if (resp1.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                s1 = resp1.Content.ReadAsStringAsync().Result;
                                //cflevel = 0;
                                System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    System.Windows.Forms.MessageBox.Show("Bitvest.io has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                });
                                if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "bitvest.io"))
                                {

                                    finishedlogin(false);
                                    return;
                                }

                            }
                            //Parent.DumpLog("BE login 2.3", 7);
                        }

                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        string sEmitResponse = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;
                        sEmitResponse = sEmitResponse.Replace("r-", "r_").Replace("n-", "n_");
                        
                        BivestGetBalanceRoot tmpbase = json.JsonDeserialize<BivestGetBalanceRoot>(sEmitResponse);
                        if (tmpbase != null)
                        {
                            if (tmpbase.data != null)
                            {
                                switch (Currency.ToLower())
                                {
                                    case "bitcoins":
                                        balance = tmpbase.data.balance; break;
                                    case "ethers":
                                        balance = tmpbase.data.ether_balance; break;
                                    case "litecoins":
                                        balance = tmpbase.data.litecoin_balance; break;
                                    default:
                                        balance = tmpbase.data.token_balance; break;
                                }
                                /*if (Currency.ToLower() == "bitcoins")
                                {
                                    balance = decimal.Parse(tmpbase.data.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }
                                else
                                {
                                    balance = decimal.Parse(tmpbase.data.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }if (Currency.ToLower() == "bitcoins")
                    {
                        balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "ethereum")
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "litecoin")
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }*/
                                Parent.updateBalance(balance);
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }
                Thread.Sleep(1000);
                
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
                    else if (Currency.ToLower() == "ethers")
                    {
                        balance = decimal.Parse(tmplogin.balance_ether, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "litecoins")
                    {
                        balance = decimal.Parse(tmplogin.balance_litecoin, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    accesstoken = tmplogin.session_token;
                    secret = tmpblogin.account.secret;
                    wagered = (decimal)(Currency.ToLower() == "bitcoins"? tmplogin.total_bet:
                        Currency.ToLower() == "ethers" ? tmplogin.ether_total_bet :
                        Currency.ToLower() == "litecoins" ? tmplogin.litecoin_total_bet :
                        tmplogin.token_total_bet) ;
                    profit = (decimal)(Currency.ToLower() == "bitcoins" ? tmplogin.total_profit :
                        Currency.ToLower() == "ethers" ? tmplogin.ether_total_profit :
                        Currency.ToLower() == "litecoins" ? tmplogin.litecoin_total_profit :
                        tmplogin.token_total_bet);
                    bets = tmplogin.bets;
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

        bitvestCurWeight Weights = null;
        double[] Limits = new double[0];

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

                string tmpresp = resp.Replace("-", "_");
                 tmpblogin = json.JsonDeserialize<bitvestLoginBase>(tmpresp);
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
                    tmpresp = resp.Replace("-", "_");
                    tmpblogin = json.JsonDeserialize<bitvestLoginBase>(tmpresp);
                    Weights = tmpblogin.currency_weight;
                    Limits = tmpblogin.rate_limits;

                    tmplogin = tmpblogin.data;
                    if (Currency.ToLower() == "bitcoins")
                    {
                        balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "ethers")
                    {
                        balance = decimal.Parse(tmplogin.balance_ether, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "litecoins")
                    {
                        balance = decimal.Parse(tmplogin.balance_litecoin, System.Globalization.NumberFormatInfo.InvariantInfo);
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
                    string x = sEmitResponse.Replace("f-", "f_").Replace("n-", "n_").Replace("ce-", "ce_").Replace("r-", "r_");
                    bitvestbet tmp = json.JsonDeserialize<bitvestbet>(x);
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
                        resbet.Guid = tmp5.Guid;
                        bets++;
                        lasthash = tmp.server_hash;
                        bool Win = (((bool)High ? (decimal)tmp.game_result.roll > (decimal)maxRoll - (decimal)(chance) : (decimal)tmp.game_result.roll < (decimal)(chance)));
                        if (Win)
                            wins++;
                        else losses++;
                        wagered += amount;
                        profit += resbet.Profit;
                        balance = decimal.Parse(
                            Currency.ToLower()=="bitcoins"?
                                tmp.data.balance:
                                Currency.ToLower()=="ethers"? tmp.data.balance_ether
                                : Currency.ToLower()=="litecoins"?tmp.data.balance_litecoin: tmp.data.token_balance, 
                            System.Globalization.NumberFormatInfo.InvariantInfo);
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
                catch (Exception e)
                {
                    Parent.updateStatus("An unknown error has occurred");
                    Parent.DumpLog(e.ToString(), -1);
                }
            }
            catch (AggregateException e)
            {
                if (retrycount++ < 3)
                {
                    Thread.Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance, (bet as PlaceBetObj).Guid));
                    return;
                }
                if (e.InnerException.Message.Contains("429") || e.InnerException.Message.Contains("502"))
                {
                    Thread .Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance, (bet as PlaceBetObj).Guid));
                }
                

            }
            catch (Exception e2)
            {

            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.High = High;
            new Thread(new ParameterizedThreadStart(placebetthread)).Start(new PlaceBetObj(High, amount, chance, Guid));
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
            decimal weight = 1;
            if (Currency.ToLower() == "bitcoins")
            {
                switch (Currency.ToLower())
                {
                    case "bitcoins":weight = decimal.Parse(Weights.BTC, System.Globalization.NumberFormatInfo.InvariantInfo);break;
                    case "tokens": weight = decimal.Parse(Weights.TOK, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                    case "litecoins": weight = decimal.Parse(Weights.LTC, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                    case "ethers": weight = decimal.Parse(Weights.ETH, System.Globalization.NumberFormatInfo.InvariantInfo); break;

                    default: weight = decimal.Parse(Weights.BTC, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                }
            }
            

            for (int i = Limits.Length-1; i>=0;i--)
            {
                if (i == Limits.Length-1 && (amount*weight)>=(decimal)Limits[i]*0.00000001m)
                {
                    return true;
                }
                else if ((amount * weight) >= (decimal)Limits[i] * 0.00000001m)
                {
                    return ((DateTime.Now - Lastbet).TotalSeconds > 1.0 / (i + 1.0));                    
                }
            }
            
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
                pairs.Add(new KeyValuePair<string, string>("currency",(Currency=="bitcoins"?"btc":
                    Currency == "litecoins" ? "ltc" :
                    Currency == "ethers" ? "eth" :
                    Currency == "tokens" ? "tok":"tok")));
                pairs.Add(new KeyValuePair<string, string>("username", User));
                pairs.Add(new KeyValuePair<string, string>("quantity", amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("act", "send_tip"));
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
        public bitvestCurWeight currency_weight { get; set; }
        public double[] rate_limits { get; set; }
    }
    public class bitvestCurWeight
    {
        public string BTC { get; set; }
        public string ETH { get; set; }
        public string LTC { get; set; }
        public string TOK { get; set; }
    }
    /*{"data":{"self-user-id":46534,"self-username":"Seuntjie",
      "balance":0.00586720655,
      "token_balance":39775.605,
      "ether_balance":0.0001,
      "litecoin_balance":0.0001,"pending":0,"ether_pending":0,"litecoin_pending":0,"address":"12Nfe1Dp9VAFRqKLKE9vgrP28qJH3Aaad4",
      "ether_address":"0x3e10685213a68b3321d6b65352ac9cc94da559f1",
      "litecoin_address":"LQe1t6YCiteYfVJ9n1BXHdXopzmp94CMWd",
      "total_bet":0.01493638,
      "total_won":0.0082235001,
      "total_profit":-0.0067128799,
      "token_total_bet":811453,
      "token_total_won":761301.63,
      "token_total_profit":-50151.37,
      "ether_total_bet":0,
      "ether_total_won":0,
      "ether_total_profit":0,
      "litecoin_total_bet":0,
      "litecoin_total_won":0,
      "litecoin_total_profit":0,
      "bets":2272,
      "server_hash":"30e01c8a1385bd3bd7cf8a2856e73bf639807eccced705159538b074582839a9"}}
      */
    public class bitvestLogin
    {
        
        public string balance { get; set; }
        public string token_balance { get; set; }
        public string balance_litecoin { get; set; }
        public string self_username { get; set; }
        public string self_user_id { get; set; }
        public string self_ref_count { get; set; }
        public string self_ref_total_profit { get; set; }
        public string self_total_bet_dice { get; set; }
        public string self_total_won_dice { get; set; }
        public string self_total_bets_dice { get; set; }
        public string session_token { get; set; }
        
        public string balance_ether { get; set; }
        public decimal pending { get; set; }
        public decimal ether_pending { get; set; }
        public decimal pending_litecoin { get; set; }
        public string address { get; set; }
        public string ether_address { get; set; }
        public string litecoin_address { get; set; }
        public decimal total_bet { get; set; }
        public decimal total_won { get; set; }
        public decimal total_profit { get; set; }
        public decimal token_total_bet { get; set; }
        public decimal token_total_won { get; set; }
        public decimal token_total_profit { get; set; }
        public decimal ether_total_bet { get; set; }
        public decimal ether_total_won { get; set; }
        public decimal ether_total_profit { get; set; }
        public decimal litecoin_total_bet { get; set; }
        public decimal litecoin_total_won { get; set; }
        public decimal litecoin_total_profit { get; set; }
        public int bets { get; set; }
        public string server_hash { get; set; }




    }

    public class BitVestGetBalance
    {
        public int self_user_id { get; set; }
        public string self_username { get; set; }
        public decimal balance { get; set; }
        public decimal token_balance { get; set; }
        public decimal ether_balance { get; set; }
        public decimal litecoin_balance { get; set; }
        public decimal pending { get; set; }
        public decimal ether_pending { get; set; }
        public decimal litecoin_pending { get; set; }
        public string address { get; set; }
        public string ether_address { get; set; }
        public string litecoin_address { get; set; }
        public decimal total_bet { get; set; }
        public decimal total_won { get; set; }
        public decimal total_profit { get; set; }
        public decimal token_total_bet { get; set; }
        public decimal token_total_won { get; set; }
        public decimal token_total_profit { get; set; }
        public decimal ether_total_bet { get; set; }
        public decimal ether_total_won { get; set; }
        public decimal ether_total_profit { get; set; }
        public decimal litecoin_total_bet { get; set; }
        public decimal litecoin_total_won { get; set; }
        public decimal litecoin_total_profit { get; set; }
        public decimal bets { get; set; }
        public string server_hash { get; set; }
    }

    public class BivestGetBalanceRoot
    {
        public BitVestGetBalance data { get; set; }
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
        public string balance_ether { get; set; }
        public string token_balance { get; set; }
        public string balance_litecoin { get; set; }
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
