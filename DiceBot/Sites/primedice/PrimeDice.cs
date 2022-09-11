
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
//using GraphQL.Client.Http;
using System.Globalization;
using DiceBot.Core;
using static DiceBot.PrimediceSchema;
namespace DiceBot.Schema.BetKing
{


}
namespace DiceBot
{
    public class SiteSchemaBase
    {

    }

    public class PrimediceSchema : SiteSchemaBase
    {

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
            public string seedHash { get; set; }
            public string seed { get; set; }
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
            public pdSeed activeServerSeed { get; set; }
            public pdSeed activeClientSeed { get; set; }
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
        public class DiceState
        {
            public double result { get; set; }
            public double target { get; set; }
            public string condition { get; set; }

        }
        public class RollDice
        {
            public string id { get; set; }
            public string iid { get; set; }
            public decimal payoutMultiplier { get; set; }
            public double amount { get; set; }
            public double payout { get; set; }
            public string createdAt { get; set; }
            public string currency { get; set; }
            public DiceState state { get; set; }
            public pdUser user { get; set; }
            public string __typename { get; set; }
            public pdSeed serverSeed { get; set; }
            public pdSeed clientSeed { get; set; }
            public int nonce { get; set; }
            public Bet ToBet(decimal maxroll)
            {
                Bet bet = new Bet
                {
                    Amount = (decimal)amount,
                    Chance = state.condition.ToLower() == "above" ? maxroll - (decimal)state.target : (decimal)state.target,
                    high = state.condition.ToLower() == "above",
                    Currency = currency,
                    date = DateTime.Now,
                    Id = id,
                    Roll = (decimal)state.result,
                    UserName = user.name,
                    clientseed = clientSeed.seed,
                    serverhash = serverSeed.seedHash,
                    nonce = nonce
                };
                bool win = (((bool)bet.high ? (decimal)bet.Roll > maxroll - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
                bet.Profit = win ? ((decimal)(payout - amount)) : ((decimal)-amount);
                return bet;
            }
        }
        public class Statistic
        {
            public string game { get; set; }
            public decimal bets { get; set; }
            public decimal wins { get; set; }
            public decimal losses { get; set; }
            public double betAmount { get; set; }
            public double? profit { get; set; }
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

    public class PrimeDice : DiceSite
    {
        protected string URL = "https://api.primedice.com/graphql";
        protected string RolName = "primediceRoll";
        protected string GameName = "CasinoGamePrimedice";
        protected string EnumName = "CasinoGamePrimediceConditionEnum";
        protected string StatGameName = "primedice";

        private static string[] _sCurrencies = new string[]
        {
            "BTC",
            "ETH",
            "LTC",
            "DOGE",
            "BCH",
            "XRP",
            "TRX",
            "EOS",
            "BNB",
            "USDT",
            "APE",
            "BUSD",
            "CRO",
            "DAI",
            "LINK",
            "SAND",
            "SHIB",
            "UNI",
            "USDC"
        };


        public static string[] sCurrencies => _sCurrencies.Select(x=>x.ToUpperInvariant()).OrderBy(x => x).ToArray();

        GraphQL.Client.GraphQLClient GQLClient;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        bool getid = false;

        public PrimeDice(cDiceBot Parent)
        {
            _PasswordText = "API Key: ";
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://api.primedice.com/bets/";
            this.Currencies = sCurrencies;
            this.Currency = "Btc";
            this.Parent = Parent;
            Name = "PrimeDice";
            this.Tip = false;
            this.Vault = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://primedice.com";

            if (File.Exists("slow") || File.Exists("slow.txt"))
            {
                getid = true;
            }
        }

        string userid = "";

        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }


        void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (userid != null && ((DateTime.Now - lastupdate).TotalSeconds >= 30 || ForceUpdateStats))
                    {

                        ForceUpdateStats = false;
                        lastupdate = DateTime.Now;

                        GraphQLRequest LoginReq = new GraphQLRequest
                        {
                            Query = "query DiceBotGetBalance{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses betAmount profit currency}}}"
                        };

                        GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                        pdUser user = Resp.GetDataFieldAs<pdUser>("user");

                        foreach (Statistic x in user.statistic)
                        {
                            if (x.currency.ToLower() == Currency.ToLower() && x.game == StatGameName)
                            {
                                this.bets = (int)x.bets;
                                this.wins = (int)x.wins;
                                this.losses = (int)x.losses;
                                this.profit = x.profit.HasValue ? (decimal)x.profit.Value : 0;
                                this.wagered = (decimal)x.betAmount;
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
                Parent.DumpLog(e.ToString(), -1);
            }
        }

        public override bool Register(string Username, string Password)
        {
            return false;
        }

        public override void Login(string Username, string Password, string otp)
        {
            try
            {

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                GQLClient = new GraphQL.Client.GraphQLClient(URL);

                GQLClient.DefaultRequestHeaders.Add("authorization", string.Format("Bearer {0}", (object)Password));
                GQLClient.DefaultRequestHeaders.Add("x-access-token", Password);

               // GQLClient.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36";


                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "query DiceBotLogin{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses betAmount profit currency}}}"
                };

                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                pdUser user = Resp.GetDataFieldAs<pdUser>("user");

                userid = user.id;

                if (string.IsNullOrWhiteSpace(userid))
                {
                    finishedlogin(false);
                }
                else
                {
                    foreach (Statistic x in user.statistic)
                    {
                        if (x.currency.ToLower() == Currency.ToLower() && x.game == StatGameName)
                        {
                            this.bets = (int)x.bets;
                            this.wins = (int)x.wins;
                            this.losses = (int)x.losses;
                            this.profit = x.profit.HasValue ? (decimal)x.profit.Value : 0;
                            this.wagered = (decimal)x.betAmount;
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
                    ispd = true;
                    Thread t = new Thread(GetBalanceThread);
                    t.Start();
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
                Parent.DumpLog(e.ToString(), -1);
                finishedlogin(false);
            }
        }

        int retrycount = 0;
        DateTime Lastbet = DateTime.Now;

        public override void UpdateMirror(string url)
        {
            if (url != "" && MirrorList.Contains(url))
            {
                BetURL = $"https://api.{url}/bets/";
                SiteURL = $"https://{url}/";
                URL = $"https://api.{url}/graphql";
                //base.UpdateMirror(url);
            }
        }

        void placebetthread(object bet)
        {

            try
            {
                PlaceBetObj tmp5 = bet as PlaceBetObj;
                decimal amount = tmp5.Amount;
                decimal chance = tmp5.Chance;
                bool High = tmp5.High;

                decimal tmpchance = High ? maxRoll - chance : chance;
                var Request = new GraphQLRequest
                {
                    Query = @"mutation DiceBotDiceBet($amount: Float! 
  $target: Float!
  $condition: " + EnumName + @"!
  $currency: CurrencyEnum!
  $identifier: String!){ " + RolName + "(amount: $amount, target: $target,condition: $condition,currency: $currency, identifier: $identifier)" +
  " { id nonce currency amount payout state { ... on " + GameName + " { result target condition } } createdAt serverSeed{seedHash seed nonce} clientSeed{seed} user{balances{available{amount currency}} statistic{game bets wins losses betAmount profit currency}}}}"
                };
                Request.Variables = new
                {
                    amount = amount,//.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo),
                    target = tmpchance,//.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo),
                    condition = (High ? "above" : "below"),
                    currency = Currency.ToLower(),
                    identifier = "0123456789abcdef",
                };
                GraphQLResponse betresult = GQLClient.PostAsync(Request).Result;

                if (betresult.Errors != null)
                {
                    if (betresult.Errors.Length > 0)
                        Parent.updateStatus(betresult.Errors[0].Message);
                }
                if (betresult.Data != null)
                {
                    RollDice tmp = betresult.GetDataFieldAs<RollDice>(RolName);


                    Lastbet = DateTime.Now;
                    try
                    {

                        lastupdate = DateTime.Now;
                        foreach (Statistic x in tmp.user.statistic)
                        {
                            if (x.currency.ToLower() == Currency.ToLower() && x.game == StatGameName)
                            {
                                this.bets = (int)x.bets;
                                this.wins = (int)x.wins;
                                this.losses = (int)x.losses;
                                this.profit = x.profit.HasValue ? (decimal)x.profit.Value : 0;
                                this.wagered = (decimal)x.betAmount;
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
                        Bet tmpbet = tmp.ToBet(maxRoll);

                        if (getid)
                        {
                            var IdRequest = new GraphQLRequest { Query = " query DiceBotGetBetId($betId: String!){bet(betId: $betId){iid}}" };
                            string betid = tmpbet.Id;
                            IdRequest.Variables = new { betId = betid /*tmpbet.Id*/ };
                            GraphQLResponse betresult2 = GQLClient.PostAsync(IdRequest).Result;

                            if (betresult2.Data != null)
                            {
                                //RollDice tmp2 = betresult2.GetDataFieldAs<RollDice>(RolName);

                                //tmpbet.Id = tmp2.iid;
                                tmpbet.Id = betresult2.Data.bet.iid;
                                if (tmpbet.Id.Contains("house:"))
                                    tmpbet.Id = tmpbet.Id.Substring("house:".Length);
                            }
                        }

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
                    Thread.Sleep(500);
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

        DBRandom R = new DBRandom();

        public override void ResetSeed()
        {
            try
            {

                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "mutation DiceBotRotateSeed ($seed: String!){rotateServerSeed{ seed seedHash nonce } changeClientSeed(seed: $seed){seed}}"
                };

                LoginReq.Variables = new { seed = R.Next(0, int.MaxValue).ToString() };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;

                if (Resp.Data != null)
                {
                    pdSeed user = Resp.GetDataFieldAs<pdSeed>("rotateServerSeed");
                }
                else if (Resp.Errors != null && Resp.Errors.Length > 0)
                {
                    foreach (var x in Resp.Errors)
                    {
                        Parent.DumpLog("GRAPHQL ERROR PD RESETSEED: " + x.Message, 1);
                    }
                }

            }
            catch (Exception e)
            {
                Parent.updateStatus("Failed to reset seed.");
            }
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            //if ((amount * 100000000m)<=100000 && (DateTime.Now - Lastbet).TotalMilliseconds < 350)
            // return false;
            //else
            //    return true;
            return true;
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {

                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "mutation DiceBotWithdrawal{createWithdrawal(currency:" + Currency.ToLower() + ", address:\"" + Address + "\",amount:" + amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) + "){id name address hash amount walletFee createdAt status currency}}"
                };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                return Resp.Data.createWithdrawal.id.Value != null;
            }
            catch
            {
                return false;
            }
        }

        public override bool InternalSendToVault(decimal amount)
        {
            try
            {

                GraphQLRequest req = new GraphQLRequest
                {
                    Query = "mutation DiceBotVault{createVaultDeposit(currency:" + Currency.ToLower() + ", amount:" + amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) + "){id}}"
                };

                GraphQLResponse Resp = GQLClient.PostAsync(req).Result;

                return Resp.Data.createVaultDeposit.id.Value != null;

            }
            catch (Exception e)
            {

            }
            return false;
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        new public static decimal sGetLucky(string server, string client, long nonce)
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
            if (accesstoken != "")
            {
                try
                {
                    string sEmitResponse = Client.GetStringAsync("logout?api_key=" + accesstoken).Result;
                    accesstoken = "";
                }
                catch
                {

                }
            }
        }

        public override void Donate(decimal Amount)
        {
            SendTip("WinMachine", Amount);
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                //mutation{sendTip(userId:"", amount:0, currency:btc,isPublic:true,chatId:""){id currency amount}}
                string userid = GetUid(User);
                string chatid = "";
                string Tippayload = "mutation SendTip($userId: String!, $amount: Float!, $currency: CurrencyEnum!, $isPublic: Boolean, $chatId: String!, $tfaToken: String) {sendTip(userId: $userId, amount: $amount, currency: $currency, isPublic: $isPublic, chatId: $chatId, tfaToken: $tfaToken) { id amount currency user { id name __typename } sendBy { id name balances { available { amount currency __typename } vault { amount currency __typename } __typename } __typename } __typename } }";
                GraphQLRequest req = new GraphQLRequest();
                req.Query = Tippayload;
                req.Variables = new
                {
                    name = User,
                    amount = amount,
                    isPublic = true,
                    userId = userid,
                    chatId = chatid,
                    currency = Currency,
                    tfaToken = (string)null
                };
                GraphQLResponse Resp = GQLClient.PostAsync(req).Result;
                return Resp.Data.sendTip.id.Value != null;
            }
            catch (Exception e)
            {
                /*if (e.Response != null)
                {

                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                    
                }*/
            }
            return false;
        }
        public string GetUid(string username)
        {
            try
            {
                string userid = "";


                GraphQLRequest LoginReq = new GraphQLRequest
                {
                    Query = "query DiceBotGetUid($username: String!){ user(name: $username){id}}"
                };
                LoginReq.Variables = new { username = username };
                GraphQLResponse Resp = GQLClient.PostAsync(LoginReq).Result;
                return Resp.Data.user.id.Value;


            }
            catch (Exception ex)
            {

                //ChatBot.DumpLog(ex.ToString());
                return null;
            }
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

    }
}
