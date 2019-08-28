using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Net.Http;

namespace DiceBot
{
    class dice999:DiceSite
    {

        string sessionCookie = "";
        Random r = new Random();
        long uid = 0;
        
         bool isD999 = false;
        
        public static string[] cCurrencies =new string[] { "btc","doge","ltc","eth","xmr" };
        HttpClientHandler ClientHandlr;
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://www.999dice.com/api/web.aspx") };
        public dice999(cDiceBot Parent, bool doge999)
        {
            NonceBased = true;
            this.doge999 = doge999;
            maxRoll = 99.9999m;
            this.Parent = Parent;
            AutoInvest = false;
            AutoWithdraw = true;
            edge = 0.1m;
            ChangeSeed = false;
            AutoLogin = false;
            if (doge999)
                BetURL = "https://www.999doge.com/Bets/?b=";
            else
            BetURL = "https://www.999dice.com/Bets/?b=";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "999Dice";
            Tip = true;
            TipUsingName = false;
            Currency = "btc";
            Currencies = cCurrencies;
            /*Thread tChat = new Thread(GetMessagesThread);
            tChat.Start();*/
            if (doge999)
                SiteURL = "https://www.999doge.com/?20073598";
            else
            SiteURL = "https://www.999dice.com/?20073598";
        }

        protected override void CurrencyChanged()
        {
            Lastbalance = DateTime.Now.AddMinutes(-2);
            GetBalance();
            GetDepositAddress();
        }
        DateTime Lastbalance = DateTime.Now;
        void GetBalanceThread()
        {
            while (isD999)
            {
                if (sessionCookie!="" && sessionCookie!=null && ((DateTime.Now-Lastbalance).TotalSeconds>=60||ForceUpdateStats))
                {
                     GetBalance();

                }
                Thread.Sleep(1100);
            }
        }

        void GetBalance()
        {
            if (sessionCookie != "" && sessionCookie != null && (DateTime.Now - Lastbalance).TotalSeconds>60)
            {
                Lastbalance = DateTime.Now;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "GetBalance"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
                pairs.Add(new KeyValuePair<string, string>("Stats", "1"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            GetBalance();
                            return;
                        }
                    }
                }
                try
                {
                    d999Login tmplogin = json.JsonDeserialize<d999Login>(responseData);
                    balance = (decimal)tmplogin.Balance / 100000000.0m;
                    wagered = Wagered = -(decimal)tmplogin.TotalPayIn / 100000000.0m;
                    profit = tmplogin.TotalProfit / 100000000.0m; ;
                    bets = (int)tmplogin.TotalBets;
                    wins = (int)tmplogin.TotalWins;
                    losses = (int)tmplogin.TotalLoseCount;
                    if (balance != 0)
                    {
                        Parent.updateBalance((decimal)balance);
                        Parent.updateBets(bets);
                        Parent.updateWagered(wagered);
                        Parent.updateProfit(profit);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                    }
                    
                }
                catch
                {

                }
            }
        }
        int retrycount = 0;
        string next = "";
        void PlaceBetThread(object _High)
        {
            
            string err = "";
            try
            {
                PlaceBetObj tmp9 = _High as PlaceBetObj;

                bool High = tmp9.High;
                decimal amount = tmp9.Amount;
                //decimal chance = tmp9.Chance;

                Parent.updateStatus(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, tmp9.Chance, High ? "High" : "Low"));

                decimal chance = (999999.0m) * (tmp9.Chance / 100.0m);
                //HttpWebResponse EmitResponse;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                if (next == "" && next!=null)
                {


                    
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("a", "GetServerSeedHash"));
                    pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                    
                   Content = new FormUrlEncodedContent(pairs);
                     responseData = "";
                    using (var response = Client.PostAsync("", Content))
                    {
                        try
                        {
                            responseData = response.Result.Content.ReadAsStringAsync().Result;
                        }
                        catch (AggregateException e)
                        {
                            if (e.InnerException.Message.Contains("ssl"))
                            {
                                PlaceBetThread(High);
                                return;
                            }
                        }
                    }
                    if (responseData.Contains("error"))
                    {
                        if (retrycount++ < 3)
                        {

                            Thread.Sleep(200);
                            PlaceBetThread(High);
                            return;
                        }
                        else
                            throw new Exception();
                    }
                    string Hash = next =  json.JsonDeserialize<d999Hash>(responseData).Hash;
                }
                string ClientSeed = r.Next(0, int.MaxValue).ToString();
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "PlaceBet"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("PayIn", ((long)((decimal)amount * 100000000m)).ToString("0",System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("Low", (High ? 999999 - (int)chance : 0).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("High", (High ? 999999 : (int)chance).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("ClientSeed", ClientSeed));
                pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
                pairs.Add(new KeyValuePair<string, string>("ProtocolVersion", "2"));

                Content = new FormUrlEncodedContent(pairs);
                string tmps = Content.ReadAsStringAsync().Result;
                
                responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                        
                    }
                    catch (AggregateException e)
                    {
                        Parent.DumpLog(e.InnerException.Message, 0);
                        if (retrycount++ < 3)
                        {
                            PlaceBetThread(High);
                            return;
                        }
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            PlaceBetThread(High);
                            return;
                        }
                        else
                        {
                            Parent.updateStatus("An error has occurred");
                        }
                    }
                }
                
                
                d999Bet tmpBet = json.JsonDeserialize<d999Bet>(responseData);
                
                if (amount>=21)
                {

                }
                if (tmpBet.ChanceTooHigh == 1 || tmpBet.ChanceTooLow == 1 | tmpBet.InsufficientFunds == 1 || tmpBet.MaxPayoutExceeded == 1 || tmpBet.NoPossibleProfit == 1)
                {
                    if (tmpBet.ChanceTooHigh == 1)
                        err = "Chance too high";
                    if (tmpBet.ChanceTooLow == 1)
                        err += "Chance too Low";
                    if (tmpBet.InsufficientFunds == 1)
                        err += "Insufficient Funds";
                    if (tmpBet.MaxPayoutExceeded == 1)
                        err += "Max Payout Exceeded";
                    if (tmpBet.NoPossibleProfit == 1)
                        err += "No Possible Profit";
                    throw new Exception();
                }
                else if (tmpBet.BetId == 0)
                {
                    throw new Exception();
                }
                else
                {
                    balance = (decimal)tmpBet.StartingBalance / 100000000.0m - (amount) + ((decimal)tmpBet.PayOut / 100000000.0m);

                    profit += -(amount) + (decimal)(tmpBet.PayOut / 100000000m);
                    Bet tmp = new Bet();
                    tmp.Guid = tmp9.Guid;
                    tmp.Amount = (decimal)amount;
                    tmp.date = DateTime.Now;
                    tmp.Chance = ((decimal)chance * 100m) / 999999m;
                    tmp.clientseed = ClientSeed;
                    tmp.Currency = Currency;
                    tmp.high = High;
                    tmp.Id = tmpBet.BetId.ToString();
                    tmp.nonce = 0;
                    tmp.Profit = ((decimal)tmpBet.PayOut / 100000000m) - ((decimal)amount);
                    tmp.Roll = tmpBet.Secret / 10000m;
                    tmp.serverhash = next;
                    tmp.serverseed = tmpBet.ServerSeed;
                    tmp.uid = (int)uid;
                    tmp.UserName = "";

                    bool win = false;
                    if ((tmp.Roll > 99.99m - tmp.Chance && High) || (tmp.Roll < tmp.Chance && !High))
                    {
                        win = true;
                    }
                    if (win)
                        wins++;
                    else
                        losses++;
                    Wagered += tmp.Amount;
                    bets++;
                    

                    sqlite_helper.InsertSeed(tmp.serverhash, tmp.serverseed);
                    next = tmpBet.Next;
                    retrycount = 0;
                    FinishedBet(tmp);
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(),-1);
                if (err != "")
                    Parent.updateStatus(err);
                else
                    Parent.updateStatus("Something went wrong! stopped betting");
            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string guid)
        {
            this.High = High;
            Thread t = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            t.Start(new PlaceBetObj(High, amount, chance, guid));
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

       
       

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            isD999 = false;
            thing = false;
        }

        public override void GetSeed(long BetID)
        {
            
        }

        public override void SendChatMessage(string Message)
        {
            
        }

        public override void Donate(decimal Amount)
        {
            InternalSendTip("20073598", Amount);
            /*switch (Currency.ToLower())
            { 
                case "btc": internalWithdraw(Amount, "1BoHcFQsUSot7jkHJcZMh1iUda3tEjzuBW");break;
                case "ltc": internalWithdraw(Amount,"LUzfcpLCy7SdXZbSiJwEmZarvCzgXbfubS");break;
                case "doge":internalWithdraw(Amount,"DAqsyP2H5vqhc9PTfbjd7nr6r8tCFRWcFJ");break;
                case "eth":internalWithdraw(Amount,"0x77a4220ca85d4103e008eb88ae15f5ac7da0660d");break;
            }*/
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "Withdraw"));
            pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
            pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
            pairs.Add(new KeyValuePair<string, string>("Amount", (amount * 100000000m).ToString("0", System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("Address", User));

            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("", Content))
            {
                while (!response.IsCompleted)
                    Thread.Sleep(100);
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        return InternalSendTip(User, amount);
                    }
                }
            }

            return true;
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "Withdraw"));
            pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
            pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
            pairs.Add(new KeyValuePair<string, string>("Amount", (Amount*100000000m).ToString("0",System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("Address", Address));

            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("", Content))
            {
                while (!response.IsCompleted)
                    Thread.Sleep(100);
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        return internalWithdraw(Amount , Address);
                    }
                }
            }
            
            return true;
        }

        public bool doge999 = false;
        decimal Wagered = 0;
        int site = 0;
        bool thing = false;
        string[] SiteA = new string[] {"https://www.999dice.com/api/web.aspx" ,
            "https://www.999proxy.com/api/web.aspx",
            "https://www.999doge.com/api/web.aspx",
            "https://www.999-dice.com/api/web.aspx",
            "http://999again.ddns.net:999/api/web.aspx" };

        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                string sitea = SiteA[site];
                /*switch (site)
                {
                    case 0: sitea = "https://www.999dice.com/api/web.aspx"; break;
                    case 1: sitea = "https://www.999doge.com/api/web.aspx"; break;
                    case 2: sitea = "https://www.999-dice.com/api/web.aspx"; break;
                    case 3: sitea = "http://999again.ddns.net:999/"; break;
                }*/
                ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null }; ;
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri(sitea) };
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "Login"));
                pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
                if (twofa != "" && twofa != null)
                    pairs.Add(new KeyValuePair<string, string>("Totp", twofa));

                pairs.Add(new KeyValuePair<string, string>("Username", Username));
                pairs.Add(new KeyValuePair<string, string>("Password", Password));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (site++ < SiteA.Length-1)
                            Login(Username, Password, twofa);
                        else
                            finishedlogin(false);
                        return;

                    }
                }

                d999Login tmpU = json.JsonDeserialize<d999Login>(responseData);
                if (tmpU.SessionCookie != "" && tmpU.SessionCookie != null)
                {
                    Lastbalance = DateTime.Now;
                    sessionCookie = tmpU.SessionCookie;
                    balance = tmpU.Balance / 100000000.0m;
                    profit = tmpU.Profit / 100000000.0m;
                    Wagered = tmpU.Wagered / 100000000m;
                    bets = (int)tmpU.BetCount;
                    wins = (int)tmpU.BetWinCount;
                    losses = (int)tmpU.BetLoseCount;
                    GetBalance();
                    Parent.updateBalance((decimal)(balance));
                    Parent.updateBets(tmpU.BetCount);
                    Parent.updateLosses(tmpU.BetLoseCount);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(Wagered);
                    Parent.updateWins(tmpU.BetWinCount);
                    Lastbalance = DateTime.Now.AddMinutes(-2);
                    GetBalance();
                    try
                    {
                        Parent.updateDeposit(tmpU.DepositAddress);
                    }
                    catch { }
                    uid = tmpU.Accountid;
                }
                else
                {

                }
                if (sessionCookie != "")
                {
                    isD999 = true;
                    Thread t = new Thread(GetBalanceThread);
                    t.Start();

                }
            }
            catch {
                if (site++ < 2)
                    Login(Username, Password, twofa);
            }
            if (!thing)
            {
                finishedlogin(sessionCookie != "");
                thing = true;
            }
        }
        public override bool Register(string username, string password)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
            if (doge999)
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.999doge.com/api/web.aspx") };
            else
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.999dice.com/api/web.aspx") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "CreateAccount"));
            pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("", Content))
            {
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        return Register(username, password);
                        
                    }
                }
            }
            
            d999Register tmp = json.JsonDeserialize<d999Register>(responseData);
            if (tmp.SessionCookie!="" && tmp.SessionCookie!=null)
            {
                sessionCookie = tmp.SessionCookie;
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "CreateUser"));
                pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Username", username));
                pairs.Add(new KeyValuePair<string, string>("Password", password));
                Content = new FormUrlEncodedContent(pairs);
                responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            return Register(username, password);

                        }
                    }
                }
                
                Parent.updateBalance((decimal)(balance));
                 Parent.updateBets(0);
                 Parent.updateLosses(0);
                 Parent.updateProfit(0m);
                 Parent.updateWagered(0m);
                 Parent.updateWins(0);
                 Parent.updateDeposit(tmp.DepositAddress);
                 uid = tmp.Accountid;
            }
            else
            {

            } 
            if (sessionCookie != "")
            {
                isD999 = true;
                Thread t = new Thread(GetBalanceThread);
                t.Start();

            }
            return sessionCookie != "" && sessionCookie != null;
        }

        public void GetDepositAddress()
        {
            if (sessionCookie != "" && sessionCookie != null)
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "GetDepositAddress"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Currency", Currency));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            GetDepositAddress();
                            return;

                        }
                    }
                }
                
                
                d999deposit tmp = json.JsonDeserialize<d999deposit>(responseData);
                Parent.updateDeposit(tmp.Address);
            }
        }

        public override decimal GetLucky(string serverSeed, string clientSeed, int nonce)
        {
            return sGetLucky(serverSeed, clientSeed, nonce);
            
        }
        public static decimal sGetLucky(string serverSeed, string clientSeed, long betNumber/*, long betResult*/, string serverSeedHash = null)
        {
            Func<string, byte[]> strtobytes = s => Enumerable
               .Range(0, s.Length / 2)
               .Select(x => byte.Parse(s.Substring(x * 2, 2), NumberStyles.HexNumber))
               .ToArray();
            byte[] server = strtobytes(serverSeed);
            byte[] client = BitConverter.GetBytes(int.Parse(clientSeed)).Reverse().ToArray();
            byte[] num = BitConverter.GetBytes(betNumber).Reverse().ToArray();
            byte[] data = server.Concat(client).Concat(num).ToArray();
            using (SHA512 sha512 = new SHA512Managed())
            {
                /* if (serverhash != null)
                     using (SHA256 sha256 = new SHA256Managed())
                         if (!sha256.ComputeHash(server).SequenceEqual(serverhash))
                             throw new Exception("Server seed hash does not match server seed");*/
                byte[] hash = sha512.ComputeHash(sha512.ComputeHash(data));
                while (true)
                {
                    for (int x = 0; x <= 61; x += 3)
                    {
                        long result = (hash[x] << 16) | (hash[x + 1] << 8) | hash[x + 2];
                        if (result < 16000000)
                        {
                            return (result % 1000000m) / 10000.0m;
                        }
                    }
                    hash = sha512.ComputeHash(hash);
                }
            }
        }
    }

    public class d999Register
    {
        public string AccountCookie { get; set; }
        public string SessionCookie { get; set; }
        public long Accountid { get; set; }
        public int MaxBetBatchSize { get; set; }
        public string ClientSeed { get; set; }
        public string DepositAddress { get; set; }
    }

    public class d999Login:d999Register
    {
        public decimal Balance { get; set; }
        public string Email { get; set; }
        public string EmergenctAddress { get; set; }
        public long BetCount { get; set; }
        public long BetWinCount { get; set; }
        public long BetLoseCount { get { return BetCount - BetWinCount; } }
        public decimal BetPayIn { get; set; }
        public decimal BetPayOut { get; set; }
        public decimal Profit { get {return BetPayIn+BetPayOut;} }
        public decimal Wagered { get { return BetPayOut - BetPayIn; } }

        public decimal TotalPayIn { get; set; }
        public decimal TotalPayOut { get; set; }
        public decimal TotalProfit { get { return TotalPayIn + TotalPayOut; } }

        public long TotalBets { get; set; }
        public long TotalWins { get; set; }
        public long TotalLoseCount { get { return TotalBets - TotalWins; } }
    }

    public class d999Hash
    {
        public string Hash { get; set; }
    }
    public class d999deposit
    {
        public string Address { get; set; }
    }
    public class d999Bet
    {
        public long BetId { get; set; }
        public decimal PayOut { get; set; }
        public decimal Secret { get; set; }
        public decimal StartingBalance { get; set; }
        public string ServerSeed { get; set; }
        public string Next { get; set; }

        public int ChanceTooHigh { get; set; }
        public int ChanceTooLow { get; set; }
        public int InsufficientFunds { get; set; }
        public int NoPossibleProfit { get; set; }
        public int MaxPayoutExceeded { get; set; }
    }

    
    
}

