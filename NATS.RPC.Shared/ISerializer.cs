namespace NATS.RPC.Shared
{
    public interface ISerializer
    {
        byte[] SerializeObject(object obj);
        byte[] SerializeObjects(object[] objs);
    }
}
