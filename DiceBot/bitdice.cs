using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace DiceBot
{
    class bitdice:DiceSite
    {

        public static string[] cCurrencies = new string[] { "btc", "doge", "ltc", "redd", "clam", "dash" };
        WebSocket Client;// = new WebSocket("");
        public bitdice(cDiceBot Parent)
        {
            maxRoll = 99.9999;
            AutoInvest = true;
            AutoWithdraw = true;
            AutoInvest = true;
            Tip = true;
            TipUsingName = false;
            ChangeSeed = true;
            Name = "BitDice";
            this.Parent = Parent;
            /*Client = new WebSocket("");
            Client.Opened += Client_Opened;
            Client.Error += Client_Error;
            Client.Closed += Client_Closed;
            Client.MessageReceived += Client_MessageReceived;*/
            
            Currencies = new string[] { "btc", "doge","ltc","redd","clam","dash"};
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
            if (!loggedin)
                finishedlogin(true);
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
                tmp.date = DateTime.Parse(Bet.created_at, System.Globalization.NumberFormatInfo.InvariantInfo);
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
        protected override void internalPlaceBet(bool High)
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

        protected override bool internalWithdraw(double Amount, string Address)
        {
            string s = string.Format("{{\"jsonrpc\":\"2.0\",\"method\":\"user:cashout\",\"params\":{{\"amount\":\"{0:0.00000000}\",\"address\":\"{1}\"}},\"id\":{2}}}", Amount, Address, id++);
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
            return true;
        }

        public override void Login(string Username, string Password, string twofa)
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

                cookie = EmitResponse.Cookies["_csn_session"].Value;
                betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/users/sign_in");


                betrequest.Method = "POST";
                betrequest.CookieContainer = new CookieContainer();

                string post = string.Format("utf8=%E2%9C%93&user%5Busername%5D={0}&user%5Bpassword%5D={1}&user%5Botp_code%5D=&button=", Username, Password);
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
                getcsrf(sEmitResponse);
                cookie = EmitResponse.Cookies["_csn_session"].Value;

                getstream(sEmitResponse);

                getcsrf(sEmitResponse);
                getstream(sEmitResponse);
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
                CurrencyChanged();
                finishedlogin(Client.State == WebSocketState.Open);
                loggedin = true;
                System.Windows.Forms.MessageBox.Show("Due to current limitations of the API, I can't show you your stats until you place a valid bet. Sorry.", "Stats Errors", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
            loggedin = false;
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

        public override bool Invest(double Amount)
        {
            string s = "{\"jsonrpc\":\"2.0\",\"method\":\"invest:invest\",\"params\":{\"amount\":\""+Amount.ToString("0.00000000")+"\"},\"id\":"+(id++)+"}";
            if (Client.State == WebSocketState.Open)
            {
                Client.Send(s);
            }
            return true;
        }
        public override void SendTip(string User, double amount)
        {
            SendChatMessage(string.Format("/tip {0} {1:0.00000000}", User, amount));
        }
        string cookie = "";
        string stream = "";
        string csrf = "";
        protected override void CurrencyChanged()
        {
            if (cookie != "")
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/users/currency");
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
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                cookie = EmitResponse.Cookies["_csn_session"].Value;
                betrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.bitdice.me/");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.CookieContainer = new CookieContainer();
                betrequest.CookieContainer.Add(new Cookie("_csn_session", cookie, "/", "bitdice.me"));
                EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                getcsrf(sEmitResponse);
                getstream(sEmitResponse);
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
        public override double GetLucky(string server, string client, int nonce)
        {
            return base.GetLucky(server, client, nonce);
        }
    }
    /*
    class BitDiceClient
    {
        private static object consoleLock = new object();
        private const int sendChunkSize = 256;
        private const int receiveChunkSize = 256;
        private const bool verbose = true;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(30000);
        public WebSocketState State { get { if (webSocket != null) return webSocket.State; else return WebSocketState.Closed; } }
        /*static void Main(string[] args)
        {
            Thread.Sleep(1000);
            Connect("ws://ws.blockchain.info:8335/inv").Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        public void disconnect()
        {
            try
            {
                webSocket.Abort();
                webSocket.Dispose();
            }
            catch
            {

            }
            
        }

        System.Net.WebProxy _prox = null;
        public System.Net.WebProxy prox { get { return _prox; } set { _prox = value; if (webSocket!=null ) webSocket.Options.Proxy=_prox; } }
        ClientWebSocket webSocket = null;
        public async Task Connect(string uri, string cookie)
        {

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                {
                    sw.WriteLine("begin connect");
                }
                webSocket = new ClientWebSocket();
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                {
                    sw.WriteLine("created socket");
                }
                if (prox!= null)
                {
                    webSocket.Options.Proxy = prox;
                }
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                {
                    sw.WriteLine("done proxy");
                }
                webSocket.Options.Cookies = new System.Net.CookieContainer();
                webSocket.Options.Cookies.Add(new System.Net.Cookie("_csn_session", cookie, "/", "bitdice.me"));
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                {
                    sw.WriteLine("added cookie\nconnecting");
                }
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                {
                    sw.WriteLine("connected\nstarting send+receive");
                }
                await Task.WhenAll(Receive(webSocket), Send(webSocket));
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                {
                    sw.WriteLine(ex.Message);
                }
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
                Console.WriteLine();

                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("WebSocket closed.");
                    Console.ResetColor();
                    using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                    {
                        sw.WriteLine("socket closed");
                    }
                }
            }
        }
        UTF8Encoding encoder = new UTF8Encoding();

        public void Send(string Message)
        {
            byte[] buffer = encoder.GetBytes(Message);
            webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Send(ClientWebSocket webSocket)
        {

            //byte[] buffer = encoder.GetBytes("{\"op\":\"blocks_sub\"}"); //"{\"op\":\"unconfirmed_sub\"}");
            byte[] buffer = encoder.GetBytes("{\"op\":\"unconfirmed_sub\"}");
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            while (webSocket.State == WebSocketState.Open)
            {
                
                await Task.Delay(delay);
            }
        }

        public delegate void dBetResult(bitstatsbet Bet);
        public delegate void dUserStats(bitstatsbuser User);
        public delegate void dChatMessage(bitChatReceived Chat);
        public event dBetResult BetResult;
        public event dUserStats UserStats;
        public event dChatMessage ChatReceived;
        List<byte> bytes = new List<byte>();

        void processReceived(string Message)
        {
            socketbase tmp = json.JsonDeserialize<socketbase>(Message);
            if (!string.IsNullOrEmpty(tmp.method))
            {
                switch (tmp.method)
                {
                    case "chat:new": if (ChatReceived != null) ChatReceived(json.JsonDeserialize<bitchatSocket>(Message.Replace("params", "_params"))._params); break;
                    case "stat.global": break;
                    case "stat.user": if (UserStats != null) UserStats(json.JsonDeserialize<bitstatsusersocket>(Message.Replace("params", "_params"))._params); break;
                    case "stat.bets": if (BetResult != null) BetResult(json.JsonDeserialize<bitstatsbetsocket>(Message.Replace("params", "_params").Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}"))._params); break;
                }
            }
        }

        private async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[receiveChunkSize];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {

                    for (int i = 0; i < result.Count; i++)
                    {
                        if (buffer[i] != 0)
                            bytes.Add(buffer[i]);
                    }
                    
                    if (result.EndOfMessage)
                    {
                        var str = System.Text.Encoding.Default.GetString(bytes.ToArray());
                        using (System.IO.StreamWriter sw = System.IO.File.AppendText("socketlog.txt"))
                        {
                            sw.WriteLine(str);
                        }
                        
                        bytes = new List<byte>();
                        processReceived(str);
                    }
                }
            }
        }

        

        
    }
*/
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
        public double balance { get; set; }
        public int total_bets { get; set; }
        public double wagered { get; set; }
        public double profit { get; set; }
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
        public double target { get; set; }
        public double mutliplier { get; set; }
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
