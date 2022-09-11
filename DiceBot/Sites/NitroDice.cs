using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    class NitroDice : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        DBRandom r = new DBRandom();
        public static string[] sCurrencies = new string[] { "Bch", "Btc", "Doge" };
        public NitroDice(cDiceBot Parent)
        {

            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://NitroDice.com/bets/";
            Currencies = sCurrencies;
            this.Parent = Parent;
            Name = "NitroDice";
            Tip = false;
            TipUsingName = true;
            NonceBased = false;
            SiteURL = "http://www.nitrodice.com/?ref=EEqWBD442qC2oxjpmA1g";
            Currency = "Bch";
        }

        public override void Disconnect()
        {
            ispd = false;

        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        protected override void CurrencyChanged()
        {
            if (accesstoken != "")
            {
                try
                {
                    string jsoncontent = json.JsonSerializer<NDChangeCoin>(new NDChangeCoin() { coin = Currency });
                    StringContent Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                    string Response = Client.PostAsync("api/changeCoin", Content).Result.Content.ReadAsStringAsync().Result;
                    NDChangeCoin getauth = json.JsonDeserialize<NDChangeCoin>(Response);
                    ForceUpdateStats = true;
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.ToString(), -1);
                }
            }
        }

        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    {
                        ForceUpdateStats = false;
                        lastupdate = DateTime.Now;
                        string sEmitResponse2 = Client.GetStringAsync("api/stats").Result;
                        NDGetBalance tmpu = json.JsonDeserialize<NDGetBalance>(sEmitResponse2);
                        try
                        {
                            sEmitResponse2 = Client.GetStringAsync("sshash").Result;
                            NDGetHash tmpHash = json.JsonDeserialize<NDGetHash>(sEmitResponse2);
                            lastHash = tmpHash.sshash;
                        }
                        catch (Exception e)
                        {

                        }
                        balance = tmpu.balance;
                        profit = tmpu.amountLost + tmpu.amountWon;
                        wins = (int)tmpu.totWins;
                        losses = (int)tmpu.totLosses;
                        bets = (int)tmpu.totBets;
                        Parent.updateBalance((balance));
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);


                    }
                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.nitrodice.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));


            try
            {
                string jsoncontent = json.JsonSerializer<NDAuth>(new NDAuth() { pass = Password, user = Username, tfa = twofa });
                StringContent Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                string Response = Client.PostAsync("api/auth", Content).Result.Content.ReadAsStringAsync().Result;
                NDGetAuth getauth = json.JsonDeserialize<NDGetAuth>(Response);
                if (getauth != null)
                {
                    if (getauth.token != null)
                    {
                        Client.DefaultRequestHeaders.Add("x-token", getauth.token);
                        Client.DefaultRequestHeaders.Add("x-user", Username);
                        accesstoken = getauth.token;
                        CurrencyChanged();
                        string sEmitResponse2 = Client.GetStringAsync("api/stats").Result;
                        NDGetBalance tmpu = json.JsonDeserialize<NDGetBalance>(sEmitResponse2);
                        try
                        {
                            sEmitResponse2 = Client.GetStringAsync("sshash").Result;
                            NDGetHash tmpHash = json.JsonDeserialize<NDGetHash>(sEmitResponse2);
                            lastHash = tmpHash.sshash;
                        }
                        catch (Exception e)
                        {

                        }
                        balance = tmpu.balance;
                        profit = tmpu.amountLost + tmpu.amountWon;
                        wins = (int)tmpu.totWins;
                        losses = (int)tmpu.totLosses;
                        bets = (int)tmpu.totBets;
                        Parent.updateBalance((balance));
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        lastupdate = DateTime.Now;
                        ispd = true;
                        Thread t = new Thread(GetBalanceThread);
                        t.Start();
                        finishedlogin(true);
                        return;
                    }
                }

            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), -1);

            }
            finishedlogin(false);
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
            throw new NotImplementedException();
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
        long nonce = -1;
        public static new decimal sGetLucky(string server, string client, long nonce)
        {
            SHA512 betgenerator = SHA512.Create();

            int charstouse = 5;

            List<byte> buffer = new List<byte>();
            string msg = server + ":" + nonce.ToString() + ":" + client;
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());
            StringBuilder hex2 = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex2.AppendFormat("{0:x2}", b);
            msg = hex2.ToString();
            buffer.Clear();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            hash = betgenerator.ComputeHash(hash);
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            int SIZE = 4;
            var r = new string[] { "0", "0", "0", "0", };
            string Hash = hex.ToString();
            if (nonce == 46)
            {

            }
            for (var i = 0; i < hash.Length; ++i)
            {
                try
                {
                    int tmp = int.Parse(r[i % SIZE], System.Globalization.NumberStyles.HexNumber) + hash[i];
                    var stringVal = tmp % 256;
                    r[i % SIZE] = (stringVal.ToString("X"));
                }
                catch
                {
                    r[i % SIZE] = "0";
                }

            }
            string hexres = "";
            for (int i = 0; i < r.Length; i++)
            {
                string tmp = r[i];
                if (tmp.Length < 2)
                {

                }

                hexres += tmp.Length < 2 ? "0" + tmp : tmp;
            }
            long Lucky = long.Parse(hexres, System.Globalization.NumberStyles.HexNumber);
            decimal result = ((decimal)(Lucky % 1000000)) / 10000m;

            return result;

            return 0;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }


        string lastHash = "";
        void placebetthread(object bet)
        {
            try
            {
                PlaceBetObj tmp5 = bet as PlaceBetObj;
                decimal amount = tmp5.Amount;
                decimal chance = tmp5.Chance;
                bool High = tmp5.High;
                string clientseed = r.Next(0, int.MaxValue).ToString();

                string jsoncontent = json.JsonSerializer<NDPlaceBet>(new NDPlaceBet()
                {
                    amount = amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo),
                    perc = chance.ToString("0.0000", System.Globalization.NumberFormatInfo.InvariantInfo),
                    pos = High ? "hi" : "lo",
                    times = 1,
                    cseed = clientseed
                });
                StringContent Content = new StringContent(jsoncontent, Encoding.UTF8, "application/json");
                string Response = Client.PostAsync("api/bet", Content).Result.Content.ReadAsStringAsync().Result;

                NDGetBet BetResult = json.JsonDeserialize<NDGetBet>(Response);


                if (BetResult.info == null)
                {
                    if (nonce == -1)
                        nonce = BetResult.index;
                    else if (nonce != BetResult.index - 1)
                    {
                        Parent.DumpLog("123 NONCE SKIPPED!!!!!! 12345!!!!", -1);
                    }
                    nonce = BetResult.index;
                    Bet tmp = new Bet
                    {
                        Amount = amount,
                        date = DateTime.Now,
                        Chance = chance,
                        clientseed = clientseed
                            ,
                        serverhash = lastHash,
                        Guid = tmp5.Guid,
                        high = High,
                        Id = BetResult.no.ToString(),
                        nonce = BetResult.index,
                        Roll = BetResult.n / 10000m,
                        serverseed = BetResult.sseed,
                        Profit = BetResult.amount
                    };
                    sqlite_helper.InsertSeed(tmp.serverhash, tmp.serverseed);

                    lastHash = BetResult.sshash;
                    bets++;
                    bool win = (tmp.Roll > 99.99m - tmp.Chance && High) || (tmp.Roll < tmp.Chance && !High);
                    balance = BetResult.balance;
                    wagered += amount;
                    profit += BetResult.amount;
                    if (win)
                    {
                        wins++;

                    }
                    else
                    {
                        losses++;
                    }

                    FinishedBet(tmp);
                }
                else
                {
                    Parent.updateStatus(BetResult.info);
                }
            }
            catch (Exception Ex)
            {
                Parent.DumpLog(Ex.ToString(), -1);
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }


    }

    public class NDGetAuth
    {
        public string token { get; set; }
    }

    public class NDAuth
    {
        public string user { get; set; }
        public string pass { get; set; }
        public string tfa { get; set; }
    }

    public class NDGetBalance
    {
        public decimal balance { get; set; }
        public decimal amountWon { get; set; }
        public decimal amountLost { get; set; }
        public long totWins { get; set; }
        public long totLosses { get; set; }
        public long totBets { get; set; }

    }

    public class NDPlaceBet
    {
        public string perc { get; set; }
        public string pos { get; set; }
        public string amount { get; set; }
        public int times { get; set; }
        public string cseed { get; set; }
    }

    public class NDGetBet
    {
        public long n { get; set; }
        public string r { get; set; }
        public decimal balance { get; set; }
        public long index { get; set; }
        public string sseed { get; set; }
        public string cseed { get; set; }
        public string target { get; set; }
        public long no { get; set; }
        public decimal amount { get; set; }
        public string sshash { get; set; }
        public string info { get; set; }
    }
    public class NDGetHash
    {
        public string sshash { get; set; }
    }
    public class NDChangeCoin { public string coin { get; set; } }
}
