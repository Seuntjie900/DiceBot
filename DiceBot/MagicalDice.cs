using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
//using Noesis.Javascript;
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
            maxRoll = 99.99m;
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
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {

            new Thread(new System.Threading.ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chance));
        }
        DateTime lastupdate = DateTime.Now;
        void getbalanceThread()
        {
            while (isMD)
            {
                if (authkey != "" && (DateTime.Now - lastupdate).TotalSeconds > 15 && isMD)
                {
                    lastupdate = DateTime.Now;
                    try
                    {
                        string s1 = Client.GetStringAsync("ajax.php?a=get_balance").Result;
                        balance = decimal.Parse(s1.Replace("\"", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                    catch (AggregateException e)
                    {
                        Parent.DumpLog(e.InnerException.Message, 3);
                        Parent.DumpLog(e.InnerException.StackTrace, 4);
                        return;
                    }
                    catch (Exception e)
                    {
                        Parent.DumpLog(e.Message, 3);
                        Parent.DumpLog(e.StackTrace, 4);
                        return;
                    }
                    Parent.updateBalance(balance);
                }
            }
        }

        int retrycount = 0;
        void PlaceBetThread(object Bool)
        {
            try
            {
                PlaceBetObj tmp9 = Bool as PlaceBetObj;
                
                bool High = tmp9.High;
                decimal amount = tmp9.Amount;
                decimal chance = tmp9.Chance;
                //decimal tmpchance = High ? 99.99 - chance : chance;
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

                        balance += (decimal)tmp2.Profit;
                        bets++;
                        if (tmp.bet_win)
                            wins++;
                        else
                            losses++;

                        wagered += (decimal)(tmp2.Amount);
                        profit += (decimal)(tmp2.Profit);
                        FinishedBet(tmp2);
                        retrycount = 0;
                    }
                    else
                    {
                        Parent.updateStatus(tmp.msg);
                    }
                }
                catch (Exception e)
                {
                    Parent.DumpLog(e.Message, 3);
                    Parent.DumpLog(e.StackTrace, 4);
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
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);

            }
            catch (Exception e2)
            {
                Parent.DumpLog(e2.Message, 3);
                Parent.DumpLog(e2.StackTrace, 4);
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
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                
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

        protected override bool internalWithdraw(decimal Amount, string Address)
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
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                return false;
            }

            

        }
        string csrf = "";
        string authkey = "";
        CookieContainer cookies;
        int cflevel = 0;
        bool doCFThing( string Response )
        {
            Thread.Sleep(4000);
            /*JavascriptContext JSC = new JavascriptContext();

            string s1 = Response;//new StreamReader(Response.GetResponseStream()).ReadToEnd();
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

            try
            {
                HttpResponseMessage Resp = Client.GetAsync("cdn-cgi/l/chk_jschl?jschl_vc=" + jschl_vc + "&pass=" + pass.Replace("+","%2") + "&jschl_answer=" + answer).Result;
                bool Found = false;

                foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://magicaldice.com")))
                {
                    if (c.Name == "cf_clearance")
                    {
                        Found = true;
                        break;
                    }
                }
                /*if (ClientHandlr.CookieContainer.Count==3)
                {
                    Thread.Sleep(2000);
                }*/
                /*if (!Found && cflevel++<5)
                    Found = doCFThing(Resp.Content.ReadAsStringAsync().Result);
                return Found;

            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
            }*/
            return false;
        }

        public override void Login(string Username, string Password, string twofa)
        {

            //get the cloudflare and site headers
            cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                Proxy = (IWebProxy)this.Prox,
                UseProxy = this.Prox != null
            };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://magicaldice.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));

            string s1 = "";
            
            try
            {
                HttpResponseMessage resp = Client.GetAsync("").Result;
                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                        cflevel = 0;
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                            {
                                System.Windows.Forms.MessageBox.Show("MagicalDice.com has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                            });
                        if (!doCFThing(s1))
                        {
                            finishedlogin(false);
                            return;
                        }
                        
                    }
                }
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                if (e.InnerException.Message.Contains("503"))
                {
                    //doCFThing(e.InnerException);
                }
            }

            
            
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("user_name", Username));
            pairs.Add(new KeyValuePair<string, string>("user_password", Password));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            try
            {
                string sEmitResponse = Client.PostAsync("", Content).Result.Content.ReadAsStringAsync().Result;
                
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
            }

            
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
                balance = decimal.Parse(s1.Replace("\"", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                Parent.updateBalance(balance);
                new Thread(GetDeposit).Start();
                finishedlogin(true);
            }
                catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                finishedlogin(false);
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.Message, 3);
                Parent.DumpLog(e.StackTrace, 4);
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

        decimal GetStats(string S, string Item)
        {
            string S1 = S.Substring(S.IndexOf(Item) + Item.Length);
            S1 = S1.Substring(S1.IndexOf("<BR>") + "<BR>".Length);
            S1 = S1.Substring(0, S1.IndexOf("<"));
            decimal t = decimal.Parse(S1, System.Globalization.NumberFormatInfo.InvariantInfo);
            return t;
        }

        public override bool Register(string username, string password)
        {

            //get the cloudflare and site headers
            cookies = new CookieContainer();
            ClientHandlr = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = (IWebProxy)this.Prox,
                UseProxy = this.Prox != null
            };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://magicaldice.com/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));

            string s1 = "";

            try
            {
                HttpResponseMessage resp = Client.GetAsync("").Result;
                if (resp.IsSuccessStatusCode)
                {
                    s1 = resp.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        s1 = resp.Content.ReadAsStringAsync().Result;
                        cflevel = 0;
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Windows.Forms.MessageBox.Show("MagicalDice.com has their cloudflare protection on HIGH\n\nThis will cause a slight delay in logging in. Please allow up to a minute.");
                        });
                        if (!doCFThing(s1))
                        {
                            finishedlogin(false);
                            return false;
                        }

                    }
                }
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                if (e.InnerException.Message.Contains("503"))
                {
                    //doCFThing(e.InnerException);
                }
            }



            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("register_name", username));
            pairs.Add(new KeyValuePair<string, string>("register_password", password));
            pairs.Add(new KeyValuePair<string, string>("register_password_check", password));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            
            try
            {
                string sEmitResponse = Client.PostAsync("", Content).Result.Content.ReadAsStringAsync().Result;

            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
            }
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
                balance = decimal.Parse(s1.Replace("\"", ""), System.Globalization.NumberFormatInfo.InvariantInfo);
                Parent.updateBalance(balance);
                new Thread(GetDeposit).Start();
                finishedlogin(true);
                return true;
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                finishedlogin(false);
                return false;
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.Message, 3);
                Parent.DumpLog(e.StackTrace, 4);
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

        public override bool InternalSendTip(string User, decimal amount)
        {
            
           string s1 = Client.GetStringAsync("ajax.php?a=get_csrf").Result;
            MDCsrf tmp = json.JsonDeserialize<MDCsrf>(s1);
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "tip_user"));
            pairs.Add(new KeyValuePair<string, string>("user_id", User));
            pairs.Add(new KeyValuePair<string, string>("amount", amount.ToString( System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("csrf", tmp.csrf));


            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            try
            {
                string sEmitResponse = Client.PostAsync("ajax.php", Content).Result.Content.ReadAsStringAsync().Result;
               
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
            }
            return false;
        }

        public override void Donate(decimal Amount)
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
        
        public decimal bet_amount { get; set; }
        public string bet_payout { get; set; }
        public string bet_game_type { get; set; }
        public string bet_game_value { get; set; }
        public decimal bet_roll { get; set; }
        public decimal bet_profit { get; set; }
        public string bet_origin { get; set; }
        public string bet_chat_display { get; set; }
        public bool bet_win { get; set; }
        public string bet_id { get; set; }
        public int error { get; set; }
        public string msg { get; set; }
        public Bet ToBet()
        {
            
            Bet tmp = new Bet {
            Amount= (decimal)(bet_amount / 100000000.0m),
            date = DateTime.Parse(bet_time),
            Id = long.Parse(bet_id),
            Profit = (decimal)(bet_profit/100000000.0m),
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
