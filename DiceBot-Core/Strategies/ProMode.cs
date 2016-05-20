using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore.Strategies
{
    class ProMode:StrategyBase
    {
        public double BaseBet { get; set; }
        public double StartingChance { get; set; }

        public override double CalculateNextBet(double Lastbet, bool Win)
        {
            throw new NotImplementedException();
        }

        public override double RunReset()
        {
            throw new NotImplementedException();
        }
    }

    public class ProModeControl
    {
        public List<ProModeCondition> Conditions { get; set; }
        public List<ProModeAction> Actions { get; set; }

    }

    public class ProModeCondition
    {
        public string FirstVar { get; set; }
        public string SecondVar { get; set; }
        public ProModeLogic LogicOperator { get; set; }
        
    }
    public class ProModeAction
    {
        public ProModeOperators Operator { get; set; }
        public string FirstVar { get; set; }
        public string SecondVar { get; set; }
    }
    public enum ProModeLogic
    {
        equals,
        not_equals,
        larger_than,
        Smaller_than,
        larger_or_equals_to,
        smaller_or_equals_to
    }
    public enum ProModeOperators
    {
        plus,
        minus,
        times_with,
        devide_by,
        modulus,
        percentage_of

    }
    public enum ProModeLogicExtentions
    {
        and,
        or,
        xor
        
    }
}
