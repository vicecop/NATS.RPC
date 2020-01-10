using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace NATS.RPC.Shared
{
    public class JsonDeserializer : IDeserializer
    {
        public object DeserialzeObject(byte[] data, Type type)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(json, type);
        }

        public object[] DeserialzeObjects(byte[] data, Type[] types)
        {
            var json = Encoding.UTF8.GetString(data);
            var jToken = JToken.Parse(json);

            var objects = new object[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                objects[i] = jToken[i].ToObject(types[i]);
            }

            return objects;
        }
    }
}
