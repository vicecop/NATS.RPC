namespace NATS.RPC.Proxy
{
    public class ProxyOptions
    {
        public static ProxyOptions Default => new ProxyOptions()
        {
            ServiceUid = "default",
            ConnectionString = "nats://localhost:4222",
            TimeoutMs = 5000
        };

        /// <summary>
        /// Remote service uid
        /// </summary>
        public string ServiceUid { get; set; }
        /// <summary>
        /// Nats url
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Request timeout (seconds)
        /// </summary>
        public int TimeoutMs { get; set; }
    }
}
