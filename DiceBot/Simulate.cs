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
