using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
namespace DiceBot
{
    class sqlite_helper
    {
        static string constring = "Data Source=DiceBot.db;Version=3;New=False;Compress=True;";
        
        static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(constring);
        }
        SQLiteConnection sqcon;
        /*public sqlite_helper()
        {
            if (File.Exists("DiceBot.db"))
            {
                sqcon = new SQLiteConnection("Data Source=DiceBot.db;Version=3;New=True;Compress=True;");
            }
            else
            {
                sqcon = new SQLiteConnection("Data Source=DiceBot.db;Version=3;New=False;Compress=True;");
            }
            sqcon.Open();
            CheckDBS();
        }*/

        public static void CheckDBS()
        {
            SQLiteConnection sqcon = GetConnection();
            if (File.Exists("DiceBot.db"))
            {
                sqcon = new SQLiteConnection("Data Source=DiceBot.db;Version=3;New=True;Compress=True;");
            }
            
            sqcon.Open();

            string seeds = "CREATE TABLE if not exists seed(	hash nvarchar(128) NOT NULL primary key,	server text NOT NULL) ";
            string bets = "CREATE TABLE if not exists bet(betid bigint NOT NULL primary key,date datetime NULL,stake decimal(18, 8) NULL,profit decimal(18, 8) NULL,chance decimal(18, 8) NULL,	high bit NULL,	lucky decimal(18, 8) NULL,	hash nvarchar(128) NULL,	nonce bigint NULL,	uid int NULL,	Client nvarchar(50) NULL, site nvarchar(20) )";
            
            //string[] sites = { "PRCDice", "JustDice", "PrimeDice","Dice999","SAfEDICE" };
            
                SQLiteCommand Command = new SQLiteCommand("", sqcon);
                Command.CommandText = seeds;
                Command.ExecuteNonQuery();
                Command.CommandText = bets;
                Command.ExecuteNonQuery();
                 
            
            sqcon.Clone();
        }

        public static void AddBet(Bet curbet, string sitename)
        {
            SQLiteConnection sqcon = GetConnection();
            try
            {
                sqcon.Open();
                SQLiteCommand Command = new SQLiteCommand("", sqcon);
                try
                {
                    Command.CommandText = string.Format("insert into seed(hash,server) values('{0}','{1}')", curbet.serverhash, curbet.serverseed, sitename);
                    Command.ExecuteNonQuery();
                }
                catch
                {

                }
                Command.CommandText = string.Format("insert into bet(betid, date,stake,profit,chance,high,lucky,hash,nonce,uid,client,site) values('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{0}')",
                    sitename,
                    curbet.Id,
                    curbet.date,
                    curbet.Amount,
                    curbet.Profit,
                    curbet.Chance,
                    curbet.high,
                    curbet.Roll,
                    curbet.serverhash,
                    curbet.nonce,
                    curbet.uid,curbet.clientseed);
                Command.ExecuteNonQuery();
            }
            catch
            {


            }
            sqcon.Clone();
            
        }

        static Bet BetParser(SQLiteDataReader Reader)
        {
            Bet tmp = new Bet();
            for (int i = 0; i< Reader.FieldCount; i++)
            {
                switch (Reader.GetName(i))
                {
                    case "betid": tmp.Id = (long)Reader[i]; break;
                    case "date": tmp.date = (DateTime)Reader[i]; break;
                    case "stake": tmp.Amount = (decimal)Reader[i]; break;
                    case "profit": tmp.Profit = (decimal)Reader[i]; break;
                    case "chance": tmp.Chance = (decimal)Reader[i]; break;
                    case "high": tmp.high = (bool)Reader[i]; break;
                    case "lucky": tmp.Roll = (decimal)Reader[i]; break;
                    case "hash": tmp.serverhash = (string)Reader[i]; break;
                    case "nonce": tmp.nonce = (long)Reader[i]; break;
                    case "uid": tmp.uid = (int)Reader[i]; break;
                    case "Client": tmp.clientseed = (string)Reader[i]; break;                    
                    case "server": tmp.serverseed = (string)Reader[i]; break;
                }
            }
            return tmp;
        }

        public static Bet[] GetBetForCharts(string site)
        {
            using (SQLiteConnection sqcon = GetConnection())
            {
                
                try
                {
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select betid, profit, stake from bet", sqcon);
                    if (site!="")
                    {
                        Command.CommandText += " where site = '" + site+"'";
                    }
                    SQLiteDataReader Reader = Command.ExecuteReader();
                    List<Bet> Bets = new List<Bet>();
                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }
                    sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                { 
                }
            sqcon.Close();
            }
            return null;
        }
        
        public static Bet[] GetBetForCharts(string site, long startID)
        {
            using (SQLiteConnection sqcon = GetConnection())
            {

                try
                {
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select betid, profit, stake from bet where betid>"+startID, sqcon);
                    if (site != "")
                    {
                        Command.CommandText += " and site = '" + site + "'";
                    }
                    SQLiteDataReader Reader = Command.ExecuteReader();
                    List<Bet> Bets = new List<Bet>();
                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }
                    sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                {
                }
                sqcon.Close();
            }
            return null;
        }

        public static Bet[] GetBetForCharts(string site, DateTime StartDate, DateTime EndDate)
        {
            using (SQLiteConnection sqcon = GetConnection())
            {

                try
                {
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select betid, profit, stake from bet where date>='" + StartDate + "' and date <= '" + EndDate + "'", sqcon);
                    if (site != "")
                    {
                        Command.CommandText += " and site = '" + site + "'";
                    }
                    SQLiteDataReader Reader = Command.ExecuteReader();
                    List<Bet> Bets = new List<Bet>();
                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }
                    sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                {
                }
                sqcon.Close();
            }
            return null;
        }
        
        public static Bet[] GetBetForVerify(string site)
        {
            throw new NotImplementedException();
        }

        public static Bet[] GetBetForVerify(string site, string seed)
        {
            throw new NotImplementedException();
        }

        public static Bet[] GetBetForVerify(string site, long starID)
        {
            throw new NotImplementedException();
        }

        public static Bet[] GetBetForVerify(string site, DateTime StartDate, DateTime EndDate)
        {
            throw new NotImplementedException();
        }
    }

    
}
