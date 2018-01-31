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
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            rtbAbout.Text = "DiceBot\n";
            rtbAbout.Text += "Programmer: Seuntjie\n";
            rtbAbout.Text += "vs 3.3.12\n";
            rtbAbout.Text += "\n";
            /*rtbAbout.Text += "This bot uses the Martingale method to automatically gamble at http://just-dice.com. All values specified for the martingale method can be se on the bot, this includes starting bet, multiplier, chance of winning.";
            rtbAbout.Text += "You can also set a maximum amount of time the bet will me multiplied, or decrease the size of the mutliplier every so many bets.\n\n";
            rtbAbout.Text += "To find out what a setting or input does, hover over the label for that setting.\n\n";
            rtbAbout.Text += "A safety key has been built into the program, so you can stop all betting, no matter the settings and no matter what window you are focused on. hitting left ctrl + left shit + s on combination will immediately STOP ALL BETTING until restarted by user\n\n";*/
            rtbAbout.Text += "This page is out of date and will be updated shortly.\n\n";
            rtbAbout.Text += "Please feel free to donate. \nBtc:  1EHPYeVGkquij8eMRQqwyb5bjpooyyfgn5 \nLtc:  LQvMRbyuuSVsvXA3mQQM3zXT53hb34CEzy \nDoge:  DR32dpGniJP9mJo4NpzXGCTdsJLcp4td2X";

        }

        

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
