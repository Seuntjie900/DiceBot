using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DiceBot
{
    public partial class fChat : Form
    {
        public fChat(string messages)
        {
            InitializeComponent();
            rtbChat.Text = messages;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            base.OnClosing(e);
        }
        public void GotMessage(string Message)
        {
            rtbChat.AppendText(Message+"\r\n");
        }
        public delegate void dSendMessage(string Message);
        public event dSendMessage SendMessage;

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (SendMessage!=null)
            { SendMessage(rtbMessage.Text); }
            rtbMessage.Text = "";
        }
    }
}
