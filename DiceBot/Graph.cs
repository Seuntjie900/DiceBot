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
    public partial class Graph : Form
    {
        dataset Data;
        Graphics graph;
        bool timestamps = false;
        public Graph()
        {
            InitializeComponent();
            graph = pnlGraph.CreateGraphics();
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
            graph.Clear(Color.White);
            double xlargest = getlargest(Data.XValues, true);
            double ylargest = getlargest(Data.YValues, true);
            double xsmallest = getlargest(Data.XValues, false);
            double ysmallest = getlargest(Data.YValues, false);
            double xfactor = xlargest / 650.0;
            double yfactor = ylargest / 325.0;
            //double revysmal = (ysmallest<0) ? 0-ysmallest:ysmallest;
            double yrange = (ylargest -ysmallest);
            double xfactor2 = (xlargest - xsmallest) / 650.0;
            double yfactor2 = yrange / 650;
            if (xsmallest < 0 && xlargest > 0)
            {
                if ((0 - xsmallest) > xlargest)
                {
                    xfactor = (0-xsmallest) / 725.0;
                }
            }
            if (ysmallest < 0 && ylargest > 0)
            {
                if ((0 - ysmallest) > ylargest)
                {
                    yfactor = (0-ysmallest) / 325.0;
                    //ylargest = 0.0 - ysmallest;
                }
            }
            
            Point origin = new Point(100, 375);
            Font f = new System.Drawing.Font(FontFamily.GenericMonospace, 8);
            Pen outline = new Pen(Color.Black, 2);
            graph.DrawLine(outline, new Point(100,675), new Point(100, 25));
            graph.DrawLine(outline, new Point(100,675), new Point(750 , 675));
            graph.DrawString(Data.XAxis, new Font(FontFamily.GenericMonospace, 12), Brushes.Black, 725, 680);
            graph.DrawString(Data.YAxis.PadLeft(10, ' '), new Font(FontFamily.GenericMonospace, 12), Brushes.Black, 15, 5);

            for (int i = 150; i < 750; i += 50)
            {
                for (int j = 30; j < 680; j += 10)
                {
                    graph.FillEllipse(Brushes.Gray, i, j, 2, 2);                    
                }
                double val = 0;

                double num3 = (double)(i-100) / 50.0;
                double num2 = (13-num3) / 13;
                double num1 = xlargest;
                val = (num1 - (num2 * (xlargest - xsmallest)));

                string msg = "";
                int writeheight = 680;
                if (!timestamps)
                {
                    msg = val.ToString("0");

                    if (val >= 1000 || val <= -1000)
                    {
                        msg = (val / 1000.0).ToString("0.00") + "K";
                    }
                    if (val >= 1000000 || val <= -1000000)
                    {
                        msg = (val / 1000000.0).ToString("0.00") + "M";
                    }
                }
                else
                {
                    DateTime dt = new DateTime((long)val);
                    msg = dt.ToShortDateString() + "\n" + dt.ToShortTimeString();
                    if (num3 % 2 == 0)
                        msg = "";
                }
                graph.DrawString(msg.PadRight(15), new Font(FontFamily.GenericMonospace, 7), Brushes.Black, i-25, writeheight);
            }
            for (int i = 25; i < 680; i += 50)
            {
                for (int j = 100; j < 750; j += 10)
                {
                    graph.FillEllipse(Brushes.Gray, j, i, 2, 2);
                }
                double val = 0;
                
                    double num3=(double)(i+25)/50.0;
                    double num2=(num3-1)/13;
                    double num1 = ylargest;
                    val = (num1 -(num2*yrange));
                
                string msg = val.ToString("0.00000000");
                if (val >= 1000 || val<= -1000)
                {
                    msg = (val / 1000.0).ToString("0.00") + "K";
                }
                if (val >= 1000000 || val <= -1000000)
                {
                    msg = (val / 1000000.0).ToString("0.00") + "M";
                }
                graph.DrawString(msg.PadLeft(14, ' '), new Font(FontFamily.GenericMonospace, 8), Brushes.Black, 0, i);
            }

            Point previous = origin;
            for (int i = 0; i < Data.XValues.Length; i++)
            {
                Point point = new Point();
                point.Y = (int)( (ysmallest/yfactor2)+ 675-(Data.YValues[i]/yfactor2));
                point.X = (int)((Data.XValues[i]/xfactor2)+101-(xsmallest/xfactor2));
                if (previous == origin)
                    previous = point;
                graph.DrawLine(new Pen(Color.Red, 1), point.X, point.Y, previous.X, previous.Y);
                previous = point;
            }

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
            drawgraph(graph);
        }

        private double getlargest(double[] numbers, bool getLargest)
        {
            double largest = 0;
            bool start = true;
            foreach (double d in numbers)
            {
                if (start)
                {
                    largest = d;
                    start = false;
                }
                else
                {
                    if (getLargest)
                    {
                        if (d > largest)
                        {
                            largest = d;
                        }
                    }
                    else
                    {
                        if (d < largest)
                        {
                            largest = d;
                        }
                    }
                }
            }
            return largest;

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
