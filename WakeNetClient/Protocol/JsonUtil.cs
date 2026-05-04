using System.Web.Script.Serialization;

namespace WakeNetClient.Protocol
{
    internal static class JsonUtil
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static string Serialize(object obj) => Serializer.Serialize(obj);

        public static T Deserialize<T>(string json) => Serializer.Deserialize<T>(json);
    }
}

