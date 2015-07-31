using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDCAPI;

namespace DiceBot
{
    class JD: DiceSite
    {
        
        jdInstance Instance = new jdInstance();
        public JD(cDiceBot Parent)
        {
            maxRoll = 99.9999;
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
        }

        void Instance_OnAddress(Address Address)
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

        void Instance_OnLossess(long Lossess)
        {
            Parent.updateLosses(Lossess);
        }

        void Instance_OnWins(long Wins)
        {
            Parent.updateWins(Wins);
        }

        void Instance_OnChat(Chat chat)
        {
            ReceivedChatMessage(chat.Date.ToShortTimeString() +" ("+chat.UID+") <"+chat.User+"> "+ chat.RawMessage);
        }

        void Instance_OnRoll(Roll roll)
        {
            if (roll.server_seed != "")
            {
                sqlite_helper.InsertSeed(roll.hash, roll.server_seed);
                
                GettingSeed = false;
            }
        }

        void Instance_OnJDMessage(string Message)
        {
            Parent.updateStatus(Message);
        }

        void Instance_OnNewClientSeed(SeedInfo SeedInfo)
        {
            sqlite_helper.InsertSeed(SeedInfo.OldServerSeed.ToString(), SeedInfo.OldServerSeed.ToString());
        }

        
        
        void Instance_OnJDError(string Error)
        {
            Parent.updateStatus(Error);
        }

        

        void Instance_OnResult(Result result, bool IsMine)
        {
            if (IsMine)
            {
                
                
                balance = (Instance.Balance);
                bets=(int)Instance.Bets;
                losses = (int)Instance.Losses;
                wins = (int)Instance.Wins;

                profit = (double)Instance.Profit;
                wagered = (double)Instance.Wagered;

                FinishedBet(ToBet(result));
            }
        }

        protected override void internalPlaceBet(bool High)
        {
            Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, chance, High ? "High" : "Low"));
            Instance.Bet(chance, amount, High);
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

        public override bool Invest(double Amount)
        {
            
            Parent.updateStatus(string.Format("Investing {0:0.00000000}", Amount));
            Instance.Invest(Amount,"");
            System.Threading.Thread.Sleep(1500);
            return true;
        }
        protected override bool internalWithdraw(double Amount, string Address)
        {
            
            Instance.Withdraw(Address, Amount ,"");
            System.Threading.Thread.Sleep(1500);
            return true;
        }
        public override void Login(string Username, string Password, string twoFa)
        {
            
            bool tmp = Instance.Connect(false, Username, Password, twoFa);
            if (Instance.Connected)
            {
                Parent.updateBalance((decimal)Instance.Balance);
                Parent.updateBets(Instance.Bets);
                Parent.updateLosses(Instance.Losses);
                Parent.updateProfit(Instance.Profit);
                Parent.updateWagered(Instance.Wagered);
                Parent.updateWins(Instance.Wins);
                System.Threading.Thread.Sleep(300);
                Instance.Deposit();
                
            }

            else
            {
                
            }

            finishedlogin(tmp);
        }
        
        Bet ToBet(JDCAPI.Result curBet)
        {
            Bet tmp = new Bet();
            tmp.Amount = decimal.Parse(curBet.bet, System.Globalization.CultureInfo.InvariantCulture);
            tmp.date = jdInstance.ToDateTime2( curBet.date.ToString());
            tmp.Id = (long)curBet.betid;
            tmp.Profit = decimal.Parse(curBet.this_profit, System.Globalization.CultureInfo.InvariantCulture);
            tmp.Roll = (decimal)curBet.lucky/10000m;
            tmp.high = curBet.high;
            tmp.Chance = decimal.Parse(curBet.chance, System.Globalization.CultureInfo.InvariantCulture);
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

        public override void SendTip(string Username, double Amount)
        {
            int uid = -1;
            if (int.TryParse(Username, out uid))
            {
                Instance.Chat(string.Format("/tip {0} {1:0.00000000}", uid, Amount));
            }
            else
            {
                Parent.updateStatus("Invalid UserID");
            }
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
            bool tmp = Instance.Connect(false);
            if (Instance.Connected)
            {
                Instance.SetupAccount(username, password);
                Parent.updateBalance((decimal)Instance.Balance);
                Parent.updateBets(Instance.Bets);
                Parent.updateLosses(Instance.Losses);
                Parent.updateProfit(Instance.Profit);
                Parent.updateWagered(Instance.Wagered);
                Parent.updateWins(Instance.Wins);
                Instance.Deposit();

            }

            else
            {

            }

            finishedlogin(tmp);
            return tmp;

        }
    }
}
