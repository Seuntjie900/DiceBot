using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DiceBot.Forms;

namespace DiceBot.Sites
{
    internal class Stake : PD
    {
        public new static string[] sCurrencies = {"Btc", "Eth", "Ltc", "Doge", "Bch", "Xrp", "Trx"};

        public Stake(cDiceBot Parent) : base(Parent) /*:base(Parent)*/
        {
            Currencies = sCurrencies;
            Currency = "Btc";
            _PasswordText = "API Key: ";
            maxRoll = 100m;
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            BetURL = "https://api.stake.com/bets/";
            register = false;
            this.Parent = Parent;
            Name = "Stake";
            edge = 1;
            Tip = true;
            TipUsingName = true;
            SiteURL = "https://stake.com/?code=seuntjie";
            URL = "https://api.stake.com/graphql/";
            RolName = "diceRoll";

            GameName = "CasinoGameDice";
            StatGameName = "dice";
        }

        public override decimal GetLucky(string server, string client, int nonce)
        {
            return sGetLucky(server, client, nonce);
        }

        public new static decimal sGetLucky(string server, string client, long nonce)
        {
            var betgenerator = new HMACSHA256();

            var charstouse = 2;
            var serverb = new List<byte>();

            for (var i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            var buffer = new List<byte>();
            var msg = client + ":" + nonce + ":0";

            foreach (var c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            var hash = betgenerator.ComputeHash(buffer.ToArray());

            var hex = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            decimal total = 0;

            for (var i = 0; i < 8; i += charstouse)
            {
                var s = hex.ToString().Substring(i, charstouse);

                decimal lucky = int.Parse(s, NumberStyles.HexNumber);
                lucky = lucky / (decimal) Math.Pow(256, i / 2 + 1);
                total += lucky;
                /*if (lucky < 1000000)
                {
                    lucky %= 10000;
                    return lucky / 100;

                }*/
            }

            total = Math.Floor(total * 10001) / 100m;

            return total;
            return 0;
        }
    }
}
