using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
namespace DiceBot
{
    class sqlite_helper: IDisposable
    {
        static string constring = "Data Source=DiceBot.db;Version=3;New=False;Compress=True;datetimeformat=CurrentCulture";
        static SQLiteConnection Conn = null;
        static SQLiteConnection GetConnection()
        {
            if (Conn==null)
            {
                Conn = new SQLiteConnection(constring);
                Conn.Open();
            }
            else if (Conn.State == System.Data.ConnectionState.Broken || Conn.State == System.Data.ConnectionState.Closed)
            {
                Conn = new SQLiteConnection(constring);
                Conn.Open();
            }
            return Conn;
        }
        
        public static void CheckDBS()
        {
            SQLiteConnection sqcon = GetConnection();
            if (File.Exists("DiceBot.db"))
            {
                sqcon = new SQLiteConnection("Data Source=DiceBot.db;Version=3;New=True;Compress=True;");
            }
            
            sqcon.Open();

            string seeds = "CREATE TABLE if not exists seed(	hash nvarchar(128) NOT NULL primary key,	server text NOT NULL) ";
            string bets = "CREATE TABLE if not exists bet(betid bigint NOT NULL ,date datetime NULL,stake decimal(18, 8) NULL,profit decimal(18, 8) NULL,chance decimal(18, 8) NULL,	high smallint NULL,	lucky decimal(18, 8) NULL,	hash nvarchar(128) NULL,	nonce bigint NULL,	uid int NULL,	Client nvarchar(50) NULL, site nvarchar(20), PRIMARY KEY(betid, site) )";
            
            //string[] sites = { "PRCDice", "JustDice", "PrimeDice","Dice999","SAfEDICE" };
            
                SQLiteCommand Command = new SQLiteCommand("", sqcon);
                Command.CommandText = seeds;
                Command.ExecuteNonQuery();
                Command.CommandText = bets;
                Command.ExecuteNonQuery();
                 
            
            sqcon.Close();
        }

        static void AddBet(object sqlBetObj)
        {
            try
            {
                SQLiteConnection sqcon = GetConnection();
                SQLiteCommand Command = new SQLiteCommand("", sqcon);
                Bet curbet = (sqlBetObj as sqbet)._Bet;
                string sitename = (sqlBetObj as sqbet).SiteName;
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
            new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(AddBet)).Start((new sqbet { _Bet = curbet, SiteName = sitename }));
            return;
            /*SQLiteConnection sqcon = GetConnection();
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
            sqcon.Close();
            */
        }

        static Bet BetParser(SQLiteDataReader Reader)
        {
            string site = "";
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
                    case "high": tmp.high = (short)Reader[i] == 1; break;
                    case "lucky": tmp.Roll = (decimal)Reader[i]; break;
                    case "hash": tmp.serverhash = (string)Reader[i]; break;
                    case "nonce": tmp.nonce = (long)Reader[i]; break;
                    case "uid": tmp.uid = (int)Reader[i]; break;
                    case "Client": tmp.clientseed = (string)Reader[i]; break;                    
                    case "server": tmp.serverseed = (string)Reader[i]; break;
                    case "site": site = (string)Reader[i]; break;
                }
            }
            if (!string.IsNullOrEmpty(tmp.serverseed) && !string.IsNullOrEmpty(tmp.clientseed) && tmp.Roll!=-1 && site!="")
            {
                switch (site)
                {
                    case "JustDice": tmp.Verified = tmp.Roll == (decimal)DiceSite.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "PrimeDice": tmp.Verified = tmp.Roll == (decimal)PD.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "999Dice": tmp.Verified = tmp.Roll== (decimal)dice999.sGetLucky(tmp.serverseed, (tmp.clientseed), (int)tmp.nonce, /*(long)(tmp.Roll*10000m),*/ tmp.serverhash); break;
                    case "SafeDice": tmp.Verified = tmp.Roll == (decimal)SafeDice.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "PRCDice": tmp.Verified = tmp.Roll == (decimal)PRC.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "RollinIO": tmp.Verified = tmp.Roll == (decimal)rollin.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "BitDice": tmp.Verified = tmp.Roll == (decimal)bitdice.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "BetterBets": tmp.Verified = tmp.Roll == (decimal)BB.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "MoneyPot": tmp.Verified = tmp.Roll == (decimal)moneypot.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "MoneroDice": tmp.Verified = tmp.Roll == (decimal)MoneroDice.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "FortuneJack": tmp.Verified = tmp.Roll == (decimal)FortuneJack.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "Coinichiwa": tmp.Verified = tmp.Roll == (decimal)Coinichiwa.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "CoinMillions": tmp.Verified = tmp.Roll == (decimal)CoinMillions.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "CryptoGames": tmp.Verified = tmp.Roll == (decimal)cryptogames.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "Bitsler": tmp.Verified = tmp.Roll == (decimal)Bitsler.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                    case "Wealthydice": tmp.Verified = tmp.Roll == (decimal)WD.sGetLucky(tmp.serverseed, tmp.clientseed,(int)tmp.nonce);break;
                    case "SatoshiDice": tmp.Verified = tmp.Roll == (decimal)SatoshiDice.sGetLucky(tmp.serverseed, tmp.clientseed, (int)tmp.nonce); break;
                }
            }
            return tmp;
        }

        public static Bet[] GetBetForCharts(string site)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {
                
                try
                {
              //      sqcon.Open();
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
            //sqcon.Close();
            }
            return null;
        }
        
        public static Bet[] GetBetForCharts(string site, long startID)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
              //      sqcon.Open();
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
                //sqcon.Close();
            }
            return null;
        }

        public static Bet[] GetBetForCharts(string site, DateTime StartDate, DateTime EndDate)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                  //  sqcon.Open();
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
                    //sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                {
                }
                //sqcon.Close();
            }
            return null;
        }

        
        public static Bet[] GetBetHistory(string site)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                  //  sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select bet.*,seed.server from bet, seed where bet.hash=seed.hash ", sqcon);
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
                    //sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                {
                }
                //sqcon.Close();
            }
            return null;
        }

        public static Bet[] GetBetHistory(string site, DateTime StartDate, DateTime EndDate)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                  //  sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select bet.*, seed.server from bet, seed where bet.hash=seed.hash and date>='" + StartDate + "' and date<='" + EndDate + "' ", sqcon);
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
                    //sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                {
                }
                //sqcon.Close();
            }
            return null;
        }

        public static void InsertSeed(string hash, string Seed)
        {
            if (Seed != null && hash!=null)
            
            {
                SQLiteConnection sqcon = GetConnection();
                try
                {
                  //  sqcon.Open();

                    SQLiteCommand Command = new SQLiteCommand("update seed set server ='"+Seed+"' where hash='"+hash+"'", sqcon);
                    SQLiteDataReader Reader = Command.ExecuteReader();
                    List<Bet> Bets = new List<Bet>();
                    while (Reader.Read())
                    {
                        Bets.Add(BetParser(Reader));
                    }
                    //sqcon.Close();
                    
                }
                catch
                {
                }
                //sqcon.Close();
            }
            
        }

        public static Bet[] Search(bool betid, bool chance, bool stake, bool roll, bool profit, bool client, bool server, bool hash, int high, int verified, string Query, string SiteName)
        {
            Query = Query.Replace("'", "\"");
            string searchSring = "select bet.*,seed.server from bet, seed where bet.hash=seed.hash ";
            if (SiteName != "")
            {
                searchSring += " and site = '" + SiteName + "'";
            }
            searchSring += " and (";
            if (betid)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " betid like '" + Query + "'";
            }
            if (chance)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " chance like '" + Query + "'";
            }
            if (stake)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " stake like '" + Query + "'";
            }
            if (roll)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " lucky like '" + Query + "'";
            }
            if (profit)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " profit like '" + Query + "'";
            }
            if (client)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " client like '" + Query + "'";
            }
            if (server)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " server like '" + Query + "'";
            }
            if (hash)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " hash like '" + Query + "'";
            }
            searchSring += " )";
            if (high == 1)
            {
                searchSring += " and high = 1";
            }
            else if (high == 2)
            {
                searchSring += " and high = 0";
            }
            
            //using 
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                  //  sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand(searchSring, sqcon);

                    SQLiteDataReader Reader = Command.ExecuteReader();
                    List<Bet> Bets = new List<Bet>();
                    while (Reader.Read())
                    {
                        Bet b = (BetParser(Reader));
                        if ((b.Verified && verified == 1) || (!b.Verified && verified == 2) || verified == 3)
                        {
                            Bets.Add(b);
                        }
                    }
                    //sqcon.Close();
                    return Bets.ToArray();
                }
                catch
                {
                }
                //sqcon.Close();

            }
            return null;
        }
        public static Bet[] Search(bool betid, bool chance, bool stake, bool roll, bool profit, bool client, bool server, bool hash, int high, int verified, string Query, string SiteName, DateTime Start, DateTime End)
        {
            Query = Query.Replace("'", "\"");
            string searchSring = "select bet.*,seed.server from bet, seed where bet.hash=seed.hash ";
            if ( SiteName != "")
            {
                searchSring += " and site = '" + SiteName + "'";
            }
            searchSring += " (";
            if (betid)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " betid like '" + Query + "'";
            }
            if (chance)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " chance like '" + Query + "'";
            }
            if (stake)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " stake like '" + Query + "'";
            }
            if (roll)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " lucky like '" + Query + "'";
            }
            if (profit)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " profit like '" + Query + "'";
            }
            if (client)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " client like '" + Query + "'";
            }
            if (server)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " server like '" + Query + "'";
            }
            if (hash)
            {
                if (!searchSring.EndsWith("("))
                {
                    searchSring += " or";
                }
                searchSring += " hash like '" + Query + "'";
            }
            searchSring += " )";
            if (high==1)
            {

                searchSring += " and high = 1";
            }
            else if (high==2)
            {
                searchSring += " and high = 0";
            }
            searchSring += "and date >= '" + Start.ToShortDateString() + "'";
            searchSring += "and date <= '" + End.ToShortDateString() + "'";
            //ing (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand(searchSring, sqcon);
                    
                    SQLiteDataReader Reader = Command.ExecuteReader();
                    List<Bet> Bets = new List<Bet>();
                    while (Reader.Read())
                    {
                        Bet b = (BetParser(Reader));
                        if ((b.Verified && verified==1)||(!b.Verified && verified==2)||verified==3)
                        {
                            Bets.Add(b);
                        }
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

        public static Bet[] GetHistoryByQuery(string Query)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {
                try
                {
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand(Query, sqcon);
                    
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

        public static List<long> GetMissingSeedIDs(string site)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                    List<long> Bets = new List<long>();
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select hash from seed where server = ''", sqcon);
                    SQLiteDataReader Reader = Command.ExecuteReader();

                    List<string> hashes = new List<string>();

                    while (Reader.Read())
                    {
                        hashes.Add((string)Reader["hash"]);
                    }
                    foreach (string s in hashes)
                    {
                        Command = new SQLiteCommand("select min(betid) as betid from bet where hash = '"+s+"' and site ='"+site+"'", sqcon);
                    
                        if (Reader.Read())
                        {
                            Bets.Add((long)Reader["betid"]);
                        }
                    }
                    sqcon.Close();
                    return Bets;
                }
                catch
                {
                }
                sqcon.Close();
            }
            return null;
        }

        public static string GetHashForBet(string site, long betid)
        {
            //using (
            SQLiteConnection sqcon = GetConnection();
            {

                try
                {
                    sqcon.Open();
                    SQLiteCommand Command = new SQLiteCommand("select hash from bet where betid="+betid+" and site='"+site+"'", sqcon);
                    SQLiteDataReader Reader = Command.ExecuteReader();
                    string hash = "";
                    if (Reader.Read())
                    {
                        hash = (string)Reader["hash"];
                    }
                    sqcon.Close();
                    return hash;
                }
                catch
                {
                }
                sqcon.Close();
            }
            return null;
        }

        public void Dispose()
        {
            Conn.Close();
        }
    }
    public class sqbet
    {
        public Bet _Bet { get; set; }
        public string SiteName { get; set; }
    }
    
}
