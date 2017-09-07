using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace Rock.Dyn.Comm
{
    public class RouterDealer
    {
        protected string _remoteAddress;
        protected ushort _vPort = 0;

        protected ZmqContext _context;
        protected ZmqSocket _socket;
        //Router的虚拟IP , 远端连接的地址, Router的ZmqContext
        public RouterDealer(ushort vPort, string remoteAddress, ZmqContext context)
        {
            _vPort = vPort;
            _remoteAddress = remoteAddress;
            _context = context;
        }

        public void Start()
        {
            ClientTask();
        }

        private  void ClientTask()
        {
            //创建DEALER类型的socket
            _socket = _context.CreateSocket(SocketType.DEALER);
            _socket.Identity = BitConverter.GetBytes(_vPort);
            _socket.SendHighWatermark = 100000;

            //连接client,有可能发送连接请求时client还没有完成初始化，造成连接失败
            bool isConnected = false;
            while (!isConnected)
            {
                try
                {
                    //连接到远端的地址
                    _socket.Connect(_remoteAddress);
                    isConnected = true;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        public ZmqSocket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
