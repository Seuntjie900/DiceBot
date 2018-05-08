
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using System.Globalization;

namespace DiceBot
{
    public class PD : DiceSite
    {
        GraphQL.Client.GraphQLClient GQLClient = new GraphQL.Client.GraphQLClient("https://api.primedice.com/graphql");
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
            _PasswordText = "Password: ";
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://api.primedice.com/bets/";
            
            this.Parent = Parent;
            Name = "PrimeDice";
            this.Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://primedice.com/?c=Seuntjie";
            
        }
        string userid = "";
        
        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (userid != null && (DateTime.Now- lastupdate).TotalSeconds>=30)
                    {
                        lastupdate = DateTime.Now;
                        GraphQLRequest LoginReq = new GraphQLRequest
                        {
                            Query = "query{user {activeSeed { serverSeedHash clientSeed nonce} id balances{available{currency amount value}} throttles{key value ttl type refType refId} statistic {bets wins losses amount profit currency value}}}"
                        };
                        GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                        pdUser user = Resp.GetDataFieldAs<pdUser>("user");
                        foreach (Statistic x in user.statistic)
                        {
                            if (x.currency.ToLower() == Currency.ToLower())
                            {
                                this.bets = (int)x.bets;
                                this.wins = (int)x.wins;
                                this.losses = (int)x.losses;
                                this.profit = (decimal)x.profit;
                                this.wagered = (decimal)x.amount;
                                break;
                            }
                        }
                        foreach (Balance x in user.balances)
                        {
                            if (x.available.currency.ToLower() == Currency.ToLower())
                            {
                                balance = (decimal)x.available.amount;
                                break;
                            }
                        }
                        Parent.updateBalance(balance);
                        Parent.updateWagered(wagered);
                        Parent.updateProfit(profit);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(),-1);
            }
        }

        public override bool Register(string Username, string Password)
        {/*
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
            */
            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {
            try
            {

                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "mutation{loginUser(name:\"" + Username + "\", password:\"" + Password + "\"" + (string.IsNullOrWhiteSpace(otp) ? "" : ",tfaToken:\"" + otp + "\"") + ") {activeSeed { serverSeedHash clientSeed nonce} id balances{available{currency amount value}} throttles{key value ttl type refType refId} statistic {bets wins losses amount profit currency value}}}"
                };
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                if (Resp.Errors.Length>0)
                {
                    Parent.DumpLog(Resp.Errors[0].Message, -1);
                    Parent.updateStatus(Resp.Errors[0].Message);
                    finishedlogin(false);
                    return;
                }
                pdUser user = Resp.GetDataFieldAs<pdUser>("loginUser");
                userid = user.id;
                if (string.IsNullOrWhiteSpace(userid))
                    finishedlogin(false);
                else
                {
                    foreach (Statistic x in user.statistic)
                    {
                        if (x.currency.ToLower() == Currency.ToLower())
                        {
                            this.bets = (int)x.bets;
                            this.wins = (int)x.wins;
                            this.losses = (int)x.losses;
                            this.profit = (decimal)x.profit;
                            this.wagered = (decimal)x.amount;
                            break;
                        }
                    }
                    foreach (Balance x in user.balances)
                    {
                        if (x.available.currency.ToLower() == Currency.ToLower())
                        {
                            balance = (decimal)x.available.amount;
                            break;
                        }
                    }

                    finishedlogin(true);
                    return;
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
                Parent.DumpLog(e.ToString(),-1);
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
                
                decimal tmpchance = High ? 99.99m - chance : chance;

                GraphQLResponse betresult = GQLClient.PostAsync(new GraphQLRequest { Query = "mutation{rollDice(amount:" + amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) + ", target:" + tmpchance.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo) + ",condition:" + (High ? "above" : "below") + ",currency:btc) { id iid nonce currency amount value payout result target condition createdAt seed{serverSeedHash serverSeed clientSeed nonce} user{balances{available{amount currency}} statistic{bets wins losses amount profit currency}}}}" }).Result;
                if (betresult.Errors.Length > 0)
                {
                    Parent.updateStatus(betresult.Errors[0].Message);
                }
                else
                {
                    RollDice tmp = betresult.GetDataFieldAs<RollDice>("rollDice");


                    Lastbet = DateTime.Now;
                    try
                    {

                        lastupdate = DateTime.Now;
                        foreach (Statistic x in tmp.user.statistic)
                        {
                            if (x.currency.ToLower() == Currency.ToLower())
                            {
                                this.bets = (int)x.bets;
                                this.wins = (int)x.wins;
                                this.losses = (int)x.losses;
                                this.profit = (decimal)x.profit;
                                this.wagered = (decimal)x.amount;
                                break;
                            }
                        }
                        foreach (Balance x in tmp.user.balances)
                        {
                            if (x.available.currency.ToLower() == Currency.ToLower())
                            {
                                balance = (decimal)x.available.amount;
                                break;
                            }
                        }
                        Bet tmpbet = tmp.ToBet();
                        tmpbet.Guid = tmp5.Guid;
                        FinishedBet(tmpbet);
                        retrycount = 0;
                    }
                    catch (Exception e)
                    {
                        Parent.DumpLog(e.ToString(), -1);
                        Parent.updateStatus("Some kind of error happened. I don't really know graphql, so your guess as to what went wrong is as good as mine.");
                    }
                }
            }
            catch (AggregateException e)
            {
                if (retrycount++ < 3)
                {
                    Thread.Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance, (bet as PlaceBetObj).Guid));
                    return;
                }
                if (e.InnerException.Message.Contains("429") || e.InnerException.Message.Contains("502"))
                {
                    Thread .Sleep(500);
                    placebetthread(new PlaceBetObj(High, amount, chance, (bet as PlaceBetObj).Guid));
                }
                

            }
            catch (Exception e2)
            {
                Parent.updateStatus("Error occured while trying to bet, retrying in 30 seconds. Probably.");
                Parent.DumpLog(e2.ToString(), -1);
            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.High = High;
            new Thread(new ParameterizedThreadStart(placebetthread)).Start(new PlaceBetObj(High, amount, chance, Guid));
        }

        Random R = new Random();
        public override void ResetSeed()
        {
            GraphQLRequest LoginReq = new GraphQLRequest
            {
                Query = "mutation{rotateSeed(clientSeed:\":" + R.Next().ToString() + "\" ){ clientSeed serverSeedHash nonce }}"
            };
            GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
            pdSeed user = Resp.GetDataFieldAs<pdSeed>("rotateSeed");

        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

      

       
        public override bool ReadyToBet()
        {
            if ((amount * 100000000m)<=100000 && (DateTime.Now - Lastbet).TotalMilliseconds < 350)
                return false;
            else
                return true;
            return true;
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                
                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "mutation{createWithdrawal(currency:"+Currency.ToLower()+", address:\""+Address+"\",amount:"+amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)+"){id name address hash amount userFee walletFee createdAt status currency}}"
                };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                return Resp.Data.createWithdrawal.id.Value != null;
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
                //mutation{sendTip(userId:"", amount:0, currency:btc,isPublic:true,chatId:""){id currency amount}}
                string userid = "";
                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "mutation{sendTip(userId:\""+userid+"\", amount:"+amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo)+", currency:"+Currency.ToLower()+ ",isPublic:true,chatId:\"14939265-52a4-404e-8a0c-6e3e10d915b4\"){id currency amount}}"
                };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                return Resp.Data.sendTip.id.Value != null;
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

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public class Sender
        {
            public string name { get; set; }
            public string __typename { get; set; }
        }

        public class Receiver
        {
            public string name { get; set; }
            public string __typename { get; set; }
        }

        public class _Tip
        {
            public string id { get; set; }
            public double amount { get; set; }
            public string currency { get; set; }
            public Sender sender { get; set; }
            public Receiver receiver { get; set; }
            public string __typename { get; set; }
        }
        public class Data2
        {
            public string message { get; set; }
            public _Tip tip { get; set; }
            public string __typename { get; set; }
        }
        public class pdSeed
        {
            public string serverSeedHash { get; set; }
            public object serverSeed { get; set; }
            public string clientSeed { get; set; }
            public int nonce { get; set; }
        }
        public class pdUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<object> roles { get; set; }
            public string __typename { get; set; }
            public Balance balance { get; set; }
            public Balance[] balances { get; set; }
            public List<Statistic> statistic { get; set; }
            public pdSeed activeSeed { get; set; }
        }

        public class ChatMessages
        {
            public string id { get; set; }
            public Data2 data { get; set; }
            public string createdAt { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
        }
        public class Chat
        {
            public string id { get; set; }
            public string __typename { get; set; }
        }
        public class Messages
        {
            public Chat chat { get; set; }
            public string id { get; set; }
            public Data2 data { get; set; }
            public string createdAt { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
        }
        public class RollDice
        {
            public string id { get; set; }
            public string iid { get; set; }
            public double result { get; set; }
            public decimal payoutMultiplier { get; set; }
            public double amount { get; set; }
            public double payout { get; set; }
            public string createdAt { get; set; }
            public double target { get; set; }
            public string condition { get; set; }
            public string currency { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
            public pdSeed seed { get; set; }
            public int nonce { get; set; }
            public Bet ToBet()
            {
                Bet bet = new Bet
                {
                    Amount = (decimal)amount,
                    Chance = condition.ToLower() == "above" ? 99.99m - (decimal)target : (decimal)target,
                    high = condition.ToLower() == "above",
                    Currency = currency,
                    date = DateTime.Now,
                    Id = iid,
                    Roll = (decimal)result,
                    UserName = user.name,
                    clientseed = seed.clientSeed,
                    serverhash = seed.serverSeedHash,
                    nonce = nonce
                };
                //User tmpu = User.FindUser(bet.UserName);
                /*if (tmpu == null)
                    bet.uid = 0;
                else
                    bet.uid = (int)tmpu.Uid;*/
                bool win = (((bool)bet.high ? (decimal)bet.Roll > (decimal)99.99 - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
                bet.Profit = win ? ((decimal)(payout - amount)) : ((decimal)-amount);
                return bet;
            }
        }
        public class Statistic
        {
            public decimal bets { get; set; }
            public decimal wins { get; set; }
            public decimal losses { get; set; }
            public double amount { get; set; }
            public double profit { get; set; }
            public string currency { get; set; }
            public string __typename { get; set; }
        }

        public class Data
        {
            public ChatMessages chatMessages { get; set; }
            public Messages messages { get; set; }
            public RollDice rollDice { get; set; }
            public pdUser user { get; set; }
            public RollDice bet { get; set; }
        }

        public class Payload
        {
            public Data data { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public string id { get; set; }
            public Payload payload { get; set; }
        }
        public class Role
        {
            public string name { get; set; }
            public string __typename { get; set; }
        }
        public class Balance
        {
            public Available available { get; set; }
            public string __typename { get; set; }
        }
        public class Available
        {
            public double amount { get; set; }
            public string currency { get; set; }
            public string __typename { get; set; }
        }
    }
    /*
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
                Id = id, 
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
       
    }*/
}
