
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Http;

namespace DiceBot
{
    public class PD : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        public PD(cDiceBot Parent)
        {
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://api.primedice.com/bets/";
            
            this.Parent = Parent;
            Name = "PrimeDice";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://primedice.com/?ref=Seuntjie";
            
        }

        
        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60||ForceUpdateStats))
                    {
                        string sEmitResponse2 = Client.GetStringAsync("users/1?api_key=" + accesstoken).Result;
                        pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                        balance = tmpu.user.balance / 100000000.0m; //i assume
                        bets = tmpu.user.bets;
                        Parent.updateBalance((decimal)(balance));
                        Parent.updateBets(tmpu.user.bets);
                        Parent.updateLosses(tmpu.user.losses);
                        Parent.updateProfit(tmpu.user.profit / 100000000m);
                        Parent.updateWagered(tmpu.user.wagered / 100000000m);
                        Parent.updateWins(tmpu.user.wins);
                        lastupdate = DateTime.Now;
                        
                    }
                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
        }

        public override bool Register(string Username, string Password)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://api.primedice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("affiliate", "seuntjie"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("register", Content).Result.Content.ReadAsStringAsync().Result;

                pdlogin tmp = json.JsonDeserialize<pdlogin>(sEmitResponse);
                accesstoken = tmp.api_key;
                if (accesstoken == "")
                    return false;
                else
                {
                    string sEmitResponse2 = Client.GetStringAsync("users/1?api_key=" + accesstoken).Result;
                    pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                    try
                    {
                        string s = "";
                        {
                            s = getDepositAddress();
                        }
                        if (s != null)
                        {
                            Parent.updateDeposit(s);
                        }
                    }
                    catch
                    {

                    }

                    balance = tmpu.user.balance; //i assume
                    bets = tmpu.user.bets;
                    Thread.Sleep(500);
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("password", Password));
                    Content = new FormUrlEncodedContent(pairs);
                     string sEmitResponse3 = Client.PostAsync("password?api_key="+accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
                     tmp = json.JsonDeserialize<pdlogin>(sEmitResponse);
                     accesstoken = tmp.api_key;
                    lastupdate = DateTime.Now;
                    ispd = true;
                    Thread t = new Thread(GetBalanceThread);
                    t.Start();
                    return true;


                }

            }
            catch
            {
                
            }

            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {
            //accept-encoding:gzip, deflate,
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://api.primedice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            
            try
            {

                /*if (Password.Length < 100)
                {

                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("username", Username));
                    pairs.Add(new KeyValuePair<string, string>("password", Password));
                    if (!string.IsNullOrWhiteSpace(otp))
                    {
                        pairs.Add(new KeyValuePair<string, string>("otp", otp));
                    }

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("login", Content).Result.Content.ReadAsStringAsync().Result;
                    if (sEmitResponse == "Invalid Password")
                    {
                        finishedlogin(false);
                        return;
                    }
                    pdlogin tmp = json.JsonDeserialize<pdlogin>(sEmitResponse);
                    accesstoken = tmp.api_key;
                    
                }
                else*/
                    accesstoken = Password;
                if (accesstoken == "")
                    finishedlogin(false);
                else
                {
                    string sEmitResponse2 = Client.GetStringAsync("users/1?api_key=" + accesstoken).Result;
                    pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                    this.username = tmpu.user.username;
                    uid = tmpu.user.userid;
                    balance = tmpu.user.balance; //i assume
                    bets = tmpu.user.bets;
                    Parent.updateBalance((decimal)(balance / 100000000.0m));
                    Parent.updateBets(tmpu.user.bets);
                    Parent.updateLosses(tmpu.user.losses);
                    Parent.updateProfit(tmpu.user.profit / 100000000m);
                    Parent.updateWagered(tmpu.user.wagered / 100000000m);
                    string s = tmpu.user.address;
                    try
                    {
                        if (s == null)
                        {
                            s = getDepositAddress();
                        }
                        if (s != null)
                        {
                            Parent.updateDeposit(s);
                        }
                    }
                    catch
                    {

                    }
                    Parent.updateWins(tmpu.user.wins);
                    lastupdate = DateTime.Now;
                    ispd = true;
                    Thread t = new Thread(GetBalanceThread);
                    t.Start();
                    finishedlogin(true);
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    
                    
                }
                finishedlogin(false);
            }
            catch (Exception e)
            {
                finishedlogin(false);
            }
        }
        int retrycount = 0;
        DateTime Lastbet = DateTime.Now;
        void placebetthread(object bet)
        {
            
            try
            {
                PlaceBetObj tmp5 = bet as PlaceBetObj;
                decimal amount = tmp5.Amount;
                decimal chance = tmp5.Chance;
                bool High = tmp5.High;
                if ((DateTime.Now - Lastbet).TotalMilliseconds<500)
                {
                    Thread.Sleep((int)(500.0 - (DateTime.Now - Lastbet).TotalMilliseconds));
                }
                decimal tmpchance = High ? 99.99m - chance : chance;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("amount", (amount * 100000000m).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("target", tmpchance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("condition", High ? ">" : "<"));
                

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("bet?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
                Lastbet = DateTime.Now;
                try
                {
                    pdbetresult tmp = json.JsonDeserialize<pdbetresult>(sEmitResponse);

                    tmp.bet.client = tmp.user.client;
                    tmp.bet.serverhash = tmp.user.server;
                    lastupdate = DateTime.Now;
                    balance = tmp.user.balance / 100000000.0m; //i assume
                    bets = tmp.user.bets;
                    wins = tmp.user.wins;
                    losses = tmp.user.losses;
                    wagered = (decimal)(tmp.user.wagered / 100000000m);
                    profit = (decimal)(tmp.user.profit / 100000000m);
                    FinishedBet(tmp.bet.toBet());
                    retrycount = 0;
                }
                catch
                {
                    Parent.updateStatus(sEmitResponse);
                }
            }
            catch (AggregateException e)
            {
                if (retrycount++ < 3)
                {
                    Thread.Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance));
                    return;
                }
                if (e.InnerException.Message.Contains("429") || e.InnerException.Message.Contains("502"))
                {
                    Thread .Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance));
                }
                

            }
            catch (Exception e2)
            {

            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            this.High = High;
            new Thread(new ParameterizedThreadStart(placebetthread)).Start(new PlaceBetObj(High, amount, chance));
        }

       
        public override void ResetSeed()
        {
            if ((DateTime.Now - LastSeedReset).TotalSeconds>90)
            {
                try
                {
                    LastSeedReset = DateTime.Now;
                    Parent.updateStatus("Resetting Seed");
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("seed", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20)));

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("seed?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
                    PDseeds tmpSeed = json.JsonDeserialize<PDseeds>(sEmitResponse);
                    sqlite_helper.InsertSeed(tmpSeed.seeds.previous_server_hashed, tmpSeed.seeds.previous_server);
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {

                        string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                        Parent.updateStatus(sEmitResponse);
                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(2000);
                            ResetSeed();
                        }
                    }
                }
            }
            else
            {
                Parent.updateStatus("Too soon to reset seed. Delaying reset.");
            }
            

        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

      

       
        public override bool ReadyToBet()
        {
            return true;
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                
                Thread.Sleep(500);
                 List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("amount", (Amount * 100000000).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("address", Address));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("withdraw?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = client + "-" + nonce.ToString();
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
                    return lucky / 10000;
            }
            return 0;
        }
        new public static decimal sGetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = client + "-" + nonce.ToString();
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
                {
                    lucky %= 10000;
                    return lucky / 100;

                }
            }
            return 0;
        }

        public string getDepositAddress()
        {
            try
            {
                 string sEmitResponse = Client.GetStringAsync("deposit?api_key=" + accesstoken).Result;
                pdDeposit tmpa = json.JsonDeserialize<pdDeposit>(sEmitResponse);
                return tmpa.address;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("429"))
                    {
                        Thread.Sleep(1500);
                        return getDepositAddress();
                    }
                    
                }
                return "";
            }
        }

        public override void Disconnect()
        {
            ispd = false;
            if (accesstoken!="")
            try
            {
                    string sEmitResponse = Client.GetStringAsync("logout?api_key=" + accesstoken).Result;
                    accesstoken = "";
            }
            catch
            {

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
                pairs.Add(new KeyValuePair<string, string>("amount", (amount * 100000000.0m).ToString("", System.Globalization.NumberFormatInfo.InvariantInfo)));
                
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("tip?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
                
                return (sEmitResponse.Contains("user"));
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    
                }
            }
            return false;
        }

        void GetRollThread(object _BetID)
        {
            try
            {
                long BetID = (long)_BetID;
                string sEmitResponse = Client.GetStringAsync("bets/"+BetID).Result;
                pdbet tmp = json.JsonDeserialize<pdbet>(sEmitResponse);
                if (tmp.bet.server !="")
                {
                    sqlite_helper.InsertSeed(tmp.bet.server, sqlite_helper.GetHashForBet(Name, long.Parse(tmp.bet.id)) );
                }
            }
            catch
            {

            }
            GettingSeed = false;
            
        }

        public override void GetSeed(long BetID)
        {
            GettingSeed = true;
            Thread GetSeedThread = new Thread(new ParameterizedThreadStart(GetRollThread));
            GetSeedThread.Start(BetID);
            //GetRollThread(BetID);
        }

        public override void SendChatMessage(string Message)
        {
            Thread send = new Thread(new ParameterizedThreadStart(Send));
            send.Start(Message);
        }

        void Send(object _Message)
        {
            if (accesstoken != "")
            {
                try
                {
                    string Message = (string)_Message;
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("username", username));
                    pairs.Add(new KeyValuePair<string, string>("userid", uid.ToString()));
                    pairs.Add(new KeyValuePair<string, string>("room", "English"));
                    pairs.Add(new KeyValuePair<string, string>("message", Message));
                    pairs.Add(new KeyValuePair<string, string>("token", accesstoken));

                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("send?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;

                }
                catch
                {

                }
            }
        }
        DateTime lastchat = DateTime.UtcNow;
        void GetMessagesThread()
        {
            while (ispd)
            {
                try
                {
                    if (accesstoken != "")
                    {
                       string sEmitResponse = Client.GetStringAsync("messages?api_key=" + accesstoken + "&room=English").Result;
                        chatmessages msgs = json.JsonDeserialize<chatmessages>(sEmitResponse);
                        bool pastlast = false;
                        for (int i = 0; i < msgs.messages.Length; i++)
                        {

                            //if (!pastlast)
                            {
                                pastlast = json.ToDateTime2(msgs.messages[i].timestamp).Ticks > lastchat.Ticks;
                            }
                            if (pastlast)
                            {
                                lastchat = json.ToDateTime2(msgs.messages[i].timestamp);
                                ReceivedChatMessage(lastchat.ToShortTimeString() + "(" + msgs.messages[i].userid + ") <" + msgs.messages[i].username + "> " + msgs.messages[i].message);
                            }
                        }
                    }
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(1500);
            }
            
        }

        
    }
    
    public class pdlogin
    {
        public bool admin { get; set; }
        public string api_key { get; set; }
    }

    public class pdbetresult
    {
        public pdbet bet { get; set; }
        public pduser user { get; set; }
    }
        public class pdDeposit
        {
            public string address { get; set; }
        }
    public class pdbet
    {
        public pdbet bet { get; set; }
        public string id { get; set; }
        public decimal profit { get; set; }
        public decimal amount { get; set; }
        public decimal target { get; set; }
        public bool win { get; set; }
        public string condition { get; set; }
        public string timestamp { get; set; }
        public decimal roll { get; set; }
        public long nonce { get; set; }
        public string client { get; set; }
        public string serverhash { get; set; }
        public string server { get; set; }
        public int player_id { get; set; }
        public Bet toBet()
        {
            Bet tmp = new Bet 
            {
                Amount = (decimal)amount / 100000000m, 
                date = DateTime.Now,
                Id = decimal.Parse(id, System.Globalization.CultureInfo.InvariantCulture), 
                Profit=(decimal)profit/100000000m, 
                Roll=(decimal)roll, 
                high=condition==">",
                Chance = condition == ">" ? 99.99m - (decimal)target : (decimal)target, 
                nonce=nonce ,
                serverhash = serverhash,
                clientseed = client,
                uid=player_id
            };
            return tmp;
        }
    }
    public class pduser
    {
        public decimal balance { get; set; }
        public int bets { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public pduser user { get; set; }
        public string client { get; set; }
        public string  server { get; set; }
        public decimal profit { get; set; }
        public decimal wagered { get; set; }
        public string address { get; set; }
        public long userid { get; set; }
        public string username { get; set; }
    }

    public class PDseeds
    {
        public PDseeds seeds { get; set; }
        public long nonce { get; set; }
        public string client { get; set; }
        public string previous_server { get; set; }
        public string previous_client { get; set; }
        public string previous_server_hashed { get; set; }
        public string next_seed { get; set; }
        public string server { get; set; }
    }

    public class chatmessages
    {
        public pdchat[] messages { get; set; }
    }

    public class pdchat
    {
        public string room { get; set; }
        public long userid { get; set; }
        public string username { get; set; }
        public string message { get; set; }
        public string timestamp { get; set; }
        public string tousername { get; set; }
        public long id { get; set; }
       
    }
}
