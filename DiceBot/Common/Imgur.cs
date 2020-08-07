using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DiceBot.Common
{
    internal class Imgur
    {
        public static string ClientId = "95911e8a0bad9d7";

        public static string UploadImage(string image)
        {
            var w = new WebClient();
            w.Headers.Add("Authorization", "Client-ID " + ClientId);
            var Keys = new NameValueCollection();

            try
            {
                Keys.Add("image", Convert.ToBase64String(File.ReadAllBytes(image)));
                var responseArray = w.UploadValues("https://api.imgur.com/3/image", Keys);
                dynamic result = Encoding.ASCII.GetString(responseArray);
                var reg = new Regex("link\":\"(.*?)\"");
                Match match = reg.Match(result);
                var url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");

                return url;
            }
            catch
            {
                //MessageBox.Show("Something went wrong. " + s.Message);
                return "0";
            }
        }
    }
}
