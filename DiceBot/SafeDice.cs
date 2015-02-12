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

        public override bool Login(string Username, string Password)
        {
            return base.Login(Username, Password);
        }
        public override bool Login(string Username, string Password, string twofa)
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
                pdlogin tmp = json.JsonDeserialize<pdlogin>(sEmitResponse);
                accesstoken = tmp.access_token;
                if (accesstoken == "")
                    return false;
                else
                {
                    /*HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/users/1?access_token=" + accesstoken);
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();

                    pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                    balance = tmpu.user.balance; //i assume
                    bets = tmpu.user.bets;
                    Parent.updateBalance((decimal)(balance / 100000000.0));
                    Parent.updateBets(tmpu.user.bets);
                    Parent.updateLosses(tmpu.user.losses);
                    Parent.updateProfit(tmpu.user.profit / 100000000m);
                    Parent.updateWagered(tmpu.user.wagered / 100000000m);
                    string s = tmpu.user.address;
                    if (s == null)
                    {
                        s = getDepositAddress();
                    }
                    if (s != null)
                    {
                        Parent.updateDeposit(s);
                    }
                    Parent.updateWins(tmpu.user.wins);
                    lastupdate = DateTime.Now;
                    System.Windows.Forms.MessageBox.Show("Logged in!\n\nWelcome " + Username);
                    Parent.updateStatus("Logged in! Welcome " + Username);*/
                    return true;
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
                return false;
            }
        }

        void PlaceBetThread(bool High)
        {

        }

        public override void PlaceBet(bool High)
        {
            throw new NotImplementedException();
        }

        public override void SetChance(string Chance)
        {
            throw new NotImplementedException();
        }

        public override void SetAmount(double Amount)
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

        public override string GetbalanceValue()
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
            throw new NotImplementedException();
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }
    }

    public class SafeDiceLogin
    {
        public string token { get; set; }
    }
}
