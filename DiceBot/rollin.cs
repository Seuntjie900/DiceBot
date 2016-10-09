using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class rollin: DiceSite
    {
        string server_hash = "";
        string client = "";
        string username = "";
        Random R = new Random();
        HttpClientHandler ClientHandlr;// = new HttpClientHandler();
        HttpClient Client;
        public rollin(cDiceBot Parent)
        {
            maxRoll = 99;
            this.Parent = Parent;
            AutoWithdraw = true;
            AutoInvest = false;
            Tip = true;
            TipUsingName = true;
            ChangeSeed = true;
            Name = "RollinIO";
            
            SiteURL = "https://rollin.io/ref/8c4";
            NonceBased = false;
            
        }
        DateTime lastbet = DateTime.Now;
        DateTime LastBalance = DateTime.Now;
        void SyncThread()
        {
            while (isRollin)
            {
                if (Token!="" && Token!=null && username!="" && ((DateTime.Now - LastBalance).TotalSeconds>15||ForceUpdateStats))
                {
                    try
                    {


                        string sEmitResponse2 = Client.GetStringAsync("customer/sync").Result;
                        RollinBet tmpStats2 = json.JsonDeserialize<RollinBet>(sEmitResponse2);
                        if (tmpStats2.success)
                        {
                            balance = (decimal.Parse(tmpStats2.customer.balance, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m);
                            Parent.updateBalance(balance);
                        }
                        LastBalance = DateTime.Now;
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus(e.Message);
                    }
                }
                System.Threading.Thread.Sleep(500);
            }
        }
        int retrycount = 0;
        
        void PlaceBetThread(object _High)
        {
            try
            {
                PlaceBetObj tmp9 = _High as PlaceBetObj;
                bool High = tmp9.High;
                decimal amount = tmp9.Amount;
                decimal chance = tmp9.Chance;
                lastbet = DateTime.Now;
                //bool High = (bool)_High;
                decimal tmpchance = High ? maxRoll - chance : chance;
                string sendchance = tmpchance.ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo);
                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("bet_amount", (amount * 1000).ToString("0.00000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("bet_number",sendchance ));
                pairs.Add(new KeyValuePair<string, string>("prediction", High ? "bigger" : "smaller"));
                pairs.Add(new KeyValuePair<string, string>("seed", R.Next(int.MaxValue).ToString()));
                
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("games/dice/play", Content).Result.Content.ReadAsStringAsync().Result;
                RollinBet tmp = json.JsonDeserialize<RollinBet>(sEmitResponse);
                if (tmp.errors != null && tmp.errors.Length>0)
                {
                    Parent.updateStatus(tmp.errors[0]);
                }
                else
                {
                    Bet tmp2 = tmp.ToBet();
                    tmp2.serverhash = server_hash;
                    server_hash = tmp.customer.server_hash;
                    balance = decimal.Parse(tmp.customer.balance, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m;
                    bets = tmp.statistics.bets;
                    
                    losses = tmp.statistics.losses;
                    profit = decimal.Parse(tmp.statistics.profit, System.Globalization.CultureInfo.InvariantCulture) / 1000.0m;
                    
                    wagered = decimal.Parse(tmp.statistics.wagered, System.Globalization.CultureInfo.InvariantCulture) / 1000.0m;
                    wins = (tmp.statistics.wins);
                    LastBalance = DateTime.Now;
                    tmp2.date = DateTime.Now;
                    FinishedBet(tmp2);
                    retrycount = 0;
                }

            }
            catch (Exception E)
            {
                if (retrycount++ < 3)
                {
                    PlaceBetThread(High);
                    return;
                }
                Parent.updateStatus(E.Message);
                if (Parent.logging > 1)
                using (StreamWriter sw = File.AppendText("log.txt"))
                {
                    sw.WriteLine(E.Message);
                    sw.WriteLine(E.StackTrace);
                    sw.WriteLine(json.JsonSerializer<System.Collections.IDictionary> (E.Data));
                }
            }

        }
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {

            Thread T = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            T.Start(new PlaceBetObj(High, amount, chance));
        }

        public override void ResetSeed()
        {
            
            
            string sEmitResponse = Client.GetStringAsync("customer/seed/randomize").Result;
            RollinRandomize rand = json.JsonDeserialize<RollinRandomize>(sEmitResponse);
            if (rand.success)
            {
                this.client = rand.client_seed;
                this.server_hash = rand.server_hash;
            }
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("address", Address));
                pairs.Add(new KeyValuePair<string, string>("amount", (Amount * 1000).ToString("0.00000", System.Globalization.NumberFormatInfo.InvariantInfo)));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("transaction/withdraw", Content).Result.Content.ReadAsStringAsync().Result;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void GetDeposit()
        {
            string sEmitResponse2 = Client.GetStringAsync("customer/address").Result;
            RollinDeposit tmp = json.JsonDeserialize<RollinDeposit>(sEmitResponse2);
            if (tmp.success)
            {
                Parent.updateDeposit(tmp.address);
            }

        }

        public override void Donate(decimal Amount)
        {
            SendTip("seuntjie", Amount);
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", User));
                pairs.Add(new KeyValuePair<string, string>("private", "0"));
                pairs.Add(new KeyValuePair<string, string>("amount", (amount * 1000).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("tipsy/tip", Content).Result.Content.ReadAsStringAsync().Result;

                return sEmitResponse.Contains("\"success\":true");
            }
            catch
            {
                return false ;
            }
        }

        
        CookieContainer Cookies = new CookieContainer();
        string Token = "";
        public override void Login(string Username, string Password, string twofa)
        {

            try
            {
                this.username = Username;
                HttpWebRequest getHeaders = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/ref/8c4");
                if (Prox != null)
                    getHeaders.Proxy = Prox;
                var cookies = new CookieContainer();
                getHeaders.CookieContainer = cookies;

                try
                {
                    HttpWebResponse Response = (HttpWebResponse)getHeaders.GetResponse();
                    string s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                    foreach (Cookie C in Response.Cookies)
                    {
                        cookies.Add(C);
                    }
                    s1 = s1.Substring(s1.IndexOf("<input name=\"_token\" type=\"hidden\""));
                    s1 = s1.Substring("<input name=\"_token\" type=\"hidden\" value=\"".Length);
                    Token = s1.Substring(0, s1.IndexOf("\""));
                }
                catch
                {
                    finishedlogin(false);
                    return;
                }


                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/login");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.CookieContainer = cookies;

                betrequest.Method = "POST";

                string post = string.Format("username={0}&password={1}&code={2}", Username, Password, twofa);
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                betrequest.Headers.Add("X-CSRF-Token", Token);
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                if (!sEmitResponse.ToLower().Contains("true"))
                {
                    finishedlogin(false);
                    return;
                }
                this.Cookies = cookies;
                HttpWebRequest betrequest2 = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/info?username=" + username);
                if (Prox != null)
                    betrequest2.Proxy = Prox;
                betrequest2.CookieContainer = cookies;
                betrequest2.Headers.Add("X-CSRF-Token", Token);
                HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest2.GetResponse();
                string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                RollinLoginStats tmpStats = json.JsonDeserialize<RollinLoginStats>(sEmitResponse2);

                //https://rollin.io/api/customer/sync
                betrequest2 = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/sync");
                if (Prox != null)
                    betrequest2.Proxy = Prox;
                betrequest2.CookieContainer = cookies;
                betrequest2.Headers.Add("X-CSRF-Token", Token);
                EmitResponse2 = (HttpWebResponse)betrequest2.GetResponse();
                sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                RollinBet tmpStats2 = json.JsonDeserialize<RollinBet>(sEmitResponse2);

                if (tmpStats.success && tmpStats2.success)
                {
                    ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
                    Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://rollin.io/api/") };
                    Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                    Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                    ClientHandlr.CookieContainer = this.Cookies;
                    Client.DefaultRequestHeaders.Add("X-CSRF-Token", Token);
                    
                    GetDeposit();
                    balance = decimal.Parse(tmpStats2.customer.balance, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m; //i assume
                    bets = tmpStats.user.bets;
                    profit = decimal.Parse(tmpStats.user.profit, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m;

                    Parent.updateBalance((decimal)(balance));
                    Parent.updateBets(tmpStats.user.bets);
                    Parent.updateLosses(tmpStats.user.losses);
                    Parent.updateProfit(decimal.Parse(tmpStats.user.profit, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m);
                    Parent.updateWagered(decimal.Parse(tmpStats.user.wagered, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m);
                    Parent.updateWins(tmpStats.user.wins);
                    isRollin = true;
                    Thread t = new Thread(new ThreadStart(SyncThread));
                    t.Start();
                    finishedlogin(true);
                    return;
                }
            }
            catch
            {

            }
            finishedlogin(false);
        }

        public override bool Register(string username, string password)
        {
            this.username = username;
            HttpWebRequest getHeaders = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/ref/8c4");
            if (Prox != null)
                getHeaders.Proxy = Prox;
            var cookies = new CookieContainer();
            getHeaders.CookieContainer = cookies;

            try
            {
                HttpWebResponse Response = (HttpWebResponse)getHeaders.GetResponse();
                string s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                foreach (Cookie C in Response.Cookies)
                {
                    cookies.Add(C);
                }
                s1 = s1.Substring(s1.IndexOf("<input name=\"_token\" type=\"hidden\""));
                s1 = s1.Substring("<input name=\"_token\" type=\"hidden\" value=\"".Length);
                Token = s1.Substring(0, s1.IndexOf("\""));
            }
            catch
            {
                finishedlogin(false);
                return false;
            }
            HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/settings/username");
            if (Prox != null)
                betrequest.Proxy = Prox;
            betrequest.CookieContainer = cookies;
            betrequest.Method = "POST";
            string post = string.Format("username={0}", username);
            betrequest.ContentLength = post.Length;
            betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            betrequest.Headers.Add("X-CSRF-Token", Token);
            using (var writer = new StreamWriter(betrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            betrequest = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/settings/password");
            if (Prox != null)
                betrequest.Proxy = Prox;
            betrequest.CookieContainer = cookies;
            betrequest.Method = "POST";
            post = string.Format("old=&new={0}&confirm={0}", password);
            betrequest.ContentLength = post.Length;
            betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            betrequest.Headers.Add("X-CSRF-Token", Token);
            using (var writer = new StreamWriter(betrequest.GetRequestStream()))
            {
                writer.Write(post);
            }
            EmitResponse = (HttpWebResponse)betrequest.GetResponse();
            sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

            HttpWebRequest betrequest2 = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/info?username=" + username);
            if (Prox != null)
                betrequest2.Proxy = Prox;
            betrequest2.CookieContainer = cookies;
            betrequest2.Headers.Add("X-CSRF-Token", Token);
            HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest2.GetResponse();
            string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
            RollinLoginStats tmpStats = json.JsonDeserialize<RollinLoginStats>(sEmitResponse2);

            //https://rollin.io/api/customer/sync
            betrequest2 = (HttpWebRequest)HttpWebRequest.Create("https://rollin.io/api/customer/sync");
            if (Prox != null)
                betrequest2.Proxy = Prox;
            betrequest2.CookieContainer = cookies;
            betrequest2.Headers.Add("X-CSRF-Token", Token);
            EmitResponse2 = (HttpWebResponse)betrequest2.GetResponse();
            sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
            RollinBet tmpStats2 = json.JsonDeserialize<RollinBet>(sEmitResponse2);

            if (tmpStats.success && tmpStats2.success)
            {
                ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://rollin.io/api/") };
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                ClientHandlr.CookieContainer = this.Cookies;
                Client.DefaultRequestHeaders.Add("X-CSRF-Token", Token);

                GetDeposit();
                balance = decimal.Parse(tmpStats2.customer.balance, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m; //i assume
                bets = tmpStats.user.bets;
                profit = decimal.Parse(tmpStats.user.profit, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m;

                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(tmpStats.user.bets);
                Parent.updateLosses(tmpStats.user.losses);
                Parent.updateProfit(decimal.Parse(tmpStats.user.profit, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m);
                Parent.updateWagered(decimal.Parse(tmpStats.user.wagered, System.Globalization.NumberFormatInfo.InvariantInfo) / 1000.0m);
                Parent.updateWins(tmpStats.user.wins);
                isRollin = true;
                Thread t = new Thread(new ThreadStart(SyncThread));
                t.Start();
                finishedlogin(true);
                return true;
            }
            finishedlogin(false);
            return false;
        }

       
        public override bool ReadyToBet()
        {
            //return true;
            if (amount == 0)
                return (DateTime.Now - lastbet).TotalMilliseconds >= 1000;
            else if (amount < 0.00000010m)
                return (DateTime.Now - lastbet).TotalMilliseconds >= 750;
            else if (amount < 0.00000100m)
                return (DateTime.Now - lastbet).TotalMilliseconds >= 500;
            else if (amount < 0.00001000m)
                return (DateTime.Now - lastbet).TotalMilliseconds >= 250;
            else
                return (DateTime.Now - lastbet).TotalMilliseconds >= 10;
        }

        bool isRollin = false;
        
        public override void Disconnect()
        {
            isRollin = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            Parent.updateStatus("Cannot chat at this moment. Sorry!");
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            /*server = "182fb47eb2f00c928b041795faf5bbd5759829086a67a46edc73a54e9505cfb0";
            client = "468538814";*/
            HMACSHA512 betgenerator = new HMACSHA512();
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }
            
            
            List<byte> buffer = new List<byte>();
            string msg = client;
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            betgenerator.Key = buffer.ToArray();
            byte[] hash = betgenerator.ComputeHash(serverb.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            string hashres = hex.ToString();
            int start = (hashres.ToString().Length / 2) - 4;
            string s = hashres.ToString().Substring(start, 8);
            UInt32 seed = UInt32.Parse(s, System.Globalization.NumberStyles.HexNumber);
            MersenneTwister twist = new MersenneTwister(seed);
            
            int t4 = (int)(twist.genrand_real2() * 100);

            
            return t4;
        }

        new public static decimal sGetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }


            List<byte> buffer = new List<byte>();
            string msg = client;
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            betgenerator.Key = buffer.ToArray();
            byte[] hash = betgenerator.ComputeHash(serverb.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            string hashres = hex.ToString();
            int start = (hashres.ToString().Length / 2) - 4;
            string s = hashres.ToString().Substring(start, 8);
            UInt32 seed = UInt32.Parse(s, System.Globalization.NumberStyles.HexNumber);
            MersenneTwister twist = new MersenneTwister(seed);
            int t4 = (int)(twist.genrand_real2() * 100);
            return t4;
        }
    }

    public class RollinBet
    {
        public bool success { get; set; }
        public RollinGame game { get; set; }
        public RollinCustomer customer { get; set; }
        public RollinStats statistics { get; set; }
        public decimal fee { get; set; }
        public string[] errors { get; set; }
       
        //public string server_seed_hash { get; set; }
        public Bet ToBet()
        {
            Bet tmp = new Bet
            {
                Amount=decimal.Parse(game.bet_amount, System.Globalization.CultureInfo.InvariantCulture)/1000m,
                date = DateTime.Now,
                Id = game.id,
                
                Roll = game.number,
                high = game.prediction=="bigger",
                Chance = game.odds,

                clientseed = game.client_seed,
                serverseed = game.server_seed
            };
            
            decimal Profit = decimal.Parse(game.profit, System.Globalization.CultureInfo.InvariantCulture) / 1000m;
            if ((tmp.high && tmp.Roll>(99m-tmp.Chance)) || (!tmp.high && tmp.Roll < tmp.Chance))
            {
                tmp.Profit = Profit;
            }
            else
            {
                tmp.Profit = -decimal.Parse(game.bet_amount, System.Globalization.CultureInfo.InvariantCulture) / 1000m;
            }
            return tmp;
        }


    }

    public class RollinLoginStats
    {
        public bool success { get; set; }
        public string[] errors { get; set; }
        public RollinStats user { get; set; }
    }

    public class RollinGame
    {
        public bool status { get; set; }
        public decimal number { get; set; }
        public decimal odds { get; set; }
        public string multiplier { get; set; }
        public string bet_number { get; set; }
        public string prediction { get; set; }
        public string profit { get; set; }
        public string bet_amount { get; set; }
        public string server_seed { get; set; }
        public long id { get; set; }
        public string client_seed { get; set; }
    }
    public class RollinCustomer
    {
        public string balance { get; set; }
        public string server_hash { get; set; }
    }
    public class RollinStats
    {
        public int level { get; set; }
        public string profit { get; set; }
        public string wagered { get; set; }
        public int bets { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public string date { get; set; }
    }
    public class RollinRandomize
    {
        public bool success { get; set; }
        public string[] errors { get; set; }
        public string client_seed { get; set; }
        public string server_hash { get; set; }
    }
    public class RollinBalance
    {
        public bool success { get; set; }
        public RollinCustomer customer { get; set; }
    }
    public class RollinDeposit
    {
        public bool success { get; set; }
        public string address { get; set; }
        public string[] errors { get; set; }
    }

}
