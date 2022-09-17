using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
using DiceBot.WolfBetClasses;
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    class WolfBet : DiceSite
    {
        string accesstoken = "";        
        public bool ispd = false;
        DateTime lastupdate = new DateTime();
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        public static string[] cCurrencies = new string[] { "btc", "eth", "ltc", "trx", "bch","doge","xrp","usdt","etc","sushi","uni","xlm","shib","bnb","ada","dot" };
        string URL = "https://wolf.bet";
        public WolfBet(cDiceBot Parent)
        {
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://wolf.bet/";
            _PasswordText = "API Key";
            this.Currencies = cCurrencies;
            Currency = "btc";
            this.Parent = Parent;
            Name = "WolfBet";
            Tip = false;
            TipUsingName = true;
            SiteURL = "https://wolf.bet?c=Seuntjie";
            NonceBased = true;
            if (File.Exists("wolf.txt"))
            {
                URL = File.ReadAllText("wolf.txt").TrimEnd(new char[]{ ' ', '\r', '\n', '\t' });
            }
            else if (File.Exists("wolf"))
            {
                URL = File.ReadAllText("wolf.txt").TrimEnd(new char[] { ' ', '\r', '\n', '\t' });
            }
        }

        public override void Disconnect()
        {
            this.ispd = false;
        }
        protected override void CurrencyChanged()
        {
            base.CurrencyChanged();
            ForceUpdateStats = true;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
      | SecurityProtocolType.Tls11
      | SecurityProtocolType.Tls12
      | SecurityProtocolType.Ssl3;
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri(URL+"/api/v1/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            Client.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36");
            Client.DefaultRequestHeaders.Add("Origin", "https://wolf.bet");
            Client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            try
            {                
                if (Password!=null)
                {
                    Client.DefaultRequestHeaders.Add("authorization", "Bearer " + Password);
                }
                string sEmitResponse = Client.GetStringAsync("user/balances").Result;
                try
                {
                    WolfBetProfile tmpProfile = json.JsonDeserialize<WolfBetProfile>(sEmitResponse);
                    if (tmpProfile.balances!= null)
                    {
                        //set balance here
                        foreach (Balance x in tmpProfile.balances)
                        {
                            if (x.currency.ToLower() == Currency.ToLower())
                            {
                                this.balance = decimal.Parse(x.amount, System.Globalization.NumberFormatInfo.InvariantInfo);
                                Parent.updateBalance(balance);
                            }
                        }
                        //get stats
                        //set stats
                        sEmitResponse = Client.GetStringAsync("user/stats/bets").Result;
                        WolfBetStats tmpStats = json.JsonDeserialize<WolfBetStats>(sEmitResponse);
                        UpdateStats(tmpStats);
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(new ThreadStart(GetBalanceThread)).Start();
                        this.finishedlogin(true);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                    Parent.DumpLog(sEmitResponse, -1);
                    Parent.updateStatus("Error: " + sEmitResponse);
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
            this.finishedlogin(false);
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
                        string  sEmitResponse = Client.GetStringAsync("user/balances").Result;
                        WolfBetProfile tmpProfile = json.JsonDeserialize<WolfBetProfile>(sEmitResponse);
                        if (tmpProfile.balances != null)
                        {
                            //set balance here
                            foreach (Balance x in tmpProfile.balances)
                            {
                                if (x.currency.ToLower() == Currency.ToLower())
                                {
                                    this.balance = decimal.Parse(x.amount, System.Globalization.NumberFormatInfo.InvariantInfo);
                                    Parent.updateBalance(balance);
                                }
                            }
                            //get stats
                            //set stats
                            sEmitResponse = Client.GetStringAsync("user/stats/bets").Result;
                            WolfBetStats tmpStats = json.JsonDeserialize<WolfBetStats>(sEmitResponse);
                            UpdateStats(tmpStats);
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(),-1);
                }
                Thread.Sleep(100);
            }
        }


        void UpdateStats(WolfBetStats Stats)
        {
            try
            {
                PropertyInfo tmp = typeof(Dice).GetProperty(Currency.ToLower());
                if (tmp != null)
                {
                    WBStat stat = tmp.GetValue(Stats.dice) as WBStat;
                    if (stat != null)
                    {
                        this.bets = int.Parse(stat.total_bets);
                        this.wins = int.Parse(stat.win);
                        this.losses = int.Parse(stat.lose);
                        this.wagered = decimal.Parse(stat.waggered, System.Globalization.NumberFormatInfo.InvariantInfo);
                        this.profit = decimal.Parse(stat.profit, System.Globalization.NumberFormatInfo.InvariantInfo);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateWagered(wagered);
                        Parent.updateProfit(profit);
                    }
                }

            }
            catch
            {
                this.bets = 0;
                this.wins = 0;
                this.losses = 0;
                this.wagered =0;
                this.profit = 0;
                Parent.updateBets(bets);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
                Parent.updateWagered(wagered);
                Parent.updateProfit(profit);
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
            try
            {
                string Result = Client.GetAsync("game/seed/refresh").Result.Content.ReadAsStringAsync().Result;
                HttpContent cont = new StringContent("{\"game\":\"dice\"}");
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Result = Client.PostAsync("game/seed/refresh", cont).Result.Content.ReadAsStringAsync().Result;
                
            }
            catch
            {

            }
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
            this.High = High;
            new Thread(new ParameterizedThreadStart(placebetthread)).Start(new PlaceBetObj(High, amount, chance, BetGuid));
        }

        private void placebetthread(object obj)
        {
            try
            {
                PlaceBetObj tmp5 = obj as PlaceBetObj;
                decimal tmpchance = Math.Round(tmp5.Chance, 2);
                WolfPlaceBet tmp = new WolfPlaceBet
                {
                    amount = tmp5.Amount.ToString("0.00000000",System.Globalization.NumberFormatInfo.InvariantInfo),
                    currency = Currency,
                    rule = tmp5.High ? "over" : "under",
                    multiplier = ((100m - edge) / tmpchance).ToString("0.####",System.Globalization.NumberFormatInfo.InvariantInfo),
                    bet_value = (High ? maxRoll - tmpchance : tmpchance).ToString("0.##",System.Globalization.NumberFormatInfo.InvariantInfo),
                    game = "dice"
                };
                string LoginString = json.JsonSerializer<WolfPlaceBet>(tmp);
                HttpContent cont = new StringContent(LoginString);
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage resp2 = Client.PostAsync("bet/place", cont).Result;

                if (resp2.IsSuccessStatusCode)
                {

                }
                string sEmitResponse = resp2.Content.ReadAsStringAsync().Result;
                try
                {
                    WolfBetResult result = json.JsonDeserialize<WolfBetResult>(sEmitResponse);
                    if (result.bet != null)
                    {
                        Bet tmpRsult = new Bet()
                        {
                            Amount = decimal.Parse(result.bet.amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                            Chance = tmp5.High ? maxRoll - decimal.Parse(result.bet.bet_value, System.Globalization.NumberFormatInfo.InvariantInfo): decimal.Parse(result.bet.bet_value, System.Globalization.NumberFormatInfo.InvariantInfo),
                            clientseed = result.bet.user_seed,
                            date = DateTime.Now,
                            Currency = Currency,
                            Guid = tmp5.Guid,
                            nonce = result.bet.nonce,
                            Id = result.bet.hash,
                            high = tmp5.High,
                            Roll = decimal.Parse(result.bet.result_value, System.Globalization.NumberFormatInfo.InvariantInfo),
                            Profit = decimal.Parse(result.bet.profit, System.Globalization.NumberFormatInfo.InvariantInfo),
                            serverhash = result.bet.server_seed_hashed
                        };
                        bets++;
                        bool Win = (((bool)High ? tmpRsult.Roll > (decimal)maxRoll - (decimal)(tmpRsult.Chance) : (decimal)tmpRsult.Roll < (decimal)(tmpRsult.Chance)));
                        if (Win)
                            wins++;
                        else losses++;
                        wagered += amount;
                        profit += tmpRsult.Profit;
                        this.balance = result.userBalance.amount;
                        FinishedBet(tmpRsult);
                    }
                    else
                    {
                        Parent.DumpLog(sEmitResponse, -1);
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                    Parent.DumpLog(sEmitResponse,-1);
                    Parent.updateStatus("Error: "+ sEmitResponse);
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);
            }
        }

        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            HMACSHA256 betgenerator = new HMACSHA256();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

           
            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */client + "_" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            betgenerator.Key = buffer.ToArray();

            byte[] hash = betgenerator.ComputeHash(serverb.ToArray());

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

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            string x = $"{{\"amount\":\"{Amount.ToString(System.Globalization.NumberFormatInfo.InvariantInfo)}\",\"currency\":\"{Currency}\",\"address\":\"{Address}\"}}";
            HttpContent cont = new StringContent(x);
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage resp2 = Client.PostAsync("bet/place", cont).Result;

            return resp2.IsSuccessStatusCode;
            
        }
    }
   
}
namespace DiceBot.WolfBetClasses
{
    public class WolfBetLogin
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }
    public class Preferences
    {
        public bool public_total_profit { get; set; }
        public bool public_total_wagered { get; set; }
        public bool public_bets { get; set; }
    }

    public class Balance
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string withdraw_fee { get; set; }
        public string withdraw_minimum_amount { get; set; }
        public bool payment_id_required { get; set; }
    }

    public class Game2
    {
        public string name { get; set; }
    }

    public class Game
    {
        public string server_seed_hashed { get; set; }
        public Game2 game { get; set; }
    }

    public class User
    {
        public string login { get; set; }
        public string email { get; set; }
        public bool two_factor_authentication { get; set; }
        public bool has_email_to_verify { get; set; }
        public string last_nonce { get; set; }
        public string seed { get; set; }
        public string channel { get; set; }
        public string joined { get; set; }
       
        public List<Balance> balances { get; set; }
        public List<Game> games { get; set; }
    }

  

    public class History
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public int step { get; set; }
        public long published_at { get; set; }
    }

    public class Values
    {
        public string btc { get; set; }
        public string eth { get; set; }
        public string ltc { get; set; }
        public string doge { get; set; }
        public string trx { get; set; }
        public string bch { get; set; }
        public string xrp { get; set; }
        public string usdt { get; set; }
        public string etc { get; set; }
        public string sushi { get; set; }
        public string uni { get; set; }
        public string xlm { get; set; }
    }

    public class Next
    {
        public decimal step { get; set; }
        public Values values { get; set; }
    }

    public class DailyStreak
    {
        public List<History> history { get; set; }
        public Next next { get; set; }
    }

    public class WolfBetProfile
    {
        public User user { get; set; }
        public List<Balance> balances { get; set; }

    }
    public class WBStat
    {
        public string total_bets { get; set; }
        public string win { get; set; }
        public string lose { get; set; }
        public string waggered { get; set; }
        public string currency { get; set; }
        public string profit { get; set; }
    }

    public class Dice
    {
        public WBStat doge { get; set; }
        public WBStat btc { get; set; }
        public WBStat eth { get; set; }
        public WBStat ltc { get; set; }
        public WBStat trx { get; set; }
        public WBStat bch { get; set; }
        public WBStat xrp { get; set; }
        public WBStat usdt { get; set; }
        public WBStat etc { get; set; }
        public WBStat sushi { get; set; }
        public WBStat uni { get; set; }
        public WBStat xlm { get; set; }
    }

    public class WolfBetStats
    {
        public Dice dice { get; set; }
    }

    public class WolfPlaceBet
    {
        public string currency { get; set; }
        public string game { get; set; }
        public string amount { get; set; }
        public string rule { get; set; }
        public string multiplier { get; set; }
        public string bet_value { get; set; }
    }
    public class WBBet
    {
        public string hash { get; set; }
        public int nonce { get; set; }
        public string user_seed { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string profit { get; set; }
        public string multiplier { get; set; }
        public string bet_value { get; set; }
        public string result_value { get; set; }
        public string state { get; set; }
        public int published_at { get; set; }
        public string server_seed_hashed { get; set; }
        public User user { get; set; }
        public Game game { get; set; }
    }

    public class UserBalance
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string withdraw_fee { get; set; }
        public string withdraw_minimum_amount { get; set; }
        public bool payment_id_required { get; set; }
    }

    public class WolfBetResult
    {
        public WBBet bet { get; set; }
        public UserBalance userBalance { get; set; }
    }
}
