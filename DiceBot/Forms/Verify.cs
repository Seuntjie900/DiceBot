using System;
using System.Windows.Forms;

namespace DiceBot.Forms
{
    public partial class Verify : Form
    {
        new cDiceBot Parent;
        public Verify(cDiceBot Parent)
        {
            InitializeComponent();
            this.Parent = Parent;
        }

        private void btnGenerateBets_Click(object sender, EventArgs e)
        {
            Parent.GenerateBets_Click(txtClientSeed.Text, txtServerSeed.Text, (long)nudGenBetsStart.Value, (long)nudGenBetsAmount.Value);
        }
    }
}
