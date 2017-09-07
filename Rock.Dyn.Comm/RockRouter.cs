using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using ZeroMQ;

namespace Rock.Dyn.Comm
{
    public delegate void AnalysisHandler(string type, long count);

    public class RockRouter
    {
        private VirtuaIP _localVIP = null;
        private Thread _thread;
        private bool _isRuning = false;
        //private long count = 0;

        private Dictionary<ushort, string> _remoteAddress = new Dictionary<ushort, string>();
        private Dictionary<ushort, ushort> _nextTripRouterDic = new Dictionary<ushort, ushort>();
        private Dictionary<ushort, RouterDealer> _dealers = new Dictionary<ushort, RouterDealer>();

        public AnalysisHandler DataAnalysis;
        public DataReceived OnDataReceived;
        private string _routerHost = null;
        private Poller _poller;

        public RockRouter(ushort router, ushort port)
        {
            _localVIP = new VirtuaIP()
            {
                Router = router,
                Node = uint.MaxValue
            };

            //绑定本地地址
            _routerHost = "tcp://*:" + port.ToString();
        }

        public Dictionary<ushort, ushort> NextTripRouterDic
        {
            get { return this._nextTripRouterDic; }
            set { this._nextTripRouterDic = value; }
        }

        public Dictionary<ushort, RouterDealer> Dealers
        {
            get { return this._dealers; }
        }

        public void Start()
        {
            if (!_isRuning)
            {
                _isRuning = true;

                _thread = new Thread(ServerTask);
                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        public void Stop()
        {
            if (_isRuning)
                _isRuning = false;
        }

        private void ServerTask()
        {
            using (ZmqContext context = ZmqContext.Create())
            {
                using (ZmqSocket socket = context.CreateSocket(SocketType.ROUTER))
                {
                    socket.Identity = _localVIP.ToBytes();
                    socket.SendHighWatermark = 100000;
                    socket.Bind(_routerHost);
                    socket.ReceiveReady += ReceiveReady;

                    //建立本地路由和相邻远端路由连接的dealer
                    foreach (KeyValuePair<ushort, string> address in _remoteAddress)
                    {
                        //第一个构造函数参数:本地的RouterID左移8位加上远端的RouterID作为dealer的Identity 唯一标识
                        //第二个构造函数参数是远端的地址,第三个参数是通讯上下文
                        RouterDealer dealer = new RouterDealer((ushort)((_localVIP.Router << 8) + address.Key), address.Value, context);
                        dealer.Start();

                        _dealers.Add(address.Key, dealer);
                    }

                    //接受数据，如果当前没有数据到达，线程一直阻塞
                    _poller = new Poller(new List<ZmqSocket> { socket });

                    while (_isRuning)
                    {
                        _poller.Poll();
                    }

                    //收到停止命令后（外界调用Stop() _isRuning变为false），释放资源
                    RouterDealer[] routerDealer = _dealers.Values.ToArray<RouterDealer>();
                    for (int i = routerDealer.Length - 1; i >= 0; i--)
                    {
                        routerDealer[i].Dispose();
                    }
                    _dealers.Clear();
                }
            }
        }

        private Dictionary<uint, string> _checkVIP = new Dictionary<uint, string>();

        /// <summary>
        /// 获取节点号
        /// </summary>
        /// <returns></returns>
        private uint GetNode()
        {
            uint i = 1;

            while (true)
            {
                if (_checkVIP.ContainsKey(i))
                {
                    i++;
                }
                else
                {
                    break;
                }
            }

            return i;
        }

        /// <summary>
        /// 根据消息的目的地址，对消息进行转发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveReady(object sender, SocketEventArgs e)
        {
            ZmqSocket socket = e.Socket;
            ZmqMessage zmqMessage = socket.ReceiveMessage();

            //不符合要求的包直接丢掉
            if (zmqMessage.FrameCount != 7)
                return;

            VirtuaIP destVIP = new VirtuaIP(zmqMessage[2].Buffer);

            #region 统计收到的数据包数量，转发速度p/s(每秒多少个包)，并通知主线程更新界面
            //byte[] data = zmqMessage[6].Buffer;
            //byte flage = data[0];
            //switch (flage)
            //{
            //    case 0: count++; count = 1; break;
            //    case 1: count++; break;
            //    case 2: count++; ; break;
            //}
            //if (DataAnalysis != null)
            //    DataAnalysis("in", count);

            #endregion

            #region 转发数据

            if (destVIP.Router == _localVIP.Router)
            {
                if (destVIP.Node == _localVIP.Node)
                {
                    //Router自己处理
                    Console.WriteLine("我的消息：{0}", Encoding.Unicode.GetString(zmqMessage[5].Buffer));
                }
                else
                {
                    /*
                     * 接收数据--共六帧数据
                     * 0、目的地址
                     * 1、空帧
                     * 2、目的地址
                     * 3、源地址
                     * 4、目的端口
                     * 5、源端口
                     * 6、数据
                     * */
                    socket.SendMore(zmqMessage[2].Buffer);
                    socket.SendMore(zmqMessage[1].Buffer);
                    socket.SendMore(zmqMessage[2].Buffer);
                    socket.SendMore(zmqMessage[3].Buffer);
                    socket.SendMore(zmqMessage[4].Buffer);
                    socket.SendMore(zmqMessage[5].Buffer);
                    socket.Send(zmqMessage[6].Buffer);

                    //如果推断不错的话,这里应该是发往本机的应用程序服务器端
                }
            }
            else
            {
                /*
                 * 转发到Router--共七帧数据
                 * 0、新的下一跳Router地址
                 * 1、空帧
                 * 2、目的地址
                 * 3、源地址
                 * 4、目的端口
                 * 5、源端口
                 * 6、数据
                 * */

                //转发的下一个Router
                //得到下一跳地址
                ushort nodeID = destVIP.Router;
                //TODO 此处可能得不到下一跳节点，因为目的节点不可到达,异常捕获比一般的判断速度慢10倍
                ushort nextRouterID = _nextTripRouterDic[nodeID];

                //得到连接到下一跳router的dealer
                RouterDealer dealer = _dealers[nextRouterID];

                //dealer.Socket.SendMore(nextRouterVIP.ToBytes());//下一跳地址
                dealer.Socket.SendMore(zmqMessage[1].Buffer);
                dealer.Socket.SendMore(zmqMessage[2].Buffer);
                dealer.Socket.SendMore(zmqMessage[3].Buffer);
                dealer.Socket.SendMore(zmqMessage[4].Buffer);
                dealer.Socket.SendMore(zmqMessage[5].Buffer);
                dealer.Socket.Send(zmqMessage[6].Buffer);

                //if (DataAnalysis != null)
                //    DataAnalysis("out", count);
            }

            #endregion 转发数据
        }

        /// <summary>
        /// 添加本地路由要连接的远端路由的真实ip地址
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="address"></param>
        public void AddRemoteAddress(ushort nodeID, string address)
        {
            _remoteAddress.Add(nodeID, address);
        }

        public void Dispose()
        {
            _isRuning = false;
            if (_poller != null)
            {
                _poller.Dispose();
            }
        }
    }
}