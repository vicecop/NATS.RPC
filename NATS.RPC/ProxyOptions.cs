using System;
using System.Collections.Generic;
using System.Text;

namespace NATS.RPC
{
    public class ProxyOptions
    {
        public static ProxyOptions Default = new ProxyOptions()
        {
            ServiceUid = "default",
            ConnectionString = "nats://localhost:4222"
        };

        public string ServiceUid { get; set; }
        public string ConnectionString { get; set; }
    }
}
