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
    public partial class DonateBox : Form
    {
        public DonateBox()
        {
            InitializeComponent();
        }
        string currency = "";
        public decimal amount { get; set; }
        decimal _Amount = 00;
        
        public DialogResult ShowDialog(decimal Amount, string currency, decimal defaultAmount)
        {
            this.currency = currency;
            _Amount = Amount;
            numericUpDown1.Value = (decimal)defaultAmount;
            lblmount.Text = ((decimal)_Amount * (numericUpDown1.Value / 100m)).ToString();
            label1.Text = label1.Text.Replace("xx.xxxxxxxx", Amount.ToString("0.00000000"));
            label1.Text = label1.Text.Replace("yyy", currency);
            return this.ShowDialog();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            lblmount.Text = ((decimal)_Amount * (numericUpDown1.Value / 100m)).ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            amount = (decimal)((decimal)_Amount * (numericUpDown1.Value / 100m));
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            amount = (decimal)((decimal)_Amount * (numericUpDown1.Value / 100m));
            DialogResult = DialogResult.Yes;
            this.Close();
        }
    }
}
