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
        public static string[] sCurrencies = new string[] { "BTC" };//,"LTC","DASH" };
        public DiceSeuntjie(cDiceBot Parent)
        {
            Currency = "BTC";
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
            Currencies = sCurrencies;
            
        }
        protected override void CurrencyChanged()
        {
            if (ispd && WSClient != null)
            {
                string Bet = string.Format(
                    System.Globalization.NumberFormatInfo.InvariantInfo,
                    "42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\",\"currency\":\"{3}\"}}]",
                    id++,
                    APPId,
                    accesstoken,
                    Currency
                    );
                WSClient.Send(Bet);
            }
        }

        protected string url { get; set; }
        long id = 0;
        string OldHash = "";
        string ClientSeed = "";
        string Guid = "";
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.Guid = Guid;
            this.High = High;
            ClientSeed = "";
            byte[] bytes = new byte[4];
            R.GetBytes(bytes);
            long client = (long)BitConverter.ToUInt32(bytes, 0);
            ClientSeed = client.ToString();
            long Roundedamount = (long)(Math.Round((amount), 8) * 100000000);
            
            string Bet = string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"42{0}[\"dice_bet\",{{\"wager\":{1:0},\"client_seed\":{2},\"hash\":\"{3}\",\"cond\":\"{4}\",\"target\":{5:0.0000},\"payout\":{6:0.0000}, \"currency\":\"{7}\"}}]",
                id++, Roundedamount, ClientSeed, OldHash, High ? ">" : "<", High ? 100m - chance : chance, (((100m - edge) / chance) * Roundedamount), Currency);
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
            Parent.DumpLog("BE login 1",8);
                
            try
            {
                accesstoken = Password;
                string s1 = "";
                HttpResponseMessage resp = Client.GetAsync("").Result;
                Parent.DumpLog("BE login 2", 8);
                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                    Parent.DumpLog("BE login 2.1", 7);
                }
                else
                {
                    Parent.DumpLog("BE login 2.2", 7);
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
                    Parent.DumpLog("BE login 2.3", 7);
                }
                string response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t="+CurrentDate()).Result;
                Parent.DumpLog("BE login 3", 7);
                string c = 
                response.Substring(response.IndexOf("sid\":\"") + "sid\":\"".Length);
                c = c.Substring(0, c.IndexOf("\""));
                Parent.DumpLog("BE login 4", 7);
                foreach (Cookie c3 in cookies.GetCookies(new Uri("http://"+ url)))
                {
                    if (c3.Name == "io")
                        c = c3.Value;
                    /*if (c3.Name == "__cfduid")
                        c2 = c3;*/
                }
                Parent.DumpLog("BE login 5", 7);
                string chatinit = "420[\"chat_init\",{\"app_id\":"+APPId+",\"access_token\":\"" + accesstoken + "\",\"subscriptions\":[\"CHAT\",\"DEPOSITS\",\"BETS\"]}]";
                chatinit = chatinit.Length + ":" + chatinit;
                var content = new StringContent(chatinit, Encoding.UTF8, "application/octet-stream");
                response = Client.PostAsync("socket.io/?EIO=3&transport=polling&t=" + CurrentDate() + "&sid=" + c, content).Result.Content.ReadAsStringAsync().Result;
                Parent.DumpLog("BE login 5", 7);
                List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
                Headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"));
                foreach (Cookie x in cookies.GetCookies(new Uri("https://" + url)))
                {
                    Cookies.Add(new KeyValuePair<string,string>(x.Name, x.Value));
                }
                Cookies.Add(new KeyValuePair<string,string>("io",c));
                Parent.DumpLog("BE login 6", 7);
                WSClient = new WebSocket("wss://"+url+"/socket.io/?EIO=3&transport=websocket&sid=" + c, null, Cookies, Headers, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36", "https://"+url, WebSocketVersion.Rfc6455, null, System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);
                WSClient.Closed += WSClient_Closed;
                WSClient.DataReceived += WSClient_DataReceived;
                WSClient.Error += WSClient_Error;
                WSClient.MessageReceived += WSClient_MessageReceived;
                WSClient.Opened += WSClient_Opened;
                WSClient.Open();
                while (WSClient.State == WebSocketState.Connecting)
                    Thread.Sleep(100);
                if (WSClient.State == WebSocketState.Open)
                {
                    Parent.DumpLog("BE login 7.1", 7);
                    ispd = true;
                    
                    lastupdate = DateTime.Now;
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    finishedlogin(true); return;
                }
                else
                {
                    Parent.DumpLog("BE login 7.2", 7);
                    finishedlogin(false);
                    return;
                }
            }
            catch (AggregateException ER)
            {
                Parent.DumpLog(ER.ToString(),-1);
                finishedlogin(false);
                return;
            }
            catch (Exception ERR)
            {
                Parent.DumpLog(ERR.ToString(), -1);
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
            WSClient.Send(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"42{0}[\"send_tip\",{{\"uname\":\"{1}\",\"amount\":{2:0.00000000},\"private\":false,\"type\":\"{3}\"}}]", id++, User, amount*100000000m, Currency));
            return true;
        }

        void GetBalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now-lastupdate).TotalSeconds>=30)
                {
                    lastupdate = DateTime.Now;
                    string Bet = string.Format(
                System.Globalization.NumberFormatInfo.InvariantInfo,
                "42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\",\"currency\":\"{3}\"}}]",
                id++,
                APPId,
                accesstoken,
                Currency
                );
                    WSClient.Send(Bet);
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
                WSClient.Send("42"+id++ +"[\"get_hash\",null]");
                string Bet = string.Format(
                System.Globalization.NumberFormatInfo.InvariantInfo,
                "42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\",\"currency\":\"{3}\"}}]",
                id++,
                APPId,
                accesstoken,
                Currency
                );
                WSClient.Send(Bet);

            }
            else
            {
                if (e.Message.Contains("[null,{\"bet_hash\":"))
                {
                     string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDiceHash tmphash = json.JsonDeserialize<SDiceHash>(msg);
                    this.OldHash = tmphash.bet_hash;
                }
                if (e.Message.Contains("[null,{\"token\":"))
                {
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    SDIceAccToken tmphash = json.JsonDeserialize<SDIceAccToken>(msg);
                    this.balance = (Currency.ToLower()=="btc"? tmphash.user.balances.btc:
                        Currency.ToLower() == "ltc" ? tmphash.user.balances.ltc :
                        Currency.ToLower() == "dash" ? tmphash.user.balances.dash :
                        0
                        ) / 100000000m;
                    this.profit = tmphash.user.betted_profit / 100000000m;
                    this.wagered = tmphash.user.betted_wager / 100000000m;
                    this.bets = (int)tmphash.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    Parent.updateBets(bets);
                }
                if (e.Message.Contains("[null,{\"auth_id\":"))
                {
                    string msg = e.Message.Substring(e.Message.IndexOf("{"));
                    msg = msg.Substring(0, msg.Length - 1);
                    sdiceauth tmphash = json.JsonDeserialize<sdiceauth>(msg);
                    this.balance = (Currency.ToLower() == "btc" ? tmphash.user.balances.btc :
                        Currency.ToLower() == "ltc" ? tmphash.user.balances.ltc :
                        Currency.ToLower() == "dash" ? tmphash.user.balances.dash :
                        0
                        ) / 100000000m;
                    this.profit = tmphash.user.betted_profit / 100000000m;
                    this.wagered = tmphash.user.betted_wager / 100000000m;
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
                    if (tmphash.bet_id > 0)
                    {
                        Bet newbet = new Bet
                        {
                            Amount = amount,
                            date = DateTime.Now,
                            Chance = chance,
                            high = High,
                            clientseed = ClientSeed,
                            Id = tmphash.bet_id.ToString(),
                            nonce = 0,
                            serverseed = tmphash.secret,//tmphash.salt,
                            serverhash = OldHash,
                            Profit = tmphash.profit / 100000000m,
                            Roll = decimal.Parse(tmphash.outcome, System.Globalization.NumberFormatInfo.InvariantInfo),
                            Guid=this.Guid
                        };
                        OldHash = tmphash.next_hash;
                        balance += newbet.Profit;
                        profit += newbet.Profit;
                        bets++;
                        wagered += newbet.Amount;
                        bool win = false;
                        if ((newbet.Roll > maxRoll - newbet.Chance && High) || (newbet.Roll < newbet.Chance && !High))
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

        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            return moneypot.sGetLucky(server, client, nonce);
        }
    }
    public class SDIceBet
    {
        public long ID { get; set; }
        public long bet_id { get; set; }
        public string outcome { get; set; }
        public decimal profit { get; set; }
        public string secret { get; set; }
        public string salt { get; set; }
        public string created_at { get; set; }
        public string next_hash { get; set; }
        public long raw_outcome { get; set; }
        public string kind { get; set; }
    }
     public class SDIceAccToken
    {
        public SDIceAccToken auth { get; set; }
        public SDiceUser user { get; set; }
    }
    
    public class SDiceUser
    {
        public string uname { get; set; }
        public string role { get; set; }
        public sdiceBalances balances { get; set; }
        public sdiceBalances unconfirmed { get; set; }
        public object balance { get; set; }
        public decimal wager24hour { get; set; }
        public decimal profit24hour { get; set; }
        public object @ref { get; set; }
        public decimal refprofit { get; set; }
        public decimal refpaid { get; set; }
        public decimal refprofitLTC { get; set; }
        public decimal refprofitDASH { get; set; }
        public decimal refprofitDOGE { get; set; }
        public List<string> open_pm { get; set; }
        public decimal betted_wager { get; set; }
        public decimal betted_count { get; set; }
        public decimal betted_profit { get; set; }
        public decimal level { get; set; }
    }
    public class SDiceHash
    {
        public string hash { get; set; }
        public string bet_hash { get; set; }
    }
    public class sdiceBalances
    {
        public decimal btc { get; set; }
        public decimal ltc { get; set; }
        public decimal dash { get; set; }
    }
    public class sdiceauth
    {
        public sdiceauth auth {get; set;}
        public int id { get; set; }
        public int auth_id { get; set; }
        public int app_id { get; set; }
        public SDiceUser user { get; set; }
    }
}
