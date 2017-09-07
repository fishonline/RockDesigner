using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Rock.Dyn.Comm
{
    public class RockWorker:IDisposable
    {
        private VirtuaIP _localVIP;
        private VirtuaIP _nextVIP;
        private string _remoteAddress;
        private string _inprocHost = "inproc://frontend";

        private Thread _thread;
        private bool _isRunning = false;

        private ZmqContext _context;
        private byte[] _emptyFrame = new byte[0];
        private bool _disposed;
        private Poller _poller;

        internal RockWorker(VirtuaIP localVIP, string remoteAddress)
        {
            _localVIP = localVIP;

            _nextVIP = new VirtuaIP()
            {
                Router = localVIP.Router,
                Node = uint.MaxValue  //运算后节点所能取得最大数：16777215
            };

            _context = ZmqContext.Create();
            _remoteAddress = remoteAddress;
        }

        /// <summary>
        /// Finalizes an instance of the RockWorker class.
        /// </summary>
        ~RockWorker()
        {
            Dispose(false);
        }

        public string InprocHost
        {
            get { return _inprocHost; }
        }

        public VirtuaIP LocalIP
        {
            get { return _localVIP; }
        }

        public ZmqContext Context
        {
            get { return _context; }
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;

                _thread = new Thread(ClientReceiver);
                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        private void ClientReceiver()
        {
            using (ZmqSocket socket = _context.CreateSocket(SocketType.ROUTER))
            {
                socket.Identity = _localVIP.ToBytes();
                socket.SendHighWatermark = 10000000;
                socket.Bind(_inprocHost);

                socket.Connect(_remoteAddress);
                socket.ReceiveReady += ReceiveReady;

                //接受数据，如果当前没有数据到达，线程一直阻塞
                _poller = new Poller(new List<ZmqSocket> { socket });

                while (_isRunning)
                {
                    try
                    {
                        _poller.Poll();
                    }
                    catch (ObjectDisposedException)
                    {                        
                        break;
                    }
                }
            }

            //通信服务停止后，释放context资源
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        /// <summary>
        /// 分发接收到的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveReady(object sender, SocketEventArgs e)
        {

            ZmqSocket socket = e.Socket;
            ZmqMessage zmqMessage = socket.ReceiveMessage();
            VirtuaIP destVIP = new VirtuaIP(zmqMessage[2].Buffer);
            //如果目标地址和本地路由一致转发到应用程序 ,如果不一致转发到Router
            if (destVIP.Router == _localVIP.Router && destVIP.Node == _localVIP.Node)
            {
                if (zmqMessage.FrameCount == 6)
                {
                    /*
                     * 把收到的本地数据转发到应用程序--共六帧数据
                     * 0、目的端口
                     * 1、空帧
                     * 2、源地址
                     * 3、目的端口
                     * 4、源端口
                     * 5、数据
                     * */
                    socket.SendMore(zmqMessage[3].Buffer);
                    socket.SendMore(zmqMessage[1].Buffer);
                    socket.SendMore(zmqMessage[2].Buffer);
                    socket.SendMore(zmqMessage[3].Buffer);
                    socket.SendMore(zmqMessage[4].Buffer);
                    socket.Send(zmqMessage[5].Buffer);
                }
                else if (zmqMessage.FrameCount == 7)
                {
                    /*
                     * 把收到的远端数据转发到应用程序--共五帧数据
                     * 0、目的端口
                     * 1、空帧
                     * 2、源地址
                     * 3、目的端口
                     * 4、源端口
                     * 5、数据
                     * */
                    socket.SendMore(zmqMessage[4].Buffer);
                    socket.SendMore(zmqMessage[1].Buffer);
                    socket.SendMore(zmqMessage[3].Buffer);
                    socket.SendMore(zmqMessage[4].Buffer);
                    socket.SendMore(zmqMessage[5].Buffer);
                    socket.Send(zmqMessage[6].Buffer);
                }
                //不符合要求的帧丢弃
            }
            else if (zmqMessage.FrameCount == 6)
            {
                /*
                 * 转发到Router--共七帧数据
                 * 0、下一跳地址
                 * 1、空帧
                 * 2、目的地址
                 * 3、源地址
                 * 4、目的端口
                 * 5、源端口
                 * 6、数据
                 * */
                socket.SendMore(_nextVIP.ToBytes());//下一跳地址
                socket.SendMore(zmqMessage[1].Buffer);
                socket.SendMore(zmqMessage[2].Buffer);
                socket.SendMore(_localVIP.ToBytes());
                socket.SendMore(zmqMessage[3].Buffer);
                socket.SendMore(zmqMessage[4].Buffer);
                socket.Send(zmqMessage[5].Buffer);
            }
            //不符合要求的帧丢弃
        }

        /// <summary>
        /// Releases all resources used by the current instance of the RockWorker class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RockWorker, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            _isRunning = false;
            if (disposing)
            {
                //释放拖管对象
            }

            if (!_disposed)
            {
                //使服务关闭

                using (ZmqSocket socket = _context.CreateSocket(SocketType.DEALER))
                {
                    bool isConnected = false;
                    while (!isConnected)
                    {
                        try
                        {
                            socket.Connect(_inprocHost);
                            isConnected = true;
                        }
                        catch
                        {
                            Thread.Sleep(500);
                        }
                    }

                    socket.SendMore(_emptyFrame);
                    socket.SendMore(RockContext.LocalVIP.ToBytes());
                    socket.Send(_emptyFrame);
                }
            }

            _disposed = true;
            if (_poller != null)
            {
                _poller.Dispose();
            }
        }
    }
}