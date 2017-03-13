using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace DiceBot
{
    class BitExo:DiceSeuntjie
    {


        public BitExo(cDiceBot Parent):base(Parent)
        {
            maxRoll = 99.9999m;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://www.moneypot.com/bets/";
            edge = 1;
            //this.Parent = Parent;
            Name = "Bit-Exo";
            Tip = true;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://bit-exo.com/?ref=seuntjie ";
            url = "bit-exo.com";
            APPId = 588;

        }
    }
}
