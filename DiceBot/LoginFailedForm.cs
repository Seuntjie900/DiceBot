using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiceBot.Core;

namespace DiceBot
{
    public partial class LoginFailedForm : Form
    {
        string HelpLink = "https://bot.seuntjie.com/faqs.aspx";
        public LoginFailedForm()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(bool register, DiceSite CurrentSite)
        {
            if (register)
            {
                richTextBox1.Text = "Failed to register account.";
                btnHelp.Visible = false;
            }
            else
            {
                richTextBox1.Text = "Failed to log in to your account on "+CurrentSite.Name+@". Check the status bar for more information if available.

Are you having trouble logging in? Click the help button below to see the most common login problems and how to resolve them.";
                HelpLink = "https://bot.seuntjie.com/faqs.aspx#faq25";
                btnHelp.Visible = true;
            }
            return ShowDialog();
        }
        public DialogResult ShowDialog(string ErrorMessage)
        {
            richTextBox1.Text = ErrorMessage;
            btnHelp.Visible = false;
            return ShowDialog();
        }

        public DialogResult ShowDialog(string ErrorMessage, string HelpLink)
        {
            richTextBox1.Text = ErrorMessage;
            this.HelpLink = HelpLink;
            btnHelp.Visible = true;
            return ShowDialog();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
            Process.Start(HelpLink);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
