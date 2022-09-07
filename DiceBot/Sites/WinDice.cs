using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
using WDClasses;
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    class WinDice: DiceSite
    {
        string[] SiteA = new string[] { "https://windice.io", "https://windice1.io", "https://windice.link" };
        int site = 0;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        public static string[] cCurrencies = new string[] { "btc","eth","ltc","doge","bch","xrp","win" };
        DBRandom R = new DBRandom();
        public WinDice(cDiceBot Parent)
        {
            this.Parent = Parent;
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://windice.io/api/v1/api/getBet?hash=";

            this.Currencies = cCurrencies;
            Currency = "btc";            
            Name = "WinDice";
            Tip = false;
            TipUsingName = true;
            SiteURL = "https://windice.io/?r=08406hjdd";
            NonceBased = false;
            _PasswordText = "API Key";
        }
        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }

        public override void Disconnect()
        {
            ispd = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            string sitea = SiteA[site];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
      | SecurityProtocolType.Tls11
      | SecurityProtocolType.Tls12
      | SecurityProtocolType.Ssl3;
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri(sitea+"/api/v1/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("UserAgent", "DiceBot");
            Client.DefaultRequestHeaders.Add("Authorization", Password);
            try
            {
                if (getbalance())
                {
                    getstats();
                    getseed();
                    ispd = true;
                    lastupdate = DateTime.Now;
                        
                    new Thread(new ThreadStart(GetBalanceThread)).Start();
                    //lasthash = tmpblogin.server_hash;
                    finishedlogin(true);
                    return;
                }
                else
                {

                    if (++site < SiteA.Length)
                        Login(username, Password, twofa);
                    else
                    {   
                        this.finishedlogin(false);
                    }
                }
            }
            catch (Exception e)
            {
                if (++site < SiteA.Length)
                    Login(username, Password, twofa);
                else
                {
                    Parent.DumpLog(e.ToString(), -1);
                    this.finishedlogin(false);
                }
            }
        }
        WDGetSeed currentseed;
        bool getbalance()
        {
            try
            {
                string response = Client.GetStringAsync("user").Result;
                WDUserResponse tmpBalance = json.JsonDeserialize<WDUserResponse>(response);
                if (tmpBalance.data != null)
                {
                    PropertyInfo tmp = typeof(WDBalance).GetProperty(Currency.ToLower());
                    if (tmp != null)
                    {
                        decimal balance = (decimal)tmp.GetValue(tmpBalance.data.balance);
                        this.balance = balance;
                        Parent.updateBalance(balance);
                    }
                }
                return tmpBalance.status == "success";
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
            return false;
        }
        bool getstats()
        {
            try
            {
                string response = Client.GetStringAsync("stats").Result;
                WDStatsResponse tmpBalance = json.JsonDeserialize<WDStatsResponse>(response);
                if (tmpBalance.data != null)
                {
                    foreach (WDStatistic x in tmpBalance.data.statistics)
                    {
                        if (x.curr == Currency)
                        {
                            this.wagered = x.bet;
                            this.profit = x.profit;
                            Parent.updateWagered(wagered);
                            Parent.updateProfit(profit);

                            break;
                        }
                    }
                    this.bets = tmpBalance.data.stats.bets;
                    this.wins = tmpBalance.data.stats.wins;
                    this.losses = tmpBalance.data.stats.loses;
                    Parent.updateBets(bets);
                    Parent.updateWins(wins);
                    Parent.updateLosses(losses);
                }
                return tmpBalance.status == "success";
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
            return false;
        }
        bool getseed()
        {
            string response = Client.GetStringAsync("seed").Result;
            WDGetSeed tmpBalance = json.JsonDeserialize<WDGetSeed>(response);
            if (tmpBalance.data != null)
            {
                currentseed = tmpBalance.data;
            }
            return tmpBalance.status == "success";
        }
        void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if (((DateTime.Now - lastupdate).TotalSeconds > 30 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                           ForceUpdateStats = false;
                        getbalance();
                        getstats();
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }
                Thread.Sleep(1000);
            }
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void ResetSeed()
        {
            string seed = "";
            while (seed.Length<12)
            {
                seed += R.Next(0, int.MaxValue).ToString();
            }
            string loginjson = json.JsonSerializer<WDResetSeed>(new WDResetSeed()
            {
                value = seed
            }); 

            HttpContent cont = new StringContent(loginjson);
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage resp2 = Client.PostAsync("seed", cont).Result;

            if (resp2.IsSuccessStatusCode)
            {
                string response = resp2.Content.ReadAsStringAsync().Result;
                WDGetSeed tmpBalance = json.JsonDeserialize<WDGetSeed>(response);
                if (tmpBalance.status=="success")
                    getseed();
            }
            
        }

        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            SHA512 betgenerator = SHA512.Create();

            int charstouse = 5;
            

            List<byte> buffer = new List<byte>();
            string msg = server + client + nonce;
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
                    return lucky % 10000/100m;
            }
            return 0;
        }
        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chancem, string BetGuid)
        {
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chancem, BetGuid));
        }
        void PlaceBetThread(object BetObj)
        {
            PlaceBetObj betobj = BetObj as PlaceBetObj;
            decimal low = 0;
            decimal high = 0;
            if (betobj.High)
            {
                high = maxRoll*100;
                low = (maxRoll - chance) * 100+1;
            }
            else
            {
                high =chance*100-1;
                low = 0;
            }
            string loginjson = json.JsonSerializer<WDPlaceBet>(new WDPlaceBet()
            {
                curr = Currency,
                bet = betobj.Amount,
                game = "in",
                high = (int)high,
                low = (int)low
            });

            HttpContent cont = new StringContent(loginjson);
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage resp2 = Client.PostAsync("roll", cont).Result;

            if (resp2.IsSuccessStatusCode)
            {
                string response = resp2.Content.ReadAsStringAsync().Result;
                WDBet tmpBalance = json.JsonDeserialize<WDBet>(response);
                if (tmpBalance.status == "success")
                {
                    Bet Result = new Bet()
                    {
                        Amount = betobj.Amount,
                        date = DateTime.Now,
                        Chance = tmpBalance.data.chance,
                        clientseed = currentseed.client,
                        Currency = Currency,
                        Guid = betobj.Guid,
                        Id = tmpBalance.data.hash,
                        high = betobj.High,
                        nonce = tmpBalance.data.nonce,
                        Profit = tmpBalance.data.win - tmpBalance.data.bet,
                        Roll = tmpBalance.data.result / 100m,
                        serverhash = currentseed.hash
                    };
                    this.bets++;
                    bool Win = (((bool)High ? (decimal)Result.Roll > (decimal)maxRoll - (decimal)(chance) : (decimal)Result.Roll < (decimal)(chance)));
                    if (Win)
                        wins++;
                    else losses++;
                    wagered += amount;
                    profit += Result.Profit;
                    balance += Result.Profit;
                    FinishedBet(Result);
                }
                else
                {
                    Parent.updateStatus(tmpBalance.message);
                }
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }

       
    }
}
namespace WDClasses
{
    public class WDBaseResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }

    public class WDUserResponse : WDBaseResponse
    {
        public WDUserResponse data { get; set; }
        public string hash { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public int rating { get; set; }
        public int reg_time { get; set; }
        public bool hide_profit { get; set; }
        public bool hide_bet { get; set; }
        public WDBalance balance { get; set; }
    }
    public class WDBalance
    {
        public decimal btc { get; set; }
        public decimal eth { get; set; }
        public decimal ltc { get; set; }
        public decimal doge { get; set; }
        public decimal bch { get; set; }
        public decimal xrp { get; set; }
        public decimal win { get; set; }
    }
    public class WDPlaceBet
    {
        public string curr { get; set; }
        public decimal bet { get; set; }
        public string game { get; set; }
        public int low { get; set; }
        public int high { get; set; }
    }
    public class WDBet : WDBaseResponse
    {
        public WDBet data { get; set; }
        public string hash { get; set; }
        public string userHash { get; set; }
        public string username { get; set; }
        public int nonce { get; set; }
        public string curr { get; set; }
        public decimal bet { get; set; }
        public decimal win { get; set; }
        public decimal jackpot { get; set; }
        public decimal pointLow { get; set; }
        public decimal pointHigh { get; set; }
        public string game { get; set; }
        public decimal chance { get; set; }
        public decimal payout { get; set; }
        public decimal result { get; set; }
        public decimal time { get; set; }
        public bool isHigh { get; set; }
    }
    public class WDStatistic
    {
        public string curr { get; set; }
        public decimal bet { get; set; }
        public decimal profit { get; set; }
    }

    public class WDStats
    {
        public int wins { get; set; }
        public int loses { get; set; }
        public int bets { get; set; }
        public int chat { get; set; }
        public int online { get; set; }
    }

    public class WDStatsResponse : WDBaseResponse
    {
        public WDStatsResponse data { get; set; }
        public WDStatistic[] statistics { get; set; }
        public WDStats stats { get; set; }
    }
    public class WDGetSeed : WDBaseResponse
    {
        public WDGetSeed data { get; set; }
        public string client { get; set; }
        public string hash { get; set; }
        public string newHash { get; set; }
        public int nonce { get; set; }
    }
    public class WDResetSeed
    {
        public string value { get; set; }

    }
    public class WDResetResult : WDBaseResponse
    {
        public WDResetResult data { get; set; }
        public string client { get; set; }
        public string hash { get; set; }
        public long nonce { get; set; }
    }
}