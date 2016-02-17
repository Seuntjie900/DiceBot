using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace DiceBot.Sites
{
    class moneypot:DiceSite
    {
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.moneypot.com/v1/") };
        HttpClientHandler ClientHandlr;
        Random R = new Random();
        public moneypot(cDiceBot Parent)
        {
            this.Parent = Parent;
            edge = 0.9m;
            maxRoll = 99.99;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = false;
            Thread t = new Thread(new ThreadStart(GetBalanceThread));
            t.Start();
            SiteURL = "https://www.moneypot.com/oauth/authorize?app_id="+appid+"&response_type=token";
            Name = "Moneypot";
        }
       protected int appid = 492;
        DateTime lastupdate = DateTime.Now;
        void GetBalanceThread()
        {
            while (ismp)
            {
                if (token != "" && token != null && (DateTime.Now-lastupdate).TotalSeconds > 30)
                {
                    lastupdate = DateTime.Now;
                    try
                    {
                        string s = Client.GetStringAsync("auth?access_token=" + token).Result;
                        MPAuth tmp2 = json.JsonDeserialize<MPAuth>(s);
                        this.balance = tmp2.user.balance / 100000000.0;
                        wagered = tmp2.user.betted_wager / 100000000.0;
                        profit = tmp2.user.betted_profit / 100000000.0;
                        bets = (int)tmp2.user.betted_count;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);

                    }
                    catch (AggregateException e)
                    { 

                    }
                    catch (Exception e)
                    {
                        
                    }
                    
                }
                Thread.Sleep(100);
            }
        }

        string next = "";
        int retrycount = 0;
        void placebetthread(object _High)
        {
            
            try
            {

                bool High = (bool)_High;
                int client = R.Next(0, int.MaxValue);
                double tmpchance = High ? maxRoll - chance : chance;
                MPBetPlace betplace = new MPBetPlace
                {
                    client_seed = client,
                    cond = High ? ">" : "<",
                    hash = next,
                    payout = (((double)(100.0m - edge) / chance) * (amount * 100000000)),
                    target = tmpchance,
                    wager = amount * 100000000
                };
                
                HttpContent cont = new StringContent(json.JsonSerializer<MPBetPlace>(betplace));
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Resp = "";
                using (var response = Client.PostAsync("bets/simple-dice?access_token=" + token, cont))
                {
                    Resp = response.Result.Content.ReadAsStringAsync().Result;

                }
                
                
                MPBet tmp = json.JsonDeserialize<MPBet>(Resp);
                if (tmp.error != null)
                {
                    if (tmp.error.ToLower() == "you did not provide a valid hash")
                    {
                        ResetSeed();
                        placebetthread(High);
                        return;
                    }
                    else
                    {
                        Parent.updateStatus(tmp.error);
                    }
                    return;
                }


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
                retrycount = 0;
                FinishedBet(tmpBet);
            }
            catch (AggregateException e)
            {
                if (retrycount++ <3)
                {
                    placebetthread(High);
                    return;
                }
                if (e.InnerException.InnerException.Message.Contains("tsl/ssl") || e.InnerException.Message.Contains("502"))
                {
                    Thread.Sleep(200);
                    placebetthread(High);
                }


            }
            catch (Exception e2)
            {

            }
        }

        protected override void internalPlaceBet(bool High)
        {
            Thread t = new Thread(new ParameterizedThreadStart(placebetthread));
            t.Start(High);
        }

        public override void ResetSeed()
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("access_token", token));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string res = Client.PostAsync("hashes?access_token=" + token, Content).Result.Content.ReadAsStringAsync().Result;

            MPSeed tmp = json.JsonDeserialize<MPSeed>(res);
            next = tmp.hash;
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public void ShowMPWithdraw()
        {
            System.Diagnostics.Process.Start("https://www.moneypot.com/dialog/withdraw?app_id="+appid+"");
        }

        public void ShowMPDeposit()
        {
            System.Diagnostics.Process.Start("https://www.moneypot.com/dialog/deposit?app_id="+appid+"");
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            return false;
        }

        string token = "";
        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip };;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://api.moneypot.com/v1/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
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
                    lastupdate = DateTime.Now;
                    string s = Client.GetStringAsync("auth?access_token=" + token).Result;

                    MPAuth tmp2 = json.JsonDeserialize<MPAuth>(s);
                    
                    this.balance = tmp2.user.balance / 100000000.0;
                    wagered = tmp2.user.betted_wager / 100000000.0;
                    profit = tmp2.user.betted_profit / 100000000.0;
                    bets = (int)tmp2.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateBets(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    ResetSeed();
                    finishedlogin(true);
                }
                catch
                {
                    finishedlogin(false);
                }
                
            }
        }

        public override bool Register(string username, string Password)
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
                    lastupdate = DateTime.Now;
                    string s = Client.GetStringAsync("auth?access_token=" + token).Result;

                    MPAuth tmp2 = json.JsonDeserialize<MPAuth>(s);

                    this.balance = tmp2.user.balance / 100000000.0;
                    wagered = tmp2.user.betted_wager / 100000000.0;
                    profit = tmp2.user.betted_profit / 100000000.0;
                    bets = (int)tmp2.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateBets(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    ResetSeed();
                    finishedlogin(true);
                    return true;
                }
                catch
                {
                    finishedlogin(false);
                    return false;
                }

            }
            return true;

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
        public string error { get; set; }
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
