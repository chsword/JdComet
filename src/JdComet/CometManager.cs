using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JdComet.Message;
using JdComet.Util;

namespace JdComet
{
    public class CometManager : ICometManager
    {
        private readonly Configuration _config;

        private readonly List<Thread> _controlThreads = new List<Thread>();
        private readonly ILogger _logger = new DefaultLogger();
        private readonly object _objLock = new object();
        public bool AllStop = false;
        public bool Bstop = false;

        /*
         * 各种状态
         */
        public ICometReader CurrentCometReader;
        public string ServerRespCode = PipelineConstants.ClientFirstConnect;
        private bool _closed;
        private ICometConnection _cometConnectionListener;
        private ICometMessageProcesser _cometMessageProcesser;
        private bool _isReconnect; //是否客户端发起重连
        private long _lastStartConsumeThread = DateTime.Now.Ticks;
        private ConsumeTreadPool _messsageConsumeTreadPool;
        private int _startConsumeThreadTimes;

        public CometManager(Configuration config)
        {
            _config = config;
        }

        public ICometConnection CometConnection
        {
            set { _cometConnectionListener = value; }
        }

        public ICometMessageProcesser CometMessageProcesser
        {
            set { _cometMessageProcesser = value; }
        }

        public void Start()
        {
            if (_cometMessageProcesser == null)
            {
                throw new Exception("Comet message listener must not null");
            }

            List<JdCometUnitClient> cometRequests = _config.CometClients;

            _messsageConsumeTreadPool = new ConsumeTreadPool(_config.MinThreads, _config.MaxThreads,
                _config.QueueSize);

            foreach (JdCometUnitClient request in cometRequests)
            {
                try
                {
                    JdCometUnitClient cometRequest = request;
                    if (cometRequest.Connection == null)
                    {
                        cometRequest.Connection = _cometConnectionListener;
                    }
                    if (cometRequest.Processer == null)
                    {
                        cometRequest.Processer = _cometMessageProcesser;
                    }
                    var controlThread =
                        new Thread(
                            () => ControlThread(cometRequest, _config, ref Bstop))
                        {
                            Name = "stream-control-thread-connectid-" + cometRequest.ConnectId
                        };
                    controlThread.Start();
                    _controlThreads.Add(controlThread);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }
        }

        public void Stop()
        {
            AllStop = true;
            try
            {
                Monitor.Enter(_objLock);
                Monitor.PulseAll(_objLock);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            finally
            {
                Monitor.Exit(_objLock);
            }
        }

        private void ControlThread(JdCometUnitClient client, Configuration config, ref bool bstop)
        {
            while (!bstop)
            {
                if (AllStop)
                {
                    break;
                }

                try
                {
                    if (PipelineConstants.ServerDeploy.Equals(ServerRespCode))
                    {
                        // 服务端在发布
                        _logger.Info("Server is upgrade sleep " + config.SleepTimeOfServerInUpgrade + " seconds");
                        Thread.Sleep(config.SleepTimeOfServerInUpgrade*1000);
                        StartConsumeThread(client);
                    }
                    else if ( /*客户端第一次发起连接请求*/
                        PipelineConstants.ClientFirstConnect.Equals(ServerRespCode) ||
                        /*服务端主动断开了所有的连接*/
                        PipelineConstants.ServerRehash.Equals(ServerRespCode) ||
                        /*连接到达最大时间*/
                        PipelineConstants.ConnectReachMaxTime.Equals(ServerRespCode) ||
                        /*在一些异常情况下需要重连*/
                        PipelineConstants.Reconnect.Equals(ServerRespCode))
                    {
                        StartConsumeThread(client);
                    }
                    else if ( /*客户端自己把自己踢开*/
                        PipelineConstants.ClientKickoff.Equals(ServerRespCode) ||
                        /*服务端把客户端踢开*/
                        PipelineConstants.ServerKickoff.Equals(ServerRespCode))
                    {
                        if ((PipelineConstants.ClientKickoff.Equals(ServerRespCode) && !_isReconnect) ||
                            PipelineConstants.ServerKickoff.Equals(ServerRespCode))
                        {
                            break; // 终止掉当前线程
                        }
                    }
                    else
                    {
                        //错误码设置出错，停止线程
                        bstop = true;
                        break;
                    }
                    //连接成功，开始休眠
                    try
                    {
                        Monitor.Enter(_objLock);
                        {
                            long lastSleepTime = DateTime.Now.Ticks;

                            Monitor.Wait(_objLock, config.HttpReconnectInterval*1000);
                            if (DateTime.Now.Ticks - lastSleepTime >= (config.HttpReconnectInterval)*1000*10000)
                            {
                                /*
                                 * 快要到达连接的最大时间了，需要重新发起连接
                                 */
                                ServerRespCode = PipelineConstants.Reconnect;
                                _isReconnect = true;
                            } //否则，是由于某种原因被唤醒的
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message);
                    }
                    finally
                    {
                        Monitor.Exit(_objLock);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Occur some error,stop the stream consume" + e.Message);
                    bstop = true;
                    try
                    {
                        Monitor.Enter(_objLock);
                        Monitor.PulseAll(_objLock);
                    }
                    finally
                    {
                        Monitor.Exit(_objLock);
                    }
                }
            }
            if (CurrentCometReader != null)
            {
                try
                {
                    CurrentCometReader.Close();
                }
                catch (Exception ex)
                {
                    // ignore
                    _logger.Error(ex.ToString());
                }
            }
            _logger.Info("Stop stream consume");
        }

        private void StartConsumeThread(JdCometUnitClient cometRequest)
        {
            ICometReader cometReader = null;
            try
            {
                cometReader = GetMessageReader(cometRequest);
                if (cometRequest.Connection != null)
                {
                    cometRequest.Connection.OnConnect();
                }
            }
            catch (JdCometException e)
            {
                Bstop = true;
                _logger.Error(e.Message);
                if (cometRequest.Connection != null)
                {
                    cometRequest.Connection.OnSysErrorException(e);
                }
            }
            catch (Exception ex)
            {
                Bstop = true;
                _logger.Error(ex.Message);
                if (cometRequest.Connection != null)
                {
                    cometRequest.Connection.OnConnectError(ex);
                }
            }

            _lastStartConsumeThread = DateTime.Now.Ticks;

            var consumeThread =
                new Thread(
                    () =>
                        StreamConsume(cometReader, _lastStartConsumeThread, ref Bstop))
                {
                    Name = "top-stream-consume-thread" + cometRequest.ConnectId
                };
            consumeThread.Start();
        }

        internal ICometReader GetMessageReader(JdCometUnitClient client)
        {
            if (client != null)
            {
                client.Connection.OnBeforeConnect();
            }
            else
            {
                _logger.Error("client is null");
                throw new JdCometException("client is null");
            }

            var param = new StringDictionary {{PipelineConstants.ParamAppkey, client.Appkey}};
            if (!String.IsNullOrEmpty(client.UserId))
            {
                param.Add(PipelineConstants.ParamUserid, client.UserId);
            }
            if (!String.IsNullOrEmpty(client.ConnectId))
            {
                param.Add(PipelineConstants.ParamConnectId, client.ConnectId);
            }
            param.Add(PipelineConstants.ParamTimestamp, DateTime.Now.Ticks);

            IDictionary<string, string> otherParam = client.OtherParam;
            if (otherParam != null && otherParam.Count > 0)
            {
                IEnumerator<KeyValuePair<string, string>> kvps = otherParam.GetEnumerator();
                while (kvps.MoveNext())
                {
                    param.Add(kvps.Current.Key, kvps.Current.Value);
                }
            }

            string sign;
            try
            {
                sign = TopUtils.SignTopRequest(param, client.Secret, true);
                if (String.IsNullOrEmpty(sign))
                {
                    throw new Exception("Get sign error");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw;
            }
            param.Add(PipelineConstants.ParamSign, sign);
            var httpClient = new HttpClient(_config, param);
            HttpResponse response = httpClient.Post();
            return
                CurrentCometReader =
                    new MessageCometReader(_messsageConsumeTreadPool, response, _cometMessageProcesser, this);
        }

        private void StreamConsume(ICometReader reader, long lastStartConsumeThread, ref bool bstop)
        {
            _startConsumeThreadTimes = 0;
            if (reader == null)
            {
                _logger.Error("reader is null");
                throw new JdCometException("reader is null");
            }

            while (reader != null && (!AllStop && !_closed && reader.IsAlive()))
            {
                try
                {
                    reader.NextMessage();
                }
                catch (Exception)
                {
//出现了read time out异常
                    // 资源清理

                    try
                    {
                        reader.Close();
                    }
                    catch (Exception e1)
                    {
                        _logger.Error(e1.Message);
                    }

                    reader = null;
                    _closed = true;
                    //通知
                    if (_cometConnectionListener != null)
                    {
                        try
                        {
                            _cometConnectionListener.OnReadTimeout();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message);
                        }
                    }
                    /**
                     * 30分钟内发送了10次IOException
                     */
                    if (DateTime.Now.Ticks - lastStartConsumeThread < 30*60*1000*1000)
                    {
                        // 短时间内由于读取IOException连接了10次，则退出
                        _startConsumeThreadTimes++;
                        if (_startConsumeThreadTimes >= 10)
                        {
                            bstop = true;
                            if (_cometConnectionListener != null)
                            {
                                try
                                {
                                    _cometConnectionListener.OnMaxReadTimeoutException();
                                }
                                catch (Exception maxE)
                                {
                                    _logger.Error(maxE.Message);
                                }
                            }
                            _logger.Error("Occure too many exception,stop the system,please check");
                            //通知唤醒控制线程，但是不在发起重连接
                            try
                            {
                                Monitor.Enter(_objLock);
                                Monitor.PulseAll(_objLock);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.ToString());
                            }
                            finally
                            {
                                Monitor.Exit(_objLock);
                            }
                        }
                        else
                        {
                            //没有到达10次，通知重连
                            _startConsumeThreadTimes = 0;
                            ServerRespCode = PipelineConstants.Reconnect;
                            try
                            {
                                Monitor.Enter(_objLock);
                                Monitor.PulseAll(_objLock);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.ToString());
                            }
                            finally
                            {
                                Monitor.Exit(_objLock);
                            }
                        }
                    }
                    else
                    {
                        // 通知重连
                        Trace.WriteLine(" 通知重连" + DateTime.Now);
                        _startConsumeThreadTimes = 0;
                        ServerRespCode = PipelineConstants.Reconnect;

                        try
                        {
                            Monitor.Enter(_objLock);
                            Trace.WriteLine(" PulseAll" + DateTime.Now);
                            Monitor.PulseAll(_objLock);
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.ToString());
                        }
                        finally
                        {
                            Monitor.Exit(_objLock);
                        }
                    }
                }
            }
            //出现异常情况下做资源清理
            if (reader != null)
            {
                try
                {
                    reader.Close();
                }
                catch (Exception e)
                {
                    _logger.Warn(e.Message);
                }
            }
        }

        public object GetControlLock()
        {
            return _objLock;
        }

        public void SetServerRespCode(string serverRespCode)
        {
            ServerRespCode = serverRespCode;
        }
    }
}