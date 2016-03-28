using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceBotCore
{
    class Logger
    {
        public static void DumpLog(Exception E)
        {
            
        }

        public static void Dumplog(string Message)
        {
            try
            {
                using (StreamWriter sw = System.IO.File.AppendText("dicebotlog.log"))
                {

                }
            }
            catch
            {

            }
        }
    }
}
