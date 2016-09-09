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
    public partial class Stats : Form
    {
        public Stats()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            base.OnClosing(e);
        }
        private void btnResetStats_Click(object sender, EventArgs e)
        {

        }

        private void btnHideStats_Click_1(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void label37_Click(object sender, EventArgs e)
        {

        }

        private void tabPage7_Click(object sender, EventArgs e)
        {

        }
        
    }
}
