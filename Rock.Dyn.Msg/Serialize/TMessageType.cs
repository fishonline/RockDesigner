using System;

namespace Rock.Dyn.Msg
{
    public enum TMessageType
    {
        RpcCall = 1,
        RpcReply = 2,
        Exception = 3,
        RpcOneway = 4,
        Ftp = 5
    }
}
