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
        }

        public void GetBalanceThread()
        {

        }

        public override void Login(string Username, string Password)
        {
            Login(Username, Password,"");
        }
        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/auth/local");
                loginrequest.Method = "POST";
                string post = "username=" + Username + "&password=" + Password + "&code=" + twofa;
                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                
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
                    
                    loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));

                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDicegetUserInfo tmp1 = json.JsonDeserialize<SafeDicegetUserInfo>(sEmitResponse);
                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + tmp1.id + "/sites/1/me");
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "", "safedice.com"));
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDiceWalletInfo tmp2 = json.JsonDeserialize<SafeDiceWalletInfo>(sEmitResponse);



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

        void PlaceBetThread(bool High)
        {
            HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/auth/local");
            loginrequest.Method = "POST";
            SafeDiceBet tmpBet = new SafeDiceBet { siteId=1,
                amount=(int)(amount*100000000),
                payout= 99.5/chance,
                isFixedPayout = true,
                isRollLow = !High};
            string post = json.JsonSerializer<SafeDiceBet>(tmpBet);
            loginrequest.ContentLength = post.Length;
            loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            SafeDiceBetResult tmpResult = json.JsonDeserialize<SafeDiceBetResult>(sEmitResponse);
            Bet bet = new Bet();


            /*
             * normal bet result stuffies
             * update balance and stats stuffies
             * */
        }

        public override void PlaceBet(bool High)
        {
            throw new NotImplementedException();
        }

       
        public override void ResetSeed()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
        public int referralId { get; set; }
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
        public int amountWagered { get; set; }
        
    }
     public class SafeDiceBet
     {
         public int siteId { get; set; }
         public int amount { get; set; }
         public int target { get; set; }
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
}
