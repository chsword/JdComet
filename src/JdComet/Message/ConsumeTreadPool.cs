using System;
using System.Threading;

namespace JdComet.Message
{
    public class ConsumeTreadPool
    {
        private int _minThreads;
        private int _maxThreads;
        private int _queueSize;

        public ConsumeTreadPool(int minThreads, int maxThreads, int queueSize)
        {
            if (minThreads <= 0 || maxThreads <= 0 || queueSize <= 0)
            {
                throw new Exception("minThread,maxThread and queueSize must large than 0");
            }
            this._minThreads = minThreads;
            this._maxThreads = maxThreads;
            this._queueSize = queueSize;

            ThreadPool.SetMinThreads(this._minThreads, this._minThreads);
            ThreadPool.SetMaxThreads(this._maxThreads, this._maxThreads);
        }

        public void Consume(WaitCallback callback)
        {
            ThreadPool.QueueUserWorkItem(callback);
        }

        public void Shutdown()
        {
        }
    }
}
