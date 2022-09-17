using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiceBot
{
    public partial class StatsForm : Form
    {
        public StatsForm()
        {
            InitializeComponent();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Controls.Remove(Controls[0]);
            /*this.Hide();
            e.Cancel = true;
            base.OnClosing(e);*/
        }
        public void AddStatsWindow(StatsControl stats)
        {
            this.Controls.Add(stats);
            stats.ShowHideButtons = true;
        }
    }
}
