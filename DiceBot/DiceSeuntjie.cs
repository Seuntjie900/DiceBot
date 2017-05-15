using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace DiceBot
{
    class DiceSeuntjie:DiceSite
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
        RandomNumberGenerator R = new System.Security.Cryptography.RNGCryptoServiceProvider();
       
        public DiceSeuntjie(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://www.moneypot.com/bets/";
            edge = 0.9m;
            this.Parent = Parent;
            Name = "DiceSeuntjie";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://dice.seuntjie.com/?ref=seuntjie";
            APPId = 2668;
            url = "dice.seuntjie.com";
            _PasswordText = "Api Key";
        }
        protected string url { get; set; }
        long id = 0;
        string OldHash = "";
        string ClientSeed = "";
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            //42207["dice_bet",{"wager":100,"client_seed":1575385442,"hash":"7cb5599644e30201d7ed12a3dce401048bbbd8718fcb0cffbba38b8da8278808","cond":">","target":50.4999,"payout":200}]
            //429["dice_bet",{"wager":100,"client_seed":3823035792,"hash":"60a4fb16aa9aa7020d6adc397730952d102adde98428007701836dcb5e4c48eb","cond":">","target":50.4499,"payout":200}]
            this.High = High;
            ClientSeed = "";
            byte[] bytes = new byte[4];
            R.GetBytes(bytes);
            long client = (long)BitConverter.ToUInt32(bytes, 0);
            ClientSeed = client.ToString();
            long Roundedamount = (long)(Math.Round((amount), 8) * 100000000);
            string Bet = string.Format("42{0}[\"dice_bet\",{{\"wager\":{1:0},\"client_seed\":{2},\"hash\":\"{3}\",\"cond\":\"{4}\",\"target\":{5:0.0000},\"payout\":{6:0.0000}}}]",
                id++, Roundedamount, ClientSeed, OldHash, High ? ">" : "<", High ? 100m - chance : chance, (((100m - edge) / chance) * Roundedamount));
            WSClient.Send(Bet);
        }

        public override void ResetSeed()
        {
            
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            return false;
        }

        public override void Login(string Username, string Password, string twofa)
        {
            //https://bit-exo.com/
            CookieContainer cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies,AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://"+url+"/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

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
                foreach (Cookie c3 in cookies.GetCookies(new Uri("http://seuntjie.com")))
                {
                    if (c3.Name == "io")
                        c = c3.Value;
                    /*if (c3.Name == "__cfduid")
                        c2 = c3;*/
                }
                response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c).Result;
                //response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c).Result;
                string chatinit = "420[\"chat_init\",{\"app_id\":2668,\"access_token\":\"" + accesstoken + "\",\"subscriptions\":[\"CHAT\",\"DEPOSITS\",\"BETS\"]}]";
                chatinit = chatinit.Length + ":" + chatinit;
                var content = new StringContent(chatinit, Encoding.UTF8, "application/octet-stream");
                response = Client.PostAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c, content).Result.Content.ReadAsStringAsync().Result;
                //response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c).Result;
                
                List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
                Headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"));
                foreach (Cookie x in cookies.GetCookies(new Uri("https://" + url)))
                {
                    Cookies.Add(new KeyValuePair<string,string>(x.Name, x.Value));
                }
                Cookies.Add(new KeyValuePair<string,string>("io",c));
                WSClient = new WebSocket("wss://"+url+"/socket.io/?EIO=3&transport=websocket&sid=" + c, null, Cookies, Headers, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36", "https://bit-exo.com", WebSocketVersion.Rfc6455, null, System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);
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

        public override void Donate(decimal Amount)
        {
            InternalSendTip("seuntjie", Amount);
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            //"4210["send_tip",{"uname":"seuntjiebot","amount":100000,"private":false,"type":"BTC"}]";
            WSClient.Send(string.Format("42{0}[\"send_tip\",{{\"uname\":\"{1}\",\"amount\":{2:0.00000000},\"private\":false,\"type\":\"BTC\"}}]", id++, User, amount*100000000m));
            return true;
        }

        void GetBalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now-lastupdate).TotalSeconds>=30)
                {
                    lastupdate = DateTime.Now;
                    string getbalance = string.Format("42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\"}}]", id++, APPId, accesstoken);
                    WSClient.Send(getbalance);
                    WSClient.Send("2");
                
                }
                Thread.Sleep(500);
            }
        }
        void WSClient_Opened(object sender, EventArgs e)
        {
            WSClient.Send("2probe");
            Parent.DumpLog("opened", -1);
        }
        public int APPId { get; set; }
        void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Parent.DumpLog(e.Message, 10);
            if (e.Message=="3probe")
            {
                WSClient.Send("5");
                //4214["access_token_data",{"app_id":2668,"access_token":"49af40dd-5f96-4c67-ab16-6ff05f0d364b"}]
                //20:421["get_hash",null]
                WSClient.Send("42"+id++ +"[\"get_hash\",null]");
                string getbalance = string.Format("42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\"}}]", id++, APPId, accesstoken );
                WSClient.Send(getbalance);
                //148:420["chat_init",{"app_id":926,"access_token":"5ed59498-d425-4c63-835e-348f56753d5a","subscriptions":["CHAT","DEPOSITS","BETS"],"room":"ENGLISH_RM"}]
                               
            }
            else
            {
                if (e.Message.Contains("[null,{\"hash\":"))
                {
                    //431[null,{"hash":"60a4fb16aa9aa7020d6adc397730952d102adde98428007701836dcb5e4c48eb","bet_hash":"60a4fb16aa9aa7020d6adc397730952d102adde98428007701836dcb5e4c48eb"}] 
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDiceHash tmphash = json.JsonDeserialize<SDiceHash>(msg);
                    this.OldHash = tmphash.bet_hash;
                }
                if (e.Message.Contains("[null,{\"token\":"))
                {
                    // get balance and stats
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDIceAccToken tmphash = json.JsonDeserialize<SDIceAccToken>(msg);
                    this.balance = tmphash.auth.user.balance / 100000000m;
                    this.profit = tmphash.auth.user.betted_profit / 100000000m;
                    this.wagered = tmphash.auth.user.betted_wagered / 100000000m;
                    this.bets = (int)tmphash.auth.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    Parent.updateBets(bets);
                }
                if (e.Message.Contains("[null,{\"auth\":"))
                {
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    sdiceauth tmphash = json.JsonDeserialize<sdiceauth>(msg).auth;
                    this.balance = tmphash.user.balance / 100000000m;
                    this.profit = tmphash.user.betted_profit / 100000000m;
                    this.wagered = tmphash.user.betted_wagered / 100000000m;
                    this.bets = (int)tmphash.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    Parent.updateBets(bets);
                }
                if (e.Message.Contains("[null,{\"id\":"))
                {
                    //do bet
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDIceBet tmphash = json.JsonDeserialize<SDIceBet>(msg);
                    Bet newbet = new Bet
                    {
                         Amount=amount,
                          date=DateTime.Now,
                           Chance=chance,
                            high=High,
                             clientseed=ClientSeed,
                         Id = tmphash.bet_id.ToString(),
                               nonce=tmphash.secret,
                                serverseed=tmphash.salt,
                                 serverhash=OldHash,
                                  Profit=tmphash.profit/100000000m,
                                   Roll=decimal.Parse(tmphash.outcome, System.Globalization.NumberFormatInfo.InvariantInfo)
                    };
                    OldHash = tmphash.next_hash;
                    balance += newbet.Profit;
                    profit += newbet.Profit;
                    bets++;
                    wagered += newbet.Amount;
                    bool win = false;
                    if ((newbet.Roll > maxRoll- newbet.Chance && High) || (newbet.Roll < newbet.Chance && !High))
                    {
                        win = true;
                    }
                    if (win)
                        wins++;
                    else
                        losses++;
                    FinishedBet(newbet);
                }
            }
        }

        void WSClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Parent.DumpLog("error", -1);
            Parent.DumpLog(e.Exception.ToString(), -1);
        }

        void WSClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }

        void WSClient_Closed(object sender, EventArgs e)
        {
            Parent.DumpLog("closed", -1);
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
            return true;
        }

        public override void Disconnect()
        {
            ispd = false;
            try
            {
                WSClient.Close();
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

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return moneypot.sGetLucky(server, client, nonce);
        }

        public static decimal sGetLucky(string server, string client, int nonce)
        {
            return moneypot.sGetLucky(server, client, nonce);
        }
    }
    //439[null,{"id":995652266,"bet_id":995652266,"outcome":"31.6029","profit":-100,"secret":1829266922,"salt":"1d6f5decb3098d00e065faa720df7d16","created_at":"2017-03-11T08:13:03.312Z",
    //"next_hash":"2b170d723dd272a1d1bc933589b4221a4426602245c2b4cf168568573f653a16","raw_outcome":1357335418,"kind":"DICE"}]
    public class SDIceBet
    {
        public long ID { get; set; }
        public long bet_id { get; set; }
        public string outcome { get; set; }
        public decimal profit { get; set; }
        public long secret { get; set; }
        public string salt { get; set; }
        public string created_at { get; set; }
        public string next_hash { get; set; }
        public long raw_outcome { get; set; }
        public string kind { get; set; }
    }
    /*4318[null,{"token":"49af40dd-5f96-4c67-ab16-6ff05f0d364b","expires_in":1086586,"expires_at":"2017-03-23T22:04:27.624798+00:00",
     * "kind":"confidential_token","auth":
     * {"id":57783,"app_id":2668,"user":
     * {"uname":"seuntjie","balance":278685.750269145,"unconfirmed_balance":0,
     * "unpaid":0,"betted_count":70959,"betted_wager":22375223.7993277,"betted_ev":-201880.321227329,"betted_profit":-321216.249730916,"role":"OWNER",
     * "wager24hour":200,"profit24hour":0,
     * "largestwin":{"id":984302700,"amt":205213.73737373733,"game":"DICE"}
     * ,"largestloss":{"id":984396746,"amt":-409600,"game":"DICE"},
     * "ref":null,"refprofit":0,"refpaid":0,"open_pm":["NONE"],"level":3}
     * }}]*/
    public class SDIceAccToken
    {
        public SDIceAccToken auth { get; set; }
        public SDiceUser user { get; set; }
    }
    
    public class SDiceUser
    {
        public string uname { get; set; }
        public decimal balance { get; set; }
        public decimal unconfirmed_Balance { get; set; }
        public long betted_count { get; set; }
        public decimal betted_wagered { get; set; }
        public decimal betted_profit { get; set; }
    }
    //431[null,{"hash":"60a4fb16aa9aa7020d6adc397730952d102adde98428007701836dcb5e4c48eb","bet_hash":"60a4fb16aa9aa7020d6adc397730952d102adde98428007701836dcb5e4c48eb"}] 
    public class SDiceHash
    {
        public string hash { get; set; }
        public string bet_hash { get; set; }
    }

    public class sdiceauth
    {
        public sdiceauth auth {get; set;}
        public int id { get; set; }
        public int auth_id { get; set; }
        public int app_id { get; set; }
        public SDiceUser user { get; set; }
        /*435[null,{ "auth":{ 
            "id":9642,
            "auth_id":9642,
            "app_id":926,
            "user":
            { "uname":"seuntjie",
                "balance":1266910.52218771,
            "unconfirmed_balance":"0","unpaid":"0","betted_count":1511,"betted_wager":19320,"betted_ev":-196.23015987452,"betted_profit":-1650.6125,"role":"member","wager24hour":0,"profit24hour":0,"largestwin":{ "id":995770044,"amt":641.2928999999999,"game":"DICE"},"largestloss":{ "id":995770162,"amt":-640,"game":"DICE"},"ref":null,"refprofit":1146003.082047149,"refpaid":1059050,"open_pm":["NONE"],"level":0}}}]*/
    }
}
