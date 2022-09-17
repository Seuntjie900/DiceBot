using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
using DiceBot.Schema.BetKing;
using WebSocket4Net;

namespace DiceBot.Schema.BetKing
{

    public class BKAccount
    {

        public string id { get; set; }
        public string username { get; set; }
        public string email { get; set; }

    }
    public class BKStat
    {
        public string num_bets { get; set; }
        public string wagered { get; set; }
        public string profit { get; set; }
        public int currency { get; set; }
    }
    public class bkGameDetails
    {
        public double roll { get; set; }
        public double chance { get; set; }
        public int target { get; set; }
    }

    public class bkPlaceBet
    {
        public string id { get; set; }
        public string date { get; set; }
        public string bet_amount { get; set; }
        public int currency { get; set; }
        public string profit { get; set; }
        public bkGameDetails game_details { get; set; }
        public string game_type { get; set; }
        public string balance { get; set; }
        public int nextNonce { get; set; }
        public string error { get; set; }
    }
    public class BKCurrency
    {
        public int id { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public int scale { get; set; }
        public string max_withdraw_limit { get; set; }
        public string min_withdraw_limit { get; set; }
        public string withdrawal_fee { get; set; }
        public string no_throttle_amount { get; set; }
        public string min_tip { get; set; }
        public string address_type { get; set; }

        public decimal EffectiveScale
        {
            get
            {
                return (decimal)Math.Pow(10.0, (double)scale);
            }
        }
    }

    public class bkGetCurrencies
    {
        public List<BKCurrency> currencies { get; set; }
    }
    public class bkBalance
    {
        public string balance { get; set; }
        public int currency { get; set; }
    }

    public class bkGetBalances
    {
        public List<bkBalance> balances { get; set; }
    }
    public class bkLoadSTate
    {
        public string clientSeed { get; set; }
        public string serverSeedHash { get; set; }
        public int nonce { get; set; }
        public string maxWin { get; set; }
        public string minBetAmount { get; set; }
        public bool isBettingDisabled { get; set; }
    }
}

namespace DiceBot
{
    class BetKing : DiceSite
    {
        //WebSocket Sock;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        CookieContainer cookies = new CookieContainer();
        string clientseed = "";
        DBRandom R = new DBRandom();
        bkGetCurrencies Currs = null;
        BKCurrency CurrentCurrency = null;
        public static string[] sCurrencies = new string[] { "Btc", "Eth", "BKT", "Ltc","EOS"/*,"OMG",
"TRX",
"SNT",
"PPT",
"GNT",
"REP",
"VERI",
"SALT",
"BAT",
"FUN",
"POWR",
"PAY",
"ZRX",
"CVC" */};
        Dictionary<string, int> Curs = new Dictionary<string, int>();
        public BetKing(cDiceBot Parent)
        {
            this.Parent = Parent;
            _UsernameText = "Username/Email:";
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://betking.io/bets/";

            this.Parent = Parent;
            Name = "BetKing";
            Tip = false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://betking.io?ref=u:seuntjie";

            Currencies = sCurrencies;
            Currency = "Btc";
            Curs.Add("Btc", 0);
            Curs.Add("Eth", 1);
            Curs.Add("Ltc", 3);
            Curs.Add("BKB", 35);
            Curs.Add("EOS", 9);
            /*Curs.Add("OmiseGo", 7);
            Curs.Add("TRON", 8);
            Curs.Add("EOS", 9);
            Curs.Add("Status", 11);
            Curs.Add("Populous", 12);
            Curs.Add("Golem", 13);
            Curs.Add("Augur", 15);
            Curs.Add("Veritaseum", 16);
            Curs.Add("SALT", 17);
            Curs.Add("Basic Attention Token", 18);
            Curs.Add("FunFair", 19);
            Curs.Add("Power Ledger", 21);
            Curs.Add("TenX", 24);
            Curs.Add("0x", 25);
            Curs.Add("CIVIC",28);*/
        }
        protected override void CurrencyChanged()
        {
            if (Currs != null)
            {
                foreach (BKCurrency x in Currs.currencies)
                {
                    if (x.symbol.ToLower() == Currency.ToLower())
                    {
                        CurrentCurrency = x;
                        break;
                    }
                }
                ForceUpdateStats = true;
            }
        }

        public override void Disconnect()
        {
            ispd = false;
            /*if (Sock != null)
            {
                if (Sock.State== WebSocketState.Open)
                
                    try
                    {
                        Sock.Close();
                    }
                    catch
                    { }
            }
            */
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        int nonce = 0;
        string serverseedhash = "";

        void GetBlanaceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 25 || ForceUpdateStats)
                {
                    ForceUpdateStats = false;
                    lastupdate = DateTime.Now;
                    GetBalance();
                    GetStats();
                    //Sock.Send("2");
                }
                Thread.Sleep(1000);
            }
        }

        void GetBalance()
        {
            if (clientseed == null)
                clientseed = R.Next(0, int.MaxValue).ToString();
            HttpResponseMessage Msg = Client.GetAsync("api/wallet/balances").Result;
            if (Msg.IsSuccessStatusCode)
            {
                string Response = Msg.Content.ReadAsStringAsync().Result;
                bkGetBalances tmp = json.JsonDeserialize<bkGetBalances>(Response);
                foreach (var x in tmp.balances)
                {

                    if (CurrentCurrency.id == x.currency && CurrentCurrency.symbol.ToLower() == Currency.ToLower())
                    {
                        this.balance = decimal.Parse(x.balance, System.Globalization.NumberFormatInfo.InvariantInfo) / (decimal)CurrentCurrency.EffectiveScale;

                    }


                }

                Parent.updateBalance(balance);
                Parent.updateProfit(profit);
                Parent.updateBets(bets);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
            }
        }



        void GetStats()
        {
            HttpResponseMessage Msg = Client.GetAsync("https://betking.io/socket-api/stats/my-stats").Result;
            if (Msg.IsSuccessStatusCode)
            {
                string Response = Msg.Content.ReadAsStringAsync().Result;

                BKStat[] tmp = json.JsonDeserialize<BKStat[]>(Response);
                foreach (BKStat x in tmp)
                {
                    foreach (BKCurrency y in Currs.currencies)
                    {
                        if (CurrentCurrency.id == x.currency && CurrentCurrency.symbol.ToLower() == Currency.ToLower())
                        {
                            this.profit = decimal.Parse(x.profit, System.Globalization.NumberFormatInfo.InvariantInfo) / (decimal)CurrentCurrency.EffectiveScale;
                            this.bets = int.Parse(x.num_bets, System.Globalization.NumberFormatInfo.InvariantInfo);
                            this.wagered = decimal.Parse(x.wagered, System.Globalization.NumberFormatInfo.InvariantInfo) / (decimal)CurrentCurrency.EffectiveScale;
                        }
                    }

                }
                string LoadState = Client.GetStringAsync("api/dice/load-state?clientSeed=" + R.Next(0, int.MaxValue) + "&currency=0").Result;
                bkLoadSTate TmpState = json.JsonDeserialize<bkLoadSTate>(LoadState);
                nonce = TmpState.nonce;
                clientseed = TmpState.clientSeed;
                serverseedhash = TmpState.serverSeedHash;


                Parent.updateProfit(profit);
                Parent.updateBets(bets);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
            }
        }

        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, AllowAutoRedirect = true };
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://betking.io/") };
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                cookies = new CookieContainer();
                ClientHandlr.CookieContainer = cookies;
                Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36");

                string s1 = "";
                HttpResponseMessage resp = Client.GetAsync("").Result;
                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Windows.Forms.MessageBox.Show("betking.io has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "betking.io"))
                        {
                            finishedlogin(false);
                            return;
                        }
                    }
                    else
                    {

                    }
                }
                resp = Client.GetAsync("").Result;
                s1 = resp.Content.ReadAsStringAsync().Result;

                s1 = s1.Substring(s1.IndexOf("window.settings"));
                s1 = s1.Substring(s1.IndexOf("\"csrfToken\":\"") + "\"csrfToken\":\"".Length);

                string csrf = s1.Substring(0, s1.IndexOf("\""));
                Client.DefaultRequestHeaders.Add("csrf-token", csrf);

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                // pairs.Add(new KeyValuePair<string, string>("_csrf", csrf));
                //pairs.Add(new KeyValuePair<string, string>("client_id", "0"));
                pairs.Add(new KeyValuePair<string, string>("fingerprint", "DiceBot-" + Process.GetCurrentProcess().Id));
                pairs.Add(new KeyValuePair<string, string>("loginmethod", Username.Contains("@") ? "email" : "username"));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //pairs.Add(new KeyValuePair<string, string>("redirect_uri", "https://betking.io/bet"));
                pairs.Add(new KeyValuePair<string, string>("otp", twofa));
                pairs.Add(new KeyValuePair<string, string>("rememberme", "false"));
                pairs.Add(new KeyValuePair<string, string>(Username.Contains("@") ? "email" : "username", Username));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage RespMsg = Client.PostAsync("api/auth/login", Content).Result;
                string responseUri = RespMsg.RequestMessage.RequestUri.ToString();
                string sEmitResponse = RespMsg.Content.ReadAsStringAsync().Result;

                if (!sEmitResponse.ToLower().Contains("error"))
                {
                    BKAccount tmpAccount = json.JsonDeserialize<BKAccount>(sEmitResponse);
                    this.username = Username;

                    sEmitResponse = Client.GetStringAsync("api/wallet/currencies").Result;
                    Currs = json.JsonDeserialize<bkGetCurrencies>(sEmitResponse);

                    if (Currs == null)
                    {
                        Parent.DumpLog("Failed to get currencies", 0);
                        finishedlogin(false);
                        return;
                    }
                    foreach (BKCurrency x in Currs.currencies)
                    {
                        if (x.symbol.ToLower() == Currency.ToLower())
                        {
                            CurrentCurrency = x;
                        }
                    }
                    resp = Client.GetAsync("bet/dice").Result;
                    s1 = resp.Content.ReadAsStringAsync().Result;

                    s1 = s1.Substring(s1.IndexOf("window.settings"));
                    s1 = s1.Substring(s1.IndexOf("\"csrfToken\":\"") + "\"csrfToken\":\"".Length);

                    csrf = s1.Substring(0, s1.IndexOf("\""));
                    Client.DefaultRequestHeaders.Remove("csrf-token");
                    Client.DefaultRequestHeaders.Add("csrf-token", csrf);
                    GetBalance();
                    GetStats();


                    string LoadState = Client.GetStringAsync("api/dice/load-state?clientSeed=" + R.Next(0, int.MaxValue) + "&currency=0").Result;
                    bkLoadSTate TmpState = json.JsonDeserialize<bkLoadSTate>(LoadState);
                    nonce = TmpState.nonce;
                    clientseed = TmpState.clientSeed;
                    serverseedhash = TmpState.serverSeedHash;
                    finishedlogin(true);
                    return;

                }

            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
                finishedlogin(false);
                return;
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
        string Guid = "";
        void PlaceBetThread(object BetObj)
        {
            try
            {
                PlaceBetObj tmpob = BetObj as PlaceBetObj;
                //LastBetAmount = (double)tmpob.Amount;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

                pairs.Add(new KeyValuePair<string, string>("betAmount", "\"" + (amount * (decimal)CurrentCurrency.EffectiveScale).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo) + "\""));
                pairs.Add(new KeyValuePair<string, string>("chance", chance.ToString("0.#####", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("currency", CurrentCurrency.id.ToString()));
                pairs.Add(new KeyValuePair<string, string>("target", tmpob.High ? "1" : "0"));
                string loginjson = "{\"betAmount\":\"" + (amount * (decimal)CurrentCurrency.EffectiveScale).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo) +
                    "\",\"currency\":" + CurrentCurrency.id.ToString() +
                    ",\"target\":" + (tmpob.High ? "1" : "0") + ",\"chance\":" + chance.ToString("0.#####", System.Globalization.NumberFormatInfo.InvariantInfo) +
                    ",\"clientSeed\":\"" + clientseed + "\"}";
                HttpContent cont = new StringContent(loginjson);
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                HttpResponseMessage tmpmsg = Client.PostAsync("api/dice/bet", cont).Result;
                string sEmitResponse = tmpmsg.Content.ReadAsStringAsync().Result;
                bkPlaceBet tmpBet = json.JsonDeserialize<bkPlaceBet>(sEmitResponse);
                if (tmpBet.error != null)
                {
                    Parent.DumpLog(tmpBet.error, 1);
                    if (tmpBet.error == ("MIN_BET_AMOUNT_FOR_CURRENCYNAME_IS"))
                    {

                        Parent.updateStatus("Bet too small");
                    }
                    else
                    {
                        Parent.updateStatus("Bet too small");
                    }
                }
                this.balance = decimal.Parse(tmpBet.balance, System.Globalization.NumberFormatInfo.InvariantInfo) / CurrentCurrency.EffectiveScale;

                Bet newBet = new Bet
                {
                    Amount = tmpob.Amount,
                    date = DateTime.Now,
                    Chance = tmpob.Chance,
                    Guid = tmpob.Guid,
                    Currency = Currency,
                    high = tmpob.High,
                    nonce = tmpBet.nextNonce - 1,
                    Roll = (decimal)tmpBet.game_details.roll,
                    UserName = username,
                    Id = tmpBet.id,
                    serverhash = serverseedhash,
                    clientseed = clientseed
                };
                bool win = false;
                if ((newBet.Roll > maxRoll - newBet.Chance && tmpob.High) || (newBet.Roll < newBet.Chance && !tmpob.High))
                {
                    win = true;

                }
                if (win)
                {
                    newBet.Profit = (newBet.Amount * (((100m - edge) / chance) - 1));

                    wins++;
                }
                else
                {
                    newBet.Profit -= newBet.Amount;
                    losses++;
                }
                profit += newBet.Profit;
                wagered += newBet.Amount;
                bets++;
                FinishedBet(newBet);
                return;
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }

        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            /* try
             {
                 string x = Client.GetStringAsync(string.Format("https://api.betking.io/api/stats/getuserstats?appId={0}&userName={1}", 0, User)).Result;
                 BKGetStats y = json.JsonDeserialize<BKGetStats>(x);
                 if (y == null)
                     return false;


                 string cont = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{receiverId:\"{0}\",amount:\"{1:0.00000000}\",currency:{2},code:null}}", y.account.id, amount, Curs[Currency]);
                 var content = new StringContent(cont, Encoding.UTF8, "application/json");

                 HttpResponseMessage response = Client.PostAsync("https://betking.io/bet/api/account/send-tip", content).Result;
                 ForceUpdateStats = (response.IsSuccessStatusCode);
                 return response.IsSuccessStatusCode;
             }
             catch (Exception e)
             {
                 Parent.DumpLog(e.ToString(), -1);
             }*/
            return false;

        }

        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();
            server = nonce.ToString() + ":" + server + ":" + nonce.ToString();
            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = nonce.ToString() + ":" + client + ":" + nonce.ToString();
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
