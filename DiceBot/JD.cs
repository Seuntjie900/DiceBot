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
            AutoInvest = true;
            AutoWithdraw = true;
            ChangeSeed = true;
            BetURL = "https://just-dice.com/roll/";
            Instance.OnResult += Instance_OnResult;
            Instance.OnJDMessage += Instance_OnJDMessage;
            Instance.OnNewClientSeed += Instance_OnNewClientSeed;
            Instance.OnRoll += Instance_OnRoll;
            Instance.logging = false;
            this.Parent = Parent;
            Name = "JustDice";
            Tip = true;
            TipUsingName = false;
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

        void Instance_OnAddress(string Address)
        {
            Parent.updateDeposit(Address);
        }

        void Instance_OnResult(Result result, bool IsMine)
        {
            if (IsMine)
            {
                
                bets = int.Parse(result.bets);
                Parent.updateBalance(Instance.Balance);
                Parent.updateBets(result.bets);
                Parent.updateLosses(result.stats.losses);
                Parent.updateProfit(result.profit );
                Parent.updateWagered(result.wagered );
                Parent.updateWins(result.stats.wins);
                Parent.AddBet(ToBet(result));
                Parent.GetBetResult(double.Parse(result.balance, System.Globalization.CultureInfo.InvariantCulture), result.win, (double.Parse(result.this_profit, System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        public override void PlaceBet(bool High)
        {
            Parent.updateStatus("Betting " + amount + " at " + chance + " " + (High?"High":"Low"));
            Instance.Bet(chance, amount, High);
        }

        public override void SetChance(string Chance)
        {
            this.chance = double.Parse(Chance, System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void SetAmount(double Amount)
        {
            this.amount = Amount;
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

        public override string GetbalanceValue()
        {
            return Instance.Balance.ToString("0.00000000");
        }

        public override string GetSiteProfitValue()
        {
            return Instance.Stats.profit.ToString("0.00000000"); 
        }

        public override string GetTotalBets()
        {
            return Instance.Bets.ToString("0.00000000");
        }

        public override string GetMyProfit()
        {
            return Instance.Profit.ToString("0.00000000");
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override bool Invest(double Amount)
        {
            Parent.updateStatus(string.Format("Investing {0:0.00000000}", Amount));
            Instance.Invest(Amount,0);
            System.Threading.Thread.Sleep(1500);
            return true;
        }
        public override bool Withdraw(double Amount, string Address)
        {
            Parent.updateStatus(string.Format("Withdrawing {0:0.00000000} to {1}", Amount, Address));
            Instance.Withdraw(Address, Amount ,"");
            System.Threading.Thread.Sleep(1500);
            return true;
        }
        public override bool Login(string Username, string Password, string twoFa)
        {
            bool tmp = Instance.Connect(false, Username, Password, twoFa);
            Parent.updateBalance((decimal)Instance.Balance);
            Parent.updateBets(Instance.Bets);
            Parent.updateLosses(Instance.Losses);
            Parent.updateProfit(Instance.Profit);
            Parent.updateWagered(Instance.Wagered);
            Parent.updateWins(Instance.Wins);
            System.Windows.Forms.MessageBox.Show("Logged in!\n\nWelcome " + Username);
            Parent.updateStatus("Logged in! Welcome " + Username);
            return tmp;
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
    }
}
