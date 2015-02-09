using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
namespace DiceBot
{
    public partial class Graph : Form
    {
        
        decimal Profit = 0;
        long betcount = 0;
        public Graph(Bet[] _Bets)
        {
            InitializeComponent();
            Series Bets;
            Bets = new Series("Profit");
            Bets.ChartType = SeriesChartType.Line;
            Bets.Points.AddXY(0, 0);
            chrtProfitChart.ChartAreas[0].AxisX.Minimum = 0;
            chrtProfitChart.ChartAreas[0].AxisX.Minimum = 0;
            chrtProfitChart.ChartAreas[0].AxisX.Title = "Bets";
            
            chrtProfitChart.ChartAreas[0].AxisY.Title = "Profit (Btc)";
            chrtProfitChart.ChartAreas[0].AxisY2.Title = "Wagered";
            chrtProfitChart.Legends.Add(Bets.Legend);
            if (_Bets!=null)
            { 
                foreach (Bet NewBet in _Bets)
                {
                    Bets.Points.Add(new DataPoint(++betcount, (double)(Profit += NewBet.Profit)));
                }
            }
            chrtProfitChart.Series.Add(Bets);

            //chrtProfitChart.Show();
        }

        public void AddBet(Bet NewBet)
        {
            chrtProfitChart.Series[0].Points.Add(new DataPoint(++betcount, (double)(Profit += NewBet.Profit)));
            //chrtProfitChart.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*.png)|*.png";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = sfd.FileName;
                //chrtProfitChart.SaveImage( sfd.FileName+ ".png", ChartImageFormat.Png);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chrtProfitChart.SaveImage(textBox1.Text+ ".png", ChartImageFormat.Png);
            if (checkBox1.Checked)
            {
                string url = imgur.UploadImage(textBox1.Text+ ".png");
                if (url!="" && url!= null)
                System.Diagnostics.Process.Start(url);
            }
        }
    }
}
