using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;
using Rock.Dyn.Core;
using Rock.Dyn.Msg;

namespace Rock.Dyn.Comm
{
    public class RpcServer : IDisposable
    {
        private byte[] _emptyFrame = new byte[0];
        private ZmqContext _context;
        private ushort _localPort;

        private bool _isRunning;
        private bool _isConnected;
        private bool _disposed;
        private string _inprocHost = "inproc://frontend";
        private Queue _socketQueue = null;

        internal RpcServer(ushort localPort, ZmqContext context)
        {
            if (context == null)
                throw new ApplicationException("创建RpcServer时context为null");

            _context = context;
            _localPort = localPort;
        }

        /// <summary>
        /// Finalizes an instance of the RpcServer class.
        /// </summary>
        ~RpcServer()
        {
            Dispose(false);
        }

        public ushort LocalPort
        {
            get { return _localPort; }
            set { _localPort = value; }
        }

        public void Start()
        {
            CreateSocketPool(300);
            if (!_isRunning)
            {
                _isRunning = true;

                Thread thread = new Thread(Recevier);
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                throw new ApplicationException("RpcServer已经运行");
            }
        }

        private void CreateSocketPool(int count)
        {
            _socketQueue = Queue.Synchronized((new Queue()));
            ZmqSocket socket = null;
            for (int i = 0; i < count; i++)
            {
                socket = _context.CreateSocket(SocketType.DEALER);
                bool isConnected = false;
                while (!isConnected)
                {
                    try
                    {
                        socket.Connect(_inprocHost);
                        _socketQueue.Enqueue(socket);
                        isConnected = true;
                    }
                    catch
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }

        private void Recevier()
        {
            using (ZmqSocket monitorSocket = _context.CreateSocket(SocketType.DEALER))
            {
                monitorSocket.Identity = BitConverter.GetBytes(_localPort);
                monitorSocket.SendHighWatermark = 10000000;

                _isConnected = false;
                while (!_isConnected)
                {
                    try
                    {
                        monitorSocket.Connect(_inprocHost);
                        _isConnected = true;
                    }
                    catch
                    {
                        Thread.Sleep(500);
                    }
                }

                while (_isRunning)
                {
                    ZmqMessage zmqMessage = monitorSocket.ReceiveMessage();

                    // 执行Rpc调用任务
                    Action<object> dealRpcAction = DealMessage;
                    Task.Factory.StartNew(dealRpcAction, zmqMessage);
                }
            }
        }

        /// <summary>
        /// 重载父类方法
        /// </summary>
        /// <param name="msgData">接收到的消息</param>
        private void DealMessage(object obj)
        {
            ZmqMessage zmqMessage = obj as ZmqMessage;
            //记录源地址和端口
            VirtuaIP originVIP = new VirtuaIP(zmqMessage[1].Buffer);
            ushort originPort = (ushort)BitConverter.ToInt16(zmqMessage[3].Buffer, 0);
            byte[] requestData = zmqMessage[zmqMessage.FrameCount - 1].Buffer;

            if (requestData != null && requestData.Length >= 4)
            {
                TSerializer serializer = new TBinarySerializer();
                serializer.FromBytes(requestData);

                // 获取消息
                TMessage msg = serializer.ReadMessageBegin();

                if (msg.Type == TMessageType.RpcCall || msg.Type == TMessageType.RpcOneway)
                {
                    //获取方法信息构造Method
                    string[] temp = msg.Name.Split('_');
                    DynMethodInstance methodInstance = null;
                    try
                    {
                        methodInstance = DynSerialize.ReadDynMethodInstance(serializer, temp[0], temp[1]);
                        serializer.ReadMessageEnd();
                        serializer.Flush();

                        Dictionary<string, object> paramValues = new Dictionary<string, object>();
                        //获取参数信
                        foreach (string paramName in methodInstance.DynMethod.GetParameterNames())
                        {
                            paramValues[paramName] = methodInstance[paramName];
                        }

                        string className = InterfaceImplementMap.InterfaceAndImplementMap[temp[0]];
                        string methodFullName = className + "_" + temp[1];
                        //方法调用
                        object ret = DynTypeManager.MethodHandler(null, methodFullName, paramValues);

                        if (methodInstance.DynMethod.Result.DynType != DynType.Void)
                        {
                            methodInstance.Result = ret;
                        }

                        // 序列化返回结果
                        serializer = new TBinarySerializer();
                        TMessage respMsg = new TMessage(msg.Name, TMessageType.RpcReply, msg.MsgID, msg.LiveLife, "", "", "", "");
                        serializer.WriteMessageBegin(respMsg);
                        DynSerialize.WriteResult(serializer, methodInstance);
                        serializer.WriteMessageEnd();
                        byte[] respData = serializer.ToBytes();
                        serializer.Flush();

                        // 返回客户端
                        Send(originVIP, originPort, respData);
                    }
                    catch (Exception ex)
                    {
                        serializer.ReadMessageEnd();
                        serializer.Flush();

                        //如果服务端需要记录日志，则由本事件把异常信息发出
                        RaiseDealMessageExceptionEvent(ex);

                        byte[] returnData = ReturnExceptionToClient("RpcServer.DealMessageException", msg, ex.Message);
                        // 返回客户端
                        Send(originVIP, originPort, returnData);
                        return;
                    }
                }
            }
        }
        //destVIP 为接收消息的originVIP destPort为接收消息的originPort
        private void Send(VirtuaIP destVIP, ushort destPort, byte[] data)
        {
            //轮流使用队列中的socket(初始化时是300个)
            ZmqSocket sendSocket = _socketQueue.Dequeue() as ZmqSocket;

            /*
             * 发送数据--共三帧
             * 0、空帧
             * 1、目的地址
             * 2、目的端口
             * 3、源端口
             * 4、数据
             * */
            sendSocket.SendMore(_emptyFrame);
            sendSocket.SendMore(destVIP.ToBytes());
            sendSocket.SendMore(BitConverter.GetBytes(destPort));
            sendSocket.SendMore(BitConverter.GetBytes(_localPort));
            sendSocket.Send(data);
            _socketQueue.Enqueue(sendSocket);
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
        /// 把异常信息传递到客户端
        /// </summary>
        /// <param name="exceptionSource">错误源</param>
        /// <param name="msg">正常的消息头</param>
        /// <param name="exceptionMsg">异常信息</param>
        private byte[] ReturnExceptionToClient(string exceptionSource, TMessage msg, string exceptionMsg)
        {
            //把异常信息传递到客户端
            TSerializer serializer = new TBinarySerializer();
            TMessage exceptMsg = new TMessage(exceptionSource, TMessageType.Exception, msg.MsgID, msg.LiveLife, "", "", "", "");
            serializer.WriteMessageBegin(exceptMsg);

            TApplicationException tAppException = new TApplicationException(TApplicationException.ExceptionType.Unknown, exceptionMsg);
            tAppException.Write(serializer);

            serializer.WriteMessageEnd();

            //如果不是单向的，把异常返回，并跳出处理函数
            byte[] respException = serializer.ToBytes();
            serializer.Flush();

            // 返回异常
            return respException;
        }

        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the RpcServer class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RpcServer, and optionally disposes of the managed resources.
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
                //已经不运行
                _isRunning = false;
                //默认连接上
                _isConnected = true;
                Send(RockContext.LocalVIP, _localPort, _emptyFrame);//使服务关闭

                //关闭socket池
                for (int i = _socketQueue.Count; i > 0; i--)
                {
                    ZmqSocket socket = _socketQueue.Dequeue() as ZmqSocket;
                    socket.Dispose();
                }

                RockContext.UnregisterPort(_localPort);
            }

            _disposed = true;
        }
    }
}
