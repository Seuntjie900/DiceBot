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

namespace DiceBot
{
    class bitdice : DiceSite
    {
        bool isbitdice = false;

        public static string[] cCurrencies = new string[] { "btc", "doge", "ltc", "eth","csno" };
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        string APIKey = "";
        BDSeed CurrentSeed;
        Random R = new Random();
        public bitdice(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = true;
            
            Tip = true;
            TipUsingName = true;
            ChangeSeed = false;
            NonceBased = false;
            Name = "BitDice";
            this.Parent = Parent;
            SiteURL = "https://www.bitdice.me/?r=82";
            /*Client = new WebSocket("");
            Client.Opened += Client_Opened;
            Client.Error += Client_Error;
            Client.Closed += Client_Closed;
            Client.MessageReceived += Client_MessageReceived;*/
            AutoUpdate = false;
            Currencies = new string[] { "btc", "doge", "ltc", "eth", "csno" };
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
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null }; ;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://stage.bitdice.me/api/") };
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
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chance, BetGuid));
        }

        public void PlaceBetThread(object BetObject)
        {
            var curBet = BetObject as PlaceBetObj;
            string ClientSeed = R.Next(0,int.MaxValue).ToString();
            string Highlow = curBet.High?">":"<";
            string BetResponse = Client.GetStringAsync($"dice?api_key={APIKey}&currency={Currency}&amount={curBet.Amount}&chance={curBet.Chance}&type={Highlow}&client={ClientSeed}&secret={CurrentSeed.id}").Result;
            Parent.DumpLog(BetResponse, -1);
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

    }
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
        public int id { get; set; }
    }
}
