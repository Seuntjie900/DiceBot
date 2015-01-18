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

            string seeds = "CREATE TABLE if not exists {0}_seed(	hash nvarchar(128) NOT NULL primary key,	server text NOT NULL) ";
            string bets = "CREATE TABLE if not exists {0}_bet(betid bigint NOT NULL primary key,date datetime NULL,stake decimal(18, 8) NULL,profit decimal(18, 8) NULL,chance decimal(18, 8) NULL,	high bit NULL,	lucky decimal(18, 8) NULL,	hash nvarchar(128) NULL,	nonce bigint NULL,	uid int NULL,	Client nvarchar(50) NULL )";
            
            string[] sites = { "PRCDice", "JustDice", "PrimeDice","Dice999","SAfEDICE" };
            foreach (string s in sites)
            {

                SQLiteCommand Command = new SQLiteCommand("", sqcon);
                Command.CommandText = string.Format(seeds, s);
                Command.ExecuteNonQuery();
                Command.CommandText = string.Format(bets, s);
                Command.ExecuteNonQuery();
                 
            }
            sqcon.Clone();
        }


        
    }
}
