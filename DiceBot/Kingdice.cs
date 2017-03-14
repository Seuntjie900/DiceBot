using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class Kingdice:DiceSite
    {

        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool iskd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public string LastHash { get; set; }
        public Kingdice(cDiceBot Parent)
        {
            Name = "KingDice";
            AutoWithdraw = true;
            Tip = true;
            TipUsingName = true;
            this.SiteURL = "https://kingdice.com/#/welcome?aff=221";
            this.register = true;
            this.NonceBased = false;
            this.maxRoll = 99;
            this.ChangeSeed = false;
            this.AutoLogin = true;
            this.AutoInvest = true;
            this.BetURL = "https://kingdice.com/#/welcome?aff=221/";
            this.edge = 1;
            this.Parent = Parent;
            
        }

        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {

                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 15 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        string sEmitResponse = Client.PostAsync("balance.php", Content).Result.Content.ReadAsStringAsync().Result;
                        KDBalance tmpbal = json.JsonDeserialize<KDBalance>(sEmitResponse);
                        if (tmpbal.code=="SUCCESS")
                        {
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("username", username));
                            pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("stats/profile.php", Content).Result.Content.ReadAsStringAsync().Result;
                            KDStat tmpstats = json.JsonDeserialize<KDStat>(sEmitResponse);
                            wagered = tmpstats.wagered / 100000000m;
                            profit = tmpstats.profit / 100000000m;
                            bets = (int)tmpstats.rolls;
                            Parent.updateBets(bets);
                            Parent.updateProfit(profit);
                            Parent.updateWagered(wagered);
                            balance = tmpbal.balance / 100000000m ;
                            Parent.updateBalance(balance);
                        }
                    }
                }
                catch { }
                Thread.Sleep(1000);
            }
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            this.High = High;
            new Thread(new ParameterizedThreadStart(PlacebetThread)).Start(new PlaceBetObj(High, amount, chance));
        }
        Random R = new Random();
        void PlacebetThread(object RollObject)
        {
            try
            {
                string ClientSeed = R.Next(0, 100).ToString();
                PlaceBetObj tmp5 = RollObject as PlaceBetObj;
                decimal amount = tmp5.Amount;
                decimal chance = tmp5.Chance;
                bool High = tmp5.High;
                decimal tmpchance = High ? 99m - chance : chance;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("rollAmount", (amount * 100000000m).ToString("0",System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("rollUnder", tmpchance.ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("mode", High ? "2" : "1"));
                pairs.Add(new KeyValuePair<string, string>("rollClient", ClientSeed));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));


                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("play.php", Content).Result.Content.ReadAsStringAsync().Result;
                //Lastbet = DateTime.Now;
                try
                {
                    KDBet tmp = json.JsonDeserialize<KDBet>(sEmitResponse);
                    if (tmp.roll_id != null && tmp.roll_id != null)
                    {
                        Bet tmpBet = new Bet
                        {
                            Amount = amount,
                            date = DateTime.Now,
                            Id = tmp.roll_id.ToString(),
                            Profit = tmp.roll_profit/100000000m,
                            Roll = tmp.roll_number,
                            high = High,
                            Chance = tmp.probability,
                            nonce = (long)tmp.provablef_serverRoll,
                            serverhash = LastHash,
                            serverseed = tmp.provablef_Hash,
                            clientseed = ClientSeed
                        };
                        if (tmp.roll_result == "win")
                            wins++;
                        else
                            losses++;
                        wagered += amount;
                        profit += tmp.roll_profit / 100000000m;
                        bets++;
                        LastHash = tmp.roll_next.hash;
                        balance = tmp.balance / 100000000m;
                        FinishedBet(tmpBet);
                    }
                    else
                    {
                        Parent.updateStatus("An unknown error has occurred. Bet will retry in 30 seconds.");
                    }
                    //retrycount = 0;
                }
                catch
                {
                    Parent.updateStatus(sEmitResponse);
                }
            }
            catch (Exception e)
            { }
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("amount", (Amount * 100000000).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("address", Address));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("withdraw.php", Content).Result.Content.ReadAsStringAsync().Result;
                ForceUpdateStats = true;
                return sEmitResponse.Contains("SUCCESS");
            }
            catch
            {
                return false;
            }
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("tipamount", (amount * 100000000).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("username", User));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("tip.php", Content).Result.Content.ReadAsStringAsync().Result;
                ForceUpdateStats = true;
                return sEmitResponse.Contains("SUCCESS");
            }
            catch
            {
                return false;
            }
        }

        public override void Donate(decimal Amount)
        {
            SendTip("seuntjie", Amount);
        }

        public override bool Invest(decimal Amount)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("amount", (Amount * 100000000).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("invest/invest.php?", Content).Result.Content.ReadAsStringAsync().Result;

                return sEmitResponse.Contains("SUCCESS");
                ForceUpdateStats = true;
            }
            catch
            {
                return false;
            }
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://kingdice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
            Client.DefaultRequestHeaders.Add("Host","kingdice.com");
            Client.DefaultRequestHeaders.Add("Origin", "https://kingdice.com");
            Client.DefaultRequestHeaders.Add("Referer","https://kingdice.com");

            try
            {
                this.username = Username;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("sdb", "8043d46408307f3ac9d14931ba27c9015349bf21b7b7"));
                pairs.Add(new KeyValuePair<string, string>("2facode", twofa/*==""?"undefined":twofa*/));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("login.php", Content).Result.Content.ReadAsStringAsync().Result;

                KDLogin tmplogin = json.JsonDeserialize<KDLogin>(sEmitResponse);
                if (tmplogin.code=="SUCCESS")
                {
                    accesstoken = tmplogin.token;
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("token", accesstoken));                    
                    Content = new FormUrlEncodedContent(pairs);
                    sEmitResponse = Client.PostAsync("logged.php", Content).Result.Content.ReadAsStringAsync().Result;
                    //sEmitResponse2 = Client.GetStringAsync("logged.php").Result;
                    KDLoggedIn tmpStats = json.JsonDeserialize<KDLoggedIn>(sEmitResponse);
                    if (tmpStats.code == "SUCCESS")
                    {
                        balance = tmpStats.balance / 100000000m;
                        if (tmpStats.address != null)
                            Parent.updateDeposit(tmpStats.address);
                        pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                        Content = new FormUrlEncodedContent(pairs);
                        sEmitResponse = Client.PostAsync("nextroll.php", Content).Result.Content.ReadAsStringAsync().Result;
                        KDNextRoll tmphash = json.JsonDeserialize<KDNextRoll>(sEmitResponse);
                        if (tmphash.code=="SUCCESS")
                        {
                            LastHash = tmphash.round_hash;
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("username", username));
                            pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("stats/profile.php", Content).Result.Content.ReadAsStringAsync().Result;
                            KDStat tmpstats = json.JsonDeserialize<KDStat>(sEmitResponse);
                            wagered = tmpstats.wagered/100000000m;
                            profit = tmpstats.profit / 100000000m;
                            bets = (int)tmpstats.rolls;
                            Parent.updateBets(bets);
                            Parent.updateProfit(profit);
                            Parent.updateWagered(wagered);
                            iskd = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            Parent.updateBalance(balance);
                            finishedlogin(true);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 1);
            }
            finishedlogin(false);
            
        }

        public override bool Register(string username, string password)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://kingdice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", username));
                pairs.Add(new KeyValuePair<string, string>("aff", "221"));
                
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("register.php", Content).Result.Content.ReadAsStringAsync().Result;

                KDLogin tmplogin = json.JsonDeserialize<KDLogin>(sEmitResponse);
                if (tmplogin.code == "SUCCESS")
                {
                    accesstoken = tmplogin.token;
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                    Content = new FormUrlEncodedContent(pairs);
                    sEmitResponse = Client.PostAsync("logged.php", Content).Result.Content.ReadAsStringAsync().Result;
                    //sEmitResponse2 = Client.GetStringAsync("logged.php").Result;
                    KDLoggedIn tmpStats = json.JsonDeserialize<KDLoggedIn>(sEmitResponse);
                    if (tmpStats.code == "SUCCESS")
                    {
                        
                        pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("pass", password));
                        pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                        Content = new FormUrlEncodedContent(pairs);
                        sEmitResponse = Client.PostAsync("setpass.php", Content).Result.Content.ReadAsStringAsync().Result;
                        KDLoggedIn tmpStats2 = json.JsonDeserialize<KDLoggedIn>(sEmitResponse);
                        if (tmpStats2.code == "SUCCESS")
                        {
                            balance = tmpStats.balance / 100000000m;
                            if (tmpStats.address != null)
                                Parent.updateDeposit(tmpStats.address);
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("nextroll.php", Content).Result.Content.ReadAsStringAsync().Result;
                            KDNextRoll tmphash = json.JsonDeserialize<KDNextRoll>(sEmitResponse);
                            if (tmphash.code == "SUCCESS")
                            {
                                
                                LastHash = tmphash.round_hash;

                                iskd = true;
                                Thread t = new Thread(GetBalanceThread);
                                finishedlogin(true);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 1);
            }
            finishedlogin(false);
            return false;
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            iskd = false;
        }

        public override void GetSeed(long BetID)
        {
            
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }
        public override decimal GetLucky(string server, string client, int nonce)
        {
            int toll = (int.Parse(client) + nonce) % ((int)maxRoll + 1);
            
            
            return (decimal)toll;
        }
        public static decimal sGetLucky(string server, string client, int nonce)
        {
            int toll = (int.Parse(client) + nonce) % ((int)99 + 1);


            return (decimal)toll;
        }
    }
    public class KDLogin
    {
        public string msg { get; set; }
        public string code { get; set; }
        public string token { get; set; }
    }
    public class KDLoggedIn
    {
        public decimal balance { get; set; }
        public decimal unconfirmedbalance { get; set; }
        public string username { get; set; }
        public string address { get; set; }
        public string cvalue { get; set; }
        public string code { get; set; }
        public decimal maxProfit { get; set; }     
    }
    public class KDBalance
    {
        public decimal balance { get; set; }
        public decimal unconfirmedbalance { get; set; }
        public decimal maxprofit { get; set; }
        public string code { get; set; } 
    }
    public class KDNextRoll
    {
        public string round_hash { get; set; }
        public long round_id { get; set; }
        public string code { get; set; }
    }
    public class KDBetNextRoll
    {
        public string hash { get; set; }
        public long id { get; set; }
    }
    public class KDBet
    {
        public decimal balance { get; set; }
        public decimal roll_number { get; set; }
        public decimal roll_id { get; set; }
        public decimal roll_under { get; set; }
        public KDBetNextRoll roll_next { get; set; }
        public string roll_time { get; set; }
        public string roll_result { get; set; }
        public decimal roll_bet { get; set; }
        public decimal roll_payout { get; set; }
        public decimal roll_profit { get; set; }
        public decimal roll_mode { get; set; }
        public decimal probability { get; set; }
        public string provablef_Hash { get; set; }
        public string provablef_Salt { get; set; }
        public decimal provablef_clientRoll { get; set; }
        public decimal provablef_serverRoll { get; set; }
    }
    public class KDStat
    {
        public string username { get; set; }
        public string registered { get; set; }
        public string lastOnline { get; set; }
        public decimal rolls { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal luck { get; set; }
    }
}
