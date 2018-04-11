using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
namespace DiceBot
{
    class PrimeDice : DiceSite
    {
        GraphQLClient Client = new GraphQLClient("https://api.primedice.com/graphql");
        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            GraphQLRequest LoginReq = new GraphQLRequest
            {
                Query = "mutation{loginUser(name:\""+Username+"\", password:\""+Password+"\""+(string.IsNullOrWhiteSpace(twofa)?"":",tfaToken:\""+twofa+"\"")+ ") {activeSeed { serverSeedHash clientSeed nonce} id balances{available{currency amount value}} throttles{key value ttl type refType refId} statistic {bets wins losses amount profit currency value}}}"
            };
            GraphQLResponse Resp = Client.PostAsync(LoginReq).Result;
            string userid = Resp.Data.user.id.value;
        }

        public override bool ReadyToBet()
        {
            throw new NotImplementedException();
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override void internalPlaceBet(bool High, decimal amount, decimal chancem, string BetGuid)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(decimal Amount, string Address)
        {
            throw new NotImplementedException();
        }
    }
}
