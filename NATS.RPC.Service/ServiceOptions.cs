namespace NATS.RPC.Service
{
    public class ServiceOptions
    {
        public static ServiceOptions Default => new ServiceOptions()
        {
            ServiceUid = "default",
            ConnectionString = "nats://localhost:4222"
        };

        public string ServiceUid { get; set; }
        public string ConnectionString { get; set; }
    }
}
