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
    internal class CryptoGames : DiceSite
    {
        public static string[] sCurrencies = {"BTC", "Doge", "ETH", "DASH", "GAS", "Bch", "STRAT", "PPC", "PLAY", "LTC", "XMR", "ETC"};
        private readonly Random ClientSeedGen = new Random();
        private string accesstoken = "";
        private HttpClient Client; // = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        private HttpClientHandler ClientHandlr;
        private string CurrenyHash = "";

        public bool iscg;
        private DateTime lastupdate;
        private long uid = 0;
        private string username = "";

        public CryptoGames(cDiceBot Parent)
        {
            _PasswordText = "API Key: ";
            NonceBased = false;
            this.Parent = Parent;
            AutoInvest = false;
            AutoLogin = true;
            AutoWithdraw = false;
            ChangeSeed = false;
            edge = 0.8m;
            maxRoll = 99.999m;
            Currencies = sCurrencies;
            Currency = "btc";
            register = false;
            SiteURL = "https://www.crypto.games?i=KaSwpL1Bky";
            BetURL = "https://www.crypto.games/fair.aspx?coin=BTC&type=3&id=";
            Tip = false;
            Name = "CryptoGames";
        }

        protected override void CurrencyChanged()
        {
            lastupdate = DateTime.Now.AddSeconds(-61);
        }

        private void GetBalanceThread()
        {
            try
            {
                while (iscg)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;

                        try
                        {
                            var sEmitResponse = Client.GetStringAsync("user/" + Currency + "/" + accesstoken).Result;
                            var tmpBal = JsonUtils.JsonDeserialize<cgUser>(sEmitResponse);
                            balance = tmpBal.Balance;
                            wagered = tmpBal.Wagered;
                            profit = tmpBal.Profit;
                            bets = tmpBal.TotalBets;
                            Parent.updateBalance(balance);
                            Parent.updateBets(bets);
                            Parent.updateLosses(losses);
                            Parent.updateProfit(profit);
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
            catch
            {
            }
        }

        private void PlaceBetThread(object _High)
        {
            var tmp9 = _High as PlaceBetObj;
            var High = tmp9.High;
            var amount = tmp9.Amount;
            var chance = tmp9.Chance;
            var Clients = ClientSeedGen.Next(0, int.MaxValue).ToString();
            var payout = decimal.Parse(((100m - edge) / chance).ToString("0.0000"));
            var tmpPlaceBet = new cgPlaceBet {Bet = amount, ClientSeed = Clients, UnderOver = High, Payout = payout};

            var post = JsonUtils.JsonSerializer(tmpPlaceBet);
            HttpContent cont = new StringContent(post);
            cont.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            /*List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("value", post));
            //pairs.Add(new KeyValuePair<string, string>("affiliate", "seuntjie"));
            FormUrlEncodedContent cont = new FormUrlEncodedContent(pairs);*/

            try
            {
                var sEmitResponse = Client.PostAsync("placebet/" + Currency + "/" + accesstoken, cont).Result.Content.ReadAsStringAsync().Result;
                var Response = JsonUtils.JsonDeserialize<cgGetBet>(sEmitResponse);

                if (Response.Message != "" && Response.Message != null)
                {
                    Parent.updateStatus(Response.Message);

                    return;
                }

                var bet = new Bet
                {
                    Guid = tmp9.Guid,
                    Amount = amount,
                    Profit = Response.Profit,
                    Roll = Response.Roll,
                    Chance = decimal.Parse(Response.Target.Substring(3), NumberFormatInfo.InvariantInfo),
                    date = DateTime.Now,
                    clientseed = Clients,
                    Currency = Currency,
                    Id = Response.BetId.ToString(),
                    high = Response.Target.Contains(">"),
                    serverhash = CurrenyHash,
                    nonce = -1,
                    serverseed = Response.ServerSeed
                };

                if (bet.high)
                    bet.Chance = maxRoll - bet.Chance;

                CurrenyHash = Response.NextServerSeedHash;
                var Win = bet.high ? bet.Roll > maxRoll - bet.Chance : bet.Roll < bet.Chance;

                if (Win)
                    wins++;
                else
                    losses++;

                bets++;
                wagered += amount;
                balance += Response.Profit;
                profit += Response.Profit;
                FinishedBet(bet);
            }
            catch
            {
            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            new Thread(PlaceBetThread).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        public override void ResetSeed()
        {
        }

        public override void SetClientSeed(string Seed)
        {
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            return false;
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://api.crypto.games/v1/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            try
            {
                accesstoken = Password;

                var sEmitResponse = Client.GetStringAsync("user/" + Currency + "/" + accesstoken).Result;
                var tmpBal = JsonUtils.JsonDeserialize<cgUser>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("nextseed/" + Currency + "/" + accesstoken).Result;
                var tmpSeed = JsonUtils.JsonDeserialize<cgNextSeed>(sEmitResponse);
                CurrenyHash = tmpSeed.NextServerSeedHash;
                balance = tmpBal.Balance;
                wagered = tmpBal.Wagered;
                profit = tmpBal.Profit;
                bets = tmpBal.TotalBets;

                //Get stats
                //assign vals to stats
                Parent.updateBalance(balance);
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                var t = new Thread(GetBalanceThread);
                iscg = true;
                t.Start();

                finishedlogin(true);
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.ToString(), -1);
                finishedlogin(false);
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
                finishedlogin(false);
            }
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
            iscg = false;
        }

        public override void GetSeed(long BetID)
        {
        }

        public override void SendChatMessage(string Message)
        {
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
            var msg = server + client;

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
                    //return lucky / 10000;
                    var tmps = lucky.ToString("000000").Substring(lucky.ToString("000000").Length - 5);

                    return decimal.Parse(tmps) / 1000.0m;
                }
            }

            return 0;
            return 0;
        }
    }

    public class cgBalance
    {
        public decimal Balance { get; set; }
    }

    public class cgPlaceBet
    {
        public decimal Bet { get; set; }
        public decimal Payout { get; set; }
        public bool UnderOver { get; set; }
        public string ClientSeed { get; set; }
    }

    public class cgGetBet
    {
        public long BetId { get; set; }
        public decimal Roll { get; set; }
        public string ClientSeed { get; set; }
        public string Target { get; set; }
        public decimal Profit { get; set; }
        public string NextServerSeedHash { get; set; }
        public string ServerSeed { get; set; }
        public string Message { get; set; }
    }

    public class cgUser
    {
        public string Nickname { get; set; }
        public decimal Balance { get; set; }
        public string Coin { get; set; }
        public int TotalBets { get; set; }
        public decimal Profit { get; set; }
        public decimal Wagered { get; set; }
    }

    public class cgNextSeed
    {
        public string NextServerSeedHash { get; set; }
    }
}
