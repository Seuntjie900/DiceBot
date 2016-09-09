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
                    sqlite_helper.InsertSeed(textBox2.Text, textBox1.Text);
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
