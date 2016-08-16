using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace DiceBot
{
    public class Email
    {
        Thread sendemail;
        public string botname = "";
        public string emailaddress = "";
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
            string subject = "DiceBot - Withdraw Alert";
            string Message = "Hi, your bot, "+botname+", has just withdrawn "+amount.ToString("0.00000000")+" Btc to "+address+".\n";
            Message+="\nYour new balance is " + balance.ToString("0.00000000");
            Message+="\n\nTo stop these email notifications, go to Settings on "+botname+" and uncheck relevant checkbox";
            if (Withdraw)
            {
                sendemail = new Thread(new ParameterizedThreadStart(sendmail));
                sendemail.Start(new eminfo(Message, subject));
            }
        }
        public void SendInvest(decimal amount, decimal balance, decimal invested)
        {
            string subject = "DiceBot - Invest Alert";
            string Message = "Hi, your bot, " + botname + ", has just invested " + amount.ToString("0.00000000") + " Btc.\n";
            Message += "\nYour new balance is " + balance.ToString("0.00000000");
            Message += "\nYour Total Investment is now " + invested.ToString("0.00000000");
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";
            if (Withdraw)
            {
                sendemail = new Thread(new ParameterizedThreadStart(sendmail));
                sendemail.Start(new eminfo(Message, subject));
            }
        }
        public void SendLowLimit(decimal balance, decimal lowmlimit, decimal lastbet)
        {
            string subject = "DiceBot - Low Limit Alert!";
            string Message = "Hi, your bot, " + botname + ", has just stopped betting, becuase the last bet, " + lastbet.ToString("0.00000000") + " would have put your balance below your lower limit of "+lowmlimit.ToString("0.00000000")+"\n";
            Message += "\nYour new balance is " + balance.ToString("0.00000000");
            Message += "\nPlease Tend to this poor bot.";
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";
            if (Lower)
            {
                sendemail = new Thread(new ParameterizedThreadStart(sendmail));
                sendemail.Start(new eminfo(Message, subject));
            }
        }
        public void SendStreak(int streak, decimal steaklimit, decimal balance)
        {
            string subject = "DiceBot - Bad Streak Alert!";
            string Message = "Hi, your bot, " + botname + ", has just had a losing streak of "+streak.ToString()+"\n";
            Message += "\nYour Current balance is " + balance.ToString("0.00000000");
            Message += "\nPlease Tend to this poor bot.";
            Message += "\n\nTo stop these email notifications, go to Settings on " + botname + " and uncheck relevant checkbox";
            if (Streak)
            {
                sendemail = new Thread(new ParameterizedThreadStart(sendmail));
                sendemail.Start(new eminfo(Message, subject));
            }
        }
        private void sendmail(object Eminfo)
        {
            try
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add(this.emailaddress);
                message.Subject = (Eminfo as eminfo).subj;
                message.From = new System.Net.Mail.MailAddress("DiceBot@seuntjie.com");
                message.Body = (Eminfo as eminfo).msg;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(SMTP);
                smtp.Send(message);
                
            }
            catch
            {
                
            }
        }
    }

    class eminfo
    {
        public eminfo(string msg, string subj)
        {
            this.msg = msg;
            this.subj = subj;
        }
        public string msg;
        public string subj;
    }


    
}
