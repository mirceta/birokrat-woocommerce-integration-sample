using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BirokratNext.Utils
{
    public static class Serializer
    {
        public static string ToJson(object data, bool indented = false)
        {
            return JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);
        }

        public static T FromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static T FromJson<T>(object data)
        {
            return ((JObject) data).ToObject<T>();
        }

        public static JObject FromJsonAnonymous(string data)
        {
            return JObject.Parse(data);
        }
    }
}
