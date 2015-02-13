using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client;

namespace DiceBot
{
    class PRC:DiceSite
    {
        HubConnection con = new HubConnection("https://pocketrocketscasino.eu");
        public PRC()
        {
            con.Start();
        }

        public override void PlaceBet(bool High)
        {
            throw new NotImplementedException();
        }

        public override void SetChance(string Chance)
        {
            throw new NotImplementedException();
        }

        public override void SetAmount(double Amount)
        {
            throw new NotImplementedException();
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue()
        {
            throw new NotImplementedException();
        }

        public override string GetSiteProfitValue()
        {
            throw new NotImplementedException();
        }

        public override string GetTotalBets()
        {
            throw new NotImplementedException();
        }

        public override string GetMyProfit()
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override bool Withdraw(double Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override bool Login(string Username, string Password)
        {
            throw new NotImplementedException();
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
