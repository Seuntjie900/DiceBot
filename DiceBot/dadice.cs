using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace DiceBot
{
    class dadice: DiceSite
    {
        string key = "";
        string username = "";
        int wins = 0;
        int losses = 0;
        decimal wagered = 0;
        public dadice(cDiceBot Parent)
        {
            this.Parent = Parent;
            AutoInvest = false;
            AutoLogin = true;
            AutoWithdraw = false;
            ChangeSeed = false;
            BetURL = "";
            Thread t = new Thread(new ThreadStart(GetBalanceThread));
            t.Start();
            Name = "dadice";
            Tip = true;
            TipUsingName = true;
            
        }

        int betcount = 0;
        void PlaceBetThread(object _High)
        {
            try
            {
                bool High = (bool)_High;
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://dadice.com/api/roll");
                betrequest.Method = "POST";
                string post = string.Format("username={0}&key={1}&amount={2:0.00000000}&chance={3:00.0}&bet={4}", username, key, amount, chance, High ? "over" : "under");
                betrequest.ContentLength = post.Length;
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                DADICERollBase tmp = json.JsonDeserialize<DADICERollBase>(sEmitResponse2);
                if (tmp.status)
                {

                    balance += (double)tmp.roll.profit;
                    Parent.updateBalance((decimal)(balance));
                    Parent.updateBets(++bets);
                    Parent.updateLosses(tmp.roll.status.ToLower() == "won" ? losses : ++losses);
                    Parent.updateProfit(profit += (double)tmp.roll.profit);
                    Parent.updateWagered(wagered += tmp.roll.amount);
                    Parent.updateWins(tmp.roll.status.ToLower() == "won" ? ++wins : wins);
                    LastBalance = DateTime.Now;
                    Parent.AddBet(tmp.roll.ToBet());
                    Parent.GetBetResult(balance, tmp.roll.ToBet());

                }
                else
                {
                    Parent.updateStatus("Bet Failed: " + tmp.error);
                }
            }
            catch (Exception E)
            {
                Parent.updateStatus(E.Message);
            }
        }



        public override void PlaceBet(bool High)
        {
            Thread t = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            t.Start(High);
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override bool Withdraw(double Amount, string Address)
        {
            try
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://dadice.com/api/withdraw");
                betrequest.Method = "POST";
                string post = string.Format("username={0}&key={1}&coin=btc&payee={2}&amount={3:0.00000000}", username, key, Address, Amount);
                betrequest.ContentLength = post.Length;
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                DADICEBlance tmp = json.JsonDeserialize<DADICEBlance>(sEmitResponse2);
                return tmp.status;
            }
            catch (Exception E)
            {
                Parent.updateStatus(E.Message);
            }
            return false;
        }

        public override void Login(string Username, string Password)
        {
            Login(username, Password, "");
        }
        public override void Login(string Username, string Password, string twofa)
        {
            username = Username;
            key = Password;

            HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://dadice.com/api/balance");
            betrequest.Method = "POST";
            string post = string.Format("username={0}&key={1}", username, key);
            betrequest.ContentLength = post.Length;
            if (Prox != null)
                betrequest.Proxy = Prox;
            betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            using (var writer = new StreamWriter(betrequest.GetRequestStream()))
            {

                writer.Write(post);
            }
            HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
            string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
            DADICEBlance tmp = json.JsonDeserialize<DADICEBlance>(sEmitResponse2);
            if (tmp.status)
            {
                HttpWebRequest betrequest2 = (HttpWebRequest)HttpWebRequest.Create("https://dadice.com/api/deposit");
                betrequest2.Method = "POST";
                string post2 = string.Format("username={0}&key={1}&coin=btc", username, key);
                betrequest2.ContentLength = post.Length;
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                using (var writer = new StreamWriter(betrequest2.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse3 = (HttpWebResponse)betrequest2.GetResponse();
                string sEmitResponse3 = new StreamReader(EmitResponse3.GetResponseStream()).ReadToEnd();
                DADICEBlance dep = json.JsonDeserialize<DADICEBlance>(sEmitResponse3);
                if (dep.status)
                    Parent.updateDeposit(dep.address);


                balance = (double)tmp.balance;
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://stats.dadice.com/api/userinfo?username=" + Username);
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";


                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();

                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                LastBalance = DateTime.Now.AddSeconds(-20);
                DADICEStatsBase tmp2 = json.JsonDeserialize<DADICEStatsBase>(sEmitResponse);
                this.wagered = decimal.Parse(tmp2.user.stats.wagered);
                this.bets = tmp2.user.stats.bets;
                this.wins = tmp2.user.stats.won;
                this.losses = tmp2.user.stats.lost;
                this.profit = double.Parse(tmp2.user.stats.profit);

                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                Parent.updateBalance(balance);
                finishedlogin(true);      
            }
            else
            {
                finishedlogin(false);
            }
                 
        }

        public override bool Register(string username, string password)
        {
            if (System.Windows.Forms.MessageBox.Show("Please use the dadice website to register an account. Go there now?", "Register", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://www.dadice.com/?referrer=seuntjie");
            }
            return false;
        }

      

        bool isdd = true;
        DateTime LastBalance = DateTime.Now;
        public void GetBalanceThread()
        {
            while (this.isdd)
            {
                if (username != "" && key != "" && (DateTime.Now - LastBalance).TotalSeconds>15)
                {
                    try
                    {


                        HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://dadice.com/api/balance");
                        betrequest.Method = "POST";
                        string post = string.Format("username={0}&key={1}", username, key);
                        betrequest.ContentLength = post.Length;
                        if (Prox != null)
                            betrequest.Proxy = Prox;
                        betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                        using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                        {

                            writer.Write(post);
                        }
                        HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                        string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
                        DADICEBlance tmp = json.JsonDeserialize<DADICEBlance>(sEmitResponse2);
                        if (tmp.status)
                            balance = (double)tmp.balance;
                        Parent.updateBalance(balance);
                    }
                    catch (Exception E)
                    {
                        Parent.updateStatus(E.Message);
                    }
                    LastBalance = DateTime.Now;
                }
                Thread.Sleep(100);
            }
        }

       

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            isdd = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            Parent.updateStatus("Can't chat at this time. Sorry!");
        }

        void SendTipThread(object args)
        {
            try
            {
                HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://dadice.com/api/tip");
                betrequest.Method = "POST";
                string post = string.Format("username={0}&key={1}&payee={2}&amount={30:00000000}", username, key, (args as string[])[0], (args as string[])[1]);
                betrequest.ContentLength = post.Length;
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse2 = (HttpWebResponse)betrequest.GetResponse();
                string sEmitResponse2 = new StreamReader(EmitResponse2.GetResponseStream()).ReadToEnd();
            }
            catch (Exception E)
            {
                Parent.updateStatus(E.Message);
            }
        }

        

        public override void SendTip(string User, double amount)
        {
            Thread t = new Thread(new ParameterizedThreadStart(SendTipThread));
            t.Start(new string[]{User, amount.ToString("0.00000000")});
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
            string msg = /*nonce.ToString() + ":" + */client + "-" + nonce.ToString();
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
                    return (lucky%10000) / 100;
            }
            return 0;
        }

        new public static double sGetLucky(string server, string client, int nonce)
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
            string msg = /*nonce.ToString() + ":" + */client + "-" + nonce.ToString();
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
                    return (lucky % 10000) / 100;
            }
            return 0;
        }
    }

    public class DADICEBlance
    {
        public bool status { get; set; }
        public decimal balance { get; set; }
        public string error { get; set; }
        public string address { get; set; }
    }

    public class DADICERollBase
    {
        public bool status { get; set; }
        public DADICERoll roll { get; set; }
        public string error { get; set; }
    }
    public class DADICERoll
    {
        public string status { get; set; }
        public long id { get; set; }        
        public int nonce { get; set; }        
        public decimal chance { get; set; }
        public string bet { get; set; }
        public decimal result { get; set; }
        public decimal amount { get; set; }
        public decimal payout { get; set; }
        public long timestamp { get; set; }
        public decimal profit 
        { 
            get 
            {
                return payout>0?payout - amount:payout;
            } 
        }

        public Bet ToBet()
        {
            Bet tmp = new Bet 
            {
                Amount = amount,
                date = json.ToDateTime2(timestamp.ToString()),
                Id=id,
                Profit = profit,
                Roll = result,
                high = bet.ToLower().StartsWith("over"),
                Chance = chance,
                nonce=nonce,
                serverhash="",
                clientseed="",
                uid=0
            };

            return tmp;
        }
    }
    public class DADICEStatsBase
    {
        public bool status { get; set; }
        public DADICEStats user { get; set; }
        public string error { get; set; }
    }
    public class DADICEStats
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string signup_date_stamp { get; set; }
        public DADICEStatsStats stats { get; set; }
    }
    public class DADICEStatsStats
    {
        public int won { get; set; }
        public int lost { get; set; }
        public string wagered { get; set; }
        public string payout { get; set; }
        public string profit { get; set; }
        public int bets { get; set; }
    }
    
}
