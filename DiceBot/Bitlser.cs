using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class Bitsler:DiceSite
    {
        bool IsBitsler = false;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        public static string[] sCurrencies = new string[] { "btc","ltc","doge" };
        HttpClientHandler ClientHandlr;
        public Bitsler(cDiceBot Parent)
        {
            Currencies = new string[] { "btc","ltc","doge" };
            maxRoll = 99.99;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://api.primedice.com/bets/";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "Bitsler";
            Tip =false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://www.bitsler.com/?ref=seuntjie";
            register = false;
            
        }
        void GetBalanceThread()
        {
            while (IsBitsler)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 15)
                {
                    lastupdate = DateTime.Now;
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                    bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    balance = bsstatsbase._return.balance;
                    profit = bsstatsbase._return.profit;
                    wagered = bsstatsbase._return.wagered;
                    bets = int.Parse(bsstatsbase._return.bets);
                    wins = int.Parse(bsstatsbase._return.wins);
                    losses = int.Parse(bsstatsbase._return.losses);
                    Parent.DumpLog(sEmitResponse, 0);
                    Parent.updateBalance(balance);
                    Parent.updateBets(bets);
                    Parent.updateLosses(losses);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    Parent.updateWins(wins);
                }
                Thread.Sleep(1000);
            }
        }

        void PlaceBetThread(object BetObj)
        {
            try
            {
                PlaceBetObj tmpob = BetObj as PlaceBetObj;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                /*access_token
type:dice
amount:0.00000001
condition:< or >
game:49.5
devise:btc*/
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("type", "dice"));
                pairs.Add(new KeyValuePair<string, string>("amount", tmpob.Amount.ToString("0.00000000")));
                pairs.Add(new KeyValuePair<string, string>("condition", tmpob.High?">":"<"));
                pairs.Add(new KeyValuePair<string, string>("game", tmpob.Chance.ToString("0.00")));
                pairs.Add(new KeyValuePair<string, string>("devise", Currency));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("bet", Content).Result.Content.ReadAsStringAsync().Result;
                bsBetBase bsbase = json.JsonDeserialize<bsBetBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                
                if (bsbase!=null)
                    if (bsbase._return!=null)
                        if (bsbase._return.success=="true")
                        {
                            balance = double.Parse(bsbase._return.new_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                            Bet tmp = bsbase._return.ToBet();
                            profit += (double)tmp.Profit;
                            wagered += (double)tmp.Amount;
                            bool win = false;
                            if ((tmp.Roll > 99.99m - tmp.Chance && tmp.high) || (tmp.Roll < tmp.Chance && !tmp.high))
                            {
                                win = true;
                            }
                            //set win
                            if (win)
                                wins++;
                            else
                                losses++;
                            bets++;
                            FinishedBet(tmp);
                        }
                //

            }
            catch
            {
                Parent.updateStatus("An Unknown error has ocurred.");
            }
        }
        protected override void internalPlaceBet(bool High, double amount, double chance)
        {
            System.Threading.Thread tBetThread = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            tBetThread.Start(new PlaceBetObj(High, amount, chance));
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitsler.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //if (!string.IsNullOrWhiteSpace(twofa))
                {
                    pairs.Add(new KeyValuePair<string, string>("two_factor", twofa));
                }

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("login", Content).Result.Content.ReadAsStringAsync().Result;
                Parent.DumpLog(sEmitResponse, 0);
                //getuserstats 
                bsloginbase bsbase = json.JsonDeserialize<bsloginbase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                
                if (bsbase!=null)
                    if (bsbase._return!=null)
                        if (bsbase._return.success=="true")
                        {
                            accesstoken = bsbase._return.access_token;
                            IsBitsler = true;
                            lastupdate = DateTime.Now;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                            bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                            balance = bsstatsbase._return.balance;
                            profit = bsstatsbase._return.profit;
                            wagered = bsstatsbase._return.wagered;
                            bets = int.Parse(bsstatsbase._return.bets);
                            wins = int.Parse(bsstatsbase._return.wins);
                            losses = int.Parse(bsstatsbase._return.losses);
                            Parent.DumpLog(sEmitResponse, 0);
                            Parent.updateBalance(balance);
                            Parent.updateBets(bets);
                            Parent.updateLosses(losses);
                            Parent.updateProfit(profit);
                            Parent.updateWagered(wagered);
                            Parent.updateWins(wins);
                            finishedlogin(true);
                            return;
                        }

            }
            catch 
            {

            }
            finishedlogin(false);
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
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

    public class bsLogin
    {
        public string success { get; set; }
        public string access_token { get; set; }
    }
    public class bsloginbase
    {
        public bsLogin _return { get; set; }
    }
    //"{\"return\":{\"success\":\"true\",\"balance\":1.0e-5,\"wagered\":0,\"profit\":0,\"bets\":\"0\",\"wins\":\"0\",\"losses\":\"0\"}}"
    public class bsStats
    {
        public string success { get; set; }
        public double balance { get; set; }
        public double wagered { get; set; }
        public double profit { get; set; }
        public string bets { get; set; }
        public string wins { get; set; }
        public string losses { get; set; }
    }
    public class bsStatsBase
    {
        public bsStats _return { get; set; }
    }
    public class bsBetBase
    {
        public bsBet _return { get; set; }
    }
    public class bsBet
    {
        public string success { get; set; }
        public string username { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string devise { get; set; }
        public long ts { get; set; }
        public string time { get; set; }
        public string amount { get; set; }
        public double roll_number { get; set; }
        public string condition { get; set; }
        public string game { get; set; }
        public double payout { get; set; }
        public string winning_chance { get; set; }
        public string amount_return { get; set; }
        public string new_balance { get; set; }
        public string _event { get; set; }

        public Bet ToBet()
        {
            Bet tmp = new Bet
            {
                Amount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                date = json.ToDateTime2(ts.ToString()),
                Id = decimal.Parse(id, System.Globalization.CultureInfo.InvariantCulture),
                Profit = decimal.Parse(amount_return, System.Globalization.NumberFormatInfo.InvariantInfo),
                Roll = (decimal)roll_number,
                high = condition == ">",
                Chance = decimal.Parse(winning_chance, System.Globalization.NumberFormatInfo.InvariantInfo),
                nonce = -1,
                serverhash = "",
                clientseed = ""                
            };
            return tmp;
        }
    }
}
