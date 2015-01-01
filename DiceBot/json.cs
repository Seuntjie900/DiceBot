using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace DiceBot
{
    /// <summary>
    /// Used for converting from the json data received by site
    /// Code copied from http://www.codeproject.com/Articles/272335/JSON-Serialization-and-Deserialization-in-ASP-NET
    /// </summary>
    class json
    {
        public static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }

        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

    }

}
