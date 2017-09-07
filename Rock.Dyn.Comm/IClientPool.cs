namespace Rock.Dyn.Comm
{
    public interface IClientPool
    {
        RpcClient GetFreeRpcClient();
        void GiveBackRpcClient(RpcClient rpcClient);
    }
}
