using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore
{
    public class Bet
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public long ID { get; set; }
        public double Profit { get; set; }
        public double Roll { get; set; }
        public bool High { get; set; }
        public double Chance { get; set; }
        public long Nonce { get; set; }
        public string ServerHash { get; set; }
        public string ServerSeed { get; set; }
        public string ClientSeed { get; set; }
        public long Uid { get; set; }
        public string Currency { get; set; }
        
    }
}
