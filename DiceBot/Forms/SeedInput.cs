using System;
using System.Windows.Forms;
using DiceBot.Common;

namespace DiceBot.Forms
{
    public partial class SeedInput : Form
    {
        public SeedInput()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    SQLiteHelper.InsertSeed(textBox2.Text, textBox1.Text);
                    lblStatus.Text = "Inserted seed: " + textBox1.Text.Substring(0, 15) + "...";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                }
                catch { lblStatus.Text = "Failed to insert seed: " + textBox1.Text.Substring(0, 15) + "..."; lblStatus.ForeColor = System.Drawing.Color.Red; }
            }
            else
            {
                lblStatus.Text = "Please enter a seed as well as the hash for the seed.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

}
