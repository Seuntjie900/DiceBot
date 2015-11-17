using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBot
{
    class InvestDice: moneypot
    {
        public InvestDice(cDiceBot Parent): base(Parent)
        {
            edge = 0.01m;
            appid = 497;
        }
    }
}
