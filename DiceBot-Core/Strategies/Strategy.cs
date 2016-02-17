using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore
{
    public abstract class StrategyBase
    {
        
        public string StrategyName { get; protected set; }
        public abstract double CalculateNextBet(double Lastbet, bool Win);
        public abstract double RunReset();
        protected double Balance {get{return GetBalance();}}
        protected int Winstreak { get { return GetWinStreak(); } }
        protected int Losestreak { get { return GetLoseStreak(); } }
        protected bool High { get { return GetHigh(); } set { UpdateHigh(value); } }
        public virtual void LoadString(string Folder)
        {

        }

        protected double GetBalance()
        {
            if (NeedBalance != null)
                return NeedBalance();
            else
                return 0;
        }

        public delegate double dNeedBalance();
        public event dNeedBalance NeedBalance;

        protected int GetWinStreak()
        {
            if (NeedWinStreak != null)
                return NeedWinStreak();
            else
                return 0;
        }

        public delegate int dNeedWinStreak();
        public event dNeedWinStreak NeedWinStreak;

        protected int GetLoseStreak()
        {
            if (NeedLoseStreak != null)
                return NeedLoseStreak();
            else
                return 0;
        }

        public delegate int dNeedLoseStreak();
        public event dNeedLoseStreak NeedLoseStreak;

        protected void UpdateChance(double NewChance)
        {
            if (ChanceUpdated != null)
                ChanceUpdated(NewChance);
        }

        public delegate void dChanceUpdated(double NewChance);
        public event dChanceUpdated ChanceUpdated;

        protected void UpdateHigh(bool High)
        {
            if (HighUpdated != null)
                HighUpdated(High);
        }

        public delegate void dHighUpdated(bool high);
        public event dHighUpdated HighUpdated;

        protected bool GetHigh()//lol
        {
            if (NeedHigh != null)
                return NeedHigh();
            return false;
        }

        public delegate bool dNeedHigh();
        public event dNeedHigh NeedHigh;

        protected void CallStop(string Reason)//lol
        {
            if (Stop != null)
                Stop(Reason);
            
        }

        public delegate void dStop(string Reason);
        public event dStop Stop;
    }
}
