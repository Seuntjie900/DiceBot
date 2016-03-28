using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBot.Core
{
    abstract class StrategyBase
    {

        public override string StrategyName { get; }
        public abstract double CalculateNextBet(bool Win);
        public abstract void RunReset(bool Win);
        public override void LoadString(string Folder);


    }
}
