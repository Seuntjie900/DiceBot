using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using DiceBot.Common;
using DiceBot.Forms;

namespace DiceBot.Sites
{
    public abstract class DiceSite
    {
        public delegate void dFinishedLogin(bool LoggedIn);

        protected string _MFAText = "2FA Code:";

        protected string _PasswordText = "Password: ";

        protected string _UsernameText = "Username: ";
        public decimal amount = 0;
        public bool AutoUpdate = true;
        protected int bets = 0;
        public string BetURL = "";
        public decimal chance = 0;
        public string[] Currencies = {"btc"};
        private string currency = "Btc";
        public decimal edge = 1;
        public bool ForceUpdateStats;
        protected bool High = false;
        protected int losses = 0;

        protected cDiceBot Parent;
        protected decimal profit = 0;
        protected WebProxy Prox;
        protected string prox_host = "";
        protected string prox_pass = "";
        protected int prox_port = 3128;
        protected string prox_username = "";
        protected decimal siteprofit = 0;
        protected decimal wagered = 0;
        protected int wins = 0;
        public bool register { get; set; } = true;

        public decimal maxRoll { get; set; }
        public string SiteURL { get; set; }

        public string UsernameText => _UsernameText;

        public string PasswordText => _PasswordText;

        public string MFAText => _MFAText;

        public string XtraText { get; set; } = "2FA Code:";

        public bool ShowXtra { get; set; } = false;

        public bool NonceBased { get; set; } = true;

        public string Currency
        {
            get => currency;
            set
            {
                currency = value;
                CurrencyChanged();
            }
        }

        public bool AutoWithdraw { get; set; }
        public bool AutoInvest { get; set; }
        public bool ChangeSeed { get; set; }
        public bool AutoLogin { get; set; }
        public string Name { get; protected set; }
        public decimal balance { get; protected set; }
        public bool Tip { get; set; }
        public bool TipUsingName { get; set; }
        public bool GettingSeed { get; set; }
        public event EventHandler<RequireCaptchaEventArgs> OnRequireCaptcha;

        protected void RequireCaptcha(RequireCaptchaEventArgs e)
        {
            OnRequireCaptcha?.Invoke(this, e);
        }

        protected virtual void CurrencyChanged()
        {
        }

        public int GetWins()
        {
            return wins;
        }

        public decimal GetProfit()
        {
            return profit;
        }

        public decimal GetWagered()
        {
            return wagered;
        }

        public int GetLosses()
        {
            return losses;
        }

        public int GetBets()
        {
            return bets;
        }

        public void PlaceBet(bool High, decimal amount, decimal chance, string BetGuid)
        {
            Parent.updateStatus(string.Format(NumberFormatInfo.InvariantInfo, "Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance,
                                              High ? "High" : "Low"));

            internalPlaceBet(High, amount, chance, BetGuid);
        }

        protected void FinishedBet(Bet newBet)
        {
            Parent.updateBalance(balance);
            Parent.updateBets(bets);
            Parent.updateLosses(losses);
            Parent.updateProfit(profit);
            Parent.updateWagered(wagered);
            Parent.updateWins(wins);
            Parent.AddBet(newBet);
            Parent.GetBetResult(balance, newBet);
        }

        protected abstract void internalPlaceBet(bool High, decimal amount, decimal chancem, string BetGuid);
        public abstract void ResetSeed();
        public abstract void SetClientSeed(string Seed);

        public virtual bool Invest(decimal Amount)
        {
            return true;
        }

        public virtual void Donate(decimal Amount)
        {
        }

        public bool Withdraw(decimal Amount, string Address)
        {
            Parent.updateStatus(string.Format(NumberFormatInfo.InvariantInfo, "Withdrawing {0} {1} to {2}", Amount, currency, Address));
            var res = internalWithdraw(Amount, Address);

            if (res)
            {
                if (AutoUpdate)
                    ForceUpdateStats = true;
                else
                    balance -= amount;
            }

            return res;
        }

        protected abstract bool internalWithdraw(decimal Amount, string Address);

        public abstract void Login(string Username, string Password, string twofa);

        public abstract bool Register(string username, string password);

        public abstract bool ReadyToBet();

        public virtual decimal GetLucky(string server, string client, int nonce)
        {
            var betgenerator = new HMACSHA512();

            var charstouse = 5;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            var buffer = new List<byte>();
            var msg = /*nonce.ToString() + ":" + */client + ":" + nonce;

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            var hash = betgenerator.ComputeHash(buffer.ToArray());

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

        public static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = new HMACSHA512();

            var charstouse = 5;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            var buffer = new List<byte>();
            var msg = /*nonce.ToString() + ":" + */client + ":" + nonce;

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            var hash = betgenerator.ComputeHash(buffer.ToArray());

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

        public abstract void Disconnect();

        public abstract void GetSeed(long BetID);
        public abstract void SendChatMessage(string Message);

        protected void ReceivedChatMessage(string Message)
        {
            Parent.AddChat(Message);
        }

        public bool SendTip(string User, decimal Amount)
        {
            Parent.updateStatus("Tipping " + Amount + " to " + User);
            var res = InternalSendTip(User, Amount);

            if (res)
            {
                if (AutoUpdate)
                    ForceUpdateStats = true;
                else
                    balance -= amount;
            }

            return res;
        }

        public virtual bool InternalSendTip(string User, decimal amount)
        {
            Parent.updateStatus("Tipping is not enabled for the current site.");

            return false;
        }

        protected void finishedlogin(bool Success)
        {
            Parent.updateBalance(balance);
            Parent.updateWagered(wagered);
            Parent.updateProfit(profit);
            Parent.updateBets(bets);
            Parent.updateWins(wins);
            Parent.updateLosses(losses);

            if (FinishedLogin != null)
                FinishedLogin(Success);
        }

        public event dFinishedLogin FinishedLogin;

        public virtual void SetProxy(string host, int port)
        {
            prox_host = host;
            prox_port = port;
            Prox = new WebProxy(prox_host, prox_port);
        }

        public virtual void SetProxy(string host, int port, string username, string password)
        {
            SetProxy(host, port);
            prox_username = username;
            prox_pass = password;
            Prox = new WebProxy(prox_host, prox_port);
            Prox.Credentials = new NetworkCredential(prox_username, prox_pass);
        }
    }

    public class PlaceBetObj
    {
        public bool High { get; set; }
        public decimal Amount { get; set; }
        public decimal Chance { get; set; }
        public string Guid { get; set; }

        public PlaceBetObj(bool High, decimal Amount, decimal Chance, string Guid)
        {
            this.High = High;
            this.Amount = Amount;
            this.Chance = Chance;
            this.Guid = Guid;
        }
    }

    public class RequireCaptchaEventArgs : EventArgs
    {
        public string PublicKey { get; set; }
        public string RequestValue { get; set; }
        public string ResponseValue { get; set; }
        public string Domain { get; set; }
    }

    public class Random
    {
        private const string chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";

        private readonly RandomNumberGenerator r = RandomNumberGenerator.Create();

        public virtual uint Next(uint max)
        {
            var bytes = new byte[4];
            r.GetBytes(bytes);
            var result = BitConverter.ToUInt32(bytes, 0);

            return result % max;
        }

        public virtual uint Next(uint min, uint max)
        {
            var result = Next(max - min);

            return min + result;
        }

        public virtual int Next(int min, int max)
        {
            return min + Next(max - min);
        }

        public virtual int Next(int max)
        {
            var bytes = new byte[4];
            r.GetBytes(bytes);
            var result = BitConverter.ToInt32(bytes, 0);

            return Math.Abs(result % max);
        }

        public virtual int Next()
        {
            return Next(int.MaxValue);
        }

        public string RandomString(int length)
        {
            var x = "";

            while (x.Length > 0)
            {
                x += chars[Next(0, chars.Length)];
            }

            return x;
        }
    }
}
