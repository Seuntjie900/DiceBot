using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Http;

namespace DiceBotCore.Helpers
{
    public class Hash
    {
        public static string HMAC512(string Message, string Key)
        {
            HMACSHA512 betgenerator = new HMACSHA512();
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < Key.Length; i++)
            {
                serverb.Add(Convert.ToByte(Key[i]));
            }

            betgenerator.Key = serverb.ToArray();
            List<byte> buffer = new List<byte>();

            foreach (char c in Message)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string SHA256(string Message)
        {
            SHA256Cng HashGen = new SHA256Cng();
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < Message.Length; i++)
            {
                serverb.Add(Convert.ToByte(Message[i]));
            }
            byte[] hash = HashGen.ComputeHash(serverb.ToArray());
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
            
        }
    }
}
