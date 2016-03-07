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
namespace DiceBot
{
    class FortuneJack:DiceSite
    {
        public FortuneJack(cDiceBot Parent)
        {
            this.Parent = Parent;
            new Thread(new ThreadStart(KeepAliveThread)).Start();
        }
        bool IsFJ = true;
        bool IsLoggedIn = false;

        protected override void internalPlaceBet(bool High)
        {
            //64, 0.000000100 ,4950,0,1988923078

            /*string s = "64,0.000000100,4950,0,33405318";
            string msg = string.Format("61,{0},1,{1}", 1061249, ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com"))["PHPSESSID"].Value);
            List<byte> bytes = new List<byte>();
            foreach (char c in msg)
            {
                bytes.Add((byte)c);
            }
            //Client.Send(67);
            */
            Client.Send("64,0.000000100,4950,0,1988923088\r\n");

        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
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
                Task.Delay(1000);
            }
        }
        Random R = new Random();
        void KeepAlive()
        {
            try
            {
                WebClient.GetStringAsync("api/main/keepAlive.php?rnd="+R.Next(0, int.MaxValue));
            }
            catch (AggregateException e)
            {

            }
            catch
            { }
        }
        protected override bool internalWithdraw(double Amount, string Address)
        {
            throw new NotImplementedException();
        }
        HttpClient WebClient = null;
        HttpClientHandler ClientHandlr = null;
        CookieContainer cookies = null;
        WebSocket Client = null;
        public override void Login(string Username, string Password, string twofa)
        {
            
            try
            {
                /*ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
       | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;*/
                cookies = new CookieContainer();
                ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
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
                pairs = new List<KeyValuePair<string, string>>();
                Content = new FormUrlEncodedContent(pairs);
                KeepAlive();
                sEmitResponse = WebClient.PostAsync("ajax/time.php", Content).Result.Content.ReadAsStringAsync().Result;
                sEmitResponse = WebClient.GetStringAsync("api/dice4/diceutils.php?act=rooms").Result;
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
                            cookies2.Add(new KeyValuePair<string, string>(curCookie.Name, curCookie.Value));
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
                /*while (Client.State == WebSocketState.Connecting)
                {
                    Task.Delay(100);
                }*/
                finishedlogin(true);
                IsLoggedIn = true;
                //Client.Send("67,2");
            }
            catch (AggregateException e)
            {

            }
        }

        void Client_DataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }
        
        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.StartsWith("63"))
            {
                Client.Send("67,2\r\n");
            }
            if (e.Message.StartsWith("65"))
            {

            }
            if (e.Message.StartsWith("90"))
            {

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
            //Client.Send("3probe");
            //61,1020272,1,f3fn2alhsjofcmcrqg28adq8p5
            //1061249
            string msg = string.Format("61,{0},1,{1}", 1020272, ClientHandlr.CookieContainer.GetCookies(new Uri("https://fortunejack.com"))["PHPSESSID"].Value + "\r\n");
            List<byte> bytes = new List<byte>();
            bytes.Add(Convert.ToByte(61));
            foreach ( char c in msg )
            {
                bytes.Add((byte)c);
            }
            byte[] tmpbytes = Encoding.UTF8.GetBytes(msg);
            //Client.Send(bytes.ToArray(), 0, tmpbytes.Length);
            //Task.Delay(1000);
            Client.Send(msg);
            //Task.Delay(1000);
            //Client.Send("67,2");
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
    }
}
