using System;
using System.Collections.Generic;

namespace JdComet
{
    public class Configuration
    {
        public Configuration(string appkey, string secret, string userid)
        {
            QueueSize = 50000;
            MaxThreads = 200;
            MinThreads = 100;
            SleepTimeOfServerInUpgrade = 5*60;
            HttpReconnectInterval = 86100;
            HttpConnectRetryInterval = 16;
            HttpConnectRetryCount = 3;
            HttpReadTimeout = 60 + 30;
            HttpConnectionTimeout = 5;
            var cometReq = new JdCometUnitClient(appkey, secret, userid, null);
            CometClients = new List<JdCometUnitClient>(1) {cometReq};
        }



        internal Configuration(string appkey, string secret, string userid, string connectId)
        {
            QueueSize = 50000;
            MaxThreads = 200;
            MinThreads = 100;
            SleepTimeOfServerInUpgrade = 5*60;
            HttpReconnectInterval = 86100;
            HttpConnectRetryInterval = 16;
            HttpConnectRetryCount = 3;
            HttpReadTimeout = 60 + 30;
            HttpConnectionTimeout = 5;
            var cometUnitClient = new JdCometUnitClient(appkey, secret, userid, connectId);
            CometClients = new List<JdCometUnitClient>(1);
            CometClients.Add(cometUnitClient);
        }


         internal Configuration(List<JdCometUnitClient> cometRequest)
        {
            QueueSize = 50000;
            MaxThreads = 200;
            MinThreads = 100;
            SleepTimeOfServerInUpgrade = 5*60;
            HttpReconnectInterval = 86100;
            HttpConnectRetryInterval = 16;
            HttpConnectRetryCount = 3;
            HttpReadTimeout = 60 + 30;
            HttpConnectionTimeout = 5;
            if (cometRequest == null || (cometRequest != null && cometRequest.Count == 0))
            {
                throw new Exception("comet request param is null");
            }
            CometClients = cometRequest;
        }

        internal List<JdCometUnitClient> CometClients { get; private set; }

        //http connection config

        public int HttpConnectionTimeout { get; set; }

        public int HttpReadTimeout { get; set; }

        public int HttpConnectRetryCount { get; set; }


        public string ConnectUrl
        {
            get { return Constants.StreamConnectUrl; }
        }

        public int HttpConnectRetryInterval { get; set; }

        public int SleepTimeOfServerInUpgrade { get; set; }


        public int HttpReconnectInterval { get; set; }

        public IDictionary<string, string> RequestHeader { get; set; }

        public int MinThreads { get; set; }

        public int MaxThreads { get; set; }

        public int QueueSize { get; set; }
    }
}