using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using System.Net.Http;

namespace DiceBot
{
    class bitdice:DiceSite
    {

        public static string[] cCurrencies = new string[] { "btc", "doge", "ltc", "clam", "eth" };
        WebSocket Client;// = new WebSocket("");
        public bitdice(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = true;
            AutoWithdraw = true;
            AutoInvest = true;
            Tip = true;
            TipUsingName = false;
            ChangeSeed = true;
            Name = "BitDice";
            this.Parent = Parent;
            SiteURL = "https://bitdice.me";
            /*Client = new WebSocket("");
            Client.Opened += Client_Opened;
            Client.Error += Client_Error;
            Client.Closed += Client_Closed;
            Client.MessageReceived += Client_MessageReceived;*/
            AutoUpdate = false;
            Currencies = new string[] { "btc", "doge", "ltc", "clam", "eth" };
        }

        void getDeposit(string html)
        {
            string tmp = html.Substring(html.IndexOf("href=\"bitcoin:") + "href=\"bitcoin:".Length);
            Parent.updateDeposit(tmp.Substring(0, tmp.IndexOf("?")));
        }

        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            socketbase tmp = json.JsonDeserialize<socketbase>(e.Message);
            if (!string.IsNullOrEmpty(tmp.method))
            {
                switch (tmp.method)
                {
                    case "chat:new": Client_ChatReceived(json.JsonDeserialize<bitchatSocket>(e.Message.Replace("params", "_params"))._params); break;
                    case "stat.global": break;
                    case "stat.user": Client_UserStats(json.JsonDeserialize<bitstatsusersocket>(e.Message.Replace("params", "_params"))._params); break;
                    case "stat.bets": Client_BetResult(json.JsonDeserialize<bitstatsbetsocket>(e.Message.Replace("params", "_params").Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}"))._params); break;
                }
            }
        }

        void Client_Closed(object sender, EventArgs e)
        {
            
        }

        void Client_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            
        }

        bool loggedin = false;
        void Client_Opened(object sender, EventArgs e)
        {
            /*if (!loggedin)
            {
                loggedin = true;
                finishedlogin(true);
            }*/
        }

        

        void Client_UserStats(bitstatsbuser User)
        {
            balance = User.balance;
            bets = User.total_bets;
            wagered = User.wagered;
            profit = User.profit;
        }
        string username = "seuntjie";
        void Client_BetResult(bitstatsbet Bet)
        {
            if (Bet.user.username == username)
            {
                Bet tmp = new Bet();
                tmp.Amount = decimal.Parse(Bet.amount, System.Globalization.NumberFormatInfo.InvariantInfo);
                tmp.date = DateTime.Now;
                tmp.Id = (decimal)(Bet.id);
                tmp.Profit = decimal.Parse(Bet.win, System.Globalization.NumberFormatInfo.InvariantInfo);
                tmp.Roll = decimal.Parse(Bet.lucky, System.Globalization.NumberFormatInfo.InvariantInfo);
                tmp.high = Bet.high;
                tmp.Chance = decimal.Parse(Bet.chance, System.Globalization.NumberFormatInfo.InvariantInfo);
                //tmp.no = decimal.Parse(Bet.amount, System.Globalization.NumberFormatInfo.InvariantInfo);
                FinishedBet(tmp);
            }
        }

        int id = 1;
        //BitDiceClient Client = new BitDiceClient();
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            string s = string.Format("{{\"jsonrpc\":\"2.0\",\"method\":\"bets:make\",\"params\":{{\"amount\":\"{0:0.00000000}\",\"chance\":\"{1:0.000000}\",\"type\":\"{2}\"}},\"id\":{3}}}", amount, chance, High?"high":"low",id++);
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
        }

        public override void ResetSeed()
        {
            string s = "{\"jsonrpc\":\"2.0\",\"method\":\"secret:change\",\"params\":{},\"id\":"+(id++)+"}";
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            string s = string.Format("{{\"jsonrpc\":\"2.0\",\"method\":\"user:cashout\",\"params\":{{\"amount\":\"{0:0.00000000}\",\"address\":\"{1}\"}},\"id\":{2}}}", Amount, Address, id++);
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
            return true;
        }

        HttpClient WebClient = null;
        HttpClientHandler ClientHandlr = null;
        CookieContainer cookies = null;
        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
       | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;
                cookies = new CookieContainer();
                ClientHandlr = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    Proxy = (IWebProxy)this.Prox,
                    UseProxy = this.Prox != null
                };
                WebClient = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitdice.me/") };
                WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                WebClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
                /*HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
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
                                    System.Windows.Forms.MessageBox.Show("bitdice.me has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                                });
                                if (!Cloudflare.doCFThing(s1, WebClient, ClientHandlr, 0, "www.bitdice.me"))
                                {
                                    finishedlogin(false);
                                    return;
                                }
                                /*if (!Cloudflare.doCFThing(s1, WebClient, ClientHandlr, 0, "www.bitdice.me"))
                                {
                                    finishedlogin(false);
                                    return;
                                }*/

                            }
                        }
                    }
                catch
                    {
                        finishedlogin(false);
                        return;
                    }
                    Cookie c = new Cookie();
                foreach (Cookie tc in ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me")))
                {
                    if (tc.Name == "__cfduid")
                    {
                        c = tc;
                        break;
                    }
                }
                /*betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = Cookies;

                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
                string sEmitResponse = WebClient.GetStringAsync("").Result;
                getcsrf(sEmitResponse);

                cookie = ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me"))["_csn_session"].Value;
                //betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/users/sign_in");
                //betrequest.Method = "POST";
                //betrequest.CookieContainer = Cookies;

                /*string post = string.Format("
                 * utf8=%E2%9C%93
                 * user%5Busername%5D={0}
                 * user%5Bpassword%5D={1}
                 * user%5Botp_code%5D=
                 * button=", Username, Password);
                username = Username;
                betrequest.ContentLength = post.Length;

                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));
                betrequest.Headers.Add("X-CSRF-Token", csrf);
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("utf8", "✓"));
                pairs.Add(new KeyValuePair<string, string>("user[username]", Username));
                pairs.Add(new KeyValuePair<string, string>("user[password]", Password));
                pairs.Add(new KeyValuePair<string, string>("user[otp_code]", twofa ));
                pairs.Add(new KeyValuePair<string, string>("button", ""));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                if (WebClient.DefaultRequestHeaders.Contains("X-CSRF-Token"))
                {
                    WebClient.DefaultRequestHeaders.Remove("X-CSRF-Token");
                }
                WebClient.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
                
                username = Username;
                try
                {
                    sEmitResponse = WebClient.PostAsync("users/sign_in", Content).Result.Content.ReadAsStringAsync().Result;
                }
                catch { finishedlogin(false); }
                cookie = ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me"))["_csn_session"].Value;
                if (WebClient.DefaultRequestHeaders.Contains("X-CSRF-Token"))
                {

                }
                try
                {
                    ClientHandlr.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));
                }catch
                { }
                /*betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = Cookies;
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));

                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
                try
                {
                    sEmitResponse = WebClient.GetStringAsync("").Result;

                }
                    catch (AggregateException e) 
                { 
                        finishedlogin(false); }
                catch { finishedlogin(false); }
                getDeposit(sEmitResponse);
                getcsrf(sEmitResponse);
                ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me"))["_csn_session"].Value = cookie;
                //cookie = ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me"))["_csn_session"].Value;

                getstream(sEmitResponse);

                getcsrf(sEmitResponse);
                getstream(sEmitResponse);
                if (WebClient.DefaultRequestHeaders.Contains("X-CSRF-Token"))
                {
                    WebClient.DefaultRequestHeaders.Remove("X-CSRF-Token");
                }
                WebClient.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
                if (Client != null)
                    Client.Close();
                
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                //headers.Add(new KeyValuePair<string, string>("Cookie", "_csn_session=" + cookie));
                List<KeyValuePair<string, string>> cookies2 = new List<KeyValuePair<string, string>>();
                cookies2.Add(new KeyValuePair<string,string>("_csn_session", cookie));
                cookies2.Add(new KeyValuePair<string, string>("__cfduid", c.Value));
                headers.Add(new KeyValuePair<string, string>("Origin", "https://www.bitdice.me"));
                headers.Add(new KeyValuePair<string, string>("Host", "www.bitdice.me"));
                headers.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
                headers.Add(new KeyValuePair<string, string>("Connection", "keep-alive, Upgrade"));
                headers.Add(new KeyValuePair<string, string>("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0"));

                Client = new WebSocket("wss://www.bitdice.me/stream/" + stream, "", cookies2, headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0",
                    "https://www.bitdice.me", WebSocketVersion.Rfc6455, null, System.Security.Authentication.SslProtocols.Tls| System.Security.Authentication.SslProtocols.Tls11| System.Security.Authentication.SslProtocols.Tls12);
                
                
                Client.Opened += Client_Opened;
                Client.Error += Client_Error;
                Client.Closed += Client_Closed;
                Client.MessageReceived += Client_MessageReceived;
                
                
                Client.Open();
                while (Client.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }
                //CurrencyChanged();
                finishedlogin(Client.State == WebSocketState.Open);
                loggedin = true;
                System.Windows.Forms.MessageBox.Show("Due to current limitations of the API, I can't show you your stats until you place a valid bet. Sorry.\n\nAlso, you will need to reselect your currency. If you already selected the currency you want to play in, please select another first, and then switch back.", "Stats Errors", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (WebException e)
            {
                if (e.Response!=null)
                {
                    string s = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                }
            }
            catch
            {
                finishedlogin(false);
            }
        }

        void Client_ChatReceived(bitChatReceived Chat)
        {

            ReceivedChatMessage(string.Format("{0} {1}{2} {3} {4}", Chat.date, Chat.symbol, Chat.user_id, Chat.username, Chat.message));
        }

        public override bool Register(string Username, string Password)
        {
            try
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = new CookieContainer();

                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                getcsrf(sEmitResponse);
                getstream(sEmitResponse);
                cookie = EmitResponse.Cookies["_csn_session"].Value;
                if (Client != null)
                    Client.Close();
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                headers.Add(new KeyValuePair<string, string>("Cookie", "_csn_session=" + cookie));

                Client = new WebSocket("wss://www.bitdice.me/stream/" + stream, "", null, headers, "dicebot", "http://bitdice.me", WebSocketVersion.Rfc6455);

                Client.Opened += Client_Opened;
                Client.Error += Client_Error;
                Client.Closed += Client_Closed;
                Client.MessageReceived += Client_MessageReceived;
                Client.Open();

                while (Client.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }
                if (Client.State == WebSocketState.Open)
                {
                    Client.Send("{\"jsonrpc\":\"2.0\",\"method\":\"user:update\",\"params\":{\"username\":\"" + Username + "\",\"user_seed\":\"1256e154283ea05b9538\",\"hide_bets_below\":\"0.0\",\"hide_other_bets\":false},\"id\":1}");
                }
                else
                {
                    finishedlogin(false);
                    return false;
                }


                betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/users/password");


                betrequest.Method = "POST";
                betrequest.CookieContainer = new CookieContainer();

                string post = string.Format("user%5Bpassword%5D={0}&user%5Bpassword_confirmation%5D={0}", Password);
                username = Username;
                betrequest.ContentLength = post.Length;

                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));
                betrequest.Headers.Add("X-CSRF-Token", csrf);
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                cookie = EmitResponse.Cookies["_csn_session"].Value;


                betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = new CookieContainer();
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));

                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                getDeposit(sEmitResponse);
                getcsrf(sEmitResponse);
                cookie = EmitResponse.Cookies["_csn_session"].Value;

                getstream(sEmitResponse);

                getcsrf(sEmitResponse);
                getstream(sEmitResponse);
                if (Client != null)
                    Client.Close();
                headers = new List<KeyValuePair<string, string>>();
                headers.Add(new KeyValuePair<string, string>("Cookie", "_csn_session=" + cookie));
                Client = new WebSocket("wss://www.bitdice.me/stream/" + stream, "", null, headers, "dicebot", "http://bitdice.me", WebSocketVersion.Rfc6455);

                Client.Opened += Client_Opened;
                Client.Error += Client_Error;
                Client.Closed += Client_Closed;
                Client.MessageReceived += Client_MessageReceived;
                Client.Open();
                while (Client.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }
                finishedlogin(Client.State == WebSocketState.Open);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            cookie =csrf = stream = "";
            loggedin = false;
            if (Client!=null)
            Client.Close();
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            string s = "{\"jsonrpc\":\"2.0\",\"method\":\"chat:post\",\"params\":{\"message\":\""+Message+"\"},\"id\":"+(id++)+"}";
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
        }

        public override bool Invest(decimal Amount)
        {
            string s = "{\"jsonrpc\":\"2.0\",\"method\":\"invest:invest\",\"params\":{\"amount\":\""+Amount.ToString("0.00000000",System.Globalization.NumberFormatInfo.InvariantInfo) +"\"},\"id\":"+(id++)+"}";
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
            return true;
        }
        public override void Donate(decimal Amount)
        {
            SendTip("1426", Amount);
        }
        public override bool InternalSendTip(string User, decimal amount)
        {
            SendChatMessage(string.Format("/tip {0} {1:0.00000000}", User, amount));
            return true;
        }
        string cookie = "";
        string stream = "";
        string csrf = "";
        protected override void CurrencyChanged()
        {
            if (cookie != "")
            {
                /*HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/users/currency");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.Method = "POST";
                string post = "wallet%5Bcurrency%5D=" + Currency;
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = new CookieContainer();
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));
                betrequest.Headers.Add("X-CSRF-Token", csrf);
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("wallet[5Bcurrency]", Currency));
               
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                if (WebClient.DefaultRequestHeaders.Contains("X-CSRF-Token"))
                {
                    WebClient.DefaultRequestHeaders.Remove("X-CSRF-Token");
                }
                WebClient.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);

                string sEmitResponse = "";
                try
                {
                    sEmitResponse = WebClient.PostAsync("users/sign_in", Content).Result.Content.ReadAsStringAsync().Result;
                }
                catch { finishedlogin(false); }

                cookie = ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me"))["_csn_session"].Value;
                /*betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = new CookieContainer();
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));
                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();*/
                sEmitResponse = WebClient.GetStringAsync("").Result;
                getDeposit(sEmitResponse);
                getcsrf(sEmitResponse);
                getstream(sEmitResponse);
                if (Client != null)
                    Client.Close();
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                //headers.Add(new KeyValuePair<string, string>("Cookie", "_csn_session=" + cookie));
                List<KeyValuePair<string, string>> cookies2 = new List<KeyValuePair<string, string>>();
                cookies2.Add(new KeyValuePair<string, string>("_csn_session", cookie));
                cookies2.Add(new KeyValuePair<string, string>("__cfduid", cookie = ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me"))["__cfduid"].Value));
                headers.Add(new KeyValuePair<string, string>("Origin", "https://www.bitdice.me"));
                headers.Add(new KeyValuePair<string, string>("Host", "www.bitdice.me"));
                headers.Add(new KeyValuePair<string, string>("Upgrade", "websocket"));
                headers.Add(new KeyValuePair<string, string>("Connection", "keep-alive, Upgrade"));
                headers.Add(new KeyValuePair<string, string>("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0"));

                Client = new WebSocket("wss://www.bitdice.me/stream/" + stream, "", cookies2, headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0",
                    "https://www.bitdice.me", WebSocketVersion.Rfc6455, null, System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);


                Client.Opened += Client_Opened;
                Client.Error += Client_Error;
                Client.Closed += Client_Closed;
                Client.MessageReceived += Client_MessageReceived;


                Client.Open();
                while (Client.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(100);
                }
            }
        }

        void getcsrf(string page)
        {
            string s = page;
            bool found = false;
            while (!found)
            {
                try
                {
                    s = s.Substring(s.IndexOf("<meta"));
                    if (s.Substring(0, s.IndexOf(">")).Contains("name=\"csrf-token"))
                    {
                        s = s.Substring(0, s.IndexOf(">"));
                        found=true;
                        break;
                    }
                    s = s.Substring(s.IndexOf(">") + 1);
                }
                catch
                {
                    break;
                }
            }
            if (found)
            {
                string c = s.Substring(s.IndexOf("content=\"")+"content=\"".Length);
                c = c.Substring(0, c.IndexOf("\""));
                csrf = c;
            }
        }
        void getstream(string page)
        {
            string s = page.Substring(page.IndexOf("<body data-request=\"") + "<body data-request=\"".Length);
            string stream = s.Substring(0, s.IndexOf("\""));
            this.stream = stream;

        }
        public override void SetProxy(string host, int port)
        {
            base.SetProxy(host, port);
           // Client.prox = Prox;
        }
        public override void SetProxy(string host, int port, string username, string password)
        {
            base.SetProxy(host, port, username, password);
            //Client.prox = Prox;
        }
        public override decimal GetLucky(string server, string client, int nonce)
        {
            return base.GetLucky(server, client, nonce);
        }
    }
    
    public class socketbase
    {
        public string jsonrpc { get; set; }
        public string method { get; set; }
        public int id { get; set; }
        public object result { get; set; }
        public socketbase()
        {
            id = 1;
        }
    }
    public class bitchatSocket:socketbase
    {
        public bitChatReceived _params { get; set; }
    }

    public class bitChatReceived
    {
        public string date { get; set; }
        public string username { get; set; }
        public int user_id { get; set; }
        public string message { get; set; }
        public int level { get; set; }
        public string symbol { get; set; }
    }

    public class bitbetreturn:socketbase
    {
        new public bitbetmini result { get; set; }
    }
    public class bitbetmini
    {
        public string bet_amount { get; set; }
        public string balance { get; set; }
        public string status { get; set; }
    }
    public class bitstatsusersocket : socketbase
    {
        public bitstatsbuser _params { get; set; }
    }
    public class bitstatsbuser
    {
        public decimal balance { get; set; }
        public int total_bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
    }
    public class bitstatsbetsocket : socketbase
    {
        public bitstatsbet _params { get; set; }
    }
    public class bitstatsbet
    {
        public long id { get; set; }
        public string created_at { get; set; }
        public string amount { get; set; }
        public string chance { get; set; }
        public bool high { get; set; }
        public string lucky { get; set; }
        public bool result { get; set; }
        public string win { get; set; }
        public decimal target { get; set; }
        public decimal mutliplier { get; set; }
        public bituser user { get; set; }
    }
    public class bituser
    {
        public string username { get; set; }
    }
    public class bitresetseedsocket : socketbase
    {
        new public bitresetseed result { get; set; }
    }
    public class bitresetseed
    {
        public bitOld old { get; set; }
        public bitNew _new { get; set; }
    }
    public class bitOld
    {
        public string secret { get; set; }
        public string secret_hash { get; set; }
    }
    public class bitNew
    {
        public string secret { get; set; }
    }
}
