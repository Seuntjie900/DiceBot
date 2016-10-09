using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Net.Http;

namespace DiceBot
{
    class SafeDice : DiceSite
    {
        string accesstoken = "";
        HttpClientHandler ClientHandlr = new HttpClientHandler
        {
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            
        };
        HttpClient Client = null;
        bool ispd = false;
        DateTime LastBalance = DateTime.Now;
        public static string[] cCurrencies = new string[] { "Btc", "XMR"};
        public SafeDice(cDiceBot Parent)
        {
            
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = false;
            BetURL = "https://safedice.com/bets/";
            
            this.Parent = Parent;
            Name = "SafeDice";
            edge = 0.5m;
            Currencies = new string[]{"Btc", "XMR"};
            Currency = "Btc";
            SiteURL = "https://safedice.com/?r=1050";
        }
        long curen = 1;
        protected override void CurrencyChanged()
        {
            curen = Currency.ToLower() == "btc" ? 1 : 2;
            try
            {
                if (accesstoken != "" && accesstoken != null)
                {
                    /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + UID + "/sites/" + curen + "/me");
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new System.Net.Cookie("token", accesstoken, "", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                    string sEmitResponse = Client.GetStringAsync("accounts/" + UID + "/sites/" + curen + "/me").Result; //new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDiceWalletInfo tmp2 = json.JsonDeserialize<SafeDiceWalletInfo>(sEmitResponse);
                    balance = tmp2.balance / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateBalance((decimal)balance);
                    

                    Parent.updateBets(tmp2.win + tmp2.lose);
                    Parent.updateLosses(tmp2.lose);
                    wins = (int)tmp2.win;
                    losses = (int)tmp2.lose;
                    Parent.updateProfit((tmp2.amountWin - tmp2.amountLose) / (curen != 2 ? 100000000.0m : 1000000000000.0m));
                    profit = (tmp2.amountWin - tmp2.amountLose) / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateWagered(tmp2.wagered / (curen != 2 ? 100000000.0m : 1000000000000.0m));
                    wagered = tmp2.wagered / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateWins(tmp2.win);
                    
                    Parent.updateDeposit(GetDepositAddress());
                }

            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
            }
        }

        public void GetBalanceThread()
        {
            while (ispd)
            {
                try
                {
                    if ((DateTime.Now - LastBalance).TotalSeconds > 60 && accesstoken != "" && accesstoken != null && Client != null)
                    {
                        /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + UID + "/sites/" + curen + "/me");
                        loginrequest.CookieContainer = new CookieContainer();
                        loginrequest.CookieContainer.Add(new System.Net.Cookie("token", accesstoken, "", "safedice.com"));
                        loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                        HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                        string sEmitResponse = Client.GetStringAsync("accounts/" + UID + "/sites/" + curen + "/me").Result;//new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                        SafeDiceWalletInfo tmp2 = json.JsonDeserialize<SafeDiceWalletInfo>(sEmitResponse);
                        balance = tmp2.balance / (curen != 2 ? 100000000.0m : 1000000000000.0m); ;
                        Parent.updateBalance((decimal)balance);
                    }

                    if (accesstoken != "" && accesstoken != null && Client!=null)
                    {
                        /* loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/chats/en_US");
                        //loginrequest.Accept = "application/json, text/plain, ";
                        loginrequest.ContentType = " application/json;charset=utf-8";
                        loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                        loginrequest.CookieContainer = new CookieContainer();
                        loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                        HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                        string sEmitResponse = Client.GetStringAsync("chats/en_US").Result; //new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                        SDChat[] sdchat = json.JsonDeserialize<SDChat[]>(sEmitResponse);

                        foreach (SDChat chat in sdchat)
                        {
                            if (chat.id > lastchat)
                            {
                                lastchat = chat.id;
                                ReceivedChatMessage(string.Format("{0:hh:mm} ({1}) <{2}> {3}", chat.Time, chat.username, chat.accountId, chat.message));
                            }
                        }
                    }
                }
                catch (AggregateException e)
                {
                    Parent.DumpLog(e.InnerException.Message, 3);
                    Parent.DumpLog(e.InnerException.StackTrace, 4);
                }
                Thread.Sleep(1500);
            }
        }

        
        
        string serverhash = "";
        string client = "";
        
        long nonce = 0;
        long UID = 0;
        public override void Login(string Username, string Password, string twofa)
        {
            LastBalance = DateTime.Now;
            try
            {
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/auth/local");
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.Method = "POST";
                string post = "username=" + Username + "&password=" + Password + "&code=" + twofa;
                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                SafeDiceLogin tmp = json.JsonDeserialize<SafeDiceLogin>(sEmitResponse);
                accesstoken = tmp.token;
                if (accesstoken == "")
                    finishedlogin(false);
                else
                {
                    
                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/me?token=" + accesstoken);
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDicegetUserInfo tmp1 = json.JsonDeserialize<SafeDicegetUserInfo>(sEmitResponse);
                    
                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + tmp1.id + "/sites/" + curen + "/me");
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDiceWalletInfo tmp2 = json.JsonDeserialize<SafeDiceWalletInfo>(sEmitResponse);

                    ClientHandlr = new HttpClientHandler()
                    {
                        UseCookies = true,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        Proxy = (IWebProxy)this.Prox,
                        UseProxy = this.Prox != null
                    };

                    ClientHandlr.CookieContainer = new CookieContainer();
                    ClientHandlr.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://safedice.com/api/") };
                    Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                    Client.DefaultRequestHeaders.Add("authorization", "Bearer " + accesstoken);
                    Parent.updateBalance(tmp2.balance / (curen != 2 ? 100000000m : 1000000000000m));
                    balance = tmp2.balance / (curen != 2 ? 100000000.0m : 1000000000000.0m);

                    Parent.updateBets(tmp2.win + tmp2.lose);
                    Parent.updateLosses(tmp2.lose);
                    wins = (int)tmp2.win;
                    losses = (int)tmp2.lose;
                    Parent.updateProfit((tmp2.amountWin - tmp2.amountLose) / (curen != 2 ? 100000000.0m : 1000000000000.0m));
                    profit = (tmp2.amountWin - tmp2.amountLose) / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateWagered(tmp2.wagered / (curen != 2 ? 100000000.0m : 1000000000000.0m));
                    wagered = tmp2.wagered / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateWins(tmp2.win);
                    Parent.updateStatus("Logged in");
                    serverhash = tmp1.serverSeedHash;
                    client = tmp1.accountSeed;
                    nonce = tmp1.nonce;
                    UID = tmp1.id;
                    Parent.updateDeposit(GetDepositAddress());
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

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("401"))
                    {
                        //System.Windows.Forms.MessageBox.Show("Could not log in. Please ensure the username, passowrd and 2fa code are all correct.");
                    }

                }
                finishedlogin(false);

            }
        }
        int retrycount = 0;
        void PlaceBetThread(object Obj)
        {
            PlaceBetObj tmp = Obj as PlaceBetObj;
            bool High = tmp.High;
            decimal amount = tmp.Amount;
            decimal chance = tmp.Chance;
            try
            {
                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, (bool)High ? "High" : "Low"));
                SafeDiceBet tmpBet = new SafeDiceBet
                {
                    siteId = curen,
                    amount = (long)(amount * (curen != 2 ? 100000000m : 1000000000000m)),
                    payout = (decimal)(((long)((99.5m / chance) * (curen != 2 ? 100000000m : 1000000000000m))) / (curen != 2 ? 100000000.0m : 1000000000000.0m)),
                    isFixedPayout = false,
                    isRollLow = !(bool)High,
                    target = ((bool)High) ? (999999m - ((long)(chance * 10000m))).ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : ((long)(chance * 10000m)).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)
                };
                string post = json.JsonSerializer<SafeDiceBet>(tmpBet);
                /*HttpWebRequest betrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/dicebets");
                if (Prox != null)
                    betrequest.Proxy = Prox;
                betrequest.Method = "POST";
                SafeDiceBet tmpBet = new SafeDiceBet
                {
                    siteId = curen,
                    amount = (long)(amount * ( Currency!="2"? 100000000: 1000000000000)),
                    payout = (decimal)(((long)((99.5 / chance) * (curen != 2 ? 100000000 : 1000000000000))) / (curen != 2 ? 100000000.0 : 1000000000000.0)),
                    isFixedPayout = false,
                    isRollLow = !(bool)High,
                    target = ((bool)High) ? (999999 - ((long)(chance * 10000))).ToString() : ((long)(chance * 10000)).ToString()
                };
                string post = json.JsonSerializer<SafeDiceBet>(tmpBet);

                betrequest.ContentLength = post.Length;
                betrequest.ContentType = " application/json;charset=utf-8";
                betrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                betrequest.CookieContainer = new CookieContainer();
                using (var writer = new StreamWriter(betrequest.GetRequestStream()))
                {

                    writer.Write(post);
                    writer.Flush();
                    writer.Close();
                }
                string tmp = betrequest.ToString();
                HttpWebResponse EmitResponse = (HttpWebResponse)betrequest.GetResponse();*/
                string sEmitResponse = ""; //new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                HttpContent cont = new StringContent(post);
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Resp = "";
                using (var response = Client.PostAsync("dicebets", cont))
                {
                    Resp = response.Result.Content.ReadAsStringAsync().Result;
                    if (Resp == "{}")
                    {
                        Parent.updateStatus("Something went wrong! Please check your bet size.");
                        return;
                    }
                }

                SafeDiceBetResult tmpResult = json.JsonDeserialize<SafeDiceBetResult>(Resp);
                Bet bet = new Bet();
                bet.Amount = (decimal)tmpResult.amount / (curen != 2 ? 100000000m : 1000000000000m);
                bet.date = DateTime.Now;
                bet.Chance = (!tmpResult.isRollLow ? 100m - (decimal)tmpResult.target / 1000000m * 100m : (decimal)tmpResult.target / 1000000m * 100m);
                bet.high = !tmpResult.isRollLow;
                bet.clientseed = client;
                bet.Id = tmpResult.id;
                bet.nonce = nonce++;
                bet.Profit = tmpResult.profit / (curen != 2 ? 100000000m : 1000000000000m);
                bet.Roll = (decimal)tmpResult.roll / 10000m;
                bet.serverhash = serverhash;
                bet.uid = (int)tmpResult.accountId;
                balance += (decimal)bet.Profit;
                
                ++bets;
                wagered += (decimal)bet.Amount;
                bool win = false;
                if (tmpResult.isRollLow && tmpResult.roll < tmpResult.target)
                    win = true;
                else if (!tmpResult.isRollLow && tmpResult.roll > tmpResult.target)
                    win = true;
                if (win)
                    ++wins;
                else
                    ++losses;
                profit += (decimal)bet.Profit;
                FinishedBet(bet);
                retrycount = 0;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    System.Windows.Forms.MessageBox.Show("Error placing bet. Betting stopped");
                    /*if (e.Message.Contains("401"))
                    {
                        System.Windows.Forms.MessageBox.Show("Could not log in. Please ensure the username, passowrd and 2fa code are all correct.");
                    }*/

                }
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
            }
            catch
            {
                if (retrycount++ < 3)
                {
                    PlaceBetThread(tmp);
                    return;
                }
            }

        }

        protected override void internalPlaceBet(bool High,decimal amount, decimal chance)
        {
            Thread t = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            t.Start(new PlaceBetObj(High, amount, chance));

        }


        public override void ResetSeed()
        {
            try
            {
                /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/randomizeseed");
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.Method = "GET";

                loginrequest.Accept = "application/json, text/plain, /*";

                loginrequest.ContentType = " application/json;charset=utf-8";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                loginrequest.CookieContainer = new CookieContainer();
                loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                string sEmitResponse = Client.GetStringAsync("accounts/randomizeseed").Result;//new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                SDRandomize tmp = json.JsonDeserialize<SDRandomize>(sEmitResponse);
                serverhash = tmp.serverSeedHash;
                nonce = 1;
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

        
        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            ispd = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }
        void sendChatThread(object Message)
        {
            if (accesstoken != "" && accesstoken != null)
            {
                try
                {
                    /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/chats/en_US");
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.Method = "POST";

                    loginrequest.Accept = "application/json, text/plain, /*";

                    loginrequest.ContentType = " application/json;charset=utf-8";
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    string post = json.JsonSerializer<SDSendChat>(new SDSendChat { _message = (string)Message, room="en_US"});

                    using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                    {

                        writer.Write(post);
                        writer.Flush();
                        writer.Close();
                    }
                    HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                    string post = json.JsonSerializer<SDSendChat>(new SDSendChat { _message = (string)Message, room = "en_US" });
                    HttpContent cont = new StringContent(post);
                    cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    string Resp = "";
                    using (var response = Client.PostAsync("chats/en_US", cont))
                    {
                        Resp = response.Result.Content.ReadAsStringAsync().Result;

                    }
                    //new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                }
                catch (AggregateException e)
                {
                    Parent.DumpLog(e.InnerException.Message, 3);
                    Parent.DumpLog(e.InnerException.StackTrace, 4);
                }
            }
        }

        public override void SendChatMessage(string Message)
        {
            Thread t = new Thread(new ParameterizedThreadStart(sendChatThread));
            t.Start(Message);

        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                Parent.updateStatus(string.Format("Withdrawing {0:0.00000000} to {1}", Amount, Address));
                /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + UID + "/sites/1/withdraw");
                if (Prox != null)
                    loginrequest.Proxy = Prox;

                loginrequest.Method = "PUT";

                loginrequest.Accept = "application/json, text/plain, /*";

                loginrequest.ContentType = " application/json;charset=utf-8";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                loginrequest.CookieContainer = new CookieContainer();
                loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                string post = json.JsonSerializer<SDSendWIthdraw>(new SDSendWIthdraw { amount = (long)(Amount * 100000000), address = Address });

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                    writer.Flush();
                    writer.Close();
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                string post = json.JsonSerializer<SDSendWIthdraw>(new SDSendWIthdraw { amount = (long)(Amount * 100000000), address = Address });
                HttpContent cont = new StringContent(post);
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Resp = "";
                using (var response = Client.PutAsync("accounts/" + UID + "/sites/1/withdraw", cont))
                {
                    while (!response.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }
                    //if (response.Result.IsSuccessStatusCode)
                    {
                        Resp = response.Result.Content.ReadAsStringAsync().Result;
                    }
                }
                string sEmitResponse = "";//new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                
                balance -= Amount;
                Parent.updateBalance(balance);
                return true;
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                return false;
            }
        }

        public override void Donate(decimal Amount)
        {
            internalWithdraw( Amount, "1BoHcFQsUSot7jkHJcZMh1iUda3tEjzuBW");
        }

        public override bool Register(string username, string password)
        {
            //System.Windows.Forms.MessageBox.Show("Registration is temporarily disabled for Safe Dice. Please use the site https://safedice.com to register and then log in using the bot.");
            //return false;
            try
            {
                HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts");
                if (Prox != null)
                    loginrequest.Proxy = Prox;
                loginrequest.Method = "POST";
                string post = "username=" + username + "&referralId=" + 1050;
                loginrequest.ContentLength = post.Length;
                loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                string sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                SafeDiceLogin tmp = json.JsonDeserialize<SafeDiceLogin>(sEmitResponse);
                accesstoken = tmp.token;
                if (accesstoken == "")
                    finishedlogin(false);
                else
                {
                    



                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/me?token=" + accesstoken);
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDicegetUserInfo tmp1 = json.JsonDeserialize<SafeDicegetUserInfo>(sEmitResponse);
                    
                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + tmp1.id + "/sites/" + curen + "/me");
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "", "safedice.com"));
                    loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();
                    SafeDiceWalletInfo tmp2 = json.JsonDeserialize<SafeDiceWalletInfo>(sEmitResponse);



                    loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/userpass");
                    if (Prox != null)
                        loginrequest.Proxy = Prox;
                    loginrequest.ContentType = "application/json;charset=utf-8";
                    loginrequest.Method = "PUT";
                    loginrequest.CookieContainer = new CookieContainer();
                    loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    loginrequest.Headers.Add("Authorization", "Bearer " + accesstoken);
                    post = json.JsonSerializer<SDSetPw>(new SDSetPw { username = username, password = password });
                    loginrequest.ContentLength = post.Length;
                    using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                    {

                        writer.Write(post);
                    }
                    EmitResponse = (HttpWebResponse)loginrequest.GetResponse();
                    sEmitResponse = new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                    ClientHandlr = new HttpClientHandler()
                    {
                        UseCookies = true,
                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                        Proxy = (IWebProxy)this.Prox,
                        UseProxy = this.Prox != null
                    };

                    ClientHandlr.CookieContainer = new CookieContainer();
                    ClientHandlr.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                    Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://safedice.com/api/") };
                    Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                    Client.DefaultRequestHeaders.Add("authorization", "Bearer " + accesstoken);
                    Parent.updateBalance(tmp2.balance / (curen != 2 ? 100000000m : 1000000000000m));
                    balance = tmp2.balance / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    
                    Parent.updateBets(tmp2.win + tmp2.lose);
                    Parent.updateLosses(tmp2.lose);
                    wins = (int)tmp2.win;
                    losses = (int)tmp2.lose;
                    Parent.updateProfit((tmp2.amountWin - tmp2.amountLose) / (curen != 2 ? 100000000.0m : 1000000000000.0m));
                    profit = (tmp2.amountWin - tmp2.amountLose) / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateWagered(tmp2.wagered / (curen != 2 ? 100000000.0m : 1000000000000.0m));
                    wagered = tmp2.wagered / (curen != 2 ? 100000000.0m : 1000000000000.0m);
                    Parent.updateWins(tmp2.win);
                    Parent.updateStatus("Logged in");
                    serverhash = tmp1.serverSeedHash;
                    client = tmp1.accountSeed;
                    nonce = tmp1.nonce;
                    UID = tmp1.id;
                    Parent.updateDeposit(GetDepositAddress());
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

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    //Parent.updateStatus(sEmitResponse);
                    if (e.Message.Contains("401"))
                    {
                        //System.Windows.Forms.MessageBox.Show("Could not log in. Please ensure the username, passowrd and 2fa code are all correct.");
                    }

                }
                finishedlogin(false);

            }
            return false;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            string comb = nonce + ":" + client + server + ":" + nonce;

            SHA512 betgenerator = SHA512.Create();


            int charstouse = 5;

            List<byte> buffer = new List<byte>();

            foreach (char c in comb)
            {
                buffer.Add(Convert.ToByte(c));
            }

            //compute first hash
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            comb = hex.ToString();
            buffer = new List<byte>();

            //convert hash to new byte array
            foreach (char c in comb)
            {
                buffer.Add(Convert.ToByte(c));
            }

            hash = betgenerator.ComputeHash(buffer.ToArray());

            hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }

        public override bool Invest(decimal Amount)
        {
            try
            {
                Parent.updateStatus(string.Format("Investing {0:0.00000000}", Amount));
                /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/" + UID + "/sites/1/invest");
                if (Prox != null)
                    loginrequest.Proxy = Prox;

                loginrequest.Method = "POST";

                loginrequest.Accept = "application/json, text/plain, /*";

                loginrequest.ContentType = " application/json;charset=utf-8";
                loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
                loginrequest.CookieContainer = new CookieContainer();
                loginrequest.CookieContainer.Add(new Cookie("token", accesstoken, "/", "safedice.com"));
                string post = json.JsonSerializer<SDSendInvest>(new SDSendInvest { amount = (long)(Amount * 100000000) });

                using (var writer = new StreamWriter(loginrequest.GetRequestStream()))
                {

                    writer.Write(post);
                    writer.Flush();
                    writer.Close();
                }
                HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
                string post = json.JsonSerializer<SDSendInvest>(new SDSendInvest { amount = (long)(Amount * 100000000) });
                HttpContent cont = new StringContent(post);
                cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Resp = "";
                using (var response = Client.PostAsync("accounts/" + UID + "/sites/1/invest", cont))
                {
                    Resp = response.Result.Content.ReadAsStringAsync().Result;

                }
                string sEmitResponse = "";//new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                
                balance -= Amount;
                Parent.updateBalance(balance);
                return true;
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                return false;
            }
        }

        public static decimal sGetLucky(string server, string client, long nonce)
        {
            string comb = nonce + ":" + client + server + ":" + nonce;

            SHA512 betgenerator = SHA512.Create();


            int charstouse = 5;

            List<byte> buffer = new List<byte>();

            foreach (char c in comb)
            {
                buffer.Add(Convert.ToByte(c));
            }

            //compute first hash
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            comb = hex.ToString();
            buffer = new List<byte>();

            //convert hash to new byte array
            foreach (char c in comb)
            {
                buffer.Add(Convert.ToByte(c));
            }

            hash = betgenerator.ComputeHash(buffer.ToArray());

            hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }

        string GetDepositAddress()
        {
            /*HttpWebRequest loginrequest = (HttpWebRequest)HttpWebRequest.Create("https://safedice.com/api/accounts/"+UID+"/sites/"+curen+"/deposit");
            if (Prox != null)
                loginrequest.Proxy = Prox;
            loginrequest.Method = "GET";
            
            
            loginrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            loginrequest.Headers.Add("authorization", "Bearer " + accesstoken);
            
            HttpWebResponse EmitResponse = (HttpWebResponse)loginrequest.GetResponse();*/
            try
            {
                string sEmitResponse = Client.GetStringAsync("accounts/" + UID + "/sites/" + curen + "/deposit").Result; //new StreamReader(EmitResponse.GetResponseStream()).ReadToEnd();

                return json.JsonDeserialize<SDDEpost>(sEmitResponse).address;
            }
            catch (AggregateException e)
            {
                Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);
                return "";
            }
        }

        long lastchat = 0;

    }
    public class SDSendInvest
    {
        public decimal amount { get; set; }
    }
    public class SDChat
    {
        public long id { get; set; }
        public string target_id { get; set; }
        public string room { get; set; }
        public string target_username { get; set; }
        public string username { get; set; }
        public long role { get; set; }
        public string time { get; set; }
        public DateTime Time { get; set; }
        public string message { get; set; }
        public long accountId { get; set; }
        public string targetUsername { get; set; }
    }

    public class SafeDiceLogin
    {
        public string token { get; set; }
    }


    public class SafeDicegetUserInfo
    {
        public long id { get; set; }
        public string username { get; set; }
        public string authHashLink { get; set; }
        //public long referralId { get; set; }
        public string accountSeed { get; set; }
        public string serverSeedHash { get; set; }
        public long nonce { get; set; }
        public long role { get; set; }
        public bool isInvestmentEnabled { get; set; }

    }

    public class SafeDiceWalletInfo
    {
        public long balance { get; set; }
        public decimal shares { get; set; }
        public decimal kelly { get; set; }
        public long win { get; set; }
        public long lose { get; set; }
        public long amountLose { get; set; }
        public long amountWin { get; set; }
        public long wagered { get; set; }
        public long commission {get;set;}
    }
     public class SafeDiceBet
     {
         public long siteId { get; set; }
         public long amount { get; set; }
         public string target { get; set; }
         public decimal payout { get; set; }
         public bool isFixedPayout { get; set; }
         public bool isRollLow { get; set; }
     }
    public class SafeDiceBetResult
    {
        public long id { get; set; }
        public long accountId { get; set; }
        public string processTime { get; set; }
        public long amount { get; set; }
        public long profit { get; set; }
        public long roll { get; set; }
        public long target { get; set; }
        public bool isRollLow { get; set; }
        public decimal payout { get; set; }
    }

    public class SDRandomize
    {
        public string serverSeedHash { get; set; }
    }
    public class SDSendWIthdraw
    {
        public long amount { get; set; }
        public string address { get; set; }
    }
    public class SDSendChat
    {
        public string _message { get; set; }
        public string room { get; set; }
    }
    public class SDDiceBetCookie
    {
        public bool isRollLow { get; set; }
        public decimal pChance { get; set; }
        public bool isHotKeysEnabled { get; set; }
        public bool isFixedPayout { get; set; }
        public bool showAutoRoll { get; set; }
        public decimal autoRollLossMultiplier { get; set; }
    }
    public class SDSetPw
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class SDDEpost
    {
        public string address { get; set; }
    }
}
