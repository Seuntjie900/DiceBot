using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiceBot
{
    class MoneroDice: DiceSite
    {
        HttpClient Client;
        HttpClientHandler ClientHandlr;

        public MoneroDice(cDiceBot Parent)
        {
            this.Parent = Parent;
            maxRoll = 99.99;
            AutoInvest = true;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://monerodice.net/";
            Name = "MoneroDice";
            Tip = false;
            SiteURL = "https://monerodice.net/";
            register = false;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(GetBalanceThread));
            t.Start();
        }
        bool ismd = true;
        DateTime lastupdate = DateTime.Now;
        void GetBalanceThread()
        {
            
                while (ismd)
                {
                    try
            {
                    if (priv != "" && pub != "" && (DateTime.Now - lastupdate).TotalSeconds > 60)
                    {
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("public_key", pub));
                        pairs.Add(new KeyValuePair<string, string>("private_key", priv));
                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                
                        string sEmitResponse = Client.PostAsync("betStats", Content).Result.Content.ReadAsStringAsync().Result;
                        MoneroStat tmp = json.JsonDeserialize<MoneroStat>(sEmitResponse);
                        bets = (int)tmp.bets;
                        wins = (int)tmp.wins;
                        losses = (int)tmp.losses;
                        wagered = tmp.wagered;
                        profit = tmp.profit;
                        balance = tmp.balance;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);
                        Parent.updateWins(wins);
                    }
                    System.Threading.Thread.Sleep(100);
            }
                    catch
                    {

                    }
                }
            
        }

        void PlaceBetThread(object high)
        {
            bool High = (bool)high;
            double prize = amount * ((100.0 - (double)edge) / chance);
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("public_key", pub));
            pairs.Add(new KeyValuePair<string, string>("private_key", priv));
            pairs.Add(new KeyValuePair<string, string>("input_bet", amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("input_prize", prize.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("input_roll_type", (High?"over":"under")));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string sEmitResponse = Client.PostAsync("bet", Content).Result.Content.ReadAsStringAsync().Result;
            monerobase tmp = json.JsonDeserialize<monerobase>(sEmitResponse);
            if (tmp.bet_data.error == null)
            {
                Bet tmp2 = new Bet();
                tmp2.Amount = decimal.Parse(tmp.bet_data.size, System.Globalization.NumberFormatInfo.InvariantInfo);
                tmp2.date = DateTime.Now;
                tmp2.Id = (decimal)tmp.bet_data.bet_id;
                tmp2.Profit = (tmp.bet_data.win) ? decimal.Parse(tmp.bet_data.profit, System.Globalization.NumberFormatInfo.InvariantInfo) : -tmp2.Amount;
                tmp2.Roll = decimal.Parse(tmp.bet_data.roll_result, System.Globalization.NumberFormatInfo.InvariantInfo);
                tmp2.high = tmp.bet_data.roll_type == "over";
                tmp2.Chance = decimal.Parse(tmp.bet_data.win_chance, System.Globalization.NumberFormatInfo.InvariantInfo);
                tmp2.nonce = (long)tmp.bet_data.nonce;
                tmp2.serverhash = tmp.bet_data.hash;
                tmp2.clientseed = tmp.bet_data.seed_user;
                bets++;
                wagered += (double)tmp2.Amount;
                balance = tmp.bet_data.balance;
                profit += (double)tmp2.Profit;
                if (tmp.bet_data.win)
                {
                    wins++;
                }
                else
                {
                    losses++;
                }
                FinishedBet(tmp2);
            }
            else Parent.updateStatus(tmp.bet_data.error);
        }

        protected override void internalPlaceBet(bool High)
        {
            new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(PlaceBetThread)).Start(High);
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

        string pub = "";
        string priv = "";
        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://monerodice.net/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {
                /*List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("public_key", Username));
                pairs.Add(new KeyValuePair<string, string>("private_key", Password));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("betStats", Content).Result.Content.ReadAsStringAsync().Result;
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("public_key", Username));
                pairs.Add(new KeyValuePair<string, string>("private_key", Password));
                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = Client.PostAsync("betStats", Content).Result.Content.ReadAsStringAsync().Result;
                MoneroStat tmp = json.JsonDeserialize < MoneroStat > (sEmitResponse);
                bets = (int)tmp.bets;
                wins = (int)tmp.wins;
                losses = (int)tmp.losses;
                wagered = tmp.wagered;
                profit = tmp.profit;*/
                priv = Password;
                pub = Username;
                /*balance = tmp.balance;
                Parent.updateBalance(balance);
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);*/
                finishedlogin(true);
            }
            catch
            {
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
            ismd = false;
        }

        public override bool Invest(double Amount)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("public_key", pub));
            pairs.Add(new KeyValuePair<string, string>("private_key", priv));
            pairs.Add(new KeyValuePair<string, string>("input_invest_add", Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string sEmitResponse = Client.PostAsync("invest", Content).Result.Content.ReadAsStringAsync().Result;
            return true;
                
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

    public class monerobase
    {
        public MoneroRoll bet_data { get; set; }
    }
    public class MoneroStat
    {
        public double balance { get; set; }
        public double wagered { get; set; }
        public double profit { get; set; }
        public double bets { get; set; }
        public double wins { get; set; }
        public double losses { get; set; }
        public double luck { get; set; }
    }
    public class MoneroRoll
    {
        public long bet_id { get; set; }
        public string seed_user { get; set; }
        public string hash { get; set; }
        public int nonce { get; set; }
        public string size { get; set; }
        public string prize { get; set; }
        public string multiplier { get; set; }
        public string profit { get; set; }
        public string win_chance { get; set; }
        public string roll_type { get; set; }
        public string roll_target { get; set; }
        public string roll_result { get; set; }
        public bool win { get; set; }
        public string bets_count { get; set; }
        public string maximum_profit { get; set; }
        public string error { get; set; }
        public double balance { get; set; }
    }
}
