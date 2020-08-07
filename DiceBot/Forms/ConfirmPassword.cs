using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DiceBot.Forms
{
    public partial class ConfirmPassword : Form
    {
        public string Password { get; set; }

        public ConfirmPassword()
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
            
        }
    }
}
