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
        dataset Data;
        Graphics graph;
        bool timestamps = false;
        public Graph()
        {
            InitializeComponent();
            graph = pnlGraph.CreateGraphics();
            drawgraph(graph);
        }

        public Graph(dataset data, bool timestamps)
        {
            InitializeComponent();
            graph = pnlGraph.CreateGraphics();
            Data = data;
            this.timestamps = timestamps;
        }

        void drawgraph(Graphics graph)
        {
            Chart tmp = new Chart();
            tmp.ChartAreas.Add(new ChartArea(""));
            Series bets = new Series("");
            bets.Name = "";

            bets.LegendText = "";
            bets.IsVisibleInLegend = true;
            tmp.ChartAreas[0].AxisX.Minimum = 0;
            tmp.ChartAreas[0].AxisX.Title = "Bets";
            tmp.ChartAreas[0].AxisY.Title = "Profit (Btc)";
            tmp.ChartAreas[0].AxisY2.Title = "Wagered";
            tmp.Legends.Add(bets.Legend);

            for (int i = 0; i < Data.XValues.Length; i++)
            {
                bets.Points.Add(new DataPoint(Data.XValues[i], Data.YValues[i]));
            }
            bets.ChartType = SeriesChartType.Line;
            tmp.Size = new System.Drawing.Size(1500, 800);
            tmp.Series.Add(bets);
            string filename = "tmp.png";
            tmp.SaveImage(filename, ChartImageFormat.Png);
           
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (svdChart.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (Bitmap b = new Bitmap(850, 750))
                {
                    using (Graphics graph = Graphics.FromImage(b))
                    {
                        drawgraph(graph);
                    }
                    b.Save(svdChart.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void pnlGraph_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                graph.DrawImage(new Bitmap("tmp.png"), new Point(0, 0));
            }
            catch
            {

            }
            
        }

        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }

    public class dataset
    {
        public string XAxis { get; set; }

        public string YAxis { get; set; }

        public double[] XValues { get; set; }

        public double[] YValues { get; set; }
    }
}
