using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiceBot.Common;
using DiceBot.Forms;

namespace DiceBot.Sites
{
    public class Bitvest : DiceSite
    {
        public static string[] cCurrencies = {"bitcoins", "tokens", "litecoins", "ethers", "dogecoins", "bcash"};
        private readonly Random R = new Random();
        private string accesstoken = "";
        private HttpClient Client; // = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        private HttpClientHandler ClientHandlr;
        public bool ispd;
        private DateTime Lastbet = DateTime.Now;

        private DateTime lastchat = DateTime.UtcNow;
        private string lasthash = "";
        private DateTime LastSeedReset = new DateTime();
        private DateTime lastupdate;
        private double[] Limits = new double[0];
        private string pw = "";

        private int retrycount;
        private string secret = "";
        private string seed = "";
        private long uid = 0;
        private string username = "";

        private bitvestCurWeight Weights;

        public Bitvest(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://bitvest.io/results?game=dice&query=";

            Currencies = cCurrencies;
            Currency = "bitcoins";
            this.Parent = Parent;
            Name = "Bitvest";
            Tip = true;
            TipUsingName = true;

            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://bitvest.io?r=46534";
            NonceBased = true;
        }

        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }

        private string RandomSeed()
        {
            var s = "";
            var chars = "0123456789abcdef";

            while (s.Length < 60)
            {
                s += chars[R.Next(0, chars.Length)];
            }

            return s;
        }

        private void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 10 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        ForceUpdateStats = false;
                        var pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                        pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                        pairs.Add(new KeyValuePair<string, string>("k", "0"));
                        pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                        pairs.Add(new KeyValuePair<string, string>("u", "0"));
                        pairs.Add(new KeyValuePair<string, string>("self_only", "1"));

                        var resp1 = Client.GetAsync("").Result;
                        var s1 = "";

                        if (resp1.IsSuccessStatusCode)
                        {
                            s1 = resp1.Content.ReadAsStringAsync().Result;

                            //Parent.DumpLog("BE login 2.1", 7);
                        }
                        else
                        {
                            //Parent.DumpLog("BE login 2.2", 7);
                            if (resp1.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                s1 = resp1.Content.ReadAsStringAsync().Result;

                                //cflevel = 0;
                                Task.Factory.StartNew(() =>
                                                      {
                                                          MessageBox
                                                              .Show("Bitvest.io has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                                      });

                                if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "bitvest.io"))
                                {
                                    finishedlogin(false);

                                    return;
                                }
                            }

                            //Parent.DumpLog("BE login 2.3", 7);
                        }

                        var Content = new FormUrlEncodedContent(pairs);
                        var sEmitResponse = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;
                        sEmitResponse = sEmitResponse.Replace("r-", "r_").Replace("n-", "n_");

                        var tmpbase = JsonUtils.JsonDeserialize<BivestGetBalanceRoot>(sEmitResponse);

                        if (tmpbase != null)
                            if (tmpbase.data != null)
                            {
                                switch (Currency.ToLower())
                                {
                                    case "bitcoins":
                                        balance = tmpbase.data.balance;

                                        break;
                                    case "ethers":
                                        balance = tmpbase.data.ether_balance;

                                        break;
                                    case "litecoins":
                                        balance = tmpbase.data.litecoin_balance;

                                        break;
                                    case "dogecoins":
                                        balance = tmpbase.data.balance_dogecoin;

                                        break;
                                    case "bcash":
                                        balance = tmpbase.data.balance_bcash;

                                        break;
                                    default:
                                        balance = tmpbase.data.token_balance;

                                        break;
                                }

                                /*if (Currency.ToLower() == "bitcoins")
                                {
                                    balance = decimal.Parse(tmpbase.data.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }
                                else
                                {
                                    balance = decimal.Parse(tmpbase.data.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }if (Currency.ToLower() == "bitcoins")
                    {
                        balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "ethereum")
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else if (Currency.ToLower() == "litecoin")
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        balance = decimal.Parse(tmplogin.token_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }*/
                                Parent.updateBalance(balance);
                            }
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }

                Thread.Sleep(1000);
            }
        }

        public override bool Register(string Username, string Password)
        {
            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            ;
            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://bitvest.io")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            try
            {
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("r", "46534"));

                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("create.php", Content).Result.Content.ReadAsStringAsync().Result;
                var tmpbase = JsonUtils.JsonDeserialize<bitvestLoginBase>(sEmitResponse.Replace("-", "_"));
                accesstoken = tmpbase.data.session_token;
                secret = tmpbase.account.secret;

                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                pairs.Add(new KeyValuePair<string, string>("k", "0"));
                pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                pairs.Add(new KeyValuePair<string, string>("u", "0"));

                //pairs.Add(new KeyValuePair<string, string>("self_only", "1"));

                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;

                tmpbase = JsonUtils.JsonDeserialize<bitvestLoginBase>(sEmitResponse.Replace("-", "_"));
                accesstoken = tmpbase.data.session_token;

                //tmplogin = tmpblogin.data;

                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("act", "convert"));
                pairs.Add(new KeyValuePair<string, string>("c", "9999999"));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("password_conf", Password));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                tmpbase = JsonUtils.JsonDeserialize<bitvestLoginBase>(sEmitResponse);

                if (!tmpbase.success)
                {
                    Parent.updateStatus(tmpbase.msg);
                    finishedlogin(false);

                    return false;
                }

                accesstoken = tmpbase.data.session_token;
                secret = tmpbase.account.secret;

                if (accesstoken == "") return false;

                var tmpblogin = tmpbase;
                var tmplogin = tmpblogin.data;

                if (Currency.ToLower() == "bitcoins")
                    balance = decimal.Parse(tmplogin.balance, NumberFormatInfo.InvariantInfo);
                else if (Currency.ToLower() == "ethers")
                    balance = decimal.Parse(tmplogin.balance_ether, NumberFormatInfo.InvariantInfo);
                else if (Currency.ToLower() == "litecoins")
                    balance = decimal.Parse(tmplogin.balance_litecoin, NumberFormatInfo.InvariantInfo);
                else
                    balance = decimal.Parse(tmplogin.token_balance, NumberFormatInfo.InvariantInfo);

                accesstoken = tmplogin.session_token;
                secret = tmpblogin.account.secret;

                wagered = Currency.ToLower() == "bitcoins" ? tmplogin.total_bet :
                          Currency.ToLower() == "ethers" ? tmplogin.ether_total_bet :
                          Currency.ToLower() == "litecoins" ? tmplogin.litecoin_total_bet :
                          tmplogin.token_total_bet;

                profit = Currency.ToLower() == "bitcoins" ? tmplogin.total_profit :
                         Currency.ToLower() == "ethers" ? tmplogin.ether_total_profit :
                         Currency.ToLower() == "litecoins" ? tmplogin.litecoin_total_profit :
                         tmplogin.token_total_bet;

                bets = tmplogin.bets;
                wins = 0;
                losses = 0;
                Parent.updateBalance(balance);
                Parent.updateWagered(wagered);
                Parent.updateProfit(profit);
                Parent.updateBets(bets);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
                Parent.updateDeposit(tmpblogin.account.address);
                lastupdate = DateTime.Now;
                ispd = true;
                pw = Password;
                new Thread(GetBalanceThread).Start();
                lasthash = tmpblogin.server_hash;
                Tip = false;
                finishedlogin(true);

                return true;
            }
            catch (Exception e)
            {
                Parent.updateStatus(e.Message);
                finishedlogin(false);

                return false;
            }

            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {
            //accept-encoding:gzip, deflate,
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;

            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://bitvest.io/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("UserAgent", "DiceBot");

            try
            {
                var resp = ""; // Client.GetStringAsync("").Result;
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("type", "secret"));
                var Content = new FormUrlEncodedContent(pairs);
                resp = Client.PostAsync("https://bitvest.io/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                var tmpblogin = JsonUtils.JsonDeserialize<bitvestLoginBase>(resp.Replace("-", "_"));
                var tmplogin = tmpblogin.data;
                secret = tmpblogin.account.secret;
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("g[]", "999999999"));
                pairs.Add(new KeyValuePair<string, string>("k", "0"));
                pairs.Add(new KeyValuePair<string, string>("m", "99999899"));
                pairs.Add(new KeyValuePair<string, string>("u", "0"));

                //pairs.Add(new KeyValuePair<string, string>("self_only", "1"));
                Content = new FormUrlEncodedContent(pairs);
                resp = Client.PostAsync("https://bitvest.io/update.php", Content).Result.Content.ReadAsStringAsync().Result;

                var tmpresp = resp.Replace("-", "_");
                tmpblogin = JsonUtils.JsonDeserialize<bitvestLoginBase>(tmpresp);
                tmplogin = tmpblogin.data;

                if (tmplogin.session_token != null)
                {
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("username", Username));
                    pairs.Add(new KeyValuePair<string, string>("password", Password));
                    pairs.Add(new KeyValuePair<string, string>("tfa", otp));
                    pairs.Add(new KeyValuePair<string, string>("token", tmplogin.session_token));

                    //pairs.Add(new KeyValuePair<string, string>("c", "secret"));
                    pairs.Add(new KeyValuePair<string, string>("secret", secret));
                    Content = new FormUrlEncodedContent(pairs);
                    resp = Client.PostAsync("https://bitvest.io/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                    tmpresp = resp.Replace("-", "_");
                    tmpblogin = JsonUtils.JsonDeserialize<bitvestLoginBase>(tmpresp);
                    Weights = tmpblogin.currency_weight;
                    Limits = tmpblogin.rate_limits;

                    tmplogin = tmpblogin.data;

                    if (Currency.ToLower() == "bitcoins")
                        balance = decimal.Parse(tmplogin.balance, NumberFormatInfo.InvariantInfo);
                    else if (Currency.ToLower() == "ethers")
                        balance = decimal.Parse(tmplogin.balance_ether, NumberFormatInfo.InvariantInfo);
                    else if (Currency.ToLower() == "litecoins")
                        balance = decimal.Parse(tmplogin.balance_litecoin, NumberFormatInfo.InvariantInfo);
                    else if (Currency.ToLower() == "dogecoins")
                        balance = decimal.Parse(tmplogin.balance_dogecoin, NumberFormatInfo.InvariantInfo);
                    else if (Currency.ToLower() == "bcash")
                        balance = decimal.Parse(tmplogin.balance_bcash, NumberFormatInfo.InvariantInfo);
                    else
                        balance = decimal.Parse(tmplogin.token_balance, NumberFormatInfo.InvariantInfo);

                    accesstoken = tmplogin.session_token;
                    secret = tmpblogin.account.secret;
                    wagered = decimal.Parse(tmplogin.self_total_bet_dice, NumberFormatInfo.InvariantInfo);
                    profit = decimal.Parse(tmplogin.self_total_won_dice, NumberFormatInfo.InvariantInfo);
                    bets = int.Parse(tmplogin.self_total_bets_dice.Replace(",", ""), NumberFormatInfo.InvariantInfo);
                    wins = 0;
                    losses = 0;
                    Parent.updateBalance(balance);
                    Parent.updateWagered(wagered);
                    Parent.updateProfit(profit);
                    Parent.updateBets(bets);
                    Parent.updateWins(wins);
                    Parent.updateLosses(losses);

                    //Parent.updateDeposit(tmpblogin.account.address);
                    lastupdate = DateTime.Now;
                    seed = tmpblogin.last_user_seed;
                    ispd = true;
                    pw = Password;
                    new Thread(GetBalanceThread).Start();
                    lasthash = tmpblogin.server_hash;
                    Tip = tmpblogin.tip.enabled;
                    finishedlogin(true);

                    return;
                }

                finishedlogin(false);

                return;
            }
            catch (AggregateException e)
            {
                finishedlogin(false);

                return;
            }
            catch (Exception e)
            {
                finishedlogin(false);

                return;
            }

            finishedlogin(false);
        }

        private void placebetthread(object bet)
        {
            if (string.IsNullOrWhiteSpace(seed))
                seed = RandomSeed();

            try
            {
                var tmp5 = bet as PlaceBetObj;
                var amount = tmp5.Amount;
                var chance = tmp5.Chance;
                var High = tmp5.High;

                var tmpchance = High ? maxRoll - chance + 0.0001m : chance - 0.0001m;
                var pairs = new List<KeyValuePair<string, string>>();

                //string seed = RandomSeed();
                pairs.Add(new KeyValuePair<string, string>("bet", amount.ToString(NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("target", tmpchance.ToString("0.0000", NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("side", High ? "high" : "low"));
                pairs.Add(new KeyValuePair<string, string>("act", "play_dice"));
                pairs.Add(new KeyValuePair<string, string>("currency", Currency));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("user_seed", seed));
                pairs.Add(new KeyValuePair<string, string>("v", "101"));

                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;
                Lastbet = DateTime.Now;

                try
                {
                    var x = sEmitResponse.Replace("f-", "f_").Replace("n-", "n_").Replace("ce-", "ce_").Replace("r-", "r_");
                    var tmp = JsonUtils.JsonDeserialize<bitvestbet>(x);

                    if (tmp.success)
                    {
                        lastupdate = DateTime.Now;

                        var resbet = new Bet
                        {
                            Amount = tmp5.Amount,
                            date = DateTime.Now,
                            Chance = tmp5.Chance,
                            high = tmp5.High,
                            clientseed = seed,
                            serverhash = tmp.server_hash,
                            serverseed = tmp.server_seed,
                            Roll = tmp.game_result.roll,
                            Profit = tmp.game_result.win == 0 ? -tmp5.Amount : tmp.game_result.win - tmp5.Amount,
                            nonce = long.Parse(tmp.player_seed.Substring(tmp.player_seed.IndexOf("|") + 1)),
                            Id = tmp.game_id.ToString(),
                            Currency = Currency
                        };

                        resbet.Guid = tmp5.Guid;
                        bets++;

                        //lasthash = tmp.server_hash;
                        var Win = High ? tmp.game_result.roll > maxRoll - chance : tmp.game_result.roll < chance;

                        if (Win)
                            wins++;
                        else losses++;

                        wagered += amount;
                        profit += resbet.Profit;

                        balance = decimal.Parse(
                                                Currency.ToLower() == "bitcoins" ? tmp.data.balance
                                                : Currency.ToLower() == "ethers" ? tmp.data.balance_ether
                                                : Currency.ToLower() == "litecoins" ? tmp.data.balance_litecoin
                                                : Currency.ToLower() == "dogecoins" ? tmp.data.balance_dogecoin
                                                : Currency.ToLower() == "bcash" ? tmp.data.balance_bcash : tmp.data.token_balance,
                                                NumberFormatInfo.InvariantInfo);

                        /*tmp.bet.client = tmp.user.client;
                        tmp.bet.serverhash = tmp.user.server;
                        lastupdate = DateTime.Now;
                        balance = tmp.user.balance / 100000000.0m; //i assume
                        bets = tmp.user.bets;
                        wins = tmp.user.wins;
                        losses = tmp.user.losses;
                        wagered = (decimal)(tmp.user.wagered / 100000000m);
                        profit = (decimal)(tmp.user.profit / 100000000m);
                        */
                        FinishedBet(resbet);
                        retrycount = 0;
                    }
                    else
                    {
                        Parent.updateStatus(tmp.msg);

                        if (tmp.msg.ToLower() == "bet rate limit exceeded")
                        {
                            Parent.updateStatus(tmp.msg + ". Retrying in a second;");
                            Thread.Sleep(1000);
                            placebetthread(bet);
                        }
                    }
                }
                catch (Exception e)
                {
                    Parent.updateStatus("An unknown error has occurred");
                    Parent.DumpLog(e.ToString(), -1);
                }
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
            catch (Exception e2)
            {
                Parent.DumpLog(e2.ToString(), -1);
            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.High = High;
            new Thread(placebetthread).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        public override void ResetSeed()
        {
            var request = $"token={accesstoken}&secret=0&act=new_server_seed";

            //{"success":true,"server_seed":"b10982af50874add19b6dc54c8e76ccb2e1f5b86b6047730ddfb4cdee2fd8fbf","server_hash":"d6de3617918df1d6c5d34f36833a02733e595c6a58a7088fa7a7e9aebc2fbe1e"}
            seed = RandomSeed();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            decimal weight = 1;

            if (Currency.ToLower() == "bitcoins")
                switch (Currency.ToLower())
                {
                    case "bitcoins":
                        weight = decimal.Parse(Weights.BTC, NumberFormatInfo.InvariantInfo);

                        break;
                    case "tokens":
                        weight = decimal.Parse(Weights.TOK, NumberFormatInfo.InvariantInfo);

                        break;
                    case "litecoins":
                        weight = decimal.Parse(Weights.LTC, NumberFormatInfo.InvariantInfo);

                        break;
                    case "ethers":
                        weight = decimal.Parse(Weights.ETH, NumberFormatInfo.InvariantInfo);

                        break;
                    case "dogecoins":
                        weight = decimal.Parse(Weights.DOGE, NumberFormatInfo.InvariantInfo);

                        break;
                    case "bcash":
                        weight = decimal.Parse(Weights.BCH, NumberFormatInfo.InvariantInfo);

                        break;

                    default:
                        weight = decimal.Parse(Weights.BTC, NumberFormatInfo.InvariantInfo);

                        break;
                }

            for (var i = Limits.Length - 1; i >= 0; i--)
            {
                if (i == Limits.Length - 1 && amount * weight >= (decimal) Limits[i] * 0.00000001m)
                    return true;

                if (amount * weight >= (decimal) Limits[i] * 0.00000001m) return (DateTime.Now - Lastbet).TotalSeconds > 1.0 / (i + 1.0);
            }

            return true;
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                Thread.Sleep(500);
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("quantity", Amount.ToString("", NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("address", Address));
                pairs.Add(new KeyValuePair<string, string>("act", "withdraw"));
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("password", pw));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                pairs.Add(new KeyValuePair<string, string>("tfa", ""));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            var betgenerator = new HMACSHA512();

            var charstouse = 5;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            var buffer = new List<byte>();
            var msg = /*nonce.ToString() + ":" + */client + "|" + nonce;

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            betgenerator.Key = buffer.ToArray();

            var hash = betgenerator.ComputeHash(serverb.ToArray());

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
                    return lucky / 10000m;
            }

            return 0;
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        public string getDepositAddress()
        {
            try
            {
                var sEmitResponse = Client.GetStringAsync("deposit?api_key=" + accesstoken).Result;
                var tmpa = JsonUtils.JsonDeserialize<pdDeposit>(sEmitResponse);

                return tmpa.address;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    var sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);

                    if (e.Message.Contains("429"))
                    {
                        Thread.Sleep(1500);

                        return getDepositAddress();
                    }
                }

                return "";
            }
        }

        public override void Disconnect()
        {
            ispd = false;

            if (accesstoken != "")
                try
                {
                    var sEmitResponse = Client.GetStringAsync("logout?api_key=" + accesstoken).Result;
                    accesstoken = "";
                }
                catch
                {
                }
        }

        public override void Donate(decimal Amount)
        {
            if (Currency.ToLower() != "tokens")
                SendTip("seuntjie", Amount);
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                var pairs = new List<KeyValuePair<string, string>>();

                pairs.Add(new KeyValuePair<string, string>("currency", Currency == "bitcoins" ? "btc" :
                                                                       Currency == "litecoins" ? "ltc" :
                                                                       Currency == "ethers" ? "eth" :
                                                                       Currency == "tokens" ? "tok" :
                                                                       Currency == "dogecoins" ? "doge" :
                                                                       Currency == "bcash" ? "bch" : "tok"));

                pairs.Add(new KeyValuePair<string, string>("username", User));
                pairs.Add(new KeyValuePair<string, string>("quantity", amount.ToString("0.00000000", NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("act", "send_tip"));
                pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("c", "99999999"));
                pairs.Add(new KeyValuePair<string, string>("secret", secret));
                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("action.php", Content).Result.Content.ReadAsStringAsync().Result;

                if (sEmitResponse.Contains("true")) return true;

                Parent.DumpLog(sEmitResponse, 1);

                return false;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    var sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                }
            }

            return false;
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

    public class bitvestLoginBase
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public bitvestLogin data { get; set; }
        public bitvestAccount account { get; set; }
        public string server_hash { get; set; }
        public bitvesttip tip { get; set; }
        public bitvestCurWeight currency_weight { get; set; }
        public double[] rate_limits { get; set; }
        public string last_user_seed { get; set; }
    }

    public class bitvestCurWeight
    {
        public string BTC { get; set; }
        public string ETH { get; set; }
        public string LTC { get; set; }
        public string TOK { get; set; }
        public string DOGE { get; set; }
        public string BCH { get; set; }
    }

    /*{"data":{"self-user-id":46534,"self-username":"Seuntjie",
      "balance":0.00586720655,
      "token_balance":39775.605,
      "ether_balance":0.0001,
      "litecoin_balance":0.0001,"pending":0,"ether_pending":0,"litecoin_pending":0,"address":"12Nfe1Dp9VAFRqKLKE9vgrP28qJH3Aaad4",
      "ether_address":"0x3e10685213a68b3321d6b65352ac9cc94da559f1",
      "litecoin_address":"LQe1t6YCiteYfVJ9n1BXHdXopzmp94CMWd",
      "total_bet":0.01493638,
      "total_won":0.0082235001,
      "total_profit":-0.0067128799,
      "token_total_bet":811453,
      "token_total_won":761301.63,
      "token_total_profit":-50151.37,
      "ether_total_bet":0,
      "ether_total_won":0,
      "ether_total_profit":0,
      "litecoin_total_bet":0,
      "litecoin_total_won":0,
      "litecoin_total_profit":0,
      "bets":2272,
      "server_hash":"30e01c8a1385bd3bd7cf8a2856e73bf639807eccced705159538b074582839a9"}}
      */
    public class bitvestLogin
    {
        public string balance { get; set; }
        public string token_balance { get; set; }
        public string balance_litecoin { get; set; }
        public string balance_dogecoin { get; set; }
        public string balance_bcash { get; set; }
        public string self_username { get; set; }
        public string self_user_id { get; set; }
        public string self_ref_count { get; set; }
        public string self_ref_total_profit { get; set; }
        public string self_total_bet_dice { get; set; }
        public string self_total_won_dice { get; set; }
        public string self_total_bets_dice { get; set; }
        public string session_token { get; set; }

        public string balance_ether { get; set; }
        public decimal pending { get; set; }
        public decimal ether_pending { get; set; }
        public decimal pending_litecoin { get; set; }
        public string address { get; set; }
        public string ether_address { get; set; }
        public string litecoin_address { get; set; }
        public decimal total_bet { get; set; }
        public decimal total_won { get; set; }
        public decimal total_profit { get; set; }
        public decimal token_total_bet { get; set; }
        public decimal token_total_won { get; set; }
        public decimal token_total_profit { get; set; }
        public decimal ether_total_bet { get; set; }
        public decimal ether_total_won { get; set; }
        public decimal ether_total_profit { get; set; }
        public decimal litecoin_total_bet { get; set; }
        public decimal litecoin_total_won { get; set; }
        public decimal litecoin_total_profit { get; set; }
        public decimal dogecoin_total_bet { get; set; }
        public decimal dogecoin_total_won { get; set; }
        public decimal dogecoin_total_profit { get; set; }
        public decimal bcash_total_bet { get; set; }
        public decimal bcash_total_won { get; set; }
        public decimal bcash_total_profit { get; set; }
        public int bets { get; set; }
        public string server_hash { get; set; }
    }

    public class BitVestGetBalance
    {
        public int self_user_id { get; set; }
        public string self_username { get; set; }
        public decimal balance { get; set; }
        public decimal token_balance { get; set; }
        public decimal ether_balance { get; set; }
        public decimal litecoin_balance { get; set; }
        public decimal balance_dogecoin { get; set; }
        public decimal balance_bcash { get; set; }
        public decimal pending { get; set; }
        public decimal ether_pending { get; set; }
        public decimal litecoin_pending { get; set; }
        public string address { get; set; }
        public string ether_address { get; set; }
        public string litecoin_address { get; set; }
        public decimal total_bet { get; set; }
        public decimal total_won { get; set; }
        public decimal total_profit { get; set; }
        public decimal token_total_bet { get; set; }
        public decimal token_total_won { get; set; }
        public decimal token_total_profit { get; set; }
        public decimal ether_total_bet { get; set; }
        public decimal ether_total_won { get; set; }
        public decimal ether_total_profit { get; set; }
        public decimal litecoin_total_bet { get; set; }
        public decimal litecoin_total_won { get; set; }
        public decimal litecoin_total_profit { get; set; }
        public decimal dogecoin_total_bet { get; set; }
        public decimal dogecoin_total_won { get; set; }
        public decimal dogecoin_total_profit { get; set; }
        public decimal bcash_total_bet { get; set; }
        public decimal bcash_total_won { get; set; }
        public decimal bcash_total_profit { get; set; }
        public decimal bets { get; set; }
        public string server_hash { get; set; }
    }

    public class BivestGetBalanceRoot
    {
        public BitVestGetBalance data { get; set; }
    }

    public class bitvesttip
    {
        public bool enabled { get; set; }
    }

    public class bitvestAccount
    {
        public string type { get; set; }
        public string address { get; set; }
        public string secret { get; set; }
    }

    public class bitvestbet
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public bitvestbetdata data { get; set; }
        public bitvestgameresult game_result { get; set; }
        public long game_id { get; set; }
        public string result { get; set; }
        public string server_seed { get; set; }
        public string server_hash { get; set; }
        public string player_seed { get; set; }
    }

    public class bitvestbetdata
    {
        public string balance { get; set; }
        public string pending { get; set; }
        public string balance_ether { get; set; }
        public string token_balance { get; set; }
        public string balance_litecoin { get; set; }
        public string balance_dogecoin { get; set; }
        public string balance_bcash { get; set; }
        public string self_username { get; set; }
        public string self_user_id { get; set; }
    }

    public class bitvestgameresult
    {
        public decimal roll { get; set; }
        public decimal win { get; set; }
        public decimal total_bet { get; set; }
        public decimal multiplier { get; set; }
    }

    public class pdDeposit
    {
        public string address { get; set; }
    }
}
