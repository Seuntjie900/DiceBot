using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace DiceBot
{
    class PRC:DiceSite
    {
        HubConnection con = new HubConnection("https://pocketrocketscasino.eu/SignalR/", "", false);
        CookieContainer Cookies = new CookieContainer();
        IHubProxy dicehub;
        
        
        public PRC(cDiceBot Parent)
        {
            maxRoll = 99.9999;
            AutoWithdraw = true;
            AutoLogin = true;
            ChangeSeed = true;
            AutoInvest = true;
            Tip = true;
            this.Parent = Parent;
            Name = "PRCDice";
            BetURL = "https://pocketrocketscasino.eu/api/bets/GetBet?id=";
            con.Received += con_Received;
            SiteURL = "https://pocketrocketscasino.eu";
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
            ReceivedChatMessage(string.Format( "{0:hh:mm:ss} ({1}) <{2}> {3}",json.ToDateTime2(time), userid, user, message ));
        }


        void con_Received(string obj)
        {
            
        }

        protected override async void internalPlaceBet(bool High)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            int retries =0;
            while (retries < 2)
            {
                retries++;
                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
                try
                {
                    var tmpStats = await dicehub.Invoke<PRCMYstats>("Bet", High ? 0 : 1, amount, chance);

                    Bet tmp = tmpStats.DiceBet;
                    if (tmp.uid == UserID)
                    {
                        balance = (double)tmpStats.AvailableBalance;
                        wins = tmpStats.Wins;
                        losses = tmpStats.Losses;
                        wagered = (double)tmpStats.Wagered;
                        bets = tmpStats.NumBets;
                        profit = (double)tmpStats.Profit;

                        tmp.serverhash = serverhash;
                        retries = 5;
                        FinishedBet(tmp);
                    }
                }
                catch
                {
                    Parent.updateStatus(string.Format("Failed to place bets after 3 retries, stopping. Please check network and bot settings."));
                }
            }
        }

      
        public override bool Invest(double Amount)
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
      
        public override void ResetSeed()
        {
            
            HttpWebRequest getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/GenerateNewServerSeed") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            getHeaders.CookieContainer =Cookies;
            
            getHeaders.Method = "POST";
            string post = string.Format("__RequestVerificationToken="+ s);
            getHeaders.ContentType = "application/x-www-form-urlencoded";
            getHeaders.ContentLength = post.Length;
            using (var writer = new StreamWriter(getHeaders.GetRequestStream()))
            {
                string writestring = post as string;
                writer.Write(writestring);
            }
            try
            {

                HttpWebResponse Response = (HttpWebResponse)getHeaders.GetResponse();
                string s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                PRCResetSeed tmp = json.JsonDeserialize<PRCResetSeed>(s1);
                if (tmp.Success)
                {
                    sqlite_helper.InsertSeed(tmp.PreviousServerHash, tmp.PreviousServerSeed);
                    serverhash = tmp.CurrentServerHash;
                    client = tmp.CurrentClientSeed;
                }
                else
                {
                    Parent.updateStatus("Failed to reset seed, too soon.");
                }

            }
            catch
            {

            }
        }

        public override void Donate(double Amount)
        {
            SendTip("357", Amount);
        }

        public override void SendTip(string User, double amount)
        {
            
            int uid = -1;
            if (int.TryParse(User, out uid))
            {
                System.Threading.Thread.Sleep(3200);
                if (dicehub != null)
                    dicehub.Invoke("tip", uid, amount, "");
                System.Threading.Thread.Sleep(120);
            }
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }


        

        public override bool ReadyToBet()
        {

            return true;
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

        protected override bool internalWithdraw(double Amount, string Address)
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
            HttpWebRequest getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/play/#dice") as HttpWebRequest;
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
                string tmp = s1.Substring(s1.IndexOf("__RequestVerificationToken")+"__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                s = rqtoken = tmp.Substring(0, tmp.IndexOf("\""));
            }
            catch (WebException e)
            {
                System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                finishedlogin(false);
            }
            
            getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/login") as HttpWebRequest;
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
                if (!s1.ToLower().Contains("success"))
                {
                    System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                    finishedlogin(false);
                }
                /*string tmp = s1.Substring(s1.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                rqtoken = tmp.Substring(0, tmp.IndexOf("\""));*/
            }
            catch (WebException e)
            {
                Response = (HttpWebResponse)e.Response;
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                finishedlogin(false);
            }
            
            foreach (Cookie c in Response.Cookies)
            {
                if (c.Name == "__RequestVerificationToken")
                    rqtoken = c.Value;
                Cookies.Add(c);
            }
            Cookies.Add((new Cookie("PRC_Affiliate", "357", "/", "pocketrocketscasino.eu")));
            con.CookieContainer = Cookies;
            try
            {
                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/play/#dice") as HttpWebRequest;
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

                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/GetUserAccount") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                PRCUser tmp = json.JsonDeserialize<PRCUser>(s1);
                balance = (double)tmp.AvailableBalance;
                profit = (double)tmp.Profit;
                wagered = (double)tmp.Wagered;
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
                
                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/GetCurrentSeed") as HttpWebRequest;
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
                    getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/getDepositAddress") as HttpWebRequest;
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
            HttpWebRequest getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/getDepositAddress") as HttpWebRequest;
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


            HttpWebRequest getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/play/#account") as HttpWebRequest;
            if (Prox != null)
                getHeaders.Proxy = Prox;
            var cookies = new CookieContainer();
            getHeaders.CookieContainer = cookies;
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
                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/GetUserAccount") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                string stmp = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                string sstmp = stmp.Substring(stmp.IndexOf("__RequestVerificationToken") + "__RequestVerificationToken\" type=\"hidden\" value=\"".Length);
                //s = rqtoken = sstmp.Substring(0, sstmp.IndexOf("\""));
                

                dicehub = con.CreateHubProxy("diceHub");
                con.Start().Wait();
                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/SaveUserNameAndPassword") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = tmpContainer;
                foreach (Cookie c in Response.Cookies)
                {

                    getHeaders.CookieContainer.Add(c);
                }
                getHeaders.CookieContainer.Add(new Cookie("PRC_Affiliate", "357", "/", "pocketrocketscasino.eu"));
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
                
                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/GetUserAccount") as HttpWebRequest;
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                getHeaders.CookieContainer = Cookies;
                Response = (HttpWebResponse)getHeaders.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                PRCUser tmp = json.JsonDeserialize<PRCUser>(s1);
                balance = (double)tmp.AvailableBalance;
                profit = (double)tmp.Profit;
                wagered =(double) tmp.Wagered;
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
                getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/GetCurrentSeed") as HttpWebRequest;
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
                    getHeaders = HttpWebRequest.Create("https://pocketrocketscasino.eu/account/getDepositAddress") as HttpWebRequest;
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
        public bool Success { get; set; }
        public string CurrentServerHash { get; set; }
        public string CurrentClientSeed { get; set; }
        public int CurrentNonce { get; set; }
        public string PreviousServerHash { get; set; }
        public string PreviousServerSeed { get; set; }
        public string PreviousClientSeed { get; set; }
        public int PreviousNonce { get; set; }
    }
    public class PRCMYstats
    {
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
