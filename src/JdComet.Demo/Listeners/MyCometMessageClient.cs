using System;
using System.Collections.Generic;

namespace JdComet.Demo.Listeners
{
    public class MyCometMessageClient
    {
     
        static MyCometMessageClient()
        {
            Pool = new Dictionary<string, ICometManager>();
        }
        static public Dictionary<string, ICometManager> Pool { get; set; }

        public static void Start(string appkey, string secret)
        {
            if (!Pool.ContainsKey(appkey))
            {
                var stream = GetTopCometStream(appkey, secret);
                Pool.Add(appkey, stream);
            }
            else
            {
                try
                {
                    if (Pool[appkey] != null)
                    {
                        Pool[appkey].Stop();
                    }
                }
                catch (Exception ex)
                {
                    //bus.Send(new HeartBeatMessage()
                    //             {
                    //                 Key = appkey + "stop",
                    //                 Time = DateTime.Now,
                    //                 Value = ex.Message
                    //             });
                }
                try
                {
                    var stream = GetTopCometStream(appkey, secret);
                    Pool[appkey] = stream;
                }
                catch (Exception ex)
                {
                    //bus.Send(new HeartBeatMessage()
                    //{
                    //    Key = appkey + "start",
                    //    Time = DateTime.Now,
                    //    Value = ex.Message
                    //});
                }
            }

        }

        private static ICometManager GetTopCometStream(string appkey, string secret)
        {
            var config = new Configuration(appkey, secret, null);

            ICometManager manager = new CometManager(config);
            manager.CometConnection = new MyCometConnection(appkey, secret);
            manager.CometMessageProcesser = new MyCometMessageProcesser(appkey, secret);
            manager.Start();
            
            return manager;
        }
    }
}