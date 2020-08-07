using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DiceBot.Common;
using DiceBot.Forms;
using NBitcoin;

namespace DiceBot.Sites
{
    internal class YoloDice : DiceSite
    {
        public static string[] cCurrencies = {"Btc", "Ltc", "Eth", "Doge"};
        private readonly string basestring = "{{\"id\":{0},\"method\":\"{1}\"{2}}}\r\n";
        private readonly Random R = new Random();
        private readonly Dictionary<long, string> Requests = new Dictionary<long, string>();

        /*HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;*/
        private TcpClient apiclient = new TcpClient();
        private string challenge = "";
        private YLSeed Currentseed;
        private decimal delay;
        private string Guid = "";
        private long id;
        private bool inauth;

        /* string accesstoken = "";
        DateTime LastSeedReset = new DateTime();*/
        public bool ispd;
        private DateTime lastbet = DateTime.Now;
        private DateTime lastupdate;
        private string privkey = "";

        private byte[] ReadBuffer = new byte[512];
        private SslStream sslStream;
        private long uid;
        private string username = "";

        public YoloDice(cDiceBot Parent)
        {
            Currencies = cCurrencies;
            maxRoll = 99.9999m;
            _PasswordText = "API Key: ";
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://yolodice.com/#";

            this.Parent = Parent;
            Name = "YoloDice";
            Tip = false;
            TipUsingName = true;

            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://yolodice.com/#r-fexD-GR";
            _PasswordText = "Private Key";
        }

        protected override void CurrencyChanged()
        {
            ForceUpdateStats = true;
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.Guid = Guid;

            var bet = string.Format(NumberFormatInfo.InvariantInfo, "{{\"attrs\":{0}}}",
                                    JsonUtils.JsonSerializer(
                                                             new YLBetSend
                                                             {
                                                                 amount = (long) (amount * 100000000), range = High ? "hi" : "lo",
                                                                 target = (int) (chance * 10000), coin = Currency.ToLower()
                                                             }));

            Write("create_bet", bet);
        }

        public override void ResetSeed()
        {
            Write("create_seed", "{\"attrs\": {\"client_seed \":\"" + R.Next(0, int.MaxValue) + "\"}}");
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override void Donate(decimal Amount)
        {
            switch (Currency.ToLower())
            {
                case "btc":
                    internalWithdraw(Amount, "1SeUNtjiMm5sdEgFrePWgF3J6kLCexJgj");

                    break;
                case "ltc":
                    internalWithdraw(Amount, "LiGugsrKkhjRFqUcqFm1rLsdWjmyHx1W3G");

                    break;
            }
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            Write("create_withdrawal",
                  "{\"attrs\": " + JsonUtils.JsonSerializer(new YLWithdrawal
                  {
                      coin = Currency.ToLower(), allow_pending = true, amount = (long) (Amount * 100000000), to_address = Address
                  }) + "}");

            return true;
        }

        //long uid = 0;
        public override void Login(string Username, string Password, string twofa)
        {
            try
            {
                apiclient = new TcpClient();

                apiclient.Connect("api.yolodice.com", 4444);

                if (apiclient.Connected)
                {
                    sslStream = new SslStream(apiclient.GetStream());
                    sslStream.AuthenticateAsClient("api.yolodice.com"); //, null, System.Security.Authentication.SslProtocols.Ssl2| System.Security.Authentication.SslProtocols.Ssl3| System.Security.Authentication.SslProtocols.Tls11|System.Security.Authentication.SslProtocols.Tls12, false);

                    var frstchallenge = string.Format(basestring, id++, "generate_auth_challenge", "");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    var bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLChallenge tmp = null;

                    try
                    {
                        tmp = JsonUtils.JsonDeserialize<YLChallenge>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        finishedlogin(false);

                        return;
                    }

                    var address = "";
                    var message = "";

                    try
                    {
                        var tmpkey = Key.Parse(Password);
                        address = tmpkey.ScriptPubKey.GetDestinationAddress(Network.GetNetwork("Main")).ToString();
                        message = tmpkey.SignMessage(tmp.result);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("API key format error. Are you using your Private key?");
                        finishedlogin(false);

                        return;
                    }

                    frstchallenge = string.Format(basestring, id++, "auth_by_address",
                                                  ",\"params\":" + JsonUtils.JsonSerializer(new YLAuthSend {address = address, signature = message}));

                    //{"id":1,"method":"auth_by_address","params":{"address":"n3kmufwdR3Zzgk3k6NYeeLBxB9SpHKe5Tc","signature":"H4W6yMaVK6EzrTw/9jqmLh1lvoyFnxCFqRon2g25lJ7FTCAUHGJWWF3UJD5wCzCVafdjIfCmIYH2KyHboodjjcU="}}

                    //{"id":1,"method":"auth_by_address","params":{"address":"1PUgaiHavJrpi7r7JhkhwWj7Kf9Ls68Z6w","signature":"Hz0oh29Nho+bVz7zggS1dqx\/N7VAyD6jsk8k98qW84ild7D71Q9rUbmEE4GIj0a5eKPcK1EjvSEwwa74jBJRyY8="}}

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLOgin tmologin = null;

                    try
                    {
                        tmologin = JsonUtils.JsonDeserialize<YLOgin>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        finishedlogin(false);

                        return;
                    }

                    uid = tmologin.result.id;
                    username = tmologin.result.name;

                    frstchallenge = string.Format(basestring, id++, "read_user_coin_data",
                                                  ",\"params\":{\"selector\":{\"id\":\"" + uid + "_" + Currency.ToLower() + "\"}}");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLUserStats tmpstats = null;

                    try
                    {
                        tmpstats = JsonUtils.JsonDeserialize<YLUserStats>(challenge).result;
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        finishedlogin(false);

                        return;
                    }

                    if (tmpstats != null)
                    {
                        balance = tmpstats.balance / 100000000m;
                        bets = (int) tmpstats.bets;
                        wins = (int) tmpstats.wins;
                        losses = (int) tmpstats.losses;
                        profit = tmpstats.profit / 100000000m;
                        wagered = tmpstats.wagered / 100000000m;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(BalanceThread).Start();
                        privkey = Password;
                        new Thread(Beginreadthread).Start();
                        Thread.Sleep(50);
                        finishedlogin(true);

                        return;
                    }

                    //tmp2.ImportPrivKey(Password, "", false);
                    //string message = tmp2.SignMessage(username, tmp.result);
                    //string message = //FullSignatureTest(tmp.result, new AsymmetricCipherKeyPair("", ""));

                    /*ispd = true;
                    new Thread(new ThreadStart(BalanceThread)).Start();*/
                }
            }
            catch (Exception e)
            {
                Parent.DumpLog(e.ToString(), 0);
            }

            finishedlogin(false);
        }

        private void BalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 15 || ForceUpdateStats)
                {
                    ForceUpdateStats = false;
                    lastupdate = DateTime.Now;
                    Write("read_user_coin_data", "{\"selector\":{\"id\":\"" + uid + "_" + Currency.ToLower() + "\"}}");
                }

                Thread.Sleep(1000);
            }
        }

        private void ReadTCP(IAsyncResult result)
        {
            try
            {
                try
                {
                    var response = "";

                    response = Encoding.ASCII.GetString(ReadBuffer, 0, 512);

                    response = response.Replace("\0", "");
                    Parent.DumpLog(response, 10);

                    if (response != "")
                        try
                        {
                            response = response.Substring(0, response.IndexOf("\n"));

                            var tmprespo = JsonUtils.JsonDeserialize<YLBasicResponse>(response);

                            if (Requests.ContainsKey(tmprespo.id))
                            {
                                switch (Requests[tmprespo.id])
                                {
                                    case "read_user_coin_data":
                                        ReadUser(response);

                                        break;
                                    case "create_bet":
                                        ProcessBet(response);

                                        break;
                                    case "read_current_seed":
                                        read_current_seed(response);

                                        break;
                                    case "read_seed":
                                        read_current_seed(response);

                                        break;
                                    case "create_seed":
                                        create_seed(response);

                                        break;
                                    case "patch_seed":
                                        patch_seed(response);

                                        break;
                                    case "create_withdrawal":
                                        create_withdrawal(response);

                                        break;
                                    case "ping":
                                        ping(response);

                                        break;
                                }

                                Requests.Remove(tmprespo.id);
                            }
                        }
                        catch (Exception e)
                        {
                            Parent.updateStatus("Error: " + response);
                        }
                }
                catch (Exception e)
                {
                    if (!apiclient.Connected)
                        if (!inauth)
                            Auth();
                }

                ReadBuffer = new byte[512];

                if (apiclient.Connected)
                    try
                    {
                        sslStream.EndRead(result);
                        sslStream.BeginRead(ReadBuffer, 0, 512, ReadTCP, sslStream);
                    }
                    catch
                    {
                        if (!apiclient.Connected)
                            if (!inauth)
                                Auth();
                    }
            }
            catch (Exception e)
            {
                if (!apiclient.Connected)
                    if (!inauth)
                        Auth();
            }
        }

        private void Beginreadthread()
        {
            sslStream.BeginRead(ReadBuffer, 0, 512, ReadTCP, sslStream);
            Write("read_current_seed", "{\"selector\":{\"user_id\":" + uid + "}}");
        }

        private void ReadUser(string response)
        {
            var tmpstats = JsonUtils.JsonDeserialize<YLUserStats>(response).result;

            if (tmpstats != null)
            {
                balance = tmpstats.balance / 100000000m;
                bets = (int) tmpstats.bets;
                wins = (int) tmpstats.wins;
                losses = (int) tmpstats.losses;
                profit = tmpstats.profit / 100000000m;
                wagered = tmpstats.wagered / 100000000m;
                Parent.updateBalance(balance);
                Parent.updateBets(bets);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
            }
        }

        private void ProcessBet(string response)
        {
            var tmpbetrespo = JsonUtils.JsonDeserialize<YLBetResponse>(response).result;
            lastbet = DateTime.Now;
            delay = tmpbetrespo.delay;

            if (tmpbetrespo != null)
            {
                var tmp = new Bet
                {
                    Guid = Guid,
                    Id = tmpbetrespo.id.ToString(),
                    Amount = tmpbetrespo.amount / 100000000m,
                    date = DateTime.Now,
                    Chance = tmpbetrespo.target / 10000m,
                    high = tmpbetrespo.range == "hi",
                    Profit = tmpbetrespo.profit / 100000000m,
                    Roll = tmpbetrespo.rolled / 10000m,
                    nonce = tmpbetrespo.nonce,
                    Currency = tmpbetrespo.coin
                };

                var sent = false;
                var StartWait = DateTime.Now;

                if (Currentseed == null)

                    //while (Currentseed == null && (DateTime.Now-StartWait).TotalSeconds<20)
                    if (!sent)
                    {
                        sent = true;
                        Write("read_seed", "{\"selector\":{\"id\":" + tmpbetrespo.seed_id + "}}");
                        Parent.updateStatus("Getting seed data. Please wait.");
                    }

                //Thread.Sleep(100);

                if (Currentseed != null)
                    if (Currentseed.id != tmpbetrespo.seed_id)

                        //while (Currentseed.id != tmpbetrespo.seed_id && (DateTime.Now - StartWait).TotalSeconds < 20)
                        if (!sent)
                        {
                            sent = true;
                            Write("read_seed", "{\"selector\":{\"id\":" + tmpbetrespo.seed_id + "}}");
                            Parent.updateStatus("Getting seed data. Please wait.");
                        }

                //Thread.Sleep(100);

                if (Currentseed != null)
                {
                    tmp.serverhash = Currentseed.secret_hashed;
                    tmp.clientseed = Currentseed.client_seed;
                }

                if (tmpbetrespo.user_data != null)
                {
                    balance = tmpbetrespo.user_data.balance / 100000000m;
                    bets = (int) tmpbetrespo.user_data.bets;
                    wins = (int) tmpbetrespo.user_data.wins;
                    losses = (int) tmpbetrespo.user_data.losses;
                    profit = tmpbetrespo.user_data.profit / 100000000m;
                    wagered = tmpbetrespo.user_data.wagered / 100000000m;
                }
                else
                {
                    balance += tmp.Profit;
                    wagered += tmp.Amount;
                    bets++;
                    var win = false;
                    if (tmp.Roll > maxRoll - tmp.Chance && High || tmp.Roll < tmp.Chance && !High) win = true;

                    if (win)
                        wins++;
                    else
                        losses++;
                }

                FinishedBet(tmp);
            }
        }

        private void read_current_seed(string response)
        {
            var tmp = JsonUtils.JsonDeserialize<YLSeed>(response).result;
            if (tmp != null) Currentseed = tmp;
        }

        private void create_seed(string response)
        {
            var tmp = JsonUtils.JsonDeserialize<YLSeed>(response).result;
            if (tmp != null) Currentseed = tmp;
        }

        private void patch_seed(string response)
        {
        }

        private void create_withdrawal(string response)
        {
            Write("read_user_coin_data", "{\"selector\":{\"id\":\"" + uid + "_" + Currency.ToLower() + "\"}}");
        }

        private void ping(string response)
        {
        }

        private void Write(string Method, string Params)
        {
            if (apiclient.Connected)
                try
                {
                    var s = string.Format(basestring, id, Method, Params == "" ? "" : ",\"params\":" + Params);

                    var bytes = Encoding.UTF8.GetBytes(s);

                    Requests.Add(id++, Method);
                    sslStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception e)
                {
                    if (apiclient.Connected)
                    {
                        Parent.updateStatus("It seems an error has occured!");
                    }
                    else
                    {
                        if (ispd)
                        {
                            Parent.updateStatus("Disconnected. Reconnecting... Click start in a few seconds.");

                            if (!inauth)
                                Auth();
                        }
                    }
                }
            else if (ispd && !inauth) Auth();
        }

        private void Auth()
        {
            inauth = true;

            try
            {
                apiclient.Close();
            }
            catch
            {
            }

            try
            {
                apiclient = new TcpClient();

                apiclient.Connect("api.yolodice.com", 4444);

                if (apiclient.Connected)
                {
                    sslStream = new SslStream(apiclient.GetStream());
                    sslStream.AuthenticateAsClient("https://api.yolodice.com");

                    var frstchallenge = string.Format(basestring, id++, "generate_auth_challenge", "");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    var bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLChallenge tmp = null;

                    try
                    {
                        tmp = JsonUtils.JsonDeserialize<YLChallenge>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        /*finishedlogin(false);
                        return;*/
                    }

                    var tmpkey = Key.Parse(privkey);
                    var address = tmpkey.ScriptPubKey.GetDestinationAddress(Network.GetNetwork("Main")).ToString();
                    var message = tmpkey.SignMessage(tmp.result);

                    frstchallenge = string.Format(basestring, id++, "auth_by_address",
                                                  ",\"params\":" + JsonUtils.JsonSerializer(new YLAuthSend {address = address, signature = message}));

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLOgin tmologin = null;

                    try
                    {
                        tmologin = JsonUtils.JsonDeserialize<YLOgin>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        /*finishedlogin(false);
                        return;*/
                    }

                    uid = tmologin.result.id;
                    username = tmologin.result.name;

                    frstchallenge = string.Format(basestring, id++, "read_user_coin_data",
                                                  ",\"params\":{\"selector\":{\"id\":\"" + uid + "_" + Currency.ToLower() + "\"}}");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 512);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLUserStats tmpstats = null;

                    try
                    {
                        tmpstats = JsonUtils.JsonDeserialize<YLUserStats>(challenge).result;
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        /*finishedlogin(false);
                        return;*/
                    }

                    if (tmpstats != null)
                    {
                        balance = tmpstats.balance / 100000000m;
                        bets = (int) tmpstats.bets;
                        wins = (int) tmpstats.wins;
                        losses = (int) tmpstats.losses;
                        profit = tmpstats.profit / 100000000m;
                        wagered = tmpstats.wagered / 100000000m;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(BalanceThread).Start();
                        new Thread(Beginreadthread).Start();

                        //sslStream.BeginRead(ReadBuffer, 0, 512, ReadTCP, sslStream);
                        //Write("read_current_seed", "{\"selector\":{\"user_id\":" + uid + "}}");
                        //privkey = Password;
                        //Thread.Sleep(50);
                        //finishedlogin(true);
                        inauth = false;

                        return;
                    }

                    //tmp2.ImportPrivKey(Password, "", false);
                    //string message = tmp2.SignMessage(username, tmp.result);
                    //string message = //FullSignatureTest(tmp.result, new AsymmetricCipherKeyPair("", ""));

                    /*ispd = true;
                    new Thread(new ThreadStart(BalanceThread)).Start();*/
                }
            }
            catch (Exception e)
            {
            }

            //finishedlogin(false);
            inauth = false;
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            return true;
            return (DateTime.Now - lastbet).TotalSeconds > (double) delay;
        }

        public override void Disconnect()
        {
            ispd = false;

            if (apiclient.Connected)
            {
                try
                {
                    sslStream.EndRead(null);
                }
                catch
                {
                }

                apiclient.Close();
            }
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = new HMACSHA512();

            var charstouse = 5;

            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i += 2)
            {
                serverb.Add((byte) int.Parse(server.Substring(i, 2), NumberStyles.HexNumber));
            }

            betgenerator.Key = serverb.ToArray();
            var hash = betgenerator.ComputeHash(Encoding.UTF8.GetBytes(client + "." + nonce));

            var hex = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            for (var i = 0; i < hex.Length; i += charstouse)
            {
                var s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, NumberStyles.HexNumber);

                if (lucky < 1000000)
                    return lucky / 10000;
            }

            return 0;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }
    }

    public class YLAuthSend
    {
        public string address { get; set; }
        public string signature { get; set; }
    }

    public class YLBasicResponse
    {
        public int id { get; set; }
    }

    public class YLChallenge
    {
        public int id { get; set; }
        public string result { get; set; }
    }

    public class YLOgin
    {
        public long id { get; set; }
        public YLOgin result { get; set; }
        public string name { get; set; }
    }

    public class YLUserStats
    {
        public string id { get; set; }

        //public long user_id { get; set; }
        public long bets { get; set; }
        public long wins { get; set; }
        public long losses { get; set; }
        public long profit { get; set; }
        public long wagered { get; set; }
        public long profit_min { get; set; }
        public long profit_max { get; set; }

        public decimal luck { get; set; }

        //public long chat_message_count  { get; set; }
        public long balance { get; set; }
        public YLUserStats result { get; set; }
    }

    public class YLBetResponse
    {
        public string coin { get; set; }
        public long id { get; set; }
        public YLBetResponse result { get; set; }
        public YLOgin user { get; set; }
        public string created_at { get; set; }
        public long target { get; set; }
        public string range { get; set; }
        public long amount { get; set; }
        public long rolled { get; set; }
        public long profit { get; set; }
        public long seed_id { get; set; }
        public long nonce { get; set; }
        public decimal delay { get; set; }
        public YLUserStats user_data { get; set; }
    }

    public class YLBetSend
    {
        public long amount { get; set; }
        public string range { get; set; }
        public int target { get; set; }
        public string coin { get; set; }
    }

    public class YLSeed
    {
        public long id { get; set; }
        public long user_id { get; set; }
        public string created_at { get; set; }
        public long bet_count { get; set; }
        public string client_seed { get; set; }
        public bool current { get; set; }
        public string secret_hashed { get; set; }
        public string secret { get; set; }
        public YLSeed result { get; set; }
    }

    public class YLWithdrawal
    {
        public string to_address { get; set; }
        public long amount { get; set; }
        public bool allow_pending { get; set; }
        public string coin { get; set; }
    }

    public class YLUserCoin
    {
    }
}
