using System.Collections.Generic;

namespace DiceBot.Common
{
    internal class Simulation
    {
        public List<string> bets = new List<string>();
        public string serverseed { get; set; }
        public string clientseed { get; set; }

        public Simulation(string balance, string bets, string server, string client)
        {
            var siminfo = "Dice Bot Simulation,,Starting Balance,Amount of bets, Server seed,,,Client Seed";
            var result = ",," + balance + "," + bets + "," + server + ",,," + clientseed;
            var columns = "Bet Number,LuckyNumber,Chance,Roll,Result,Wagered,Profit,Balance,Total Profit";
            this.bets.Add(siminfo);
            this.bets.Add(result);
            this.bets.Add("");
            this.bets.Add(columns);
        }
    }
}
