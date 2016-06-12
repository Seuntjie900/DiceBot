using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DiceBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new cLogin());
            Application.Run(new cDiceBot(args));
        }
    }
}
