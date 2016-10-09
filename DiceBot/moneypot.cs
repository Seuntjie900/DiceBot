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
using System.Security.Cryptography;

namespace DiceBot
{
    class moneypot:DiceSite
    {
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.moneypot.com/v1/") };
        HttpClientHandler ClientHandlr;
        RandomNumberGenerator R = new  System.Security.Cryptography.RNGCryptoServiceProvider();
        public moneypot(cDiceBot Parent)
        {
            NonceBased = false;
            Name = "MoneyPot";
            this.Parent = Parent;
            edge = 0.9m;
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            Tip = true;
            TipUsingName = true;
            ChangeSeed = true;
            AutoLogin = false;
            
            SiteURL = "https://www.moneypot.com/oauth/authorize?app_id="+appid+"&response_type=token";
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
                        this.balance = tmp2.user.balance / 100000000.0m;
                        wagered = tmp2.user.betted_wager / 100000000.0m;
                        profit = tmp2.user.betted_profit / 100000000.0m;
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
                PlaceBetObj tmp9 = _High as PlaceBetObj;
                bool High = tmp9.High;
                decimal amount = tmp9.Amount;
                decimal chance = tmp9.Chance;
                byte[] bytes = new byte[4];
                R.GetBytes(bytes);
                long client = (long)BitConverter.ToUInt32(bytes, 0);
                decimal tmpchance = High ? maxRoll - chance : chance;
                MPBetPlace betplace = new MPBetPlace
                {
                    client_seed = client,
                    cond = High ? ">" : "<",
                    hash = next,
                    payout = (((decimal)(100.0m - edge) / chance) * (amount * 100000000)),
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
                    if (tmp.error.ToLower().Contains("invalid_hash") || tmp.error.ToLower().Contains("valid hash"))
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
                nonce = (int)(tmp.outcome*100),
                serverhash = next,
                serverseed = tmp.secret.ToString(),
                clientseed = client.ToString()
                };
                next = tmp.next_hash;
                //lastupdate = DateTime.Now;
                balance += tmp.profit / 100000000.0m; //i assume
                bets++;
                bool Win = (((bool)tmpBet.high ? (decimal)tmpBet.Roll > (decimal)maxRoll - (decimal)(tmpBet.Chance) : (decimal)tmpBet.Roll < (decimal)(tmpBet.Chance)));
                if (Win)
                    wins++;
                else losses++;
                
                wagered +=amount;
                profit += (tmp.profit / 100000000.0m);
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

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            Thread t = new Thread(new ParameterizedThreadStart(placebetthread));
            t.Start(new PlaceBetObj(High, amount, chance));
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

        public override bool InternalSendTip(string user, decimal tip)
        {
            MPTipSend Tip = new MPTipSend
            {
                uname = user,
                amount = tip * 100000000
            };

            HttpContent cont = new StringContent(json.JsonSerializer<MPTipSend>(Tip));
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            string Resp = "";
            using (var response = Client.PostAsync("tip?access_token=" + token, cont))
            {
                while (!response.IsCompleted)
                    Thread.Sleep(100);
                Resp = response.Result.Content.ReadAsStringAsync().Result;
            }

            MPTip tmp = json.JsonDeserialize<MPTip>(Resp);
            if (tmp.error != null)
            {
                if (tmp.error.ToLower().Contains("invalid amount"))
                {
                    MessageBox.Show("Invalid Tip Amount");
                    return false;
                }
                else
                {
                    Parent.updateStatus(tmp.error);
                }
                return false;
            }
            else
            {
                this.balance = this.balance - (tmp.amount / 100000000);
                Parent.updateBalance(balance);
                Parent.updateStatus("Sent " + (tmp.amount / 100000000).ToString("F8") + " to " + tmp.to);
                return true;
            }
            return false;
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

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            return false;
        }

        string token = "";
        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };
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
                    
                    this.balance = tmp2.user.balance / 100000000.0m;
                    wagered = tmp2.user.betted_wager / 100000000.0m;
                    profit = tmp2.user.betted_profit / 100000000.0m;
                    bets = (int)tmp2.user.betted_count;
                    Parent.updateBalance(balance);
                    Parent.updateBets(bets);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    ResetSeed();
                    ismp = true;
                    Thread t = new Thread(new ThreadStart(GetBalanceThread));
                    t.Start();
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

                    this.balance = tmp2.user.balance / 100000000.0m;
                    wagered = tmp2.user.betted_wager / 100000000.0m;
                    profit = tmp2.user.betted_profit / 100000000.0m;
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

        bool ismp = false;
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

        public override decimal GetLucky(string server, string client, int nonce)
        {
            long cl = long.Parse(client);
            decimal roll = Math.Floor((100.0m / 4294967296.0m) * (decimal)((long)(((decimal)nonce) + cl) % (long)(4294967296)) * (long)100) / (long)100;
            return roll;
        }
    }

    public class MPTipSend
    {
        public string uname { get; set; }
        public decimal amount { get; set; }
    }
    public class MPTip
    {
        public int id { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public decimal amount { get; set; }
        public string created_at { get; set; }
        public string error { get; set; }
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
        public decimal balance { get; set; }
        public decimal unpaid { get; set; }
        public long betted_count { get; set; }
        public decimal betted_wager { get; set; }
        public decimal betted_ev { get; set; }
        public decimal betted_profit { get; set; }
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
        public decimal outcome { get; set; }
        public decimal profit { get; set; }
        public decimal secret { get; set; }
        public string salt { get; set; }
        public string next_hash { get; set; }
        public string error { get; set; }
    }
    public class MPBetPlace
    {
        public long client_seed { get; set; }
        public string hash { get; set; }
        public string cond { get; set; }
        public decimal target { get; set; }
        public decimal payout { get; set; }        
        public decimal wager { get; set; }
    }
    public class MPSeed
    {
        public string hash { get; set; }
    }

}
