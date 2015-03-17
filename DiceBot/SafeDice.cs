using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
namespace DiceBot
{
    class SafeDice: DiceSite
    {
        string accesstoken = "";

        bool ispd = true;
        public SafeDice(cDiceBot Parent)
        {
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = false;
            BetURL = "https://safedice.com/bets/";
            Thread t = new Thread(GetBalanceThread);
            t.Start();
            this.Parent = Parent;
            Name = "SafeDice";
            edge = 0.5m;
        }

        public void GetBalanceThread()
        {

        }

        public override void Login(string Username, string Password)
        {
            Login(Username, Password,"");
        }
        string serverhash = "";
        string client = "";
        int wins = 0;
        int losses = 0;
        double wagered = 0;
        int nonce = 0;
        int UID = 0;
        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/auth/local");
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.Method = "POST";
                string post = "username=" + Username + "&password=" + Password + "&code=" + twofa;
                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                SafeDiceLogin tmp = json.JsonDeserialize<SafeDiceLogin>(sEmitResponse);
                accesstoken = tmp.token;
                if (accesstoken == "")
                        finishedlogin(false);
                else
                {
                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/me?token=" + accesstoken);
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDicegetUserInfo tmp1 = json.JsonDeserialize<SafeDicegetUserInfo>(sEmitResponse);
                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + tmp1.id + "/sites/1/me");
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDiceWalletInfo tmp2 = json.JsonDeserialize<SafeDiceWalletInfo>(sEmitResponse);
                    Parent.updateBalance(tmp2.balance / 100000000m);
                    balance = tmp2.balance / 100000000.0;
                    
                    Parent.updateBets(tmp2.win + tmp2.lose);                    
                    Parent.updateLosses(tmp2.lose);
                    wins = tmp2.win;
                    losses=tmp2.lose;
                    Parent.updateProfit((tmp2.amountWin - tmp2.amountLose) / 100000000.0);
                    Parent.updateWagered(tmp2.wagered / 100000000.0);
                    wagered = tmp2.wagered / 100000000.0;
                    Parent.updateWins(tmp2.win);
                    Parent.updateStatus("Logged in");
                    serverhash = tmp1.serverSeedHash;
                    client = tmp1.accountSeed;
                    nonce = tmp1.nonce;
                    UID = tmp1.id;
                    finishedlogin(true);
                }
                
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("401"))
                    {
                        System.Windows.Forms.MessageBox.Show("Could not log in. Please ensure the username, passowrd and 2fa code are all correct.");
                    }

                }
                finishedlogin(false);

            }
        }

        void PlaceBetThread(object High)
        {
            try
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/dicebets");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.Method = "POST";
                SafeDiceBet tmpBet = new SafeDiceBet
                {
                    siteId = 1,
                    amount = (int)(amount * 100000000),
                    payout = (double)(((int)((99.5 / chance) * 100000000)) / 100000000.0),
                    isFixedPayout = false,
                    isRollLow = !(bool)High,
                    target = ((bool)High) ? (999999 - ((int)(chance * 10000))).ToString() : ((int)(chance * 10000)).ToString()
                };
                string post = json.JsonSerializer<SafeDiceBet>(tmpBet);
                betrequest.Accept = "application/json, text/plain, */*";
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/json, text/plain, */*";
                betrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                betrequest.CookieContainer = new CookieContainer();
                betrequest.CookieContainer.Add(new Cookie("siteId", "1", "/", "safedice.com"));
                betrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                string s = json.JsonSerializer<SDDiceBetCookie>(new SDDiceBetCookie 
                { 
                    autoRollLossMultiplier=1, 
                    isFixedPayout =false,
                    isHotKeysEnabled=false,
                    isRollLow= !(bool)High,
                    pChance=chance,
                    showAutoRoll=false
                });
                s = s.Replace("{", "%7B").Replace("", "").Replace("", "").Replace("", "").Replace("", "");
                betrequest.CookieContainer.Add(new Cookie("diceBet", s ,"/","safedice.com"));
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                    writer.Flush();
                    writer.Close();
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                SafeDiceBetResult tmpResult = json.JsonDeserialize<SafeDiceBetResult>(sEmitResponse);
                Bet bet = new Bet();
                bet.Amount = (decimal)tmpResult.amount * 100000000m;
                bet.date = tmpResult.processTime;
                bet.Chance = (decimal)tmpResult.target / 970000m * 100m;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("401"))
                    {
                        System.Windows.Forms.MessageBox.Show("Could not log in. Please ensure the username, passowrd and 2fa code are all correct.");
                    }

                }
            }


            /*
             * normal bet result stuffies
             * update balance and stats stuffies
             * */
        }

        public override void PlaceBet(bool High)
        {
            Thread t = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            t.Start(High);

        }

       
        public override void ResetSeed()
        {
            HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/randomizeseed");
            if (Prox != null)
                loginrequest.Proxy = Prox;
            loginrequest.Method = "GET";
            
            loginrequest.Accept = "application/json, text/plain, */*";
            
            loginrequest.ContentType = "application/json, text/plain, */*";
            loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
            loginrequest.CookieContainer = new CookieContainer();
            loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
            HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

            SDRandomize tmp = json.JsonDeserialize<SDRandomize>(sEmitResponse);
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

       
        public override string GetSiteProfitValue()
        {
            throw new NotImplementedException();
        }

        public override string GetTotalBets()
        {
            throw new NotImplementedException();
        }

        public override string GetMyProfit()
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
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override bool Withdraw(double Amount, string Address)
        {
            try
            {
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/"+ UID +"/sites/1/withdraw");
                if (Prox != null)
                    loginrequest.Proxy = Prox;

                loginrequest.Method = "PUT";

                loginrequest.Accept = "application/json, text/plain, */*";

                loginrequest.ContentType = "application/json, text/plain, */*";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                loginrequest.CookieContainer = new CookieContainer();
                loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                string post = json.JsonSerializer<SDSendWIthdraw>(new SDSendWIthdraw { amount = (int)(Amount * 100000000), address = Address });
                
                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                    writer.Flush();
                    writer.Close();
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                
                SDRandomize tmp = json.JsonDeserialize<SDRandomize>(sEmitResponse);
                balance -= Amount;
                Parent.updateBalance(balance);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }
    }

    public class SafeDiceLogin
    {
        public string token { get; set; }
    }


    public class SafeDicegetUserInfo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string authHashLink { get; set; }
        //public int referralId { get; set; }
        public string accountSeed { get; set; }
        public string serverSeedHash { get; set; }
        public int nonce { get; set; }
        public int role { get; set; }
        public bool isInvestmentEnabled { get; set; }

    }

    public class SafeDiceWalletInfo
    {
        public int balance { get; set; }
        public double shares { get; set; }
        public double kelly { get; set; }
        public int win { get; set; }
        public int lose { get; set; }
        public int amountLose { get; set; }
        public int amountWin { get; set; }
        public int wagered { get; set; }
        
    }
     public class SafeDiceBet
     {
         public int siteId { get; set; }
         public int amount { get; set; }
         public string target { get; set; }
         public double payout { get; set; }
         public bool isFixedPayout { get; set; }
         public bool isRollLow { get; set; }
     }
    public class SafeDiceBetResult
    {
        public int id { get; set; }
        public int accountId { get; set; }
        public DateTime processTime { get; set; }
        public int amount { get; set; }
        public int profit { get; set; }
        public int roll { get; set; }
        public int target { get; set; }
        public bool isRollLow { get; set; }
        public double payout { get; set; }
    }

    public class SDRandomize
    {
        public string serverSeedHash { get; set; }
    }
    public class SDSendWIthdraw
    {
        public int amount { get; set; }
        public string address { get; set; }
    }

    public class SDDiceBetCookie
    {
        public bool isRollLow { get; set; }
        public double pChance { get; set; }
        public bool isHotKeysEnabled { get; set; }
        public bool isFixedPayout { get; set; }
        public bool showAutoRoll { get; set; }
        public double autoRollLossMultiplier { get; set; }
    }
}
