using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Dyn.Comm
{
    public class RpcException : Exception
    {
        public new string Message;

        public RpcException()
        {
        }

        public RpcException(string message)
            : base(message)
        {
            Message = message;
        }
    }
}
