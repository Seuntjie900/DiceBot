using System;
using System.Collections.Generic;
using System.IO;
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
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new cLogin());
                Application.Run(new cDiceBot(args));
            }
            catch (Exception e)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText("DICEBOTLOG.txt"))
                    {
                        sw.WriteLine("########################################################################################\r\n\r\nFATAL CRASH\r\n");
                        sw.WriteLine(e.ToString());
                        sw.WriteLine("########################################################################################");
                    }
                }
                catch { }
                throw e;

            }
        }
    }
}
