
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
using DiceBot.Core;
using static DiceBot.PrimediceSchema;
using GamblingTools.sdk.Connectors.Stake;
using GamblingTools.sdk;
using Newtonsoft.Json;
using RestSharp.Serialization.Json;
using GamblingTools.sdk.gql;

namespace DiceBot.Schema.Stake
{


}

namespace DiceBot
{


    // https://stackoverflow.com/questions/35767845/how-to-handle-recaptcha-on-third-party-site-in-my-client-application

    public class Stake : DiceSite, IDiceSite
    {

        protected ClientSettings settings;

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


        public static string[] sCurrencies => _sCurrencies.Select(x => x.ToUpperInvariant()).OrderBy(x => x).ToArray();

        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();



        // HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        // HttpClientHandler ClientHandlr;
        // GraphQL.Client.GraphQLClient GQLClient;

        StakeApiClient ApiClient;

        Cnx connection;


        bool getid = false;

        public Stake(cDiceBot Parent)
        {

            this.Currencies = sCurrencies;
            this.Currency = "Btc";
            _PasswordText = "API Key: ";
            maxRoll = 100m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://api.stake.bet/bets/";
            register = false;
            this.Parent = Parent;
            Name = "Stake";
            edge = 1;
            Tip = false;
            TipUsingName = true;

            SiteURL = "https://stake.com/";
            URL = "https://api.stake.bet/graphql";
            RolName = "diceRoll";
            Vault = true;
            GameName = "CasinoGameDice";
            StatGameName = "dice";
            EnumName = "CasinoGameDiceConditionEnum";
            HaveMirrors = true;
            MirrorList = new List<string>();
            MirrorList.Add("stake.com");
            MirrorList.Add("stake.bet");
            MirrorList.Add("stake.games");
            MirrorList.Add("staketr.com");
            MirrorList.Add("staketr2.com");
            MirrorList.Add("staketr3.com");
            MirrorList.Add("staketr4.com");
            MirrorList.Add("staketr5.com");
            MirrorList.Add("stake.bz");
            MirrorList.Add("stake.jp");
            MirrorList.Add("stake.ac");
            MirrorList.Add("stake.icu");
            MirrorList.Add("stake.us");

            CurrentMirror = "";


            settings = new ClientSettings()
            {
                Site = SiteURL
            };

        }

        string userid = "";
        int retrycount = 0;
        DateTime Lastbet = DateTime.Now;
        DBRandom R = new DBRandom();

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

                        var req = new RequestData()
                        {
                            operationName = "DiceBotGetBalance",
                            query = "query DiceBotGetBalance{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses betAmount profit currency}}}"
                        };

                        var response = ApiClient.Execute(req);

                        var user = response.Get<pdUser>("user");

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
                    //Thread.Sleep(1000);
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


        //public override void Login(string Username, string Password, string otp)
        //{
        //    try
        //    {
        //        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        //        settings.Update("", Password);
        //        connection = new Cnx(settings.Site, settings.GraphQLEndPoint, settings.ApiKey);
        //        var payload = new RequestData()
        //        {
        //            operationName = "DiceBotLogin",
        //            query = "query DiceBotLogin{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses betAmount profit currency}}}"
        //        };
        //        var result = connection.Query(payload);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public override void Login(string Username, string Password, string otp)
        {
            try
            {

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                settings.Update("", Password);

                ApiClient = null;
                ApiClient = new StakeApiClient(settings);

                var req = new RequestData()
                {
                    operationName = "DiceBotLogin",
                    query = "query DiceBotLogin{user {activeServerSeed { seedHash seed nonce} activeClientSeed{seed} id balances{available{currency amount}} statistic {game bets wins losses betAmount profit currency}}}"
                };

                var response = ApiClient.Execute(req);

                var user = response.Get<pdUser>("user");

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

        public override void UpdateMirror(string url)
        {
            if (url != "" && MirrorList.Contains(url))
            {
                BetURL = $"https://api.{url}/bets/";
                SiteURL = $"https://{url}/";
                URL = $"https://api.{url}/graphql";

                settings.Update(url);

                ApiClient = null;
                ApiClient = new StakeApiClient(settings);

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

                var req = new RequestData()
                {
                    operationName = "DiceRoll",
                    query = @"mutation DiceRoll($amount: Float! 
  $target: Float!
  $condition: CasinoGameDiceConditionEnum!
  $currency: CurrencyEnum!
  $identifier: String!){ diceRoll(amount: $amount, target: $target, condition: $condition, currency: $currency, identifier: $identifier)" +
        " { id nonce currency amount payout state { ... on CasinoGameDice { result target condition } } createdAt serverSeed{seedHash seed nonce} clientSeed{seed} user{balances{available{amount currency}} statistic{game bets wins losses betAmount profit currency}}}}",
                    variables = new
                    {
                        amount = amount,//.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo),
                        target = tmpchance,//.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo),
                        condition = (High ? "above" : "below"),
                        currency = Currency.ToLower(),
                        identifier = "0123456789abcdef"
                    }
                };

                var response = ApiClient.Execute(req);

                if (response.Errors != null)
                {
                    if (response.Errors.Count > 0)
                    {
                        Parent.updateStatus(response.Errors[0].message);
                    }
                }

                if (response.Data != null)
                {

                    RollDice tmp = response.Get<RollDice>("diceRoll");

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
                            tmpbet.Id = GetBetIId(tmpbet.Id);
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

        internal string GetBetIId(string betId)
        {

            using (var connector = new StakeSharedConnector(settings))
            {
                var req = new RequestData()
                {
                    operationName = "DiceBotGetBetId",
                    query = "query DiceBotGetBetId ( $betId: String! ) { bet ( betId: $betId ) {iid} }",
                    variables = new
                    {
                        betId = betId
                    }
                };

                connector.AddRequest(req);

                var response = connector.ExecuteRequest();

                if (response.Data != null)
                {

                    var iid = response.GetData().bet.iid;

                    //tmpbet.Id = betresult2.Data.bet.iid;
                    if (iid.Contains("house:"))
                    {
                        betId = iid.Substring("house:".Length);
                    }

                }

            }

            return betId;

        }

        public override void ResetSeed()
        {
            try
            {
                using (var gqlrequest = new StakeCustomGQLRequest(settings))
                {
                    var req = new RequestData()
                    {
                        operationName = "DiceBotRotateSeed",
                        query = "mutation DiceBotRotateSeed ($seed: String!){rotateServerSeed{ seed seedHash nonce } changeClientSeed(seed: $seed){seed}}",
                        variables = new
                        {
                            seed = R.Next(0, int.MaxValue).ToString()
                        }
                    };

                    gqlrequest.AddRequest(req);

                    var response = gqlrequest.ExecuteRequest();

                    if (response.Data != null)
                    {
                        //this.se
                        //pdSeed user = response.GetDataFieldAs<rotateServerSeed>("rotateServerSeed");
                        //pdSeed = rs.Get<pdSeed>("user");
                    }
                    else if (response.Errors != null && response.Errors.Count > 0)
                    {
                        foreach (var x in response.Errors)
                        {
                            Parent.DumpLog("GRAPHQL ERROR PD RESETSEED: " + x.message, 1);
                        }
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
                using (var gqlrequest = new StakeCustomGQLRequest(settings))
                {

                    var req = new RequestData()
                    {
                        operationName = "createWithdrawal",
                        query = "mutation DiceBotWithdrawal{createWithdrawal(currency:" + Currency.ToLower() + ", address:\"" + Address + "\",amount:" + amount.ToString("0.00000000", System.Globalization.NumberFormatInfo.InvariantInfo) + "){id name address hash amount walletFee createdAt status currency}}"
                    };

                    gqlrequest.AddRequest(req);

                    var response = gqlrequest.ExecuteRequest();

                    return response.Data != null;
                }

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

                using (var gqlrequest = new StakeCustomGQLRequest(settings))
                {

                    var req = new RequestData()
                    {
                        variables = new
                        {
                            currency = Currency.ToLower(),
                            amount = amount
                        },
                        query = "mutation CreateVaultDeposit($currency: CurrencyEnum!, $amount: Float!) {\n  createVaultDeposit(currency: $currency, amount: $amount) {\n    id\n    amount\n    currency\n    user {\n      id\n      balances {\n        available {\n          amount\n          currency\n          __typename\n        }\n        vault {\n          amount\n          currency\n          __typename\n        }\n        __typename\n      }\n      __typename\n    }\n    __typename\n  }\n}\n"
                    };

                    gqlrequest.AddRequest(req);

                    var response = gqlrequest.ExecuteRequest();

                    return response.Data != null;
                }

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
            HMACSHA256 betgenerator = new HMACSHA256();

            int charstouse = 2;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = client + ":" + nonce.ToString() + ":0";
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            decimal total = 0;
            for (int i = 0; i < 8; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                lucky = lucky / (decimal)(Math.Pow(256, (double)((i / 2) + 1)));
                total += lucky;
                /*if (lucky < 1000000)
                {
                    lucky %= 10000;
                    return lucky / 100;

                }*/
            }
            total = Math.Floor(total * 10001) / 100m;
            return total;
            return 0;
        }

        public override void Disconnect()
        {
            ispd = false;
            if (accesstoken != "")
            {
                try
                {
                    //string sEmitResponse = Client.GetStringAsync("logout?api_key=" + accesstoken).Result;

                    ApiClient.Disconnect();
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

                var gqlrequest = new StakeCustomGQLRequest(settings);

                var payload = new RequestData()
                {
                    operationName = "SendTip",
                    query = "mutation SendTip($userId: String!, $amount: Float!, $currency: CurrencyEnum!, $isPublic: Boolean, $chatId: String!, $tfaToken: String) {sendTip(userId: $userId, amount: $amount, currency: $currency, isPublic: $isPublic, chatId: $chatId, tfaToken: $tfaToken) { id amount currency user { id name __typename } sendBy { id name balances { available { amount currency __typename } vault { amount currency __typename } __typename } __typename } __typename } }",
                    variables = new
                    {
                        name = User,
                        amount = amount,
                        isPublic = true,
                        userId = userid,
                        //chatId = chatid,
                        currency = Currency,
                        tfaToken = (string)null
                    }
                };

                gqlrequest.AddRequest(payload);

                var response = gqlrequest.ExecuteRequest();

                return response.Data != null;

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

                using (var gqlrequest = new StakeCustomGQLRequest(settings))
                {
                    var payload = new RequestData()
                    {
                        operationName = "DiceBotGetUid",
                        query = "query DiceBotGetUid($username: String!){ user(name: $username){id}}",
                        variables = new { username = username }
                    };

                    gqlrequest.AddRequest(payload);

                    var response = gqlrequest.ExecuteRequest();

                    var user = response.Get<pdUser>("user");

                    userid = user.id;
                }

                return userid;

            }
            catch (Exception ex)
            {

                //ChatBot.DumpLog(ex.ToString());
                return null;
            }
        }

        public override void SendChatMessage(string Message)
        {
            // Thread send = new Thread(new ParameterizedThreadStart(Send));
            // send.Start(Message);
        }

        void Send(object _Message)
        {

            //if (accesstoken != "")
            //{
            //    try
            //    {
            //        string Message = (string)_Message;
            //        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            //        pairs.Add(new KeyValuePair<string, string>("username", username));
            //        pairs.Add(new KeyValuePair<string, string>("userid", uid.ToString()));
            //        pairs.Add(new KeyValuePair<string, string>("room", "English"));
            //        pairs.Add(new KeyValuePair<string, string>("message", Message));
            //        pairs.Add(new KeyValuePair<string, string>("token", accesstoken));
            //        FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            //        string sEmitResponse = Client.PostAsync("send?api_key=" + accesstoken, Content).Result.Content.ReadAsStringAsync().Result;
            //    }
            //    catch
            //    {
            //    }
            //}

            throw new NotImplementedException();
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

    }
}
