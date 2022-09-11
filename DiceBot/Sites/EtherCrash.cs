using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using WebSocket4Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DiceBot.Core;
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    class EtherCrash : DiceSite
    {
        string Token = "";
        CookieContainer Cookies;
        HttpClientHandler ClientHandlr;
        HttpClient Client;
        WebSocket Sock;
        WebSocket ChatSock;
        bool isec = false;
        string username = "";
        public EtherCrash(cDiceBot Parent)
        {
            this.Parent = Parent;
            maxRoll = 99.99999m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://www.ethercrash.io/game/";
            edge = 1;
            //this.Parent = Parent;
            Name = "EtherCrash";
            _PasswordText = "API Key";
            Tip = false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://www.ethercrash.io/";

        }

        public override void Disconnect()
        {
            if (Sock != null)
            {
                try
                {
                    isec = false;
                    Sock.Close();
                }
                catch
                {

                }
            }
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }
        string io = "";
        string cfuid = "";
        public override void Login(string Username, string Password, string twofa)
        {
            Cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = Cookies, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null }; ;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://ethercrash.io") };
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
            Client.DefaultRequestHeaders.Add("referer", "https://www.ethercrash.io/play");
            Client.DefaultRequestHeaders.Add("origin", "https://www.ethercrash.io");
            try
            {
                Cookies.Add(new Cookie("id", Password, "/", "ethercrash.io"));


                string Response = Client.GetStringAsync("https://www.ethercrash.io/play").Result;


                Response = Client.GetStringAsync("https://www.ethercrash.io/socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate()).Result;

                Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&transport=polling&t=" + json.CurrentDate()).Result;

                string iochat = "";
                foreach (Cookie c3 in Cookies.GetCookies(new Uri("http://www.ethercrash.io")))
                {
                    if (c3.Name == "io")
                        iochat = c3.Value;
                    if (c3.Name == "__cfduid")
                        cfuid = c3.Value;
                }
                Response = Client.GetStringAsync("https://www.ethercrash.io/socket.io/?EIO=3&sid=" + iochat + "&transport=polling&t=" + json.CurrentDate()).Result;


                foreach (Cookie c3 in Cookies.GetCookies(new Uri("http://gs.ethercrash.io")))
                {
                    if (c3.Name == "io")
                        io = c3.Value;
                    if (c3.Name == "__cfduid")
                        cfuid = c3.Value;
                }

                StringContent ottcontent = new StringContent("");
                HttpResponseMessage RespMsg = Client.PostAsync("https://www.ethercrash.io/ott", ottcontent).Result;

                Response = RespMsg.Content.ReadAsStringAsync().Result;
                if (RespMsg.IsSuccessStatusCode)
                    ott = Response;
                else
                {
                    finishedlogin(false);
                    return;
                }


                string body = "420[\"join\",{\"ott\":\"" + ott + "\",\"api_version\":1}]";
                body = body.Length + ":" + body;
                StringContent stringContent = new StringContent(body, UnicodeEncoding.UTF8, "text/plain");

                Response = Client.PostAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate(), stringContent).Result.Content.ReadAsStringAsync().Result;




                body = "420[\"join\",[\"english\"]]";
                body = body.Length + ":" + body;
                StringContent stringContent2 = new StringContent(body, UnicodeEncoding.UTF8, "text/plain");

                Response = Client.PostAsync("https://www.ethercrash.io/socket.io/?EIO=3&sid=" + iochat + "&transport=polling&t=" + json.CurrentDate(), stringContent2).Result.Content.ReadAsStringAsync().Result;

                Response = Client.GetStringAsync("https://www.ethercrash.io/socket.io/?EIO=3&sid=" + iochat + "&transport=polling&t=" + json.CurrentDate()).Result;

                List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>();
                cookies.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                cookies.Add(new KeyValuePair<string, string>("io", iochat));
                cookies.Add(new KeyValuePair<string, string>("id", Password));

                ChatSock = new WebSocket("wss://www.ethercrash.io/socket.io/?EIO=3&transport=websocket&sid=" + iochat,
                   null,
                   cookies/*,
                   null,
                   "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36",
                   "https://www.ethercrash.io",
                    WebSocketVersion.None*/);
                ChatSock.Opened += Sock_Opened;
                ChatSock.Error += Sock_Error;
                ChatSock.MessageReceived += Sock_MessageReceived;
                ChatSock.Closed += Sock_Closed;
                ChatSock.Open();
                while (ChatSock.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(300);
                    //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                }
                if (ChatSock.State == WebSocketState.Open)
                {
                }
                else
                {
                    finishedlogin(false);
                    return;
                }


                //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                Thread.Sleep(200);
                //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                List<KeyValuePair<string, string>> cookies2 = new List<KeyValuePair<string, string>>();
                cookies2.Add(new KeyValuePair<string, string>("__cfduid", cfuid));
                cookies2.Add(new KeyValuePair<string, string>("io", io));
                cookies2.Add(new KeyValuePair<string, string>("id", Password));

                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                headers.Add(new KeyValuePair<string, string>("Host", "gs.ethercrash.io"));
                headers.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36"));
                headers.Add(new KeyValuePair<string, string>("Origin", "https://www.ethercrash.io"));

                Sock = new WebSocket("wss://gs.ethercrash.io/socket.io/?EIO=3&transport=websocket&sid=" + io/*,
                    null,
                    cookies2,
                    headers,
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36",
                    "https://www.ethercrash.io",
                        WebSocketVersion.None*/);
                Sock.Opened += Sock_Opened;
                Sock.Error += Sock_Error;
                Sock.MessageReceived += Sock_MessageReceived;
                Sock.Closed += Sock_Closed;
                Sock.Open();
                while (Sock.State == WebSocketState.Connecting)
                {
                    Thread.Sleep(300);
                    //Response = Client.GetStringAsync("https://gs.ethercrash.io/socket.io/?EIO=3&sid=" + io + "&transport=polling&t=" + json.CurrentDate()).Result;

                }
                if (Sock.State == WebSocketState.Open)
                {
                    finishedlogin(true);
                    isec = true;
                    Thread t = new Thread(pingthread);
                    t.Start();
                    return;
                }
                finishedlogin(false);
            }
            catch (Exception ex)
            {
                Parent.DumpLog(ex.ToString(), -1);
                finishedlogin(false);
            }

        }

        DateTime LastPing = DateTime.Now;
        void pingthread()
        {
            while (isec)
            {
                if ((DateTime.Now - LastPing).TotalSeconds >= 15)
                {
                    LastPing = DateTime.Now;
                    try
                    {
                        Sock.Send("2");
                        ChatSock.Send("2");
                    }
                    catch
                    {
                        isec = false;
                    }
                }
            }
        }

        private void Sock_Closed(object sender, EventArgs e)
        {

        }

        string ott = "";
        bool betsOpen;
        bool cashedout = false;
        string GameId = "";
        private void Sock_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message == "3probe")
            {
                (sender as WebSocket).Send("5");
                //Sock.Send("420[\"join\",{\"ott\":\"" + ott + "\",\"api_version\":1}]");     
            }
            else
            {
                Parent.DumpLog(e.Message, -1);
                if (e.Message.StartsWith("42[\"game_starting\","))
                {
                    //42["game_starting",{"game_id":214035,"max_win":3782817516,"time_till_start":5000}]
                    string id = e.Message.Substring(e.Message.IndexOf("\"game_id\":") + "\"game_id\":".Length);
                    id = id.Substring(0, id.IndexOf(","));
                    GameId = id;
                    cashedout = false;
                    this.guid = "";
                    //open betting for user
                    betsOpen = true;
                    Parent.updateStatus("Game starting - waiting for bet");
                }
                else if (e.Message.StartsWith("42[\"game_started\","))
                {
                    //close betting and wait for result
                    betsOpen = false;
                }
                else if (e.Message.StartsWith("42[\"game_crash\",") && guid != "")
                {
                    //if not cashed out yet, it's a loss and debit balance

                    Bet bet = new Bet
                    {
                        Amount = (decimal)amount,
                        Profit = -amount,
                        Chance = chance,
                        high = true,
                        Currency = Currency,
                        date = DateTime.Now,
                        Id = GameId != "" ? GameId : Guid.NewGuid().ToString(),
                        Roll = 0,
                        UserName = username,
                        nonce = -1,
                        Guid = guid
                    };

                    this.balance -= amount;
                    this.profit -= amount;
                    this.wagered += amount;
                    this.losses++;
                    this.bets++;
                    this.guid = "";
                    FinishedBet(bet);
                    Parent.updateStatus("Game crashed - Waiting for next game");
                }
                else if (e.Message.StartsWith("42[\"cashed_out\"") && guid != "")
                {
                    //check if the cashed out user is the current user, if it is, it's a win.

                    if (e.Message.Contains("\"" + username + "\""))
                    {
                        cashedout = true;
                        Bet bet = new Bet
                        {
                            Amount = (decimal)amount,
                            Profit = ((100 - edge) / chance) * amount - amount,
                            Chance = chance,
                            high = true,
                            Currency = Currency,
                            date = DateTime.Now,
                            Id = GameId != "" ? GameId : Guid.NewGuid().ToString(),
                            Roll = (100 - chance) + 0.00001m,
                            UserName = username,
                            nonce = -1,
                            Guid = guid
                        };
                        this.guid = "";
                        this.balance += bet.Profit;
                        this.profit += bet.Profit;
                        this.wagered += amount;
                        this.wins++;
                        this.bets++;
                        FinishedBet(bet);
                    }
                }
                else if (e.Message.StartsWith("430[null,{\"state"))
                {
                    string content = e.Message.Substring(e.Message.IndexOf("{"));
                    content = content.Substring(0, content.LastIndexOf("}"));
                    ECLogin tmplogin = json.JsonDeserialize<ECLogin>(content);
                    if (tmplogin != null)
                    {
                        this.username = tmplogin.username;
                        this.balance = (tmplogin.balance_satoshis) / 100000000m;
                        Parent.updateBalance(balance);

                    }
                }
                else if (e.Message.StartsWith("42[\"game_tick\""))
                {
                    if (guid != "" || cashedout)
                    {
                        //42["game_tick",13969]
                        string x = e.Message.Substring(e.Message.IndexOf(",") + 1);
                        x = x.Substring(0, x.LastIndexOf("]"));
                        decimal tickval = 0;
                        if (decimal.TryParse(x, out tickval))
                        {
                            double mult = 0;
                            mult = Math.Floor(100 * Math.Pow(Math.E, 0.00006 * (double)tickval));
                            string message = "";

                            if (guid != null)
                            {
                                message = string.Format("Game running - {0:0.00000000} at {1:0.0000} - {2:0.00}x", amount, (100 - edge) / chance, mult / 100);
                            }
                            else
                            {
                                message = string.Format("Game running - Cashed out - {2:0.00}x", amount, (100 - edge) / chance, mult / 100);
                            }

                            Parent.updateStatus(message);
                        }
                    }
                }
            }
        }

        private void Sock_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Parent.DumpLog(e.Exception.ToString(), -1);
            Parent.updateStatus("Websocket error - disconnected.");
        }

        private void Sock_Opened(object sender, EventArgs e)
        {
            (sender as WebSocket).Send("2probe");
        }

        public override bool ReadyToBet()
        {
            return betsOpen;
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
        int reqid = 1;
        string guid = "";
        protected override void internalPlaceBet(bool High, decimal amount, decimal chancem, string BetGuid)
        {
            if (ReadyToBet() && Sock.State == WebSocketState.Open)
            {
                this.amount = Math.Round(amount, 6);
                amount = this.amount;
                decimal payout = (100 - edge) / chance;
                decimal returna = payout * 100;
                Sock.Send("42" + (reqid++).ToString() + "[\"place_bet\"," + (amount * 100000000).ToString("0") + "," + returna.ToString("0") + "]");
                this.guid = BetGuid;
                Parent.updateStatus(string.Format("Game Starting - Betting {0:0.00000000} at {1:0.00}x", amount, payout));
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }
    }

    public class ECLogin
    {
        public string state { get; set; }
        public int game_id { get; set; }
        public string last_hash { get; set; }
        public double max_win { get; set; }
        public int elapsed { get; set; }
        public string created { get; set; }

        public string username { get; set; }
        public long balance_satoshis { get; set; }
    }
}
