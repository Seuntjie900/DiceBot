using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using System.Threading;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
namespace DiceBot
{
    class FortuneJack:DiceSite
    {
        long uid = -1;
        public static string[] cCurrencies = new string[11] { "btc", "ltc", "xdg", "clam", "nvc", "dash", "ppc", "nmc", "rdd", "xmr", "fjc" };
        public FortuneJack(cDiceBot Parent)
        {
            this.Parent = Parent;
            NonceBased = false;
            maxRoll = 99.98m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://fortunejack.com";
             Currencies = new string[11] { "btc", "ltc", "xdg", "clam", "nvc", "dash", "ppc", "nmc", "rdd", "xmr", "fjc"};

            Currency = "BTC";
            Name = "FortuneJack";
            Tip = true;
            TipUsingName = false;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://fortunejack.com/affiliate/179043/";
        }

        protected override void CurrencyChanged()
        {
            if (Client != null)
            {
                if (Client.State == WebSocketState.Open)
                {
                    //Client.Send("62");
                    Client.Close();
                    StartSocket();
                    /*string msg = string.Format("61,{0},{2},{1}", uid, ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com"))["PHPSESSID"].Value + "\r\n", Rooms[Currency.ToLower()]);

                    Client.Send(msg);*/
                }
            }
            
        }

        void StartSocket()
        {
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, string>> cookies2 = new List<KeyValuePair<string, string>>();
            //headers.Add(new KeyValuePair<string, string>("Cookie", "_csn_session=" + cookie));
            foreach (Cookie curCookie in ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com")))
            {
                cookies2.Add(new KeyValuePair<string, string>(curCookie.Name, curCookie.Value));
            }
            var domainTableField = ClientHandlr.CookieContainer.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            var domains = (IDictionary)domainTableField.GetValue(ClientHandlr.CookieContainer);

            foreach (var val in domains.Values)
            {
                var type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
                var values = (IDictionary)type.GetValue(val);
                foreach (CookieCollection cooks in values.Values)
                {
                    foreach (Cookie curCookie in cooks)
                    {
                        cookies2.Add(new KeyValuePair<string, string>(curCookie.Name.ToLower(), curCookie.Value));
                    }

                }
            }

            /*headers.Add(new KeyValuePair<string, string>("Origin", "https://fortunejack.com"));
            //headers.Add(new KeyValuePair<string, string>("Host", "btrader.fortunejack.com"));
            headers.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
            headers.Add(new KeyValuePair<string, string>("Connection", "Upgrade"));
            headers.Add(new KeyValuePair<string, string>("Accept-Encoding", "deflate"));
            headers.Add(new KeyValuePair<string, string>("accept-language", "en-GB,en-US;q=0.8,en;q=0.6"));
            headers.Add(new KeyValuePair<string, string>("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36"));
            headers.Add(new KeyValuePair<string, string>("Sec-WebSocket-Extensions", "client_max_window_bits"));
            */
            Client = new WebSocket("wss://btrader.fortunejack.com:8080/ndice", "", cookies2, headers);

            Client.Opened += Client_Opened;
            Client.Error += Client_Error;
            Client.Closed += Client_Closed;
            Client.MessageReceived += Client_MessageReceived;
            Client.DataReceived += Client_DataReceived;
            Client.AutoSendPingInterval = 4;
            Client.EnableAutoSendPing = true;

            Client.Open(); 
        }

        bool IsFJ = false;
        bool IsLoggedIn = false;

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            chance = High ? 99.99m - chance : chance;
            decimal tmpamount = (decimal)amount;
            decimal tmpchancet = (decimal)chance;
            string s = string.Format("64,{0:0.00000000},{1},{2},{3}\r\n", tmpamount, (int)(tmpchancet * 100), High ? 1 : 0, 1/*R.Next(0, int.MaxValue-1000000)*/);
            string[] Something = s.Split('\n');
            
            Client.Send(Something[0]+"\n");
            //new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(High);

        }

        void PlaceBetThread(object _High)
        {
            bool High = (bool)_High;
            chance = High ? 99.99m - chance : chance;
            string s = string.Format("64,{0:0.00000000},{1},{2},{3}\r\n", amount, (int)(chance * 100), High ? 1 : 0, R.Next(0, int.MaxValue));
            
            Client.Send(s);
            
        }

        public override void ResetSeed()
        {
            //throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }
        DateTime LastKeepalive = DateTime.Now;
        void KeepAliveThread()
        {
            while (IsFJ)
            {


                if (IsLoggedIn && IsFJ && (DateTime.Now - LastKeepalive).TotalSeconds > 120)
                {
                    LastKeepalive = DateTime.Now;
                    KeepAlive();


                }
                Thread.Sleep(1000);
            }
        }
        Random R = new Random();
        void KeepAlive()
        {
            try
            {
                string s = WebClient.GetStringAsync("api/main/keepAlive.php?rnd="+R.Next(0, int.MaxValue)).Result;
                if (s.StartsWith("<?xml version="))
                {
                    //<?xml version="1.0"?>
//<userid uid='1020272' />
                    s = s.Substring(s.IndexOf("'")+1);
                    s = s.Substring(0, s.LastIndexOf("'"));
                    long t = -1;
                    if (long.TryParse(s, out t) && uid < 0)
                    {
                        uid = t;
                    }
                }
            }
            catch (AggregateException e)
            {

            }
            catch
            { }
        }
        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            string wdrwToken = WebClient.GetStringAsync("ajax/withdraw.php").Result;
            wdrwToken = wdrwToken.Substring(wdrwToken.IndexOf("wdrwToken"));
            wdrwToken = wdrwToken.Substring(wdrwToken.IndexOf("=")+2);
            wdrwToken = wdrwToken.Substring(0,wdrwToken.IndexOf("'"));


            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("amount", Amount.ToString("0.00000000")));
            pairs.Add(new KeyValuePair<string, string>("payoutHash", Address));
            pairs.Add(new KeyValuePair<string, string>("curID", Curs[Currency.ToLower()].ToString()));
            pairs.Add(new KeyValuePair<string, string>("paymentID", ""));
            pairs.Add(new KeyValuePair<string, string>("wdrwToken", wdrwToken));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string sEmitResponse = WebClient.PostAsync("ajax/wdrw.php?mcur=", Content).Result.Content.ReadAsStringAsync().Result;
            

            return true;
        }
        HttpClient WebClient = null;
        HttpClientHandler ClientHandlr = null;
        CookieContainer cookies = null;
        WebSocket Client = null;
        string ChatToken = "";
        public override void Login(string Username, string Password, string twofa)
        {
            
            try
            {
                /*ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
       | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;*/
                cookies = new CookieContainer();
                ClientHandlr = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    Proxy = (IWebProxy)this.Prox,
                    UseProxy = this.Prox != null
                };
                WebClient = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://fortunejack.com/") };
                WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("br"));
                WebClient.DefaultRequestHeaders.Host = "fortunejack.com";
                WebClient.DefaultRequestHeaders.Add("Origin", "https://fortunejack.com");
                WebClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36");
                string tmps = json.ToDateString(DateTime.UtcNow);
                
                /*HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://fortunejack.com/");
                    if (Prox != null)
                        betrequest.Proxy = Prox;
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    betrequest.CookieContainer = Cookies;
                HttpWebResponse EmitResponse;*/
                string s1 = "";
                    try
                    {
                        HttpResponseMessage resp = WebClient.GetAsync("").Result;
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
                                    System.Windows.Forms.MessageBox.Show("fortunejack.com has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                });
                                if (!Cloudflare.doCFThing(s1, WebClient, ClientHandlr, 0, "fortunejack.com"))
                                {
                                    finishedlogin(false);
                                    return;
                                }

                            }
                        }
                    }
                catch (AggregateException e)
                    {
                        finishedlogin(false);
                        return;
                    }
                    Cookie c = new Cookie();
                foreach (Cookie tc in ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com")))
                {
                    if (tc.Name == "__cfduid")
                    {
                        c = tc;
                        break;
                    }
                }
                /*betrequest = (HttpWebRequest)HttpWebRequest.Create("https://fortunejack.com/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = Cookies;

                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
                string sEmitResponse = WebClient.GetStringAsync("").Result;
                string phpsess = "";
                CookieCollection tmp = ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com"));
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = WebClient.PostAsync("ajax/time.php", Content).Result.Content.ReadAsStringAsync().Result;
                //https://fortunejack.com/ajax/gamesSearch.php
                pairs = new List<KeyValuePair<string, string>>();
                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = WebClient.PostAsync("ajax/gamesSearch.php", Content).Result.Content.ReadAsStringAsync().Result;
               
                pairs = new List<KeyValuePair<string, string>>();
                Content = new FormUrlEncodedContent(pairs);
                sEmitResponse = WebClient.PostAsync("ajax/time.php", Content).Result.Content.ReadAsStringAsync().Result;
                

                tmp = ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com"));
                
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("logName", Username));
                pairs.Add(new KeyValuePair<string, string>("logPassword", Password));
                //1456046111067
                pairs.Add(new KeyValuePair<string, string>("nocache", tmps));
                 WebClient.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");

                Content = new FormUrlEncodedContent(pairs);
                
                try
                {
                    sEmitResponse = WebClient.PostAsync("ajax/login.php", Content).Result.Content.ReadAsStringAsync().Result;
                    if (!sEmitResponse.Contains("success"))
                    { finishedlogin(false); return; }
                }
                catch (AggregateException e)
                { finishedlogin(false); return; }

                sEmitResponse = WebClient.GetStringAsync("user.php").Result;
                sEmitResponse = WebClient.GetStringAsync("games/dice/").Result;
                GetChatToken(sEmitResponse);
                pairs = new List<KeyValuePair<string, string>>();
                Content = new FormUrlEncodedContent(pairs);
                KeepAlive();
                sEmitResponse = WebClient.PostAsync("ajax/time.php", Content).Result.Content.ReadAsStringAsync().Result;
                sEmitResponse = WebClient.GetStringAsync("api/dice4/diceutils.php?act=rooms").Result;
                try
                {

                    FJCurrency[] tmpCurs = json.JsonDeserialize<FJCurrency[]>(sEmitResponse);
                    foreach (FJCurrency tc in tmpCurs)
                    {
                        Curs.Add(tc.currency_name.ToLower(), tc.currency_id);
                        Rooms.Add(tc.currency_name.ToLower(), tc.room_id);
                    }
                }
                catch
                { finishedlogin(false); return; }
                //get stats
                try
                {
                    string stats = WebClient.GetStringAsync("api/dice3/utils.php?stats&rnd=" + R.Next(0, int.MaxValue)).Result;
                    string[] StatsVals = stats.Split('|');
                    wagered = decimal.Parse(StatsVals[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    profit = decimal.Parse(StatsVals[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    bets = int.Parse(StatsVals[2], System.Globalization.NumberFormatInfo.InvariantInfo);
                    wins = int.Parse(StatsVals[3], System.Globalization.NumberFormatInfo.InvariantInfo);
                    losses = int.Parse(StatsVals[4], System.Globalization.NumberFormatInfo.InvariantInfo);
                }
                catch
                {
                    finishedlogin(false);
                    return;
                }
                

                StartSocket();
                while (Client.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }
                IsFJ = true;
                new Thread(new ThreadStart(KeepAliveThread)).Start();
                finishedlogin(true);
                IsLoggedIn = true;
                //Client.Send("67,2");
            }
            catch (AggregateException e)
            {

            }
        }

        private void GetChatToken(string sEmitResponse)
        {
            string stmp = sEmitResponse.Substring(sEmitResponse.IndexOf("chat_token") + "chat_token\" value=\"".Length);
            //chat_token\" value=\"
            //stmp = stmp.Substring(stmp.IndexOf("\"" + "chat_token\" value=\"".Length));
            //tmp = tmp.Substring(tmp.IndexOf("\"" + 1));
            stmp = stmp.Substring(0, stmp.IndexOf("\""));
            ChatToken = stmp;
        }

        Dictionary<string, int> Curs = new Dictionary<string, int>();
        Dictionary<string, int> Rooms = new Dictionary<string, int>();
        public override bool InternalSendTip(string User, decimal amount)
        {
            string sEmitResponse = WebClient.GetStringAsync("games/dice/").Result;
            GetChatToken(sEmitResponse);
                
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("pid", User));
            pairs.Add(new KeyValuePair<string, string>("chattoken", ChatToken));
            pairs.Add(new KeyValuePair<string, string>("amount", amount.ToString("0.00000000")));
            pairs.Add(new KeyValuePair<string, string>("currency_id", Curs[Currency.ToLower()].ToString() ));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            sEmitResponse = WebClient.PostAsync("ajax/tip.php", Content).Result.Content.ReadAsStringAsync().Result;
            return sEmitResponse.Contains("success");
                //return true;
        }

        void Client_DataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }

        public override void Donate(decimal Amount)
        {
            SendTip("", Amount);
        }

        string Incomplete = "";
        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            
            try
            {
                List<string> Msgs = e.Message.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                if (Incomplete != "")
                {
                    Msgs[0] = Incomplete + Msgs[0];
                    Incomplete = "";
                }
                if (!e.Message.EndsWith("\r\n"))
                {
                    Incomplete = Msgs[Msgs.Count - 1];
                    Msgs.RemoveAt(Msgs.Count - 1);
                }
                
                foreach (string s in Msgs)
                {
                    string[] RetObjs = s.Split(',');
                    if (RetObjs[0] == ("63"))
                    {
                        Client.Send("67,2\r\n");
                        balance = decimal.Parse(RetObjs[2], System.Globalization.NumberFormatInfo.InvariantInfo);
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateWagered(wagered);
                        Parent.updateProfit(profit);
                    }
                    if (RetObjs[0] == ("53"))
                    {

                    }
                    if (RetObjs[0] == ("65"))
                    {
                        //string[] values = e.Message.Split(',');

                        Bet Result = new Bet
                        {
                            Amount = decimal.Parse(RetObjs[5], System.Globalization.NumberFormatInfo.InvariantInfo),
                            Id = long.Parse(RetObjs[1]),
                            date = DateTime.Now,
                            Profit = decimal.Parse(RetObjs[6], System.Globalization.NumberFormatInfo.InvariantInfo),
                            Roll = decimal.Parse(RetObjs[9], System.Globalization.NumberFormatInfo.InvariantInfo) / 100m,
                            serverseed = (RetObjs[10]),
                            clientseed = (RetObjs[11]),
                            high = RetObjs[8] == "1",
                            uid = int.Parse(RetObjs[3])
                            
                        };
                        decimal tmpChance = decimal.Parse(RetObjs[7], System.Globalization.NumberFormatInfo.InvariantInfo) / 100m;
                        Result.Chance = Result.high ? (decimal)maxRoll - tmpChance : tmpChance;

                        bool win = false;
                        if ((Result.Roll > 99.99m - Result.Chance && Result.high) || (Result.Roll < Result.Chance && !Result.high))
                        {
                            win = true;
                        }
                        //set win
                        if (win)
                            wins++;
                        else
                            losses++;
                        bets++;
                        wagered += (decimal)Result.Amount;
                        profit += (decimal)Result.Profit;
                        balance += (decimal)Result.Profit;

                        FinishedBet(Result);
                    }
                    if (RetObjs[0] == ("90"))
                    {
                        string error = "";
                        
                        switch (RetObjs[1])
                        {
                            case "1": error = "CONCURRENT_CONNECTION"; break;
                            case "2": error = "INVALID_SESSION"; break;
                            case "3": error = "INSUFFICIENT_FUNDS"; break;
                            case "4": error = "INVALID_BET_AMOUNT"; break;
                            case "5": error = "MONEY_TRANSFER_ERROR"; break;
                        }
                        Parent.updateStatus("An error has occured: "+error);
                    }
                }
            }
            catch
            {
                Parent.updateStatus("An error has occured");
            }
        }

        void Client_Closed(object sender, EventArgs e)
        {
            
        }

        void Client_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            
        }

        void Client_Opened(object sender, EventArgs e) 
        {
            
            string msg = string.Format("61,{0},{2},{1}", uid, ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com"))["PHPSESSID"].Value + "\r\n", Rooms[Currency.ToLower()]);
            
            Client.Send(msg);
           
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
            IsFJ = false;
            IsLoggedIn = false;
            if (Client!=null)
                try
                {
                    Client.Close();
                }
                catch { };
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            SHA512 HashGen = SHA512.Create();
            string seed = server + client;
            byte[] seedbytes = Encoding.UTF8.GetBytes(seed);
            byte[] hash = HashGen.ComputeHash(seedbytes);
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            string hashres = hex.ToString();
            int k = 0;
            while (k < hashres.Length - 6)
            {
                string sub = hashres.Substring(k, k + 6);
                int roll = int.Parse(sub, System.Globalization.NumberStyles.HexNumber);
                k += 6;
                if (roll < 10000000)
                {
                    return ((decimal)(roll % 10000m)) / 100.0m;
                }
                if (k >= hashres.Length - 6)
                {
                    seedbytes = Encoding.UTF8.GetBytes(hashres);
                    hash = HashGen.ComputeHash(seedbytes);
                    hex = new StringBuilder(hash.Length * 2);
                    foreach (byte b in hash)
                        hex.AppendFormat("{0:x2}", b);
                    hashres = hex.ToString();
                    k = 0;
                }
            }
            return 0;
        }

        public static decimal sGetLucky(string server, string client, int Nonce)
        {
            SHA512 HashGen = SHA512.Create();
            string seed = server + client;
            byte[] seedbytes = Encoding.UTF8.GetBytes(seed);
            byte[] hash = HashGen.ComputeHash(seedbytes);
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            string hashres = hex.ToString();
            int k = 0;
            while (k < hashres.Length - 6)
            {
                string sub = hashres.Substring(k, k + 6);
                int roll = int.Parse(sub, System.Globalization.NumberStyles.HexNumber);
                k += 6;
                if (roll < 10000000)
                {
                    return ((decimal)(roll % 10000m)) / 100.0m;
                }
                if (k >= hashres.Length - 6)
                {
                    seedbytes = Encoding.UTF8.GetBytes(hashres);
                    hash = HashGen.ComputeHash(seedbytes);
                    hex = new StringBuilder(hash.Length * 2);
                    foreach (byte b in hash)
                        hex.AppendFormat("{0:x2}", b);
                    hashres = hex.ToString();
                    k = 0;
                }
            }
            return 0;
        }
    }
    public enum FJProtocols
    {

    }
    public class FJCurrency
    {
        public int room_id { get; set; }
        public int currency_id { get; set; }
        public string currency_name { get; set; }
        public string bal { get; set; }
    }
}
