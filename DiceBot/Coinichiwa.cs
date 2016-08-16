using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace DiceBot
{
    class Coinichiwa: DiceSite
    {
        TcpClient Client;
        int reqID = 0;
        SHA1 Hasher;
        string challenge = "";
        string apikey = "";
        string username = "";
        public Coinichiwa(cDiceBot Parent)
        {
            this.Parent = Parent;
            Hasher = SHA1.Create();
            maxRoll = 99.99m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL="https://www.coinichiwa.com/a/2947221";
            Name = "Coinichiwa";
            Tip = false;
            SiteURL = "https://www.coinichiwa.com/a/2947221";
            register = false;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(GetBalanceThread));
            t.Start();
        }
        bool iscw = true;
        DateTime lastBalance = DateTime.Now;
        void GetBalanceThread()
        {
            
            while (iscw)
            {
                try
                {
                    if (Client!=null)
                    if (Client.Connected && (DateTime.Now-lastBalance).Seconds>30)
                    {
                        lastBalance = DateTime.Now;
                        Write("stats", username);
                    }
                }
                catch
                {

                }
            }
            
        }

        void placebetthread(object High)
        {
            Write("dice", (amount * 100000000).ToString(System.Globalization.NumberFormatInfo.InvariantInfo), chance.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), ((bool)High ? "h" : "l"));
        }
        int delay = 0;
        DateTime LastBet = DateTime.Now;
        void ReceivedBet(CoinichiwaBet ReceivedBet)
        {
            delay = ReceivedBet.delay;
            LastBet = DateTime.Now;
            Bet tmp = new Bet();
            tmp.Amount = (decimal)amount;
            tmp.date = DateTime.Now;
            tmp.Roll = (decimal)ReceivedBet.luckyNumber;
            tmp.Id = ReceivedBet.betid;
            tmp.Profit = (decimal)ReceivedBet.profit / 100000000m;
            tmp.high = High;
            tmp.Chance = (decimal)chance;
            tmp.nonce = ReceivedBet.seedIncrement;
            tmp.serverhash = "";
            tmp.clientseed = "";
            bool Win = (((bool)tmp.high ? (decimal)tmp.Roll > (decimal)maxRoll - (decimal)(tmp.Chance) : (decimal)tmp.Roll < (decimal)(tmp.Chance)));
            if (Win)
                wins++;
            else
                losses++;
            bets++;
            balance = ReceivedBet.balance/100000000.0m;
            profit += (decimal)tmp.Profit;
            wagered += (decimal)tmp.Amount; 
            FinishedBet(tmp);
        }
        protected override void internalPlaceBet(bool High, decimal chance, decimal amount)
        {
            
            this.High = High;
            new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(placebetthread)).Start(High);
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }
        byte[] ReadBuffer = new byte[256];

        public override void Login(string Username, string Password, string twofa)
        {
            reqID = 0;
            if (Client!=null)
            {
                try { Client.Close(); }catch
                { }
            }
            Client = new TcpClient("api.coinichiwa.com", 81);
            if (Client.Connected)
            {
                apikey = Password;
                //Client.Connect("api.coinichiwa.com", 81);
                ReadBuffer = new byte[256];
                Client.GetStream().Read(ReadBuffer, 0, 256);

                foreach (byte b in ReadBuffer)
                {
                    if ((char)b != '\n' && (char)b != '\0')
                    {
                        challenge += (char)b;
                    }
                }
                challenge = challenge.Substring(challenge.IndexOf(":") + 1);
                challenge = challenge.Replace(" ", "");
                string msg = string.Format("{0}:{1}:{2}:{3}:{4}", reqID, "auth", Username, challenge, Password);
                string msg2 = string.Format("{0}:{1}:{2}:{3}\n", reqID++, "auth", Username, computehash(msg));
                ReadBuffer = new byte[256];
                for (int i = 0; i < msg2.Length && i < ReadBuffer.Length; i++)
                {
                    ReadBuffer[i] = (byte)msg2[i];
                }
                Client.GetStream().Write(ReadBuffer, 0, msg2.Length);
                Client.GetStream().Read(ReadBuffer, 0, 256);
                string response = "";
                foreach (byte b in ReadBuffer)
                {
                    if ((char)b != '\n' && (char)b != '\0')
                    {
                        response += (char)b;
                    }
                }
                if (response.Split(':')[1] == "ok")
                {
                    ReadBuffer = new byte[256];
                    username = Username;
                    Client.GetStream().BeginRead(ReadBuffer, 0, 256, Read, Client.GetStream());
                    System.Threading.Thread.Sleep(200);
                    Write("info", Username);
                    System.Threading.Thread.Sleep(200);
                    Write("stats", Username);
                    System.Threading.Thread.Sleep(200);
                    Write("deposit", Username);
                    finishedlogin(true);
                }
                else finishedlogin(false);
            }
            else finishedlogin(false);
        }

        string serverSeed = "";
        string ClientSeed = "";
        int nonce = 0;

        void info(CoinichiwaInfo Info)
        {
            serverSeed = Info.hash;
            ClientSeed = Info.clientSeed;
            nonce = Info.nonce;
        }

        void stats(CoinichiwatStat Stat)
        {
            wins = Stat.wins;
            losses = Stat.losses;
            balance = Stat.balance/100000000.0m;
            wagered = Stat.wagered / 100000000.0m;
            profit = Stat.profit / 100000000.0m;
            bets = Stat.bets;
            Parent.updateBalance(balance);
            Parent.updateBets(bets);
            Parent.updateLosses(losses);
            Parent.updateProfit(profit);
            Parent.updateWagered(wagered);
            Parent.updateWins(wins);
            
        }

        void deposit(CoinichiwaDepost Deposit)
        {
            Parent.updateDeposit(Deposit.address);
        }

        void Write(string Method, params string[] Params)
        {
            if (Client.Connected)
            {
                try
                {
                    string s = reqID + ":" + Method + ":";
                    foreach (string p in Params)
                    {
                        s += p + ":";
                    }
                    string hash = computehash(s + challenge + ":" + apikey);
                    s = s + hash + "\n";
                    byte[] bytes = new byte[s.Length];
                    for (int i = 0; i < s.Length; i++)
                    {
                        bytes[i] = (byte)s[i];
                    }
                    Requests.Add(reqID++, Method);
                    Client.GetStream().Write(bytes, 0, bytes.Length);
                }
                catch
                {
                    Parent.updateStatus("It seems an error has occured!");
                }
            }
        }

        string computehash(string msg)
        {
            List<byte> bytes = new List<byte>();
            foreach (char c in msg)
            {
                bytes.Add((byte)c);
            }
            byte[] to = Hasher.ComputeHash(bytes.ToArray());
            StringBuilder hex = new StringBuilder(to.Length * 2);
            foreach (byte b in to)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        Dictionary<int, string> Requests = new Dictionary<int, string>();
        private void Read(IAsyncResult ar)
        {
            try
            {
                string response = "";
                foreach (byte b in ReadBuffer)
                {
                    if ((char)b != '\n' && (char)b != '\0')
                    {
                        response += (char)b;
                    }
                }
                ReadBuffer = new byte[256];
                string[] parts = response.Split(':');
                int id = 0;
                if (parts.Length > 1)
                {
                    if (int.TryParse(parts[0], out id) && parts[1] == "ok")
                    {
                        if (Requests.ContainsKey(id))
                        {
                            string method = Requests[id];
                            Requests.Remove(id);
                            switch (method)
                            {
                                case "dice": ReceivedBet(new CoinichiwaBet
                                {
                                    delay = int.Parse(parts[2]),
                                    betid = long.Parse(parts[3]),
                                    result = parts[4],
                                    luckyNumber = decimal.Parse(parts[5], System.Globalization.NumberFormatInfo.InvariantInfo),
                                    seedIncrement = int.Parse(parts[6]),
                                    profit = decimal.Parse(parts[7], System.Globalization.NumberFormatInfo.InvariantInfo),
                                    jackpot = int.Parse(parts[8]),
                                    balance = decimal.Parse(parts[9], System.Globalization.NumberFormatInfo.InvariantInfo)
                                }); break;
                                case "info": info(new CoinichiwaInfo { username = parts[2], clientSeed = parts[3], nonce = int.Parse(parts[4]), hash = parts[5] }); break;
                                case "stats": stats(new CoinichiwatStat { balance = decimal.Parse(parts[2], System.Globalization.NumberFormatInfo.InvariantInfo), 
                                    bets = int.Parse(parts[3]), 
                                    wins = int.Parse(parts[4]), 
                                    losses = int.Parse(parts[5]), 
                                    wagered = decimal.Parse(parts[6], System.Globalization.NumberFormatInfo.InvariantInfo), 
                                    profit = decimal.Parse(parts[7], System.Globalization.NumberFormatInfo.InvariantInfo) }); break;
                                case "deposit": deposit(new CoinichiwaDepost { address = parts[2] }); break;
                            }
                        }
                    }
                }
                if (Client.Connected)
                {
                    try
                    {
                        Client.GetStream().EndRead(ar);
                        Client.GetStream().BeginRead(ReadBuffer, 0, 256, Read, Client.GetStream());
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {
                Parent.updateStatus("It seems an error as occured");
            }
        }


        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            return ((DateTime.Now - LastBet).TotalMilliseconds > delay);
        }

        public override void Disconnect()
        {
            iscw = false;
            try
            {
                if (Client != null)
                    if (Client.Connected)
                    {
                        Client.Close();
                    }
            }
            catch
            {

            }
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

    class CoinichiwaBet
    {
        public int delay { get; set; }
        public long betid { get; set; }
        public string result { get; set; }
        public decimal luckyNumber { get; set; }
        public int seedIncrement { get; set; }
        public decimal profit { get; set; }
        public int jackpot { get; set; }
        public decimal balance { get; set; }
    }
    class CoinichiwatStat
    {
        public decimal balance { get; set; }
        public int wins { get; set; }
        public int bets { get; set; }
        public int losses { get; set; }
        public decimal wagered { get; set; }
        public decimal profit { get; set; }
    }
    class CoinichiwaInfo
    {
        public string username { get; set; }
        public string clientSeed { get; set; }
        public int nonce { get; set; }
        public string hash { get; set; }
    }
    class CoinichiwaDepost
    {
        public string address { get; set; }
    }
}
