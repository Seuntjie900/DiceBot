using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiceBotCore.Strategies;

namespace DiceBotCore
{
    public class DiceBot
    {
        #region Stats Vars

        #endregion

        #region Settings Vars

        #endregion

        private StrategyBase strategy = null;

        public StrategyBase Strategy
        {
            get { return strategy; }
            set { strategy = value; }
        }

        public double Balance { get; set; }
        public double SessionWagered { get; set; }

        public delegate void dFinishedBetEvent(Bet CurrentBet);
        public event dFinishedBetEvent FinishedBet;

        

    }
}
