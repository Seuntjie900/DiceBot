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
    internal class NitroDice : DiceSite
    {
        public static string[] sCurrencies = {"Bch", "Btc", "Doge"};
        private string accesstoken = "";
        private HttpClient Client; // = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        private HttpClientHandler ClientHandlr;
        public bool ispd;

        private string lastHash = "";
        private DateTime LastSeedReset = new DateTime();
        private DateTime lastupdate;
        private long nonce = -1;
        private readonly Random r = new Random();
        private long uid = 0;
        private string username = "";

        public NitroDice(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://NitroDice.com/bets/";
            Currencies = sCurrencies;
            this.Parent = Parent;
            Name = "NitroDice";
            Tip = false;
            TipUsingName = true;
            NonceBased = false;
            SiteURL = "http://www.nitrodice.com/?ref=EEqWBD442qC2oxjpmA1g";
            Currency = "Bch";
        }

        public override void Disconnect()
        {
            ispd = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        protected override void CurrencyChanged()
        {
            if (accesstoken != "")
                try
                {
                    var jsoncontent = JsonUtils.JsonSerializer(new NDChangeCoin {coin = Currency});
                    var Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                    var Response = Client.PostAsync("api/changeCoin", Content).Result.Content.ReadAsStringAsync().Result;
                    var getauth = JsonUtils.JsonDeserialize<NDChangeCoin>(Response);
                    ForceUpdateStats = true;
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }
        }

        private void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    {
                        ForceUpdateStats = false;
                        lastupdate = DateTime.Now;
                        var sEmitResponse2 = Client.GetStringAsync("api/stats").Result;
                        var tmpu = JsonUtils.JsonDeserialize<NDGetBalance>(sEmitResponse2);

                        try
                        {
                            sEmitResponse2 = Client.GetStringAsync("sshash").Result;
                            var tmpHash = JsonUtils.JsonDeserialize<NDGetHash>(sEmitResponse2);
                            lastHash = tmpHash.sshash;
                        }
                        catch (Exception e)
                        {
                        }

                        balance = tmpu.balance;
                        profit = tmpu.amountLost + tmpu.amountWon;
                        wins = (int) tmpu.totWins;
                        losses = (int) tmpu.totLosses;
                        bets = (int) tmpu.totBets;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                    }

                    Thread.Sleep(1000);
                }
            }
            catch
            {
            }
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://www.nitrodice.com/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            try
            {
                var jsoncontent = JsonUtils.JsonSerializer(new NDAuth {pass = Password, user = Username, tfa = twofa});
                var Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                var Response = Client.PostAsync("api/auth", Content).Result.Content.ReadAsStringAsync().Result;
                var getauth = JsonUtils.JsonDeserialize<NDGetAuth>(Response);

                if (getauth != null)
                    if (getauth.token != null)
                    {
                        Client.DefaultRequestHeaders.Add("x-token", getauth.token);
                        Client.DefaultRequestHeaders.Add("x-user", Username);
                        accesstoken = getauth.token;
                        CurrencyChanged();
                        var sEmitResponse2 = Client.GetStringAsync("api/stats").Result;
                        var tmpu = JsonUtils.JsonDeserialize<NDGetBalance>(sEmitResponse2);

                        try
                        {
                            sEmitResponse2 = Client.GetStringAsync("sshash").Result;
                            var tmpHash = JsonUtils.JsonDeserialize<NDGetHash>(sEmitResponse2);
                            lastHash = tmpHash.sshash;
                        }
                        catch (Exception e)
                        {
                        }

                        balance = tmpu.balance;
                        profit = tmpu.amountLost + tmpu.amountWon;
                        wins = (int) tmpu.totWins;
                        losses = (int) tmpu.totLosses;
                        bets = (int) tmpu.totBets;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        lastupdate = DateTime.Now;
                        ispd = true;
                        var t = new Thread(GetBalanceThread);
                        t.Start();
                        finishedlogin(true);

                        return;
                    }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }

            finishedlogin(false);
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
            throw new NotImplementedException();
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

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = SHA512.Create();

            var charstouse = 5;

            var buffer = new List<byte>();
            var msg = server + ":" + nonce + ":" + client;

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            var hash = betgenerator.ComputeHash(buffer.ToArray());
            var hex2 = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                hex2.AppendFormat("{0:x2}", b);
            }

            msg = hex2.ToString();
            buffer.Clear();

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            hash = betgenerator.ComputeHash(hash);
            var hex = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            var SIZE = 4;
            var r = new[] {"0", "0", "0", "0"};
            var Hash = hex.ToString();

            if (nonce == 46)
            {
            }

            for (var i = 0; i < hash.Length; ++i)
            {
                try
                {
                    var tmp = int.Parse(r[i % SIZE], NumberStyles.HexNumber) + hash[i];
                    var stringVal = tmp % 256;
                    r[i % SIZE] = stringVal.ToString("X");
                }
                catch
                {
                    r[i % SIZE] = "0";
                }
            }

            var hexres = "";

            for (var i = 0; i < r.Length; i++)
            {
                var tmp = r[i];

                if (tmp.Length < 2)
                {
                }

                hexres += tmp.Length < 2 ? "0" + tmp : tmp;
            }

            var Lucky = long.Parse(hexres, NumberStyles.HexNumber);
            var result = Lucky % 1000000 / 10000m;

            return result;

            return 0;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        private void placebetthread(object bet)
        {
            try
            {
                var tmp5 = bet as PlaceBetObj;
                var amount = tmp5.Amount;
                var chance = tmp5.Chance;
                var High = tmp5.High;
                var clientseed = r.Next(0, int.MaxValue).ToString();

                var jsoncontent = JsonUtils.JsonSerializer(new NDPlaceBet
                {
                    amount = amount.ToString("0.00000000", NumberFormatInfo.InvariantInfo),
                    perc = chance.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                    pos = High ? "hi" : "lo",
                    times = 1,
                    cseed = clientseed
                });

                var Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                var Response = Client.PostAsync("api/bet", Content).Result.Content.ReadAsStringAsync().Result;

                var BetResult = JsonUtils.JsonDeserialize<NDGetBet>(Response);

                if (BetResult.info == null)
                {
                    if (nonce == -1)
                        nonce = BetResult.index;
                    else if (nonce != BetResult.index - 1) Parent.DumpLog("123 NONCE SKIPPED!!!!!! 12345!!!!", -1);

                    nonce = BetResult.index;

                    var tmp = new Bet
                    {
                        Amount = amount,
                        date = DateTime.Now,
                        Chance = chance,
                        clientseed = clientseed,
                        serverhash = lastHash,
                        Guid = tmp5.Guid,
                        high = High,
                        Id = BetResult.no.ToString(),
                        nonce = BetResult.index,
                        Roll = BetResult.n / 10000m,
                        serverseed = BetResult.sseed,
                        Profit = BetResult.amount
                    };

                    SQLiteHelper.InsertSeed(tmp.serverhash, tmp.serverseed);

                    lastHash = BetResult.sshash;
                    bets++;
                    var win = tmp.Roll > 99.99m - tmp.Chance && High || tmp.Roll < tmp.Chance && !High;
                    balance = BetResult.balance;
                    wagered += amount;
                    profit += BetResult.amount;

                    if (win)
                        wins++;
                    else
                        losses++;

                    FinishedBet(tmp);
                }
                else
                {
                    Parent.updateStatus(BetResult.info);
                }
            }
            catch (Exception Ex)
            {
                Parent.DumpLog(Ex.ToString(), -1);
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }
    }

    public class NDGetAuth
    {
        public string token { get; set; }
    }

    public class NDAuth
    {
        public string user { get; set; }
        public string pass { get; set; }
        public string tfa { get; set; }
    }

    public class NDGetBalance
    {
        public decimal balance { get; set; }
        public decimal amountWon { get; set; }
        public decimal amountLost { get; set; }
        public long totWins { get; set; }
        public long totLosses { get; set; }
        public long totBets { get; set; }
    }

    public class NDPlaceBet
    {
        public string perc { get; set; }
        public string pos { get; set; }
        public string amount { get; set; }
        public int times { get; set; }
        public string cseed { get; set; }
    }

    public class NDGetBet
    {
        public long n { get; set; }
        public string r { get; set; }
        public decimal balance { get; set; }
        public long index { get; set; }
        public string sseed { get; set; }
        public string cseed { get; set; }
        public string target { get; set; }
        public long no { get; set; }
        public decimal amount { get; set; }
        public string sshash { get; set; }
        public string info { get; set; }
    }

    public class NDGetHash
    {
        public string sshash { get; set; }
    }

    public class NDChangeCoin
    {
        public string coin { get; set; }
    }
}
