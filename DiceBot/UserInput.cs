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
    public partial class UserInput : Form
    {
        public UserInput()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }
        public object Value { get; set; }
        int mode = 0;
        bool SetDefaultText = true;
        public DialogResult ShowDialog(string prompt, int mode)
        {
            this.mode = mode;
            lblPrompt.Text = prompt;
            if (mode == 0)
            {
                txtTextInput.Visible = false;
                nudNumInput.Visible = false;
                if (SetDefaultText)
                {
                    btnCancel.Text = "No";
                    btnOk.Text = "Yes";
                }
            }
            if (mode == 1)
            {
                nudNumInput.Visible = true;
                txtTextInput.Visible = false;
                nudNumInput.DecimalPlaces = 0;
                nudNumInput.Increment = 1;
            }
            if (mode == 2)
            {
                nudNumInput.Visible = true;
                txtTextInput.Visible = false;
                nudNumInput.DecimalPlaces = 8;
                nudNumInput.Increment = 0.0001m;
            }
            if (mode == 2)
            {
                nudNumInput.Visible = false;
                txtTextInput.Visible = true;
            }

            return ShowDialog();
        }
        public DialogResult ShowDialog(string prompt, int mode, string userinputext, string btncanceltext, string btnoktext)
        {
            lblPrompt.Text = prompt;
            this.Text = userinputext;
            btnCancel.Text = btncanceltext;
            btnOk.Text = btnoktext;                        
            return ShowDialog(prompt, mode);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            switch (mode)
            { case 0: Value = true; break;
                case 1: Value = (int)nudNumInput.Value; break;
                case 2: Value = (decimal)nudNumInput.Value; break;
                case 3: Value = txtTextInput.Text; break;
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            if (mode == 0)
                Value = false;
            else
                Value = null;
            this.Close();
        }
    }
}
