using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceBot.Core;

namespace DiceBot.WolfBetSchema
{


    // 




    public class WolfBetLogin
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }
    public class Preferences
    {
        public bool public_total_profit { get; set; }
        public bool public_total_wagered { get; set; }
        public bool public_bets { get; set; }
    }

    public class Balance
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string withdraw_fee { get; set; }
        public string withdraw_minimum_amount { get; set; }
        public bool payment_id_required { get; set; }
    }

    public class Game2
    {
        public string name { get; set; }
    }

    public class Game
    {
        public string server_seed_hashed { get; set; }
        public Game2 game { get; set; }
    }

    public class User
    {
        public string login { get; set; }
        public string email { get; set; }
        public bool two_factor_authentication { get; set; }
        public bool has_email_to_verify { get; set; }
        public string last_nonce { get; set; }
        public string seed { get; set; }
        public string channel { get; set; }
        public string joined { get; set; }

        public List<Balance> balances { get; set; }
        public List<Game> games { get; set; }
    }



    public class History
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public int step { get; set; }
        public long published_at { get; set; }
    }

    public class Values
    {
        public string btc { get; set; }
        public string eth { get; set; }
        public string ltc { get; set; }
        public string doge { get; set; }
        public string trx { get; set; }
        public string bch { get; set; }
        public string xrp { get; set; }
        public string usdt { get; set; }
        public string etc { get; set; }
        public string sushi { get; set; }
        public string uni { get; set; }
        public string xlm { get; set; }
    }

    public class Next
    {
        public decimal step { get; set; }
        public Values values { get; set; }
    }

    public class DailyStreak
    {
        public List<History> history { get; set; }
        public Next next { get; set; }
    }

    public class WolfBetProfile
    {
        public User user { get; set; }
        public List<Balance> balances { get; set; }

    }
    public class WBStat
    {
        public string total_bets { get; set; }
        public string win { get; set; }
        public string lose { get; set; }
        public string waggered { get; set; }
        public string currency { get; set; }
        public string profit { get; set; }
    }

    public class Dice
    {
        public WBStat doge { get; set; }
        public WBStat btc { get; set; }
        public WBStat eth { get; set; }
        public WBStat ltc { get; set; }
        public WBStat trx { get; set; }
        public WBStat bch { get; set; }
        public WBStat xrp { get; set; }
        public WBStat usdt { get; set; }
        public WBStat etc { get; set; }
        public WBStat sushi { get; set; }
        public WBStat uni { get; set; }
        public WBStat xlm { get; set; }
    }

    public class WolfBetStats
    {
        public Dice dice { get; set; }
    }

    public class WolfPlaceBet
    {
        public string currency { get; set; }
        public string game { get; set; }
        public string amount { get; set; }
        public string rule { get; set; }
        public string multiplier { get; set; }
        public string bet_value { get; set; }
    }
    public class WBBet
    {
        public string hash { get; set; }
        public int nonce { get; set; }
        public string user_seed { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string profit { get; set; }
        public string multiplier { get; set; }
        public string bet_value { get; set; }
        public string result_value { get; set; }
        public string state { get; set; }
        public int published_at { get; set; }
        public string server_seed_hashed { get; set; }
        public User user { get; set; }
        public Game game { get; set; }
    }

    public class UserBalance
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string withdraw_fee { get; set; }
        public string withdraw_minimum_amount { get; set; }
        public bool payment_id_required { get; set; }
    }

    public class WolfBetResult
    {
        public WBBet bet { get; set; }
        public UserBalance userBalance { get; set; }
    }
}
