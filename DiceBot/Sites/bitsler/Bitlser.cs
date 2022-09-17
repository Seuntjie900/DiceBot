using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    class Bitsler : DiceSite
    {
        bool IsBitsler = false;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();

        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        public static string[] _sCurrencies = new string[] {
            "BTC","ETH","LTC","BCH","XRP","DOGE","DASH",
            "BSV", "ZEC","ETC","NEO","KMD","BTG",
            "DGB","QTUM","STRAT","WAVES","BTSLR","EOS",
            "XLM","USDT","TRX", "ADA", "BNB" };

        public static string[] sCurrencies => _sCurrencies.OrderBy(x => x).ToArray();

        HttpClientHandler ClientHandlr;

        public Bitsler(cDiceBot Parent)
        {
            _MFAText = "API Key";
            ShowXtra = true;


            Currencies = sCurrencies;
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://www.bitsler.com/?ref=seuntjie/";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "Bitsler";
            Tip = false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://www.bitsler.com/?ref=seuntjie";
            register = false;
            AutoUpdate = true;



            ///        }

            DateTime LastBet = DateTime.Now;
            double LastBetAmount = 0;//
        }
        void GetBalanceThread()
        {
            while (IsBitsler)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats)
                {
                    lastupdate = DateTime.Now;
                    try
                    {
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        HttpResponseMessage resp = Client.PostAsync("api/getuserstats", Content).Result;

                        string s1 = "";
                        string sEmitResponse = "";// resp.Content.ReadAsStringAsync().Result;

                        if (resp.IsSuccessStatusCode)
                        {
                            sEmitResponse = resp.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            sEmitResponse = "";
                            if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                s1 = resp.Content.ReadAsStringAsync().Result;
                                //cflevel = 0;
                                System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    System.Windows.Forms.MessageBox.Show(Name + " has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                });
                                if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "www.bitsler.com"))
                                {
                                    //return;
                                }
                            }
                            else
                            {

                            }
                        }
                        if (sEmitResponse != "")
                        {
                            bsStats bsstatsbase = json.JsonDeserialize<bsStats>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                            if (bsstatsbase != null)
                                if (bsstatsbase != null)
                                    if (bsstatsbase.success == "true")
                                    {
                                        switch (Currency.ToLower())
                                        {
                                            case "btc":
                                                balance = bsstatsbase.btc_balance;
                                                profit = bsstatsbase.btc_profit;
                                                wagered = bsstatsbase.btc_wagered; break;
                                            case "ltc":
                                                balance = bsstatsbase.ltc_balance;
                                                profit = bsstatsbase.ltc_profit;
                                                wagered = bsstatsbase.ltc_wagered; break;
                                            case "doge":
                                                balance = bsstatsbase.doge_balance;
                                                profit = bsstatsbase.doge_profit;
                                                wagered = bsstatsbase.doge_wagered; break;
                                            case "eth":
                                                balance = bsstatsbase.eth_balance;
                                                profit = bsstatsbase.eth_profit;
                                                wagered = bsstatsbase.eth_wagered; break;
                                            case "burst":
                                                balance = bsstatsbase.burst_balance;
                                                profit = bsstatsbase.burst_profit;
                                                wagered = bsstatsbase.burst_wagered; break;
                                            case "dash":
                                                balance = bsstatsbase.dash_balance;
                                                profit = bsstatsbase.dash_profit;
                                                wagered = bsstatsbase.dash_wagered; break;
                                            case "zec":
                                                balance = bsstatsbase.zec_balance;
                                                profit = bsstatsbase.zec_profit;
                                                wagered = bsstatsbase.zec_wagered; break;
                                            case "bch":
                                                balance = bsstatsbase.bch_balance;
                                                profit = bsstatsbase.bch_profit;
                                                wagered = bsstatsbase.bch_wagered; break;
                                            case "xmr":
                                                balance = bsstatsbase.xmr_balance;
                                                profit = bsstatsbase.xmr_profit;
                                                wagered = bsstatsbase.xmr_wagered; break;
                                            case "etc":
                                                balance = bsstatsbase.etc_balance;
                                                profit = bsstatsbase.etc_profit;
                                                wagered = bsstatsbase.etc_wagered; break;
                                            case "neo":
                                                balance = bsstatsbase.neo_balance;
                                                profit = bsstatsbase.neo_profit;
                                                wagered = bsstatsbase.neo_wagered; break;
                                            case "strat":
                                                balance = bsstatsbase.strat_balance;
                                                profit = bsstatsbase.strat_profit;
                                                wagered = bsstatsbase.strat_wagered; break;
                                            case "kmd":
                                                balance = bsstatsbase.kmd_balance;
                                                profit = bsstatsbase.kmd_profit;
                                                wagered = bsstatsbase.kmd_wagered; break;
                                            case "xrp":
                                                balance = bsstatsbase.xrp_balance;
                                                profit = bsstatsbase.xrp_profit;
                                                wagered = bsstatsbase.xrp_wagered; break;
                                            case "btg":
                                                balance = bsstatsbase.btg_balance;
                                                profit = bsstatsbase.btg_profit;
                                                wagered = bsstatsbase.btg_wagered; break;
                                            case "qtum":
                                                balance = bsstatsbase.qtum_balance;
                                                profit = bsstatsbase.qtum_profit;
                                                wagered = bsstatsbase.qtum_wagered; break;
                                            case "lsk":
                                                balance = bsstatsbase.lsk_balance;
                                                profit = bsstatsbase.lsk_profit;
                                                wagered = bsstatsbase.lsk_wagered; break;
                                            case "dgb":
                                                balance = bsstatsbase.dgb_balance;
                                                profit = bsstatsbase.dgb_profit;
                                                wagered = bsstatsbase.dgb_wagered; break;
                                            case "waves":
                                                balance = bsstatsbase.waves_balance;
                                                profit = bsstatsbase.waves_profit;
                                                wagered = bsstatsbase.waves_wagered; break;
                                            case "btslr":
                                                balance = bsstatsbase.btslr_balance;
                                                profit = bsstatsbase.btslr_profit;
                                                wagered = bsstatsbase.btslr_wagered; break;
                                            case "bsv":
                                                balance = bsstatsbase.bsv_balance;
                                                profit = bsstatsbase.bsv_profit;
                                                wagered = bsstatsbase.bsv_wagered; break;
                                            case "xlm":
                                                balance = bsstatsbase.xlm_balance;
                                                profit = bsstatsbase.xlm_profit;
                                                wagered = bsstatsbase.xlm_wagered; break;
                                            case "usdt":
                                                balance = bsstatsbase.usdt_balance;
                                                profit = bsstatsbase.usdt_profit;
                                                wagered = bsstatsbase.usdt_wagered; break;
                                            case "trx":
                                                balance = bsstatsbase.trx_balance;
                                                profit = bsstatsbase.trx_profit;
                                                wagered = bsstatsbase.trx_wagered; break;
                                            case "eos":
                                                balance = bsstatsbase.eos_balance;
                                                profit = bsstatsbase.eos_profit;
                                                wagered = bsstatsbase.eos_wagered; break;
                                        }
                                        bets = int.Parse(bsstatsbase.bets, System.Globalization.NumberFormatInfo.InvariantInfo);
                                        wins = int.Parse(bsstatsbase.wins, System.Globalization.NumberFormatInfo.InvariantInfo);
                                        losses = int.Parse(bsstatsbase.losses, System.Globalization.NumberFormatInfo.InvariantInfo);

                                        Parent.updateBalance(balance);
                                        Parent.updateBets(bets);
                                        Parent.updateLosses(losses);
                                        Parent.updateProfit(profit);
                                        Parent.updateWagered(wagered);
                                        Parent.updateWins(wins);
                                    }
                                    else
                                    {
                                        if (bsstatsbase.error != null)
                                        {

                                            Parent.updateStatus(bsstatsbase.error);

                                        }
                                    }
                        }
                    }
                    catch { }
                }
                Thread.Sleep(1000);
            }
        }

        protected override void CurrencyChanged()
        {
            base.CurrencyChanged();
            lastupdate = DateTime.Now;
            if (accesstoken != "" && IsBitsler)
            {
                try
                {
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("api/getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                    bsStats bsstatsbase = json.JsonDeserialize<bsStats>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    if (bsstatsbase != null)
                        if (bsstatsbase != null)
                            if (bsstatsbase.success == "true")
                            {
                                switch (Currency.ToLower())
                                {
                                    case "btc":
                                        balance = bsstatsbase.btc_balance;
                                        profit = bsstatsbase.btc_profit;
                                        wagered = bsstatsbase.btc_wagered; break;
                                    case "ltc":
                                        balance = bsstatsbase.ltc_balance;
                                        profit = bsstatsbase.ltc_profit;
                                        wagered = bsstatsbase.ltc_wagered; break;
                                    case "doge":
                                        balance = bsstatsbase.doge_balance;
                                        profit = bsstatsbase.doge_profit;
                                        wagered = bsstatsbase.doge_wagered; break;
                                    case "eth":
                                        balance = bsstatsbase.eth_balance;
                                        profit = bsstatsbase.eth_profit;
                                        wagered = bsstatsbase.eth_wagered; break;
                                    case "burst":
                                        balance = bsstatsbase.burst_balance;
                                        profit = bsstatsbase.burst_profit;
                                        wagered = bsstatsbase.burst_wagered; break;
                                    case "dash":
                                        balance = bsstatsbase.dash_balance;
                                        profit = bsstatsbase.dash_profit;
                                        wagered = bsstatsbase.dash_wagered; break;
                                    case "zec":
                                        balance = bsstatsbase.zec_balance;
                                        profit = bsstatsbase.zec_profit;
                                        wagered = bsstatsbase.zec_wagered; break;
                                    case "bch":
                                        balance = bsstatsbase.bch_balance;
                                        profit = bsstatsbase.bch_profit;
                                        wagered = bsstatsbase.bch_wagered; break;
                                    case "xmr":
                                        balance = bsstatsbase.xmr_balance;
                                        profit = bsstatsbase.xmr_profit;
                                        wagered = bsstatsbase.xmr_wagered; break;
                                    case "etc":
                                        balance = bsstatsbase.etc_balance;
                                        profit = bsstatsbase.etc_profit;
                                        wagered = bsstatsbase.etc_wagered; break;
                                    case "neo":
                                        balance = bsstatsbase.neo_balance;
                                        profit = bsstatsbase.neo_profit;
                                        wagered = bsstatsbase.neo_wagered; break;
                                    case "strat":
                                        balance = bsstatsbase.strat_balance;
                                        profit = bsstatsbase.strat_profit;
                                        wagered = bsstatsbase.strat_wagered; break;
                                    case "kmd":
                                        balance = bsstatsbase.kmd_balance;
                                        profit = bsstatsbase.kmd_profit;
                                        wagered = bsstatsbase.kmd_wagered; break;
                                    case "xrp":
                                        balance = bsstatsbase.xrp_balance;
                                        profit = bsstatsbase.xrp_profit;
                                        wagered = bsstatsbase.xrp_wagered; break;
                                    case "btg":
                                        balance = bsstatsbase.btg_balance;
                                        profit = bsstatsbase.btg_profit;
                                        wagered = bsstatsbase.btg_wagered; break;
                                    case "qtum":
                                        balance = bsstatsbase.qtum_balance;
                                        profit = bsstatsbase.qtum_profit;
                                        wagered = bsstatsbase.qtum_wagered; break;
                                    case "lsk":
                                        balance = bsstatsbase.lsk_balance;
                                        profit = bsstatsbase.lsk_profit;
                                        wagered = bsstatsbase.lsk_wagered; break;
                                    case "dgb":
                                        balance = bsstatsbase.dgb_balance;
                                        profit = bsstatsbase.dgb_profit;
                                        wagered = bsstatsbase.dgb_wagered; break;
                                    case "waves":
                                        balance = bsstatsbase.waves_balance;
                                        profit = bsstatsbase.waves_profit;
                                        wagered = bsstatsbase.waves_wagered; break;
                                    case "btslr":
                                        balance = bsstatsbase.btslr_balance;
                                        profit = bsstatsbase.btslr_profit;
                                        wagered = bsstatsbase.btslr_wagered; break;
                                    case "bsv":
                                        balance = bsstatsbase.bsv_balance;
                                        profit = bsstatsbase.bsv_profit;
                                        wagered = bsstatsbase.bsv_wagered; break;
                                    case "xlm":
                                        balance = bsstatsbase.xlm_balance;
                                        profit = bsstatsbase.xlm_profit;
                                        wagered = bsstatsbase.xlm_wagered; break;
                                    case "usdt":
                                        balance = bsstatsbase.usdt_balance;
                                        profit = bsstatsbase.usdt_profit;
                                        wagered = bsstatsbase.usdt_wagered; break;
                                    case "trx":
                                        balance = bsstatsbase.trx_balance;
                                        profit = bsstatsbase.trx_profit;
                                        wagered = bsstatsbase.trx_wagered; break;
                                    case "eos":
                                        balance = bsstatsbase.eos_balance;
                                        profit = bsstatsbase.eos_profit;
                                        wagered = bsstatsbase.eos_wagered; break;
                                }
                                bets = int.Parse(bsstatsbase.bets, System.Globalization.NumberFormatInfo.InvariantInfo);
                                wins = int.Parse(bsstatsbase.wins, System.Globalization.NumberFormatInfo.InvariantInfo);
                                losses = int.Parse(bsstatsbase.losses, System.Globalization.NumberFormatInfo.InvariantInfo);

                                Parent.updateBalance(balance);
                                Parent.updateBets(bets);
                                Parent.updateLosses(losses);
                                Parent.updateProfit(profit);
                                Parent.updateWagered(wagered);
                                Parent.updateWins(wins);
                            }
                            else
                            {
                                if (bsstatsbase.error != null)
                                {

                                    Parent.updateStatus(bsstatsbase.error);

                                }
                            }
                }
                catch { }
            }
        }

        void PlaceBetThread(object BetObj)
        {
            try
            {

                PlaceBetObj tmpob = BetObj as PlaceBetObj;
                LastBetAmount = (double)tmpob.Amount;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                /*access_token
type:dice
amount:0.00000001
condition:< or >
game:49.5
currency:btc*/
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("amount", tmpob.Amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("over", tmpob.High.ToString().ToLower()));
                pairs.Add(new KeyValuePair<string, string>("target", !tmpob.High ? tmpob.Chance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) : (maxRoll - tmpob.Chance).ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("currency", Currency));
                pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpmsg = Client.PostAsync("api/bet-dice", Content).Result;
                string sEmitResponse = tmpmsg.Content.ReadAsStringAsync().Result;
                bsBet bsbase = null;
                try
                {
                    bsbase = json.JsonDeserialize<bsBet>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                }
                catch (Exception e)
                {

                }

                if (bsbase != null)
                    if (bsbase != null)
                        if (bsbase.success)
                        {
                            balance = decimal.Parse(bsbase.new_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                            lastupdate = DateTime.Now;
                            Bet tmp = bsbase.ToBet();
                            tmp.high = tmpob.High;
                            tmp.Chance = tmpob.Chance;
                            tmp.Guid = tmpob.Guid;
                            profit += (decimal)tmp.Profit;
                            wagered += (decimal)tmp.Amount;
                            tmp.date = DateTime.Now;
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
                            LastBetAmount = (double)tmpob.Amount;
                            LastBet = DateTime.Now;
                            FinishedBet(tmp);
                            return;
                        }
                        else
                        {
                            if (bsbase.error != null)
                            {
                                if (bsbase.error.Contains("Bet in progress, please wait few seconds and retry."))
                                {
                                    Parent.updateStatus("Bet in progress. You need to log in with your browser and place a bet manually to fix this.");
                                }
                                else
                                {
                                    Parent.updateStatus(bsbase.error);
                                }
                            }
                        }
                //

            }
            catch (AggregateException e)
            {
                Parent.updateStatus("An Unknown error has ocurred.");
            }
            catch (Exception e)
            {
                Parent.updateStatus("An Unknown error has ocurred.");
            }
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            System.Threading.Thread tBetThread = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            tBetThread.Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        DBRandom R = new DBRandom();
        DateTime LastReset = new DateTime();
        public override void ResetSeed()
        {
            //Just wanted to test if this works. It doesn't. Will work with the bitsler team to
            //expand functionality in the future.
            Thread.Sleep(100);
            try
            {
                if ((DateTime.Now - LastReset).TotalMinutes >= 3)
                {
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    pairs.Add(new KeyValuePair<string, string>("seed_client", R.Next(0, int.MaxValue).ToString()));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("api/change-seeds", Content).Result.Content.ReadAsStringAsync().Result;
                    bsResetSeed bsbase = json.JsonDeserialize<bsResetSeed>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    //sqlite_helper.InsertSeed(bsbase.last_seeds_revealed.seed_server_hashed, bsbase.last_seeds_revealed.seed_server_revealed);
                    sqlite_helper.InsertSeed(bsbase.previous_hash, bsbase.previous_seed);
                }
                else
                {
                    Parent.updateStatus("Too soon to update seed.");
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("429"))
                    {
                        Thread.Sleep(2000);
                        ResetSeed();
                    }
                }
            }
            catch
            {
                Parent.updateStatus("Too soon to update seed.");
            }
            Thread.Sleep(51);
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
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitsler.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "DiceBot");

            try
            {
                string actual2fa = "";
                if (twofa.Contains("&"))
                {
                    string[] pars = twofa.Split('&');
                    actual2fa = pars[1];
                    twofa = pars[0];
                }
                HttpResponseMessage resp = Client.GetAsync("https://www.bitsler.com").Result;
                string s1 = "";


                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //pairs.Add(new KeyValuePair<string, string>("api_key", "0b2edbfe44e98df79665e52896c22987445683e78"));
                if (!string.IsNullOrWhiteSpace(actual2fa))
                {
                    pairs.Add(new KeyValuePair<string, string>("two_factor", actual2fa));
                }
                pairs.Add(new KeyValuePair<string, string>("api_key", twofa));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage tmpresp = Client.PostAsync("api/login", Content).Result;

                byte[] bytes = tmpresp.Content.ReadAsByteArrayAsync().Result;
                string sEmitResponse = tmpresp.Content.ReadAsStringAsync().Result;

                //getuserstats 
                bsLogin bsbase = json.JsonDeserialize<bsLogin>(sEmitResponse.Replace("\"return\":", "\"_return\":"));

                if (bsbase != null)
                    if (bsbase != null)
                        if (bsbase.success == "true")
                        {
                            accesstoken = bsbase.access_token;
                            IsBitsler = true;
                            lastupdate = DateTime.Now;


                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("api/getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                            bsStats bsstatsbase = json.JsonDeserialize<bsStats>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                            if (bsstatsbase != null)
                                if (bsstatsbase != null)
                                    if (bsstatsbase.success == "true")
                                    {
                                        switch (Currency.ToLower())
                                        {
                                            case "btc":
                                                balance = bsstatsbase.btc_balance;
                                                profit = bsstatsbase.btc_profit;
                                                wagered = bsstatsbase.btc_wagered; break;
                                            case "ltc":
                                                balance = bsstatsbase.ltc_balance;
                                                profit = bsstatsbase.ltc_profit;
                                                wagered = bsstatsbase.ltc_wagered; break;
                                            case "doge":
                                                balance = bsstatsbase.doge_balance;
                                                profit = bsstatsbase.doge_profit;
                                                wagered = bsstatsbase.doge_wagered; break;
                                            case "eth":
                                                balance = bsstatsbase.eth_balance;
                                                profit = bsstatsbase.eth_profit;
                                                wagered = bsstatsbase.eth_wagered; break;
                                            case "burst":
                                                balance = bsstatsbase.burst_balance;
                                                profit = bsstatsbase.burst_profit;
                                                wagered = bsstatsbase.burst_wagered; break;
                                            case "dash":
                                                balance = bsstatsbase.dash_balance;
                                                profit = bsstatsbase.dash_profit;
                                                wagered = bsstatsbase.dash_wagered; break;
                                            case "zec":
                                                balance = bsstatsbase.zec_balance;
                                                profit = bsstatsbase.zec_profit;
                                                wagered = bsstatsbase.zec_wagered; break;
                                            case "bch":
                                                balance = bsstatsbase.bch_balance;
                                                profit = bsstatsbase.bch_profit;
                                                wagered = bsstatsbase.bch_wagered; break;
                                            case "xmr":
                                                balance = bsstatsbase.xmr_balance;
                                                profit = bsstatsbase.xmr_profit;
                                                wagered = bsstatsbase.xmr_wagered; break;
                                            case "etc":
                                                balance = bsstatsbase.etc_balance;
                                                profit = bsstatsbase.etc_profit;
                                                wagered = bsstatsbase.etc_wagered; break;
                                            case "neo":
                                                balance = bsstatsbase.neo_balance;
                                                profit = bsstatsbase.neo_profit;
                                                wagered = bsstatsbase.neo_wagered; break;
                                            case "strat":
                                                balance = bsstatsbase.strat_balance;
                                                profit = bsstatsbase.strat_profit;
                                                wagered = bsstatsbase.strat_wagered; break;
                                            case "kmd":
                                                balance = bsstatsbase.kmd_balance;
                                                profit = bsstatsbase.kmd_profit;
                                                wagered = bsstatsbase.kmd_wagered; break;
                                            case "xrp":
                                                balance = bsstatsbase.xrp_balance;
                                                profit = bsstatsbase.xrp_profit;
                                                wagered = bsstatsbase.xrp_wagered; break;
                                            case "btg":
                                                balance = bsstatsbase.btg_balance;
                                                profit = bsstatsbase.btg_profit;
                                                wagered = bsstatsbase.btg_wagered; break;
                                            case "qtum":
                                                balance = bsstatsbase.qtum_balance;
                                                profit = bsstatsbase.qtum_profit;
                                                wagered = bsstatsbase.qtum_wagered; break;
                                            case "lsk":
                                                balance = bsstatsbase.lsk_balance;
                                                profit = bsstatsbase.lsk_profit;
                                                wagered = bsstatsbase.lsk_wagered; break;
                                            case "dgb":
                                                balance = bsstatsbase.dgb_balance;
                                                profit = bsstatsbase.dgb_profit;
                                                wagered = bsstatsbase.dgb_wagered; break;
                                            case "waves":
                                                balance = bsstatsbase.waves_balance;
                                                profit = bsstatsbase.waves_profit;
                                                wagered = bsstatsbase.waves_wagered; break;
                                            case "btslr":
                                                balance = bsstatsbase.btslr_balance;
                                                profit = bsstatsbase.btslr_profit;
                                                wagered = bsstatsbase.btslr_wagered; break;
                                            case "bsv":
                                                balance = bsstatsbase.bsv_balance;
                                                profit = bsstatsbase.bsv_profit;
                                                wagered = bsstatsbase.bsv_wagered; break;
                                            case "xlm":
                                                balance = bsstatsbase.xlm_balance;
                                                profit = bsstatsbase.xlm_profit;
                                                wagered = bsstatsbase.xlm_wagered; break;
                                            case "usdt":
                                                balance = bsstatsbase.usdt_balance;
                                                profit = bsstatsbase.usdt_profit;
                                                wagered = bsstatsbase.usdt_wagered; break;
                                            case "trx":
                                                balance = bsstatsbase.trx_balance;
                                                profit = bsstatsbase.trx_profit;
                                                wagered = bsstatsbase.trx_wagered; break;
                                            case "eos":
                                                balance = bsstatsbase.eos_balance;
                                                profit = bsstatsbase.eos_profit;
                                                wagered = bsstatsbase.eos_wagered; break;
                                        }
                                        bets = int.Parse(bsstatsbase.bets == null ? "0" : bsstatsbase.bets, System.Globalization.NumberFormatInfo.InvariantInfo);
                                        wins = int.Parse(bsstatsbase.wins == null ? "0" : bsstatsbase.wins, System.Globalization.NumberFormatInfo.InvariantInfo);
                                        losses = int.Parse(bsstatsbase.losses == null ? "0" : bsstatsbase.losses, System.Globalization.NumberFormatInfo.InvariantInfo);

                                        Parent.updateBalance(balance);
                                        Parent.updateBets(bets);
                                        Parent.updateLosses(losses);
                                        Parent.updateProfit(profit);
                                        Parent.updateWagered(wagered);
                                        Parent.updateWins(wins);
                                        this.username = Username;
                                    }
                                    else
                                    {
                                        if (bsstatsbase.error != null)
                                        {

                                            Parent.updateStatus(bsstatsbase.error);

                                        }
                                    }


                            IsBitsler = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            finishedlogin(true);
                            return;
                        }
                        else
                        {
                            if (bsbase.error != null)
                                Parent.updateStatus(bsbase.error);
                        }

            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
            }
            finishedlogin(false);
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        DateTime LastBet = DateTime.Now;
        double LastBetAmount = 0;

        public override bool ReadyToBet()
        {
            //return true;

            int type_delay = 0;

            if (Currency.ToLower() == "btc")
            {
                if (LastBetAmount <= 0.00000005 || (double)amount <= 0.00000005)
                    type_delay = 1;
                else
                    type_delay = 2;
            }
            else if (Currency.ToLower() == "eth")
            {
                if (LastBetAmount <= 0.00000250 || (double)amount <= 0.00000250)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "ltc")
            {
                if (LastBetAmount <= 0.00001000 || (double)amount <= 0.00001000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "doge")
            {
                if (LastBetAmount <= 5.00000000 || (double)amount <= 5.00000000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "burst")
            {
                if (LastBetAmount <= 5.00000000 || (double)amount <= 5.00000000)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "bch")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "dash")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "zec")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "xmr")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "etc")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "neo")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "strat")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "kmd")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            else if (Currency.ToLower() == "xrp")
            {
                if (LastBetAmount <= 0.00000025 || (double)amount <= 0.00000025)
                    type_delay = 1;
                else
                    type_delay = 2;

            }
            int delay = 0;
            if (type_delay == 1)
                delay = 300;
            else if (type_delay == 2)
                delay = 200;
            else
                delay = 200;

            return (DateTime.Now - LastBet).TotalMilliseconds > delay;
        }

        public override void Disconnect()
        {
            IsBitsler = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            SHA512 betgenerator = SHA512.Create();

            int charstouse = 5;

            List<byte> buffer = new List<byte>();
            string msg = server + "," + client + "," + nonce.ToString();
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
                    return (lucky % 10000 / 100m);
            }
            return 0;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {

            return sGetLucky(server, client, nonce);
        }
    }

    public class bsLogin
    {
        public string success { get; set; }
        public string error { get; set; }
        public string access_token { get; set; }
    }
    public class bsloginbase
    {
        public bsLogin _return { get; set; }
    }
    //"{\"return\":{\"success\":\"true\",\"balance\":1.0e-5,\"wagered\":0,\"profit\":0,\"bets\":\"0\",\"wins\":\"0\",\"losses\":\"0\"}}"
    //{"return":{"success":"true","bets":0,"wins":0,"losses":0,"btc_profit":"0.00000000","btc_wagered":"0.00000000","btc_balance":"0.00000000","eth_profit":"0.00000000","eth_wagered":"0.00000000","eth_balance":"0.00000000","ltc_profit":"0.00000000","ltc_wagered":"0.00000000","ltc_balance":"0.00000000","bch_profit":"0.00000000","bch_wagered":"0.00000000","bch_balance":"0.00000000","doge_profit":"0.00000000","doge_wagered":"0.00000000","doge_balance":"0.00000000","dash_profit":"0.00000000","dash_wagered":"0.00000000","dash_balance":"0.00000000","zec_profit":"0.00000000","zec_wagered":"0.00000000","zec_balance":"0.00000000","burst_profit":"0.00000000","burst_wagered":"0.00000000","burst_balance":"0.00000000"}}
    public class bsStats
    {
        public string success { get; set; }
        public string error { get; set; }
        public decimal btc_balance { get; set; }
        public decimal btc_wagered { get; set; }
        public decimal btc_profit { get; set; }
        public string bets { get; set; }
        public decimal ltc_balance { get; set; }
        public decimal ltc_wagered { get; set; }
        public decimal ltc_profit { get; set; }

        public decimal doge_balance { get; set; }
        public decimal doge_wagered { get; set; }
        public decimal doge_profit { get; set; }
        public decimal eth_balance { get; set; }
        public decimal eth_wagered { get; set; }
        public decimal eth_profit { get; set; }
        public decimal burst_balance { get; set; }
        public decimal burst_wagered { get; set; }
        public decimal zec_profit { get; set; }
        public decimal zec_balance { get; set; }
        public decimal zec_wagered { get; set; }
        public decimal bch_profit { get; set; }
        public decimal bch_balance { get; set; }
        public decimal bch_wagered { get; set; }
        public decimal dash_profit { get; set; }
        public decimal dash_balance { get; set; }
        public decimal dash_wagered { get; set; }
        public decimal burst_profit { get; set; }
        public decimal etc_balance { get; set; }
        public decimal etc_wagered { get; set; }
        public decimal etc_profit { get; set; }

        public decimal xmr_balance { get; set; }
        public decimal xmr_wagered { get; set; }
        public decimal xmr_profit { get; set; }

        public decimal neo_balance { get; set; }
        public decimal neo_wagered { get; set; }
        public decimal neo_profit { get; set; }

        public decimal strat_balance { get; set; }
        public decimal strat_wagered { get; set; }
        public decimal strat_profit { get; set; }

        public decimal kmd_balance { get; set; }
        public decimal kmd_wagered { get; set; }
        public decimal kmd_profit { get; set; }

        public decimal xrp_balance { get; set; }
        public decimal xrp_wagered { get; set; }
        public decimal xrp_profit { get; set; }

        public decimal btg_balance { get; set; }
        public decimal btg_wagered { get; set; }
        public decimal btg_profit { get; set; }

        public decimal lsk_balance { get; set; }
        public decimal lsk_wagered { get; set; }
        public decimal lsk_profit { get; set; }

        public decimal dgb_balance { get; set; }
        public decimal dgb_wagered { get; set; }
        public decimal dgb_profit { get; set; }

        public decimal qtum_balance { get; set; }
        public decimal qtum_wagered { get; set; }
        public decimal qtum_profit { get; set; }

        public decimal waves_balance { get; set; }
        public decimal waves_wagered { get; set; }
        public decimal waves_profit { get; set; }
        public decimal btslr_balance { get; set; }
        public decimal btslr_wagered { get; set; }
        public decimal btslr_profit { get; set; }
        public decimal bsv_balance { get; set; }
        public decimal bsv_wagered { get; set; }
        public decimal bsv_profit { get; set; }
        public decimal xlm_balance { get; set; }
        public decimal xlm_wagered { get; set; }
        public decimal xlm_profit { get; set; }
        public decimal usdt_balance { get; set; }
        public decimal usdt_wagered { get; set; }
        public decimal usdt_profit { get; set; }

        public string wins { get; set; }
        public string losses { get; set; }
        public decimal trx_balance { get; internal set; }
        public decimal trx_profit { get; internal set; }
        public decimal trx_wagered { get; internal set; }
        public decimal eos_balance { get; internal set; }
        public decimal eos_profit { get; internal set; }
        public decimal eos_wagered { get; internal set; }
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
        public bool success { get; set; }
        public string username { get; set; }
        public string id { get; set; }
        public string currency { get; set; }
        public long timestamp { get; set; }
        public string amount { get; set; }
        public decimal result { get; set; }
        public bool over { get; set; }
        public decimal target { get; set; }
        public decimal payout { get; set; }
        public decimal chance { get; set; }
        public string profit { get; set; }
        public string new_balance { get; set; }
        public string server_seed { get; set; }
        public string client_seed { get; set; }
        public long nonce { get; set; }
        public List<object> notifications { get; set; }
        public string error { get; set; }
        public Bet ToBet()
        {
            Bet tmp = new Bet
            {
                Amount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                date = DateTime.Now,
                Id = id,
                Profit = decimal.Parse(profit, System.Globalization.NumberFormatInfo.InvariantInfo),
                Roll = (decimal)result,
                high = over,
                Chance = chance,
                nonce = nonce,
                serverhash = server_seed,
                clientseed = client_seed
            };
            return tmp;
        }
    }
    public class bsResetSeedBase
    {
        public bsResetSeed _return { get; set; }
    }
    public class bsResetSeed
    {
        public string previous_hash { get; set; }
        public string previous_seed { get; set; }
        public string previous_client { get; set; }
        public string previous_total { get; set; }
        public string current_client { get; set; }
        public string current_hash { get; set; }
        public string next_hash { get; set; }
        public bool success { get; set; }
    }

}
