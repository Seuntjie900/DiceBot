﻿using System;

namespace DiceBot
{
    public class Bet
    {
        public string Guid { get; set; }
        public string Currency { get; set; }
        public string Id { get; set; }
        private decimal chance = 0;
        public decimal Chance { get { return chance; } set { if (value > 100) chance = value / 10000; else chance = value; } }
        private string betdate = "";
        public string BetDate
        {
            get
            {
                return betdate;
            }
            set
            {
                string tmp = value;
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
            get
            {
                return json.ToDateTime2(BetDate);
            }
            set
            {
                DateTime s = value;
                BetDate = json.ToDateString(s);
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
            get { return BetType == 0; }
            set
            {
                BetType = value ? 0 : 1;
            }
        }
        public Bet()
        {
            serverhash = serverseed = clientseed = "";
            Id = "-1";
            Roll = -1;
            nonce = -1;
        }
        public bool Verified { get; set; }
        public int UserAccountId { get { return uid; } set { uid = value; } }

        public bool CheckWin(decimal maxroll)
        {
            if ((Roll > maxroll - Chance && high) || (Roll < Chance && !high))
            {
                return true;
            }
            return false;
        }
    }

}

