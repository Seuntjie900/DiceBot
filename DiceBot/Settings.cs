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
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void btnSaveUser_Click(object sender, EventArgs e)
        {
            this.DialogResult =  System.Windows.Forms.DialogResult.OK;
        }
        public string ching { get; set; }
        public string salarm { get; set; }
        private void btnBrowseChing_Click(object sender, EventArgs e)
        {
            bool ching = ((sender as Button).Name == "btnBrowseChing");
            OpenFileDialog ofdSound = new OpenFileDialog();
            ofdSound.CheckFileExists = true;
            ofdSound.Filter = "MP3|*.mp3|Wave Files|*.wav";
            if (ofdSound.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ching)
                {
                    this.ching = ofdSound.FileName;
                    txtPathChing.Text = this.ching;
                }
                else
                {
                    salarm = ofdSound.FileName;
                    txtPathAlarm.Text = salarm;
                }
            }
        }
    }
}
