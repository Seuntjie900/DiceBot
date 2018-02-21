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
    class BetKing : DiceSite
    {
        WebSocket Sock;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        CookieContainer cookies = new CookieContainer();
        string clientseed = "";
        Random R = new Random();
        public static string[] sCurrencies = new string[] { "Btc", "Eth", "BKB", "Ltc","OmiseGo",
"TRON",
"EOS",
"Status",
"Populous",
"Golem",
"Augur",
"Veritaseum",
"SALT",
"Basic Attention Token",
"FunFair",
"Power Ledger",
"TenX",
"0x",
"CIVIC" };
        Dictionary<string, int> Curs = new Dictionary<string, int>();
        public BetKing(cDiceBot Parent)
        {
            this.Parent = Parent;
            _UsernameText = "Username/Email:";
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://betking.io/bets/";

            this.Parent = Parent;
            Name = "BetKing";
            Tip = false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://betking.io?ref=u:seuntjie";

            Currencies = sCurrencies;
            Currency = "Btc";
            Curs.Add("Btc", 0);
            Curs.Add("Eth", 1);
            Curs.Add("Ltc", 3);
            Curs.Add("BKB", 6);
            Curs.Add("OmiseGo", 7);
            Curs.Add("TRON", 8);
            Curs.Add("EOS", 9);
            Curs.Add("Status", 11);
            Curs.Add("Populous", 12);
            Curs.Add("Golem", 13);
            Curs.Add("Augur", 15);
            Curs.Add("Veritaseum", 16);
            Curs.Add("SALT", 17);
            Curs.Add("Basic Attention Token", 18);
            Curs.Add("FunFair", 19);
            Curs.Add("Power Ledger", 21);
            Curs.Add("TenX", 24);
            Curs.Add("0x", 25);
            Curs.Add("CIVIC",28);
        }
        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }

        public override void Disconnect()
        {
            ispd = false;
            if (Sock != null)
            {
                if (Sock.State== WebSocketState.Open)
                
                    try
                    {
                        Sock.Close();
                    }
                    catch
                    { }
            }
            
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        int nonce = 0;
        string serverseedhash = "";

        void GetBlanaceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now- lastupdate).TotalSeconds>25 || ForceUpdateStats)
                {
                    ForceUpdateStats = false;
                    lastupdate = DateTime.Now;
                    GetBalance();
                    GetStats();
                    Sock.Send("2");
                }
                Thread.Sleep(1000);
            }
        }

        void GetBalance()
        {
            if (clientseed == null)
                clientseed = R.Next(0, int.MaxValue).ToString();
            HttpResponseMessage Msg = Client.GetAsync(string.Format("https://api.betking.io/api/app/loadstate?currency={0}&appId={1}&clientSeed=FV5CCoio666H6XDmz0o", Curs[Currency], 0, clientseed)).Result;
            if (Msg.IsSuccessStatusCode)
            {
                string Response = Msg.Content.ReadAsStringAsync().Result;
                BKGetBalance tmp = json.JsonDeserialize<BKGetBalance>(Response);
                this.balance = tmp.Balance;
                 this.clientseed = tmp.ClientSeed;
                nonce = tmp.Nonce;
                serverseedhash = tmp.ServerSeedHash;
                this.username = tmp.UserName;
                Parent.updateBalance(balance);
                Parent.updateProfit(profit);
                Parent.updateBets(bets);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
            }
        }

        
        
        void GetStats()
        {
            HttpResponseMessage Msg = Client.GetAsync(string.Format("https://api.betking.io/api/stats/getuserstats?appId={0}&userName={1}",  0, username)).Result;
            if (Msg.IsSuccessStatusCode)
            {
                string Response = Msg.Content.ReadAsStringAsync().Result;
                
                BKGetStats tmp = json.JsonDeserialize<BKGetStats>(Response);
                foreach (BKStat x in tmp.stats)
                {
                    if (x.currency == Curs[Currency])
                    {
                        this.profit = x.profit;
                        this.bets = x.numBets;
                        this.wagered =x.wagered;
                        

                    }
                }
                Parent.updateProfit(profit);
                Parent.updateBets(bets);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
            }
        }

        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, AllowAutoRedirect=false };
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://betking.io/") };
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                cookies = new CookieContainer();
                ClientHandlr.CookieContainer = cookies;
                Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36");
                
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
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Windows.Forms.MessageBox.Show("betking.io has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        if (!Cloudflare.doCFThing(s1, Client, ClientHandlr, 0, "betking.io"))
                        {
                            finishedlogin(false);
                            return;
                        }
                    }
                    else
                    {

                    }
                }
                string LoginPage = Client.GetStringAsync("").Result;
                LoginPage = Client.GetStringAsync("bet/login").Result;
                LoginPage = LoginPage.Substring(LoginPage.IndexOf("<input type=\"hidden\" name=\"_csrf\" value=\"") + "<input type=\"hidden\" name=\"_csrf\" value=\"".Length);
                string csrf = LoginPage.Substring(0, LoginPage.IndexOf("\""));
                

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("_csrf", csrf));
                pairs.Add(new KeyValuePair<string, string>("client_id", "0"));
                pairs.Add(new KeyValuePair<string, string>("fingerprint", ""));
                pairs.Add(new KeyValuePair<string, string>("loginvia", Username.Contains("@")?"email": "username"));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                pairs.Add(new KeyValuePair<string, string>("redirect_uri", "https://betking.io/bet"));
                pairs.Add(new KeyValuePair<string, string>("twoFactorCode", twofa));
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                HttpResponseMessage RespMsg = Client.PostAsync("bet/login", Content).Result;
                string responseUri = RespMsg.RequestMessage.RequestUri.ToString();
                string sEmitResponse = RespMsg.Content.ReadAsStringAsync().Result;
                
                if (!sEmitResponse.ToLower().Contains("error"))
                {
                    
                    RespMsg = Client.GetAsync(RespMsg.Headers.Location.OriginalString).Result;
                    sEmitResponse = RespMsg.Content.ReadAsStringAsync().Result;
                    if (RespMsg.Headers.Location!=null)
                    {
                        accesstoken = RespMsg.Headers.Location.OriginalString;
                        accesstoken = accesstoken.Substring(accesstoken.IndexOf("=") + 1);
                        accesstoken = accesstoken.Substring(0, accesstoken.IndexOf("&"));
                        Client.DefaultRequestHeaders.Add("authorization", "Bearer " + accesstoken);
                        while (RespMsg.Headers.Location != null)
                        {
                            RespMsg = Client.GetAsync(RespMsg.Headers.Location.OriginalString).Result;

                        }
                    }
                    string r = Client.GetStringAsync("https://betking.io/bet/dice").Result;
                    sEmitResponse = Client.GetStringAsync("https://api.betking.io/api/stats/gethousestats?appId=0").Result;
                    sEmitResponse = Client.GetStringAsync("https://betking.io/bet/api/account/current-profile").Result;

                    this.username = Username;
                    if (ConnectSocket())
                    {
                        lastupdate = DateTime.Now;
                        ispd = true;
                        new Thread(new ThreadStart(GetBlanaceThread)).Start();
                        GetBalance();
                        GetStats();
                        finishedlogin(true);
                    }
                    else
                        finishedlogin(false);
                    return;
                }

            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
                finishedlogin(false);
                return;
            }
            finishedlogin(false);
        }

        bool ConnectSocket()
        {
            string sid = "";
            string s = Client.GetStringAsync(string.Format("https://socket.betking.io/socket.io/?appId={0}&token={1}&EIO=3&transport=polling&t={2}",0,accesstoken,json.CurrentDate())).Result;
            List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
           
            foreach (Cookie x in cookies.GetCookies(new Uri("https://socket.betking.io")))
            {
                if (x.Name=="io")
                {
                    sid = x.Value;
                    Cookies.Add(new KeyValuePair<string, string>("io", sid));
                }
            }
            
            s = Client.GetStringAsync(string.Format("https://socket.betking.io/socket.io/?appId={0}&token={1}&EIO=3&transport=polling&t={2}&sid={3}", 0, accesstoken, json.CurrentDate(), sid)).Result;

            string address = string.Format("wss://socket.betking.io/socket.io/?appId={0}&token={1}&EIO=3&transport=websocket&sid={2}",0, accesstoken, sid);
            Sock = new WebSocket(address, null, Cookies);
            Sock.Closed += Sock_Closed;
            Sock.Error += Sock_Error;
            Sock.MessageReceived += Sock_MessageReceived;
            Sock.Opened += Sock_Opened;
            Sock.Open();

            while (Sock.State == WebSocketState.Connecting)
                Thread.Sleep(10);

            return(Sock.State == WebSocketState.Open);
            
        }

        private void Sock_Opened(object sender, EventArgs e)
        {
            Sock.Send("2probe");

        }
        long id = 0;
        private void Sock_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            
            if (e.Message == "3probe")
            {
                Sock.Send("5");
            }
            Parent.DumpLog(e.Message, 7);
            
            if (e.Message.StartsWith("42[\"showDiceBet\""))
            {
                string x = e.Message.Substring(e.Message.IndexOf(",") + 1);
                x = x.Substring(0, x.Length - 1);
                BKBet tmpbet = json.JsonDeserialize<BKBet>(x);
                if (tmpbet!=null)
                {
                    if (tmpbet.userName == username && id!=tmpbet.id)
                    {
                        id = tmpbet.id;
                        Bet NewBet = new Bet()
                        {
                            Guid = this.Guid,
                            Amount = tmpbet.betAmount,
                            date = DateTime.Now,
                            Chance = tmpbet.chance,
                            clientseed = clientseed,
                            Currency = tmpbet.currency.ToString(),
                            high = tmpbet.target == 0,
                            Id = tmpbet.id.ToString(),
                            Profit = tmpbet.profit,
                            Roll = tmpbet.roll,
                            serverhash = serverseedhash,
                            nonce = nonce     
                        };
                        this.wagered += NewBet.Amount;
                        this.profit += NewBet.Profit;
                        this.bets++;
                        bool win = (NewBet.Roll > 99.99m - NewBet.Chance && High) || (NewBet.Roll < NewBet.Chance && !High);
                        if (win)
                            wins++;
                        else
                            losses++;
                        FinishedBet(NewBet);
                    }
                }
            }
            if (e.Message.StartsWith("42[\"diceBetResult\","))
            {
                string x = e.Message.Substring(e.Message.IndexOf(",") + 1);
                x = x.Substring(0, x.Length - 1);
                BKBet2 tmpbet = json.JsonDeserialize<BKBet2>(x);
                if (tmpbet!=null)
                {
                    this.nonce = tmpbet.nonce;
                    this.balance = tmpbet.balance;

                }

            }

        }

        private void Sock_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
       {
            
        }

        private void Sock_DataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }

        private void Sock_Closed(object sender, EventArgs e)
        {
            
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }
        string Guid = "";
        void PlaceBetThread(object BetObj)
        {
            PlaceBetObj bet = BetObj as PlaceBetObj;
            this.Guid = bet.Guid;
            string cont = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"appId\":{0},\"chance\":{1:0.0000},\"betAmount\":{2:0.00000000},\"target\":{3},\"currency\":{4}}}", 0, chance, amount, High ? 0 : 1, Curs[Currency]);
            var content = new StringContent(cont, Encoding.UTF8, "application/json");

            HttpResponseMessage response = Client.PostAsync("https://api.betking.io/api/dice/bet", content).Result;

            
        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                string x = Client.GetStringAsync(string.Format("https://api.betking.io/api/stats/getuserstats?appId={0}&userName={1}", 0, User)).Result;
                BKGetStats y = json.JsonDeserialize<BKGetStats>(x);
                if (y == null)
                    return false;


                string cont = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{receiverId:\"{0}\",amount:\"{1:0.00000000}\",currency:{2},code:null}}", y.account.id, amount, Curs[Currency]);
                var content = new StringContent(cont, Encoding.UTF8, "application/json");

                HttpResponseMessage response = Client.PostAsync("https://betking.io/bet/api/account/send-tip", content).Result;
                ForceUpdateStats = (response.IsSuccessStatusCode);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
            return false;

        }

        public static decimal sGetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();
            server = nonce.ToString() + ":" + server + ":" + nonce.ToString();
            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = nonce.ToString() + ":" + client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;

        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }
    }
    public class BKAccount
    {
        public string id { get; set; }
        public string username { get; set; }
        public string creationDate { get; set; }
    }

    public class BKStat
    {
        public string id { get; set; }
        public int currency { get; set; }
        public int appId { get; set; }
        public int numBets { get; set; }
        public decimal profit { get; set; }
        public decimal wagered { get; set; }
    }
    
public class BKGetStats
    {
        public BKAccount account { get; set; }
        public List<BKStat> stats { get; set; }
    }

    public class BKGetBalance
    {
        public decimal Balance { get; set; }
        public string UserName { get; set; }
        public int MaxWin { get; set; }
        public decimal Wagered { get; set; }
        public decimal Profit { get; set; }
        public int NumBets { get; set; }
        public string ServerSeedHash { get; set; }
        public string ClientSeed { get; set; }
        public int Nonce { get; set; }
        public bool IsBettingDisabled { get; set; }
        public bool AreBetsAndStatsHidden { get; set; }
    }

    public class BKBet
    {
        public string userName { get; set; }
        public decimal betAmount { get; set; }
        public decimal chance { get; set; }
        public int target { get; set; }
        public decimal roll { get; set; }
        public decimal profit { get; set; }
        public string date { get; set; }
        public int id { get; set; }
        public int currency { get; set; }
    }
    public class BKBet2
    { 
        public string id { get; set; }
        public decimal balance { get; set; }
        public int nonce { get; set; }
        public int currency { get; set; }
        public int appId { get; set; }
        public decimal profit { get; set; }
        public decimal betAmount { get; set; }
        public decimal roll { get; set; }
    }
}
