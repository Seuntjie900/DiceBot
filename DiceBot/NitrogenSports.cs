using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace DiceBot
{
    class NitrogenSports : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool iskd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        string token = "";
        string link = "";
        WebSocket NSSocket = null;
        public NitrogenSports(cDiceBot Parent)
        {
            this.Parent = Parent;
            this.AutoInvest = false;
            this.AutoLogin = true;
            this.AutoWithdraw = true;
            this.BetURL = "";
            this.ChangeSeed = true;
            this.edge = 1;
            this.maxRoll = 99.99m;
            this.Name = "Nitrogen Sports";
            this.NonceBased = true;
            this.register = false;
            this.SiteURL = "https://nitrogensports.eu/r/1435541";
            this.Tip = true;
            this.TipUsingName = false;

        }
        string CreateRandomString()
        {
            //p4s61ntwgyj5s91igpm0zr529
            int length = 25;
            string s = "";
            string chars = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            while (s.Length < length)
            {
                s += chars[R.Next(0, chars.Length)];
            } return s;
        }
        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {
                    if ((DateTime.Now - lastupdate).TotalSeconds > 30)
                    {
                        lastupdate = DateTime.Now;
                        string s = CreateRandomString();
                        //Requests.Add(s,1);
                        NSSocket.Send("[2,\"0." + s + "\",\"ping\",{}]");
                        string result = Client.GetStringAsync("php/login/load_login.php").Result;
                        NSLogin tmplogin = json.JsonDeserialize<NSLogin>(result);
                        balance = decimal.Parse(tmplogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                        Parent.updateBalance(balance);
                        //GetStats();


                    }
                }
                catch { }
            }
        }

        void GetStats()
        {
            Thread.Sleep(1);
            string t = CreateRandomString();
            string s = "[2,\"0." + t + "\",\"game\",{}]";
            Requests.Add(t, 1);
            NSSocket.Send(s);
        }

        void ConnectSocket()
        {
            try
            {
                if (NSSocket != null)
                {
                    try
                    {
                        NSSocket.Close();
                    }
                    catch { }
                }
                string cfclearnace = "";
                string cfuid = "";
                string PHPID = "";
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("setting_name", "dice_sound_on"));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("php/query/account_get_setting.php", Content).Result.Content.ReadAsStringAsync().Result;

                foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://nitrogensports.eu")))
                {
                    switch (c.Name)
                    {
                        case "PHPSESSID": PHPID = c.Value; break;
                        case "__cfduid": cfuid = c.Value; break;
                        case "cf_clearance": cfclearnace = c.Value; break;
                        case "login_link": link = c.Value; break;
                        case "x-csrftoken": token = c.Value; break;

                        default: break;
                    }
                }


                List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                Cookies.Add(new KeyValuePair<string, string>("PHPSESSID", PHPID));
                Cookies.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                Cookies.Add(new KeyValuePair<string, string>("cf_clearance", cfclearnace));
                Cookies.Add(new KeyValuePair<string, string>("login_link", link));
                Cookies.Add(new KeyValuePair<string, string>("x-csrftoken", token));
                //Cookies.Add(new KeyValuePair<string, string>("csrf_token", token));

                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
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

                NSSocket = new WebSocket("wss://nitrogensports.eu/ws/", "wamp", Cookies, headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0", "https://nitrogensports.eu", WebSocketVersion.Rfc6455, null);

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
                new Thread(new ThreadStart(GetBalanceThread)).Start();
                finishedlogin(NSSocket.State == WebSocketState.Open);
                return;
                //loggedin = true;


            }
            catch
            {

            }
        }

        void NSSocket_Opened(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
        Dictionary<string, int> Requests = new Dictionary<string, int>();
        void NSSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                //Parent.DumpLog(e.Message, -1);
                if (e.Message.StartsWith("[4"))
                {
                    //error!
                    string[] msgs = e.Message.Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
                    Parent.updateStatus(msgs[msgs.Length - 1]);
                }
                if (e.Message.StartsWith("[3"))
                {
                    //NOT ERROR!

                    //determine what this is
                    string key = e.Message.Substring("[3,\"0.".Length);
                    key = key.Substring(0, key.IndexOf("\""));
                    //get key from request

                    string tmp = e.Message.Substring(e.Message.IndexOf("\",{\"") + 2);
                    tmp = tmp.Substring(0, tmp.Length - 1);

                    if (Requests.ContainsKey(key))
                    {
                        switch (Requests[key])
                        {
                            case 0: processbet(json.JsonDeserialize<NSBet>(tmp)); break;
                            case 1: processStats(json.JsonDeserialize<NSGame>(tmp)); break;
                            case 2: ProcessSeed(json.JsonDeserialize<NSSeed>(tmp)); break;
                        }
                        Requests.Remove(key);
                    }




                    //NSBet tmpbet = 
                }
            }
            catch (Exception ex)
            {
                Parent.DumpLog(ex.ToString(),-1);
            }
            //throw new NotImplementedException();
        }

        private void processStats(NSGame nSGame)
        {
            wins = (int)nSGame.betsWon;
            losses = (int)nSGame.betsLost;
            bets = (int)nSGame.betsMade;
            Parent.updateWins(wins);
            Parent.updateLosses(losses);
            Parent.updateBets(bets);
        }

        void processbet(NSBet tmpbbet)
        {
            Bet newbet = new Bet
            {
                Id = tmpbbet.id,
                Amount = decimal.Parse(tmpbbet.betAmount, System.Globalization.NumberFormatInfo.InvariantInfo),
                date = DateTime.Now,
                clientseed = tmpbbet.dice.clientSeed,
                high = tmpbbet.betCondition == "H",
                Chance = tmpbbet.betCondition == "H" ? maxRoll - decimal.Parse(tmpbbet.betTarget, System.Globalization.NumberFormatInfo.InvariantInfo) : decimal.Parse(tmpbbet.betTarget, System.Globalization.NumberFormatInfo.InvariantInfo),
                nonce = tmpbbet.nonce,
                Guid=this.Guid,
                Roll = decimal.Parse(tmpbbet.roll, System.Globalization.NumberFormatInfo.InvariantInfo),
                serverhash = tmpbbet.dice.serverSeedHash
            };
            newbet.Profit = tmpbbet.outcome == "W"?decimal.Parse(tmpbbet.profitAmount, System.Globalization.NumberFormatInfo.InvariantInfo):-newbet.Amount;
            if (tmpbbet.outcome == "W")
                wins++;
            else
                losses++;
            bets++;
            balance = decimal.Parse(tmpbbet.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
            wagered += newbet.Amount;
            profit += newbet.Profit;
            FinishedBet(newbet);
        }
        void ProcessSeed(NSSeed tmpSeed)
        {
            sqlite_helper.InsertSeed(tmpSeed.previousServerSeedHash, tmpSeed.previousServerSeed);
        }
        void NSSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Parent.DumpLog(e.Exception.ToString(), -1);
            //throw new NotImplementedException();
        }

        void NSSocket_Closed(object sender, EventArgs e)
        {
            try
            {
                Parent.updateStatus("Connection Lost! Please close NitrogenSports in your browser");
            }
            catch { };
            //throw new NotImplementedException();
        }
        public override void Donate(decimal Amount)
        {
            SendTip("1435541", Amount);
        }
        string Guid = "";
        void PlaceBetThread(object BetObj)
        {
            PlaceBetObj obj = BetObj as PlaceBetObj;
            this.Guid = obj.Guid;
            bool High = obj.High;
            decimal chance = obj.Chance;
            decimal amount = obj.Amount;
            decimal tmpchance = High ? maxRoll - chance : chance;
            string t = CreateRandomString();
            this.Requests.Add(t, 0);
            string s = string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"[2,\"0.{0}\",\"bet\",{{\"betAmount\":{1:0.00000000},\"betCondition\":\"{2}\",\"betTarget\":{3:0.00}}}]", t, amount, High ? "H" : "L", tmpchance);
            NSSocket.Send(s);
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        Random R = new Random();
        public override void ResetSeed()
        {
            int length = R.Next(8, 32);
            string s = "";
            string chars = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            while (s.Length < length)
            {
                s += chars[R.Next(0, chars.Length)];
            }
            Thread.Sleep(1);
            string t = CreateRandomString();
            Requests.Add(t, 2);

            NSSocket.Send(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"[2,\"0.{0}\",\"seed\",{{\"clientSeed\":\"{1}\"}}]", t, s));
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }
        string password = "";
        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

            pairs.Add(new KeyValuePair<string, string>("bitcoin_address", Address));
            pairs.Add(new KeyValuePair<string, string>("bitcoin_amount", Amount.ToString("0.00000000")));
            pairs.Add(new KeyValuePair<string, string>("password", password));
            pairs.Add(new KeyValuePair<string, string>("otp", ""));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string sEmitResponse = Client.PostAsync("php/cashier/bitcoin_request_withdraw.php", Content).Result.Content.ReadAsStringAsync().Result;
            return sEmitResponse.StartsWith("{\"errno\":0");
        }
        public override bool InternalSendTip(string User, decimal amount)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("user_id_to", User));
            pairs.Add(new KeyValuePair<string, string>("bitcoin_amount", amount.ToString("0.00000000")));
            pairs.Add(new KeyValuePair<string, string>("password", password));
            pairs.Add(new KeyValuePair<string, string>("otp", ""));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string sEmitResponse = Client.PostAsync("php/cashier/bitcoin_request_transfer_queue.php", Content).Result.Content.ReadAsStringAsync().Result;
            return sEmitResponse.StartsWith("{\"errno\":0");
        }
        public override void Login(string Username, string Password, string twofa)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
       | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://nitrogensports.eu/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");


            try
            {
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
                        //cflevel = 0;
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Windows.Forms.MessageBox.Show("nitrogensports.eu has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "nitrogensports.eu"))
                        {
                            
                            finishedlogin(false);
                            return;
                        }

                    }
                }
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("captcha_code", ""));
                pairs.Add(new KeyValuePair<string, string>("otp", twofa/*==""?"undefined":twofa*/));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("php/login/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                NSLogin tmpLogin = json.JsonDeserialize<NSLogin>(sEmitResponse);
                if (tmpLogin.errno != 0)
                {
                    finishedlogin(false);
                    return;

                }
                else
                {
                    this.balance = decimal.Parse(tmpLogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    token = tmpLogin.csrf_token;
                    ConnectSocket();
                    balance = decimal.Parse(tmpLogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    Parent.updateBalance(balance);
                    GetStats();
                    this.password = Password;
                    return;
                }

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
            catch { }
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
