using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http;

namespace DiceBot
{
    class dadice: DiceSite
    {
        string key = "";
        string username = "";
        public dadice(cDiceBot Parent)
        {
            register = false;
            maxRoll = 99.99m;
            this.Parent = Parent;
            AutoInvest = false;
            AutoLogin = true;
            AutoWithdraw = true;
            ChangeSeed = false;
            BetURL = "";
            Thread t = new Thread(new ThreadStart(GetBalanceThread));
            t.Start();
            Name = "dadice";
            Tip = true;
            TipUsingName = true;
            SiteURL = "https://www.dadice.com/?referrer=seuntjie";
            
        }
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://dadice.com/api/") };
        HttpClientHandler ClientHandlr;
        int betcount = 0;
        DateTime Lastbet = DateTime.Now;
        int retrycount = 0;
        void PlaceBetThread(object _High)
        {
            try
            {
                Lastbet = DateTime.Now;
                bool High = (bool)_High;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username",username));
                pairs.Add(new KeyValuePair<string, string>("key", key));
                pairs.Add(new KeyValuePair<string, string>("amount", amount.ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("chance", chance.ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("bet", High ? "over" : "under"));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                DADICERollBase tmp = new DADICERollBase();
                using (var response = Client.PostAsync("roll", Content))
                {
                    string sEmitResponse2 = response.Result.Content.ReadAsStringAsync().Result;
                    tmp = json.JsonDeserialize<DADICERollBase>(sEmitResponse2);
                }
                if (tmp.status)
                {

                    balance += (decimal)tmp.roll.profit;
                    
                    ++bets;
                    ++losses;
                    profit += (decimal)tmp.roll.profit;
                    wagered += (decimal)tmp.roll.amount;
                    ++wins;
                    LastBalance = DateTime.Now;
                    betcount = 0;
                    Lastbet = DateTime.Now;
                    FinishedBet(tmp.roll.ToBet());
                    retrycount = 0;

                }
                else
                {
                    Parent.updateStatus("Bet Failed: " + tmp.error);
                }
            }
            catch (Exception E)
            {
                if (betcount++<3)
                {
                    PlaceBetThread(_High);
                }
                else
                {
                    
                    Parent.updateStatus(E.Message);
                }
                
            }
        }



        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
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

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", username));
                pairs.Add(new KeyValuePair<string, string>("key", key));
                pairs.Add(new KeyValuePair<string, string>("amount", Amount.ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("payee", Address));                

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                DADICEBlance tmp = new DADICEBlance();
                using (var response = Client.PostAsync("withdraw", Content))
                {
                    string sEmitResponse2 = response.Result.Content.ReadAsStringAsync().Result;
                    tmp = json.JsonDeserialize<DADICEBlance>(sEmitResponse2);
                }
                LastBalance = DateTime.Now.AddMinutes(-1);
                return tmp.status;
            }
            catch (Exception E)
            {
                Parent.updateStatus(E.Message);
            }
            return false;
        }

        
        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://dadice.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            username = Username;
            key = Password;
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("username", username));
            pairs.Add(new KeyValuePair<string, string>("key", key));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            DADICEBlance tmp = new DADICEBlance();
            using (var response = Client.PostAsync("balance", Content))
            {
                string Resp = response.Result.Content.ReadAsStringAsync().Result;
                tmp = json.JsonDeserialize<DADICEBlance>(Resp);
            }            
            if (tmp.status)
            {
                pairs.Add(new KeyValuePair<string, string>("coin", "btc"));
                Content = new FormUrlEncodedContent(pairs);
                DADICEBlance dep = new DADICEBlance();
                using (var response = Client.PostAsync("deposit", Content))
                {
                    string Resp = response.Result.Content.ReadAsStringAsync().Result;
                    dep = json.JsonDeserialize<DADICEBlance>(Resp);
                }
                
                if (dep.status)
                    Parent.updateDeposit(dep.address);
                
                balance = (decimal)tmp.balance;

                
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://stats.dadice.com/api/userinfo?username=" + Username);
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";


                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();

                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                LastBalance = DateTime.Now.AddSeconds(-20);
                DADICEStatsBase tmp2 = json.JsonDeserialize<DADICEStatsBase>(sEmitResponse);
                this.wagered = decimal.Parse(tmp2.user.stats.wagered, System.Globalization.NumberFormatInfo.InvariantInfo);
                this.bets = tmp2.user.stats.bets;
                this.wins = tmp2.user.stats.won;
                this.losses = tmp2.user.stats.lost;
                this.profit = decimal.Parse(tmp2.user.stats.profit);

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

                        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                        pairs.Add(new KeyValuePair<string, string>("username", username));
                        pairs.Add(new KeyValuePair<string, string>("key", key));
                        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                        DADICEBlance tmp = new DADICEBlance();
                        using (var response = Client.PostAsync("balance", Content))
                        {
                            string Resp = response.Result.Content.ReadAsStringAsync().Result;
                            tmp = json.JsonDeserialize<DADICEBlance>(Resp);
                        }
                        if (tmp.status)
                            balance = (decimal)tmp.balance;
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
            return (DateTime.Now-Lastbet).TotalMilliseconds>500;
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
        public override void Donate(decimal Amount)
        {
            SendTip("seuntjie", Amount);
        }
        void SendTipThread(object args)
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", username));
                pairs.Add(new KeyValuePair<string, string>("key", key));
                pairs.Add(new KeyValuePair<string, string>("payee", (args as string[])[0]));
                pairs.Add(new KeyValuePair<string, string>("amount", (args as string[])[1]));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                
                using (var response = Client.PostAsync("tip", Content))
                {
                    string Resp = response.Result.Content.ReadAsStringAsync().Result;
                    
                }
            }
            catch (Exception E)
            {
                Parent.updateStatus(E.Message);
            }
        }




        public override bool InternalSendTip(string User, decimal amount)
        {
            Thread t = new Thread(new ParameterizedThreadStart(SendTipThread));
            t.Start(new string[]{User, amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) });
            return false;
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

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return (lucky%10000) / 100;
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

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
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
