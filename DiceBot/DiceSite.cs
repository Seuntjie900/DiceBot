using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;
using System.Globalization;
using System.Web;
using System.Net;
using System.IO;
namespace DiceBot
{
    public abstract class DiceSite
    {
        bool reg = true;
        public bool register { get { return reg; } set { reg = value; } }
        protected string prox_host = "";
        protected int prox_port = 3128;
        protected string prox_username = "";
        protected string prox_pass = "";
        protected WebProxy Prox;
        public string[] Currencies = new string[] { "btc" };
        public decimal maxRoll { get; set; }
        string currency = "Btc";
        public string SiteURL { get; set; }

        protected string _UsernameText = "Username: ";
        public string UsernameText
        {
            get { return _UsernameText; }            
        }

        protected string _PasswordText = "Password: ";

        public string PasswordText
        {
            get { return _PasswordText; }
            
        }
        protected string _MFAText = "2FA Code:";

        public string MFAText
        {
            get { return _MFAText; }
            
        }
        private string xtraText="2FA Code:";

        public string XtraText
        {
            get { return xtraText; }
            set { xtraText = value; }
        }
        private bool showXtra=false;

        public bool ShowXtra
        {
            get { return showXtra; }
            set { showXtra = value; }
        }


        private bool _NonceBased=true;
        public bool NonceBased
        {
            get { return _NonceBased; }
            set { _NonceBased = value; }
        }
        
        public string Currency
        {
            get { return currency; }
            set { currency= value; CurrencyChanged();}}
            
        
        protected virtual void CurrencyChanged(){}

        protected cDiceBot Parent;
        public bool AutoWithdraw { get; set; }
        public bool AutoInvest { get; set; }
        public bool ChangeSeed {get;set;}
        public bool AutoLogin { get; set; }
        public decimal edge = 1;
        public string Name { get; protected set; }
        public decimal chance = 0;
        public decimal amount = 0;
        public decimal balance { get; protected set; }
        protected int bets = 0;
        protected decimal profit = 0;
        protected decimal wagered = 0;
        protected int wins = 0;
        protected int losses = 0;
        protected decimal siteprofit = 0;
        protected bool High = false;
        public string BetURL = "";
        public bool Tip { get; set; }
        public bool TipUsingName { get; set; }
        public bool GettingSeed { get; set; }
        public bool ForceUpdateStats = false;
        public bool AutoUpdate = true;

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
            Parent.updateStatus(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
            internalPlaceBet(High,amount, chance, BetGuid);
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
        protected abstract void internalPlaceBet(bool High,decimal amount, decimal chancem, string BetGuid);
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
            Parent.updateStatus(string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"Withdrawing {0} {1} to {2}", Amount, currency, Address));
            bool res = internalWithdraw(Amount, Address);

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
            HMACSHA512 betgenerator = new HMACSHA512();
            
            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i+=charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);
                
                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }
        public static decimal sGetLucky(string server, string client, int nonce)
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
            string msg = /*nonce.ToString() + ":" + */client + ":" + nonce.ToString();
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

        public abstract void Disconnect();
        
        public abstract void GetSeed(long BetID);
        public abstract void SendChatMessage(string Message);
        protected void ReceivedChatMessage(string Message)
        {
            Parent.AddChat(Message);
        }
        public bool SendTip(string User, decimal Amount)
        {
            Parent.updateStatus("Tipping "+Amount+" to "+User);
            bool res = InternalSendTip(User, Amount);
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
            if (FinishedLogin!=null)
                FinishedLogin(Success);
        }

        public delegate void dFinishedLogin(bool LoggedIn);
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
        public PlaceBetObj(bool High, decimal Amount, decimal Chance, string Guid)
        {
            this.High = High;
            this.Amount = Amount;
            this.Chance = Chance;
            this.Guid = Guid;
        }
        public bool High { get; set; }
        public decimal Amount { get; set; }
        public decimal Chance { get; set; }
        public string Guid { get; set; }
    }

}
