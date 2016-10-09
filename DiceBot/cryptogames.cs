using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class cryptogames:DiceSite
    {
        string accesstoken = "";
        
        public bool iscg = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;
        
        public static string[] sCurrencies = new string[] { "BTC", "Doge", "ETH", "DASH", "GRC", "GAME", "PPC", "PLAY", "LTC" };
        public cryptogames(cDiceBot Parent)
        {
            NonceBased = false;
            this.Parent = Parent;
            AutoInvest = false;
            AutoLogin = true;
            AutoWithdraw = false;
            ChangeSeed = false;
            edge = 0.8m;
            maxRoll = 99.999m;
            this.Currencies = new string[] { "BTC", "Doge", "ETH", "DASH", "GRC", "GAME", "PPC", "PLAY", "LTC" };
            this.Currency = "btc";
            register = false;
            SiteURL = "https://www.crypto-games.net?i=KaSwpL1Bky";
            BetURL = "https://www.crypto-games.net/fair.aspx?coin=BTC&type=3&id=";
            Tip = false;
            Name = "CryptoGames";
            
        }

        protected override void CurrencyChanged()
        {
            lastupdate = DateTime.Now.AddSeconds(-61);

        }

        void GetBalanceThread()
        {
            try
            {
                while (iscg)
                {
                    if (accesstoken != "" && ((DateTime.Now - lastupdate).TotalSeconds > 60||ForceUpdateStats))
                    {
                        try
                        {
                            lastupdate = DateTime.Now;
                            string sEmitResponse = Client.GetStringAsync("user/" + Currency + "/" + accesstoken).Result;
                            cgUser tmpBal = json.JsonDeserialize<cgUser>(sEmitResponse);
                            this.balance = tmpBal.Balance;
                            this.wagered = tmpBal.Wagered;
                            this.profit = tmpBal.Profit;
                            this.bets = tmpBal.TotalBets;
                            Parent.updateBalance((decimal)(balance));
                            Parent.updateBets(bets);
                            Parent.updateLosses(losses);
                            Parent.updateProfit(profit );
                            Parent.updateWagered(wagered );
                            Parent.updateWins(wins);
                        }
                        catch { }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch { }
        }
        Random ClientSeedGen = new Random();
        string CurrenyHash = "";
        void PlaceBetThread(object _High)
        {
            PlaceBetObj tmp9 = _High as PlaceBetObj;
            bool High = tmp9.High;
            decimal amount = tmp9.Amount;
            decimal chance = tmp9.Chance;
            string Clients = ClientSeedGen.Next(0, int.MaxValue).ToString();
            decimal payout = decimal.Parse(((100m - edge) / (decimal)chance).ToString("0.0000"));
            cgPlaceBet tmpPlaceBet = new cgPlaceBet() { Bet=amount, ClientSeed=Clients, UnderOver=High, Payout=(decimal)payout };

            string post = json.JsonSerializer<cgPlaceBet>(tmpPlaceBet);
            HttpContent cont = new StringContent(post);
            cont.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            
            /*List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("value", post));
            //pairs.Add(new KeyValuePair<string, string>("affiliate", "seuntjie"));
            FormUrlEncodedContent cont = new FormUrlEncodedContent(pairs);*/
               
            try
            {
                
                string sEmitResponse = Client.PostAsync("placebet/" + Currency + "/" + accesstoken, cont).Result.Content.ReadAsStringAsync().Result;
                cgGetBet Response = json.JsonDeserialize<cgGetBet>(sEmitResponse);
                if (Response.Message!="" && Response.Message!=null)
                {
                    Parent.updateStatus(Response.Message);
                    return;
                }
                Bet bet = new Bet()
                    {
                        Amount = (decimal)amount,
                        Profit = (decimal)Response.Profit,
                        Roll = (decimal)Response.Roll,
                        Chance = decimal.Parse(Response.Target.Substring(3), System.Globalization.NumberFormatInfo.InvariantInfo),
                          date =DateTime.Now,
                          clientseed=Clients,
                           Currency=Currency,
                            Id=Response.BetId,
                             high= Response.Target.Contains(">"),
                              serverhash = CurrenyHash,
                               nonce=-1,
                               serverseed=Response.ServerSeed
                    };
                if (bet.high)
                    bet.Chance = (decimal)maxRoll - bet.Chance;
                this.CurrenyHash = Response.NextServerSeedHash;
                bool Win = (((bool)bet.high ? (decimal)bet.Roll > (decimal)maxRoll - (decimal)(bet.Chance) : (decimal)bet.Roll < (decimal)(bet.Chance)));
                if (Win)
                    wins++;
                else
                    losses++;
                bets++;
                wagered+=amount;
                balance += Response.Profit;
                profit+=Response.Profit;
                FinishedBet(bet);
                
            }
            catch
            { }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            new Thread(new ParameterizedThreadStart(PlaceBetThread)).Start(new PlaceBetObj(High, amount, chance));
        }

        public override void ResetSeed()
        {
            
        }

        public override void SetClientSeed(string Seed)
        {
            
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            return false;
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://api.crypto-games.net/v1/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {
                accesstoken = Password;

                string sEmitResponse = Client.GetStringAsync("user/" + Currency + "/" + accesstoken).Result;
                cgUser tmpBal = json.JsonDeserialize<cgUser>(sEmitResponse);
                sEmitResponse = Client.GetStringAsync("nextseed/" + Currency + "/" + accesstoken).Result;
                cgNextSeed tmpSeed = json.JsonDeserialize<cgNextSeed>(sEmitResponse);
                CurrenyHash = tmpSeed.NextServerSeedHash;
                this.balance = tmpBal.Balance;
                this.wagered = tmpBal.Wagered;
                this.profit = tmpBal.Profit;
                this.bets = tmpBal.TotalBets;
                //Get stats
                //assign vals to stats
                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
                Thread t = new Thread(GetBalanceThread);
                iscg = true;
                t.Start();
                
                finishedlogin(true);
            }
            catch (AggregateException e)
            { finishedlogin(false); }
            catch (Exception e)
            { finishedlogin(false); }
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            iscg = false;
        }

        public override void GetSeed(long BetID)
        {
            
        }

        public override void SendChatMessage(string Message)
        {
            
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            SHA512 betgenerator = SHA512.Create();

            int charstouse = 5;


            List<byte> buffer = new List<byte>();
            string msg = server + client;
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
                if (lucky < 1000000m)
                {
                    string tmps = lucky.ToString("000000").Substring(lucky.ToString("000000").Length - 5);
                    return decimal.Parse(tmps) / 1000.0m;
                }
            }
            return 0;

        }
        
        public static decimal sGetLucky(string server, string client, int nonce)
        {
            SHA512 betgenerator = SHA512.Create();

            int charstouse = 5;


            List<byte> buffer = new List<byte>();
            string msg = server + client;
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
                    //return lucky / 10000;
                    string tmps = lucky.ToString("000000").Substring(lucky.ToString("000000").Length - 5);
                    return decimal.Parse(tmps) / 1000.0m;
                }
            }
            return 0;
            return 0;
        }

    }

    public class cgBalance
    {
        public decimal Balance { get; set; }
    }
    public class cgPlaceBet
    {
        public decimal Bet { get; set; }
        public decimal Payout { get; set; }
        public bool UnderOver { get; set; }
        public string ClientSeed { get; set; }
    }
    public class cgGetBet
    {
        public long BetId { get; set; }
        public decimal Roll { get; set; }
        public string ClientSeed { get; set; }
        public string Target { get; set; }
        public decimal Profit { get; set; }
        public string NextServerSeedHash { get; set; }
        public string ServerSeed { get; set; }
        public string Message { get; set; }
    }
    public class cgUser
    {
        public string Nickname { get; set; }
        public decimal Balance { get; set; }
        public string Coin { get; set; }
        public int TotalBets { get; set; }
        public decimal Profit { get; set; }
        public decimal Wagered { get; set; }
    }
    public class cgNextSeed
    {
        public string NextServerSeedHash { get; set; }
    }
}
