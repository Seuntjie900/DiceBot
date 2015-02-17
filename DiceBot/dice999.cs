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
        int wins=0, losses =0;
        public dice999(cDiceBot Parent)
        {
            this.Parent = Parent;
            AutoInvest = false;
            AutoWithdraw = true;
            
            ChangeSeed = false;
            AutoLogin = false;
            BetURL = "https://www.999dice.com/Bets/?b=";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "999Dice";
            Tip = false;
            TipUsingName = true;
            Currency = "btc";
            Currencies = new string[] { "btc","doge","ltc" };
            /*Thread tChat = new Thread(GetMessagesThread);
            tChat.Start();*/
        }

        int BetRetries = 0;
        string next = "";
        void PlaceBetThread()
        {
            try
            {
                HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                string post = string.Format("a=GetServerSeedHash&s={0}", sessionCookie);
                string sEmitResponse = "";
                double chance = (999999.0) * (this.chance / 100.0);
                HttpWebResponse EmitResponse;
                if (next == "")
                {
                    
                    Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, this.chance, High ? "High" : "Low"));
                    loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
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
                string ClientSeed = r.Next(0, int.MaxValue).ToString();
                 post = string.Format("a=PlaceBet&s={0}&PayIn={1}&Low={2}&High={3}&ClientSeed={4}&Currency={5}", sessionCookie, amount * 100000000, High ? 999999 - (int)chance : 0, High ? 999999 : (int)chance, ClientSeed, Currency);
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
                d999Bet tmpBet = json.JsonDeserialize<d999Bet>(sEmitResponse);
                balance = (double)tmpBet.StartingBalance / 100000000.0 - (amount / 100000000.0) + ((double)tmpBet.PayOut / 100000000.0);

                profit += -(amount ) + (double)(tmpBet.PayOut / 100000000m);
                Bet tmp = new Bet();
                tmp.Amount = (decimal)amount / 100000000m;
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
                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(Wagered);
                Parent.updateWins(wins);
                BetRetries = 0;
                Parent.AddBet(tmp);
                sqlite_helper.InsertSeed(tmp.serverhash, tmp.serverseed);
                next = tmpBet.Next;
                Parent.GetBetResult((double)balance, win, (double)tmp.Profit);
                
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Something went wrong! betting stopped.");
            }
        }

        public override void PlaceBet(bool High)
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

        public override string GetbalanceValue()
        {
            return balance.ToString("0.00000000");
        }

        public override string GetSiteProfitValue()
        {
            throw new NotImplementedException();
        }

        public override string GetTotalBets()
        {
            return bets.ToString();
        }

        public override string GetMyProfit()
        {
            return profit.ToString();
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            
        }

        public override void GetSeed(long BetID)
        {
            
        }

        public override void SendChatMessage(string Message)
        {
            
        }

        
        public override bool Withdraw(double Amount, string Address)
        {
            HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
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

        public override bool Login(string Username, string Password)
        {
            return Login(Username, Password, "");
        }
        decimal Wagered = 0;
        public override bool Login(string Username, string Password, string twofa)
        {
            HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
            string post = "a=Login&Key=7a3ada10cb804ec695cda315db6b8789&Username=" + Username + "&Password=" + Password + (twofa != "" ? "&Totp=" + twofa : "");
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
            if (tmpU.SessionCookie!="")
            { 
                sessionCookie = tmpU.SessionCookie;
                balance = (double)tmpU.Balance/100000000.0;
                profit= (double)tmpU.Profit/100000000.0;
                Wagered = tmpU.Wagered/100000000m;
                bets = (int)tmpU.BetCount;
                wins = (int)tmpU.BetWinCount;
                losses = (int)tmpU.BetLoseCount;
                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(tmpU.BetCount);
                Parent.updateLosses(tmpU.BetLoseCount);
                Parent.updateProfit(profit);
                Parent.updateWagered(Wagered);
                Parent.updateWins(tmpU.BetWinCount);
                Parent.updateDeposit(tmpU.DepositAddress);
                uid = tmpU.Accountid;
            }      
            return sessionCookie != "";
        }
        public override bool Register(string username, string password)
        {
            HttpWebRequest loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
            string post = "a=CreateAccount&Key=7a3ada10cb804ec695cda315db6b8789";
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
            if (tmp.SessionCookie!="")
            {
                sessionCookie = tmp.SessionCookie;
                loginrequest = HttpWebRequest.Create("https://www.999dice.com/api/web.aspx") as HttpWebRequest;
                post = "a=CreateUser&Key=7a3ada10cb804ec695cda315db6b8789&s=" + sessionCookie + "&Username="+username+"&Password="+password;
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
            return sessionCookie != "";
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

    public class d999Bet
    {
        public long BetId { get; set; }
        public decimal PayOut { get; set; }
        public decimal Secret { get; set; }
        public decimal StartingBalance { get; set; }
        public string ServerSeed { get; set; }
        public string Next { get; set; }
    }
}
