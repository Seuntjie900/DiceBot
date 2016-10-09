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
        Bet[] Data;
        int page = 0;
        int NumPerPage = 0;
        int lastPage = 0;
        public BetHistory(string SiteName)
        {
            InitializeComponent();
            this.SiteName = SiteName;
            cmbViewPerPage.SelectedIndex = 0;
            cmbView.SelectedIndex = 0;
            NumPerPage = int.Parse(cmbViewPerPage.SelectedItem.ToString());
            
        }

        private void cmbView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbView.SelectedIndex<=3)
            {
                label2.Visible= label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = false;
                GetBets(GetDate(cmbView.SelectedIndex), DateTime.Now.AddDays(1000));
                pnlSearch.Visible = false;
            }
            else if (cmbView.SelectedIndex==4)
            {
                label2.Visible = label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = false;
                pnlSearch.Visible = false;
                GetBets();
            }
            else if (cmbView.SelectedIndex == 5)
            {
                label2.Visible = label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = true;
                pnlSearch.Visible = false;
            }
            else 
            {
                label2.Visible = label3.Visible = dtpFrom.Visible = dtpUntill.Visible = btnView.Visible = false;
                pnlSearch.Visible = true;
            }
            
        }
        DateTime GetDate(int Type)
        {
            DateTime tmp = new DateTime();
            //get start date for Day.
            if (Type == 0)
            {
                return (DateTime.Today);
            }

            //get start date for week.
            if (Type==1)
            {
                return (DateTime.Today).AddDays(-(int)DateTime.Now.DayOfWeek);
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
            Data = sqlite_helper.GetBetHistory(SiteName);
            CalcLastPage();
            List<Bet> Bets = new List<Bet>();
            for (int i = page * NumPerPage; i < (page + 1) * NumPerPage && i < Data.Length; i++)
            {
                Bets.Add(Data[i]);
            }
            BindingSource bs = new BindingSource();
            bs.DataSource = Bets;
            dgvBets.DataSource = bs;
        }

        void GetBets(DateTime Start, DateTime End)
        {
            Data = sqlite_helper.GetBetHistory(SiteName, Start, End);
            CalcLastPage();
            List<Bet> Bets = new List<Bet>();
            for (int i = page*NumPerPage; i< (page+1)*NumPerPage && i< Data.Length; i++)
            {
                Bets.Add(Data[i]);
            }
            BindingSource bs = new BindingSource();
            bs.DataSource = Bets;
            dgvBets.DataSource = bs;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            GetBets(dtpFrom.Value, dtpUntill.Value);
        }        

        private void rdbDateRange_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbDateRange.Checked)
            {
                lblSearchDateFrom.Visible = lblSearchDateUntill.Visible = dtpSearchFrom.Visible = dtpSearchUntil.Visible = true;
            }
            else
            {                
                lblSearchDateFrom.Visible = lblSearchDateUntill.Visible = dtpSearchFrom.Visible = dtpSearchUntil.Visible = false;                
            }
        }

       
        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            int high = (rdbHigh.Checked ? 1 : rdbLow.Checked ? 2 : 3);
            int verify = rdbVerified.Checked ? 1 : rdbNotVerified.Checked ? 2 : 3;
            if (rdbDateAll.Checked)            
            Data = sqlite_helper.Search(chkBetId.Checked,
                chkChance.Checked,
                chkStake.Checked,
                chkRoll.Checked,
                chkProfit.Checked,
                chkClientSeed.Checked,
                chkServerSeed.Checked,
                chkServerHash.Checked,
                high,
                verify,
                textBox1.Text,                
                SiteName);
            else
                Data = sqlite_helper.Search(chkBetId.Checked,
                chkChance.Checked,
                chkStake.Checked,
                chkRoll.Checked,
                chkProfit.Checked,
                chkClientSeed.Checked,
                chkServerSeed.Checked,
                chkServerHash.Checked,
                high,
                verify,
                textBox1.Text,
                SiteName, 
                dtpSearchFrom.Value,
                dtpSearchUntil.Value);
            CalcLastPage();
            List<Bet> Bets = new List<Bet>();
            for (int i = page * NumPerPage; i < (page + 1) * NumPerPage && i < Data.Length; i++)
            {
                Bets.Add(Data[i]);
            }
            BindingSource bs = new BindingSource();
            bs.DataSource = Bets;
            dgvBets.DataSource = bs;
        }

        void CalcLastPage()
        {
            if (Data != null)
            {
                lastPage = (int)Math.Ceiling((decimal)Data.Length / (decimal)NumPerPage);
                cmbJumpTo.Items.Clear();
                for (int i = 0; i < lastPage; i++)
                {
                    cmbJumpTo.Items.Add(i + 1);
                }
                if (cmbJumpTo.Items.Count > 0)
                    cmbJumpTo.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Could not read from DB. Please verify the file dicebot.db exists.");
            }
        }

        private void cmbViewPerPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            page = 0;
            NumPerPage = int.Parse(cmbViewPerPage.SelectedItem.ToString());
            
            
            if (Data != null)
            {
                CalcLastPage();
                List<Bet> Bets = new List<Bet>();
                for (int i = page * NumPerPage; i < (page + 1) * NumPerPage && i < Data.Length; i++)
                {
                    Bets.Add(Data[i]);
                }
                BindingSource bs = new BindingSource();
                bs.DataSource = Bets;
                dgvBets.DataSource = bs;
            }
        }



        private void cmbJumpTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            page = int.Parse(cmbJumpTo.SelectedItem.ToString())-1;
            if (Data != null)
            {
                List<Bet> Bets = new List<Bet>();
                for (int i = page * NumPerPage; i < (page + 1) * NumPerPage && i < Data.Length; i++)
                {
                    Bets.Add(Data[i]);
                }
                BindingSource bs = new BindingSource();
                bs.DataSource = Bets;
                dgvBets.DataSource = bs;
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (page > 0)
                cmbJumpTo.SelectedIndex--;

            
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (page < lastPage-1)
                cmbJumpTo.SelectedIndex++ ;
            
        }

       

    }
}
