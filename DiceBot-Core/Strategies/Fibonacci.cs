using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore.Strategies
{
    class Fibonacci: StrategyBase
    {
        public double minbet { get; set; }
        public override double CalculateNextBet(double Lastbet, bool Win)
        {
            if (Win)
            {
                if (EnableFiboWinIncrement)
                {
                    FibonacciLevel += FiboWinIncrement;
                }
                else if (EnableFiboWinReset)
                {
                    FibonacciLevel = 0;
                }
                else
                {
                    FibonacciLevel = 0;
                    CallStop("Fibonacci bet won.");

                }
            }
            else
            {
                if (EnableFiboLossIncrement)
                {
                    FibonacciLevel += FiboLossIncrement;
                }
                else if (EnableFiboLossReset)
                {
                    FibonacciLevel = 0;
                }
                else
                {
                    FibonacciLevel = 0;
                    CallStop("Fibonacci bet lost.");

                }
            }
            if (FibonacciLevel < 0)
                FibonacciLevel = 0;

            if (FibonacciLevel >= FiboLeve & EnableFiboLevel)
            {
                if (EnableFiboLevelReset)
                    FibonacciLevel = 0;
                else
                {

                    FibonacciLevel = 0;
                    CallStop("Fibonacci level " + FiboLeve + ".");

                }
            }
            return CalculateFibonacci(FibonacciLevel);
        }

        public override double RunReset()
        {
            FibonacciLevel = 1;
            return CalculateFibonacci(FibonacciLevel);
        }

        double CalculateFibonacci(int n)
        {
            int x = (int)((1.0 / (Math.Sqrt(5.0))) * (Math.Pow((1.0 + Math.Sqrt(5.0)) / 2.0, n) - Math.Pow((1.0 + Math.Sqrt(5.0)) / 2.0, n)));
            return minbet * (double)(x);
        }

        public bool EnableFiboWinIncrement { get; set; }

        public int FibonacciLevel { get; set; }

        public int FiboWinIncrement { get; set; }

        public bool EnableFiboWinReset { get; set; }

        public bool EnableFiboLossIncrement { get; set; }

        public int FiboLossIncrement { get; set; }

        public bool EnableFiboLossReset { get; set; }

        public int FiboLeve { get; set; }

        public bool EnableFiboLevel { get; set; }

        public bool EnableFiboLevelReset { get; set; }
    }
}
