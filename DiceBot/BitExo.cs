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
    class BitExo:DiceSite
    {

         string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        WebSocket WSClient;// = new WebSocket("");
       
        public BitExo(cDiceBot Parent)
        {
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://bit-exo.com/?ref=seuntjie";
            
            this.Parent = Parent;
            Name = "Bit-Exo";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://bit-exo.com/?ref=seuntjie ";
            
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            //42207["dice_bet",{"wager":100,"client_seed":1575385442,"hash":"7cb5599644e30201d7ed12a3dce401048bbbd8718fcb0cffbba38b8da8278808","cond":">","target":50.4999,"payout":200}]
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
            //https://bit-exo.com/
            CookieContainer cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies,AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://bit-exo.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0");

            try
            {
                accesstoken = Password;
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
                            System.Windows.Forms.MessageBox.Show("bit-exo has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "bit-exo.com"))
                        {

                            finishedlogin(false);
                            return;
                        }

                    }
                }
                string response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t="+CurrentDate()).Result;
                
                string c = 
                response.Substring(response.IndexOf("sid\":\"") + "sid\":\"".Length);
                c = c.Substring(0, c.IndexOf("\""));
                foreach (Cookie c3 in cookies.GetCookies(new Uri("http://bit-exo.com")))
                {
                    if (c3.Name == "io")
                        c = c3.Value;
                    /*if (c3.Name == "__cfduid")
                        c2 = c3;*/
                }
                response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c).Result;
                //response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c).Result;
                string chatinit = "420[\"chat_init\",{\"app_id\":926,\"access_token\":\"" + accesstoken + "\",\"subscriptions\":[\"CHAT\",\"DEPOSITS\",\"BETS\"],\"room\":\"ENGLISH_RM\"}]";
                chatinit = chatinit.Length + ":" + chatinit;
                var content = new StringContent(chatinit, Encoding.UTF8, "application/octet-stream");
                response = Client.PostAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c, content).Result.Content.ReadAsStringAsync().Result;
                response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c).Result;
                
                List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
                foreach (Cookie x in cookies.GetCookies(new Uri("http://bit-exo.com")))
                {
                    Cookies.Add(new KeyValuePair<string,string>(x.Name, x.Value));
                }
                //Cookies.Add(new KeyValuePair<string,string>("io",c));
                WSClient = new WebSocket("wss://bit-exo.com/socket.io/?EIO=3&transport=websocket&sid=" + c, null, Cookies, Headers, "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0", "https://bit-exo.com", WebSocketVersion.Rfc6455, null, System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);
                WSClient.Closed += WSClient_Closed;
                WSClient.DataReceived += WSClient_DataReceived;
                WSClient.Error += WSClient_Error;
                WSClient.MessageReceived += WSClient_MessageReceived;
                WSClient.Opened += WSClient_Opened;
                /*WSClient.AutoSendPingInterval = 30;
                WSClient.EnableAutoSendPing = true;*/
                WSClient.Open();
                while (WSClient.State == WebSocketState.Connecting)
                    Thread.Sleep(100);
                if (WSClient.State == WebSocketState.Open)
                {
                    ispd = true;
                    
                    lastupdate = DateTime.Now;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    finishedlogin(true); return;
                }
                else
                {
                    finishedlogin(false);
                    return;
                }
            }
            catch (AggregateException ER)
            {
                finishedlogin(false);
                return;
            }
            catch (Exception ERR)
            {
                finishedlogin(false);
                return;
            }
            finishedlogin(false);
            return;
        }
        void GetBalanceThread()
        {

        }
        void WSClient_Opened(object sender, EventArgs e)
        {
            WSClient.Send("2probe");
        }
        
        void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Parent.DumpLog(e.Message, -1);
            if (e.Message=="3probe")
            {
                WSClient.Send("5");
                //148:420["chat_init",{"app_id":926,"access_token":"5ed59498-d425-4c63-835e-348f56753d5a","subscriptions":["CHAT","DEPOSITS","BETS"],"room":"ENGLISH_RM"}]
                
            }
            else
            {

            }
        }

        void WSClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            
        }

        void WSClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }

        void WSClient_Closed(object sender, EventArgs e)
        {
            
        }
        public static string CurrentDate()
        {
            TimeSpan dt = DateTime.UtcNow - DateTime.Parse("1970/01/01 00:00:00", System.Globalization.CultureInfo.InvariantCulture);
            double mili = dt.TotalMilliseconds;
            return ((long)mili).ToString();

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
}
