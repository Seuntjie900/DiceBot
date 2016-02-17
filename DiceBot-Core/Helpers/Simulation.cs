using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiceBot
{
    class Simulation
    {
        public string serverseed { get; set; }
        public string clientseed { get; set; }
        public List<string> bets = new List<string>();

        public Simulation(string balance, string bets, string server, string client)
        {
            string siminfo = "Dice Bot Simulation,,Starting Balance,Amount of bets, Server seed,,,Client Seed";
            string result = ",," + balance + "," + bets + "," + server + ",,," + clientseed;
            string columns = "Bet Number,LuckyNumber,Chance,Roll,Result,Wagered,Profit,Balance,Total Profit";
            this.bets.Add(siminfo);
            this.bets.Add(result);
            this.bets.Add("");
            this.bets.Add(columns);
        }
    }
}
