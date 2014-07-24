using System;
using System.Collections.Generic;
using System.Net;

namespace JdComet
{
    internal class JdCometUnitClient
    {
        public JdCometUnitClient(string appkey, string secret, string userId, string connectId)
        {
            if (String.IsNullOrEmpty(appkey))
            {
                throw new Exception("appkey is null");
            }
            if (String.IsNullOrEmpty(secret))
            {
                throw new Exception("secret is null");
            }
            if (!String.IsNullOrEmpty(userId))
            {
                long u;
                if (!long.TryParse(userId, out u))
                {
                    throw new Exception("userid must a number type");
                }
            }
            else
            {
                userId = "-1";
            }

            ConnectId = String.IsNullOrEmpty(connectId) ? GetDefaultConnectId() : connectId;
            Appkey = appkey;
            Secret = secret;
            UserId = userId;
        }

        public string Appkey { get; private set; }

        public string Secret { get; private set; }

        public string UserId { get; private set; }

        public string ConnectId { get; private set; }

        public ICometConnection Connection { get; set; }

        public ICometMessageProcesser Processer { get; set; }

        public IDictionary<string, string> OtherParam { get; set; }

        private static string GetDefaultConnectId()
        {
            try
            {
                string strHostName = Dns.GetHostName(); //得到本机的主机名 
                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName); //取得本机IP  
                return ipEntry.AddressList[0].ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}