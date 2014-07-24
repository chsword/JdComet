using System;
using JdComet.Message;

namespace JdComet.Demo.Listeners
{
    public class MyCometMessageProcesser : ICometMessageProcesser
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public MyCometMessageProcesser(string appkey, string secret)
        {
            AppKey = appkey;
            AppSecret = secret;
        }

        private void SendMessage(string title, string messageString)
        {
            try
            {
                Console.WriteLine("{0} , {1}", title, messageString);
            }
            catch
            {
            }
        }

        public void OnConnectMsg(string message)
        {
            SendMessage("OnConnectMsg ", DateTime.Now.ToString());
        }

        public void OnHeartBeat()
        {
            SendMessage("OnHeartBeat ", DateTime.Now.ToString());
        }

        public void OnReceiveMsg(string message)
        {
            SendMessage("Receive ", message);
            try
            {
                SendMessage("可以使用 JDynamic来解析JSON ", "http://jdynamic.codeplex.com/");
            }
            catch(Exception ex)
            {
                SendMessage("receiveMsg.exception ", ex.ToString());

            }
        }

        public void OnConnectReachMaxTime()
        {
            SendMessage("OnConnectReachMaxTime ", DateTime.Now.ToString());
        }

        public void OnDiscardMsg(string message)
        {
 
        }

        public void OnServerUpgrade(string message)
        {
        }

        public void OnServerRehash()
        {
        }

        public void OnServerKickOff()
        {
            SendMessage("OnServerKickOff ", DateTime.Now.ToString());
        }

        public void OnClientKickOff()
        {
            SendMessage("OnClientKickOff ", DateTime.Now.ToString());
        }

        public void OnOtherMsg(string message)
        {
            SendMessage("OnOtherMsg ", message);
        }

        public void OnException(Exception ex)
        {
            SendMessage("OnException ", ex.ToString());
        }
    }
}