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
using System.Net.Sockets;

namespace DiceBot
{
    class bitdice:DiceSite
    {

        public static string[] cCurrencies = new string[] { "btc", "doge", "ltc", "eth" };
        WebSocket Client;// = new WebSocket("");
        public bitdice(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = true;
            
            Tip = true;
            TipUsingName = true;
            ChangeSeed = false;
            NonceBased = false;
            Name = "BitDice";
            this.Parent = Parent;
            SiteURL = "https://www.bitdice.me/?r=82";
            /*Client = new WebSocket("");
            Client.Opened += Client_Opened;
            Client.Error += Client_Error;
            Client.Closed += Client_Closed;
            Client.MessageReceived += Client_MessageReceived;*/
            AutoUpdate = false;
            Currencies = new string[] { "btc", "doge", "ltc", "eth" };
        }

        void getDeposit(string html)
        {
            string tmp = html.Substring(html.IndexOf("href=\"bitcoin:") + "href=\"bitcoin:".Length);
            Parent.updateDeposit(tmp.Substring(0, tmp.IndexOf("?")));
        }

        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                Parent.DumpLog(e.Message, 4);
                string s = e.Message.Replace("\\\"", "\"").Replace("\"{","{").Replace("}\"","}");
                //Parent.DumpLog("", -1);
                //Parent.DumpLog("", -1);
                //Parent.DumpLog(s,-1);
                bitdicebetbase tmp = json.JsonDeserialize<bitdicebetbase>(s);
                if (tmp.type == "confirm_subscription")
                {
                    switch (tmp.identifier.channel)
                    {
                        case "ProfileChannel": Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"ProfileChannel\\\"}\",\"data\":\"{\\\"action\\\":\\\"profile\\\"}\"}");
                            Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"ProfileChannel\\\"}\",\"data\":\"{\\\"action\\\":\\\"dashboard\\\"}\"}");
                            break;
                        case "WalletChannel": Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"WalletChannel\\\"}\",\"data\":\"{\\\"balance\\\":\\\"profile\\\"}\"}");
                            break;
                        case "EventsChannel": Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"EventsChannel\\\"}\",\"data\":\"{\\\"action\\\":\\\"events\\\"}\"}"); break;
                        case "DiceChannel": Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"DiceChannel\\\"}\",\"data\":\"{\\\"action\\\":\\\"secret\\\"}\"}"); break;
                    }
                    return;
                }
                else if (tmp.message.type=="changeCurrency")
                {
                    Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"ProfileChannel\\\"}\",\"data\":\"{\\\"action\\\":\\\"dashboard\\\"}\"}");
                    Client.Send("{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"WalletChannel\\\"}\",\"data\":\"{\\\"balance\\\":\\\"profile\\\"}\"}");
                }
                else if (tmp.message.type == "balances")
                {
                    bitdicebalancedata tmp2 = json.JsonDeserialize<bitdicebalancedata>(s);
                    switch (tmp2.message.data.active)
                    {
                        case "doge": balance = decimal.Parse(tmp2.message.data.wallets.doge.balance, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "btc": balance = decimal.Parse(tmp2.message.data.wallets.btc.balance, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "ltc": balance = decimal.Parse(tmp2.message.data.wallets.ltc.balance, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                        case "eth": balance = decimal.Parse(tmp2.message.data.wallets.eth.balance, System.Globalization.NumberFormatInfo.InvariantInfo); break;
                            
                    }
                    Parent.updateBalance(balance);
                }
                else if (tmp.message.type == "dashboard" )
                {
                    bitdiceprofiledata tmp2 = json.JsonDeserialize<bitdiceprofiledata>(s);
                    this.bets = (int)tmp2.message.profile.bets;
                    this.profit = tmp2.message.profile.profit;
                    this.wagered = tmp2.message.profile.wagered;
                    Parent.updateBets(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                }
                else if (tmp.message.type == "dashboard_update")
                {
                    bitdiceprofiledata tmp2 = json.JsonDeserialize<bitdiceprofiledata>(s);
                    this.bets = (int)tmp2.message.update.bets;
                    this.profit = tmp2.message.update.profit;
                    this.wagered = tmp2.message.update.wagered;
                    Parent.updateBets(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                }
                switch (tmp.identifier.channel)
                {
                    case "DiceChannel": ProcessBitdiceBet(tmp); break;
                    default: break;
                }
            }
            catch (Exception er)
            {
                Parent.DumpLog(er.ToString(), 1);
            }

            


            
            //socketbase tmp = json.JsonDeserialize<socketbase>(e.Message);
            /*if (!string.IsNullOrEmpty(tmp.method))
            {
                switch (tmp.method)
                {
                    case "chat:new": Client_ChatReceived(json.JsonDeserialize<bitchatSocket>(e.Message.Replace("params", "_params"))._params); break;
                    case "stat.global": break;
                    case "stat.user": Client_UserStats(json.JsonDeserialize<bitstatsusersocket>(e.Message.Replace("params", "_params"))._params); break;
                    case "stat.bets": Client_BetResult(json.JsonDeserialize<bitstatsbetsocket>(e.Message.Replace("params", "_params").Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}"))._params); break;
                }
            }*/
        }
        public long server { get; set; }
        void ProcessBitdiceBet(bitdicebetbase Bet)
        {
            if (Bet.message.type=="secret")
            {
                server = Bet.message.secret.id;
            }

            Bet newbet = new Bet()
            {
                Id = Bet.message.data.bet.id,
                 Amount=decimal.Parse(Bet.message.data.bet.amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                  date=DateTime.Now,
                Chance = decimal.Parse(Bet.message.data.bet.chance, System.Globalization.NumberFormatInfo.InvariantInfo),
                Currency = Bet.message.data.bet.currency,
                  nonce=-1,
                Roll = decimal.Parse(Bet.message.data.bet.lucky, System.Globalization.NumberFormatInfo.InvariantInfo),
                Profit = decimal.Parse(Bet.message.data.bet.win, System.Globalization.NumberFormatInfo.InvariantInfo),
                high = Bet.message.data.bet.high,
                clientseed = Bet.message.data.old.client,
                serverhash = Bet.message.data.old.hash,
                 serverseed = Bet.message.data.old.secret

            };
            server = Bet.message.data.secret.id;
            balance = decimal.Parse(Bet.message.data.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
            wagered += newbet.Amount;
            profit += newbet.Profit;
            bets++;
            if (Bet.message.data.bet.result)
                wins++;
            else
                losses++;
            FinishedBet(newbet);

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
            Client.Send("{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"ProfileChannel\\\"}\"}");
            Client.Send("{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"WalletChannel\\\"}\"}");
            Client.Send("{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"EventsChannel\\\"}\"}");
            Client.Send("{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"DiceChannel\\\"}\"}");
            Client.Send("{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"ChatChannel\\\"}\"}");
            Client.Send("{\"command\":\"subscribe\",\"identifier\":\"{\\\"channel\\\":\\\"EndorphinaChannel\\\"}\"}");

            /*if (!loggedin)
            {
                loggedin = true;
                finishedlogin(true);
            }*/
        }

        

        
        string username = "seuntjie";
        

        int id = 1;
        Random R = new Random();
        //BitDiceClient Client = new BitDiceClient();
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            string clientsee = "";
            while (clientsee.Length<"3ebbd6d21ca843b6".Length-1)
            {
                clientsee += R.Next(0, 16 * 16).ToString("X");
            }
            //string server = "";
            string s = string.Format("{{\"command\":\"message\",\"identifier\":\"{{\\\"channel\\\":\\\"DiceChannel\\\"}}\",\"data\":\"{{\\\"amount\\\":{0},\\\"chance\\\":\\\"{1}\\\",\\\"type\\\":\\\"{2}\\\",\\\"client\\\":\\\"{4}\\\",\\\"server\\\":{5},\\\"hot_key\\\":false,\\\"manual\\\":true,\\\"number\\\":{3},\\\"action\\\":\\\"bet\\\"}}\"}}",
                amount, chance, High ? "high" : "low", id++, clientsee.ToLower(), server);
            Parent.DumpLog(s, 5);
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
        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            string s = string.Format("{{\"command\":\"message\",\"identifier\":\"{{\\\"channel\\\":\\\"WalletChannel\\\"}}\",\"data\":\"{{\\\"code\\\":\\\"\\\",\\\"amount\\\":\\\"{0}\\\",\\\"address\\\":\\\"{1}\\\",\\\"number\\\":{2},\\\"action\\\":\\\"withdraw\\\"}}\"}}", Amount, Address, id++);
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
               
                string s1 = "";
                    try
                    {
                        /*HttpResponseMessage resp = WebClient.GetAsync("").Result;
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
                                }*/
                                /*if (!Cloudflare.doCFThing(s1, WebClient, ClientHandlr, 0, "www.bitdice.me"))
                                {
                                    finishedlogin(false);
                                    return;
                                }*/
                        
                            /*}
                        }*/
                        string a = json.JsonSerializer<bitdicedatainfo>(new bitdicedatainfo());

                        //encode
                        //a = System.Web.HttpUtility.HtmlEncode(a);
                        //a = a.Replace("+", "%20");
                        //unescape
                        a = System.Web.HttpUtility.UrlDecode(a);

                        //base 64 encode
                        /*a = EncodeTo64(a);
                            user[username]:etaeasdf
user[two_fa]:
user[password]:asdfasdfasdf*/
                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        if (Username.Contains("@") && Username.Contains("."))
                        {
                            
                            pairs.Add(new KeyValuePair<string, string>("user[email]", Username));
                        }
                        else
                        {
                            pairs.Add(new KeyValuePair<string, string>("user[username]", a));
                        }
                        pairs.Add(new KeyValuePair<string, string>("data[info]", a));
                        pairs.Add(new KeyValuePair<string, string>("user[password]", Password/*==""?"undefined":twofa*/));
                        pairs.Add(new KeyValuePair<string, string>("user[two_fa]", twofa/*==""?"undefined":twofa*/));
                        
                        
                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        string sEmitResponse = WebClient.PostAsync("api/sign_in", Content).Result.Content.ReadAsStringAsync().Result;
                        bitdicelogin login = json.JsonDeserialize<bitdicelogin>(sEmitResponse);
                        if (login.status)
                            stream = login.token;
                        else
                        {
                            finishedlogin(false);
                            return;
                        }

                    }
                catch
                    {
                        finishedlogin(false);
                        return;
                    }
                    Cookie c = new Cookie();
              
                username = Username;
               
                //
                
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> cookies2 = new List<KeyValuePair<string, string>>();
                foreach ( Cookie x in ClientHandlr.CookieContainer.GetCookies(new Uri("https://www.bitdice.me")))
                {
                    cookies2.Add(new KeyValuePair<string, string>(x.Name, x.Value));
                }
                
                headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0"));

                Client = new WebSocket("wss://www.bitdice.me/socket/?token=" + stream, "actioncable-v1-json", cookies2, headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0",
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
                //System.Windows.Forms.MessageBox.Show("Due to current limitations of the API, I can't show you your stats until you place a valid bet. Sorry.\n\nAlso, you will need to reselect your currency. If you already selected the currency you want to play in, please select another first, and then switch back.", "Stats Errors", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (WebException e)
            {
                if (e.Response!=null)
                {
                    string s = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    finishedlogin(false);
                    return;
                }
            }
            catch
            {
                finishedlogin(false);
                return;
            }
        }

        

        public override bool Register(string Username, string Password)
        {
            throw new NotImplementedException();
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
           
        }

        public override bool Invest(decimal Amount)
        {
            throw new NotImplementedException();
        }
        public override void Donate(decimal Amount)
        {
            SendTip("1426", Amount);
        }
        public override bool InternalSendTip(string User, decimal amount)
        {
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(string.Format("{{\"command\":\"message\",\"identifier\":\"{{\\\"channel\\\":\\\"ChatChannel\\\"}}\",\"data\":\"{{\\\"message\\\":\\\"/tip {0} {1}\\\",\\\"action\\\":\\\"chat\\\"}}\"}}", User, amount));
            }
            //SendChatMessage(string.Format("/tip {0} {1:0.00000000}", User, amount));
            return true;
        }
        string cookie = "";
        string stream = "";
        string csrf = "";
        protected override void CurrencyChanged()
        {
           if (Client!=null)
           {
               if (Client.State == WebSocketState.Open)
               {
                   string s = "{\"command\":\"message\",\"identifier\":\"{\\\"channel\\\":\\\"WalletChannel\\\"}\",\"data\":\"{\\\"currency\\\":\\\"" + Currency + "\\\",\\\"action\\\":\\\"currency\\\"}\"}";
                   Parent.DumpLog(s, 5);
                   Client.Send(s);
               }
           }
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
        public static bool Enable(string token)
        {
            //data[info=eyJsYW5nIjoiZW4tVVMsIGVuIiwicGxhdGZvcm0iOiJXaW4zMiIsImNwdSI6OCwic2l6ZSI6IjE5MjB4MzY0MyAoMTkyMHgxMDgwKSIsIndlYnJ0YyI6IjE2OS4yNTQuODAuODAsIDE5Mi4xNjguMS4zIiwidGltZXpvbmUiOiJBZnJpY2EvSm9oYW5uZXNidXJnIiwidGltZSI6IlNhdCBEZWMgMDMgMjAxNiAxMDoxMTo0MyBHTVQrMDIwMCAoU291dGggQWZyaWNhIFN0YW5kYXJkIFRpbWUpIn0=
            //token=017f8216daa8349d55170806c0e02cef66252acc082616dc7be742e6c9b5081d
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
       | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;
                CookieContainer cookies = new CookieContainer();
                {
                    string Token = "";
                    HttpWebRequest tmp = (HttpWebRequest)HttpWebRequest.Create(token);
                    tmp.AllowAutoRedirect = true;
                    HttpWebResponse myResp = (HttpWebResponse)tmp.GetResponse();
                    //if (myResp.StatusCode == HttpStatusCode.Redirect)
                    {
                        Token = myResp.ResponseUri.Segments[myResp.ResponseUri.Segments.Length-1];
                        if (Token.Contains("?"))
                        {
                            Token = Token.Substring(Token.IndexOf("?")+1);
                        }
                        if (Token.Contains("/"))
                        {
                            Token = Token.Substring(Token.IndexOf("/") + 1);
                        }
                    }

                    using (HttpClientHandler ClientHandlr = new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = cookies,
                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                        /*Proxy = (IWebProxy)this.Prox,
                        UseProxy = this.Prox != null*/
                    })
                    {
                        using (HttpClient WebClient = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitdice.me/") })
                        {
                            WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                            WebClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                            WebClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");
                            string a = json.JsonSerializer<bitdicedatainfo>(new bitdicedatainfo());

                            //encode
                            //a = System.Web.HttpUtility.HtmlEncode(a);
                            //a = a.Replace("+", "%20");
                            //unescape
                            a = System.Web.HttpUtility.UrlDecode(a);

                            //base 64 encode
                            a = EncodeTo64(a);
                            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("data[info]", a));
                            pairs.Add(new KeyValuePair<string, string>("token", Token));
                            //data[info]
                            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                            /*if (WebClient.DefaultRequestHeaders.Contains("X-CSRF-Token"))
                            {
                                WebClient.DefaultRequestHeaders.Remove("X-CSRF-Token");
                            }
                            WebClient.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
                            */


                            string sEmitResponse = WebClient.PostAsync("api/device_confirm", Content).Result.Content.ReadAsStringAsync().Result;
                            return sEmitResponse.Contains("\"status\":true");
                            
                        }
                    }
                }
            }
            catch{};
            return false;
        }

    }
    public class bitdicelogin
    {
        public bool status { get; set; }
        public string token { get; set; }
    }
    public class bitdicebet
    {
        public long id { get; set; }
        public long date { get; set; }
        public string amount { get; set; }
        public string chance { get; set; }
        public bool high { get; set; }
        public string lucky { get; set; }
        public bool result { get; set; }
        public string win { get; set; }
        public string target { get; set; }
        public double multiplier { get; set; }
        public string currency { get; set; }
        public string wagered { get; set; }
        public bitdiceuser user { get; set; }
        public string game { get; set; }
        
    }
    public class bitdicebetdata
    {
        public bitdicebet bet { get; set; }
        public biddiceold old { get; set; }
        public bitdicenew secret { get; set; }
        public string balance { get; set; }
    }
    public class biddiceold
    {
        public string secret { get; set; }
        public string client { get; set; }
        public string hash { get; set; }
    }
    public class bitdicenew
    {
        public long id { get; set; }
        public string hash { get; set; }
    }
    public class bitdicebetmessage
    {
        public bitdicebetdata data { get; set; }
        public string type { get; set; }
        public bitdicenew secret { get; set; }
    }
    public class bitdicebetbase
    {
        public bitdciceBetIdentifier identifier { get; set; }
        public bitdicebetmessage message { get; set; }
        public string type { get; set; }
    }
   public class bitdciceBetIdentifier
   {
       public string channel { get; set; }
   }
    public class bitdiceuser
    {
        public string username { get; set; }
        public double level { get; set; }
        public string balance { get; set; }
        
    }

    public class bitdicebalancedata
    {
        public bitdicebalances wallets { get; set; }
        public bitdicebalancedata data { get; set; }
        public string type0 { get; set; }
        public bitdicebalancedata message { get; set; }
        public bitdciceBetIdentifier identifier { get; set; }
        public string active { get; set; }
    }

    public class bitdicebalances
    {
        public bitdicewallet btc { get; set; }
        public bitdicewallet ltc { get; set; }
        public bitdicewallet doge { get; set; }
        public bitdicewallet eth { get; set; }
    }
    public class bitdicewallet
    {
        public string balance { get; set; }
        public string address { get; set; }
    }
    //{"identifier":"{\"channel\":\"ProfileChannel\"}","type":"confirm_subscription"}
    /*{"identifier":"{\"channel\":\"ProfileChannel\"}",
    "message":{"type":"dashboard",
    "profile":{"bets":30,"wagered":0.0038544802485028,"profit":2.8000000000000003e-06,"level":0,"progress":91.57,"token_balance":"0.004661955","switch_cap":false}
}}*/
    public class bitdiceprofiledata
    {
        public bitdiceprofile profile { get; set; }
        public bitdiceprofiledata message { get; set; }
        public bitdciceBetIdentifier identifier { get; set; }
        public string type { get; set; }
        public bitdiceprofile update { get; set; }
    }

    public class bitdiceprofile
    {
        public long bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public double level { get; set; }
        public double progress { get; set; }
        public string token_balance { get; set; }
        public bool switch_cap { get; set; }
    }
    public class bitdicedatainfo
    {
        public string lang { get; set; }
        public string platform { get; set; }
        public int cpu { get; set; }
        public string size { get; set; }
        public string webrtc { get; set; }
        public string timezone { get; set; }
        public string time { get; set; }

        public bitdicedatainfo()
        {
            lang = "en-US, en";
            platform = "Win32";
            webrtc = GetLocalIPAddress();
            timezone = TimeZoneInfo.Local.DisplayName;
            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss \"GMT\"zzz") + " (" + TimeZone.CurrentTimeZone.StandardName+")"; 
            cpu = Environment.ProcessorCount;
            string sz = System.Windows.SystemParameters.VirtualScreenWidth + "x" + System.Windows.SystemParameters.VirtualScreenHeight;
            /*System.Windows.SystemParameters.FullPrimaryScreenHeight
            System.Windows.SystemParameters.FullPrimaryScreenWidth*/
                size = sz+=" ("+System.Windows.SystemParameters.PrimaryScreenWidth+"x"+
                    System.Windows.SystemParameters.PrimaryScreenHeight+")";

        }

        public static string GetLocalIPAddress()
        {
            string s = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (s != "")
                        s += ", ";
                    s+= ip.ToString();
                }
            }
            return s;
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
