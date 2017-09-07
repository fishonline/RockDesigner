using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ZeroMQ;
using Rock.Dyn.Core;
using Rock.Dyn.Msg;
using System.Diagnostics;

namespace Rock.Dyn.Comm
{
    internal delegate DynObject ReceivedDelegate(DynObject fileFragment);
    public delegate void ExceptionDelegate(Exception e);

    public class FtpServer : IDisposable
    {
        private Encoding _encoding = Encoding.UTF8;
        private byte[] _emptyFrame = new byte[0];
        private ushort _ftpServerPort = 21;
        private string _inprocHost = "inproc://frontend";

        private ZmqContext _context;
        private Thread _thread;
        private bool _isRunning;
        private bool _disposed;

        private List<Client> _clients = new List<Client>();
        public ExceptionDelegate OnException;

        internal FtpServer(ZmqContext context)
        {
            if (context == null)
                throw new ApplicationException("创建FTPServer时context为null");

            _context = context;
        }

        /// <summary>
        /// Finalizes an instance of the RockClient class.
        /// </summary>
        ~FtpServer()
        {
            Dispose(false);
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;

                _thread = new Thread(OnServer);
                _thread.IsBackground = true;
                _thread.Start();
            }
            else
            {
                throw new ApplicationException("FTPServer已经运行");
            }
        }

        private void OnServer()
        {
            using (ZmqSocket serverSocket = _context.CreateSocket(SocketType.DEALER))
            {
                serverSocket.Identity = BitConverter.GetBytes(_ftpServerPort);
                serverSocket.SendHighWatermark = 100000;

                bool isConnected = false;
                while (!isConnected)
                {
                    try
                    {
                        serverSocket.Connect(_inprocHost);
                        isConnected = true;
                    }
                    catch
                    {
                        Thread.Sleep(500);
                    }
                }

                while (_isRunning)
                {
                    string responseData = null;
                    VirtuaIP originVIP = null;
                    ushort originPort = 0;

                    try
                    {
                        ZmqMessage zmqMessage = serverSocket.ReceiveMessage(TimeSpan.MaxValue);

                        if (zmqMessage.FrameCount == 0)
                        {
                            throw new FtpException();
                        }
                        originVIP = new VirtuaIP(zmqMessage[1].Buffer);
                        originPort = (ushort)BitConverter.ToInt16(zmqMessage[3].Buffer, 0);
                        byte[] requestMsg = zmqMessage[zmqMessage.FrameCount - 1].Buffer;

                        //当有传送文件的请求时，创建一个Client和客户端通信，并把Client的端口返回给客户端--TODO
                        ushort port = RockContext.GetAvailablePort();
                        string requestType = _encoding.GetString(requestMsg);

                        Client client = new Client(port, _context, this, requestType);
                        client.OnException = OnException;
                        client.Start();

                        responseData = "Success:" + port;
                        Thread.Sleep(2000);
                    }
                    catch (ThreadInterruptedException )
                    {
                        continue;
                    }
                    catch (FtpException )
                    {
                        continue;
                    }
                    catch (Exception e)
                    {
                        if (OnException != null)
                            OnException(e);
                        responseData = "Error:" + e.Message;
                    }

                    /*
                     * 发送数据--共三帧
                     * 0、空帧
                     * 1、目的地址
                     * 2、目的端口
                     * 3、源端口
                     * 4、数据
                     * */
                    serverSocket.SendMore(_emptyFrame);
                    serverSocket.SendMore(originVIP.ToBytes());
                    serverSocket.SendMore(BitConverter.GetBytes(originPort));
                    serverSocket.SendMore(BitConverter.GetBytes(_ftpServerPort));
                    serverSocket.Send(_encoding.GetBytes(responseData));
                }
            }
        }

        internal void RegisterClient(Client client)
        {
            lock (this)
            {
                _clients.Add(client);
            }
        }

        internal void UnregisterClient(Client client)
        {
            lock (this)
            {
                _clients.Remove(client);
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the RockClient class.
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
                }
                //释放非托管对象
                _isRunning = false;
                _thread.Interrupt();

                Client[] objs = _clients.ToArray();
                foreach (Client client in objs)
                {
                    client.Dispose();
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// 内部类处理和FTPClient的通信
        /// </summary>
        internal class Client : IDisposable
        {
            public ExceptionDelegate OnException;
            private ReceivedDelegate OnFileDataReceived;
            private Encoding _encoding = Encoding.UTF8;
            private BinaryFormatter bf = new BinaryFormatter();

            private FtpServer _ftpServer;
            private ZmqContext _context;
            private FileStream _fileStream = null;

            private static readonly string _inprocHost = "inproc://frontend";
            private static readonly int _bufferLength = 102400;
            private byte[] buffer = new byte[_bufferLength];

            private ushort _localPort;
            private Thread _thread = null;
            private bool _isRunning;
            //private bool _disposed;
            //private int count;

            public Client(ushort localPort, ZmqContext context, FtpServer ftpServer, string type)
            {
                if (context == null)
                    throw new ApplicationException("创建Client时context为null");

                if ("FileUpload" == type)
                {
                    OnFileDataReceived = OnFileUpload;
                }
                else if ("FileDownload" == type)
                {
                    OnFileDataReceived = onFileDownload;
                }
                else
                {
                    throw new ApplicationException("请求命令错误！");
                }

                _context = context;
                _localPort = localPort;

                _ftpServer = ftpServer;
                _ftpServer.RegisterClient(this);
                RockContext.RegisterPort(_localPort);
            }

            /// <summary>
            /// Finalizes an instance of the RockClient class.
            /// </summary>
            ~Client()
            {
                Dispose(false);
            }

            public bool IsRunning
            {
                get { return _isRunning; }
            }

            public void Start()
            {
                if (!_isRunning)
                {
                    _isRunning = true;

                    _thread = new Thread(Received);
                    _thread.IsBackground = true;
                    _thread.Start();
                }
                else
                {
                    throw new ApplicationException("Client已经运行");
                }
            }

            private void Received()
            {
                using (ZmqSocket clientSocket = _context.CreateSocket(SocketType.REP))
                {
                    clientSocket.Identity = BitConverter.GetBytes(_localPort);
                    clientSocket.SendHighWatermark = 10000;

                    bool isConnected = false;
                    while (!isConnected)
                    {
                        try
                        {
                            clientSocket.Connect(_inprocHost);
                            isConnected = true;
                        }
                        catch
                        {
                            Thread.Sleep(500);
                        }
                    }

                    try
                    {
                        while (_isRunning)
                        {
                            ZmqMessage zmqMessage = clientSocket.ReceiveMessage(new TimeSpan(0, 2, 0));
                            if (zmqMessage.FrameCount == 0)
                            {
                                //一分钟内没有收到任何消息，自行关闭
                                _isRunning = false;
                                continue;
                            }

                            VirtuaIP originVIP = new VirtuaIP(zmqMessage[0].Buffer);
                            ushort originPort = (ushort)BitConverter.ToInt16(zmqMessage[2].Buffer, 0);

                            byte[] respMsg = DealMessage(zmqMessage[zmqMessage.FrameCount - 1].Buffer);

                            //向客户端返回数据
                            clientSocket.SendMore(originVIP.ToBytes());
                            clientSocket.SendMore(BitConverter.GetBytes(originPort));
                            clientSocket.SendMore(BitConverter.GetBytes(_localPort));
                            clientSocket.Send(respMsg);
                        }
                    }
                    catch (ThreadInterruptedException ) { }
                }

                //自行关闭释放资源
                Dispose(false);
            }

            /// <summary>
            /// 处理接收到的数据
            /// </summary>
            /// <param name="msgData">接收到的消息</param>
            private byte[] DealMessage(byte[] msgData)
            {
                //反序列化，取出数据
                TSerializer serializer = new TBinarySerializer();
                serializer.FromBytes(msgData);
                DynObject reqFragment = DynSerialize.ReadDynObject(serializer);
                serializer.Flush();

                try
                {
                    DynObject repFragment = OnFileDataReceived(reqFragment);

                    //序列化并把数据返回给客户端
                    serializer = new TBinarySerializer();
                    DynSerialize.WriteDynObject(serializer, repFragment);
                    byte[] repData = serializer.ToBytes();
                    serializer.Flush();

                    return repData;
                }
                catch (Exception ex)
                {
                    //如果服务端需要记录日志，则由本事件把异常信息发出
                    if (OnException != null)
                        OnException(ex);

                    _isRunning = false;

                    DynObject repFragment = new DynObject("FileFragment");
                    repFragment["State"] = (byte)105;
                    repFragment["ExcepMsg"] = "FtpServer端异常，" + ex.Message;

                    //序列化并把异常数据返回给客户端
                    serializer = new TBinarySerializer();
                    DynSerialize.WriteDynObject(serializer, repFragment);
                    byte[] repData = serializer.ToBytes();
                    serializer.Flush();

                    return repData;
                }
            }

            public DynObject OnFileUpload(DynObject fileFragment)
            {
                byte[] data = fileFragment["Data"] as byte[];
                int dataLength = (int)fileFragment["DataLength"];
                int msgID = (int)fileFragment["MsgID"];
                Console.WriteLine(msgID);

                switch ((byte)fileFragment["State"])
                {
                    case 0: //文件传送开始
                        string savePath = fileFragment["Path"] as string;
                        string md5 = fileFragment["MD5"] as string;
                        string fileName = fileFragment["FileName"] as string;

                        if (File.Exists(Path.Combine(savePath, fileName)))
                        {
                            throw new FtpException("文件" + Path.Combine(savePath, fileName) + "已存在");
                        }

                        //如果存在该文件的临时文件，则进行断点续传,否则以MD5为名字创建临时文件开始传送数据
                        string tempFile = Path.Combine(savePath, md5 + ".temp");

                        if (File.Exists(tempFile))
                        {
                            _fileStream = new FileStream(tempFile, FileMode.Open);
                            fileFragment["MsgID"] = (int)(_fileStream.Length / _bufferLength);
                            fileFragment["State"] = (byte)1;
                            fileFragment["Data"] = null;
                        }
                        else
                        {
                            _fileStream = new FileStream(tempFile, FileMode.Create);
                            fileFragment["MsgID"] = 0;
                            fileFragment["State"] = (byte)1;
                            fileFragment["Data"] = null;
                        }
                        break;
                    case 1: //文件传送中
                        _fileStream.Seek(msgID * _bufferLength, SeekOrigin.Begin);
                        _fileStream.Write(data, 0, dataLength);
                        _fileStream.Flush();

                        fileFragment["MsgID"] = (int)fileFragment["MsgID"] + 1;
                        fileFragment["Data"] = null;
                        break;
                    case 2: //文件传送结束
                        _fileStream.Write(data, 0, dataLength);
                        _fileStream.Flush();
                        _fileStream.Close();
                        _fileStream = null;

                        //校验文件传送是否正确
                        string path = Path.Combine(fileFragment["Path"] as string, fileFragment["MD5"] as string + ".temp");
                        string tempFileMD5 = GetMD5HashFromFile(path);

                        if (tempFileMD5 == fileFragment["MD5"] as string)
                        {
                            File.Move(path, Path.Combine(fileFragment["Path"] as string, fileFragment["FileName"] as string));
                        }
                        else
                        {
                            //File.Delete(path);
                            //throw new FtpException("文件未能成功上传");
                        }

                        //文件上传完成，client自行关闭
                        _isRunning = false;
                        break;
                    case 3://中断文件传送
                        _fileStream.Close();
                        _fileStream = null;

                        _isRunning = false;
                        break;
                }

                return fileFragment;
            }

            public DynObject onFileDownload(DynObject fileFragment)
            {
                byte ctlCode = (byte)fileFragment["State"];
                int dataLength = (int)fileFragment["DataLength"];

                switch (ctlCode)
                {
                    case 100: //请求下载文件
                        string filePath = Path.Combine(fileFragment["Path"] as string, fileFragment["FileName"] as string);
                        string md5 = GetMD5HashFromFile(filePath);
                        _fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                        fileFragment["MD5"] = md5;
                        fileFragment["FileLength"] = _fileStream.Length;
                        fileFragment["DataLength"] = 0;
                        fileFragment["Data"] = null;
                        break;
                    case 101://文件传送中
                        if (_fileStream == null)
                        {
                            throw new FtpException("FtpServer文件流已被中断，请重新传送");
                        }

                        _fileStream.Seek((int)fileFragment["MsgID"] * _bufferLength, SeekOrigin.Begin);
                        dataLength = _fileStream.Read(buffer, 0, buffer.Length);

                        fileFragment["DataLength"] = dataLength;
                        fileFragment["Data"] = buffer;
                        break;
                    case 102: //文件传送结束
                        _isRunning = false;

                        fileFragment["DataLength"] = 0;
                        fileFragment["Data"] = null;

                        _fileStream.Close();
                        _fileStream = null;
                        break;
                    case 103: //中断文件传送
                        _isRunning = false;

                        _fileStream.Close();
                        _fileStream = null;
                        break;
                }

                return fileFragment;
            }

            /// <summary>
            /// 获得文件的md5码
            /// </summary>
            /// <param name="fileName">文件的路径</param>
            /// <returns></returns>
            public string GetMD5HashFromFile(string fileName)
            {
                try
                {
                    FileStream file = new FileStream(fileName, FileMode.Open);
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    file.Close();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("获得文件的md5码 失败!error:" + ex.Message);
                }
            }

            /// <summary>
            /// Releases all resources used by the current instance of the RockClient class.
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
                //释放非托管对象
                if (!disposing)
                {
                    if (_fileStream != null)
                    {
                        _fileStream.Close();
                        _fileStream = null;
                    }

                    _ftpServer.UnregisterClient(this);
                    RockContext.UnregisterPort(_localPort);
                }
                else
                {
                    _isRunning = false;
                    _thread.Interrupt();//中断线程
                }
            }
        }
    }
}
