using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore.Strategies
{
    class Martingale: StrategyBase
    {
        #region Settings
        public bool EnableWinMaxMultiplier { get; set; }
        public double WinMaxMultiplies { get; set; }
        public double WinMultiplier { get; set; }
        public bool EnableWinDevider { get; set; }
        public int WinDevideCounter { get; set; }
        public double WinDevider { get; set; }
        public int WinDevidecounter { get; set; }
        public bool rdbWinReduce { get; set; }
        public int StretchWin { get; set; }
        public bool EnableFirstResetWin { get; set; }
        public bool EnableMK { get; set; }
        public double MinBet { get; set; }
        public double Chance { get; set; }
        public bool EnableTrazel { get; set; }
        public bool starthigh { get; set; }
        public double MKDecrement { get; set; }
        public double trazelwin { get; set; }
        public double TrazelWin { get; set; }
        public double trazelwinto { get; set; }
        public bool trazelmultiply { get; set; }
        public bool EnableChangeWinStreak { get; set; }
        public int ChangeWinStreak { get; set; }
        public double ChangeWinStreakTo { get; set; }
        public bool checkBox1 { get; set; }
        public int MutawaWins { get; set; }
        public double mutawaprev { get; set; }
        public double MutawaMultiplier { get; set; }
        public int ChangeChanceWinStreak { get; set; }
        public bool EnableChangeChanceWin { get; set; }
        public double ChangeChanceWinTo { get; set; }
        public bool rdbMaxMultiplier { get; set; }        
        public int MaxMultiplies { get; set; }
        public double Multiplier { get; set; }
        public bool rdbDevider { get; set; }
        public int Devidecounter { get; set; }
        public double Devider { get; set; }
        public bool rdbReduce { get; set; }
        public double TrazelMultiplier { get; set; }
        public int TrazelLose { get; set; }
        public double trazelloseto { get; set; }
        public int StretchLoss { get; set; }
        public bool EnableFirstResetLoss { get; set; }
        public double MKIncrement { get; set; }
        public int ChangeLoseStreak { get; set; }
        public bool EnableChangeLoseStreak { get; set; }
        public double ChangeLoseStreakTo { get; set; }
        public bool EnablePercentage { get; set; }
        public double Percentage { get; set; }
        public double BaseChance { get; set; }
        #endregion


        public override double CalculateNextBet(double Lastbet, bool Win)
        {
            int Winstreak = 0;
            if (Win)
            {
                if (EnableWinMaxMultiplier && Winstreak >= WinMaxMultiplies)
                {
                    WinMultiplier = 1;
                }
                else if (EnableWinDevider && Winstreak % WinDevideCounter == 1 && Winstreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                else if (rdbWinReduce && Winstreak == WinDevidecounter && Winstreak > 0)
                {
                    WinMultiplier *= WinDevider;
                }
                if (Winstreak % StretchWin == 0)
                    Lastbet *= WinMultiplier;
                if (Winstreak == 1)
                {
                    if (EnableFirstResetWin && !EnableMK)
                    {
                        Lastbet = MinBet;
                    }
                    try
                    {
                        UpdateChance(BaseChance);
                    }
                    catch (Exception e)
                    {
                        Logger.DumpLog(e);
                        Logger.DumpLog(e);
                    }
                }
                if (EnableTrazel)
                {

                    High = starthigh;
                }
                if (EnableMK)
                {
                    if (double.Parse((Lastbet - MKDecrement).ToString("0.00000000"), System.Globalization.CultureInfo.InvariantCulture) > 0)
                    {
                        Lastbet -= MKDecrement;
                    }
                }
                if (EnableTrazel && trazelwin % TrazelWin == 0 && trazelwin != 0)
                {
                    Lastbet = trazelwinto;
                    trazelwin = -1;
                    trazelmultiply = true;
                    High = !starthigh;
                }
                else
                {
                    if (EnableTrazel)
                    {
                        Lastbet = MinBet;
                        trazelmultiply = false;
                    }
                }


                if (EnableChangeWinStreak && (Winstreak == ChangeWinStreak))
                {
                    Lastbet = ChangeWinStreakTo;
                }
                if (checkBox1)
                {
                    if (Winstreak == MutawaWins)
                        Lastbet = mutawaprev *= MutawaMultiplier;
                    if (Winstreak == MutawaWins + 1)
                    {
                        Lastbet = MinBet;
                        mutawaprev = ChangeWinStreakTo / MutawaMultiplier;
                    }

                }
                if (EnableChangeChanceWin && (Winstreak == ChangeChanceWinStreak))
                {
                    try
                    {
                        UpdateChance(ChangeChanceWinTo);
                        
                    }
                    catch (Exception e)
                    {
                        Logger.DumpLog(e);
                    }
                }


            }
            else
            {
                //stop multiplying if at max or if it goes below 1
                if (rdbMaxMultiplier && Losestreak >= MaxMultiplies)
                {
                    Multiplier = 1;
                }
                else if (rdbDevider && Losestreak % Devidecounter == 0 && Losestreak > 0)
                {
                    Multiplier *= Devider;
                    if (Multiplier < 1)
                        Multiplier = 1;
                }
                //adjust multiplier according to devider
                else if (rdbReduce && Losestreak == Devidecounter && Losestreak > 0)
                {
                    Multiplier *= Devider;
                }
                if (EnableTrazel && trazelmultiply)
                {
                    Multiplier = TrazelMultiplier;
                }
                if (EnableTrazel)
                {
                    High = starthigh;
                }
                if (EnableTrazel && Losestreak + 1 >= TrazelLose && !trazelmultiply)
                {
                    Lastbet = trazelloseto;
                    trazelmultiply = true;
                    High = !starthigh;
                }
                if (trazelmultiply)
                {
                    trazelwin = -1;

                }
                else
                {
                    trazelwin = 0;
                }
                //set new bet size
                if (Losestreak % StretchLoss == 0)
                    Lastbet *= Multiplier;
                if (Losestreak == 1)
                {
                    if (EnableFirstResetLoss)
                    {
                        Lastbet = MinBet;
                    }
                }
                if (EnableMK)
                {
                    Lastbet += MKIncrement;
                }
                if (checkBox1)
                {
                    Lastbet = MinBet;
                }


                //change bet after a certain losing streak
                if (EnableChangeLoseStreak && (Losestreak == ChangeLoseStreak))
                {
                    Lastbet = ChangeLoseStreakTo;
                }
            }
            if (EnablePercentage)
            {
                Lastbet = (Percentage / 100.0) * Balance;
            }
            return Lastbet;
        }

        public override double RunReset()
        {
            return MinBet;
        }

        
    }
}
