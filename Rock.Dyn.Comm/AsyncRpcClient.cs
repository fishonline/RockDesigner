using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rock.Dyn.Core;
using Rock.Dyn.Msg;
using ZeroMQ;

namespace Rock.Dyn.Comm
{
    internal class AsyncRpcClient : IDisposable
    {
        private byte[] _emptyFrame = new byte[0];
        private bool _disposed;
        private string _inprocHost = "inproc://frontend";
        private ushort _sendVPort;
        private ushort _receiverVPort;

        private ZmqContext _context;
        private ZmqSocket _sendDealer;
        private ZmqSocket _receiverDealer;

        public DataReceived OnDataReceived = null;
        private bool _isRuning = false;
        private Poller _poller;

        public AsyncRpcClient(ushort vPort, ZmqContext context)
        {
            if (context == null)
                throw new ApplicationException("创建RpcClient时context为null");
            _context = context;

            _sendVPort = (ushort)(vPort + 1);
            _receiverVPort = vPort;
        }

        ~AsyncRpcClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// 启动RpClient
        /// </summary>
        public void Start()
        {
            _isRuning = true;
            ReceiveTask();
            SendTask();
        }

        public void Stop()
        {
            Dispose();
        }

        protected void SendTask()
        {
            //发送数据的task所在线程有用户决定
            _sendDealer = _context.CreateSocket(SocketType.DEALER);
            _sendDealer.Identity = BitConverter.GetBytes(_sendVPort);
            _sendDealer.SendHighWatermark = 100000;

            //连接client,有可能发送连接请求时client还没有完成初始化，造成连接失败
            bool isConnected = false;
            while (!isConnected)
            {
                try
                {
                    _sendDealer.Connect(_inprocHost);
                    isConnected = true;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        protected void ReceiveTask()
        {
            //另启动一线程专门负责接收数据
            Thread thread = new Thread(() =>
            {
                _receiverDealer = _context.CreateSocket(SocketType.DEALER);
                _receiverDealer.Identity = BitConverter.GetBytes(_receiverVPort);
                _receiverDealer.SendHighWatermark = 100000;
                _receiverDealer.ReceiveReady += ReceiveReady;

                //连接client,有可能发送连接请求时client还没有完成初始化，造成连接失败
                bool isConnected = false;
                while (!isConnected)
                {
                    try
                    {
                        _receiverDealer.Connect(_inprocHost);
                        isConnected = true;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }

                //接受数据，如果当前没有数据到达，线程一直阻塞
                _poller = new Poller(new List<ZmqSocket> { _receiverDealer });

                while (_isRuning)
                {
                    _poller.Poll();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 远程方法调用
        /// </summary>
        /// <param name="recvAddress">接收节点的虚拟IP地址，如3.1/param>
        /// <param name="interfaceName">接口名称</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="paramDict">参数字段</param>
        /// <param name="messageType">消息类型</param>
        public void Call(string recvAddress, ushort destVPort, string interfaceName, string methodName, Dictionary<string, object> paramDict, TMessageType messageType = TMessageType.RpcCall)
        {
            try
            {
                VirtuaIP receiverVIP = new VirtuaIP(recvAddress);
                string msgID = Guid.NewGuid().ToString();

                if (!InterfaceImplementMap.InterfaceAndImplementMap.ContainsKey(interfaceName))
                {
                    throw new ApplicationException("找不到接口" + interfaceName + "的具体实现！！");
                }

                //本地应用端口随机获取，目标端口确定
                TMessage reqMsg = new TMessage(interfaceName + "_" + methodName, messageType, msgID, -1, "", "", "", "");
                DynMethodInstance dynMethodInstance = new DynMethodInstance(interfaceName, methodName);
                TSerializer serializerReq = new TBinarySerializer();

                serializerReq.WriteMessageBegin(reqMsg);
                foreach (var para in paramDict)
                {
                    dynMethodInstance[para.Key] = para.Value;
                }

                DynSerialize.WriteDynMethodInstance(serializerReq, dynMethodInstance);
                serializerReq.WriteMessageEnd();
                byte[] reqData = serializerReq.ToBytes();

                serializerReq.Flush();

                Send(receiverVIP, destVPort, reqData);
            }
            catch (Exception ex)
            {
                this.RaiseDealMessageExceptionEvent(ex);
                throw new ApplicationException(ex.Message);
            }
        }

        private void Send(VirtuaIP destVIP, ushort destPort, byte[] data)
        {
            /*
             * 发送数据--共三帧
             * 0、空帧
             * 1、目的地址
             * 2、目的端口
             * 3、源端口
             * 4、数据
             * */

            _sendDealer.SendMore(_emptyFrame);
            _sendDealer.SendMore(destVIP.ToBytes());
            _sendDealer.SendMore(BitConverter.GetBytes(destPort));
            _sendDealer.SendMore(BitConverter.GetBytes(_receiverVPort));
            _sendDealer.Send(data);
        }

        public void Send(string recvAddress, ushort destVPort, string data)
        {
            VirtuaIP receiverVIP = new VirtuaIP(recvAddress);
            Send(receiverVIP, destVPort, Encoding.UTF8.GetBytes(data));
        }


        /// <summary>
        /// 处理接受数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ReceiveReady(object sender, SocketEventArgs e)
        {
            ZmqSocket socket = e.Socket;
            ZmqMessage zmqMessage = socket.ReceiveMessage();

            /*
             * 接收数据--共三帧
             * 1、源地址（发送数据的地址 路由号.节点号）
             * 2、端口
             * 3、数据
             * 这里的接收数据是和发送数据相一致的见上面的Send方法 */
            VirtuaIP originVIP = new VirtuaIP(zmqMessage[1].Buffer);
            ushort originPort = (ushort)BitConverter.ToInt16(zmqMessage[3].Buffer, 0);

            object resultMsg = DealRepMessage(zmqMessage[4].Buffer);

            if (OnDataReceived != null)
                OnDataReceived(resultMsg);
        }

        /// <summary>
        /// 处理返回消息
        /// </summary>
        /// <param name="msgData">返回消息</param>
        private object DealRepMessage(byte[] msgData)
        {
            TSerializer serializerResp = new TBinarySerializer();
            serializerResp.FromBytes(msgData);
            TMessage respMsg = serializerResp.ReadMessageBegin();

            if (respMsg.Type == TMessageType.Exception)
            {
                this.RaiseDealMessageExceptionEvent(TApplicationException.Read(serializerResp));

                serializerResp.ReadMessageEnd();
                serializerResp.Flush();

                return null; ;
            }

            if (respMsg.Type != TMessageType.RpcReply)
            {
                this.RaiseDealMessageExceptionEvent(new ApplicationException("非法的返回类型：" + respMsg.Type.ToString()));

                serializerResp.ReadMessageEnd();
                serializerResp.Flush();

                return null;
            }

            string[] temp = respMsg.Name.Split('_');
            DynMethodInstance dynMethodInstance = new DynMethodInstance(temp[0], temp[1]);

            try
            {
                dynMethodInstance.Result = DynSerialize.ReadResult(serializerResp, dynMethodInstance);
            }
            catch (Exception ex)
            {
                this.RaiseDealMessageExceptionEvent(ex);

                serializerResp.ReadMessageEnd();
                serializerResp.Flush();
            }

            // 消息结束
            serializerResp.ReadMessageEnd();
            serializerResp.Flush();
            return dynMethodInstance.Result;
        }

        /// <summary>
        /// 向外部传递异常信息
        /// </summary>
        /// <param name="e"></param>
        private void RaiseDealMessageExceptionEvent(System.Exception e)
        {
            //if (DealMessageExceptionEvent != null)
            //{
            //    ///发出异常事件
            //    DealMessageExceptionEvent(this, e);
            //}
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncRpcClient"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AsyncRpcClient"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //释放拖管对象
                }
                //释放非托管对象
                _isRuning = false;
                if (_poller != null)
                {
                    _poller.Dispose();
                }
                if (_sendDealer != null)
                {
                    _sendDealer.Dispose();
                }
                if (_receiverDealer != null)
                {
                    _receiverDealer.Dispose();
                }
            }

            _disposed = true;
        }
    }
}