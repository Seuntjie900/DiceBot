using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
using DiceBot.Schema.DuckDice;

namespace DiceBot
{
    class DuckDice : DiceSite
    {


        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        //public static string[] cCurrencies = new string[] { "BTC", "ETH", "LTC", "DOGE", "DASH", "BCH", "XMR", "XRP", "ETC", "BTG", "XLM", "ZEC", "USDT", "DTP" };


        // document.querySelectorAll('.checkbox-block .checkbox-label__text div').forEach(p => console.log("\"" + p.textContent +"\","))
        private static string[] _cCurrencies = new string[]
        {
            "BTTC",
            "BTC",
            "LTC",
            "DOGE",
            "XRP",
            "USDT",
            "ETH",
            "TRX",
            "XLM",
            "BCH",
            "SHIB",
            "DASH",
            "BNB",
            "ETC",
            "ADA",
            "XMR",
            "BTG",
            "ZEC",
            "EOS",
            "DOT",
            "BUSD",
            "MATIC",
            "USDC",
            "DAI",
            "SOL",
            "NEAR",
            "RVN",
            "AVAX",
            "ZEN",
            "FTM"
        };


        public static string[] cCurrencies => _cCurrencies.Select(x => x.ToUpperInvariant()).OrderBy(x => x).ToArray();


        string[] mirrors = new string[] { "https://duckdice.io/", "https://duckdice.me", "https://duckdice.net" };

        private int mod;

        string apiversion = "1.1.1";

        string TLEhash = null;

        public int Mode
        {
            get { return mod; }
            set
            {
                mod = value;
                ForceUpdateStats = true; if (Mode == 3) this.edge = 2m; else this.edge = 1m;
            }
        }

        public DuckDice(cDiceBot Parent)
        {
            _PasswordText = "Api Key: ";
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://duckdice.io";

            this.Parent = Parent;
            Name = "DuckDice";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://duckdice.io/?c=53ea652da4";
            Currencies = cCurrencies;
            Currency = "BTC";
        }

        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
            /*try
            {
                if (ispd)
                {
                    string sEmitResponse = Client.GetStringAsync("load/" + Currency + "?api_key=" + accesstoken + "&api_version=" + apiversion).Result;
                    Quackbalance balance = json.JsonDeserialize<Quackbalance>(sEmitResponse);
                    this.balance = decimal.Parse(this.Mode==1? balance.user.balances.main: balance.user.balances.faucet, System.Globalization.NumberFormatInfo.InvariantInfo);
                    Parent.updateBalance(this.balance);
                    sEmitResponse = Client.GetStringAsync("stat/" + Currency + "?api_key=" + accesstoken).Result;
                    QuackStatsDetails Stats = json.JsonDeserialize<QuackStatsDetails>(sEmitResponse);
                    this.profit = decimal.Parse(Stats.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                    this.wagered = decimal.Parse(Stats.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                    bets = Stats.bets;
                    wins = Stats.wins;
                    losses = bets - wins;
                    Parent.updateProfit(this.profit);
                    Parent.updateBets(this.bets);
                    Parent.updateLosses(losses);
                    Parent.updateWagered(wagered);
                    Parent.updateWins(wins);
                }
            }
            catch { }*/
        }

        void GetBalanceThread()
        {

            while (ispd)
            {
                if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                {
                    try
                    {
                        ForceUpdateStats = false;
                        lastupdate = DateTime.Now;
                        string sEmitResponse = Client.GetStringAsync("load/" + Currency + "?api_key=" + accesstoken + "&api_version=" + apiversion).Result;
                        Quackbalance balance = json.JsonDeserialize<Quackbalance>(sEmitResponse);
                        //switch (this.Mode)
                        //{
                        //    case 1: this.balance = decimal.Parse(balance.user.balances.faucet, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        //    case 2:
                        //        this.balance = decimal.Parse(balance.user.balances.main, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        //    case 3: this.balance = decimal.Parse(balance.user.balances.tle, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        //    default: this.balance = decimal.Parse(balance.user.balances.main, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        //}
                        this.balance = decimal.Parse(this.Mode == 2 ? balance.user.balances.faucet : balance.user.balances.main, System.Globalization.NumberFormatInfo.InvariantInfo);
                        Parent.updateBalance(this.balance);
                        sEmitResponse = Client.GetStringAsync("stat/" + Currency + "?api_key=" + accesstoken).Result;
                        QuackStatsDetails Stats = json.JsonDeserialize<QuackStatsDetails>(sEmitResponse);
                        this.profit = decimal.Parse(Stats.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                        this.wagered = decimal.Parse(Stats.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                        bets = Stats.bets;
                        wins = Stats.wins;
                        losses = bets - wins;
                        Parent.updateProfit(this.profit);
                        Parent.updateBets(this.bets);
                        Parent.updateLosses(losses);
                        Parent.updateWagered(wagered);
                        Parent.updateWins(wins);
                    }
                    catch
                    {

                    }

                }
                Thread.Sleep(1000);
            }

        }
        QuackSeed currentseed = null;
        void PlaceBetThreead(object bet)
        {
            if (Mode == 3 && TLEhash == null)
            {
                string TLE = Client.GetAsync("tle").Result.Content.ReadAsStringAsync().Result;
                var tleobj = json.JsonDeserialize<DDTLE>(TLE);
                TLEhash = tleobj.data.FirstOrDefault()?.hash;
            }
            PlaceBetObj tmp5 = bet as PlaceBetObj;
            decimal amount = tmp5.Amount;
            decimal chance = tmp5.Chance;
            bool High = tmp5.High;
            StringContent Content = new StringContent(string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"amount\":\"{0:0.00000000}\",\"symbol\":\"{1}\",\"chance\":{2:0.00},\"isHigh\":{3},\"faucet\":{4},\"tleHash\":\"{5}\"}}", amount, Currency, chance, High ? "true" : "false", this.Mode == 2 ? "true" : "false", Mode == 3 ? TLEhash : null), Encoding.UTF8, "application/json");
            try
            {
                string sEmitResponse = Client.PostAsync("play" + "?api_key=" + accesstoken + "&api_version=" + apiversion, Content).Result.Content.ReadAsStringAsync().Result;
                QuackBet newbet = json.JsonDeserialize<QuackBet>(sEmitResponse);
                if (newbet.error != null)
                {
                    Parent.updateStatus(newbet.error);
                    return;
                }
                Bet tmp = new Bet
                {
                    //Id=newbet.ha
                    Amount = decimal.Parse(newbet.bet.betAmount, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Chance = newbet.bet.chance,
                    clientseed = currentseed.clientSeed,
                    Currency = Currency,
                    date = DateTime.Now,
                    high = High,
                    nonce = currentseed.nonce++,
                    Profit = decimal.Parse(newbet.bet.profit, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Roll = newbet.bet.number / 100,
                    serverhash = currentseed.serverSeedHash,
                    Id = newbet.bet.hash,
                    Guid = tmp5.Guid
                };
                lastupdate = DateTime.Now;
                profit = decimal.Parse(newbet.user.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                wagered = decimal.Parse(newbet.user.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                balance = decimal.Parse(newbet.user.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                wins = newbet.user.wins;
                bets = newbet.user.bets;
                losses = bets - wins;
                FinishedBet(tmp);
            }
            catch (Exception e)
            {
                Parent.updateStatus("There was an error placing your bet.");
                Parent.DumpLog(e.ToString(), -1);
            }
        }


        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            new Thread(new ParameterizedThreadStart(PlaceBetThreead)).Start(new PlaceBetObj(High, amount, chance, Guid));
        }
        DBRandom R = new DBRandom();
        public override void ResetSeed()
        {
            try
            {
                string alf = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
                string Clientseed = "";
                while (Clientseed.Length < R.Next(15, 25))
                {
                    Clientseed += alf[R.Next(0, alf.Length)];
                }
                StringContent Content = new StringContent(string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"clientSeed\":\"{0}\"}}", Clientseed), Encoding.UTF8, "application/json");
                string sEmitResponse = Client.PostAsync("randomize/" + "?api_key=" + accesstoken + "&api_version=" + apiversion, Content).Result.Content.ReadAsStringAsync().Result;
                currentseed = json.JsonDeserialize<QuackSeed>(sEmitResponse).current;
            }
            catch (Exception e)
            {

            }

        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            return false;
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                string cont = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"username\":\"{1}\",\"symbol\":\"{0}\",\"amount\":{2:0.00000000}}}", Currency, User, amount);
                StringContent Content = new StringContent(cont, Encoding.UTF8, "application/json");
                string sEmitResponse = Client.PostAsync("tip-username" + "?api_key=" + accesstoken + "&api_version=" + apiversion, Content).Result.Content.ReadAsStringAsync().Result;
                //Parent.DumpLog(sEmitResponse, -1);
                QuackWithdraw tmp = json.JsonDeserialize<QuackWithdraw>(sEmitResponse);
                if (tmp.error == null)
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
            }
            return false;
        }

        public override void Donate(decimal Amount)
        {
            SendTip("WinMachine", Amount);
        }

        int site = 0;

        public override void Login(string Username, string Password, string twofa)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            ClientHandlr.CookieContainer = new CookieContainer();
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri(mirrors[site] + "/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");

            //Client.DefaultRequestHeaders.Add("origin", "https://duckdice.io");
            try
            {

                accesstoken = Password;
                //Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",accesstoken);
                accesstoken = Password;


                string sEmitResponse = "";
                try
                {
                    using (var response = Client.GetAsync("load/" + Currency + "?api_key=" + accesstoken + "&api_version=" + apiversion).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            sEmitResponse = response.Content.ReadAsStringAsync().Result;
                            Parent.DumpLog(sEmitResponse, 2);
                        }
                        else
                        {
                            //Parent.DumpLog(e.ToString(), -1);
                            if (site++ < mirrors.Length - 1)
                                Login(Username, Password, twofa);
                            else
                                finishedlogin(false);
                            return;
                        }
                    };
                }

                catch (AggregateException e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                    if (site++ < mirrors.Length - 1)
                        Login(Username, Password, twofa);
                    else
                        finishedlogin(false);
                    return;

                }

                Quackbalance balance = json.JsonDeserialize<Quackbalance>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("stat/" + Currency + "?api_key=" + accesstoken + "&api_version=" + apiversion).Result;
                QuackStatsDetails Stats = json.JsonDeserialize<QuackStatsDetails>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("randomize" + "?api_key=" + accesstoken + "&api_version=" + apiversion).Result;
                currentseed = json.JsonDeserialize<QuackSeed>(sEmitResponse).current;
                if (balance != null && Stats != null)
                {
                    this.balance = decimal.Parse(this.Mode == 1 ? balance.user.balances.main : balance.user.balances.faucet, System.Globalization.NumberFormatInfo.InvariantInfo);
                    this.profit = decimal.Parse(Stats.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                    this.wagered = decimal.Parse(Stats.volume, System.Globalization.NumberFormatInfo.InvariantInfo);
                    bets = Stats.bets;
                    wins = Stats.wins;
                    losses = bets - wins;
                    Parent.updateBalance(this.balance);
                    Parent.updateProfit(this.profit);
                    Parent.updateBets(this.bets);
                    Parent.updateLosses(losses);
                    Parent.updateWagered(wagered);
                    Parent.updateWins(wins);

                    ispd = true;
                    lastupdate = DateTime.Now;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    finishedlogin(true);
                    return;
                }
                /*}
            }*/
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
                if (site++ < mirrors.Length - 1)
                    Login(Username, Password, twofa);
                else
                    finishedlogin(false);
                return;
            }
            finishedlogin(false);
            return;
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            ispd = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {

            return sGetLucky(server, client, nonce);
        }
        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            SHA512 betgenerator = SHA512.Create();

            int charstouse = 5;

            List<byte> buffer = new List<byte>();
            string msg = server + client + nonce.ToString();
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
                    decimal tmp = (lucky % 10000) / 100m;
                    return tmp;
                }
            }
            return 0;
        }

    }


}
