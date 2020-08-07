using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Threading;
using DiceBot.Sites;

namespace DiceBot.Common
{
    internal class SQLiteHelper : IDisposable
    {
        private static readonly string constring = "Data Source=DiceBot.db;Version=3;New=False;Compress=True;datetimeformat=CurrentCulture";
        private static SQLiteConnection Conn;

        public void Dispose()
        {
            Conn.Close();
        }

        private static SQLiteConnection GetConnection()
        {
            if (Conn == null)
            {
                Conn = new SQLiteConnection(constring);
                Conn.Open();
            }
            else if (Conn.State == ConnectionState.Broken || Conn.State == ConnectionState.Closed)
            {
                Conn = new SQLiteConnection(constring);
                Conn.Open();
            }

            return Conn;
        }

        public static void CheckDBS()
        {
            var sqcon = GetConnection();
            if (File.Exists("DiceBot.db")) sqcon = new SQLiteConnection("Data Source=DiceBot.db;Version=3;New=True;Compress=True;");

            sqcon.Open();

            var seeds = "CREATE TABLE if not exists seed(	hash nvarchar(128) NOT NULL primary key,	server text NOT NULL) ";

            var bets =
                "CREATE TABLE if not exists bet(betid nvarchar(50) NOT NULL ,date datetime NULL,stake decimal(18, 8) NULL,profit decimal(18, 8) NULL,chance decimal(18, 8) NULL,	high smallint NULL,	lucky decimal(18, 8) NULL,	hash nvarchar(128) NULL,	nonce bigint NULL,	uid int NULL,	Client nvarchar(50) NULL, site nvarchar(20), PRIMARY KEY(betid, site) )";

            //string[] sites = { "PRCDice", "JustDice", "PrimeDice","Dice999","SAfEDICE" };

            var Command = new SQLiteCommand("", sqcon);
            Command.CommandText = seeds;
            Command.ExecuteNonQuery();
            Command.CommandText = bets;
            Command.ExecuteNonQuery();

            ////sqcon.close();
        }

        private static void AddBet(object sqlBetObj)
        {
            try
            {
                var sqcon = GetConnection();
                var Command = new SQLiteCommand("", sqcon);
                var curbet = (sqlBetObj as sqbet)._Bet;
                var sitename = (sqlBetObj as sqbet).SiteName;

                try
                {
                    Command.CommandText = string.Format(NumberFormatInfo.InvariantInfo, "insert into seed(hash,server) values('{0}','{1}')", curbet.serverhash,
                                                        curbet.serverseed, sitename);

                    Command.ExecuteNonQuery();
                }
                catch
                {
                }

                Command.CommandText = string.Format(NumberFormatInfo.InvariantInfo,
                                                    "insert into bet(betid, date,stake,profit,chance,high,lucky,hash,nonce,uid,client,site) values('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{0}')",
                                                    sitename,
                                                    curbet.Id,
                                                    curbet.date.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    curbet.Amount,
                                                    curbet.Profit,
                                                    curbet.Chance,
                                                    curbet.high ? "1" : "0",
                                                    curbet.Roll,
                                                    curbet.serverhash,
                                                    curbet.nonce,
                                                    curbet.uid, curbet.clientseed);

                Command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
        }

        public static void AddBet(Bet curbet, string sitename)
        {
            new Thread(AddBet).Start(new sqbet {_Bet = curbet, SiteName = sitename});
            /*SQLiteConnection sqcon = GetConnection();
            try
            {
                sqcon.Open();
                SQLiteCommand Command = new SQLiteCommand("", sqcon);
                try
                {
                    Command.CommandText = string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"insert into seed(hash,server) values('{0}','{1}')", curbet.serverhash, curbet.serverseed, sitename);
                    Command.ExecuteNonQuery();
                }
                catch
                {
                    

                }
                Command.CommandText = string.Format( System.Globalization.NumberFormatInfo.InvariantInfo,"insert into bet(betid, date,stake,profit,chance,high,lucky,hash,nonce,uid,client,site) values('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{0}')",
                    sitename,
                    curbet.Id,
                    curbet.date,
                    curbet.Amount,
                    curbet.Profit,
                    curbet.Chance,
                    curbet.high?"1":"0",
                    curbet.Roll,
                    curbet.serverhash,
                    curbet.nonce,
                    curbet.uid,curbet.clientseed);
                Command.ExecuteNonQuery();
            }
            catch
            {


            }
            //sqcon.close();
            */
        }

        private static Bet BetParser(SQLiteDataReader Reader)
        {
            var site = "";
            var tmp = new Bet();

            for (var i = 0; i < Reader.FieldCount; i++)
            {
                switch (Reader.GetName(i))
                {
                    case "betid":
                        tmp.Id = (string) Reader[i];

                        break;
                    case "date":
                        tmp.date = (DateTime) Reader[i];

                        break;
                    case "stake":
                        tmp.Amount = (decimal) Reader[i];

                        break;
                    case "profit":
                        tmp.Profit = (decimal) Reader[i];

                        break;
                    case "chance":
                        tmp.Chance = (decimal) Reader[i];

                        break;
                    case "high":
                        tmp.high = (short) Reader[i] == 1;

                        break;
                    case "lucky":
                        tmp.Roll = (decimal) Reader[i];

                        break;
                    case "hash":
                        tmp.serverhash = (string) Reader[i];

                        break;
                    case "nonce":
                        tmp.nonce = (long) Reader[i];

                        break;
                    case "uid":
                        tmp.uid = (int) Reader[i];

                        break;
                    case "Client":
                        tmp.clientseed = (string) Reader[i];

                        break;
                    case "server":
                        tmp.serverseed = (string) Reader[i];

                        break;
                    case "site":
                        site = (string) Reader[i];

                        break;
                }
            }

            if (!string.IsNullOrEmpty(tmp.serverseed) && !string.IsNullOrEmpty(tmp.clientseed) && tmp.Roll != -1 && site != "")
                switch (site)
                {
                    case "JustDice":
                        tmp.Verified = tmp.Roll == DiceSite.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "PrimeDice":
                        tmp.Verified = tmp.Roll == PD.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "999Dice":
                        tmp.Verified = tmp.Roll == Dice999.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce, /*(long)(tmp.Roll*10000m),*/ tmp.serverhash);

                        break;
                    case "SafeDice":
                        tmp.Verified = tmp.Roll == SafeDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "BetKing":
                        tmp.Verified = tmp.Roll == BetKing.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "BitDice":
                        tmp.Verified = tmp.Roll == BitDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "FortuneJack":
                        tmp.Verified = tmp.Roll == FortuneJack.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "CryptoGames":
                        tmp.Verified = tmp.Roll == (decimal) CryptoGames.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "Bitsler":
                        tmp.Verified = tmp.Roll == Bitsler.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "SatoshiDice":
                    case "MegaDice":
                        tmp.Verified = tmp.Roll == SatoshiDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "Bitvest":
                        tmp.Verified = tmp.Roll == Bitvest.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "KingDice":
                        tmp.Verified = tmp.Roll == KingDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "YoloDice":
                        tmp.Verified = tmp.Roll == YoloDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "DuckDice":
                        tmp.Verified = tmp.Roll == DuckDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "NitroDice":
                        tmp.Verified = tmp.Roll == NitroDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "WolfBet":
                        tmp.Verified = tmp.Roll == WolfBet.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "WinDice":
                        tmp.Verified = tmp.Roll == WinDice.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "Bit-Exo":
                        tmp.Verified = tmp.Roll == BitExo.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "FreeBitcoin":
                        tmp.Verified = tmp.Roll == FreeBitcoin.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "Nitrogen Sports":
                        tmp.Verified = tmp.Roll == NitrogenSports.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                    case "Stake":
                        tmp.Verified = tmp.Roll == Stake.sGetLucky(tmp.serverseed, tmp.clientseed, tmp.nonce);

                        break;
                }

            return tmp;
        }

        public static Bet[] GetBetForCharts(string site)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    //      sqcon.Open();
                    var Command = new SQLiteCommand("select betid, profit, stake from bet", sqcon);
                    if (site != "") Command.CommandText += " where site = '" + site + "'";
                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    ////sqcon.close();
                    return Bets.ToArray();
                }
                catch
                {
                }

                ////sqcon.close();
            }

            return null;
        }

        public static Bet[] GetBetForCharts(string site, long startID)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    //      sqcon.Open();
                    var Command = new SQLiteCommand("select betid, profit, stake from bet where betid>" + startID, sqcon);
                    if (site != "") Command.CommandText += " and site = '" + site + "'";
                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    ////sqcon.close();
                    return Bets.ToArray();
                }
                catch
                {
                }

                ////sqcon.close();
            }

            return null;
        }

        public static Bet[] GetBetForCharts(string site, DateTime StartDate, DateTime EndDate)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    //  sqcon.Open();
                    var Command =
                        new
                            SQLiteCommand("select betid, profit, stake from bet where date>='" + StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and date <= '" + EndDate.ToString("yyyy-MM-dd HH:mm:ss") + "'",
                                          sqcon);

                    if (site != "") Command.CommandText += " and site = '" + site + "'";
                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    ////sqcon.close();
                    return Bets.ToArray();
                }
                catch
                {
                }

                ////sqcon.close();
            }

            return null;
        }

        public static Bet[] GetBetHistory(string site)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    //  sqcon.Open();
                    var Command = new SQLiteCommand("select bet.*,seed.server from bet, seed where bet.hash=seed.hash ", sqcon);
                    if (site != "") Command.CommandText += " and site = '" + site + "'";
                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    ////sqcon.close();
                    return Bets.ToArray();
                }
                catch (Exception e)
                {
                }

                ////sqcon.close();
            }

            return null;
        }

        public static Bet[] GetBetHistory(string site, DateTime StartDate, DateTime EndDate)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    //  sqcon.Open();
                    var Command =
                        new
                            SQLiteCommand("select bet.*, seed.server from bet, seed where bet.hash=seed.hash and date>='" + StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and date<='" + EndDate.ToString("yyyy-MM-dd HH:mm:ss") + "' ",
                                          sqcon);

                    if (site != "") Command.CommandText += " and site = '" + site + "'";
                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    ////sqcon.close();
                    return Bets.ToArray();
                }
                catch
                {
                }

                ////sqcon.close();
            }

            return null;
        }

        public static void InsertSeed(string hash, string Seed)
        {
            if (Seed != null && hash != null)

            {
                var sqcon = GetConnection();

                try
                {
                    //  sqcon.Open();

                    var Command = new SQLiteCommand("update seed set server ='" + Seed + "' where hash='" + hash + "'", sqcon);
                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    ////sqcon.close();
                }
                catch
                {
                }

                ////sqcon.close();
            }
        }

        public static Bet[] Search(bool betid, bool chance, bool stake, bool roll, bool profit, bool client, bool server, bool hash, int high, int verified,
                                   string Query, string SiteName)
        {
            Query = Query.Replace("'", "\"");
            var searchSring = "select bet.*,seed.server from bet, seed where bet.hash=seed.hash ";
            if (SiteName != "") searchSring += " and site = '" + SiteName + "'";
            searchSring += " and (";

            if (betid)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " betid like '" + Query + "'";
            }

            if (chance)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " chance like '" + Query + "'";
            }

            if (stake)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " stake like '" + Query + "'";
            }

            if (roll)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " lucky like '" + Query + "'";
            }

            if (profit)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " profit like '" + Query + "'";
            }

            if (client)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " client like '" + Query + "'";
            }

            if (server)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " server like '" + Query + "'";
            }

            if (hash)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " hash like '" + Query + "'";
            }

            searchSring += " )";

            if (high == 1)
                searchSring += " and high = 1";
            else if (high == 2) searchSring += " and high = 0";

            //using 
            var sqcon = GetConnection();

            {
                try
                {
                    //  sqcon.Open();
                    var Command = new SQLiteCommand(searchSring, sqcon);

                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        var b = BetParser(Reader);
                        if (b.Verified && verified == 1 || !b.Verified && verified == 2 || verified == 3) Bets.Add(b);
                    }

                    ////sqcon.close();
                    return Bets.ToArray();
                }
                catch
                {
                }

                ////sqcon.close();
            }

            return null;
        }

        public static Bet[] Search(bool betid, bool chance, bool stake, bool roll, bool profit, bool client, bool server, bool hash, int high, int verified,
                                   string Query, string SiteName, DateTime Start, DateTime End)
        {
            Query = Query.Replace("'", "\"");
            var searchSring = "select bet.*,seed.server from bet, seed where bet.hash=seed.hash ";
            if (SiteName != "") searchSring += " and site = '" + SiteName + "'";
            searchSring += " (";

            if (betid)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " betid like '" + Query + "'";
            }

            if (chance)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " chance like '" + Query + "'";
            }

            if (stake)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " stake like '" + Query + "'";
            }

            if (roll)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " lucky like '" + Query + "'";
            }

            if (profit)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " profit like '" + Query + "'";
            }

            if (client)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " client like '" + Query + "'";
            }

            if (server)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " server like '" + Query + "'";
            }

            if (hash)
            {
                if (!searchSring.EndsWith("(")) searchSring += " or";
                searchSring += " hash like '" + Query + "'";
            }

            searchSring += " )";

            if (high == 1)
                searchSring += " and high = 1";
            else if (high == 2) searchSring += " and high = 0";

            searchSring += "and date >= '" + Start.ToShortDateString() + "'";
            searchSring += "and date <= '" + End.ToShortDateString() + "'";

            //ing (
            var sqcon = GetConnection();

            {
                try
                {
                    //sqcon.Open();
                    var Command = new SQLiteCommand(searchSring, sqcon);

                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        var b = BetParser(Reader);
                        if (b.Verified && verified == 1 || !b.Verified && verified == 2 || verified == 3) Bets.Add(b);
                    }

                    //sqcon.close();
                    return Bets.ToArray();
                }
                catch
                {
                }

                //sqcon.close();
            }

            return null;
        }

        public static Bet[] GetHistoryByQuery(string Query)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    //sqcon.Open();
                    var Command = new SQLiteCommand(Query, sqcon);

                    var Reader = Command.ExecuteReader();
                    var Bets = new List<Bet>();

                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }

                    //sqcon.close();

                    return Bets.ToArray();
                }
                catch
                {
                }

                //sqcon.close();
            }

            return null;
        }

        public static List<long> GetMissingSeedIDs(string site)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    var Bets = new List<long>();

                    //sqcon.Open();
                    var Command = new SQLiteCommand("select hash from seed where server = ''", sqcon);
                    var Reader = Command.ExecuteReader();

                    var hashes = new List<string>();

                    while (Reader.Read())
                    {
                        hashes.Add((string) Reader["hash"]);
                    }

                    foreach (var s in hashes)
                    {
                        Command = new SQLiteCommand("select min(betid) as betid from bet where hash = '" + s + "' and site ='" + site + "'", sqcon);

                        if (Reader.Read()) Bets.Add((long) Reader["betid"]);
                    }

                    //sqcon.close();
                    return Bets;
                }
                catch
                {
                }

                //sqcon.close();
            }

            return null;
        }

        public static string GetHashForBet(string site, long betid)
        {
            //using (
            var sqcon = GetConnection();

            {
                try
                {
                    // sqcon.Open();
                    var Command = new SQLiteCommand("select hash from bet where betid=" + betid + " and site='" + site + "'", sqcon);
                    var Reader = Command.ExecuteReader();
                    var hash = "";
                    if (Reader.Read()) hash = (string) Reader["hash"];

                    //sqcon.close();
                    return hash;
                }
                catch
                {
                }

                //sqcon.close();
            }

            return null;
        }
    }

    public class sqbet
    {
        public Bet _Bet { get; set; }
        public string SiteName { get; set; }
    }
}
