using System.ComponentModel;
using System.Windows.Forms;
using DiceBot.Controls;

namespace DiceBot.Forms
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
        public void AddStatsWindow(Stats stats)
        {
            this.Controls.Add(stats);
            stats.ShowHideButtons = true;
        }
    }
}
