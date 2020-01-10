using System;
using System.Collections.Generic;

namespace NATS.RPC.Shared
{
    public interface IDeserializer
    {
        object DeserialzeObject(byte[] data, Type type);
        object[] DeserialzeObjects(byte[] data, Type[] types);
    }
}
