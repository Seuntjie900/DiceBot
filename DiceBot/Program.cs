using System;
using System.IO;
using System.Windows.Forms;
using DiceBot.Forms;

namespace DiceBot
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new cDiceBot(args));
            }
            catch (Exception e)
            {
                try
                {
                    using (var sw = File.AppendText("DICEBOTLOG.txt"))
                    {
                        sw.WriteLine("########################################################################################\r\n\r\nFATAL CRASH\r\n");
                        sw.WriteLine(e.ToString());
                        sw.WriteLine("########################################################################################");
                    }
                }
                catch { }
                throw;
            }
        }
    }
}
