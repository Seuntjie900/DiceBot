using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore.Strategies
{
    class Labouchere: StrategyBase
    {
        List<double> OrigList = new List<double>();
        List<double> LabList = new List<double>();
        public override double CalculateNextBet(double Lastbet, bool Win)
        {
            if (Win)
            {
                if (rdbLabEnable)
                {
                    if (chkReverseLab)
                    {
                        if (LabList.Count == 1)
                            LabList.Add(LabList[0]);
                        else
                            LabList.Add(LabList[0] + LabList[LabList.Count - 1]);
                    }
                    else if (LabList.Count > 1)
                    {
                        LabList.RemoveAt(0);
                        LabList.RemoveAt(LabList.Count - 1);
                        if (LabList.Count == 0)
                        {
                            if (rdbLabStop)
                            {
                                CallStop("End of labouchere list reached");

                            }
                            else
                            {
                                RunReset();
                            }
                        }

                    }
                    else
                    {
                        if (rdbLabStop)
                        {
                            CallStop("End of labouchere list reached");

                        }
                        else
                        {
                            LabList = OrigList.ToArray().ToList<double>();
                            if (LabList.Count == 1)
                                Lastbet = LabList[0];
                            else if (LabList.Count > 1)
                                Lastbet = LabList[0] + LabList[LabList.Count - 1];
                        }
                    }
                }


            }
            else
            {
                //do laboucghere logic
                if (rdbLabEnable)
                {
                    if (!chkReverseLab)
                    {
                        if (LabList.Count == 1)
                            LabList.Add(LabList[0]);
                        else
                            LabList.Add(LabList[0] + LabList[LabList.Count - 1]);
                    }
                    else
                    {
                        if (LabList.Count > 1)
                        {
                            LabList.RemoveAt(0);
                            LabList.RemoveAt(LabList.Count - 1);
                            if (LabList.Count == 0)
                            {
                                CallStop("Stopping: End of labouchere list reached.");

                            }
                        }
                        else
                        {
                            if (rdbLabStop)
                            {
                                CallStop("Stopping: End of labouchere list reached.");

                            }
                            else
                            {
                                LabList = OrigList.ToArray().ToList<double>();
                                if (LabList.Count == 1)
                                    Lastbet = LabList[0];
                                else if (LabList.Count > 1)
                                    Lastbet = LabList[0] + LabList[LabList.Count - 1];
                            }
                        }
                    }
                }


                //end labouchere logic
            }

            if (LabList.Count == 1)
                Lastbet = LabList[0];
            else if (LabList.Count > 1)
                Lastbet = LabList[0] + LabList[LabList.Count - 1];
            else
            {
                if (rdbLabStop)
                {
                    CallStop("Stopping: End of labouchere list reached.");

                }
                else
                {
                    LabList = OrigList.ToArray().ToList<double>();
                    if (LabList.Count == 1)
                        Lastbet = LabList[0];
                    else if (LabList.Count > 1)
                        Lastbet = LabList[0] + LabList[LabList.Count - 1];
                }
            }
            return Lastbet;
        }

        public override double RunReset()
        {
            
            LabList = OrigList.ToArray().ToList<double>();
            if (LabList.Count == 1)
                return LabList[0];
            else if (LabList.Count > 1)
                return LabList[0] + LabList[LabList.Count - 1];
            return 0;
        }

        public bool rdbLabEnable { get; set; }

        public bool chkReverseLab { get; set; }

        public bool rdbLabStop { get; set; }
    }
}
