using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class Stake: PD
    {
       
        public static new string[] sCurrencies = new string[] { "Btc", "Eth", "Ltc","Doge", "Bch","Xrp" };

        public Stake(cDiceBot Parent):base(Parent)/*:base(Parent)*/
        {
            this.Currencies = sCurrencies;
            this.Currency = "Btc";
            _PasswordText = "API Key: ";
            maxRoll = 100m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://api.stake.com/bets/";
            register = false;
            this.Parent = Parent;
            Name = "Stake";
            edge = 1;
            Tip = true;
            TipUsingName = true;
            SiteURL = "https://stake.com/?code=seuntjie";
            URL = "https://api.stake.com/graphql/";
            RolName = "diceRoll";
            
            GameName = "CasinoGameDice";
            StatGameName = "dice";
        }
    }
    /*class Stake : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        CookieContainer cookies = new CookieContainer();
        public static string[] sCurrencies = new string[] { "Btc", "Eth" };
        public Stake(cDiceBot Parent)/*:base(Parent)* /
        {
            this.Currencies = sCurrencies;
            this.Currency = "Btc";
            _PasswordText = "Password: ";
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://api.stake.com/bets/";
            register = false;
            this.Parent = Parent;
            Name = "Stake";
            edge = 2;
            Tip = true;
            TipUsingName = true;
            //SiteURL = "https://stake.com/?code=seuntjie";
            //URL = "https://api.stake.com/graphql/";
            //RolName = "diceRoll";
        }
        public override void Disconnect()
        {
            ispd = false;

        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

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
                    if (CrrentLogin != null && ((DateTime.Now - lastupdate).TotalSeconds > 60 || ForceUpdateStats))
                    {
                        lastupdate = DateTime.Now;
                        ForceUpdateStats = false;
                        try
                        {
                            string result = Client.GetStringAsync("auth/me?currency="+Currency.ToLower()+"&hash=" + hash).Result;
                            StakeLogin newlogin = json.JsonDeserialize<StakeLogin>(result);
                            if (newlogin != null)
                            {
                                if (newlogin.user != null)
                                {
                                    CrrentLogin.user = newlogin.user;
                                    CrrentLogin.user = newlogin.user;
                                    balance = (decimal)((Currency.ToLower()=="btc"? CrrentLogin.user.balances.btc.available_balance: Currency.ToLower() == "eth"? CrrentLogin.user.balances.eth.available_balance: CrrentLogin.user.balances.btc.available_balance)) / 100000000.0m;
                                    bets = (int)CrrentLogin.user.stats.dice.bets;
                                    wins = (int)CrrentLogin.user.stats.dice.wins;
                                    losses = (int)CrrentLogin.user.stats.dice.losses;
                                    profit = (decimal)CrrentLogin.user.stats.dice.profit / 100000000.0m;
                                    wagered = (decimal)CrrentLogin.user.stats.dice.wagered / 100000000.0m;
                                    Parent.updateBalance(balance);
                                    Parent.updateBets(bets);
                                    Parent.updateWins(wins);
                                    Parent.updateLosses(losses);
                                    Parent.updateProfit(profit);
                                    Parent.updateWagered(wagered);
                                }
                            }
                        }
                        catch (Exception er)
                        {
                            Parent.DumpLog(er.ToString(),0);
                        }
                        
                    }
                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
            
        }

        string hash = "";
        StakeLogin CrrentLogin = null;
        public override void Login(string username, string password, string ga)
        {
            try
            {
                ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://api.stake.com/api/") };
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                cookies = new CookieContainer();
                ClientHandlr.CookieContainer = cookies;
                hash = "";
                var content = new StringContent(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"{{\"username\":\"{0}\",\"password\":\"{1}\",\"tfa\":\"{2}\",\"captcha\":\"\"}}", username, password, ga), Encoding.UTF8, "application/json");

                HttpResponseMessage response = Client.PostAsync("auth/login?hash=" + hash, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string ResponseMsg = response.Content.ReadAsStringAsync().Result;
                    StakeLogin login = json.JsonDeserialize<StakeLogin>(ResponseMsg);
                    this.CrrentLogin = login;
                    if (login.token != null && login.user != null)
                    {
                        Client.DefaultRequestHeaders.Add("x-access-token", login.token);
                        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.token);



                        balance = (decimal)((Currency.ToLower() == "btc" ? CrrentLogin.user.balances.btc.available_balance : Currency.ToLower() == "eth" ? CrrentLogin.user.balances.eth.available_balance : CrrentLogin.user.balances.btc.available_balance) ) / 100000000.0m;
                        bets = (int)login.user.stats.dice.bets;
                        wins = (int)login.user.stats.dice.wins;
                        losses = (int)login.user.stats.dice.losses;
                        profit = (decimal)login.user.stats.dice.profit / 100000000.0m;
                        wagered = (decimal)login.user.stats.dice.wagered / 100000000.0m;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);

                        finishedlogin(true);
                        lastupdate = DateTime.Now;
                        ispd = true;
                        Thread t = new Thread(GetBalanceThread);
                        t.Start();
                        return;

                    }
                }
                else
                {
                    Parent.updateStatus(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception E)
            {
                Parent.DumpLog(E.ToString(),0);
            }
            finishedlogin(false);
            return;
        }
        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.High = High;
            new Thread(new ParameterizedThreadStart(placebetthread)).Start(new PlaceBetObj(High, amount, chance, Guid));
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
                try
                {
                    string jsons = string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"amount\":{0:0},\"target\":\"{1:00.00}\",\"guess\":\"{2}\",\"currency\":\"{3}\"}}", amount * 100000000.0m, High ? 99.99m - (chance) : chance, High ? ">" : "<", Currency.ToLower());
                    var content = new StringContent(jsons, Encoding.UTF8, "application/json");

                    HttpResponseMessage Response = Client.PostAsync("games/dice/bet?hash=" + hash, content).Result;
                    if (Response.IsSuccessStatusCode)
                    {
                        string Result = Response.Content.ReadAsStringAsync().Result;
                        StakeDiceBetResult tmpresult = json.JsonDeserialize<StakeDiceBetResult>(Result);
                        if (tmpresult != null)
                        {
                            if (tmpresult.bet != null && tmpresult.user != null)
                            {
                                Bet Tmp = new Bet
                                {
                                    Amount = tmpresult.bet.amount / 100000000m,
                                    Chance = chance,
                                    clientseed = tmpresult.bet.clientSeedId,
                                    Currency = "btc",
                                    date = DateTime.Now,
                                    high = High,
                                    Id = tmpresult.bet.iid.ToString(),
                                    Profit = tmpresult.bet.state.profit / 100000000m,
                                    Roll = (decimal)tmpresult.bet.state.result,
                                    Guid=tmp5.Guid
                                    
                                };
                                foreach (StakeBetBalance x in tmpresult.balances)
                                {
                                    if (x.currency.ToLower()==Currency.ToLower())
                                    {
                                        balance = x.balance / 100000000.0m;
                                    }
                                }
                                //balance = (decimal)(tmpresult.userBalance / 100000000.0m);
                                bets++;
                                if ((Tmp.high && Tmp.Roll > maxRoll - Tmp.Chance) || (!Tmp.high && Tmp.Roll < Tmp.Chance))
                                {
                                    wins++;
                                }
                                else
                                    losses++;
                                profit += Tmp.Profit;
                                wagered += Tmp.Amount;
                                FinishedBet(Tmp);
                            }
                        }
                    }
                    else
                    {
                        string x = Response.Content.ReadAsStringAsync().Result;
                        if (x.ToLower().Contains("valid amount"))
                            Parent.updateStatus("Invalid Bet amount");
                        else if (x.ToLower().Contains("funds"))
                            Parent.updateStatus("Insufficient Funds, probably.");
                        Parent.updateStatus(x);
                    }
                }
                catch (Exception er)
                {
                    Parent.DumpLog(er.ToString(),0);
                }
            }
            catch (Exception er)
            {
                Parent.DumpLog(er.ToString(), 0);
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            try
            {
                var content = new StringContent(string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{{\"address\":\"{0}\",\"amount\":{1:0}}}",Address, Amount * 100000000.0m), Encoding.UTF8, "application/json");

                HttpResponseMessage Response = Client.PostAsync("users/wallet/withdrawals?hash=" + hash, content).Result;
                if (Response.IsSuccessStatusCode)
                {
                    StakeWithdrawalRoot withdrawal = json.JsonDeserialize<StakeWithdrawalRoot>(Response.Content.ReadAsStringAsync().Result);
                    if (withdrawal.withdrawal!=null && withdrawal.user!=null)
                    {
                        if (withdrawal.withdrawal.txid!=null && withdrawal.withdrawal.txid!="")
                        {
                            return true;

                        }
                    }
                }
            }
            catch (Exception ER)
            {
                Parent.DumpLog(ER.ToString(), 0);
            }
            return false;
        }

        public override bool InternalSendTip(string User, decimal amount)
        {
            try
            {
                //https://api.stake.com/api/users/username/
                string msg = Client.GetStringAsync("users/username/" + User).Result;
                StakeUser newuser = json.JsonDeserialize<StakeUser>(msg);

                if (newuser!=null)
                {
                    var content = new StringContent(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"{{\"amount\":{0:0},\"is_public\":true}}", amount * 100000000.0m), Encoding.UTF8, "application/json");

                    HttpResponseMessage Response = Client.PostAsync("users/" + newuser.id + "/tip?hash=" + hash, content).Result;
                    string result = Response.Content.ReadAsStringAsync().Result;
                   StakeTipResult newlogin = json.JsonDeserialize<StakeTipResult>(result);
                    if (newlogin != null)
                    {
                        if (newlogin.user != null && newlogin.receiverId != null)
                        {
                            CrrentLogin.user = newlogin.user;
                            return true;
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Parent.DumpLog(er.ToString(), 0);
            }
            return false;
        }
    }























    public class StakeBaccarat
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeDiamondPoker
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeMines
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeRoulette
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakePlinko
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeChartbet
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeBlackjack
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeHilo
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeKeno
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeDice
    {
        public string game { get; set; }
        public string user_id { get; set; }
        public decimal wins { get; set; }
        public decimal losses { get; set; }
        public decimal ties { get; set; }
        public decimal bets { get; set; }
        public decimal wagered { get; set; }
        public double profit { get; set; }
        public decimal max_win { get; set; }
    }

    public class StakeStats
    {
        public StakeBaccarat baccarat { get; set; }
        public StakeDiamondPoker diamond_poker { get; set; }
        public StakeMines mines { get; set; }
        public StakeRoulette roulette { get; set; }
        public StakePlinko plinko { get; set; }
        public StakeChartbet chartbet { get; set; }
        public StakeBlackjack blackjack { get; set; }
        public StakeHilo hilo { get; set; }
        public StakeKeno keno { get; set; }
        public StakeDice dice { get; set; }
    }

    public class StakeBaccarat2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeDiamondPoker2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeMines2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeRoulette2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakePlinko2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeChartbet2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeBlackjack2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeHilo2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeKeno2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeDice2
    {
        public decimal bet_podecimals { get; set; }
        public decimal wager_podecimals { get; set; }
        public decimal win_podecimals { get; set; }
    }

    public class StakeGames
    {
        public StakeBaccarat2 baccarat { get; set; }
        public StakeDiamondPoker2 diamond_poker { get; set; }
        public StakeMines2 mines { get; set; }
        public StakeRoulette2 roulette { get; set; }
        public StakePlinko2 plinko { get; set; }
        public StakeChartbet2 chartbet { get; set; }
        public StakeBlackjack2 blackjack { get; set; }
        public StakeHilo2 hilo { get; set; }
        public StakeKeno2 keno { get; set; }
        public StakeDice2 dice { get; set; }
    }

    public class StakeProfile
    {
        public string avatar { get; set; }
        public decimal level { get; set; }
        public double exp_podecimals { get; set; }
        public decimal chat_messages { get; set; }
        public decimal chat_podecimals { get; set; }
        public double all_wager_podecimals { get; set; }
        public decimal profile_podecimals { get; set; }
        public decimal profile_name_podecimals { get; set; }
        public decimal profile_address_podecimals { get; set; }
        public StakeGames games { get; set; }
        public string country_code { get; set; }
        public string home_phone { get; set; }
        public string skype { get; set; }
        public string telegram { get; set; }
        public string email { get; set; }
        public bool email_confirmed { get; set; }
        public bool email_disabled { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string user_id { get; set; }
        public string name { get; set; }
        public bool stats_show_profits { get; set; }
        public bool chat_receive_nonfriends { get; set; }
        public bool notification_achievements { get; set; }
        public bool notification_friends_requests { get; set; }
        public bool notification_private_messages { get; set; }
        public bool notification_highrollers { get; set; }
        public bool notification_announcements { get; set; }
    }
    public class StakeBalanceDetails
    {
        public double available_balance { get; set; }
        public decimal in_play_balance { get; set; }
        public decimal non_available_balance { get; set; }
        public decimal deposits_balance { get; set; }
        public decimal withdrawals_balance { get; set; }
        public decimal withdrawals_fees_balance { get; set; }
        public decimal losses_balance { get; set; }
        public double wins_balance { get; set; }
        public decimal faucet_balance { get; set; }
        public decimal bonus_balance { get; set; }
        public decimal given_tips_balance { get; set; }
        public decimal received_tips_balance { get; set; }
        public decimal affiliate_balance { get; set; }
    }
    public class StakeBalances
    {
        public StakeBalanceDetails btc { get; set; }
        public StakeBalanceDetails eth { get; set; }
    }

    public class StakeUser
    {
        public StakeStats stats { get; set; }
        public string id { get; set; }
        public string username { get; set; }
        public string created_at { get; set; }
        public StakeProfile profile { get; set; }
        public bool otp_enabled { get; set; }
        public StakeBalances balances { get; set; }
        public List<object> roles { get; set; }
        public bool has_password { get; set; }
    }

    public class StakeLogin
    {
        public StakeUser user { get; set; }
        public string token { get; set; }
    }

    public class StakeRoomPublic
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }

    }

    public class StakeRooms
    {

        public List<StakeRoomJoined> joined { get; set; }
        public List<StakeRoomPublic> @public { get; set; }
    }

    public class StakeRoomRoom
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }

    }

    public class StakeRoomJoined
    {
        public string user_id { get; set; }
        public StakeRoomRoom room { get; set; }
        public string created_at { get; set; }
    }

    public class StakeTags
    {
        public int VIP { get; set; }
        public List<object> roles { get; set; }
        public int privilege { get; set; }
    }

    public class StakeData2
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string room_id { get; set; }
        public string from_id { get; set; }
        public string to_id { get; set; }
        public string message { get; set; }
        public StakeTags tags { get; set; }
        public string created_at { get; set; }
        public string @event { get; set; }
        public bool read { get; set; }
    }

    public class StakeData
    {
        public string channel { get; set; }
        public StakeData2 data { get; set; }
    }

    public class StakeMessage
    {
        public string @event { get; set; }
        public StakeData data { get; set; }
    }

    public class StakeIntUser
    {
        public string StakeID { get; set; }
        public DateTime LastMessageTime { get; set; }
        public string UserName { get; set; }
        public long IntID { get; set; }
    }

    public class StakeTipResult
    {
        public long amount { get; set; }
        public string receiverId { get; set; }
        public StakeUser user { get; set; }
    }

    public class StakeDiceInfo
    {
        public double target { get; set; }
        public string guess { get; set; }
        public double result { get; set; }
        public double roll { get; set; }
        public decimal amount { get; set; }
        public decimal profit { get; set; }
        public bool win { get; set; }


    }

    public class StakeDiceBet
    {
        public string game { get; set; }
        public string id { get; set; }
        public string user_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public decimal amount { get; set; }
        public decimal profit { get; set; }
        public bool win { get; set; }
        public string player { get; set; }
        public double payout { get; set; }
        public long iid { get; set; }
        public string clientSeedId { get; set; }
        public string serverSeedId { get; set; }
        public StakeDiceInfo state { get; set; }
    }

    public class StakeDiceBetResult
    {
        public StakeDiceBet bet { get; set; }
        public StakeUser user { get; set; }
        public StakeBetBalance[] balances { get; set; }
    }
    public class StakeBetBalance
    {
        public string currency { get; set; }
        public decimal balance { get; set; }
    }
    public class StakeWithdrawal
    {
        public string id { get; set; }
        public int amount { get; set; }
        public int user_withdrawal_fee { get; set; }
        public string user_id { get; set; }
        public object admin_user_id { get; set; }
        public object fee { get; set; }
        public string txid { get; set; }
        public string address { get; set; }
        public int resubmits { get; set; }
        public int confirmations { get; set; }
        public object wallet_error_code { get; set; }
        public string ip_address { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public bool is_owner { get; set; }
        public string status { get; set; }
        public bool withdrawals_active { get; set; }
        public bool reversed { get; set; }
    }


    public class StakeWithdrawalRoot
    {
        public StakeWithdrawal withdrawal { get; set; }
        public StakeUser user { get; set; }
    }*/


}
