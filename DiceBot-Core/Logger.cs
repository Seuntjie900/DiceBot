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
            try
            {
                string Message = E.Message;
                Message += "\r\n\r\n";
                Message += E.StackTrace;
                if (E.InnerException != null)
                {
                    try
                    {
                        Message += E.InnerException.Message + "\r\n\r\n";
                        Message += E.InnerException.StackTrace;
                    }
                    catch { }
                }
                DumpLog(Message);
            }
            catch
            { }
        }

        public static void DumpLog(string Message)
        {
            try
            {
                using (StreamWriter sw = System.IO.File.AppendText("dicebotlog.log"))
                {
                    sw.WriteLine(string.Format(@"------------------------------------------------------------------------------------------------------
{1} {0}
------------------------------------------------------------------------------------------------------", Message, DateTime.UtcNow));


                }
            }
            catch
            {

            }
        }
    }
}
