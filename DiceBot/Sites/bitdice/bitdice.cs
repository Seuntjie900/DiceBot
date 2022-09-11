using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using DiceBot.Core;
using DiceBot.Schema.BitDice;

namespace DiceBot.Schema.BitDice
{
    public class BDSTats
    {
        public decimal balance { get; set; }
        public string currency { get; set; }
        public decimal profit { get; set; }
        public decimal wagered { get; set; }
    }
    public class BDSeed
    {
        public string hash { get; set; }
        public long id { get; set; }
    }
    public class BDUser
    {
        public int level { get; set; }
        public string username { get; set; }
    }

    public class BDBetData
    {
        public decimal chance { get; set; }
        public bool high { get; set; }
        public decimal lucky { get; set; }
        public decimal multiplier { get; set; }
        public bool result { get; set; }
        public long secret { get; set; }
        public decimal target { get; set; }
        public BDUser user { get; set; }
        public long nonce { get; set; }
    }

    public class BDBet
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public BDBetData data { get; set; }
        public long date { get; set; }
        public long game { get; set; }
        public long id { get; set; }
        public decimal profit { get; set; }
        public decimal wagered { get; set; }

    }

    public class BDJackpot
    {
        public bool status { get; set; }
    }

    public class BDOld
    {
        public string client { get; set; }
        public string hash { get; set; }
        public string secret { get; set; }
    }

    public class BDSecret
    {
        public decimal game { get; set; }
        public string hash { get; set; }
        public long id { get; set; }
    }

    public class BDBetResponse
    {
        public string error { get; set; }
        public decimal balance { get; set; }
        public BDBet bet { get; set; }
        public BDJackpot jackpot { get; set; }
        public BDOld old { get; set; }
        public BDSecret secret { get; set; }
    }


}

namespace DiceBot
{
    class bitdice : DiceSite
    {
        bool isbitdice = false;

        public static string[] cCurrencies = new string[] { "btc", "doge", "ltc", "eth", "csno", "eos" };
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        string APIKey = "";
        BDSeed CurrentSeed;
        DBRandom R = new DBRandom();
        public bitdice(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = false;
            _PasswordText = "API Key";
            Tip = true;
            TipUsingName = true;
            ChangeSeed = false;
            NonceBased = false;
            Name = "BitDice";
            this.Parent = Parent;
            SiteURL = "https://www.bitdice.me/?r=65";
            /*Client = new WebSocket("");
            Client.Opened += Client_Opened;
            Client.Error += Client_Error;
            Client.Closed += Client_Closed;
            Client.MessageReceived += Client_MessageReceived;*/
            AutoUpdate = false;
            Currencies = new string[] { "btc", "doge", "ltc", "eth", "csno", "eos" };
            Currency = "BTC";
        }

        public override void Disconnect()
        {
            isbitdice = false;
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
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null }; ;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitdice.me/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            APIKey = Password;
            try
            {
                string Response = Client.GetStringAsync($"user/balance?api_key={APIKey}&currency={Currency}").Result;
                BDSTats tmpStats = json.JsonDeserialize<BDSTats>(Response);
                //Parent.DumpLog(Response, -1);
                string SecretResponse = Client.GetStringAsync($"dice/secret?api_key={APIKey}").Result;
                BDSeed tmpSeed = json.JsonDeserialize<BDSeed>(SecretResponse);
                //Parent.DumpLog(SecretResponse, -1);
                this.balance = tmpStats.balance;
                this.wagered = tmpStats.wagered;
                this.profit = tmpStats.profit;

                Parent.updateBalance(tmpStats.balance);
                Parent.updateWagered(tmpStats.wagered);
                Parent.updateProfit(tmpStats.profit);

                CurrentSeed = tmpSeed;
                lastupdate = DateTime.Now;
                isbitdice = true;
                new Thread(new ThreadStart(GetBalanceThread)).Start();

                finishedlogin(true);
                return;
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
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chancem, BetGuid));
            Parent.DumpLog(amount + ":" + this.amount, -1);
        }

        public void PlaceBetThread(object BetObject)
        {
            try
            {
                var curBet = BetObject as PlaceBetObj;
                string ClientSeed = R.Next(0, int.MaxValue).ToString();
                string Highlow = curBet.High ? "high" : "low";
                string request = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "dice?api_key={0}&currency={1}&amount={2}&chance={3}&type={4}",
                    APIKey,
                    Currency,
                    curBet.Amount,
                    curBet.Chance,
                    Highlow,
                    ClientSeed,
                    CurrentSeed.id);
                var BetResponse = Client.PostAsync(request, new StringContent("")).Result;
                string sbetresult = BetResponse.Content.ReadAsStringAsync().Result;

                BDBetResponse NewBet = json.JsonDeserialize<BDBetResponse>(sbetresult);
                try
                {
                    if (!string.IsNullOrWhiteSpace(NewBet.error))
                    {
                        Parent.updateStatus(NewBet.error);
                        return;
                    }
                    if (CurrentSeed.id != NewBet.bet.data.secret)
                    {
                        string SecretResponse = Client.GetStringAsync($"dice/secret?api_key={APIKey}").Result;
                        BDSeed tmpSeed = json.JsonDeserialize<BDSeed>(SecretResponse);
                        CurrentSeed = tmpSeed;
                    }
                    Bet result = new Bet
                    {
                        Amount = NewBet.bet.amount,
                        date = DateTime.Now,
                        Chance = NewBet.bet.data.chance,
                        clientseed = ClientSeed,
                        Guid = curBet.Guid,
                        Currency = Currency,
                        high = NewBet.bet.data.high,
                        Id = NewBet.bet.id.ToString(),
                        nonce = NewBet.bet.data.nonce,
                        Profit = NewBet.bet.profit,
                        Roll = NewBet.bet.data.lucky,
                        serverhash = CurrentSeed.hash,
                        //serverseed = NewBet.old.secret
                    };


                    //CurrentSeed = new BDSeed { hash = NewBet.secret.hash, id = NewBet.secret.id };
                    bool win = NewBet.bet.data.result;
                    if (win)
                        wins++;
                    else losses++;
                    bets++;
                    wagered += result.Amount;
                    profit += result.Profit;
                    balance = NewBet.balance;
                    FinishedBet(result);
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                    Parent.updateStatus("An unknown error has occurred");
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
                Parent.updateStatus("An unknown error has occurred");
            }

        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }
        DateTime lastupdate = new DateTime();
        void GetBalanceThread()
        {

            while (isbitdice)
            {
                try
                {
                    if (APIKey != "" && ((DateTime.Now - lastupdate).TotalSeconds > 10 || ForceUpdateStats))
                    {
                        string Response = Client.GetStringAsync($"user/balance?api_key={APIKey}&currency={Currency}").Result;
                        BDSTats tmpStats = json.JsonDeserialize<BDSTats>(Response);
                        //Parent.DumpLog(SecretResponse, -1);
                        this.balance = tmpStats.balance;
                        this.wagered = tmpStats.wagered;
                        this.profit = tmpStats.profit;
                        Parent.updateBalance(tmpStats.balance);
                        Parent.updateWagered(tmpStats.wagered);
                        Parent.updateProfit(tmpStats.profit);
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }
                Thread.Sleep(1000);

            }

        }
        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */client;
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
                    return lucky / 10000;
            }
            return 0;
        }
        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

    }

}