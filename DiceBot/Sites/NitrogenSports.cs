using System;
using System.Collections.Generic;
using System.Globalization;
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
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace DiceBot.Sites
{
    internal class NitrogenSports : DiceSite
    {
        private readonly Random R = new Random();
        private readonly Dictionary<string, int> Requests = new Dictionary<string, int>();
        private string accesstoken = "";
        private HttpClient Client; // = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        private HttpClientHandler ClientHandlr;
        private string Guid = "";
        public bool iskd;
        private DateTime LastSeedReset = new DateTime();
        private DateTime lastupdate;
        private string link = "";
        private WebSocket NSSocket;
        private string password = "";
        private string token = "";
        private long uid = 0;
        private string username = "";

        public NitrogenSports(cDiceBot Parent)
        {
            this.Parent = Parent;
            AutoInvest = false;
            AutoLogin = true;
            AutoWithdraw = true;
            BetURL = "";
            ChangeSeed = true;
            edge = 1;
            maxRoll = 99.99m;
            Name = "Nitrogen Sports";
            NonceBased = true;
            register = false;
            SiteURL = "https://nitrogensports.eu/r/1435541";
            Tip = true;
            TipUsingName = false;
        }

        private string CreateRandomString()
        {
            //p4s61ntwgyj5s91igpm0zr529
            var length = 25;
            var s = "";
            var chars = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";

            while (s.Length < length)
            {
                s += chars[R.Next(0, chars.Length)];
            }

            return s;
        }

        private void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {
                    if ((DateTime.Now - lastupdate).TotalSeconds > 30)
                    {
                        lastupdate = DateTime.Now;
                        var s = CreateRandomString();

                        //Requests.Add(s,1);
                        NSSocket.Send("[2,\"0." + s + "\",\"ping\",{}]");
                        var result = Client.GetStringAsync("php/login/load_login.php").Result;
                        var tmplogin = JsonUtils.JsonDeserialize<NSLogin>(result);
                        balance = decimal.Parse(tmplogin.balance, NumberFormatInfo.InvariantInfo);
                        Parent.updateBalance(balance);

                        //GetStats();
                    }
                }
                catch
                {
                }
            }
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = new HMACSHA512();

            var charstouse = 5;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            var buffer = new List<byte>();
            var msg = client + "-" + nonce;

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            var hash = betgenerator.ComputeHash(buffer.ToArray());

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
                {
                    lucky %= 10000;

                    return lucky / 100;
                }
            }

            return 0;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        private void GetStats()
        {
            Thread.Sleep(1);
            var t = CreateRandomString();
            var s = "[2,\"0." + t + "\",\"game\",{}]";
            Requests.Add(t, 1);
            NSSocket.Send(s);
        }

        private void ConnectSocket()
        {
            try
            {
                if (NSSocket != null)
                    try
                    {
                        NSSocket.Close();
                    }
                    catch
                    {
                    }

                var cfclearnace = "";
                var cfuid = "";
                var PHPID = "";
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("setting_name", "dice_sound_on"));

                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("php/query/account_get_setting.php", Content).Result.Content.ReadAsStringAsync().Result;

                foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://nitrogensports.eu")))
                {
                    switch (c.Name)
                    {
                        case "PHPSESSID":
                            PHPID = c.Value;

                            break;
                        case "__cfduid":
                            cfuid = c.Value;

                            break;
                        case "cf_clearance":
                            cfclearnace = c.Value;

                            break;
                        case "login_link":
                            link = c.Value;

                            break;
                        case "x-csrftoken":
                            token = c.Value;

                            break;
                    }
                }

                var Cookies = new List<KeyValuePair<string, string>>();
                Cookies.Add(new KeyValuePair<string, string>("PHPSESSID", PHPID));
                Cookies.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                Cookies.Add(new KeyValuePair<string, string>("cf_clearance", cfclearnace));
                Cookies.Add(new KeyValuePair<string, string>("login_link", link));
                Cookies.Add(new KeyValuePair<string, string>("x-csrftoken", token));

                //Cookies.Add(new KeyValuePair<string, string>("csrf_token", token));

                var headers = new List<KeyValuePair<string, string>>();
                /*headers.Add(new KeyValuePair<string, string>("Origin", "https://nitrogensports.eu"));
                headers.Add(new KeyValuePair<string, string>("Host", "nitrogensports.eu"));
                headers.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
                headers.Add(new KeyValuePair<string, string>("Connection", "Upgrade"));
                headers.Add(new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate, sdch, br"));
                //Accept-Encoding: gzip, deflate, sdch, br
                //Accept-Language: en-US,en;q=0.8,ru;q=0.6
                headers.Add(new KeyValuePair<string, string>("Accept-Language", "en-US,en;q=0.8,af;q=0.6"));
                //headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Protocol", "wamp"));
                //headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits"));
                //headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Version", "13"));
                /*Sec-WebSocket-Extensions:permessage-deflate; client_max_window_bits
Sec-WebSocket-Key:a0NUCgmYHsEzWIjTfgBuUQ==
Sec-WebSocket-Protocol:actioncable-v1-json
Sec-WebSocket-Version:13*/
                headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0"));

                NSSocket = new WebSocket("wss://nitrogensports.eu/ws/", "wamp", Cookies, headers,
                                         "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0", "https://nitrogensports.eu",
                                         WebSocketVersion.Rfc6455);

                NSSocket.Closed += NSSocket_Closed;
                NSSocket.Error += NSSocket_Error;
                NSSocket.MessageReceived += NSSocket_MessageReceived;
                NSSocket.Opened += NSSocket_Opened;
                NSSocket.Open();

                while (NSSocket.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }

                //CurrencyChanged();
                iskd = true;
                new Thread(GetBalanceThread).Start();
                finishedlogin(NSSocket.State == WebSocketState.Open);

                //loggedin = true;
            }
            catch
            {
            }
        }

        private void NSSocket_Opened(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void NSSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                //Parent.DumpLog(e.Message, -1);
                if (e.Message.StartsWith("[4"))
                {
                    //error!
                    var msgs = e.Message.Split(new[] {"\",\""}, StringSplitOptions.RemoveEmptyEntries);
                    Parent.updateStatus(msgs[msgs.Length - 1]);
                }

                if (e.Message.StartsWith("[3"))
                {
                    //NOT ERROR!

                    //determine what this is
                    var key = e.Message.Substring("[3,\"0.".Length);
                    key = key.Substring(0, key.IndexOf("\""));

                    //get key from request

                    var tmp = e.Message.Substring(e.Message.IndexOf("\",{\"") + 2);
                    tmp = tmp.Substring(0, tmp.Length - 1);

                    if (Requests.ContainsKey(key))
                    {
                        switch (Requests[key])
                        {
                            case 0:
                                processbet(JsonUtils.JsonDeserialize<NSBet>(tmp));

                                break;
                            case 1:
                                processStats(JsonUtils.JsonDeserialize<NSGame>(tmp));

                                break;
                            case 2:
                                ProcessSeed(JsonUtils.JsonDeserialize<NSSeed>(tmp));

                                break;
                        }

                        Requests.Remove(key);
                    }

                    //NSBet tmpbet = 
                }
            }
            catch (Exception ex)
            {
                Parent.DumpLog(ex.ToString(), -1);
            }

            //throw new NotImplementedException();
        }

        private void processStats(NSGame nSGame)
        {
            wins = (int) nSGame.betsWon;
            losses = (int) nSGame.betsLost;
            bets = (int) nSGame.betsMade;
            Parent.updateWins(wins);
            Parent.updateLosses(losses);
            Parent.updateBets(bets);
        }

        private void processbet(NSBet tmpbbet)
        {
            var newbet = new Bet
            {
                Id = tmpbbet.id,
                Amount = decimal.Parse(tmpbbet.betAmount, NumberFormatInfo.InvariantInfo),
                date = DateTime.Now,
                clientseed = tmpbbet.dice.clientSeed,
                high = tmpbbet.betCondition == "H",
                Chance = tmpbbet.betCondition == "H"
                             ? maxRoll - decimal.Parse(tmpbbet.betTarget, NumberFormatInfo.InvariantInfo)
                             : decimal.Parse(tmpbbet.betTarget, NumberFormatInfo.InvariantInfo),
                nonce = tmpbbet.nonce,
                Guid = Guid,
                Roll = decimal.Parse(tmpbbet.roll, NumberFormatInfo.InvariantInfo),
                serverhash = tmpbbet.dice.serverSeedHash
            };

            newbet.Profit = tmpbbet.outcome == "W" ? decimal.Parse(tmpbbet.profitAmount, NumberFormatInfo.InvariantInfo) : -newbet.Amount;

            if (tmpbbet.outcome == "W")
                wins++;
            else
                losses++;

            bets++;
            balance = decimal.Parse(tmpbbet.balance, NumberFormatInfo.InvariantInfo);
            wagered += newbet.Amount;
            profit += newbet.Profit;
            FinishedBet(newbet);
        }

        private void ProcessSeed(NSSeed tmpSeed)
        {
            SQLiteHelper.InsertSeed(tmpSeed.previousServerSeedHash, tmpSeed.previousServerSeed);
        }

        private void NSSocket_Error(object sender, ErrorEventArgs e)
        {
            Parent.DumpLog(e.Exception.ToString(), -1);

            //throw new NotImplementedException();
        }

        private void NSSocket_Closed(object sender, EventArgs e)
        {
            try
            {
                Parent.updateStatus("Connection Lost! Please close NitrogenSports in your browser");
            }
            catch
            {
            }

            ;

            //throw new NotImplementedException();
        }

        public override void Donate(decimal Amount)
        {
            SendTip("1435541", Amount);
        }

        private void PlaceBetThread(object BetObj)
        {
            var obj = BetObj as PlaceBetObj;
            Guid = obj.Guid;
            var High = obj.High;
            var chance = obj.Chance;
            var amount = obj.Amount;
            var tmpchance = High ? maxRoll - chance : chance;
            var t = CreateRandomString();
            Requests.Add(t, 0);

            var s = string.Format(NumberFormatInfo.InvariantInfo,
                                  "[2,\"0.{0}\",\"bet\",{{\"betAmount\":{1:0.00000000},\"betCondition\":\"{2}\",\"betTarget\":{3:0.00}}}]", t, amount,
                                  High ? "H" : "L", tmpchance);

            NSSocket.Send(s);
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            new Thread(PlaceBetThread).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        public override void ResetSeed()
        {
            var length = R.Next(8, 32);
            var s = "";
            var chars = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";

            while (s.Length < length)
            {
                s += chars[R.Next(0, chars.Length)];
            }

            Thread.Sleep(1);
            var t = CreateRandomString();
            Requests.Add(t, 2);

            NSSocket.Send(string.Format(NumberFormatInfo.InvariantInfo, "[2,\"0.{0}\",\"seed\",{{\"clientSeed\":\"{1}\"}}]", t, s));
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            var pairs = new List<KeyValuePair<string, string>>();

            pairs.Add(new KeyValuePair<string, string>("bitcoin_address", Address));
            pairs.Add(new KeyValuePair<string, string>("bitcoin_amount", Amount.ToString("0.00000000")));
            pairs.Add(new KeyValuePair<string, string>("password", password));
            pairs.Add(new KeyValuePair<string, string>("otp", ""));
            var Content = new FormUrlEncodedContent(pairs);
            var sEmitResponse = Client.PostAsync("php/cashier/bitcoin_request_withdraw.php", Content).Result.Content.ReadAsStringAsync().Result;

            return sEmitResponse.StartsWith("{\"errno\":0");
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("user_id_to", User));
            pairs.Add(new KeyValuePair<string, string>("bitcoin_amount", amount.ToString("0.00000000")));
            pairs.Add(new KeyValuePair<string, string>("password", password));
            pairs.Add(new KeyValuePair<string, string>("otp", ""));
            var Content = new FormUrlEncodedContent(pairs);
            var sEmitResponse = Client.PostAsync("php/cashier/bitcoin_request_transfer_queue.php", Content).Result.Content.ReadAsStringAsync().Result;

            return sEmitResponse.StartsWith("{\"errno\":0");
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;

            ClientHandlr = new HttpClientHandler
                {UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = Prox, UseProxy = Prox != null};

            Client = new HttpClient(ClientHandlr) {BaseAddress = new Uri("https://nitrogensports.eu/")};
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");

            try
            {
                var s1 = "";
                var resp = Client.GetAsync("").Result;

                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;

                        //cflevel = 0;
                        Task.Factory.StartNew(() =>
                                              {
                                                  MessageBox
                                                      .Show("nitrogensports.eu has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                              });

                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "nitrogensports.eu"))
                        {
                            finishedlogin(false);

                            return;
                        }
                    }
                }

                var pairs = new List<KeyValuePair<string, string>>();

                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("captcha_code", ""));
                pairs.Add(new KeyValuePair<string, string>("otp", twofa /*==""?"undefined":twofa*/));
                var Content = new FormUrlEncodedContent(pairs);
                var sEmitResponse = Client.PostAsync("php/login/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                var tmpLogin = JsonUtils.JsonDeserialize<NSLogin>(sEmitResponse);

                if (tmpLogin.errno != 0)
                {
                    finishedlogin(false);

                    return;
                }

                balance = decimal.Parse(tmpLogin.balance, NumberFormatInfo.InvariantInfo);
                token = tmpLogin.csrf_token;
                ConnectSocket();
                balance = decimal.Parse(tmpLogin.balance, NumberFormatInfo.InvariantInfo);
                Parent.updateBalance(balance);
                GetStats();
                password = Password;

                return;
            }
            catch (AggregateException e)
            {
            }
            catch (Exception e)
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
            return true;
        }

        public override void Disconnect()
        {
            iskd = false;

            try
            {
                NSSocket.Close();
            }
            catch
            {
            }
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

    public class NSLogin
    {
        public int errno { get; set; }
        public string error { get; set; }
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string login_mode { get; set; }
        public string login_link { get; set; }
        public string bitcoin_address { get; set; }
        public string chat_nickname { get; set; }
        public string odds_format { get; set; }
        public string chat_token { get; set; }
        public string poker_token { get; set; }
        public string balance { get; set; }
        public string inplay { get; set; }
        public string csrf_token { get; set; }
    }

    public class NSSeed
    {
        public string id { get; set; }
        public string clientSeed { get; set; }
        public string serverSeedHash { get; set; }
        public long betsMade { get; set; }
        public string createdAt { get; set; }
        public string userId { get; set; }
        public string previousGameId { get; set; }
        public string previousClientSeed { get; set; }
        public string previousServerSeed { get; set; }
        public string previousServerSeedHash { get; set; }
    }

    public class NSBet
    {
        public string id { get; set; }
        public string betAmount { get; set; }
        public string betCondition { get; set; }
        public string betPayout { get; set; }
        public string betTarget { get; set; }
        public long nonce { get; set; }
        public string roll { get; set; }
        public string outcome { get; set; }
        public string profitAmount { get; set; }
        public string streak { get; set; }
        public string jackpot { get; set; }
        public string createdAt { get; set; }
        public string diceJackpot { get; set; }
        public NSSeed dice { get; set; }
        public string balance { get; set; }
    }

    public class NSGame
    {
        public string id { get; set; }
        public long userId { get; set; }
        public string clientSeed { get; set; }
        public string serverSeedHash { get; set; }
        public bool active { get; set; }
        public long betsMade { get; set; }
        public long betsWon { get; set; }
        public long betsLost { get; set; }
        public string createdAt { get; set; }
        public string previousGameId { get; set; }
        public string previousClientSeed { get; set; }
        public string previousServerSeed { get; set; }
        public string previousServerSeedHash { get; set; }
        public string max_profit_amount { get; set; }
    }
}
