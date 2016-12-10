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
    class NitrogenSports:DiceSite
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
            this.AutoWithdraw = false;
            this.BetURL = "";
            this.ChangeSeed = false;
            this.edge = 1;
            this.maxRoll = 99.99m;
            this.Name = "Nitrogen Sports";
            this.NonceBased = true;
            this.register = false;
            this.SiteURL = "";
            this.Tip = false;
            this.TipUsingName = false;
            
        }

        void GetBalanceThread()
        {
            while (iskd)
            {
                try
                {
                    if ((DateTime.Now-lastupdate).TotalSeconds>10)
                    {
                        GetStats();
                    }
                }
                catch { }
            }
        }

        void GetStats()
        {

        }

        void ConnectSocket()
        {
            try
            {
                if (NSSocket!=null)
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
                    foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://nitrogensports.eu")))
                    {
                        switch (c.Name)
                        {
                            case "PHPSESSID": PHPID = c.Value; break;
                            case "__cfduid": cfuid = c.Value; break;
                            case "cf_clearance": cfclearnace = c.Value; break;
                            case "login_link": link = c.Value; break;
                            case "x-csrftoken": token=c.Value;break;

                            default: break;
                        }
                    }
                    List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                    Cookies.Add(new KeyValuePair<string, string>("PHPSESSID", PHPID));
                    Cookies.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                    Cookies.Add(new KeyValuePair<string, string>("cf_clearance", cfclearnace));
                    Cookies.Add(new KeyValuePair<string, string>("login_link", link));
                    Cookies.Add(new KeyValuePair<string, string>("csrftoken", token));

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

                    NSSocket = new WebSocket("wss://nitrogensports.eu/ws/", "wamp", Cookies, headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0", "https://nitrogesports.eu", WebSocketVersion.Rfc6455, null);

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

        void NSSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Parent.DumpLog(e.Message,-1);
            //throw new NotImplementedException();
        }

        void NSSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Parent.DumpLog(e.Exception.ToString(), -1);
            //throw new NotImplementedException();
        }

        void NSSocket_Closed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            throw new NotImplementedException();
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
                            System.Windows.Forms.MessageBox.Show("fortunejack.com has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
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
                if (tmpLogin.errno!=0)
                {
                    finishedlogin(false);
                    return;

                }
                else
                {
                    this.balance = decimal.Parse(tmpLogin.balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                    token = tmpLogin.csrf_token;
                    ConnectSocket();
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
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
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
}
