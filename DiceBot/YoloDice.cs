using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading;
using System.Security.Cryptography;

namespace DiceBot
{
    class YoloDice:DiceSite
    {

        /* string accesstoken = "";
        DateTime LastSeedReset = new DateTime();*/
        public bool ispd = false;
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        /*HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        HttpClientHandler ClientHandlr;*/
        TcpClient apiclient = new TcpClient();
        SslStream sslStream;
        long id = 0;
        string basestring = "{{\"id\":{0},\"method\":\"{1}\"{2}}}\r\n";
        public YoloDice(cDiceBot Parent)
        {
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
        protected override void internalPlaceBet(bool High, decimal amount, decimal chance)
        {
            string bet = string.Format("{{\"attrs\":{0}}}", json.JsonSerializer<YLBetSend>(new YLBetSend{ amount=(long)(amount*100000000), range=High?"hi":"lo", target=(int)(chance*10000)}));
            Write("create_bet", bet);
        }
        Random R = new Random();

        public override void ResetSeed()
        {
            Write("create_seed", "{\"attrs\": {\"client_seed \":\""+R.Next(0,int.MaxValue)+"\"}}");
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }
        public override void Donate(decimal Amount)
        {
            internalWithdraw(Amount, "1SeUNtjiMm5sdEgFrePWgF3J6kLCexJgj");
        }
        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            Write("create_withdrawal", "{\"attrs\": " + json.JsonSerializer<YLWithdrawal>(new YLWithdrawal { allow_pending=true, amount=(long)(Amount*100000000), to_address=Address }) + "}");
            return true;
        }


        byte[] ReadBuffer = new byte[256];
        string challenge = "";
        string privkey = "";
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
                    sslStream.AuthenticateAsClient("https://api.yolodice.com");
                    
                    string frstchallenge = string.Format(basestring, id++, "generate_auth_challenge", "");
                    
                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    int bytes = sslStream.Read(ReadBuffer, 0, 256);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLChallenge tmp =null;
                    try
                    {
                        tmp = json.JsonDeserialize<YLChallenge>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        finishedlogin(false);
                        return;
                    }
                    string address = "";
                    string message = "";
                    try
                    {
                        NBitcoin.Key tmpkey = NBitcoin.Key.Parse(Password);
                        address = tmpkey.ScriptPubKey.GetDestinationAddress(NBitcoin.Network.GetNetwork("Main")).ToString();
                        message = tmpkey.SignMessage(tmp.result);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("API key format error. Are you using your Private key?");
                        finishedlogin(false);
                        return;
                    }
                    frstchallenge = string.Format(basestring, id++, "auth_by_address", ",\"params\":" + json.JsonSerializer<YLAuthSend>(new YLAuthSend { address = address, signature = message }));
                    //{"id":1,"method":"auth_by_address","params":{"address":"n3kmufwdR3Zzgk3k6NYeeLBxB9SpHKe5Tc","signature":"H4W6yMaVK6EzrTw/9jqmLh1lvoyFnxCFqRon2g25lJ7FTCAUHGJWWF3UJD5wCzCVafdjIfCmIYH2KyHboodjjcU="}}

                    //{"id":1,"method":"auth_by_address","params":{"address":"1PUgaiHavJrpi7r7JhkhwWj7Kf9Ls68Z6w","signature":"Hz0oh29Nho+bVz7zggS1dqx\/N7VAyD6jsk8k98qW84ild7D71Q9rUbmEE4GIj0a5eKPcK1EjvSEwwa74jBJRyY8="}}

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 256);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                     YLOgin tmologin=null;
                    try
                    {
                        tmologin = json.JsonDeserialize<YLOgin>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        finishedlogin(false);
                        return;
                    }

                    uid = tmologin.result.id;
                    this.username = tmologin.result.name;
                    frstchallenge = string.Format(basestring, id++, "read_user_data", ",\"params\":{\"selector\":{\"id\":" + uid + "}}");
                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 256);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLUserStats tmpstats = null;
                    try
                    {
                        tmpstats = json.JsonDeserialize<YLUserStats>(challenge).result;
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        finishedlogin(false);
                        return;
                    }

                    if (tmpstats != null)
                    {

                        balance = (decimal)tmpstats.balance / 100000000m;
                        bets = (int)tmpstats.bets;
                        wins = (int)tmpstats.wins;
                        losses = (int)tmpstats.losses;
                        profit = (decimal)tmpstats.profit / 100000000m;
                        wagered = (decimal)tmpstats.wagered / 100000000m;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(new ThreadStart(BalanceThread)).Start();
                        privkey = Password;
                        new Thread(new ThreadStart(Beginreadthread)).Start();
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
        

        void BalanceThread()
        {
            while (ispd)
            {
                if ((DateTime.Now- lastupdate).TotalSeconds>15)
                {
                    lastupdate = DateTime.Now;
                    Write("read_user_data", "{\"selector\":{\"id\":" + uid + "}}");
                    //string frstchallenge = string.Format(basestring, id++, "read_user_data", "");
                    //sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                
                }
                Thread.Sleep(1000);
            }
        }
        void ReadTCP(IAsyncResult result)
        {
            try
            {
                try
                {
                    string response = "";
                    
                    response = Encoding.ASCII.GetString(ReadBuffer, 0, 256);
                    
                    response = response.Replace("\0", "");
                    Parent.DumpLog(response, 10);
                    if (response != "")
                    {
                        
                        try
                        {
                            response = response.Substring(0, response.IndexOf("\n"));
                            
                            YLBasicResponse tmprespo = json.JsonDeserialize<YLBasicResponse>(response);
                            if (Requests.ContainsKey(tmprespo.id))
                            {
                                switch (Requests[tmprespo.id])
                                {
                                    case "read_user_data": ReadUser(response); break;
                                    case "create_bet": ProcessBet(response); break;
                                    case "read_current_seed": read_current_seed(response); break;
                                    case "read_seed": read_current_seed(response); break;
                                    case "create_seed": create_seed(response); break;
                                    case "patch_seed": patch_seed(response); break;
                                    case "create_withdrawal": create_withdrawal(response); break;
                                    case "ping": ping(response); break;
                                    
                                }
                                Requests.Remove(tmprespo.id);
                            }
                        }
                        catch (Exception e)
                        {
                            Parent.updateStatus("Error: " + response);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!apiclient.Connected)
                    {
                        if (!inauth)
                        Auth();
                    }
                }
                ReadBuffer = new byte[256];
                if (apiclient.Connected)
                {
                    try
                    {
                        sslStream.EndRead(result);
                        sslStream.BeginRead(ReadBuffer, 0, 256, ReadTCP, sslStream);
                    }
                    catch
                    {
                        if (!apiclient.Connected)
                        {
                            if (!inauth)
                            Auth();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!apiclient.Connected)
                {
                    if (!inauth)
                    Auth();
                }
            }
            
        }
        void Beginreadthread()
        {
            sslStream.BeginRead(ReadBuffer, 0, 256, ReadTCP, sslStream);
            Write("read_current_seed", "{\"selector\":{\"user_id\":" + uid + "}}");
                        
        }
        void ReadUser(string response)
        {
            YLUserStats tmpstats = json.JsonDeserialize<YLUserStats>(response).result;
            if (tmpstats != null)
            {
                balance = (decimal)tmpstats.balance / 100000000m;
                bets = (int)tmpstats.bets;
                wins = (int)tmpstats.wins;
                losses = (int)tmpstats.losses;
                profit = (decimal)tmpstats.profit / 100000000m;
                wagered = (decimal)tmpstats.wagered / 100000000m;
                Parent.updateBalance(balance);
                Parent.updateBets(bets);
                Parent.updateWins(wins);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
            }
        }
        DateTime lastbet = DateTime.Now;
        decimal delay = 0;
        void ProcessBet(string response)
        {
            YLBetResponse tmpbetrespo = json.JsonDeserialize<YLBetResponse>(response).result;
            lastbet = DateTime.Now;
            delay = tmpbetrespo.delay;
            if (tmpbetrespo!=null)
            {
                Bet tmp = new Bet()
                {
                    Id = tmpbetrespo.id.ToString(),
                     Amount= (decimal)tmpbetrespo.amount/100000000m,
                      date= DateTime.Now,
                       Chance= (decimal)tmpbetrespo.target/10000m,
                        high = tmpbetrespo.range=="hi",
                         Profit= (decimal)tmpbetrespo.profit/100000000m,
                            Roll = (decimal)tmpbetrespo.rolled / 10000m,
                             nonce = tmpbetrespo.nonce
                };
                bool sent = false;
                DateTime StartWait = DateTime.Now;
                if (Currentseed == null)
                {
                    //while (Currentseed == null && (DateTime.Now-StartWait).TotalSeconds<20)
                    {
                        if (!sent)
                        {
                            sent = true;
                            Write("read_seed", "{\"selector\":{\"id\":" + tmpbetrespo.seed_id + "}}");
                            Parent.updateStatus("Getting seed data. Please wait.");
                        }
                        //Thread.Sleep(100);
                    }
                }
                if (Currentseed != null)
                {
                    if (Currentseed.id != tmpbetrespo.seed_id)
                    {
                        //while (Currentseed.id != tmpbetrespo.seed_id && (DateTime.Now - StartWait).TotalSeconds < 20)
                        {
                            if (!sent)
                            {
                                sent = true;
                                Write("read_seed", "{\"selector\":{\"id\":" + tmpbetrespo.seed_id + "}}");
                                Parent.updateStatus("Getting seed data. Please wait.");
                            }
                            //Thread.Sleep(100);
                        }
                    }
                }
                if (Currentseed!=null)
                {
                    
                    tmp.serverhash = Currentseed.secret_hashed;
                    tmp.clientseed = Currentseed.client_seed;
                }
                if (tmpbetrespo.user_data!=null)
                {
                    balance = (decimal)tmpbetrespo.user_data.balance / 100000000m;
                    bets = (int)tmpbetrespo.user_data.bets;
                    wins = (int)tmpbetrespo.user_data.wins;
                    losses = (int)tmpbetrespo.user_data.losses;
                    profit = (decimal)tmpbetrespo.user_data.profit / 100000000m;
                    wagered = (decimal)tmpbetrespo.user_data.wagered / 100000000m;
                }
                else
                {
                    balance += tmp.Profit;
                    wagered += tmp.Amount;
                    bets++;
                    bool win = false;
                    if ((tmp.Roll > maxRoll - tmp.Chance && High) || (tmp.Roll < tmp.Chance && !High))
                    {
                        win = true;
                    }
                    if (win)
                        wins++;
                    else
                        losses++;
                }
                FinishedBet(tmp);
            }
        }
        YLSeed Currentseed = null;
        void read_current_seed(string response)
        {
            YLSeed tmp = json.JsonDeserialize<YLSeed>(response).result;
            if (tmp!=null)
            {
                Currentseed = tmp; 
            }
        }
        void create_seed(string response)
        {
            YLSeed tmp = json.JsonDeserialize<YLSeed>(response).result;
            if (tmp != null)
            {
                Currentseed = tmp;
            }
        }
        void patch_seed(string response)
        {

        }
        void create_withdrawal(string response)
        {
            Write("read_user_data", "{\"selector\":{\"id\":" + uid + "}}");
                    
        }
        void ping(string response)
        {

        }
        Dictionary<long, string> Requests = new Dictionary<long, string>();
        void Write(string Method, string Params)
        {
            if (apiclient.Connected)
            {
                try
                {
                    string s = string.Format(basestring,id, Method, (Params == "" ? "" : ",\"params\":" + Params));

                    byte[] bytes = Encoding.UTF8.GetBytes(s);

                    Requests.Add(id++, Method);
                    sslStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception e)
                {
                    if (apiclient.Connected)
                        Parent.updateStatus("It seems an error has occured!");
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
            }
            else if (ispd && !inauth)
            {
                Auth();
            }
        }
        bool inauth = false;
        void Auth()
        {
            inauth = true;
            try
            {
                apiclient.Close();

            }
            catch { }
            try
            {
                apiclient = new TcpClient();

                apiclient.Connect("api.yolodice.com", 4444);
                if (apiclient.Connected)
                {
                    sslStream = new SslStream(apiclient.GetStream());
                    sslStream.AuthenticateAsClient("https://api.yolodice.com");

                    string frstchallenge = string.Format(basestring, id++, "generate_auth_challenge", "");

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    int bytes = sslStream.Read(ReadBuffer, 0, 256);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLChallenge tmp = null;
                    try
                    {
                        tmp = json.JsonDeserialize<YLChallenge>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        /*finishedlogin(false);
                        return;*/
                    }
                    NBitcoin.Key tmpkey = NBitcoin.Key.Parse(privkey);
                    string address = tmpkey.ScriptPubKey.GetDestinationAddress(NBitcoin.Network.GetNetwork("Main")).ToString();
                    string message = tmpkey.SignMessage(tmp.result);

                    frstchallenge = string.Format(basestring, id++, "auth_by_address", ",\"params\":" + json.JsonSerializer<YLAuthSend>(new YLAuthSend { address = address, signature = message }));

                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 256);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLOgin tmologin = null;
                    try
                    {
                        tmologin = json.JsonDeserialize<YLOgin>(challenge);
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        /*finishedlogin(false);
                        return;*/
                    }

                    uid = tmologin.result.id;
                    this.username = tmologin.result.name;
                    frstchallenge = string.Format(basestring, id++, "read_user_data", ",\"params\":{\"selector\":{\"id\":" + uid + "}}");
                    sslStream.Write(Encoding.ASCII.GetBytes(frstchallenge));
                    bytes = sslStream.Read(ReadBuffer, 0, 256);
                    challenge = Encoding.ASCII.GetString(ReadBuffer, 0, bytes);
                    YLUserStats tmpstats = null;
                    try
                    {
                        tmpstats = json.JsonDeserialize<YLUserStats>(challenge).result;
                    }
                    catch (Exception e)
                    {
                        Parent.updateStatus("error: " + challenge);
                        /*finishedlogin(false);
                        return;*/
                    }

                    if (tmpstats != null)
                    {

                        balance = (decimal)tmpstats.balance / 100000000m;
                        bets = (int)tmpstats.bets;
                        wins = (int)tmpstats.wins;
                        losses = (int)tmpstats.losses;
                        profit = (decimal)tmpstats.profit / 100000000m;
                        wagered = (decimal)tmpstats.wagered / 100000000m;
                        Parent.updateBalance(balance);
                        Parent.updateBets(bets);
                        Parent.updateWins(wins);
                        Parent.updateLosses(losses);
                        Parent.updateProfit(profit);
                        Parent.updateWagered(wagered);
                        ispd = true;
                        lastupdate = DateTime.Now;
                        new Thread(new ThreadStart(BalanceThread)).Start();
                        new Thread(new ThreadStart(Beginreadthread)).Start(); 
                        //sslStream.BeginRead(ReadBuffer, 0, 256, ReadTCP, sslStream);
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
            return ((DateTime.Now - lastbet).TotalSeconds > (double)delay);
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
                catch { }
                apiclient.Close();
            }
        }

        public virtual decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }
        public static decimal sGetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();

            int charstouse = 5;

            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i += 2)
            {
                serverb.Add((byte)int.Parse(server.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
            }
            betgenerator.Key = serverb.ToArray();
            byte[] hash = betgenerator.ComputeHash(Encoding.UTF8.GetBytes(client + "." + nonce));

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
        public long id { get; set; }
        public long bets { get; set; }
        public long wins { get; set; }
        public long losses { get; set; }
        public long profit  { get; set; }
        public long wagered  { get; set; }
        public long profit_min  { get; set; }
        public long profit_max  { get; set; }
        public decimal luck   { get; set; }
        public long chat_message_count  { get; set; }
        public long balance  { get; set; }
        public YLUserStats result { get; set; }
    }
    public class YLBetResponse
    {
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
    }
    public class YLSeed
    {
        public long id { get; set; }
        public long user_id { get; set; }
        public string created_at  { get; set; }
        public long bet_count  { get; set; }
        public string client_seed  { get; set; }
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
    }
}
