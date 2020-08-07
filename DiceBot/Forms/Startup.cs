using System;
using System.Windows.Forms;

namespace DiceBot.Forms
{
    public partial class Startup : Form
    {
        public Startup()
        {
            InitializeComponent();
        }
        string Link = "";
        public void Show(string Message, string Link)
        {
            rtbNews.Text = Message;
            if (!string.IsNullOrEmpty(Link))
            {
                this.Link = Link;
                btnSite.Visible = true;
            }
            this.Show();
        }
       
        private void btnSite_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Link))
            System.Diagnostics.Process.Start(Link);
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
