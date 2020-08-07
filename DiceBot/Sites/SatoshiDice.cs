using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using DiceBot.Common;
using DiceBot.Forms;

namespace DiceBot.Sites
{
    internal class SatoshiDice : DiceSite
    {
        private string accesstoken = "";
        private HttpClient Client; // = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        private HttpClientHandler ClientHandlr;

        private string curHash = "";
        private long GameID;
        public bool ispd = false;
        private bool IsSatDice;
        private DateTime LastBalance = DateTime.Now;
        private DateTime LastSeedReset = new DateTime();
        private DateTime lastupdate = new DateTime();

        private readonly Random R = new Random();
        private long uid = 0;
        private string username = "";

        public SatoshiDice(cDiceBot Parent)
        {
            _UsernameText = "Email: ";

            this.Parent = Parent;
            maxRoll = 99.99m;
            edge = 1.9m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://megadice.com";
            register = false;
            this.Parent = Parent;
            Name = "MegaDice";
            Tip = false;
            TipUsingName = true;

            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "http://megarefer.com/?a=562654666";
        }

        private void GetBalanceThread()
        {
            while (IsSatDice)
            {
                try
                {
                    if (((DateTime.Now - LastBalance).TotalSeconds > 15 || ForceUpdateStats) && accesstoken != "")
                    {
                        LastBalance = DateTime.Now;
                        var sEmitResponse = Client.GetStringAsync("userbalance/?ctoken=" + accesstoken).Result;
                        var tmpbal = JsonUtils.JsonDeserialize<SatBalance>(sEmitResponse);
                        balance = tmpbal.balanceInSatoshis / 100000000.0m;
                        Parent.updateBalance(balance);
                        LastBalance = DateTime.Now;
                    }
                }
                catch
                {
                }

                Thread.Sleep(500);
            }
        }

        private void PlaceBetThread(object BetObject)
        {
            try
            {
                var obj = BetObject as PlaceBetObj;

                /*List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("ctoken", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("betInSatoshis", (obj.Amount*100000000m).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("id", GameID.ToString()));
                pairs.Add(new KeyValuePair<string, string>("serverHash", curHash));
                pairs.Add(new KeyValuePair<string, string>("clientRoll", "200"));//R.Next(0, int.MaxValue).ToString()));
                pairs.Add(new KeyValuePair<string, string>("belowRollToWin", ((obj.Chance/100m)*65535).ToString("0")));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("placebet.php", Content).Result.Content.ReadAsStringAsync().Result;
                */
                var sEmitResponse = Client.GetStringAsync(string.Format(
                                                                        "placebet.php?ctoken={0}&betInSatoshis={1}&" +
                                                                        "id={2}&serverHash={3}&clientRoll={4}&belowRollToWin={5}",
                                                                        accesstoken,
                                                                        (obj.Amount * 100000000m).ToString("0", NumberFormatInfo.InvariantInfo),
                                                                        GameID,
                                                                        curHash,
                                                                        R.Next(0, int.MaxValue).ToString(),
                                                                        (obj.Chance / 100m * 65535).ToString("0"))).Result;

                var betresult = JsonUtils.JsonDeserialize<SatGame>(sEmitResponse);

                if (betresult.status == "success")
                {
                    var tmpRes = new Bet
                    {
                        Amount = betresult.bet.betInSatoshis / 100000000m,
                        date = DateTime.Now,
                        Chance = decimal.Parse(betresult.bet.probability),
                        clientseed = betresult.clientRoll.ToString(),
                        high = false,
                        Id = betresult.bet.betID.ToString(),
                        nonce = -1,
                        Profit = betresult.bet.profitInSatoshis / 100000000m,
                        serverhash = betresult.serverHash,
                        serverseed = betresult.serverRoll + "-" + betresult.serverSalt,
                        Roll = decimal.Parse(betresult.bet.rollInPercent),
                        Guid = obj.Guid
                    };

                    balance = betresult.userBalanceInSatoshis / 100000000.0m;
                    bets++;

                    if (betresult.bet.result == "loss")
                        losses++;
                    else
                        wins++;

                    wagered += tmpRes.Amount;
                    profit += tmpRes.Profit;
                    curHash = betresult.nextRound.hash;
                    GameID = betresult.nextRound.id;
                    FinishedBet(tmpRes);
                }
                else
                {
                    Parent.updateStatus(betresult.message);
                }
            }
            catch
            {
                Parent.updateStatus("An error has occurred. Bot will retry in 30 seconds.");
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
            try
            {
                var Result = Client.GetStringAsync(string.Format(NumberFormatInfo.InvariantInfo,
                                                                 "withdraw_execute.php?ctoken={0}&address={1}&amount={2:0.00000000}", accesstoken, Address,
                                                                 amount)).Result;

                var tmpres = JsonUtils.JsonDeserialize<SatWithdraw>(Result);

                return tmpres.status == 7;
            }
            catch
            {
            }

            return false;
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            ;
            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://session.megadice.com/userapi/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            try
            {
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("email", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("login.php", Content).Result.Content.ReadAsStringAsync().Result;
                var tmplog = JsonUtils.JsonDeserialize<SatDiceLogin>(sEmitResponse);

                if (tmplog.message.ToLower() == "login successful.")
                {
                    accesstoken = tmplog.ctoken;
                    sEmitResponse = Client.GetStringAsync("useraddress/?ctoken=" + accesstoken).Result;
                    var tmpDep = JsonUtils.JsonDeserialize<SatDepAddress>(sEmitResponse);
                    sEmitResponse = Client.GetStringAsync("userbalance/?ctoken=" + accesstoken).Result;
                    var tmpbal = JsonUtils.JsonDeserialize<SatBalance>(sEmitResponse);
                    sEmitResponse = Client.GetStringAsync("startround/?ctoken=" + accesstoken).Result;
                    var tmpgame = JsonUtils.JsonDeserialize<SatGameRound>(sEmitResponse);
                    curHash = tmpgame.hash;
                    GameID = tmpgame.id;

                    if (GameID > 0 && curHash != "")
                    {
                        Parent.updateBalance(tmpbal.balanceInSatoshis / 100000000m);
                        Parent.updateDeposit(tmpDep.depositaddress);
                        IsSatDice = true;
                        new Thread(GetBalanceThread).Start();
                        finishedlogin(true);

                        return;
                    }
                }
                else
                {
                    finishedlogin(false);

                    return;
                }
            }
            catch
            {
            }

            finishedlogin(true);
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
            IsSatDice = false;
        }

        public override void GetSeed(long BetID)
        {
            //throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            //throw new NotImplementedException();
        }

        public override void Donate(decimal Amount)
        {
            Withdraw(Amount, "1BoHcFQsUSot7jkHJcZMh1iUda3tEjzuBW");
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var dserver = decimal.Parse(server.Substring(0, server.IndexOf("-")), NumberFormatInfo.InvariantInfo);
            var dclient = decimal.Parse(client, NumberFormatInfo.InvariantInfo);
            var res = (dserver + dclient) % 65536m;
            res = res / 65535.0m * 100m;
            res = (int) (res * 1000);
            res /= 1000m;
            res = Math.Round(res, 2);
            res = decimal.Parse(Math.Round(res, 2).ToString("00.00", NumberFormatInfo.InvariantInfo), NumberFormatInfo.InvariantInfo);

            return res;
        }
    }

    public class SatDiceLogin
    {
        public string ctoken { get; set; }
        public string message { get; set; }
        public string secret { get; set; }
        public string status { get; set; }
    }

    public class SatDepAddress
    {
        public string nick { get; set; }
        public string depositaddress { get; set; }
        public double queryTimeInSeconds { get; set; }
    }

    public class SatBalance
    {
        public string nick { get; set; }
        public long balanceInSatoshis { get; set; }
        public string unconfirmedBalanceInsSatoshis { get; set; }
        public string hash { get; set; }
        public long maxProfitInSatoshis { get; set; }
        public double queryTimeInSeconds { get; set; }
    }

    public class SatGameRound
    {
        public long id { get; set; }
        public string hash { get; set; }
        public string welcomeMessage { get; set; }
        public long maxProfitInSatoshis { get; set; }
    }

    public class SatBet
    {
        public string game { get; set; }
        public long betID { get; set; }
        public string betTX { get; set; }
        public string playerNick { get; set; }
        public string playerHash { get; set; }
        public string betType { get; set; }
        public long target { get; set; }
        public string probability { get; set; }
        public int streak { get; set; }
        public int roll { get; set; }
        public string rollInPercent { get; set; }
        public string time { get; set; }
        public string result { get; set; }
        public long betInSatoshis { get; set; }
        public string prize { get; set; }
        public long payoutInSatoshis { get; set; }
        public string payoutTX { get; set; }
        public long profitInSatoshis { get; set; }
    }

    public class SatGame
    {
        public double newLuck { get; set; }
        public string message { get; set; }
        public string serverRoll { get; set; }
        public string serverSalt { get; set; }

        public string serverHash { get; set; }

        //public string clientRoll { get; set; }
        public int resultingRoll { get; set; }
        public int clientRoll { get; set; }
        public long userBalanceInSatoshis { get; set; }
        public SatGameRound nextRound { get; set; }
        public string status { get; set; }
        public SatBet bet { get; set; }
    }

    public class SatWithdraw
    {
        public double amountWithdrawn { get; set; }
        public string transactionId { get; set; }
        public int status { get; set; }
        public string message { get; set; }
        public int confirmationsRequired { get; set; }
        public double balance { get; set; }
    }
}
