using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;

namespace DiceBot
{
    class dice999:DiceSite
    {

        string sessionCookie = "";
        Random r = new Random();
        long uid = 0;
        
        bool isD999 = true;
        
        public static string[] cCurrencies =new string[] { "btc","doge","ltc" };
        public dice999(cDiceBot Parent)
        {
            maxRoll = 99.9999;
            this.Parent = Parent;
            AutoInvest = false;
            AutoWithdraw = true;
            
            ChangeSeed = false;
            AutoLogin = false;
            BetURL = "https://www.999dice.com/Bets/?b=";
            Thread t = new Thread(GetBalanceThread);
            t.Start();
            this.Parent = Parent;
            Name = "999Dice";
            Tip = false;
            TipUsingName = true;
            Currency = "btc";
            Currencies = cCurrencies;
            /*Thread tChat = new Thread(GetMessagesThread);
            tChat.Start();*/
        }

        protected override void CurrencyChanged()
        {
            GetBalance();
            GetDepositAddress();
        }
        DateTime Lastbalance = DateTime.Now;
        void GetBalanceThread()
        {
            while (isD999)
            {
                if (sessionCookie!="" && sessionCookie!=null && (DateTime.Now-Lastbalance).TotalSeconds>=60)
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
                HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                string post = string.Format("a=GetBalance&s={0}&Currency={1}", sessionCookie, Currency);
                loginrequest.Method = "POST";

                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                balance = (double)json.JsonDeserialize<d999Login>(sEmitResponse).Balance / 100000000.0;
                if (balance == 0)
                {

                }
                Parent.updateBalance((decimal)balance);
            }
        }
        int BetRetries = 0;
        string next = "";
        void PlaceBetThread()
        {
            try
            {
                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, this.chance, High ? "High" : "Low"));
                HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                string post = string.Format("a=GetServerSeedHash&s={0}", sessionCookie);
                string sEmitResponse = "";
                double chance = (999999.0) * (this.chance / 100.0);
                HttpWebResponse EmitResponse;
                
                if (next == "" && next!=null)
                {
                    
                    
                    loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    post = string.Format("a=GetServerSeedHash&s={0}", sessionCookie);
                    loginrequest.Method = "POST";

                    loginrequest.ContentLength = post.Length;
                    loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                    {

                        writer.Write(post);
                    }
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    if (sEmitResponse.Contains("error"))
                    {
                        if (BetRetries++ < 3)
                        {

                            Thread.Sleep(200);
                            PlaceBetThread();
                            return;
                        }
                        else
                            throw new Exception();
                    }
                    string Hash = next =  json.JsonDeserialize<d999Hash>(sEmitResponse).Hash;
                }
                loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                string ClientSeed = r.Next(0, int.MaxValue).ToString();


                post = string.Format("a=PlaceBet&s={0}&PayIn={1}&Low={2}&High={3}&ClientSeed={4}&Currency={5}&ProtocolVersion=2", sessionCookie, (long)Math.Ceiling(amount * 100000000.0), High ? 999999 - (int)chance : 0, High ? 999999 : (int)chance, ClientSeed, Currency);
                loginrequest.Method = "POST";

                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                 sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                
                d999Bet tmpBet = json.JsonDeserialize<d999Bet>(sEmitResponse);
                if (amount>=21)
                {

                }
                if (tmpBet.ChanceTooHigh==1 || tmpBet.ChanceTooLow==1| tmpBet.InsufficientFunds == 1|| tmpBet.MaxPayoutExceeded==1|| tmpBet.NoPossibleProfit==1)
                {
                    throw new Exception();
                }
                if (tmpBet.BetId==0)
                {

                }
                balance = (double)tmpBet.StartingBalance / 100000000.0 - (amount) + ((double)tmpBet.PayOut / 100000000.0);

                profit += -(amount ) + (double)(tmpBet.PayOut / 100000000m);
                Bet tmp = new Bet();
                tmp.Amount = (decimal)amount;
                tmp.BetDate = DateTime.Now.ToString(); ;
                tmp.Chance = ((decimal)chance * 100m) / 999999m;
                tmp.clientseed = ClientSeed;
                tmp.Currency = Currency;
                tmp.high = High;
                tmp.Id = tmpBet.BetId;
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
                BetRetries = 0;
                
                sqlite_helper.InsertSeed(tmp.serverhash, tmp.serverseed);
                next = tmpBet.Next;
                FinishedBet(tmp);
                
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Something went wrong! betting stopped.");
            }
        }

        protected override void internalPlaceBet(bool High)
        {
            this.High = High;
            Thread t = new Thread(PlaceBetThread);
            t.Start();
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
        }

        public override void GetSeed(long BetID)
        {
            
        }

        public override void SendChatMessage(string Message)
        {
            
        }


        protected override bool internalWithdraw(double Amount, string Address)
        {
            
            HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
            if (Prox != null)
                loginrequest.Proxy = Prox;
            string post = string.Format("a=Withdraw&s={0}&Amount={1}&Address={2}&currency={3}", sessionCookie, Amount*100000000, Address, Currency);
            loginrequest.Method = "POST";

            loginrequest.ContentLength = post.Length;
            loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            return true;
        }

        
        decimal Wagered = 0;
        public override void Login(string Username, string Password, string twofa)
        {
            HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
            if (Prox != null)
                loginrequest.Proxy = Prox;
            string post = "a=Login&key=7a3ada10cb804ec695cda315db6b8789&Username=" + Username + "&Password=" + Password + (twofa != "" ? "&Totp=" + twofa : "");
            loginrequest.Method = "POST";

            loginrequest.ContentLength = post.Length;
            loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            d999Login tmpU = json.JsonDeserialize<d999Login>(sEmitResponse);
            if (tmpU.SessionCookie!="" && tmpU.SessionCookie!=null)
            { 
                sessionCookie = tmpU.SessionCookie;
                
                profit= (double)tmpU.Profit/100000000.0;
                Wagered = tmpU.Wagered/100000000m;
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
            finishedlogin(sessionCookie != "");
        }
        public override bool Register(string username, string password)
        {
            HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
            if (Prox != null)
                loginrequest.Proxy = Prox;
            string post = "a=CreateAccount&key=7a3ada10cb804ec695cda315db6b8789";
            loginrequest.Method = "POST";

            loginrequest.ContentLength = post.Length;
            loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            d999Register tmp = json.JsonDeserialize<d999Register>(sEmitResponse);
            if (tmp.SessionCookie!="" && tmp.SessionCookie!=null)
            {
                sessionCookie = tmp.SessionCookie;
                loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                post = "a=CreateUser&key=7a3ada10cb804ec695cda315db6b8789&s=" + sessionCookie + "&Username="+username+"&Password="+password;
                loginrequest.Method = "POST";

                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                 EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                 sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
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
            return sessionCookie != "" && sessionCookie != null;
        }

        public void GetDepositAddress()
        {
            if (sessionCookie != "" && sessionCookie != null)
            {
                HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                string post = "a=GetDepositAddress&s=" + sessionCookie + "&Currency=" + Currency;
                loginrequest.Method = "POST";

                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                d999deposit tmp = json.JsonDeserialize<d999deposit>(sEmitResponse);
                Parent.updateDeposit(tmp.Address);
            }
        }

        public override double GetLucky(string serverSeed, string clientSeed, int nonce)
        {
            Func<string, byte[]> strtobytes = s => Enumerable
                .Range(0, s.Length / 2)
                .Select(x => byte.Parse(s.Substring(x * 2, 2), NumberStyles.HexNumber))
                .ToArray();
            byte[] server = strtobytes(serverSeed);
            byte[] client = BitConverter.GetBytes(int.Parse(clientSeed)).Reverse().ToArray();
            byte[] num = BitConverter.GetBytes(nonce).Reverse().ToArray();
            //byte[] serverhash = serverSeedHash == null ? null : strtobytes(serverSeedHash);
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
                            return (result % 1000000)/10000.0;
                        }
                    }
                    hash = sha512.ComputeHash(hash);
                }
            }
            
        }
        public static double sGetLucky(string serverSeed, string clientSeed, int betNumber/*, long betResult*/, string serverSeedHash = null)
        {
            Func<string, byte[]> strtobytes = s => Enumerable
                .Range(0, s.Length / 2)
                .Select(x => byte.Parse(s.Substring(x * 2, 2), NumberStyles.HexNumber))
                .ToArray();
            byte[] server = strtobytes(serverSeed);
            byte[] client = BitConverter.GetBytes(int.Parse(clientSeed)).Reverse().ToArray();
            byte[] num = BitConverter.GetBytes(betNumber).Reverse().ToArray();
            byte[] serverhash = serverSeedHash == null ? null : strtobytes(serverSeedHash);
            byte[] data = server.Concat(client).Concat(num).ToArray();
            using (SHA512 sha512 = new SHA512Managed())
            {
                if (serverhash != null)
                    using (SHA256 sha256 = new SHA256Managed())
                        if (!sha256.ComputeHash(server).SequenceEqual(serverhash))
                        {
                            //throw new Exception("Server seed hash does not match server seed");
                        }
                byte[] hash = sha512.ComputeHash(sha512.ComputeHash(data));
                while (true)
                {
                    for (int x = 0; x <= 61; x += 3)
                    {
                        long result = (hash[x] << 16) | (hash[x + 1] << 8) | hash[x + 2];
                        if (result < 16000000)
                        {
                            return (result % 1000000)/10000.0;
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
