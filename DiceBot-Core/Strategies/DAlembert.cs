using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore.Strategies
{
    class DAlembert: StrategyBase
    {

        public int AlembertStretchWin { get; set; }

        public int AlembertStretchLoss { get; set; }

        public double AlembertIncrementLoss { get; set; }

        public double MinBet { get; set; }

        public double AlembertIncrementWin { get; set; }

        public override double CalculateNextBet(double Lastbet, bool Win)
        {
            
            if (Win)
            {
                
                if ((Winstreak) % (AlembertStretchWin +1) == 0)
                {
                    Lastbet += AlembertIncrementWin;
                }
            }
            else
            {
                if ((Losestreak) % (AlembertStretchLoss + 1) == 0)
                {
                    Lastbet += AlembertIncrementLoss;
                }
            }
            if (Lastbet < MinBet)
                Lastbet = MinBet;

            return Lastbet;
        }

        public override double RunReset()
        {
            return MinBet;
        }

        
    }
}
