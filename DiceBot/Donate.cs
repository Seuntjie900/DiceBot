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
    public partial class Donate : Form
    {
        public Donate(string SiteName)
        {
            InitializeComponent();
            string tipmsg = "You can tip to the account {0} on {1}";
            switch (SiteName.ToLower())
            {
                case "primedice": tipmsg = string.Format(tipmsg, "seuntjie", SiteName); break;
                case "999dice": tipmsg = string.Format(tipmsg, "seuntjie", SiteName); break;
                case "justdice": tipmsg = string.Format(tipmsg, "seuntjie (91380)", SiteName); break;
                case "prcdice": tipmsg = string.Format(tipmsg, "seuntjie (357)", SiteName); break;
                case "safedice": tipmsg = string.Format(tipmsg, "seuntjie (1050)", SiteName); break;
            }
            lblTip.Text = tipmsg;
        }

        private void btnBtc_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtBtc.Text);
        }

        private void btcDoge_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtDoge.Text);
        }

        private void btcLtc_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtLtc.Text);
        }

        private void btcClam_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtClam.Text);
        }
    }
}
