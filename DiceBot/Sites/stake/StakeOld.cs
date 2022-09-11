//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace DiceBot
//{
//    class StakeOld : PrimeDice
//    {

//        private static string[] _sCurrencies = new string[]
//        {
//            "BTC",
//            "ETH",
//            "LTC",
//            "DOGE",
//            "BCH",
//            "XRP",
//            "TRX",
//            "EOS",
//            "BNB",
//            "USDT",
//            "ape",
//            "busd",
//            "cro",
//            "dai",
//            "link",
//            "sand",
//            "shib",
//            "uni",
//            "usdc"
//        };


//        public static new string[] sCurrencies => _sCurrencies.Select(x => x.ToUpperInvariant()).OrderBy(x => x).ToArray();


//        public StakeOld(cDiceBot Parent) : base(Parent)
//        {
//            this.Currencies = sCurrencies;
//            this.Currency = "Btc";
//            _PasswordText = "API Key: ";
//            maxRoll = 100m;
//            AutoInvest = false;
//            AutoWithdraw = true;
//            ChangeSeed = true;
//            AutoLogin = true;
//            BetURL = "https://api.stake.bet/bets/";
//            register = false;
//            this.Parent = Parent;
//            Name = "Stake";
//            edge = 1;
//            Tip = false;
//            TipUsingName = true;

//            SiteURL = "https://stake.com/";
//            URL = "https://api.stake.bet/graphql";
//            RolName = "diceRoll";
//            Vault = true;
//            GameName = "CasinoGameDice";
//            StatGameName = "dice";
//            EnumName = "CasinoGameDiceConditionEnum";
//            HaveMirrors = true;
//            MirrorList = new List<string> { "stake.com",
//                                            "stake.bet",
//                                            "stake.games",
//                                            "staketr.com",
//                                            "staketr2.com",
//                                            "staketr3.com",
//                                            "staketr4.com",
//                                            "staketr5.com",
//                                            "stake.bz",
//                                            "stake.jp",
//                                            "stake.ac",
//                                            "stake.icu" ,
//            "stake.us"
//            };
//            CurrentMirror = "";
//        }
//        public override decimal GetLucky(string server, string client, int nonce)
//        {
//            return sGetLucky(server, client, nonce);
//        }
//        new public static decimal sGetLucky(string server, string client, long nonce)
//        {
//            HMACSHA256 betgenerator = new HMACSHA256();

//            int charstouse = 2;
//            List<byte> serverb = new List<byte>();

//            for (int i = 0; i < server.Length; i++)
//            {
//                serverb.Add(Convert.ToByte(server[i]));
//            }

//            betgenerator.Key = serverb.ToArray();

//            List<byte> buffer = new List<byte>();
//            string msg = client + ":" + nonce.ToString() + ":0";
//            foreach (char c in msg)
//            {
//                buffer.Add(Convert.ToByte(c));
//            }

//            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

//            StringBuilder hex = new StringBuilder(hash.Length * 2);
//            foreach (byte b in hash)
//                hex.AppendFormat("{0:x2}", b);

//            decimal total = 0;
//            for (int i = 0; i < 8; i += charstouse)
//            {

//                string s = hex.ToString().Substring(i, charstouse);

//                decimal lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
//                lucky = lucky / (decimal)(Math.Pow(256, (double)((i / 2) + 1)));
//                total += lucky;
//                /*if (lucky < 1000000)
//                {
//                    lucky %= 10000;
//                    return lucky / 100;

//                }*/
//            }
//            total = Math.Floor(total * 10001) / 100m;
//            return total;
//            return 0;
//        }



//        public override void UpdateMirror(string url)
//        {
//            //CurrentMirror = Parent.
//            if (url != "" && MirrorList.Contains(url))
//            {
//                BetURL = $"https://api.{url}/bets/";
//                SiteURL = $"https://{url}/";
//                URL = $"https://api.{url}/graphql";
//                //base.UpdateMirror(URL);
//            }

//        }






//    }
//}
