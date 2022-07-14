using System;

using System.Security.Cryptography;

namespace DiceBot.Core
{
    public class DBRandom
    {

        RandomNumberGenerator r = RandomNumberGenerator.Create();
        const string chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";
        public virtual uint Next(uint max)
        {
            byte[] bytes = new byte[4];
            r.GetBytes(bytes);
            uint result = BitConverter.ToUInt32(bytes, 0);
            return (result % max);
        }
        public virtual uint Next(uint min, uint max)
        {
            uint result = Next(max - min);
            return (min + result);
        }
        public virtual int Next(int min, int max)
        {
            return min + Next(max - min);
        }
        public virtual int Next(int max)
        {
            byte[] bytes = new byte[4];
            r.GetBytes(bytes);
            int result = BitConverter.ToInt32(bytes, 0);
            return (Math.Abs(result % max));
        }
        public virtual int Next()
        {

            return Next(int.MaxValue);
        }
        public string RandomString(int length)
        {
            string x = "";
            while (x.Length > 0)
            {
                x += chars[Next(0, chars.Length)];
            }
            return x;
        }
    }
}
