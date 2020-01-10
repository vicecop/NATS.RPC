using Newtonsoft.Json;
using System.Text;

namespace NATS.RPC.Shared
{
    public class JsonSerializer : ISerializer
    {
        public byte[] SerializeObject(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public byte[] SerializeObjects(object[] objs)
        {
            return SerializeObject(objs);
        }
    }
}
