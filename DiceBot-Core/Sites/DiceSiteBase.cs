using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore.Sites
{
    public abstract class DiceSiteBase
    {

        #region Properties
        /// <summary>
        /// Specifies wether the user can register a new account on the website using the bot.
        /// </summary>
        public bool CanRegister { get; protected set; }

        /// <summary>
        /// Specifies wether the bot is able to withdraw from the specified site.
        /// </summary>
        public bool AutoWithdraw { get; protected set; }

        /// <summary>
        /// Specifies whether the bot can invest coins into the site, if the site supports the feature.
        /// </summary>
        public bool AutoInvest { get; protected set; }

        /// <summary>
        /// Specifies whether the bot can reset the seed for the player.
        /// </summary>
        public bool CanChangeSeed { get; protected set; }

        /// <summary>
        /// Specifies Whether the bot can set the client seed for the current or next seed.
        /// </summary>
        public bool CanSetClientSeed { get; protected set; }

        /// <summary>
        /// Specifies whether the bot can send a tip to another player, if the site supports the feature.
        /// </summary>
        public bool CanTip { get; protected set; }

        /// <summary>
        /// Specify whether tipping on the site uses a username (true, string) or a userID (false, int)
        /// </summary>
        public bool TipUsingName { get; protected set; }

        /// <summary>
        /// Specify whether the bot can fetch the server seed for a specific bet
        /// </summary>
        public bool CanGetSeed { get; protected set; }

        /// <summary>
        /// True if the bot is busy getting the server seed for a specific bet
        /// </summary>
        public bool GettingSeed { get; protected set; }

        /// <summary>
        /// Specifies whether the roll verifier for the site is implemented and working.
        /// </summary>
        public bool CanVerify { get; protected set; }

        /// <summary>
        /// The Reflink URL of the site
        /// </summary>
        public string SiteURL { get; protected set; }

        /// <summary>
        /// The Name of the site
        /// </summary>
        public string SiteName { get; protected set; }

        /// <summary>
        /// The URL where more details for a bet can be seen, using string.format formatting, where {0} is the betID.
        /// </summary>
        public string BetURL { get; protected set; }

        /// <summary>
        /// The index of the list of supported currencies for the current currency
        /// </summary>
        public int Currency { get; set; }

        /// <summary>
        /// The name/abbreviation of the currency currently in use
        /// </summary>
        public string CurrentCurrency { get { return Currencies[Currency]; } }

        /// <summary>
        /// The maximum roll allowed at the site. Usually 99.99. Used to determine whether the roll is a win
        /// </summary>
        public double MaxRoll { get; protected set; }

        /// <summary>
        /// The house edge for the site. Used to determine payout and profit for bets and simulations
        /// </summary>
        public double Edge { get; protected set; }

        /// <summary>
        /// List of currencies supported by the site
        /// </summary>
        public string[] Currencies { get; protected set; }

        /// <summary>
        /// Indicates whether the bot can connect to and use the chat on the site
        /// </summary>
        public bool CanChat { get; protected set; }

        /// <summary>
        /// Site Statistics about the user 
        /// </summary>
        public SiteStats Stats { get; protected set; }

        public string SiteAbbreviation { get; set; }
        #endregion

        #region Required Methods
        
        /// <summary>
        /// Tell the site interface to place the bet. Required function for basic operation of the bot. 
        /// </summary>
        /// <param name="Amount">Amount to bet in full Coin.</param>
        /// <param name="Chance">Chance to win (0-maxroll-edge)</param>
        /// <param name="High">Roll High/Over or Low/Under</param>
        protected abstract void _PlaceBet(double Amount, double Chance, bool High);

        /// <summary>
        /// Place a Bet
        /// </summary>
        /// <param name="Amount">Amount to bet in full Coin.</param>
        /// <param name="Chance">Chance to win (0-maxroll-edge)</param>
        /// <param name="High">Roll High/Over or Low/Under</param>
        public void PlaceBet(double Amount, double Chance, bool High)
        {
            
        }

        /// <summary>
        /// Interface with site to handle login.
        /// </summary>
        /// <param name="Username">Account Username</param>
        /// <param name="Password">Account password or API key</param>
        /// <param name="TFA">Two factor authentication, if required</param>
        protected abstract bool _Login(string Username, string Password, string TFA);

        /// <summary>
        /// Logs the user into the site if correct details were provided
        /// </summary>
        /// <param name="Username">Account Username</param>
        /// <param name="Password">Account password or API key</param>
        /// <param name="TFA">Two factor authentication, if required</param>
        public void LogIn(string Username, string Password, string TFA)
        {
            bool Success = _Login(Username, Password, TFA);
            UpdateStats();
            if (LoginFinished != null)
                LoginFinished(Success, this.Stats);
        }

        /// <summary>
        /// Interface with site to disconnect and dispose of applicable objects
        /// </summary>
        protected abstract void _Disconnect();

        /// <summary>
        /// Disconnect from the site, if connected
        /// </summary>
        public void Disconnect()
        {

        }

        /// <summary>
        /// Set the proxy for the connection to the site
        /// </summary>
        /// <param name="ProxyInfo"></param>
        public abstract void SetProxy(Helpers.ProxyDetails ProxyInfo);

        /// <summary>
        /// Update the site statistics for whatever reason.
        /// </summary>
        public void UpdateStats()
        {
            _UpdateStats();
            if (StatsUpdated != null)
                StatsUpdated(this.Stats);
        }

        /// <summary>
        /// Interface with the site to get the latest user stats
        /// </summary>
        protected abstract void _UpdateStats();
        #endregion

        #region Extention Methods
        public void ResetSeed(string ClientSeed)
        {
            if (CanChangeSeed)
            {
                _ResetSeed();
                if (CanSetClientSeed)
                {
                    SetClientSeed(ClientSeed);
                }
            }
            else
                callError("Reset Seed not allowed!", false);
        }
        protected virtual void _ResetSeed() { }

        public void SetClientSeed(string ClientSeed)
        {
            if (CanSetClientSeed)
            {
                _SetClientSeed(ClientSeed);
            }
            else
                callError("Setting Client Seed not allowed!", false);
        }
        protected virtual void _SetClientSeed(string ClientSeed){ }

        public void Invest(double Amount)
        {
            if (AutoInvest)
            {
                _Invest(Amount);
                UpdateStats();
            }
            else
                callError("Investing not allowed!", false);
        }
        protected virtual void _Invest(double Amount) { }

        public void Donate(double Amount)
        {
            if (AutoWithdraw || CanTip)
            {
                _Donate(Amount);
                UpdateStats();
            }
            else
                callError("Donations not Implemented!", false);
        }
        protected virtual void _Donate(double Amount) { }

        public void Withdraw(string Address, double Amount)
        {
            if (AutoWithdraw)
            {
                _Withdraw(Address, Amount);
                UpdateStats();
            }
            else
                callError("Withdrawing not allowed!", false);
        }
        protected virtual void _Withdraw(string Address, double Amount) { }

        public void Register(string Username, string Password)
        {
            if (CanRegister)
            {
                bool Success = _Register(Username, Password);
                UpdateStats();
                if (RegisterFinished != null)
                    RegisterFinished(Success);
            }
            else
                callError("Registering not allowed!", false);
            
        }
        protected virtual bool _Register(string Username, string Password) { return false; }

        public double GetLucky(string Hash, string ServerSeed, string ClientSeed, int Nonce)
        {
            if (CanVerify)
            {
                return _GetLucky(Hash, ServerSeed, ClientSeed, Nonce);
            }
            else
            {

                callError("Roll verifying not implemented!", false);
                return -1;
            }
        }
        protected virtual double _GetLucky(string Hash, string ServerSeed, string ClientSeed, int Nonce) { return int.MaxValue; }

        public string GetSeed(long BetID)
        {
            if (CanGetSeed)
            {
                return _GetSeed(BetID);
            }
            else
            {
                
                callError("Getting server seed not allowed!", false);
                return "-1";
            }
        }
        protected virtual string _GetSeed(long BetID) { return "-1"; }

        public void SendTip(string Username, double Amount)
        {
            if (CanTip)
            {
                _SendTip(Username, Amount);
            }
            else
                callError("Tipping not allowed!", false);
        }
        protected virtual void _SendTip(string Username, double Amount) { }

        public void SendChat(string Message)
        {
            if (CanChat)
            {
                _SendChat(Message);
            }
            else
                callError("Chatting not allowed!", false);
        }
        protected virtual void _SendChat(string Message) { }

        public virtual int _TimeToBet()
        {
            return -1;
        }

        public int TimeToBet()
        {
            return _TimeToBet();
        }
        #endregion

        #region Events
        public delegate void dStatsUpdated(SiteStats NewStats);
        public delegate void dBetFinished(Bet NewBet);
        public delegate void dLoginFinished(bool Success, SiteStats NewStats);
        public delegate void dRegisterFinished(bool Success);
        public delegate void dError(string Message, bool Fatal);
        public delegate void dNotify(string Message);
        public delegate void dAction(string CurrenctAction);
        public delegate void dChat(string Message);

        public event dStatsUpdated StatsUpdated;
        public event dBetFinished BetFinished;
        public event dLoginFinished LoginFinished;
        public event dRegisterFinished RegisterFinished;
        public event dError Error;
        public event dNotify Notify;
        public event dAction Action;
        public event dChat ChatReceived;

        protected void callStatsUpdated(SiteStats Stats)
        {
            if (StatsUpdated!=null)
            {
                StatsUpdated(Stats);
            }
        }
        protected void callBetFinished(Bet NewBet)
        {
            if (BetFinished != null)
            {
                BetFinished(NewBet);
            }
        }
        protected void callLoginFinished(bool Success)
        {
            if (LoginFinished != null)
            {
                LoginFinished(Success, this.Stats);
            }
        }
        protected void callRegisterFinished(bool Success)
        {
            if (RegisterFinished != null)
            {
                RegisterFinished(Success);
            }
        }
        protected void callError(string Message, bool Fatal)
        {
            if (Error != null)
            {
                Error(Message, Fatal);
            }
        }
        protected void callNotify(string Message)
        {
            if (Notify != null)
            {
                Notify(Message);
            }
        }
        protected void callAction(string CurrentAction)
        {
            if (Action != null)
            {
                Action(CurrentAction);
            }
        }
        protected void callChatReceived(string Message)
        {
            if (ChatReceived != null)
            {
                ChatReceived(Message);
            }
        }
        #endregion

    }

    public class SiteStats
    {
        public double Balance { get; set; }
        public double Wagered { get; set; }
        public double Profit { get; set; }
        public long bets { get; set; }
        public long wins { get; set; }
        public long losses { get; set; }
    }
}
