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
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    class Freebitcoin : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        DBRandom R = new DBRandom();
        public Freebitcoin(cDiceBot Parent)
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

        void PlaceBetThread(object _High)
        {
            try
            {
                PlaceBetObj tmp9 = _High as PlaceBetObj;
                string clientseed = "";
                bool High = tmp9.High;
                decimal amount = tmp9.Amount;
                decimal chance = tmp9.Chance;
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm1234567890";
                while (clientseed.Length < 16)
                {
                    clientseed += chars[R.Next(0, chars.Length)];
                }
                string Params = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "m={0}&client_seed={1}&jackpot=0&stake={2}&multiplier={3}&rand={5}&csrf_token={4}",
                    High ? "hi" : "lo", clientseed, amount, (100m - edge) / chance, csrf, R.Next(0, 9999999) / 10000000);

                var betresult = Client.GetAsync("https://freebitco.in/cgi-bin/bet.pl?" + Params).Result;
                if (betresult.IsSuccessStatusCode)
                {

                    string Result = betresult.Content.ReadAsStringAsync().Result;
                    string[] msgs = Result.Split(':');
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
                        Bet tmp = new Bet
                        {
                            Guid = tmp9.Guid,
                            Amount = amount,
                            date = DateTime.Now,
                            Chance = chance,
                            clientseed = msgs[7],
                            high = High,
                            Id = bets.ToString(),
                            Profit = msgs[1] == "w" ? decimal.Parse(msgs[4]) : -decimal.Parse(msgs[4], System.Globalization.NumberFormatInfo.InvariantInfo),
                            nonce = long.Parse(msgs[12], System.Globalization.NumberFormatInfo.InvariantInfo),
                            serverhash = msgs[10],
                            serverseed = msgs[9],
                            Roll = decimal.Parse(msgs[2], System.Globalization.NumberFormatInfo.InvariantInfo) / 100.0m

                        };
                        balance = decimal.Parse(msgs[3], System.Globalization.NumberFormatInfo.InvariantInfo);
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
                            if (msgs[1] == "20")
                            {
                                Parent.updateStatus("Balance too low.");
                            }
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
            Thread T = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            T.Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }
        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();
            server = nonce + ":" + server + ":" + nonce;
            string msg = nonce + ":" + client + ":" + nonce;
            int charstouse = 8;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }


            List<byte> buffer = new List<byte>();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            betgenerator.Key = buffer.ToArray();
            byte[] hash = betgenerator.ComputeHash(serverb.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
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

        void GetBalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 30)
                {
                    try
                    {
                        lastupdate = DateTime.Now;
                        string s = Client.GetStringAsync("https://freebitco.in/cgi-bin/api.pl?op=get_user_stats").Result;
                        FreebtcStats stats = json.JsonDeserialize<FreebtcStats>(s);
                        if (stats != null)
                        {
                            this.balance = stats.balance / 100000000m;
                            bets = (int)stats.rolls_played;
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
                }
                Thread.Sleep(1000);
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }
        CookieContainer Cookies = new CookieContainer();
        string csrf = "";
        string address = "";
        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://freebitco.in/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            ClientHandlr.CookieContainer = Cookies;
            try
            {
                string s1 = "";
                HttpResponseMessage resp = Client.GetAsync("").Result;
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
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Windows.Forms.MessageBox.Show("freebitcoin has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
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
                    if (x.Name == "csrf_token")
                    {
                        csrf = x.Value;
                    }
                }
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("csrf_token", csrf));
                pairs.Add(new KeyValuePair<string, string>("op", "login_new"));
                pairs.Add(new KeyValuePair<string, string>("btc_address", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("tfa_code", twofa));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                var EmitResponse = Client.PostAsync("" + accesstoken, Content).Result;

                if (EmitResponse.IsSuccessStatusCode)
                {
                    string s = EmitResponse.Content.ReadAsStringAsync().Result;
                    string[] messages = s.Split(':');
                    if (messages.Length > 2)
                    {
                        address = messages[1];
                        accesstoken = messages[2];
                        Cookies.Add(new Cookie("btc_address", address, "/", "freebitco.in"));
                        Cookies.Add(new Cookie("password", accesstoken, "/", "freebitco.in"));
                        Cookies.Add(new Cookie("have_account", "1", "/", "freebitco.in"));

                        s = Client.GetStringAsync("https://freebitco.in/cgi-bin/api.pl?op=get_user_stats").Result;
                        FreebtcStats stats = json.JsonDeserialize<FreebtcStats>(s);
                        if (stats != null)
                        {
                            this.balance = stats.balance / 100000000m;
                            bets = (int)stats.rolls_played;
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
                            Thread t = new Thread(GetBalanceThread);
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
