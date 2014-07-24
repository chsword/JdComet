using System;

namespace JdComet.Message
{
    abstract class BaseCometReader : ICometReader
    {
        protected HttpResponse Response;
        private readonly ConsumeTreadPool _messsageConsumeTreadPool;
        protected bool StreamAlive = true;

        public BaseCometReader(ConsumeTreadPool messsageConsumeTreadPool, HttpResponse response)
        {
            this._messsageConsumeTreadPool = messsageConsumeTreadPool;
            this.Response = response;
        }

        public void NextMessage()
        {
            if (!StreamAlive)
            {
                throw new JdCometException("Stream closed");
            }
            try
            {
                string line = Response.GetMsg();

                if (string.IsNullOrEmpty(line))
                {//正常读到流的末尾了。
                    StreamAlive = false;
                    Response.Close();
                    return;
                }
                if (!string.IsNullOrEmpty(line))
                {
                    _messsageConsumeTreadPool.Consume(delegate(object obj)
                        {
                            string parseString = ParseLine(line);
                            if (!string.IsNullOrEmpty(parseString))
                            {
                                MessageProcesser.OnReceiveMsg(parseString);
                            }
                        });
                }

            }
            catch (Exception e)
            {
                Response.Close();
                StreamAlive = false;
                throw e;
            }
        }

        public bool IsAlive()
        {
            return StreamAlive;
        }

        public abstract ICometMessageProcesser MessageProcesser{get;}
        public abstract string ParseLine(string msg);
        public abstract void OnException(Exception ex);
        public abstract void Close();

    }
}
