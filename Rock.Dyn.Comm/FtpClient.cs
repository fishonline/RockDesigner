using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ZeroMQ;
using Rock.Dyn.Core;
using Rock.Dyn.Msg;
using System.Runtime.Serialization.Formatters.Binary;

namespace Rock.Dyn.Comm
{
    /// <summary>
    /// 文件传输进度
    /// </summary>
    /// <param name="total"></param>
    /// <param name="transfered"></param>
    public delegate void ProgressEventHandler(double total, double transfered);

    /// <summary>
    /// FtpClient是线程不安全，要同时下载或上传多个文件请创建多个FtpClient，创建时要使用RockContext.CreateFtpClient()
    /// 使用过之后要调用Dispose()方法释放占用的系统资源
    /// </summary>
    public class FtpClient : IDisposable
    {
        private Encoding _encoding = Encoding.UTF8;
        public event ProgressEventHandler ProgressEvent;
        private List<string> _disallowedPostfixList = new List<string>();

        private static readonly ushort FTPVPORT = 21;
        private ZmqContext _context;
        private ZmqSocket _ftpSocket;
        private VirtuaIP _remoteAddress;
        private string _inprocHost = "inproc://frontend";

        private ushort _localPort;
        private ushort _serverPort;
        /// <summary>
        /// 1024*100
        /// </summary>
        private int _bufferLength = 1024 * 100;

        private bool _isCancelUpload;
        private bool _isCancelDownload;
        private bool _isWorking;

        private bool _disposed;

        internal FtpClient(ushort localPort, ZmqContext context)
        {

            if (context == null)
                throw new ApplicationException("创建RpcClient时context为null");

            _context = context;
            _localPort = localPort;
        }

        /// <summary>
        /// Finalizes an instance of the RockClient class.
        /// </summary>
        ~FtpClient()
        {
            Dispose(false);
        }

        public bool IsWorking
        {
            get { return _isWorking; }
        }

        public void Start()
        {
            if (_ftpSocket != null) return;

            _ftpSocket = _context.CreateSocket(SocketType.REQ);
            _ftpSocket.Identity = BitConverter.GetBytes(_localPort);

            bool isConnected = false;
            while (!isConnected)
            {
                try
                {
                    _ftpSocket.Connect(_inprocHost);
                    isConnected = true;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void ReStart()
        {
            if (_ftpSocket != null)
            {
                _ftpSocket.Close();
                _ftpSocket = null;
            }
            Start();
        }

        /// <summary>
        ///向服务端发送传送文件的请求,服务端返回数据通信的端口
        /// </summary>
        /// <returns>服务端通信端口号</returns>
        private ushort GetServerPort(string requestData)
        {
            Send(FTPVPORT, _encoding.GetBytes(requestData));
            ZmqMessage message = _ftpSocket.ReceiveMessage(new TimeSpan(0, 1, 0));
            if (message.FrameCount == 0)
            {
                ReStart();
                throw new TimeoutException("发送超时，请检查你的网络是否有问题");
            }
            else
            {
                string responseData = _encoding.GetString(message[message.FrameCount - 1].Buffer);
                string[] temp = responseData.Split(':');

                if ("Success" == temp[0])
                {
                    return ushort.Parse(temp[1]);
                }
                else if ("Error" == temp[0])
                {
                    throw new FtpException(temp[1]);
                }
                else
                {
                    throw new FtpException("未知的服务器返回信息");
                }
            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="recvAddress">服务端机器的地址</param>
        /// <param name="filePath">准备上传的文件</param>
        /// <param name="savePath">文件在服务端的保存路径</param>
        public void FileUpload(string recvAddress, string filePath, string savePath)
        {
            if (_isWorking)
            {
                throw new FtpException("一个FtpClient不能同时执行多个任务");
            }

            CheckPostfix(filePath);//检查上传的文件类型是否合法

            string md5 = GetMD5HashFromFile(filePath);
            _remoteAddress = new VirtuaIP(recvAddress);
            _serverPort = GetServerPort("FileUpload");

            using (FileStream fStream = File.OpenRead(filePath))
            {
                long fileSize = fStream.Length;//文件数据长度
                long remaining = fileSize; // 剩余没上传的数据长度
                byte[] buffer = new byte[_bufferLength];

                //构建文件片段
                DynObject fileFragment = new DynObject("FileFragment");
                fileFragment["MD5"] = md5;
                fileFragment["FileName"] = Path.GetFileName(filePath);
                fileFragment["Extension"] = Path.GetExtension(filePath);
                fileFragment["Path"] = savePath;
                fileFragment["DataLength"] = 0;
                fileFragment["MsgID"] = -1;
                fileFragment["State"] = (byte)0;

                _isCancelUpload = false;
                _isWorking = true;
                int offset = 0;

                while (remaining >= 0 && _isWorking)
                {
                    if (_isCancelUpload)
                    {
                        fileFragment["State"] = (byte)3;
                    }

                    switch ((byte)fileFragment["State"])
                    {
                        case 0: //文件传送开始
                            //第一次发送文件,发送一些基本信息（如文件名等）
                            fileFragment["DataLength"] = 0;
                            fileFragment["Data"] = null;
                            break;
                        case 1: //文件传送中
                            fStream.Seek((int)fileFragment["MsgID"] * _bufferLength, SeekOrigin.Begin);
                            fileFragment["DataLength"] = fStream.Read(buffer, offset, buffer.Length - offset);
                            fileFragment["Data"] = buffer;

                            //重置剩余数据长度
                            remaining = fileSize - ((int)fileFragment["MsgID"] * _bufferLength + (int)fileFragment["DataLength"]);
                            if (remaining == 0)
                            {
                                _isWorking = false;

                                remaining = -1;
                                fileFragment["State"] = (byte)2;
                            }
                            break;
                        case 3: //中断文件传送
                            fileFragment["DataLength"] = 0;
                            fileFragment["Data"] = null;

                            _isWorking = false;
                            break;
                    }

                    //序列化
                    TSerializer serializer = new TBinarySerializer();
                    DynSerialize.WriteDynObject(serializer, fileFragment);
                    byte[] data = serializer.ToBytes();
                    serializer.Flush();

                    DynObject repFragment = Transfer(_serverPort, data, 0);
                    fileFragment["State"] = repFragment["State"];
                    fileFragment["MsgID"] = repFragment["MsgID"];

                    //向外界通知文件传送进度
                    if (ProgressEvent != null)
                        ProgressEvent(fileSize, fileSize - remaining);
                }
            }
        }

        private DynObject Transfer(ushort serverPort, byte[] buffer, int count)
        {
            //发送数据
            Send(serverPort, buffer);

            ZmqMessage message = _ftpSocket.ReceiveMessage(new TimeSpan(0, 1, 30));
            if (message.FrameCount == 0)
            {
                _isWorking = false;
                ReStart();
                throw new TimeoutException("发送超时，请检查你的网络是否有问题,稍后重新上传");
            }
            else
            {
                //反序列化，取出服务端响应数据
                byte[] respData = message[message.FrameCount - 1].Buffer;
                TSerializer serializer = new TBinarySerializer();
                serializer.FromBytes(respData);
                DynObject fileFragment = DynSerialize.ReadDynObject(serializer);
                serializer.Flush();

                //105：发生异常
                if ((byte)fileFragment["State"] == 105)
                {
                    _isWorking = false;
                    throw new ApplicationException(fileFragment["ExcepMsg"] as string);
                }

                return fileFragment;
            }
        }

        /// <summary>
        /// 取消上传任务
        /// </summary>
        public void CancelUpload()
        {
            _isCancelUpload = true;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="recvAddress">服务端机器的地址</param>
        /// <param name="savePath">文件在本地的保存路径</param>
        /// <param name="downloadPath">要下载的文件在服务端的路径</param>
        public void FileDownload(string recvAddress, string savePath, string downloadPath)
        {
            #region 前置检查
            if (_isWorking)
            {
                throw new FtpException("一个FtpClient不能同时执行多个任务");
            }

            //验证本地是否已有同名文件
            if (File.Exists(Path.Combine(savePath, Path.GetFileName(downloadPath))))
            {
                throw new FtpException(savePath + "目录中已存在同名文件");
            }
            #endregion 前置检查

            #region 获取文件的基本信息
            _remoteAddress = new VirtuaIP(recvAddress);
            _serverPort = GetServerPort("FileDownload");
            _isWorking = true;

            //发送下载文件的请求
            long fileSize = 0;//文件大小（字节）
            long remaining = 0;//剩余未下载的文件大小
            DynObject fileFragment = RequestDownload(_serverPort, downloadPath);
            fileSize = remaining = (long)fileFragment["FileLength"];

            #endregion 获取文件的基本信息

            FileStream writeFileStream = null;
            _isCancelDownload = false;

            while (remaining >= 0 && _isWorking)
            {
                if (_isCancelDownload)
                {
                    fileFragment["State"] = (byte)103;
                }

                switch ((byte)fileFragment["State"])
                {
                    case 100:
                        string tempFile = Path.Combine(savePath, fileFragment["MD5"] + ".temp");

                        //如果存在该文件的临时文件，则进行断点续传,否则以MD5为名字创建临时文件开始传送数据
                        if (File.Exists(tempFile))
                        {
                            writeFileStream = new FileStream(tempFile, FileMode.Open);
                            fileFragment["MsgID"] = (int)(writeFileStream.Length / _bufferLength);
                            fileFragment["State"] = (byte)101;
                            fileFragment["Data"] = null;
                        }
                        else
                        {
                            writeFileStream = new FileStream(tempFile, FileMode.Create);
                            fileFragment["MsgID"] = 0;
                            fileFragment["State"] = (byte)101;
                            fileFragment["Data"] = null;
                        }
                        remaining = fileSize - (int)fileFragment["MsgID"] * _bufferLength;
                        break;
                    case 101: //文件传送中
                        writeFileStream.Seek((int)fileFragment["MsgID"] * _bufferLength, SeekOrigin.Begin);
                        writeFileStream.Write(fileFragment["Data"] as byte[], 0, (int)fileFragment["DataLength"]);

                        remaining = fileSize - ((int)fileFragment["MsgID"] * _bufferLength + (int)fileFragment["DataLength"]);
                        if (remaining <= 0)
                        {
                            //文件下载结束
                            fileFragment["State"] = (byte)102;

                            writeFileStream.Flush();
                            writeFileStream.Close();
                            writeFileStream = null;
                            _isWorking = false;

                            //校验文件传送是否正确
                            string path = Path.Combine(savePath, fileFragment["MD5"] as string + ".temp");
                            string tempFileMD5 = GetMD5HashFromFile(path);

                            if (tempFileMD5 == fileFragment["MD5"] as string)
                            {
                                File.Move(path, Path.Combine(savePath, fileFragment["FileName"] as string));
                            }
                            else
                            {
                                File.Delete(path);
                                throw new FtpException("文件未能成功下载");
                            }
                        }

                        fileFragment["MsgID"] = (int)fileFragment["MsgID"] + 1;
                        fileFragment["DataLength"] = 0;
                        fileFragment["Data"] = null;
                        break;
                    case 103: //中断文件传送
                        fileFragment["DataLength"] = 0;
                        fileFragment["Data"] = null;

                        writeFileStream.Flush();
                        writeFileStream.Close();
                        writeFileStream = null;
                        _isWorking = false;
                        break;
                    case 105: //服务端发生异常
                        writeFileStream.Flush();
                        writeFileStream.Close();
                        writeFileStream = null;
                        _isWorking = false;
                        throw new FtpException(fileFragment["ExcepMsg"] as string);
                }

                //向外界通知文件床送进度
                if (ProgressEvent != null)
                    ProgressEvent(fileSize, fileSize - remaining);

                //发送请求下一个包数据（发生异常告诉对方关闭文件流）
                TSerializer serialize = new TBinarySerializer();
                DynSerialize.WriteDynObject(serialize, fileFragment);
                byte[] buffer = serialize.ToBytes();
                serialize.Flush();

                Send(_serverPort, buffer);
                ZmqMessage message = _ftpSocket.ReceiveMessage(new TimeSpan(0, 1, 30));
                if (message.FrameCount == 0)
                {
                    writeFileStream.Close();
                    writeFileStream = null;
                    _isWorking = false;
                    ReStart();
                    throw new TimeoutException("发送超时，请检查你的网络是否有问题,稍后重新下载");
                }

                serialize = new TBinarySerializer();
                serialize.FromBytes(message[message.FrameCount - 1].Buffer);
                fileFragment = DynSerialize.ReadDynObject(serialize);
                serialize.Flush();
            }
        }

        /// <summary>
        /// 向服务端发送下载文件的请求
        /// </summary>
        /// <param name="destVPort">目的端口</param>
        /// <param name="downloadPath">要下载的文件在服务端的路径</param>
        /// <returns>将要下载文件的基本信息</returns>
        private DynObject RequestDownload(ushort destVPort, string downloadPath)
        {
            DynObject fileFragment = new DynObject("FileFragment");
            fileFragment["Path"] = Path.GetDirectoryName(downloadPath);
            fileFragment["FileName"] = Path.GetFileName(downloadPath);
            fileFragment["Extension"] = Path.GetExtension(downloadPath);
            fileFragment["State"] = (byte)100;
            fileFragment["MsgID"] = -1;
            fileFragment["DataLength"] = 0;
            fileFragment["Data"] = null;

            //序列化要发送的数据
            TSerializer serializer = new TBinarySerializer();
            DynSerialize.WriteDynObject(serializer, fileFragment);
            byte[] buffer = serializer.ToBytes();
            serializer.Flush();

            Send(_serverPort, buffer);
            ZmqMessage msg = _ftpSocket.ReceiveMessage(new TimeSpan(0, 1, 30));
            if (msg.FrameCount == 0)
            {
                _isWorking = false;
                ReStart();
                throw new TimeoutException("发送超时，请检查你的网络是否有问题,稍后重新下载");
            }

            //反序列化，服务器返回的数据
            serializer = new TBinarySerializer();
            serializer.FromBytes(msg[msg.FrameCount - 1].Buffer);
            DynObject respFragment = DynSerialize.ReadDynObject(serializer);
            serializer.Flush();

            //发生异常
            if ((byte)respFragment["State"] == 105)
            {
                _isWorking = false;
                throw new FtpException(respFragment["ExcepMsg"] as string);
            }

            return respFragment;
        }

        public void CancelDownload()
        {
            _isCancelDownload = true;
        }

        private void Send(ushort destVPort, byte[] requestData)
        {
            if (_ftpSocket == null)
                throw new ApplicationException("请你先启动FTPClient");

            /*
             * 发送数据--共三帧
             * 1、目的地址
             * 2、目的端口
             * 3、源端口
             * 4、数据
             * */
            _ftpSocket.SendMore(_remoteAddress.ToBytes());
            _ftpSocket.SendMore(BitConverter.GetBytes(destVPort));
            _ftpSocket.SendMore(BitConverter.GetBytes(_localPort));
            _ftpSocket.Send(requestData);
        }

        /// <summary>
        /// 向外部传递异常信息
        /// </summary>
        /// <param name="e"></param>
        private void RaiseDealMessageExceptionEvent(System.Exception e)
        {

        }

        /// <summary>
        /// 添加不允许上传的文件后缀名，如果有多个，用“,”分割，如："exe,jpg,jsp"
        /// </summary>
        /// <param name="postfixes"></param>
        public void AddDisallowedPostfix(string postfixes)
        {
            string[] temp = postfixes.Split(',');
            foreach (string postfix in temp)
            {
                if (!_disallowedPostfixList.Contains(postfix))
                    _disallowedPostfixList.Add(postfix);
            }
        }

        /// <summary>
        /// 检查文件的类型是否容许上传
        /// </summary>
        /// <param name="filePath"></param>
        private void CheckPostfix(string filePath)
        {
            foreach (string postfix in _disallowedPostfixList)
            {
                if (filePath.EndsWith("." + postfix))
                    throw new FtpException("服务器不支持" + postfix + "类型文件的上传");
            }
        }

        /// <summary>
        /// 得到文件的MD5
        /// </summary>
        /// <param name="fileName">文件路径</param>
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

        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the FTPClient class.
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
                _isWorking = false;
                if (_ftpSocket != null)
                {
                    _ftpSocket.Dispose();
                    _ftpSocket = null;
                }
                RockContext.UnregisterPort(_localPort);
            }
            _disposed = true;
        }
    }
}