using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace DiceBot
{
    class PRC : DiceSite
    {
        HubConnection con = new HubConnection("https://betking.io/SignalR/", "", false);
        CookieContainer Cookies = new CookieContainer();
        IHubProxy dicehub;


        public PRC(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoWithdraw = true;
            AutoLogin = true;
            ChangeSeed = true;
            AutoInvest = true;
            Tip = true;
            this.Parent = Parent;
            Name = "PRCDice";
            BetURL = "https://betking.io/api/bets/GetBet?id=";
            con.Received += con_Received;
            SiteURL = "https://betking.io";
        }

        public override void SetProxy(string host, int port)
        {
            base.SetProxy(host, port);
            con.Proxy = Prox;
        }
        public override void SetProxy(string host, int port, string username, string password)
        {
            base.SetProxy(host, port, username, password);
            con.Proxy = Prox;
        }

        void GotChatMessage(string message, string time, string user, string userid, string roomid, string ismod)
        {
            ReceivedChatMessage(string.Format("{0:hh:mm:ss} ({1}) <{2}> {3}", json.ToDateTime2(time), userid, user, message));
        }


        void con_Received(string obj)
        {

        }
        DateTime LastBet = DateTime.Now;
        decimal LastBetAmount = 0;
        protected override async void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            int retries = 0;
            while (retries < 2)
            {
                retries++;
                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
                try
                {
                    await dicehub.Invoke("Bet", High ? 0 : 1, amount, chance);
                    retries = 5;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    Parent.updateStatus(string.Format("Bet Failed. Retrying."));
                }
            }
            if (retries < 5)
                Parent.updateStatus(string.Format("Failed to place bets after 3 retries, stopping. Please check network and bot settings."));
        }

        private void DiceBetResult(PRCMYstats tmpStats)
        {
            if (tmpStats.Success)
            {
                LastBet = DateTime.Now;
                LastBetAmount = amount;

                Bet tmp = tmpStats.DiceBet;
                if (tmp.uid == UserID)
                {
                    balance = (decimal)tmpStats.AvailableBalance;
                    wins = tmpStats.Wins;
                    losses = tmpStats.Losses;
                    wagered = (decimal)tmpStats.Wagered;
                    bets = tmpStats.NumBets;
                    profit = (decimal)tmpStats.Profit;

                    tmp.serverhash = serverhash;
                    tmp.date = DateTime.Now;
                    FinishedBet(tmp);
                }
            }
            else
                Parent.updateStatus(string.Format("Failed to place bets, stopping. Please check network and bot settings."));
        }


        public override bool Invest(decimal Amount)
        {
            /*withdraw = 2;
            withdrawTime = DateTime.Now;
            wdAmount = Amount;
            return true;*/
            System.Threading.Thread.Sleep(3200);
            dicehub.Invoke("invest", Amount, 0.5m);
            System.Threading.Thread.Sleep(120);
            return true;
        }

        Random r = new Random();
        public override void ResetSeed()
        {

            HttpWebRequest getHeaders = HttpWebRequest.Create("https://betking.io/account/GenerateNewServerSeed") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            getHeaders.CookieContainer = Cookies;

            getHeaders.Method = "POST";
            string post = string.Format("__RequestVerificationToken=" + s);
            getHeaders.ContentType = "application/x-www-form-urlencoded";
            getHeaders.ContentLength = post.Length;
            using (var writer = new StreamWriter(getHeaders.GetRequestStream()))
            {
                string writestring = post as string;
                writer.Write(writestring);
            }
            Thread.Sleep(5000);
        }

        private void SaveClientSeedResult(PRCResetSeed clientSeedReset)
        {
            if (clientSeedReset.Result)
            {
                //sqlite_helper.InsertSeed(tmp.PreviousServerHash, tmp.PreviousServerSeed);

                client = clientSeedReset.ClientSeed;
            }
            else
            {
                Parent.updateStatus("Failed to reset seed, too soon.");
            }
        }

        private void GenerateServerSeedResult(PRCResetSeed serverSeedReset)
        {
            if (serverSeedReset.Result)
            {
                sqlite_helper.InsertSeed(serverSeedReset.PreviousServerHash, serverSeedReset.PreviousServerSeed);
                serverhash = serverSeedReset.CurrentServerHash;


                HttpWebRequest getHeaders2 = HttpWebRequest.Create("https://betking.io/account/SaveClientSeed?gameType=0") as HttpWebRequest;
                if (Prox != null)
                    getHeaders2.Proxy = Prox;
                getHeaders2.CookieContainer = Cookies;

                getHeaders2.Method = "POST";
                string tmpClient = r.Next(0, int.MaxValue).ToString();
                string post2 = string.Format("clientSeed=" + tmpClient + "&__RequestVerificationToken=" + s);
                getHeaders2.ContentType = "application/x-www-form-urlencoded";
                getHeaders2.ContentLength = post2.Length;
                using (var writer = new StreamWriter(getHeaders2.GetRequestStream()))
                {
                    string writestring = post2 as string;
                    writer.Write(writestring);
                }
            }
            else
            {
                Parent.updateStatus("Failed to reset seed, too soon.");
            }
        }

        public override void Donate(decimal Amount)
        {
            SendTip("357", Amount);
        }

        public override bool InternalSendTip(string User, decimal amount)
        {

            int uid = -1;
            if (int.TryParse(User, out uid))
            {
                System.Threading.Thread.Sleep(3200);
                if (dicehub != null)
                    dicehub.Invoke("tip", uid, amount, "");
                System.Threading.Thread.Sleep(120);
            }
            return true;
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }




        public override bool ReadyToBet()
        {
            decimal millis = (decimal)(DateTime.Now - LastBet).TotalMilliseconds;
            if (amount >= 0.001m)//&& millis>250)
                return true;
            else if (LastBetAmount >= 0.0001m && millis > 210m)
                return true;
            else if (LastBetAmount >= 0.00001m && millis > 510m)
                return true;
            else if (LastBetAmount >= 0.000001m && millis > 810m)
                return true;
            else if (millis > 1010m)
                return true;
            return false;
        }


        public override void Disconnect()
        {

            con.Stop();
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            dicehub.Invoke("Chat", Message, 1);
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            System.Threading.Thread.Sleep(1200);
            dicehub.Invoke("Withdraw", Address, Amount, "");
            System.Threading.Thread.Sleep(120);
            return true;
        }
        void ReceivedChat(string messages, string time, string user, int id, int room, bool ismod)
        {
            ReceivedChatMessage(string.Format("{0:hh:mm} ({1}) <{2}> {3}", DateTime.Parse(time, System.Globalization.DateTimeFormatInfo.InvariantInfo), user, id, messages));
        }
        void ReceivedChat(string messages, string time, string user, int from, bool ismod)
        {
            ReceivedChatMessage(string.Format("{0:hh:mm} ({1}) <{2}> PM: {3}", DateTime.Parse(time, System.Globalization.DateTimeFormatInfo.InvariantInfo), user, from, messages));
        }

        string s = "";
        public override void Login(string Username, string Password, string twofa)
        {
            HttpWebRequest getHeaders = HttpWebRequest.Create("https://betking.io/bitcoindice#dice") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            var cookies = new CookieContainer();
            getHeaders.CookieContainer = cookies;
            HttpWebResponse Response = null;
            string rqtoken = "";
            string s1 = "";
            try
            {

                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                string tmp = s1.Substring(s1.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                s = rqtoken = tmp.Substring(0, tmp.IndexOf("\""));
            }
            catch (WebException e)
            {

                finishedlogin(false);
                return;
            }

            getHeaders = HttpWebRequest.Create("https://betking.io/account/login") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            getHeaders.CookieContainer = new CookieContainer();
            foreach (Cookie c in Response.Cookies)
            {

                getHeaders.CookieContainer.Add(c);
            }
            getHeaders.Method = "POST";
            string post = string.Format("userName={0}&password={1}&twoFactorCode={2}&__RequestVerificationToken={3}", Username, Password, twofa, rqtoken);
            getHeaders.ContentType = "application/x-www-form-urlencoded";
            getHeaders.ContentLength = post.Length;
            using (var writer = new StreamWriter(getHeaders.GetRequestStream()))
            {
                string writestring = post as string;
                writer.Write(writestring);
            }
            try
            {

                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                if (!s1.ToLower().Contains("true"))
                {

                    finishedlogin(false);
                    return;
                }
                /*string tmp = s1.Substring(s1.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                rqtoken = tmp.Substring(0, tmp.IndexOf("\""));*/
            }
            catch (WebException e)
            {
                Response = (HttpWebResponse)e.Response;
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                finishedlogin(false);
                return;
            }

            foreach (Cookie c in Response.Cookies)
            {
                if (c.Name == "__RequestVerificationToken")
                    rqtoken = c.Value;
                Cookies.Add(c);
            }
            Cookies.Add((new Cookie("PRC_Affiliate", "357", "/", "betking.io")));
            con.CookieContainer = Cookies;
            try
            {
                getHeaders = HttpWebRequest.Create("https://betking.io/bitcoindice#dice") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                string stmp = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                string sstmp = stmp.Substring(stmp.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                s = rqtoken = sstmp.Substring(0, sstmp.IndexOf("\""));




                dicehub = con.CreateHubProxy("diceHub");
                con.Start().Wait();

                dicehub.Invoke("joinChatRoom", 1);
                dicehub.On<string, string, string, int, int, bool>("chat", ReceivedChat);
                dicehub.On<string, string, string, int, bool>("receivePrivateMesssage", ReceivedChat);
                dicehub.On<PRCMYstats>("diceBetResult", DiceBetResult);
                dicehub.On<PRCResetSeed>("generateServerSeedResult", GenerateServerSeedResult);
                dicehub.On<PRCResetSeed>("saveClientSeedResult", SaveClientSeedResult);

                getHeaders = HttpWebRequest.Create("https://betking.io/account/GetUserAccount") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                PRCUser tmp = json.JsonDeserialize<PRCUser>(s1);
                balance = (decimal)tmp.AvailableBalance;
                profit = (decimal)tmp.Profit;
                wagered = (decimal)tmp.Wagered;
                bets = (int)tmp.NumBets;
                wins = (int)tmp.Wins;
                losses = (int)tmp.Losses;
                UserID = tmp.Id;
                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(tmp.NumBets);
                Parent.updateLosses(tmp.Losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(tmp.Wins);
                //Parent.updateDeposit(tmp.DepositAddress);

                getHeaders = HttpWebRequest.Create("https://betking.io/account/GetCurrentSeed?gameType=0") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                prcSeed getseed = json.JsonDeserialize<prcSeed>(s1);
                client = getseed.ClientSeed;
                serverhash = getseed.ServerHash;

                try
                {
                    getHeaders = HttpWebRequest.Create("https://betking.io/account/getDepositAddress") as HttpWebRequest;
                    if (Prox != null)
                        getHeaders.Proxy = Prox;
                    getHeaders.CookieContainer = Cookies;
                    Response = (HttpWebResponse)getHeaders.GetResponse();
                    s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                    PRCDepost dep = json.JsonDeserialize<PRCDepost>(s1);
                    Parent.updateDeposit(dep.Address);
                }
                catch
                {
                    new System.Threading.Thread(GetDeposit).Start();
                }
                finishedlogin(true);
                return;
            }
            catch
            {
                finishedlogin(false);
                return;
            }
            finishedlogin(false);
        }

        void GetDeposit()
        {
            System.Threading.Thread.Sleep(10000);
            HttpWebRequest getHeaders = HttpWebRequest.Create("https://betking.io/account/getDepositAddress") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            getHeaders.CookieContainer = Cookies;
            HttpWebResponse Response = (HttpWebResponse)getHeaders.GetResponse();
            string s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            PRCDepost dep = json.JsonDeserialize<PRCDepost>(s1);
            Parent.updateDeposit(dep.Address);
        }

        string client = "", serverhash = "";
        public override bool Register(string Username, string Passwrd)
        {


            HttpWebRequest getHeaders = HttpWebRequest.Create("https://betking.io/bitcoindice/#account") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            var cookies = new CookieContainer();
            getHeaders.CookieContainer = cookies;
            getHeaders.CookieContainer.Add(new Cookie("PRC_Affiliate", "357", "/", "betking.io"));
            HttpWebResponse Response = null;
            string rqtoken = "";
            try
            {

                Response = (HttpWebResponse)getHeaders.GetResponse();
                string s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                string tmp = s1.Substring(s1.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                rqtoken = tmp.Substring(0, tmp.IndexOf("\""));
            }
            catch (WebException e)
            {
                return false;
            }
            CookieContainer tmpContainer = getHeaders.CookieContainer;


            foreach (Cookie c in Response.Cookies)
            {
                /*if (c.Name == "__RequestVerificationToken")
                    rqtoken = c.Value;*/
                Cookies.Add(c);
            }
            con.CookieContainer = Cookies;
            try
            {
                string s1 = "";
                getHeaders = HttpWebRequest.Create("https://betking.io/account/GetUserAccount") as HttpWebRequest;
                //getHeaders.CookieContainer.Add(new Cookie("PRC_Affiliate", "357", "/", "betking.io"));
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                string stmp = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                string sstmp = stmp.Substring(stmp.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                //s = rqtoken = sstmp.Substring(0, sstmp.IndexOf("\""));


                dicehub = con.CreateHubProxy("diceHub");
                con.Start().Wait();
                getHeaders = HttpWebRequest.Create("https://betking.io/account/SaveUserNameAndPassword") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = tmpContainer;
                foreach (Cookie c in Response.Cookies)
                {

                    getHeaders.CookieContainer.Add(c);
                }
                getHeaders.CookieContainer.Add(new Cookie("PRC_Affiliate", "357", "/", "betking.io"));
                System.Threading.Thread.Sleep(5000);
                getHeaders.Method = "POST";
                string post = string.Format("userName={0}&password={1}&confirmPassword={1}&__RequestVerificationToken={2}", Username, Passwrd, rqtoken);
                getHeaders.ContentType = "application/x-www-form-urlencoded";
                getHeaders.ContentLength = post.Length;
                using (var writer = new StreamWriter(getHeaders.GetRequestStream()))
                {
                    string writestring = post as string;
                    writer.Write(writestring);
                }
                try
                {

                    Response = (HttpWebResponse)getHeaders.GetResponse();
                    s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                    /*string tmp = s1.Substring(s1.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                    rqtoken = tmp.Substring(0, tmp.IndexOf("\""));*/
                }
                catch (WebException e)
                {
                    Response = (HttpWebResponse)e.Response;
                    s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                    return false;
                }

                dicehub.On<string, string, string, string, string, string>("receiveChatMessage", GotChatMessage);
                dicehub.On<PRCMYstats>("diceBetResult", DiceBetResult);
                dicehub.On<PRCResetSeed>("generateServerSeedResult", GenerateServerSeedResult);
                dicehub.On<PRCResetSeed>("saveClientSeedResult", SaveClientSeedResult);

                getHeaders = HttpWebRequest.Create("https://betking.io/account/GetUserAccount") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                //Response = (HttpWebResponse)getHeaders.GetResponse();
                //s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                //PRCUser tmp = json.JsonDeserialize<PRCUser>(s1);
                balance = 0;// (decimal)tmp.AvailableBalance;
                profit = 0;//(decimal)tmp.Profit;
                wagered = 0;//(decimal) tmp.Wagered;
                bets = 0;//(int)tmp.NumBets;
                wins = 0;//(int)tmp.Wins;
                losses = 0;//(int)tmp.Losses;
                UserID = 0;//tmp.Id;
                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                //Parent.updateDeposit(tmp.DepositAddress);
                /*getHeaders = HttpWebRequest.Create("https://betking.io/account/GetCurrentSeed") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                prcSeed getseed = json.JsonDeserialize<prcSeed>(s1);
                client = getseed.ClientSeed;
                serverhash = getseed.ServerHash;*/
                try
                {
                    getHeaders = HttpWebRequest.Create("https://betking.io/account/getDepositAddress") as HttpWebRequest;
                    if (Prox != null)
                        getHeaders.Proxy = Prox;
                    getHeaders.CookieContainer = Cookies;
                    Response = (HttpWebResponse)getHeaders.GetResponse();
                    s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                    PRCDepost dep = json.JsonDeserialize<PRCDepost>(s1);
                    Parent.updateDeposit(dep.Address);
                }
                catch
                {
                    new System.Threading.Thread(GetDeposit).Start();
                }
                finishedlogin(true);
                return true;
            }
            catch
            {
                finishedlogin(false);
                return false;
            }
        }
        int UserID = 0;
        public virtual decimal GetLucky(string server, string client, int nonce)
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
    }

    public class PRCLogin
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class PRCUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int NumBets { get; set; }
        public decimal Wagered { get; set; }
        public long Wins { get; set; }
        public long Losses { get; set; }
        public decimal Profit { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    public class prcSeed
    {
        public string ServerHash { get; set; }
        public string ClientSeed { get; set; }
    }

    public class prcDiceResult
    {

        public long Id { get; set; }
        public bool PlayerWin { get; set; }
        public decimal Amount { get; set; }
        public decimal Profit { get; set; }
        public decimal Roll { get; set; }
        public decimal Chance { get; set; }
        public int BetType { get; set; }
        public int Nonce { get; set; }
        public int UserAccountId { get; set; }
        public string BetDate { get; set; }
        public string ClientSeed { get; set; }
        public string ServerSeed { get; set; }
        public string UserName { get; set; }
    }

    public class PRCResetSeed
    {
        public bool Result { get; set; }
        public string CurrentServerHash { get; set; }
        public string CurrentClientSeed { get; set; }
        public int CurrentNonce { get; set; }
        public string PreviousServerHash { get; set; }
        public string PreviousServerSeed { get; set; }
        public string PreviousClientSeed { get; set; }
        public int PreviousNonce { get; set; }
        public string ClientSeed { get; internal set; }
    }
    public class PRCMYstats
    {
        public bool Success { get; set; }
        public Bet DiceBet { get; set; }
        public decimal AvailableBalance { get; set; }
        public int NumBets { get; set; }
        public decimal Wagered { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public decimal Profit { get; set; }
    }
    public class PRCDepost
    {
        public bool Success { get; set; }
        public string Address { get; set; }
    }
}
