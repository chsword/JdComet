using System;

namespace JdComet.Demo.Listeners
{
    public class MyCometConnection : ICometConnection
    {   public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public MyCometConnection(string appkey, string secret)
        {
            AppKey = appkey;
            AppSecret = secret;
        }

        public void OnBeforeConnect()
        {
        }

        public void OnConnect()
        {
        }

        public void OnException(Exception throwable)
        {
        }

        public void OnConnectError(Exception e)
        {
        }

        public void OnReadTimeout()
        { 
        }

        public void OnMaxReadTimeoutException()
        {
            
        }

        public void OnSysErrorException(Exception e)
        {
        }
    }
}