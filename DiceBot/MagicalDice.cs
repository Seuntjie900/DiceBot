using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using Noesis.Javascript;
using System.Threading;

namespace DiceBot
{
    class MagicalDice: DiceSite
    {
        HttpClient Client;
        HttpClientHandler ClientHandlr;
        bool isMD = true;

        public MagicalDice(cDiceBot Parent)
        {
            this.Parent = Parent;
            maxRoll = 99.99;
            edge = 1;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            Tip = true;
            TipUsingName = false;
            BetURL = "https://magicaldice.com/ajax.php?a=get_bet_details&bet_id=";
            Name = "MagicalDice";
            SiteURL = "https://magicaldice.com/";
            Thread t = new Thread( getbalanceThread );
            t.Start();

        }
        protected override void internalPlaceBet(bool High)
        {

            new Thread(new System.Threading.ParameterizedThreadStart(PlaceBetThread)).Start(High);
        }
        DateTime lastupdate = DateTime.Now;
        void getbalanceThread()
        {
            if (authkey!="" && (DateTime.Now-lastupdate).TotalSeconds>15 && isMD)
            {
                lastupdate = DateTime.Now;
                string s1 = Client.GetStringAsync("ajax.php?a=get_balance").Result;
                balance = double.Parse(s1.Replace("\"", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                Parent.updateBalance(balance);
            }
        }

        int retrycount = 0;
        void PlaceBetThread(object Bool)
        {
            try
            {
                High = (bool)Bool;
                //double tmpchance = High ? 99.99 - chance : chance;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "place_bet"));
                pairs.Add(new KeyValuePair<string, string>("amount", (amount).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("win_chance", chance.ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("direction", High ? "over" : "under"));
                pairs.Add(new KeyValuePair<string, string>("auth_key", authkey));
                pairs.Add(new KeyValuePair<string, string>("origin", "manual"));


                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);

                string sEmitResponse = Client.PostAsync("ajax.php", Content).Result.Content.ReadAsStringAsync().Result;
                try
                {

                    MDBet tmp = json.JsonDeserialize<MDBet>(sEmitResponse);
                    if (tmp.error == 0)
                    {
                        Bet tmp2 = tmp.ToBet();

                        balance += (double)tmp2.Profit;
                        bets++;
                        if (tmp.bet_win)
                            wins++;
                        else
                            losses++;

                        wagered += (double)(tmp2.Amount);
                        profit += (double)(tmp2.Profit);
                        FinishedBet(tmp2);
                        retrycount = 0;
                    }
                    else
                    {
                        Parent.updateStatus(tmp.msg);
                    }
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
                    PlaceBetThread(High);
                    return;
                }
                if (e.InnerException.Message.Contains("429") || e.InnerException.Message.Contains("502"))
                {
                    Thread.Sleep(200);
                    PlaceBetThread(High);
                }


            }
            catch (Exception e2)
            {

            }
        }
        Random R = new Random();
        public override void ResetSeed()
        {
            //client_seed=278807cba0029ab1e21e8c51d6d7&generate=SET+CLIENT+SEED+%26+RANDOMIZE+SERVER+SEED
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("client_seed", R.Next(0, int.MaxValue).ToString()));
            pairs.Add(new KeyValuePair<string, string>("generate", "SET+CLIENT+SEED+%26+RANDOMIZE+SERVER+SEED"));
            
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            try
            {
                string sEmitResponse = Client.PostAsync("account/provably-fair", Content).Result.Content.ReadAsStringAsync().Result;
                
            }
            catch (AggregateException e)
            {
                
            }
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        void GetDeposit()
        {
            string s1 = Client.GetStringAsync("account/deposit_withdraw").Result;
            s1 = s1.Substring(s1.IndexOf("<p class=\"big\">") + "<p class=\"big\">".Length);
            s1 = s1.Substring(0, s1.IndexOf("<"));
            Parent.updateDeposit(s1);
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            string s1 = Client.GetStringAsync("ajax.php?a=get_csrf").Result;
            MDCsrf tmp = json.JsonDeserialize<MDCsrf>(s1);

            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "withdraw"));
            pairs.Add(new KeyValuePair<string, string>("address", Address));
            pairs.Add(new KeyValuePair<string, string>("amount", Amount.ToString()));
            pairs.Add(new KeyValuePair<string, string>("csrf", csrf));


            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            try
            {
                string sEmitResponse = Client.PostAsync("ajax.php", Content).Result.Content.ReadAsStringAsync().Result;
                return true;
            }
            catch (AggregateException e)
            {
                return false;
            }

            

        }
        string csrf = "";
        string authkey = "";
        CookieContainer cookies;
        int cflevel = 0;
        bool doCFThing( WebResponse Response )
        {
            Thread.Sleep(4000);
            JavascriptContext JSC = new JavascriptContext();
            
            string s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            string Script = "";
            string jschl_vc = s1.Substring(s1.IndexOf("jschl_vc"));
            jschl_vc = jschl_vc.Substring(jschl_vc.IndexOf("value=\"") + "value=\"".Length);
            jschl_vc = jschl_vc.Substring(0, jschl_vc.IndexOf("\""));
            string pass = s1.Substring(s1.IndexOf("pass"));
            pass = pass.Substring(pass.IndexOf("value=\"") + "value=\"".Length);
            pass = pass.Substring(0, pass.IndexOf("\""));

            //do the CF bypass thing and get the headers
            Script = s1.Substring(s1.IndexOf("var t,r,a,f,") + "var t,r,a,f, ".Length);
            string Script1 = "var " + Script.Substring(0, Script.IndexOf(";") + 1);
            string varName = Script.Substring(0, Script.IndexOf("="));
            string varNamep2 = Script.Substring(Script.IndexOf("\"") + 1);
            varName += "." + varNamep2.Substring(0, varNamep2.IndexOf("\""));
            Script1 += Script.Substring(Script.IndexOf(varName));
            Script1 = Script1.Substring(0, Script1.IndexOf("f.submit()"));
            Script1 = Script1.Replace("t.length", "magicaldice.com".Length + "");
            Script1 = Script1.Replace("a.value", "var answer");
            JSC.Run(Script1);
            string answer = JSC.GetParameter("answer").ToString();

            string url = "https://magicaldice.com/cdn-cgi/l/chk_jschl?jschl_vc=" + jschl_vc + "&pass=" + pass.Replace("+","%2") + "&jschl_answer=" + answer;
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(url);
            Request.Host = "magicaldice.com";
            Request.Referer = "https://magicaldice.com/";
            Request.KeepAlive = true;
            Request.Headers.Add("Accept-Encoding", "gzip, deflate");
            Request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            //Request.UserAgent = "DiceBot automated gambling";
            //Request.Headers.Add("referer", "magicaldice.com");

            if (Prox != null)
                Request.Proxy = Prox;
            
            Request.CookieContainer = cookies;
            Response = null;
            s1 = "";
            try
            {

                Response = (HttpWebResponse)Request.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                cookies = Request.CookieContainer;
                return true;
            }
            catch (WebException e)
            {

                if (e.Message.Contains("503") && cflevel++<5)
                {
                    return doCFThing(e.Response);
                }
                else
                {
                    return false;
                }
                
            }
        }

        public override void Login(string Username, string Password, string twofa)
        {

            //get the cloudflare and site headers
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create("https://magicaldice.com/");
            if (Prox != null)
                Request.Proxy = Prox;
            cookies = new CookieContainer();
            Request.CookieContainer = cookies;
            HttpWebResponse Response = null;
            string s1 = "";
            try
            {

                Response = (HttpWebResponse)Request.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                cookies = Request.CookieContainer;
            }
            catch (WebException e)
            {
                if (e.Message.Contains("503") && e.Response!=null)
                {
                    cflevel = 0;
                    cookies = Request.CookieContainer;
                    
                    if (doCFThing(e.Response))
                    {
                        
                    }
                    else
                    {
                        finishedlogin(false);
                        return;
                    }
                    //Response = (HttpWebResponse)e.Response;
                    
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                    finishedlogin(false);
                }
            }
            //log in
            Request = HttpWebRequest.Create("https://magicaldice.com/") as HttpWebRequest;
            if (Prox != null)
                Request.Proxy = Prox;
            
            foreach (Cookie c in Response.Cookies)
            {

                cookies.Add(c);
            }
            Request.CookieContainer = cookies;
            Request.Method = "POST";
            string post = string.Format("user_name={0}&user_password={1}", Username, Password);
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.ContentLength = post.Length;
            using (var writer = new StreamWriter(Request.GetRequestStream()))
            {
                string writestring = post as string;
                writer.Write(writestring);
            }
            try
            {

                Response = (HttpWebResponse)Request.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                
            }
            catch (WebException e)
            {
                Response = (HttpWebResponse)e.Response;
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                finishedlogin(false);
            }
            //get the auth key
            ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://magicaldice.com/") };
            try
            {
                s1 = Client.GetStringAsync("ajax.php?a=get_authkey").Result;

                //make auth key from s1*/
                MDAuthKey au = json.JsonDeserialize<MDAuthKey>(s1);
                //set auth key
                authkey = au.auth_key;
                //get user stats
                //disect html to get stats*/
                s1 = Client.GetStringAsync("account/statistics").Result;
                GetStats(s1);
                //get balance

                s1 = Client.GetStringAsync("ajax.php?a=get_balance").Result;
                balance = double.Parse(s1.Replace("\"", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                Parent.updateBalance(balance);
                new Thread(GetDeposit).Start();
                finishedlogin(true);
            }
            catch
            {
                finishedlogin(false);
            }
        }

        void GetStats(string S)
        {
            wagered = GetStats(S, "wagered");
            Parent.updateWagered(wagered);
            profit = GetStats(S, "profit");
            Parent.updateProfit(profit);
            bets = (int)GetStats(S, "bets");
            Parent.updateBets(bets);
            wins = (int)GetStats(S, "wins");
            Parent.updateWins(wins);
            losses = (int)GetStats(S, "losses");
            Parent.updateLosses(losses);
        }

        double GetStats(string S, string Item)
        {
            string S1 = S.Substring(S.IndexOf(Item) + Item.Length);
            S1 = S1.Substring(S1.IndexOf("<BR>") + "<BR>".Length);
            S1 = S1.Substring(0, S1.IndexOf("<"));
            double t = double.Parse(S1, System.Globalization.NumberFormatInfo.InvariantInfo);
            return t;
        }

        public override bool Register(string username, string password)
        {

            //get the cloudflare and site headers
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create("https://magicaldice.com/");
            if (Prox != null)
                Request.Proxy = Prox;
            cookies = new CookieContainer();
            Request.CookieContainer = cookies;
            HttpWebResponse Response = null;
            string s1 = "";
            try
            {

                Response = (HttpWebResponse)Request.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                cookies = Request.CookieContainer;
            }
            catch (WebException e)
            {
                if (e.Message.Contains("503") && e.Response != null)
                {
                    cflevel = 0;
                    cookies = Request.CookieContainer;

                    if (doCFThing(e.Response))
                    {

                    }
                    else
                    {
                        finishedlogin(false);
                        return false;
                    }
                    //Response = (HttpWebResponse)e.Response;

                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                    finishedlogin(false);
                    return false;
                }
            }
            //log in
            Request = HttpWebRequest.Create("https://magicaldice.com/") as HttpWebRequest;
            if (Prox != null)
                Request.Proxy = Prox;

            foreach (Cookie c in Response.Cookies)
            {

                cookies.Add(c);
            }
            Request.CookieContainer = cookies;
            Request.Method = "POST";
            string post = string.Format("register_name={0}&register_password={1}&register_password_check={1}", username, password);
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.ContentLength = post.Length;
            using (var writer = new StreamWriter(Request.GetRequestStream()))
            {
                string writestring = post as string;
                writer.Write(writestring);
            }
            try
            {

                Response = (HttpWebResponse)Request.GetResponse();
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();

            }
            catch (WebException e)
            {
                Response = (HttpWebResponse)e.Response;
                s1 = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                System.Windows.Forms.MessageBox.Show("Failed to log in. Please check your username and password.");
                finishedlogin(false);
                return false;
            }
            try
            {
                //get the auth key
                ClientHandlr = new HttpClientHandler { UseCookies = true, CookieContainer = cookies };
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://magicaldice.com/") };

                s1 = Client.GetStringAsync("ajax.php?a=get_authkey").Result;

                //make auth key from s1*/
                MDAuthKey au = json.JsonDeserialize<MDAuthKey>(s1);
                //set auth key
                authkey = au.auth_key;
                //get user stats
                //disect html to get stats*/
                s1 = Client.GetStringAsync("account/statistics").Result;
                GetStats(s1);
                //get balance

                s1 = Client.GetStringAsync("ajax.php?a=get_balance").Result;
                balance = double.Parse(s1.Replace("\"", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                Parent.updateBalance(balance);
                new Thread(GetDeposit).Start();
                finishedlogin(true);
                return true;
            }
            catch
            {
                finishedlogin(false);
                return false;
            }
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            isMD = false;
            authkey = "";
            lastupdate = DateTime.Now;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override void SendTip(string User, double amount)
        {
            string s1 = Client.GetStringAsync("ajax.php?a=get_csrf").Result;
            MDCsrf tmp = json.JsonDeserialize<MDCsrf>(s1);

            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "tip_user"));
            pairs.Add(new KeyValuePair<string, string>("user_id", User));
            pairs.Add(new KeyValuePair<string, string>("amount", amount.ToString( System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("csrf", csrf));


            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            try
            {
                string sEmitResponse = Client.PostAsync("ajax.php", Content).Result.Content.ReadAsStringAsync().Result;
               
            }
            catch (AggregateException e)
            {
                ;
            }
        }

        public override void Donate(double Amount)
        {
            SendTip("4236", Amount);
        }
        
    }

    public class MDAuthKey
    {
        public string auth_key { get; set; }
        public string user_id { get; set; }
        public string expires { get; set; }
    }

    public class MDBet
    {
        public string bet_user_id { get; set; }
        public string bet_time { get; set; }
        public string bet_seed_id { get; set; }
        public string bet_key_id { get; set; }
        public string bet_nonce { get; set; }
        
        public double bet_amount { get; set; }
        public string bet_payout { get; set; }
        public string bet_game_type { get; set; }
        public string bet_game_value { get; set; }
        public decimal bet_roll { get; set; }
        public double bet_profit { get; set; }
        public string bet_origin { get; set; }
        public string bet_chat_display { get; set; }
        public bool bet_win { get; set; }
        public string bet_id { get; set; }
        public int error { get; set; }
        public string msg { get; set; }
        public Bet ToBet()
        {
            
            Bet tmp = new Bet {
            Amount= (decimal)(bet_amount / 100000000.0),
            date = DateTime.Parse(bet_time),
            Id = long.Parse(bet_id),
            Profit = (decimal)(bet_profit/100000000.0),
            Roll = bet_roll,
            
            high = bet_game_type=="over",
            nonce = long.Parse(bet_nonce, System.Globalization.NumberFormatInfo.InvariantInfo),
            serverhash = bet_seed_id,
            clientseed = bet_key_id};
            decimal tchance = decimal.Parse(bet_game_value, System.Globalization.NumberFormatInfo.InvariantInfo);
            tmp.Chance = (tmp.high ? 99.99m - tchance : tchance);
            return tmp;
        }
    }
    public class MDCsrf
    {
        public string csrf { get; set; }
    }
}
