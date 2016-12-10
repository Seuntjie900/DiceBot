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
    public partial class BitdiceConfirm : Form
    {
        public BitdiceConfirm()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            /*if (textBox1.Text.StartsWith("https://"))
                textBox1.Text = textBox1.Text.Substring(textBox1.Text.LastIndexOf("/")+1);*/
            bool success = bitdice.Enable(textBox1.Text);
            MessageBox.Show(success ? "Activated" : "Failed to activate this device. Please check your code");
            if (success)
                this.Close();
        }
    }
}
