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
