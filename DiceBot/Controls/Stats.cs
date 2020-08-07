using System;
using System.Windows.Forms;

namespace DiceBot.Controls
{
    public partial class Stats : UserControl
    {
        public Stats()
        {
            InitializeComponent();
        }

        public bool ShowHideButtons { get { return btnHideStats.Visible; } set { btnHideStats.Visible = button2.Visible = value; } }

        private void btnResetStats_Click(object sender, EventArgs e)
        {

        }

        private void btnHideStats_Click_1(object sender, EventArgs e)
        {
            this.FindForm().Close();
        }

        private void label37_Click(object sender, EventArgs e)
        {

        }

        private void tabPage7_Click(object sender, EventArgs e)
        {

        }
        
    }
}
