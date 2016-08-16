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
    public partial class StreakTable : Form
    {
        decimal minbet;
        decimal multliplier;
        decimal devider;
        int nbets;
        int maxmultiplies;
        int mode;
        public StreakTable(decimal minbet, decimal multliplier, decimal devider, int nbets, int maxmultiplies, int mode, decimal chance )
        {
            
            InitializeComponent();
            txtMinBet.Text = (this.minbet = minbet).ToString(); ;
            txtMultiplier.Text =  (this.multliplier = multliplier).ToString();
            txtDevider.Text = (this.devider = devider).ToString(); ;
            txtNBets.Text = (this.nbets = nbets).ToString();
            txtReturn.Text = (99 / chance).ToString();
            nudMaxMultiply.Value= this.maxmultiplies = maxmultiplies;
            this.mode = mode;
            switch (mode)
            {
                case 3: rdbOnce.Checked = true; break;
                case 2: rdbMax.Checked = true; ; break;
                case 1: rdbVariable.Checked = true; break;
                case 0: rdbConstant.Checked = true; ; break;

            }
            populatedg();
        }

        void populatedg()
        {
            List<cBet> bets = new List<cBet>();
            for (int i =0; i<30; i++)
            {
                cBet bet = new cBet();
                bet.BetNr = (i+1).ToString();
                if (mode == 3 && i != 0 && i ==nbets)
                {
                    multliplier = multliplier * devider;
                }
                if (mode == 1 && i!=0 && i%nbets==0)
                {
                    multliplier = multliplier * devider;
                }
                else if (mode == 2 && i > maxmultiplies)
                {
                    multliplier = 1;
                }
                if (i > 0)
                    bet.Bet_Amount = (decimal.Parse(bets[i - 1].Bet_Amount) * multliplier).ToString("0.00000000");
                else
                    bet.Bet_Amount = (minbet * multliplier).ToString();

                bet.Total_Wagered = bet.Bet_Amount;
                if (i != 0)
                    bet.Total_Wagered = (decimal.Parse(bets[i - 1].Total_Wagered)+decimal.Parse(bet.Bet_Amount)).ToString("0.00000000");
                bet.Return_on_win = (decimal.Parse(bet.Bet_Amount) * decimal.Parse(txtReturn.Text)).ToString("0.00000000");
                bet.Profit = (decimal.Parse(bet.Return_on_win) - decimal.Parse(bet.Total_Wagered)).ToString("0.00000000");
                bets.Add(bet);
            }

            BindingSource bs = new BindingSource();
            bs.DataSource = bets;
            dataGridView1.DataSource = bs;
        }

        private void btnCacl_Click(object sender, EventArgs e)
        {
            bool valid = true;
            if (!decimal.TryParse(txtDevider.Text, out devider))
            {
                valid = false;
            }
            if (!decimal.TryParse(txtMinBet.Text, out minbet))
            {
                valid = false;
            }
            if (!decimal.TryParse(txtMultiplier.Text, out multliplier))
            {
                valid = false;
            }
            if (!int.TryParse(txtNBets.Text, out nbets))
            {
                valid = false;
            }

           
            maxmultiplies = (int)nudMaxMultiply.Value;
            if (rdbConstant.Checked)
                mode = 0;
            if (rdbMax.Checked)
                mode = 2;
            if (rdbVariable.Checked)
                mode = 1;
            if (rdbOnce.Checked)
                mode = 3;
            if (valid)
            {
                populatedg();
            }
        }
    }
}
