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
    public partial class StatsControl : UserControl
    {
        public StatsControl()
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
