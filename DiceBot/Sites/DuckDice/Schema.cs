using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBot.Schema.DuckDice
{
    public class QuackLogin
    {
        public string token { get; set; }
    }
    public class Quackbalance
    {
        public QuackStats user { get; set; }
        public string hash { get; set; }
        public string username { get; set; }
        public QuackBalValues balances { get; set; }
        public QuackStats session { get; set; }
    }
    public class QuackBalValues
    {
        public string main { get; set; }
        public string faucet { get; set; }
        public string tle { get; set; }
    }
    public class QuackStats
    {

        public QuackStats user { get; set; }
        public string hash { get; set; }
        public string username { get; set; }
        public string balance { get; set; }
        public QuackBalValues balances { get; set; }
        public QuackStats session { get; set; }
        public int bets { get; set; }
        public int wins { get; set; }
        public string volume { get; set; }
        public string profit { get; set; }

    }
    public class QuackStatsDetails
    {
        public int bets { get; set; }
        public int wins { get; set; }
        public string profit { get; set; }
        public string volume { get; set; }
    }
    public class QuackBet
    {
        public string error { get; set; }
        public QuackBet bet { get; set; }
        public QuackStats user { get; set; }
        public string hash { get; set; }
        public string symbol { get; set; }
        public bool result { get; set; }
        public bool isHigh { get; set; }
        public decimal number { get; set; }
        public decimal threshold { get; set; }
        public decimal chance { get; set; }
        public decimal payout { get; set; }
        public string betAmount { get; set; }
        public string winAmount { get; set; }
        public string profit { get; set; }
        public long nonce { get; set; }

    }
    public class QuackSeed
    {
        public QuackSeed current { get; set; }
        public string clientSeed { get; set; }
        public long nonce { get; set; }
        public string serverSeedHash { get; set; }
    }
    public class QuackWithdraw
    {
        public string error { get; set; }

    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class DDRewardConfig
    {
        public int xp { get; set; }
        public int level { get; set; }
        public string amount { get; set; }
    }

    public class DDTLEData
    {
        public string hash { get; set; }
        public string slug { get; set; }
        public string name { get; set; }
        public string gameMode { get; set; }
        public List<DDRewardConfig> rewardConfig { get; set; }
        public string minBetAmount { get; set; }
        public string symbol { get; set; }
        public string paySymbol { get; set; }
        public int defaultXpPerLevel { get; set; }
        public string defaultAmountPerLevel { get; set; }
        public string houseEdge { get; set; }
        public int endDate { get; set; }
    }

    public class DDTLE
    {
        public List<DDTLEData> data { get; set; }
    }


}

