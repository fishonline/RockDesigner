using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;
using Rock.Dyn.Core;
using Rock.Dyn.Msg;

namespace Rock.Dyn.Comm
{
    /// <summary>
    /// RpcClient是线程不安全的
    /// </summary>
    public class RpcClient : IDisposable
    {
        private ZmqContext _context;
        private ushort _localPort;
        private ZmqSocket _reqSocket;
        private string _inprocHost = "inproc://frontend";
        private bool _disposed;
        private int _msgID = 0;
        private byte[] _emptyFrame = new byte[0];

        internal RpcClient(ushort localPort, ZmqContext context)
        {

            if (context == null)
                throw new ApplicationException("创建RpcClient时context为null");

            _context = context;
            _localPort = localPort;
        }

        /// <summary>
        /// Finalizes an instance of the RockClient class.
        /// </summary>
        ~RpcClient()
        {
            Dispose(false);
        }

        public void Start()
        {
            if (_reqSocket != null) return;

            _reqSocket = _context.CreateSocket(SocketType.DEALER);
            _reqSocket.Identity = BitConverter.GetBytes(_localPort);

            bool isConnected = false;
            while (!isConnected)
            {
                try
                {
                    _reqSocket.Connect(_inprocHost);
                    isConnected = true;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void Send(VirtuaIP receiverVIP, ushort destVPort, byte[] reqData)
        {
            if (_reqSocket == null)
                throw new ApplicationException("请你先启动RpcClient");

            /*
             * 发送数据--共三帧
             * 1、目的地址
             * 2、目的端口
             * 3、源端口
             * 4、数据
             * */
            _reqSocket.SendMore(_emptyFrame);
            _reqSocket.SendMore(receiverVIP.ToBytes());
            _reqSocket.SendMore(BitConverter.GetBytes(destVPort));
            _reqSocket.SendMore(BitConverter.GetBytes(_localPort));
            _reqSocket.Send(reqData);
        }

        /// <summary>
        /// 远程方法调用
        /// </summary>
        /// <param name="recvAddress">接收节点的虚拟IP地址，如3.1/param>
        /// <param name="interfaceName">接口名称</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="paramDict">参数字段</param>
        /// <param name="liveLife">生命周期,以秒为单位</param>
        /// <param name="messageType">消息类型</param>
        /// <returns>服务端返回的数据或者null</returns>
        public object Call(string recvAddress, ushort destVPort, string interfaceName, string methodName, Dictionary<string, object> paramDict, short liveLife, TMessageType messageType = TMessageType.RpcCall)
        {
            object result = null;
            _msgID = _msgID < int.MaxValue ? ++_msgID : 0;

            try
            {
                VirtuaIP receiverVIP = new VirtuaIP(recvAddress);
                string msgID = _msgID.ToString();

                if (!InterfaceImplementMap.InterfaceAndImplementMap.ContainsKey(interfaceName))
                {
                    throw new ApplicationException("找不到接口" + interfaceName + "的具体实现！");
                }

                DynMethodInstance dynMethodInstance = new DynMethodInstance(interfaceName, methodName);
                TMessage reqMsg = new TMessage(interfaceName + "_" + methodName, messageType, msgID, liveLife, "", "", "", "");
                TSerializer serializerReq = new TBinarySerializer();
                //将消息头信息写入流
                serializerReq.WriteMessageBegin(reqMsg);
                foreach (var para in paramDict)
                {
                    dynMethodInstance[para.Key] = para.Value;
                }
                //将方法的实例写入流
                DynSerialize.WriteDynMethodInstance(serializerReq, dynMethodInstance);

                //什么都不做
                serializerReq.WriteMessageEnd();
                //将消息流转换成二进制流
                byte[] reqData = serializerReq.ToBytes();
                serializerReq.Flush();

                //发送消息
                Send(receiverVIP, destVPort, reqData);

                var timeout = new TimeSpan(0, 0, liveLife);
                //创建一个 Stopwatch 实例：使用 StartNew() 会创建一个 Stopwatch 实例并马上开始计时，即等效于如下代码：
                //Stopwatch sw2 = new Stopwatch();
                //sw2.Start();
                var timer = Stopwatch.StartNew();
                bool isReceive = false;

                do
                {
                    ZmqMessage zmqMessage = _reqSocket.ReceiveMessage(timeout - timer.Elapsed);
                    if (zmqMessage.FrameCount == 0)
                    {
                        throw new TimeoutException("调用超时，请检查网络是否联通！");
                    }
                    else
                    {
                        //zmqMessage[zmqMessage.FrameCount - 1].Buffer 只需要处理zmqMessage中最后一帧,这一帧存的是消息中的数据部分
                        result = DealRepMessage(zmqMessage[zmqMessage.FrameCount - 1].Buffer, msgID, out isReceive);

                        //消息ID不匹配且超时时间未到时，丢掉当前接收到的数据包，继续接受数据
                        if (isReceive)
                        {
                            return result;
                        }
                    }
                }
                while (timer.Elapsed <= timeout);

                throw new TimeoutException("调用超时，请检查网络是否联通！");
            }
            catch (Exception ex)
            {
                this.RaiseDealMessageExceptionEvent(ex);
                throw;
            }
        }

        /// <summary>
        /// 处理返回消息
        /// </summary>
        /// <param name="msgData">返回消息</param>
        private object DealRepMessage(byte[] msgData, string msgID, out bool isReceive)
        {
            //初始化序列化容器
            TSerializer serializerResp = new TBinarySerializer();
            //将数据读入序列化容器
            serializerResp.FromBytes(msgData);
            //从序列化容器中读取消息
            TMessage respMsg = serializerResp.ReadMessageBegin();

            //如果消息的id不对返回空值
            if (msgID != respMsg.MsgID)
            {
                //空方法
                serializerResp.ReadMessageEnd();
                serializerResp.Flush();

                isReceive = false;
                return null;
            }

            //如果是异常消息
            if (respMsg.Type == TMessageType.Exception)
            {
                TApplicationException appException = TApplicationException.Read(serializerResp);
                this.RaiseDealMessageExceptionEvent(appException);

                serializerResp.ReadMessageEnd();
                serializerResp.Flush();

                throw appException;
            }
            //如果消息是远程调用回应
            if (respMsg.Type != TMessageType.RpcReply)
            {
                this.RaiseDealMessageExceptionEvent(new ApplicationException("非法的返回类型：" + respMsg.Type.ToString()));

                serializerResp.ReadMessageEnd();
                serializerResp.Flush();

                isReceive = true;
                return null;
            }

            //构造方法的实例
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

            isReceive = true;
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
        /// Releases all resources used by the current instance of the RpcClient class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RockClient, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //释放拖管对象
                    _inprocHost = null;
                }

                //释放非托管对象
                if (_reqSocket != null)
                {
                    _reqSocket.Dispose();
                    _reqSocket = null;
                }

                RockContext.UnregisterPort(_localPort);
            }

            _disposed = true;
        }

        public void Close()
        {
            Dispose();
        }
    }
}
