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
    public partial class Custom_Chart : Form
    {
        public int  ChartType { get; set; }
        public long StartID { get { return (long)nudGraphStartBetID.Value; } }
        public DateTime StartDate { get { return dtpStart.Value; } }
        public DateTime EndDate { get { return dtpEnd.Value; } }
        public Custom_Chart()
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnChartBetID_Click(object sender, EventArgs e)
        {
            ChartType = 1;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnChartTimeRange_Click(object sender, EventArgs e)
        {
            ChartType = 2;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
