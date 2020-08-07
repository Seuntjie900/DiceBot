using System;

namespace DiceBot.Common
{
    public class Bet
    {
        private string betdate = "";
        private decimal chance;
        public string Guid { get; set; }
        public string Currency { get; set; }
        public string Id { get; set; }

        public decimal Chance
        {
            get => chance;
            set
            {
                if (value > 100) chance = value / 10000;
                else chance = value;
            }
        }

        public string BetDate
        {
            get => betdate;
            set
            {
                var tmp = value;

                if (tmp.Contains("("))
                {
                    tmp = tmp.Substring(tmp.IndexOf("(") + 1);
                    tmp = tmp.Substring(0, tmp.Length - 2);
                }

                betdate = tmp;
            }
        }

        public bool PlayerWin { get; set; }

        public DateTime date
        {
            get => JsonUtils.ToDateTime2(BetDate);
            set
            {
                var s = value;
                BetDate = JsonUtils.ToDateString(s);
            }
        }

        public int BetType { get; set; }
        public decimal Roll { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public long nonce { get; set; }
        public string serverhash { get; set; }
        public string serverseed { get; set; }
        public string clientseed { get; set; }
        public decimal Profit { get; set; }
        public int uid { get; set; }

        public bool high
        {
            get => BetType == 0;
            set => BetType = value ? 0 : 1;
        }

        public bool Verified { get; set; }

        public int UserAccountId
        {
            get => uid;
            set => uid = value;
        }

        public Bet()
        {
            serverhash = serverseed = clientseed = "";
            Id = "-1";
            Roll = -1;
            nonce = -1;
        }

        public bool CheckWin(decimal maxroll)
        {
            if (Roll > maxroll - Chance && high || Roll < Chance && !high) return true;

            return false;
        }
    }
}
