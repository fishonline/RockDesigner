using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Dyn.Comm
{
    public class FtpException:Exception
    {
        public new string Message;

        public FtpException()
        {
        }

        public FtpException(string message)
            : base(message)
        {
            Message = message;
        }
    }
}
