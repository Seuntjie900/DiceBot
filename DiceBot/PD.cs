
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading; 
using System.Security.Cryptography;

namespace DiceBot
{
    public class PD : DiceSite
    {
        string accesstoken = "";
        
        public bool ispd = true;
        bool High = false;
        DateTime lastupdate = new DateTime();

        public PD(cDiceBot Parent)
        {
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = false;
            BetURL = "https://api.primedice.com/api/bets/";
            Thread t = new Thread(GetBalanceThread);
            t.Start();
            this.Parent = Parent;
            Name = "PrimeDice";
        }
        

        void GetBalanceThread()
        {
            while (ispd)
            if (accesstoken!="" && (DateTime.Now - lastupdate).TotalSeconds>60)
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/users/1?access_token=" + accesstoken);
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();

                pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                balance = tmpu.user.balance; //i assume
                bets = tmpu.user.bets;
                Parent.updateBalance((decimal)(balance / 100000000.0));
                Parent.updateBets(tmpu.user.bets);
                Parent.updateLosses(tmpu.user.losses);
                Parent.updateProfit(tmpu.user.profit / 100000000m);
                Parent.updateWagered(tmpu.user.wagered / 100000000m);
                Parent.updateWins(tmpu.user.wins);
                lastupdate = DateTime.Now;
                Thread.Sleep(1000);
            }
        }

        public override bool Register(string Username, string Password)
        {
            try
            {
                HttpWebRequest RegRequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/register");
                RegRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                RegRequest.Method = "POST";
                string post = "username=" + Username+"&affiliate=seuntjie";
                RegRequest.ContentLength = post.Length;
                using (var writer = new StreamWriter(RegRequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)RegRequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                pdlogin tmp = json.JsonDeserialize<pdlogin>(sEmitResponse);
                accesstoken = tmp.access_token;
                if (accesstoken == "")
                    return false;
                else
                {
                    HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/users/1?access_token=" + accesstoken);
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();

                    pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                    string s = "";
                    {
                        s = getDepositAddress();
                    }
                    if (s != null)
                    {
                        Parent.updateDeposit(s);
                    }
                    Parent.updateDeposit(tmpu.user.address);
                    balance = tmpu.user.balance; //i assume
                    bets = tmpu.user.bets;
                    Thread.Sleep(500);
                    HttpWebRequest PWRequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/password?access_token=" + accesstoken);
                    PWRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    PWRequest.Method = "POST";
                    post = "password=" + Password; ;
                    PWRequest.ContentLength = post.Length;
                    using (var writer = new StreamWriter(PWRequest.GetRequestStream()))
                    {

                        writer.Write(post);
                    }
                    HttpWebResponse EmitResponse3 = (HttpWebResponse)PWRequest.GetResponse();
                    string sEmitResponse3 = new StreamReader(EmitResponse3.GetResponseStream()).ReadToEnd();
                    lastupdate = DateTime.Now;
                    System.Windows.Forms.MessageBox.Show("Successfully registered " + Username + ". To to update profile settings or enable 2fa, please log into the site.");
                    Parent.updateStatus("Registered! Pleaes deposit some funds to start playing.");
                    return true;


                }

            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Failed to register account! Please make sure your username isn't already in use.");
            }

            return false;
        }        

        public override bool Login(string Username, string Password, string otp)
        {
            try
            {
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/login");
                loginrequest.Method = "POST";
                string post = "username=" + Username + "&password=" + Password + (!string.IsNullOrWhiteSpace(otp) ? "&otp=" + otp : "");
                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                pdlogin tmp = json.JsonDeserialize<pdlogin>(sEmitResponse);
                accesstoken = tmp.access_token;
                if (accesstoken == "")
                    return false;
                else
                {
                    HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/users/1?access_token=" + accesstoken);
                    betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                    string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();

                    pduser tmpu = json.JsonDeserialize<pduser>(sEmitResponse2);
                    balance = tmpu.user.balance; //i assume
                    bets = tmpu.user.bets;
                    Parent.updateBalance((decimal)(balance / 100000000.0));
                    Parent.updateBets(tmpu.user.bets);
                    Parent.updateLosses(tmpu.user.losses);
                    Parent.updateProfit(tmpu.user.profit / 100000000m);
                    Parent.updateWagered(tmpu.user.wagered / 100000000m);
                    string s = tmpu.user.address;
                    if (s==null)
                    {
                        s= getDepositAddress();
                    }
                    if (s != null)
                    {
                        Parent.updateDeposit(s);
                    }
                    Parent.updateWins(tmpu.user.wins);
                    lastupdate = DateTime.Now;
                    System.Windows.Forms.MessageBox.Show("Logged in!\n\nWelcome "+Username);
                    Parent.updateStatus("Logged in! Welcome "+Username);
                    return true;
                }
            }
            catch (WebException e)
            {
                string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                Parent.updateStatus(sEmitResponse);
                if (e.Message.Contains("401"))
                {
                    System.Windows.Forms.MessageBox.Show("Could not log in. Please ensure the username, passowrd and 2fa code are all correct.");
                }
                return false;

            }
        }

        void placebetthread()
        {
            try
            {
                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/bet?access_token=" + accesstoken);
                betrequest.Method = "POST";
                double tmpchance = High ? 100.0 - chance : chance;
                string post = string.Format("amount={0}&target={1}&condition={2}", (amount * 100000000).ToString(""), tmpchance.ToString("0.00"), High ? ">" : "<");
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                pdbetresult tmp = json.JsonDeserialize<pdbetresult>(sEmitResponse);
                tmp.bet.client = tmp.user.client;
                tmp.bet.serverhash = tmp.user.server;
                Parent.updateBalance((decimal)(tmp.user.balance / 100000000.0));
                Parent.updateBets(tmp.user.bets);
                Parent.updateLosses(tmp.user.losses);
                Parent.updateProfit(tmp.user.profit / 100000000m);
                Parent.updateWagered(tmp.user.wagered / 100000000m);
                Parent.updateWins(tmp.user.wins);
                
                lastupdate = DateTime.Now;

                balance = tmp.user.balance/ 100000000.0; //i assume
                bets = tmp.user.bets;
                Parent.AddBet(tmp.bet.toBet());
                Parent.GetBetResult(tmp.user.balance / 100000000.0, tmp.bet.win, tmp.bet.profit/100000000.0);
                
            }
            catch (WebException e)
            {
                string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                Parent.updateStatus(sEmitResponse);
                if (e.Message.Contains("429"))
                {
                    Thread .Sleep(500);
                    placebetthread();
                }
                

            }
        }

        public override void PlaceBet(bool High)
        {
            this.High = High;
            new Thread(placebetthread).Start();
        }

        public override void SetChance(string Chance)
        {
            Parent.updateChance(decimal.Parse(Chance));
            chance = double.Parse(Chance);
        }

        public override void SetAmount(double Amount)
        {
            amount = Amount;
            Parent.updateBet((decimal)Amount);
        }

        public override void ResetSeed()
        {
            try
            {
                Parent.updateStatus("Resetting Seed");
            HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/seed?access_token=" + accesstoken);
            betrequest.Method = "POST";
            string post = string.Format("seed={0}", Guid.NewGuid().ToString().Replace("-","").Substring(0,20));
            betrequest.ContentLength = post.Length;
            betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(betrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
            string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException e)
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

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue()
        {
            return (balance/100000000.0).ToString("0.00000000");            
        }

        public override string GetSiteProfitValue()
        {
            return "0";
        }

        public override string GetTotalBets()
        {
            return bets.ToString() ;
        }

        public override string GetMyProfit()
        {
            return (profit/100000000.0).ToString();
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Withdraw(double Amount, string Address)
        {
            try
            {
                
                Parent.updateStatus(string.Format("Withdrawing {0:0.00000000} to {1}", Amount, Address));
                Thread.Sleep(500);
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/withdraw?access_token=" + accesstoken);
                betrequest.Method = "POST";
                string post = string.Format("amount={0}&address={1}", (Amount * 100000000).ToString(""), Address);
                betrequest.ContentLength = post.Length;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();


                return true;
            }
            catch
            {
                return false;
            }
        }

        public override double GetLucky(string server, string client, int nonce)
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

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }

        public string getDepositAddress()
        {
            try
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://api.primedice.com/api/deposit?access_token=" + accesstoken);
                betrequest.Method = "GET";
                string post = string.Format("seed={0}", Guid.NewGuid().ToString().Replace("-","").Substring(0,20));
           
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

           
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                pdDeposit tmpa = json.JsonDeserialize<pdDeposit>(sEmitResponse);
                return tmpa.address;
            }
            catch (WebException e)
            {
                string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                Parent.updateStatus(sEmitResponse);
                if (e.Message.Contains("429"))
                {
                    Thread.Sleep(1500);
                    return getDepositAddress();
                }
                return "";
            }
        }
    }

    public class pdlogin
    {
        public bool admin { get; set; }
        public string access_token { get; set; }
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
        public string id { get; set; }
        public double profit { get; set; }
        public double amount { get; set; }
        public double target { get; set; }
        public bool win { get; set; }
        public string condition { get; set; }
        public string timestamp { get; set; }
        public double roll { get; set; }
        public long nonce { get; set; }
        public string client { get; set; }
        public string serverhash { get; set; }
        public string server { get; set; }
        public Bet toBet()
        {
            Bet tmp = new Bet 
            {
                Amount = (decimal)amount / 100000000m, 
                date = json.ToDateTime2(timestamp), 
                Id=decimal.Parse(id), 
                Profit=(decimal)profit/100000000m, 
                Roll=(decimal)roll, 
                high=condition==">",
                Chance = condition == ">" ? 100m - (decimal)target : (decimal)target, 
                nonce=nonce ,
                serverhash = serverhash,
                clientseed = client

            };
            return tmp;
        }
    }
    public class pduser
    {
        public double balance { get; set; }
        public int bets { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public pduser user { get; set; }
        public string client { get; set; }
        public string  server { get; set; }
        public decimal profit { get; set; }
        public decimal wagered { get; set; }
        public string address { get; set; }
    }
}
