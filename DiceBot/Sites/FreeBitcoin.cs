using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiceBot.Common;
using DiceBot.Forms;

namespace DiceBot.Sites
{
    internal class FreeBitcoin : DiceSite
    {
        private readonly CookieContainer Cookies = new CookieContainer();
        private readonly Random R = new Random();
        private string accesstoken = "";
        private string address = "";
        private HttpClient Client;
        private HttpClientHandler ClientHandlr;
        private string csrf = "";
        public bool ispd;
        private DateTime LastSeedReset = new DateTime();
        private DateTime lastupdate;
        private long uid = 0;
        private string username = "";

        public FreeBitcoin(cDiceBot Parent)
        {
            _UsernameText = "Email:";
            _PasswordText = "Password: ";
            maxRoll = 100m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = false;
            BetURL = "https://freebitco.in/?r=2310118&bet=";
            edge = 5m;
            this.Parent = Parent;
            Name = "FreeBitcoin";
            Tip = false;
            TipUsingName = true;

            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://freebitco.in/?r=2310118";
        }

        private void PlaceBetThread(object _High)
        {
            try
            {
                var tmp9 = _High as PlaceBetObj;
                var clientseed = "";
                var High = tmp9.High;
                var amount = tmp9.Amount;
                var chance = tmp9.Chance;
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm1234567890";

                while (clientseed.Length < 16)
                {
                    clientseed += chars[R.Next(0, chars.Length)];
                }

                var Params = string.Format(NumberFormatInfo.InvariantInfo, "m={0}&client_seed={1}&jackpot=0&stake={2}&multiplier={3}&rand={5}&csrf_token={4}",
                                           High ? "hi" : "lo", clientseed, amount, (100m - edge) / chance, csrf, R.Next(0, 9999999) / 10000000);

                var betresult = Client.GetAsync("https://freebitco.in/cgi-bin/bet.pl?" + Params).Result;

                if (betresult.IsSuccessStatusCode)
                {
                    var Result = betresult.Content.ReadAsStringAsync().Result;
                    var msgs = Result.Split(':');

                    if (msgs.Length > 2)
                    {
                        /*
                            1. Success code (s1)
                            2. Result (w/l)
                            3. Rolled number
                            4. User balance
                            5. Amount won or lost (always positive). If 2. is l, then amount is subtracted from balance else if w it is added.
                            6. Redundant (can ignore)
                            7. Server seed hash for next roll
                            8. Client seed of previous roll
                            9. Nonce for next roll
                            10. Server seed for previous roll
                            11. Server seed hash for previous roll
                            12. Client seed again (can ignore)
                            13. Previous nonce
                            14. Jackpot result (1 if won 0 if not won)
                            15. Redundant (can ignore)
                            16. Jackpot amount won (0 if lost)
                            17. Bonus account balance after bet
                            18. Bonus account wager remaining
                            19. Max. amount of bonus eligible
                            20. Max bet
                            21. Account balance before bet
                            22. Account balance after bet
                            23. Bonus account balance before bet
                            24. Bonus account balance after bet
                         */
                        var tmp = new Bet
                        {
                            Guid = tmp9.Guid,
                            Amount = amount,
                            date = DateTime.Now,
                            Chance = chance,
                            clientseed = msgs[7],
                            high = High,
                            Id = bets.ToString(),
                            Profit = msgs[1] == "w" ? decimal.Parse(msgs[4]) : -decimal.Parse(msgs[4], NumberFormatInfo.InvariantInfo),
                            nonce = long.Parse(msgs[12], NumberFormatInfo.InvariantInfo),
                            serverhash = msgs[10],
                            serverseed = msgs[9],
                            Roll = decimal.Parse(msgs[2], NumberFormatInfo.InvariantInfo) / 100.0m
                        };

                        balance = decimal.Parse(msgs[3], NumberFormatInfo.InvariantInfo);

                        if (msgs[1] == "w")
                            wins++;
                        else losses++;

                        bets++;
                        wagered += amount;
                        profit += tmp.Profit;
                        FinishedBet(tmp);
                    }
                    else if (msgs.Length > 0)
                    {
                        //20 - too low balance
                        if (msgs.Length > 1)

                        {
                            if (msgs[1] == "20") Parent.updateStatus("Balance too low.");
                        }
                        else
                        {
                            Parent.updateStatus("Site returned unknown error. Retrying in 30 seconds.");
                        }
                    }
                    else
                    {
                        Parent.updateStatus("Site returned unknown error. Retrying in 30 seconds.");
                    }
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
                Parent.updateStatus("An internal error occured. Retrying in 30 seconds.");
            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            var T = new Thread(PlaceBetThread);
            T.Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = new HMACSHA512();
            server = nonce + ":" + server + ":" + nonce;
            var msg = nonce + ":" + client + ":" + nonce;
            var charstouse = 8;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            var buffer = new List<byte>();

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            betgenerator.Key = buffer.ToArray();
            var hash = betgenerator.ComputeHash(serverb.ToArray());

            var hex = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            for (var i = 0; i < hex.Length; i += charstouse)
            {
                var s = hex.ToString().Substring(i, charstouse);

                decimal lucky = long.Parse(s, NumberStyles.HexNumber);
                /*if (lucky < 1000000)
                    return lucky / 10000;*/
                lucky = Math.Round(lucky / 429496.7295m);

                return lucky / 100;
            }

            return 0;
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        private void GetBalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 30)
                    try
                    {
                        lastupdate = DateTime.Now;
                        var s = Client.GetStringAsync("https://freebitco.in/cgi-bin/api.pl?op=get_user_stats").Result;
                        var stats = JsonUtils.JsonDeserialize<FreebtcStats>(s);

                        if (stats != null)
                        {
                            balance = stats.balance / 100000000m;
                            bets = (int) stats.rolls_played;

                            //wins = losses = 0;
                            profit = stats.dice_profit / 100000000m;
                            wagered = stats.wagered / 100000000m;
                            Parent.updateBalance(balance);
                            Parent.updateBets(bets);
                            Parent.updateWins(wins);
                            Parent.updateLosses(losses);
                            Parent.updateWagered(wagered);
                            Parent.updateProfit(profit);
                        }
                    }
                    catch (Exception e)
                    {
                        Parent.DumpLog(e.ToString(), -1);
                    }

                Thread.Sleep(1000);
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://freebitco.in/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            ClientHandlr.CookieContainer = Cookies;

            try
            {
                var s1 = "";
                var resp = Client.GetAsync("").Result;

                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;

                        //cflevel = 0;
                        Task.Factory.StartNew(() =>
                                              {
                                                  MessageBox
                                                      .Show("freebitcoin has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                              });

                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "freebitco.in"))
                        {
                            finishedlogin(false);

                            return;
                        }
                    }
                }

                foreach (Cookie x in Cookies.GetCookies(new Uri("https://freebitco.in")))
                {
                    if (x.Name == "csrf_token") csrf = x.Value;
                }

                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("csrf_token", csrf));
                pairs.Add(new KeyValuePair<string, string>("op", "login_new"));
                pairs.Add(new KeyValuePair<string, string>("btc_address", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("tfa_code", twofa));
                var Content = new FormUrlEncodedContent(pairs);
                var EmitResponse = Client.PostAsync("" + accesstoken, Content).Result;

                if (EmitResponse.IsSuccessStatusCode)
                {
                    var s = EmitResponse.Content.ReadAsStringAsync().Result;
                    var messages = s.Split(':');

                    if (messages.Length > 2)
                    {
                        address = messages[1];
                        accesstoken = messages[2];
                        Cookies.Add(new Cookie("btc_address", address, "/", "freebitco.in"));
                        Cookies.Add(new Cookie("password", accesstoken, "/", "freebitco.in"));
                        Cookies.Add(new Cookie("have_account", "1", "/", "freebitco.in"));

                        s = Client.GetStringAsync("https://freebitco.in/cgi-bin/api.pl?op=get_user_stats").Result;
                        var stats = JsonUtils.JsonDeserialize<FreebtcStats>(s);

                        if (stats != null)
                        {
                            balance = stats.balance / 100000000m;
                            bets = (int) stats.rolls_played;
                            wins = losses = 0;
                            profit = stats.dice_profit / 100000000m;
                            wagered = stats.wagered / 100000000m;
                            Parent.updateBalance(balance);
                            Parent.updateBets(bets);
                            Parent.updateWins(wins);
                            Parent.updateLosses(losses);
                            Parent.updateWagered(wagered);
                            Parent.updateProfit(profit);
                            lastupdate = DateTime.Now;
                            ispd = true;
                            var t = new Thread(GetBalanceThread);
                            t.Start();
                            finishedlogin(true);

                            return;
                        }

                        finishedlogin(false);

                        return;
                    }

                    finishedlogin(false);

                    return;
                }

                //Lastbet = DateTime.Now;
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 1);
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
    }

    public class FreebtcStats
    {
        public long wagered { get; set; }
        public long rolls_played { get; set; }
        public decimal lottery_spent { get; set; }
        public string status { get; set; }
        public decimal jackpot_winnings { get; set; }
        public decimal jackpot_spent { get; set; }
        public decimal reward_points { get; set; }
        public decimal balance { get; set; }
        public decimal total_winnings { get; set; }
        public decimal dice_profit { get; set; }
    }
}
