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
    class coinpro:DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool iskd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        public string LastHash { get; set; }
        public coinpro(cDiceBot Parent)
        {
            _PasswordText = "API Key: ";
            Name = "coinpro";
            AutoWithdraw = false;
            Tip = false;
            TipUsingName = true;
            this.SiteURL = "https://coinpro.fit/";
            this.register = false;
            this.NonceBased = false;
            this.maxRoll = 99.99m;

            this.ChangeSeed = false;
            this.AutoLogin = true;
            this.AutoInvest = false;
            this.BetURL = "https://coinpro.fit/";
            this.edge = 0.9m;
            this.Parent = Parent;
            
        }

        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {
                    if ((DateTime.Now - lastupdate).TotalSeconds > 30 || ForceUpdateStats)
                    {
                        lastupdate = DateTime.Now;
                        string Stats = Client.GetStringAsync("userstats").Result;
                        PIOStats tmpstats = json.JsonDeserialize<PIOStats>(Stats);
                        this.balance = ((decimal)tmpstats.user.balance) / 100000000m;
                        this.bets = (int)tmpstats.user.betted_count;
                        this.wagered = ((decimal)tmpstats.user.betted_wager) / 100000000m;
                        this.profit = ((decimal)tmpstats.user.betted_profit) / 100000000m;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWagered(wagered);
                        Parent.updateProfit(profit);
                    }
                }
                catch { }
                Thread.Sleep(1000);
            }
        }
        Random Ra = new Random();
        RandomNumberGenerator R = new System.Security.Cryptography.RNGCryptoServiceProvider();
        
        string lasthash = "";
        void Placebetthread(object Obj)
        {
            try
            {
                PlaceBetObj tmpObj = Obj as PlaceBetObj;
                byte[] bytes = new byte[4];
                R.GetBytes(bytes);
                string seed = ((long)BitConverter.ToUInt32(bytes, 0)).ToString();
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("wager", (tmpObj.Amount).ToString("0.00000000")));
                pairs.Add(new KeyValuePair<string, string>("region", tmpObj.High ? ">" : "<"));
                pairs.Add(new KeyValuePair<string, string>("target", (tmpObj.High ? maxRoll - tmpObj.Chance : tmpObj.Chance).ToString("0.00")));
                pairs.Add(new KeyValuePair<string, string>("odds", tmpObj.Chance.ToString("0.00")));
                pairs.Add(new KeyValuePair<string, string>("clientSeed", seed));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("bet", Content).Result.Content.ReadAsStringAsync().Result;
                PIOBet tmpbet = json.JsonDeserialize<PIOBet>(sEmitResponse);
                if (tmpbet.error!=null)
                {
                    Parent.updateStatus(tmpbet.error);
                    return;
                }
                Bet tmp = new Bet {
                Amount = (decimal)tmpObj.Amount,
                date = DateTime.Now,
                Id = tmpbet.bet_id.ToString(),
                Profit = (decimal)tmpbet.profit / 100000000m,
                Roll = (decimal)tmpbet.outcome,
                high = tmpObj.High,
                Chance = (decimal)tmpObj.Chance,
                nonce = (int)(tmpbet.outcome * 100),
                serverhash = lasthash,
                serverseed = tmpbet.secret.ToString(),
                clientseed = seed
                };
                
                lasthash = tmpbet.next_hash;
                bets++;
                bool Win = (((bool)tmp.high ? (decimal)tmp.Roll > (decimal)maxRoll - (decimal)(tmp.Chance) : (decimal)tmp.Roll < (decimal)(tmp.Chance)));
                if (Win)
                    wins++;
                else
                    losses++;
                wagered += tmpObj.Amount;
                profit += tmp.Profit;
                balance = tmpbet.balance / 100000000m;
                FinishedBet(tmp);
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
                Parent.updateStatus("An unknown error has occured while placing a bet.");
            }
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            new Thread(new ParameterizedThreadStart(Placebetthread)).Start(new PlaceBetObj(High, amount, chance));
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
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://coinpro.fit/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
            Client.DefaultRequestHeaders.Add("Host", "coinpro.fit");
            Client.DefaultRequestHeaders.Add("Origin", "https://coinpro.fit/");
            Client.DefaultRequestHeaders.Add("Referer", "https://coinpro.fit/");

            try
            {
                //ClientHandlr.CookieContainer.Add(new Cookie("socket", Password,"/","coinpro.fit"));
                ClientHandlr.CookieContainer.Add(new Cookie("PHPSESSID", Password, "/", "coinpro.fit"));
                //string page = Client.GetStringAsync()
                string Stats = Client.GetStringAsync("userstats").Result;
                PIOStats tmpstats = json.JsonDeserialize<PIOStats>(Stats);
                accesstoken = Password;
                this.balance = (tmpstats.user.balance) / 100000000m;
                this.bets = (int)tmpstats.user.betted_count;
                this.wagered = (tmpstats.user.betted_wager) / 100000000m;
                this.profit = (tmpstats.user.betted_profit) / 100000000m;
                Parent.updateBalance(balance);
                Parent.updateBets(bets);
                Parent.updateWagered(wagered);
                Parent.updateProfit(profit);
                iskd = true;
                lastupdate = DateTime.Now;
                //lasthash=tmpstats.user.
                new Thread(new ThreadStart(GetBalanceThread)).Start();
                finishedlogin(true);
            }
            catch
            {
                finishedlogin(false);
                return;
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
            iskd = false;
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
}
