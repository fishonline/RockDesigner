using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Rock.Dyn.Comm
{
    /// <summary>
    /// 通信上下文，负责创建rpc，ftp的Client，管理已占用的端口，此类所有的公共方法都是线程安全的
    /// </summary>
    public class RockContext
    {
        private static ConcurrentDictionary<ushort, object> _componentDict = new ConcurrentDictionary<ushort, object>();
        //已经使用的端口号,用于端口冲突检查
        private static List<ushort> _usedPort = new List<ushort>(21);
        private static RockWorker _worker = null;
        private static VirtuaIP _localVIP;
        private static object _lock = new object();

        public static VirtuaIP LocalVIP
        {
            get { return RockContext._localVIP; }
        }

        public static RockWorker CreateRockWorker(ushort router, uint node, string remoteAddress)
        {
            _localVIP = new VirtuaIP()
            {
                Router = router,
                Node = node,
            };

            //加载通信所需DynClass
            DynClassLoader.LoadDynClass();

            if (_worker != null)
            {
                throw new ApplicationException("RockWorker已经创建完成，不需要再次创建");
            }
            _worker = new RockWorker(_localVIP, remoteAddress);
            _worker.Start();
            return _worker;
        }

        public static RpcClient CreateRpcClient(ushort vPort)
        {
            lock (_lock)
            {
                return NewRpcClient(vPort);
            }
        }

        public static RpcClient CreateRpcClient()
        {
            lock (_lock)
            {
                //如果没有端口参数就随机生成
                ushort port = GetAvailablePort();
                return NewRpcClient(port);
            }
        }

        private static RpcClient NewRpcClient(ushort vPort)
        {
            if (CheckPort(vPort))
            {
                throw new ApplicationException(string.Format("端口 ：{0}已经被占用，请选择其他端口！", vPort));
            }
            else
            {
                //创建RpcClient前必须先创建RockWorker
                if (_worker == null)
                {
                    throw new ApplicationException(string.Format("请先创建RockWorker"));
                }
                else
                {
                    RpcClient rpcClient = new RpcClient(vPort, _worker.Context);
                    rpcClient.Start();

                    RegisterPort(vPort);
                    _componentDict.TryAdd(vPort, rpcClient);

                    return rpcClient;
                }
            }
        }

        public static RpcServer CreateRpcServer(ushort vPort)
        {
            if (CheckPort(vPort))
            {
                throw new ApplicationException(string.Format("端口 ：{0}已经被占用，请选择其他端口！", vPort));
            }
            else
            {
                if (_worker == null)
                {
                    throw new ApplicationException(string.Format("请先创建RockWorker"));
                }
                else
                {
                    RpcServer rpcServer = new RpcServer(vPort, _worker.Context);
                    rpcServer.Start();

                    RegisterPort(vPort);
                    _componentDict.TryAdd(vPort, rpcServer);

                    return rpcServer;
                }
            }
        }

        internal static AsyncRpcClient CreateAsyncRpcClient(ushort vPort)
        {
            if (CheckPort(vPort) || CheckPort((ushort)(vPort + 1)))
            {
                throw new ApplicationException(string.Format("端口 ：{0}已经被占用，请选择其他端口！", vPort));
            }
            else
            {
                if (_worker == null)
                {
                    throw new ApplicationException(string.Format("请先创建RockWorker"));
                }
                else
                {
                    AsyncRpcClient asyncRpcClient = new AsyncRpcClient(vPort, _worker.Context);
                    asyncRpcClient.Start();

                    RegisterPort(vPort);
                    RegisterPort((ushort)(vPort + 1));
                    _componentDict.TryAdd(vPort, asyncRpcClient);

                    return asyncRpcClient;
                }
            }
        }

        public static FtpServer CreateFTPServer()
        {
            object ftpServer = null;
            if (_componentDict.TryGetValue(21, out ftpServer))
            {
                return (FtpServer)_componentDict[21];
            }
            else
            {
                if (_worker == null)
                {
                    throw new ApplicationException(string.Format("请先创建RockWorker"));
                }
                else
                {
                    FtpServer serServer = new FtpServer(_worker.Context);
                    serServer.Start();

                    _componentDict.TryAdd(21, serServer);

                    return serServer;
                }
            }
        }

        public static FtpClient CreateFTPClient(ushort vPort)
        {
            lock (_lock)
            {
                return NewFtpClient(vPort);
            }
        }

        public static FtpClient CreateFTPClient()
        {
            lock (_lock)
            {
                //如果没有端口参数就随机生成
                ushort port = GetAvailablePort();
                return NewFtpClient(port);
            }
        }

        private static FtpClient NewFtpClient(ushort vPort)
        {
            if (CheckPort(vPort))
            {
                throw new ApplicationException(string.Format("端口 ：{0}已经被占用，请选择其他端口！", vPort));
            }
            else
            {
                if (_worker == null)
                {
                    throw new ApplicationException(string.Format("请先创建RockWorker"));
                }
                else
                {
                    FtpClient ftpClient = new FtpClient(vPort, _worker.Context);
                    ftpClient.Start();

                    RegisterPort(vPort);
                    _componentDict.TryAdd(vPort, ftpClient);

                    return ftpClient;
                }
            }
        }

        internal static void RegisterPort(ushort vPort)
        {
            lock (_lock)
            {
                _usedPort.Add(vPort);
            }
        }

        internal static void UnregisterPort(ushort vPort)
        {
            lock (_lock)
            {
                _usedPort.Remove(vPort);
            }

            //如果占用该端口的是个组件，则删除组件
            if (_componentDict.ContainsKey(vPort))
            {
                object obj = null;
                _componentDict.TryRemove(vPort, out obj);
            }
        }

        /// <summary>
        /// 检查端口是否可用，如果该端口已被占用返回true，未被占用返回false
        /// </summary>
        /// <param name="vPort">要检查的端口</param>
        /// <returns>如果该端口已被占用返回true，未被占用返回false</returns>
        public static bool CheckPort(ushort vPort)
        {
            return _usedPort.Contains(vPort);
        }

        /// <summary>
        /// 得到一个可用的端口
        /// </summary>
        /// <returns></returns>
        public static ushort GetAvailablePort()
        {
            Random randow = new Random();
            while (true)
            {
                ushort port = (ushort)randow.Next(1024, 65536);
                if (!CheckPort(port) && port % 128 != 0)
                    return port;
            }
        }

        /// <summary>
        /// 释放线程，及关闭ZmqContext, ZmqSocket等
        /// </summary>
        public static void Dispose()
        {
            object[] objs = _componentDict.Values.ToArray<object>();

            for (int i = objs.Length - 1; i >= 0; i--)
            {
                IDisposable socket = objs[i] as IDisposable;
                socket.Dispose();
            }

            Thread.Sleep(200);
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }
        }
    }
}