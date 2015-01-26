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
    public partial class BetHistory : Form
    {
        string SiteName = "";
        public BetHistory(string SiteName)
        {
            InitializeComponent();
            cmbView.SelectedIndex = 0;
            this.SiteName = SiteName;
        }

        private void cmbView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbView.SelectedIndex<=3)
            {
                label2.Visible= label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = false;
                GetBets(GetDate(cmbView.SelectedIndex), DateTime.Parse(DateTime.Now.ToShortDateString()).AddDays(1));
            }
            else if (cmbView.SelectedIndex==4)
            {
                label2.Visible = label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = false;
                GetBets();
            }
            else
            {
                label2.Visible = label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = true;
            }
            
        }
        DateTime GetDate(int Type)
        {
            DateTime tmp = new DateTime();
            //get start date for Day.
            if (Type == 1)
            {
                return DateTime.Parse(DateTime.Now.ToShortDateString());
            }

            //get start date for week.
            if (Type==1)
            {
                return DateTime.Parse(DateTime.Now.ToShortDateString()).AddDays(-(int)DateTime.Now.DayOfWeek);
            }

            //get start date for Month.
            if (Type == 2)
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            //get start date for Year.
            if (Type == 3)
            {
                return new DateTime(DateTime.Now.Year, 1, 1);
            }
            return tmp;
        }
        void GetBets()
        {
            Bet[] Bets = sqlite_helper.GetBetHistory(SiteName);
            BindingSource bs = new BindingSource();
            bs.DataSource = Bets;
            dgvBets.DataSource = bs;
        }

        void GetBets(DateTime Start, DateTime End)
        {
            Bet[] Bets = sqlite_helper.GetBetHistory(SiteName, Start, End);
            BindingSource bs = new BindingSource();
            bs.DataSource = Bets;
            dgvBets.DataSource = bs;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            GetBets(dtpFrom.Value, dtpUntill.Value);
        }
    }
}
