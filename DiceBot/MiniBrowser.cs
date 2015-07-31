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
    public partial class MiniBrowser : Form
    {
        public object result;
        public MiniBrowser(string URL)
        {
            InitializeComponent();
            webBrowser1.Navigate(URL);
            webBrowser1.ScriptErrorsSuppressed = true;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            
            
        }

        
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //{https://bot.seuntjie.com/mpdb/oauth.aspx#access_token=3aa9a7dc-859a-45ab-8c67-31a25d9c943a&expires_in=1209599}
            if (e.Url.AbsoluteUri.Contains("bot.seuntjie.com/mpdb"))
            {
                if (e.Url.AbsoluteUri.Contains("oauth.aspx"))
                {
                    string tmp = e.Url.AbsoluteUri.Substring(e.Url.AbsoluteUri.IndexOf("access_token=") + "access_token=".Length);
                    result = tmp.Substring(0, tmp.IndexOf("&"));
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                    return;
                }
            }
            
        }
    }
}
