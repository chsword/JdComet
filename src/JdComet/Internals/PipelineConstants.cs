namespace JdComet
{
    class PipelineConstants
    {
        public const string ErrMsgHeader = "errmsg";
        public const string ParamAppkey = "app_key";
        public const string ParamUserid = "user";
        public const string ParamConnectId = "id";
        public const string ParamTimestamp = "timestamp";
        public const string ParamSign = "sign";

        //code
        public const string ConnectSuccess = "200";//连接成功的code
        public const string HeartBeat = "201";//心跳
        public const string NewMessage = "202";//消息
        public const string DiscardMessage = "203";//当客户端断开连接后，服务端会记录下来丢弃消息的开始时间
        public const string ConnectReachMaxTime = "101";//连接到达最大时间，服务端主动断开
        public const string ServerDeploy = "102";//服务端在发布
        public const string ServerRehash = "103";//服务端负载不均衡了，断开所有的客户端重连
        public const string ClientKickoff = "104";//对于重复的连接，服务端用新的连接替换掉旧的连接
        public const string ServerKickoff = "105";//由于消息量太大，而isv接收的速度太慢，服务端断开isv的连接

        public const string Reconnect = "500";//客户端主动重连,或者出现了异常需要重连
        public const string ClientFirstConnect = "501";//客户端第一次发起连接
    }
}