using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DiceBot.Common;
using DiceBot.Forms;

namespace DiceBot.Sites
{
    internal class DuckDice : DiceSite
    {
        public static string[] cCurrencies = {"BTC", "ETH", "LTC", "DOGE", "DASH", "BCH", "XMR", "XRP", "ETC", "BTG", "XLM", "ZEC", "USDT"};
        private string accesstoken = "";
        private HttpClient Client; // = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        private HttpClientHandler ClientHandlr;
        private QuackSeed currentseed;
        public bool ispd;
        private DateTime LastSeedReset = new DateTime();
        private DateTime lastupdate;
        private readonly string[] mirrors = {"https://duckdice.io/", "https://duckdice.me", "https://duckdice.net"};
        private readonly Random R = new Random();
        private int site;
        private long uid = 0;
        private string username = "";

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
            try
            {
                if (ispd)
                {
                    var sEmitResponse = Client.GetStringAsync("load/" + Currency + "?api_key=" + accesstoken).Result;
                    var balance = JsonUtils.JsonDeserialize<Quackbalance>(sEmitResponse);
                    this.balance = decimal.Parse(balance.user.balance, NumberFormatInfo.InvariantInfo);
                    Parent.updateBalance(this.balance);
                    sEmitResponse = Client.GetStringAsync("stat/" + Currency + "?api_key=" + accesstoken).Result;
                    var Stats = JsonUtils.JsonDeserialize<QuackStatsDetails>(sEmitResponse);
                    profit = decimal.Parse(Stats.profit, NumberFormatInfo.InvariantInfo);
                    wagered = decimal.Parse(Stats.volume, NumberFormatInfo.InvariantInfo);
                    bets = Stats.bets;
                    wins = Stats.wins;
                    losses = bets - wins;
                    Parent.updateProfit(profit);
                    Parent.updateBets(bets);
                    Parent.updateLosses(losses);
                    Parent.updateWagered(wagered);
                    Parent.updateWins(wins);
                }
            }
            catch
            {
            }
        }

        private void GetBalanceThread()
        {
            while (ispd)
            {
                if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    try
                    {
                        lastupdate = DateTime.Now;
                        var sEmitResponse = Client.GetStringAsync("load/" + Currency + "?api_key=" + accesstoken).Result;
                        var balance = JsonUtils.JsonDeserialize<Quackbalance>(sEmitResponse);
                        this.balance = decimal.Parse(balance.user.balance, NumberFormatInfo.InvariantInfo);
                        Parent.updateBalance(this.balance);
                        sEmitResponse = Client.GetStringAsync("stat/" + Currency + "?api_key=" + accesstoken).Result;
                        var Stats = JsonUtils.JsonDeserialize<QuackStatsDetails>(sEmitResponse);
                        profit = decimal.Parse(Stats.profit, NumberFormatInfo.InvariantInfo);
                        wagered = decimal.Parse(Stats.volume, NumberFormatInfo.InvariantInfo);
                        bets = Stats.bets;
                        wins = Stats.wins;
                        losses = bets - wins;
                        Parent.updateProfit(profit);
                        Parent.updateBets(bets);
                        Parent.updateLosses(losses);
                        Parent.updateWagered(wagered);
                        Parent.updateWins(wins);
                    }
                    catch
                    {
                    }

                Thread.Sleep(1000);
            }
        }

        private void PlaceBetThreead(object bet)
        {
            var tmp5 = bet as PlaceBetObj;
            var amount = tmp5.Amount;
            var chance = tmp5.Chance;
            var High = tmp5.High;

            var Content =
                new
                    StringContent(string.Format(NumberFormatInfo.InvariantInfo, "{{\"amount\":\"{0:0.00000000}\",\"symbol\":\"{1}\",\"chance\":{2:0.00},\"isHigh\":{3}}}", amount, Currency, chance, High ? "true" : "false"),
                                  Encoding.UTF8, "application/json");

            try
            {
                var sEmitResponse = Client.PostAsync("play" + "?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
                var newbet = JsonUtils.JsonDeserialize<QuackBet>(sEmitResponse);

                if (newbet.error != null)
                {
                    Parent.updateStatus(newbet.error);

                    return;
                }

                var tmp = new Bet
                {
                    //Id=newbet.ha
                    Amount = decimal.Parse(newbet.bet.betAmount, NumberFormatInfo.InvariantInfo),
                    Chance = newbet.bet.chance,
                    clientseed = currentseed.clientSeed,
                    Currency = Currency,
                    date = DateTime.Now,
                    high = High,
                    nonce = currentseed.nonce++,
                    Profit = decimal.Parse(newbet.bet.profit, NumberFormatInfo.InvariantInfo),
                    Roll = newbet.bet.number / 100,
                    serverhash = currentseed.serverSeedHash,
                    Id = newbet.bet.hash,
                    Guid = tmp5.Guid
                };

                lastupdate = DateTime.Now;
                profit = decimal.Parse(newbet.user.profit, NumberFormatInfo.InvariantInfo);
                wagered = decimal.Parse(newbet.user.volume, NumberFormatInfo.InvariantInfo);
                balance = decimal.Parse(newbet.user.balance, NumberFormatInfo.InvariantInfo);
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
            new Thread(PlaceBetThreead).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        public override void ResetSeed()
        {
            try
            {
                var alf = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
                var Clientseed = "";

                while (Clientseed.Length < R.Next(15, 25))
                {
                    Clientseed += alf[R.Next(0, alf.Length)];
                }

                var Content = new StringContent(string.Format(NumberFormatInfo.InvariantInfo, "{{\"clientSeed\":\"{0}\"}}", Clientseed), Encoding.UTF8,
                                                "application/json");

                var sEmitResponse = Client.PostAsync("randomize/" + "?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
                currentseed = JsonUtils.JsonDeserialize<QuackSeed>(sEmitResponse).current;
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
                var cont = string.Format(NumberFormatInfo.InvariantInfo, "{{\"username\":\"{1}\",\"symbol\":\"{0}\",\"amount\":{2:0.00000000}}}", Currency,
                                         User, amount);

                var Content = new StringContent(cont, Encoding.UTF8, "application/json");
                var sEmitResponse = Client.PostAsync("tip-username" + "?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;

                //Parent.DumpLog(sEmitResponse, -1);
                var tmp = JsonUtils.JsonDeserialize<QuackWithdraw>(sEmitResponse);

                if (tmp.error == null) return true;
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
            }

            return false;
        }

        public override void Donate(decimal Amount)
        {
            SendTip("seuntjie", Amount);
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            ClientHandlr.CookieContainer = new CookieContainer();
            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri(mirrors[site] + "/api/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");

            //Client.DefaultRequestHeaders.Add("origin", "https://duckdice.io");
            try
            {
                accesstoken = Password;

                //Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",accesstoken);
                accesstoken = Password;

                var sEmitResponse = "";

                try
                {
                    using (var response = Client.GetAsync("load/" + Currency + "?api_key=" + accesstoken).Result)
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
                    }

                    ;
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

                var balance = JsonUtils.JsonDeserialize<Quackbalance>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("stat/" + Currency + "?api_key=" + accesstoken).Result;
                var Stats = JsonUtils.JsonDeserialize<QuackStatsDetails>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("randomize" + "?api_key=" + accesstoken).Result;
                currentseed = JsonUtils.JsonDeserialize<QuackSeed>(sEmitResponse).current;

                if (balance != null && Stats != null)
                {
                    this.balance = decimal.Parse(balance.user.balance, NumberFormatInfo.InvariantInfo);
                    profit = decimal.Parse(Stats.profit, NumberFormatInfo.InvariantInfo);
                    wagered = decimal.Parse(Stats.volume, NumberFormatInfo.InvariantInfo);
                    bets = Stats.bets;
                    wins = Stats.wins;
                    losses = bets - wins;
                    Parent.updateBalance(this.balance);
                    Parent.updateProfit(profit);
                    Parent.updateBets(bets);
                    Parent.updateLosses(losses);
                    Parent.updateWagered(wagered);
                    Parent.updateWins(wins);

                    ispd = true;
                    lastupdate = DateTime.Now;
                    new Thread(GetBalanceThread).Start();
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

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = SHA512.Create();

            var charstouse = 5;

            var buffer = new List<byte>();
            var msg = server + client + nonce;

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            var hash = betgenerator.ComputeHash(buffer.ToArray());

            var hex = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            for (var i = 0; i < hex.Length; i += charstouse)
            {
                var s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, NumberStyles.HexNumber);

                if (lucky < 1000000)
                {
                    var tmp = lucky % 10000 / 100m;

                    return tmp;
                }
            }

            return 0;
        }
    }

    public class QuackLogin
    {
        public string token { get; set; }
    }

    public class Quackbalance
    {
        public QuackStats user { get; set; }
        public string hash { get; set; }
        public string username { get; set; }
        public string balance { get; set; }
        public QuackStats session { get; set; }
    }

    public class QuackStats
    {
        public QuackStats user { get; set; }
        public string hash { get; set; }
        public string username { get; set; }
        public string balance { get; set; }
        public QuackStats session { get; set; }
        public int bets { get; set; }
        public int wins { get; set; }
        public string volume { get; set; }
        public string profit { get; set; }
    }

    public class QuackStatsDetails
    {
        public int bets { get; set; }
        public int wins { get; set; }
        public string profit { get; set; }
        public string volume { get; set; }
    }

    public class QuackBet
    {
        public string error { get; set; }
        public QuackBet bet { get; set; }
        public QuackStats user { get; set; }
        public string hash { get; set; }
        public string symbol { get; set; }
        public bool result { get; set; }
        public bool isHigh { get; set; }
        public decimal number { get; set; }
        public decimal threshold { get; set; }
        public decimal chance { get; set; }
        public decimal payout { get; set; }
        public string betAmount { get; set; }
        public string winAmount { get; set; }
        public string profit { get; set; }
        public long nonce { get; set; }
    }

    public class QuackSeed
    {
        public QuackSeed current { get; set; }
        public string clientSeed { get; set; }
        public long nonce { get; set; }
        public string serverSeedHash { get; set; }
    }

    public class QuackWithdraw
    {
        public string error { get; set; }
    }
}
