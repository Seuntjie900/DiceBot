using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiceBot
{
    class moneypot:DiceSite
    {
        Random R = new Random();
        public moneypot(cDiceBot Parent)
        {
            this.Parent = Parent;
            edge = 1;
            maxRoll = 99.99;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = false;
            Thread t = new Thread(new ThreadStart(GetBalanceThread));
            t.Start();
            SiteURL = "https://www.moneypot.com/oauth/authorize?app_id=492&response_type=token";
        }

        DateTime lastupdate = DateTime.Now;
        void GetBalanceThread()
        {
            while (ismp)
            {
                if (token != "" && token != null && (DateTime.Now-lastupdate).TotalSeconds > 30)
                {
                    lastupdate = DateTime.Now;
                    HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.moneypot.com/v1/auth?access_token=" + token);
                    if (Prox != null)
                        betrequest.Proxy = Prox;
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                    MPAuth tmp2 = json.JsonDeserialize<MPAuth>(sEmitResponse2);
                    this.balance = tmp2.user.balance / 100000000.0;
                    wagered = tmp2.user.betted_wager / 100000000.0;
                    profit = tmp2.user.betted_profit / 100000000.0;
                    bets = (int)tmp2.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateBet(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                }
                Thread.Sleep(100);
            }
        }

        string next = "";
        void placebetthread(object _High)
        {
            
            try
            {

                bool High = (bool)_High;
                int client = R.Next(0, int.MaxValue);
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.moneypot.com/v1/bets/simple-dice?access_token="+token);
                betrequest.CookieContainer = new CookieContainer();
                betrequest.CookieContainer.Add(new Cookie("sessionId", token, "/", "moneypot.com"));
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.Method = "POST";
                double tmpchance = High ? maxRoll - chance : chance;
                MPBetPlace betplace = new MPBetPlace {
                    client_seed = client,
                    cond = High ? ">" : "<",
                    hash=next,
                    payout = (((double)(100.0m - edge) / chance) * (amount * 100000000)),
                    target = tmpchance,
                    wager = amount * 100000000
                };
                string post = json.JsonSerializer<MPBetPlace>(betplace);
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/json";

                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                MPBet tmp = json.JsonDeserialize<MPBet>(sEmitResponse);

                Bet tmpBet = new Bet {
                Amount = (decimal)amount,
                date = DateTime.Now,
                Id = tmp.bet_id,
                Profit = (decimal)tmp.profit/100000000m,
                Roll = (decimal)tmp.outcome,
                high = High,
                Chance = (decimal)chance,
                nonce = 0,
                serverhash = next,
                serverseed = tmp.secret.ToString(),
                clientseed = client.ToString()
                };
                next = tmp.next_hash;
                //lastupdate = DateTime.Now;
                balance += tmp.profit / 100000000.0; //i assume
                bets++;
                bool Win = (((bool)tmpBet.high ? (decimal)tmpBet.Roll > (decimal)maxRoll - (decimal)(tmpBet.Chance) : (decimal)tmpBet.Roll < (decimal)(tmpBet.Chance)));
                if (Win)
                    wins++;
                else losses++;
                
                wagered +=amount;
                profit += (tmp.profit / 100000000.0);
                FinishedBet(tmpBet);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                }
                if (e.Message.Contains("429") || e.Message.Contains("502"))
                {
                    Thread.Sleep(200);
                    placebetthread(High);
                }


            }
        }

        protected override void internalPlaceBet(bool High)
        {
            Thread t = new Thread(new ParameterizedThreadStart(placebetthread));
            t.Start(High);
        }

        public override void ResetSeed()
        {
            HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.moneypot.com/v1/hashes?access_token=" + token);
            if (Prox != null)
                betrequest.Proxy = Prox;
            betrequest.Method = "POST";

            string post = "access_token="+token;
            betrequest.ContentLength = post.Length;
            betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(betrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
            string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
            MPSeed tmp = json.JsonDeserialize<MPSeed>(sEmitResponse2);
            next = tmp.hash;
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public void ShowMPWithdraw()
        {
            System.Diagnostics.Process.Start("https://www.moneypot.com/dialog/withdraw?app_id=492");
        }

        public void ShowMPDeposit()
        {
            System.Diagnostics.Process.Start("https://www.moneypot.com/dialog/deposit?app_id=492");
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            return false;
        }

        string token = "";
        public override void Login(string Username, string Password, string twofa)
        {
            if (Password == "")
            {
                System.Diagnostics.Process.Start(SiteURL);
                finishedlogin(false);
            }
            else
            {
                try
                {
                    token = Password;
                    lastupdate = DateTime.Now;
                    HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.moneypot.com/v1/auth?access_token=" + token);
                    if (Prox != null)
                        betrequest.Proxy = Prox;
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                    MPAuth tmp2 = json.JsonDeserialize<MPAuth>(sEmitResponse2);
                    this.balance = tmp2.user.balance / 100000000.0;
                    wagered = tmp2.user.betted_wager / 100000000.0;
                    profit = tmp2.user.betted_profit / 100000000.0;
                    bets = (int)tmp2.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateBet(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    ResetSeed();
                    finishedlogin(true);
                }
                catch
                {
                    finishedlogin(true);
                }
                
            }
        }

        public override bool Register(string username, string password)
        {
            if (password == "")
            {
                System.Diagnostics.Process.Start(SiteURL);
                finishedlogin(false);
                return false;
            }
            else
            {
                try
                {
                    token = password;
                    lastupdate = DateTime.Now;
                    HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.moneypot.com/v1/auth?access_token=" + token);
                    if (Prox != null)
                        betrequest.Proxy = Prox;
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                    MPAuth tmp2 = json.JsonDeserialize<MPAuth>(sEmitResponse2);
                    this.balance = tmp2.user.balance / 100000000.0;
                    wagered = tmp2.user.betted_wager / 100000000.0;
                    profit = tmp2.user.betted_profit / 100000000.0;
                    bets = (int)tmp2.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateBet(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    ResetSeed();
                    finishedlogin(true);
                    return true;
                }
                catch
                {
                    finishedlogin(true);
                    return false;
                }

            }
            
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        bool ismp = true;
        public override void Disconnect()
        {
            ismp = false;
            token = "";
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            
        }
    }

    public class MPUInfo
    {
        public string token { get; set; }
        public int expires_in { get; set; }
        public string expires_at { get; set; }
        public string kind { get; set; }
        public MPAuth auth { get; set; }
    }
    public class MPUser
    {
        public string uname { get; set; }
        public double balance { get; set; }
        public double unpaid { get; set; }
        public long betted_count { get; set; }
        public double betted_wager { get; set; }
        public double betted_ev { get; set; }
        public double betted_profit { get; set; }
        public string role { get; set; }
    }
    public class MPAuth
    {
        public int id { get; set; }
        public int app_id { get; set; }
        public MPUser user { get; set; }
    }
    public class MPBet
    {
        public int bet_id { get; set; }
        public double outcome { get; set; }
        public double profit { get; set; }
        public double secret { get; set; }
        public string salt { get; set; }
        public string next_hash { get; set; }
    }
    public class MPBetPlace
    {
        public int client_seed { get; set; }
        public string hash { get; set; }
        public string cond { get; set; }
        public double target { get; set; }
        public double payout { get; set; }        
        public double wager { get; set; }
    }
    public class MPSeed
    {
        public string hash { get; set; }
    }

}
