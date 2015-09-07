using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class CoinMillions : DiceSite
    {
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        public bool ispd = true;
        string username = "";

        DateTime lastupdate = new DateTime();
        Random R = new Random();
        HttpClient Client = new HttpClient { BaseAddress = new Uri("https://coinmillions.com/api/1/") };
        
        public CoinMillions(cDiceBot Parent)
        {
            Currency = "btc";
            Currencies = new string[] { "btc","xrp","ltc" };
            this.Parent = Parent;
            maxRoll = 99.9999;
            AutoInvest = false;
            AutoLogin = true;
            AutoWithdraw = false;
            ChangeSeed = true;
            BetURL = "https://coinmillions.com/api/1/bet?game=dice&bet_id=";
            Thread t = new Thread(GetBalanceThread);
            t.Start();
            this.Parent = Parent;
            Name = "CoinMillions";
            Tip = false;
            TipUsingName = true;
            SiteURL = "https://coinmillions.com?a=10156";
            new HttpClient { BaseAddress = new Uri("https://coinmillions.com/api/1/") };
        }

        void dumapipcontent(string message)
        {
            using (StreamWriter sw = System.IO.File.AppendText("cmlog.txt"))
            {
                sw.WriteLine(message);
            }
        }
        DateTime lastChat = DateTime.Now;
        int lastChatId = 0;
        private void GetBalanceThread()
        {
            try
            {
                while (ispd)
                {
                    if (accesstoken != "")
                    {
                        if ((DateTime.Now - lastupdate).TotalSeconds > 60)
                            {
                            lastupdate = DateTime.Now;
                            try
                            {
                                //HttpResponseMessage tmp = Client.GetAsync("user/balance").Result;
                                
                                string s = Client.GetAsync("user/balance").Result.Content.ReadAsStringAsync().Result;
                                
                                /*bbStats tmpu = json.JsonDeserialize<bbStats>(s);
                                balance = tmpu.balance; //i assume
                                bets = tmpu.total_bets;
                                wagered = tmpu.total_wagered;
                                profit = tmpu.total_profit;
                                wins = tmpu.total_wins;
                                losses = bets - losses;
                                Parent.updateBalance((decimal)(balance));
                                Parent.updateBets(bets);
                                Parent.updateLosses(losses);
                                Parent.updateProfit(profit);
                                Parent.updateWagered(wagered);
                                Parent.updateWins(wins);
                                */
                            }
                            catch
                            {

                            }
                            
                        }
                        if ((DateTime.Now - lastChat).TotalSeconds > 1)
                        {
                            try
                            {
                                lastChat = DateTime.Now;
                                if (lastChatId == 0)
                                {
                                    string s = Client.GetAsync("chat/recent").Result.Content.ReadAsStringAsync().Result;
                                    
                                    CmChatItem[] chats = json.JsonDeserialize<CmChatItem[]>(s);
                                    foreach (CmChatItem c in chats)
                                    {
                                        ReceivedChatMessage(string.Format("{0} ({1}) <{2}> {3}", json.ToDateTime2(c.created.ToString()), c.user_id, c.username, c.content));
                                        
                                    }
                                    lastChatId = chats[chats.Length - 1].msg_id;
                                }
                                else
                                {
                                    string s = Client.GetAsync("chat/after?msg_id=" + lastChatId).Result.Content.ReadAsStringAsync().Result;
                                    
                                    CmChatItem[] chats = json.JsonDeserialize<CmChatItem[]>(s);
                                    foreach (CmChatItem c in chats)
                                    {
                                        ReceivedChatMessage(string.Format("{0} ({1}) <{2}> {3}", json.ToDateTime2(c.created.ToString()), c.user_id, c.username, c.content));

                                    }
                                    lastChatId = chats[chats.Length - 1].msg_id;
                                }
                            }
                            catch
                            {

                            }
                        }

                    }
                    Thread.Sleep(1000);
                }
            }
            catch
            {

            }
        }

        public override void Disconnect()
        {
            ispd = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            accesstoken = Password;
            Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Password);
            //Client.DefaultRequestHeaders.Add("X-Affiliate-ID", "10156");
            lastupdate = DateTime.Now.AddMinutes(-1);
            try
            {
                //HttpResponseMessage tmp = Client.GetAsync("user/balance").Result;

                string s = Client.GetAsync("user/balance").Result.Content.ReadAsStringAsync().Result;
                
                finishedlogin(true);
            }
            catch
            {

            }
                finishedlogin(false);
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
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("user/seeds/change", Content))
            {
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        ResetSeed();
                        return;
                    }
                }
            }
            
        }

        public override void SendChatMessage(string Message)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("content", Message));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("chat/message/publish", Content))
            {
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        SendChatMessage(Message);
                        return;
                    }
                }
            }
            
        }

        

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        void PlaceBetThread(object _High)
        {
            bool High = (bool)_High;
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("crypto", Currency));
                pairs.Add(new KeyValuePair<string, string>("amount", amount.ToString("0.00000000")));
                pairs.Add(new KeyValuePair<string, string>("chance_to_win", chance.ToString("0.0000")));
                pairs.Add(new KeyValuePair<string, string>("roll_type", High ? "h" : "l"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("game/dice/bet", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            PlaceBetThread(_High);
                            return;
                        }
                    }
                }
                
                CmBetResult tmp = json.JsonDeserialize<CmBetResult>(responseData);

                Bet tmp2 = tmp.Tobet((decimal)chance);
                //next = tmp.nextServerSeed;
                lastupdate = DateTime.Now;
                balance = double.Parse(tmp.new_balance.btc.available, System.Globalization.NumberFormatInfo.InvariantInfo);
                bets++;
                /*if (tmp2)
                    wins++;
                else losses++;*/
                
                wagered += (double)(tmp2.Amount);
                profit += (double)tmp2.Profit;


                
                //tmp2.serverhash = next;
                //next = tmp.nextServerSeed;

                FinishedBet(tmp2);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string sEmitResponse = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    Parent.updateStatus(sEmitResponse);
                }
                if (e.Message.Contains("429") || e.Message.Contains("502"))
                {
                    Thread.Sleep(200);
                    PlaceBetThread(_High);
                }


            }
            catch (Exception e)
            {

            }
        }

        protected override void internalPlaceBet(bool High)
        {
            Thread t = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            t.Start(High);
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            throw new NotImplementedException();
        }
    }

    public class CmChatItem
    {
        public string username { get; set; }
        public string content { get; set; }
        public int msg_id { get; set; }
        public int user_id { get; set; }
        public long created { get; set; }
    }

    public class CmBetResult
    {
        public cmNewBalanceBase new_balance { get; set; }
        public string target_number { get; set; }
        public string amount { get; set; }
        public string roll_type { get; set; }
        public string multiplier { get; set; }
        public string profit { get; set; }
        public string lucky_number { get; set; }
        public long id { get; set; }
        public string crypto { get; set; }

        public Bet Tobet(decimal chance)
        {
            Bet tmp = new Bet {
                Amount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                date = DateTime.Now,
                Id = id,
                Profit = decimal.Parse(profit, System.Globalization.NumberFormatInfo.InvariantInfo),
                high = roll_type == "h",
                Chance = chance,
                Roll = decimal.Parse(lucky_number, System.Globalization.NumberFormatInfo.InvariantInfo)

            };
            return tmp;
        }
    }

    public class cmNewBalanceBase
    {
        public CmNewBalance btc { get; set; }
        public CmNewBalance xrp { get; set; }
        public CmNewBalance ltc { get; set; }
    }

    public class CmNewBalance
    {
        public string available { get; set; }
        public string site_bankroll_percent { get; set; }
        public int num_confirmed_deposits { get; set; }
        public string profit_fee_base { get; set; }

        public int num_unconfirmed_deposits { get; set; }
    }
}
