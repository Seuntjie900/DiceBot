using System.Net.Mail;
using System.Threading;

namespace DiceBot.Common
{
    public class Email
    {
        public string botname = "";
        public string emailaddress = "";
        private Thread sendemail;
        public bool Withdraw { get; set; }
        public bool Lower { get; set; }
        public bool Streak { get; set; }
        public int StreakSize { get; set; }
        public bool Enable { get; set; }
        public string SMTP { get; set; }

        public Email(string botname, string emailaddress)
        {
            this.botname = botname;
            this.emailaddress = emailaddress;
            SMTP = "smtp.secureserver.net";
        }

        public void SendWithdraw(decimal amount, decimal balance, string address)
        {
            var subject = "DiceBot - Withdraw Alert";
            var Message = "Hi, your bot, " + botname + ", has just withdrawn " + amount.ToString("0.00000000") + " Btc to " + address + ".\n";
            Message += "\nYour new balance is " + balance.ToString("0.00000000");
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";

            if (Withdraw)
            {
                sendemail = new Thread(sendmail);
                sendemail.Start(new eminfo(Message, subject));
            }
        }

        public void SendInvest(decimal amount, decimal balance, decimal invested)
        {
            var subject = "DiceBot - Invest Alert";
            var Message = "Hi, your bot, " + botname + ", has just invested " + amount.ToString("0.00000000") + " Btc.\n";
            Message += "\nYour new balance is " + balance.ToString("0.00000000");
            Message += "\nYour Total Investment is now " + invested.ToString("0.00000000");
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";

            if (Withdraw)
            {
                sendemail = new Thread(sendmail);
                sendemail.Start(new eminfo(Message, subject));
            }
        }

        public void SendLowLimit(decimal balance, decimal lowmlimit, decimal lastbet)
        {
            var subject = "DiceBot - Low Limit Alert!";

            var Message = "Hi, your bot, " + botname + ", has just stopped betting, becuase the last bet, " + lastbet.ToString("0.00000000") +
                          " would have put your balance below your lower limit of " + lowmlimit.ToString("0.00000000") + "\n";

            Message += "\nYour new balance is " + balance.ToString("0.00000000");
            Message += "\nPlease Tend to this poor bot.";
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";

            if (Lower)
            {
                sendemail = new Thread(sendmail);
                sendemail.Start(new eminfo(Message, subject));
            }
        }

        public void SendStreak(int streak, decimal steaklimit, decimal balance)
        {
            var subject = "DiceBot - Bad Streak Alert!";
            var Message = "Hi, your bot, " + botname + ", has just had a losing streak of " + streak + "\n";
            Message += "\nYour Current balance is " + balance.ToString("0.00000000");
            Message += "\nPlease Tend to this poor bot.";
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";

            if (Streak)
            {
                sendemail = new Thread(sendmail);
                sendemail.Start(new eminfo(Message, subject));
            }
        }

        private void sendmail(object Eminfo)
        {
            try
            {
                var message = new MailMessage();
                message.To.Add(emailaddress);
                message.Subject = (Eminfo as eminfo).subj;
                message.From = new MailAddress("DiceBot@seuntjie.com");
                message.Body = (Eminfo as eminfo).msg;
                var smtp = new SmtpClient(SMTP);
                smtp.Send(message);
            }
            catch
            {
            }
        }
    }

    internal class eminfo
    {
        public string msg;
        public string subj;

        public eminfo(string msg, string subj)
        {
            this.msg = msg;
            this.subj = subj;
        }
    }
}
