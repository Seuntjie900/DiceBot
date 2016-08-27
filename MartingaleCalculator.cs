using System;
using System.Windows.Forms;

namespace DiceBot
{
    public partial class MartingaleCalculator : Form
    {
        public MartingaleCalculator()
        {
            InitializeComponent();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            int rolls = Convert.ToInt32(textBox1.Text);
            decimal bet = (decimal)Convert.ToDecimal(textBox2.Text);
            int multi = Convert.ToInt32(textBox3.Text);
            decimal heretotal = bet;
            ListViewItem item0 = new ListViewItem(0.ToString());
            item0.SubItems.Add(bet.ToString());
            item0.SubItems.Add(heretotal.ToString());
            listView1.Items.Add(item0);
            decimal[] n = new decimal[rolls + 1];
            decimal[] g = new decimal[rolls + 1];
            int i, j;           
            for (i = 1; i < rolls + 1; i++)
            {

                bet = (bet * multi);
                n[i] = bet;
                heretotal = (heretotal + bet);
                g[i] = heretotal;
            }            
            for (j = 1; j < rolls + 1; j++)
            {
                ListViewItem item = new ListViewItem(j.ToString());
                item.SubItems.Add(n[j].ToString());
                item.SubItems.Add(g[j].ToString());
                listView1.Items.Add(item);

            }
        }

        
    }
}
