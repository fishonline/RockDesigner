using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.Dyn.Comm
{
    public class RpcClientPool : IClientPool
    {
        public Task InitTask;
        private Queue _queue = Queue.Synchronized((new Queue()));
        private static RpcClientPool _interance = null;


        public int MaxPoolNum = 60;
        /// <summary>
        /// 1000*3600*12
        /// </summary>
        public int Interval = 43200000;

        #region Singetion
        private RpcClientPool()
        {
            InitTask = new Task(Init);
            InitTask.Start();
        }

        public static RpcClientPool Interance
        {
            get
            {
                if (_interance == null)
                {
                    _interance = new RpcClientPool();
                }
                return RpcClientPool._interance;
            }
        }
        #endregion

        public void Init()
        {
            while (true)
            {
                //如果_queue.Count == 0 说明还没有初始化
                if (_queue.Count < MaxPoolNum)
                {
                    RpcClient rpc = null;

                    while (_queue.Count < MaxPoolNum)
                    {
                        try
                        {
                            rpc = RockContext.CreateRpcClient();
                            _queue.Enqueue(rpc);
                        }
                        catch { }
                    }
                }

                //每300分钟更新一次租借池
                Thread.Sleep(Interval);
            }
        }

        /// <summary>
        /// 借RPC
        /// </summary>
        /// <returns></returns>
        public RpcClient GetFreeRpcClient()
        {
            RpcClient result = _queue.Dequeue() as RpcClient;
            return result;
        }

        /// <summary>
        /// 还RpcClient
        /// </summary>
        /// <param name="rpcClient"></param>
        public void GiveBackRpcClient(RpcClient rpcClient)
        {
            if (rpcClient == null)
            {
                return;
            }

            _queue.Enqueue(rpcClient);
        }
    }
}
