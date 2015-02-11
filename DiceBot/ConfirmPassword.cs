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
    public partial class ConfirmPassword : Form
    {
        public string Password { get; set; }

        public ConfirmPassword()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Password = txtPassword.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
