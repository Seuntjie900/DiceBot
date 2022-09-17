using System;

namespace DiceBot.Core
{
    public interface IDiceSite
    {
        event DiceSite.dFinishedLogin FinishedLogin;
        event EventHandler<RequireCaptchaEventArgs> OnRequireCaptcha;

        void Disconnect();
        
        int GetBets();
        int GetLosses();
        decimal GetLucky(string server, string client, int nonce);
        decimal GetProfit();
        void GetSeed(long BetID);
        decimal GetWagered();
        int GetWins();

        void Login(string Username, string Password, string twofa);
        void PlaceBet(bool High, decimal amount, decimal chance, string BetGuid);
        bool ReadyToBet();
        bool Register(string username, string password);
        void ResetSeed();
        void SendChatMessage(string Message);

        void SetClientSeed(string Seed);
        void SetProxy(string host, int port);
        void SetProxy(string host, int port, string username, string password);
        void UpdateMirror(string url);


        bool Withdraw(decimal Amount, string Address);
        bool SendTip(string User, decimal Amount);
        bool SendToVault(decimal Amount);
        bool InternalSendTip(string User, decimal amount);
        bool InternalSendToVault(decimal amount);
        bool Invest(decimal Amount);

        void Donate(decimal Amount);

    }
}