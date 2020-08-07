using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    internal class WolfBet : DiceSite
    {
        public static string[] cCurrencies = {"btc", "eth", "ltc", "trx", "bch", "doge", "xrp"};
        private string accesstoken = "";
        private HttpClient Client;
        private HttpClientHandler ClientHandlr;
        public bool ispd;
        private DateTime lastupdate;
        private readonly string URL = "https://wolf.bet";

        public WolfBet(cDiceBot Parent)
        {
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://wolf.bet/";
            _PasswordText = "API Key";
            Currencies = cCurrencies;
            Currency = "btc";
            this.Parent = Parent;
            Name = "WolfBet";
            Tip = false;
            TipUsingName = true;
            SiteURL = "https://wolf.bet?c=Seuntjie";
            NonceBased = true;

            if (File.Exists("wolf.txt"))
                URL = File.ReadAllText("wolf.txt").TrimEnd(' ', '\r', '\n', '\t');
            else if (File.Exists("wolf")) URL = File.ReadAllText("wolf.txt").TrimEnd(' ', '\r', '\n', '\t');
        }

        public override void Disconnect()
        {
            ispd = false;
        }

        protected override void CurrencyChanged()
        {
            base.CurrencyChanged();
            ForceUpdateStats = true;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;

            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri(URL + "/api/v1/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            Client.DefaultRequestHeaders.Add("UserAgent",
                                             "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36");

            Client.DefaultRequestHeaders.Add("Origin", "https://wolf.bet");

            Client.DefaultRequestHeaders.Add("Accept",
                                             "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");

            try
            {
                if (Password != null) Client.DefaultRequestHeaders.Add("authorization", "Bearer " + Password);
                var sEmitResponse = Client.GetStringAsync("user/balances").Result;

                try
                {
                    var tmpProfile = JsonUtils.JsonDeserialize<WolfBetProfile>(sEmitResponse);

                    if (tmpProfile.balances != null)
                    {
                        //set balance here
                        foreach (var x in tmpProfile.balances)
                        {
                            if (x.currency.ToLower() == Currency.ToLower())
                            {
                                balance = decimal.Parse(x.amount, NumberFormatInfo.InvariantInfo);
                                Parent.updateBalance(balance);
                            }
                        }

                        //get stats
                        //set stats
                        sEmitResponse = Client.GetStringAsync("user/stats/bets").Result;
                        var tmpStats = JsonUtils.JsonDeserialize<WolfBetStats>(sEmitResponse);
                        UpdateStats(tmpStats);
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(GetBalanceThread).Start();
                        finishedlogin(true);

                        return;
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                    Parent.DumpLog(sEmitResponse, -1);
                    Parent.updateStatus("Error: " + sEmitResponse);
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }

            finishedlogin(false);
        }

        private void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if ((DateTime.Now - lastupdate).TotalSeconds > 30 || ForceUpdateStats)
                    {
                        lastupdate = DateTime.Now;
                        ForceUpdateStats = false;
                        var sEmitResponse = Client.GetStringAsync("user/balances").Result;
                        var tmpProfile = JsonUtils.JsonDeserialize<WolfBetProfile>(sEmitResponse);

                        if (tmpProfile.balances != null)
                        {
                            //set balance here
                            foreach (var x in tmpProfile.balances)
                            {
                                if (x.currency.ToLower() == Currency.ToLower())
                                {
                                    balance = decimal.Parse(x.amount, NumberFormatInfo.InvariantInfo);
                                    Parent.updateBalance(balance);
                                }
                            }

                            //get stats
                            //set stats
                            sEmitResponse = Client.GetStringAsync("user/stats/bets").Result;
                            var tmpStats = JsonUtils.JsonDeserialize<WolfBetStats>(sEmitResponse);
                            UpdateStats(tmpStats);
                        }
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }

                Thread.Sleep(100);
            }
        }

        private void UpdateStats(WolfBetStats Stats)
        {
            try
            {
                var tmp = typeof(Dice).GetProperty(Currency.ToLower());

                if (tmp != null)
                {
                    var stat = tmp.GetValue(Stats.dice) as WBStat;

                    if (stat != null)
                    {
                        bets = int.Parse(stat.total_bets);
                        wins = int.Parse(stat.win);
                        losses = int.Parse(stat.lose);
                        wagered = decimal.Parse(stat.waggered, NumberFormatInfo.InvariantInfo);
                        profit = decimal.Parse(stat.profit, NumberFormatInfo.InvariantInfo);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateWagered(wagered);
                        Parent.updateProfit(profit);
                    }
                }
            }
            catch
            {
                bets = 0;
                wins = 0;
                losses = 0;
                wagered = 0;
                profit = 0;
                Parent.updateBets(bets);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
                Parent.updateWagered(wagered);
                Parent.updateProfit(profit);
            }
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void ResetSeed()
        {
            try
            {
                var Result = Client.GetAsync("game/seed/refresh").Result.Content.ReadAsStringAsync().Result;
                HttpContent cont = new StringContent("{\"game\":\"dice\"}");
                cont.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Result = Client.PostAsync("game/seed/refresh", cont).Result.Content.ReadAsStringAsync().Result;
            }
            catch
            {
            }
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chancem, string BetGuid)
        {
            this.High = High;
            new Thread(placebetthread).Start(new PlaceBetObj(High, amount, chance, BetGuid));
        }

        private void placebetthread(object obj)
        {
            try
            {
                var tmp5 = obj as PlaceBetObj;

                var tmp = new WolfPlaceBet
                {
                    amount = tmp5.Amount.ToString("0.00000000", NumberFormatInfo.InvariantInfo),
                    currency = Currency,
                    rule = tmp5.High ? "over" : "under",
                    multiplier = ((100m - edge) / tmp5.Chance).ToString("0.####", NumberFormatInfo.InvariantInfo),
                    bet_value = (High ? maxRoll - tmp5.Chance : tmp5.Chance).ToString("0.##", NumberFormatInfo.InvariantInfo),
                    game = "dice"
                };

                var LoginString = JsonUtils.JsonSerializer(tmp);
                HttpContent cont = new StringContent(LoginString);
                cont.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resp2 = Client.PostAsync("bet/place", cont).Result;

                if (resp2.IsSuccessStatusCode)
                {
                }

                var sEmitResponse = resp2.Content.ReadAsStringAsync().Result;

                try
                {
                    var result = JsonUtils.JsonDeserialize<WolfBetResult>(sEmitResponse);

                    if (result.bet != null)
                    {
                        var tmpRsult = new Bet
                        {
                            Amount = decimal.Parse(result.bet.amount, NumberFormatInfo.InvariantInfo),
                            Chance = tmp5.High
                                         ? maxRoll - decimal.Parse(result.bet.bet_value, NumberFormatInfo.InvariantInfo)
                                         : decimal.Parse(result.bet.bet_value, NumberFormatInfo.InvariantInfo),
                            clientseed = result.bet.user_seed,
                            date = DateTime.Now,
                            Currency = Currency,
                            Guid = tmp5.Guid,
                            nonce = result.bet.nonce,
                            Id = result.bet.hash,
                            high = tmp5.High,
                            Roll = decimal.Parse(result.bet.result_value, NumberFormatInfo.InvariantInfo),
                            Profit = decimal.Parse(result.bet.profit, NumberFormatInfo.InvariantInfo),
                            serverhash = result.bet.server_seed_hashed
                        };

                        bets++;
                        var Win = High ? tmpRsult.Roll > maxRoll - tmpRsult.Chance : tmpRsult.Roll < tmpRsult.Chance;

                        if (Win)
                            wins++;
                        else losses++;

                        wagered += amount;
                        profit += tmpRsult.Profit;
                        balance = result.userBalance.amount;
                        FinishedBet(tmpRsult);
                    }
                    else
                    {
                        Parent.DumpLog(sEmitResponse, -1);
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                    Parent.DumpLog(sEmitResponse, -1);
                    Parent.updateStatus("Error: " + sEmitResponse);
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = new HMACSHA256();

            var charstouse = 5;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            var buffer = new List<byte>();
            var msg = /*nonce.ToString() + ":" + */client + "_" + nonce;

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

                decimal lucky = int.Parse(s, NumberStyles.HexNumber);

                if (lucky < 1000000)
                    return lucky % 10000 / 100m;
            }

            return 0;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            var x = $"{{\"amount\":\"{Amount.ToString(NumberFormatInfo.InvariantInfo)}\",\"currency\":\"{Currency}\",\"address\":\"{Address}\"}}";
            HttpContent cont = new StringContent(x);
            cont.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp2 = Client.PostAsync("bet/place", cont).Result;

            return resp2.IsSuccessStatusCode;
        }
    }

    public class WolfBetLogin
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }

    public class Preferences
    {
        public bool public_total_profit { get; set; }
        public bool public_total_wagered { get; set; }
        public bool public_bets { get; set; }
    }

    public class Balance
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string withdraw_fee { get; set; }
        public string withdraw_minimum_amount { get; set; }
        public bool payment_id_required { get; set; }
    }

    public class Game2
    {
        public string name { get; set; }
    }

    public class Game
    {
        public string server_seed_hashed { get; set; }
        public Game2 game { get; set; }
    }

    public class User
    {
        public string login { get; set; }
        public string email { get; set; }
        public bool two_factor_authentication { get; set; }
        public bool has_email_to_verify { get; set; }
        public string last_nonce { get; set; }
        public string seed { get; set; }
        public string channel { get; set; }
        public string joined { get; set; }

        public List<Balance> balances { get; set; }
        public List<Game> games { get; set; }
    }

    public class History
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public int step { get; set; }
        public long published_at { get; set; }
    }

    public class Values
    {
        public string btc { get; set; }
        public string eth { get; set; }
        public string ltc { get; set; }
        public string doge { get; set; }
        public string trx { get; set; }
        public string bch { get; set; }
    }

    public class Next
    {
        public decimal step { get; set; }
        public Values values { get; set; }
    }

    public class DailyStreak
    {
        public List<History> history { get; set; }
        public Next next { get; set; }
    }

    public class WolfBetProfile
    {
        public User user { get; set; }
        public List<Balance> balances { get; set; }
    }

    public class WBStat
    {
        public string total_bets { get; set; }
        public string win { get; set; }
        public string lose { get; set; }
        public string waggered { get; set; }
        public string currency { get; set; }
        public string profit { get; set; }
    }

    public class Dice
    {
        public WBStat doge { get; set; }
        public WBStat btc { get; set; }
        public WBStat eth { get; set; }
        public WBStat ltc { get; set; }
        public WBStat trx { get; set; }
        public WBStat bch { get; set; }
    }

    public class WolfBetStats
    {
        public Dice dice { get; set; }
    }

    public class WolfPlaceBet
    {
        public string currency { get; set; }
        public string game { get; set; }
        public string amount { get; set; }
        public string rule { get; set; }
        public string multiplier { get; set; }
        public string bet_value { get; set; }
    }

    public class WBBet
    {
        public string hash { get; set; }
        public int nonce { get; set; }
        public string user_seed { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string profit { get; set; }
        public string multiplier { get; set; }
        public string bet_value { get; set; }
        public string result_value { get; set; }
        public string state { get; set; }
        public int published_at { get; set; }
        public string server_seed_hashed { get; set; }
        public User user { get; set; }
        public Game game { get; set; }
    }

    public class UserBalance
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string withdraw_fee { get; set; }
        public string withdraw_minimum_amount { get; set; }
        public bool payment_id_required { get; set; }
    }

    public class WolfBetResult
    {
        public WBBet bet { get; set; }
        public UserBalance userBalance { get; set; }
    }
}
