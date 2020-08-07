using System;
using System.Globalization;
using System.Threading;
using DiceBot.Common;
using DiceBot.Forms;
using JDCAPI;
using Bet = DiceBot.Common.Bet;

namespace DiceBot.Sites
{
    internal class JD : DiceSite
    {
        private string Guid = "";

        private readonly jdInstance Instance = new jdInstance();

        public JD(cDiceBot Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = true;
            AutoWithdraw = true;
            ChangeSeed = true;
            BetURL = "https://just-dice.com/roll/";
            Instance.OnResult += Instance_OnResult;
            Instance.OnJDMessage += Instance_OnJDMessage;
            Instance.OnNewClientSeed += Instance_OnNewClientSeed;
            Instance.OnRoll += Instance_OnRoll;
            Instance.OnChat += Instance_OnChat;
            Instance.OnWins += Instance_OnWins;
            Instance.OnLossess += Instance_OnLossess;
            Instance.OnAddress += Instance_OnAddress;
            Instance.logging = false;

            this.Parent = Parent;
            Name = "JustDice";
            Tip = true;
            TipUsingName = false;
            SiteURL = "https://just-dice.com";
        }

        private void Instance_OnAddress(Address Address)
        {
            Parent.updateDeposit(Address.DepositAddress);
        }

        public override void SetProxy(string host, int port)
        {
            base.SetProxy(host, port);
            Instance.SetProxy(prox_host, prox_port);
        }

        public override void SetProxy(string host, int port, string username, string password)
        {
            base.SetProxy(host, port, username, password);
            Instance.SetProxy(prox_host, prox_port, prox_username, prox_pass);
        }

        private void Instance_OnLossess(long Lossess)
        {
            Parent.updateLosses(Lossess);
        }

        private void Instance_OnWins(long Wins)
        {
            Parent.updateWins(Wins);
        }

        private void Instance_OnChat(Chat chat)
        {
            ReceivedChatMessage(chat.Date.ToShortTimeString() + " (" + chat.UID + ") <" + chat.User + "> " + chat.RawMessage);
        }

        private void Instance_OnRoll(Roll roll)
        {
            if (roll.server_seed != "")
            {
                SQLiteHelper.InsertSeed(roll.hash, roll.server_seed);

                GettingSeed = false;
            }
        }

        private void Instance_OnJDMessage(string Message)
        {
            Parent.updateStatus(Message);
        }

        private void Instance_OnNewClientSeed(SeedInfo SeedInfo)
        {
            SQLiteHelper.InsertSeed(SeedInfo.OldServerSeed, SeedInfo.OldServerSeed);
        }

        private void Instance_OnJDError(string Error)
        {
            Parent.updateStatus(Error);
        }

        private void Instance_OnResult(Result result, bool IsMine)
        {
            if (IsMine)
            {
                balance = (decimal) Instance.Balance;
                bets = (int) Instance.Bets;
                losses = (int) Instance.Losses;
                wins = (int) Instance.Wins;

                profit = Instance.Profit;
                wagered = Instance.Wagered;
                var tmp = ToBet(result);
                tmp.Guid = Guid;
                FinishedBet(tmp);
            }
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chance, string Guid)
        {
            this.Guid = Guid;

            Parent.updateStatus(string.Format(NumberFormatInfo.InvariantInfo, "Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance,
                                              High ? "High" : "Low"));

            Instance.Bet((double) chance, (double) amount, High);
        }

        public override void ResetSeed()
        {
            Parent.updateStatus("Resetting Seed");

            Instance.Randomize();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Invest(decimal Amount)
        {
            Parent.updateStatus(string.Format(NumberFormatInfo.InvariantInfo, "Investing {0:0.00000000}", Amount));
            Instance.Invest((double) Amount, "");
            Thread.Sleep(1500);

            return true;
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            Instance.Withdraw(Address, (double) Amount, "");
            Thread.Sleep(1500);

            return true;
        }

        public override void Login(string Username, string Password, string twoFa)
        {
            var tmp = Instance.Connect(false, Username, Password, twoFa);

            if (Instance.Connected)
            {
                Parent.updateBalance((decimal) Instance.Balance);
                Parent.updateBets(Instance.Bets);
                Parent.updateLosses(Instance.Losses);
                Parent.updateProfit(Instance.Profit);
                Parent.updateWagered(Instance.Wagered);
                Parent.updateWins(Instance.Wins);
                Thread.Sleep(300);
                Instance.Deposit();
            }

            finishedlogin(tmp);
        }

        private Bet ToBet(Result curBet)
        {
            var tmp = new Bet();
            tmp.Amount = decimal.Parse(curBet.bet, CultureInfo.InvariantCulture);
            tmp.date = DateTime.Now;
            tmp.Id = curBet.betid.ToString();
            tmp.Profit = decimal.Parse(curBet.this_profit, CultureInfo.InvariantCulture);
            tmp.Roll = (decimal) curBet.lucky / 10000m;
            tmp.high = curBet.high;
            tmp.Chance = decimal.Parse(curBet.chance, CultureInfo.InvariantCulture);
            tmp.nonce = curBet.nonce;
            tmp.serverhash = Instance.shash;
            tmp.clientseed = Instance.seed;
            tmp.uid = int.Parse(Instance.uid);

            return tmp;
        }

        public override void Disconnect()
        {
            if (Instance.Connected)
                Instance.Disconnect();
        }

        public override void Donate(decimal Amount)
        {
            SendTip("91380", Amount);
        }

        public override bool InternalSendTip(string Username, decimal Amount)
        {
            var uid = -1;

            if (int.TryParse(Username, out uid))
            {
                Instance.Chat(string.Format(NumberFormatInfo.InvariantInfo, "/tip noconf {0} {1:0.00000000}", uid, Amount));

                return true;
            }

            Parent.updateStatus("Invalid UserID");

            return false;
        }

        public override void GetSeed(long BetID)
        {
            GettingSeed = true;
            Instance.Roll(BetID);
        }

        public override void SendChatMessage(string Message)
        {
            Instance.Chat(Message);
        }

        public override bool Register(string username, string password)
        {
            var tmp = Instance.Connect(false);

            if (Instance.Connected)
            {
                Instance.SetupAccount(username, password);
                Parent.updateBalance((decimal) Instance.Balance);
                Parent.updateBets(Instance.Bets);
                Parent.updateLosses(Instance.Losses);
                Parent.updateProfit(Instance.Profit);
                Parent.updateWagered(Instance.Wagered);
                Parent.updateWins(Instance.Wins);
                Instance.Deposit();
            }

            finishedlogin(tmp);

            return tmp;
        }
    }
}
