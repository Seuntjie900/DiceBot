using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace DiceBot.Common
{
    /// <summary>
    ///     Used for converting from the json data received by site
    ///     Code copied from http://www.codeproject.com/Articles/272335/JSON-Serialization-and-Deserialization-in-ASP-NET
    /// </summary>
    public class JsonUtils
    {
        public static T JsonDeserialize<T>(string jsonString)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                var ser = new DataContractJsonSerializer(typeof(T));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                var obj = (T) ser.ReadObject(ms);

                return obj;
            }
            catch (Exception E)
            {
                throw E;
            }
        }

        public static string JsonSerializer<T>(T t)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var ser = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream();
            ser.WriteObject(ms, t);
            var jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();

            return jsonString;
        }

        public static string ToDateString(DateTime Value)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var dt = Value - DateTime.Parse("1970/01/01 00:00:00", DateTimeFormatInfo.InvariantInfo);
            var mili = (decimal) dt.TotalMilliseconds;

            return ((long) mili).ToString();
        }

        public static DateTime ToDateTime2(string milliseconds)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                var dotNetDate = new DateTime(1970, 1, 1);
                dotNetDate = dotNetDate.AddMilliseconds(long.Parse(milliseconds));

                if (dotNetDate.Year < 1972)
                {
                    dotNetDate = new DateTime(1970, 1, 1);
                    dotNetDate = dotNetDate.AddSeconds(long.Parse(milliseconds));
                }

                return dotNetDate;
            }
            catch
            {
                try
                {
                    var s = milliseconds.ToLower().Replace("z", " ").Replace("t", " ");
                    var dotNetDate = DateTime.Parse(s, DateTimeFormatInfo.InvariantInfo);

                    return dotNetDate;
                }
                catch
                {
                    return new DateTime();
                }
            }
        }

        public static string CurrentDate()
        {
            var dt = DateTime.UtcNow - DateTime.Parse("1970/01/01 00:00:00", CultureInfo.InvariantCulture);
            var mili = dt.TotalMilliseconds;

            return ((long) mili).ToString();
        }
    }
}
