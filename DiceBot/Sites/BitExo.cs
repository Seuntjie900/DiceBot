using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
using DiceBot.Schema.BitExo;
using SuperSocket.ClientEngine;
using WebSocket4Net;
namespace DiceBot.Schema.BitExo
{

    public class BEPayout
    {
        public long from { get; set; }
        public long to { get; set; }
        public decimal value { get; set; }
    }

    public class BEBetResult
    {
        public string created_at { get; set; }
        public long raw_outcome { get; set; }
        public string uname { get; set; }
        public long secret { get; set; }
        public string salt { get; set; }
        public string hash { get; set; }
        public long client_seed { get; set; }
        public List<BEPayout> payouts { get; set; }
        public long wager { get; set; }
        public decimal profit { get; set; }
        public string kind { get; set; }
        public decimal edge { get; set; }
        public object @ref { get; set; }
        public string currency { get; set; }
        public long _id { get; set; }
        public long id { get; set; }
        public string next_hash { get; set; }
        public decimal outcome { get; set; }
    }
    public class BEBalances
    {
        public decimal doge { get; set; }
        public decimal bxo { get; set; }
        public decimal clam { get; set; }
        public decimal btc { get; set; }
        public decimal eth { get; set; }
        public decimal ltc { get; set; }
        public decimal flash { get; set; }
    }
    public class BECoininf
    {
        public decimal bets { get; set; }
        public decimal wager { get; set; }
        public decimal profit { get; set; }
        public decimal wager24hour { get; set; }
    }

    public class BEStats
    {
        public BECoininf btc { get; set; }
        public BECoininf bxo { get; set; }
        public BECoininf clam { get; set; }
        public BECoininf doge { get; set; }
        public BECoininf eth { get; set; }
        public BECoininf flash { get; set; }
        public BECoininf ltc { get; set; }
    }
    public class BEUser
    {
        public string uname { get; set; }
        public string email { get; set; }
        public BEBalances balances { get; set; }
        public BEStats stats { get; set; }

        public string last_claim_time { get; set; }
        public double last_claim_betted_wager { get; set; }
        public double total_claims { get; set; }
        public string token { get; set; }
        public double level { get; set; }
        public double levelwager { get; set; }
    }

    public class BEBalanceBase
    {
        public BEUser user { get; set; }
    }

}
namespace DiceBot
{
    class BitExo : DiceSite
    {
        long id = 1;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        WebSocket WSClient;// = new WebSocket("");
        DBRandom R = new DBRandom();
        new public static string[] sCurrencies = new string[] { "BTC", "BXO", "CLAM", "DOGE", "LTC", "ETH" };
        string url = "bit-exo.com";
        public BitExo(cDiceBot Parent)
        {
            this.Parent = Parent;
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://www.moneypot.com/bets/";
            edge = 1;
            //this.Parent = Parent;
            Name = "Bit-Exo";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://bit-exo.com/?ref=seuntjie";
            Currency = "BTC";
            _PasswordText = "API Token";
        }
        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }
        public override void Disconnect()
        {
            ispd = false;
            if (WSClient != null)
            {
                try
                {
                    WSClient.Close();
                }
                catch { };
            }
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }
        string sid = "";
        CookieContainer cookies = new CookieContainer();
        List<KeyValuePair<string, string>> Cookies = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
        public override void Login(string Username, string Password, string twofa)
        {
            cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://" + url + "/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            Parent.DumpLog("BE login 1", 8);

            try
            {
                accesstoken = Password;
                ConnectSocket();
                if (WSClient.State == WebSocketState.Open)
                {
                    Parent.DumpLog("BE login 7.1", 7);
                    ispd = true;

                    lastupdate = DateTime.Now;
                    ForceUpdateStats = false;
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
                Parent.DumpLog(ER.ToString(), -1);
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

        void ConnectSocket()
        {
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
            string response = Client.GetStringAsync("socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate()).Result;
            Parent.DumpLog("BE login 3", 7);
            sid = response.Substring(response.IndexOf("sid\":\"") + "sid\":\"".Length);
            sid = sid.Substring(0, sid.IndexOf("\""));
            Parent.DumpLog("BE login 4", 7);
            foreach (Cookie c3 in cookies.GetCookies(new Uri("http://" + url)))
            {
                if (c3.Name == "io")
                    sid = c3.Value;
                /*if (c3.Name == "__cfduid")
                    c2 = c3;*/
            }
            Parent.DumpLog("BE login 5", 7);
            string chatinit = "42" + id++ + "[\"access_token_data\",{\"access_token\":\"" + accesstoken + "\"}]";
            chatinit = chatinit.Length + ":" + chatinit;
            var content = new StringContent(chatinit, Encoding.UTF8, "application/octet-stream");
            response = Client.PostAsync("socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate() + "&sid=" + sid, content).Result.Content.ReadAsStringAsync().Result;
            Parent.DumpLog("BE login 5", 7);
            Cookies = new List<KeyValuePair<string, string>>();
            Headers = new List<KeyValuePair<string, string>>();
            Headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"));
            foreach (Cookie x in cookies.GetCookies(new Uri("https://" + url)))
            {
                Cookies.Add(new KeyValuePair<string, string>(x.Name, x.Value));
            }
            Cookies.Add(new KeyValuePair<string, string>("io", sid));

            Parent.DumpLog("BE login 6", 7);
            WSClient = new WebSocket("wss://" + url + "/socket.io/?EIO=3&transport=websocket&sid=" + sid,
                null,
                Cookies,
                Headers,
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36",
                "https://" + url,
                WebSocketVersion.Rfc6455,
                null,
                System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);
            WSClient.Closed += WSClient_Closed;
            WSClient.DataReceived += WSClient_DataReceived;
            WSClient.Error += WSClient_Error;
            WSClient.MessageReceived += WSClient_MessageReceived;
            WSClient.Opened += WSClient_Opened;
            WSClient.Open();
            while (WSClient.State == WebSocketState.Connecting)
                Thread.Sleep(100);
        }



        public enum ReqType { balance, bet, hash, tip }
        Dictionary<long, ReqType> Requests = new Dictionary<long, ReqType>();
        private void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if (WSClient.State == WebSocketState.Open && ((DateTime.Now - lastupdate).TotalSeconds > 15 || ForceUpdateStats))
                    {
                        ForceUpdateStats = false;
                        lastupdate = DateTime.Now;
                        WSClient.Send("2");
                        long tmpid = id++;
                        Requests.Add(tmpid, ReqType.balance);
                        WSClient.Send("42" + tmpid + "[\"access_token_data\",{\"access_token\":\"" + accesstoken + "\"}]");
                    }
                }
                catch
                {

                }
                Thread.Sleep(1000);
            }
        }
        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }
        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            long cl = long.Parse(client);
            decimal serverseed = decimal.Parse(server.Substring(0, server.IndexOf("-")), System.Globalization.NumberFormatInfo.InvariantInfo);
            decimal rollb = ((serverseed) + cl) % (long)(4294967296);
            decimal roll = rollb / (Decimal)Math.Pow(2, 32) * 99.9999m;
            roll = roll * (long)10000;
            roll = Math.Floor(roll);
            roll = roll / (long)10000;
            return roll;
        }
        private void WSClient_Opened(object sender, EventArgs e)
        {
            WSClient.Send("2probe");
        }

        private void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Parent.DumpLog(e.Message, 10);
            if (e.Message == "3probe")
            {
                WSClient.Send("5");
                long tmpid = this.id++;
                Requests.Add(tmpid, ReqType.hash);
                WSClient.Send("42" + tmpid + "[\"get_hash\"]");
                tmpid = this.id++;
                Requests.Add(tmpid, ReqType.balance);
                WSClient.Send("42" + tmpid + "[\"access_token_data\",{\"access_token\":\"" + accesstoken + "\"}]");
                /*string Bet = string.Format(
                System.Globalization.NumberFormatInfo.InvariantInfo,
                "42{0}[\"access_token_data\",{{\"app_id\":{1},\"access_token\":\"{2}\",\"currency\":\"{3}\"}}]",
                id++,
                APPId,
                accesstoken,
                Currency
                );
                WSClient.Send(Bet);*/

            }
            else if (e.Message == "3")
            {
            }
            else
            {
                try
                {
                    string response = e.Message;
                    response = response.Substring(2);
                    string id = response.Substring(0, response.IndexOf("["));
                    long lid = 0;

                    if (long.TryParse(id, out lid))
                    {
                        if (Requests.ContainsKey(lid))
                        {
                            ReqType tmp = Requests[lid];
                            Requests.Remove(lid);
                            switch (tmp)
                            {
                                case ReqType.balance:
                                    response = response.Substring(response.IndexOf("{"));
                                    response = response.Substring(0, response.LastIndexOf("}") + 1); ProcessBalance(response); break;
                                case ReqType.hash:
                                    response = response.Substring(response.IndexOf("\"") + 1);
                                    response = response.Substring(0, response.LastIndexOf("\"")); ProcessHash(response); break;
                                case ReqType.bet:
                                    if (response.IndexOf("{") >= 0)
                                    {
                                        response = response.Substring(response.IndexOf("{"));
                                        response = response.Substring(0, response.LastIndexOf("}") + 1); ProcessBet(response);
                                    }
                                    else
                                    {
                                        Parent.updateStatus(response);
                                        if (response.Contains("HASH ERROR"))
                                        {
                                            long tmpid = this.id++;
                                            Requests.Add(tmpid, ReqType.balance);
                                            WSClient.Send("42" + tmpid + "[\"get_hash\"]");
                                        }

                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Parent.DumpLog(ex.ToString(), -1);
                }
            }

        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            long tmpid = id++;
            Requests.Add(tmpid, ReqType.tip);
            //426["send_tip",{"uname":"professor","amount":1000,"private":false,"type":"BTC"}]
            string request = string.Format("42{3}[\"send_tip\",{{\"uname\":\"{0}\",\"amount\":{1},\"private\":false,\"type\":\"{2}\"}}]",
               User,
                Math.Floor(amount * 100000000m),
                Currency,
                tmpid);
            WSClient.Send(request);
            ForceUpdateStats = true;
            return true;
        }

        void ProcessBalance(string Res)
        {
            BEBalanceBase tmpResult = json.JsonDeserialize<BEBalanceBase>(Res);
            switch (Currency.ToLower())
            {
                case "btc":
                    balance = tmpResult.user.balances.btc / 100000000m;
                    this.bets = (int)tmpResult.user.stats.btc.bets;
                    this.wagered = tmpResult.user.stats.btc.wager / 100000000m;
                    this.profit = tmpResult.user.stats.btc.profit / 100000000m;
                    break;
                case "bxo":
                    balance = tmpResult.user.balances.bxo / 100000000m;
                    this.bets = (int)tmpResult.user.stats.bxo.bets;
                    this.wagered = tmpResult.user.stats.bxo.wager / 100000000m;
                    this.profit = tmpResult.user.stats.bxo.profit / 100000000m;
                    break;
                case "clam":
                    balance = tmpResult.user.balances.clam / 100000000m;
                    this.bets = (int)tmpResult.user.stats.clam.bets;
                    this.wagered = tmpResult.user.stats.clam.wager / 100000000m;
                    this.profit = tmpResult.user.stats.clam.profit / 100000000m;
                    break;
                case "doge":
                    balance = tmpResult.user.balances.doge / 100000000m;
                    this.bets = (int)tmpResult.user.stats.doge.bets;
                    this.wagered = tmpResult.user.stats.doge.wager / 100000000m;
                    this.profit = tmpResult.user.stats.doge.profit / 100000000m;
                    break;
                case "eth":
                    balance = tmpResult.user.balances.eth / 100000000m;
                    this.bets = (int)tmpResult.user.stats.eth.bets;
                    this.wagered = tmpResult.user.stats.eth.wager / 100000000m;
                    this.profit = tmpResult.user.stats.eth.profit / 100000000m;
                    break;
                case "flash":
                    balance = tmpResult.user.balances.flash / 100000000m;
                    this.bets = (int)tmpResult.user.stats.flash.bets;
                    this.wagered = tmpResult.user.stats.flash.wager / 100000000m;
                    this.profit = tmpResult.user.stats.flash.profit / 100000000m;
                    break;
                case "ltc":
                    balance = tmpResult.user.balances.ltc / 100000000m;
                    this.bets = (int)tmpResult.user.stats.ltc.bets;
                    this.wagered = tmpResult.user.stats.ltc.wager / 100000000m;
                    this.profit = tmpResult.user.stats.ltc.profit / 100000000m;
                    break;
            }
            Parent.updateBalance(balance);
            Parent.updateProfit(profit);
            Parent.updateWagered(wagered);
            Parent.updateBets(bets);
        }

        void ProcessHash(string Res)
        {
            this.ServerHash = Res;
        }

        void ProcessBet(string Res)
        {
            //433[null,{"created_at":"2019-01-19T11:47:37.991Z","raw_outcome":4264578078,"uname":"seuntjie","secret":2732336931,"salt":"7e59aAbbf531c9649a89a761ac96aba8","hash":"8cf7f15c173d47020d99b20bfa65f4b438799254b69ea8d208aba7fb4dc1c244","client_seed":1532241147,"payouts":[{"from":2168956358,"to":4294967295,"value":2}],"wager":1,"profit":1,"kind":"DICE","edge":1,"ref":null,"currency":"BXO","_id":3917670,"id":3917670,"next_hash":"dc407a52731281f7d9d32eb4e3dff3c84828b4284071483f9826a3072085261c","outcome":99.2923}]
            BEBetResult tmpResult = json.JsonDeserialize<BEBetResult>(Res);
            Bet newBet = new Bet
            {
                Amount = ((decimal)tmpResult.wager) / 100000000m,
                date = DateTime.Now,
                clientseed = tmpResult.client_seed.ToString(),
                Currency = Currency,
                Guid = guid,
                high = this.High,
                nonce = -1,
                Id = tmpResult.id.ToString(),
                Roll = (decimal)tmpResult.outcome,
                serverhash = ServerHash,
                serverseed = tmpResult.secret + "-" + tmpResult.salt,
                Profit = ((decimal)tmpResult.profit) / 100000000m,
                Chance = this.chance
            };
            this.bets++;
            this.wagered += newBet.Amount;
            this.balance += newBet.Profit;
            this.profit += newBet.Profit;
            if ((newBet.high && newBet.Roll > maxRoll - newBet.Chance) || (!newBet.high && newBet.Roll < newBet.Chance))
            {
                wins++;
            }
            else
                losses++;
            this.ServerHash = tmpResult.next_hash;
            FinishedBet(newBet);
        }

        private void WSClient_Error(object sender, ErrorEventArgs e)
        {
            Parent.DumpLog(e.Exception.ToString(), -1);
        }

        private void WSClient_DataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        private void WSClient_Closed(object sender, EventArgs e)
        {
            Parent.DumpLog("BE socket closed", 1);
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
        string guid = "";
        string clientseed = "";
        string ServerHash = "";
        protected override void internalPlaceBet(bool High, decimal amount, decimal chancem, string BetGuid)
        {

            try
            {
                if (WSClient.State != WebSocketState.Open)
                {
                    Parent.DumpLog("Attempting Reconnect", -1);
                    ConnectSocket();
                    Thread.Sleep(5);
                }
                //4268["dice_bet",{"wager":100,"client_seed":537799417,"hash":"02516014ab3098848d8e406968180f0d3f117ba511e5fa7d95d204d4362601da","cond":">","target":50.4999,"payout":200,"currency":"BTC"}]
                this.guid = BetGuid;
                clientseed = R.Next(0, int.MaxValue).ToString();
                long tmpid = id++;
                this.High = High;
                this.chance = chancem;
                Requests.Add(tmpid, ReqType.bet);
                string request = string.Format("42{7}[\"dice_bet\",{{\"wager\":{0:0},\"client_seed\":{1},\"hash\":\"{2}\",\"cond\":\"{3}\",\"target\":{4},\"payout\":{5},\"currency\":\"{6}\"}}]",
                    Math.Floor(amount * 100000000m),
                    clientseed,
                    ServerHash,
                    High ? ">" : "<",
                    High ? maxRoll - chancem : chancem,
                    Math.Floor((amount * 100000000m)) * ((100 - edge) / chancem),
                    Currency,
                    tmpid);
                WSClient.Send(request);
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);

                if (WSClient.State != WebSocketState.Open)
                {

                }
            }

        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }



    }


}
