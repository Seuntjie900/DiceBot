using System.ComponentModel;
using System.Windows.Forms;

namespace DiceBot.Forms
{
    public partial class Simulate : Form
    {
        new cDiceBot Parent;
        public Simulate(cDiceBot Parent)
        {
            InitializeComponent();
            this.Parent = Parent;
            btnSim.Click += Parent.btnSim_Click;
            btnExportSim.Click += Parent.btnExportSim_Click;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            base.OnClosing(e);
        }
    }
}
