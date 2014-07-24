using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace JdComet.Message
{
    class MessageCometReader : BaseCometReader
    {
        private readonly ILogger _logger = new DefaultLogger();
        private readonly ICometMessageProcesser _msg;
        private readonly CometManager _cometManager;
        private const string Pattern = "\\{\"packet\":\\{\"code\":(?<code>(\\d+))(,\"msg\":(?<msg>((.+))))?\\}\\}";
        private readonly object _objLock = new object();

        public MessageCometReader(ConsumeTreadPool messsageConsumeTreadPool,
                HttpResponse response, ICometMessageProcesser msg, CometManager cometManager)
            : base(messsageConsumeTreadPool, response)
        {
             _msg = msg;
             _objLock = cometManager.GetControlLock();
             _cometManager = cometManager;
        }

        public override string ParseLine(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                try
                {
                    Regex rg = new Regex(Pattern, RegexOptions.Compiled);
                    MatchCollection matches = rg.Matches(msg);

                    if (matches.Count > 0)
                    {
                        string code = matches[0].Groups["code"].Value;

                        if (PipelineConstants.NewMessage.Equals(code))
                        {
                            return matches[0].Groups["msg"].Value; 
                        }
                        else if (PipelineConstants.HeartBeat.Equals(code))
                        {
                            _msg.OnHeartBeat();
                        }
                        else if (PipelineConstants.ConnectReachMaxTime.Equals(code))
                        {
                            _msg.OnConnectReachMaxTime();
                            WakeUp(code);
                        }
                        else if (PipelineConstants.DiscardMessage.Equals(code))
                        {
                            _msg.OnDiscardMsg(matches[0].Groups["msg"].Value.ToString());
                        }
                        else if (PipelineConstants.ServerDeploy.Equals(code))
                        {
                            _msg.OnServerUpgrade(matches[0].Groups["msg"].Value.ToString());
                            WakeUp(code);
                        }
                        else if (PipelineConstants.ServerRehash.Equals(code))
                        {
                            _msg.OnServerRehash();
                            WakeUp(code);
                        }
                        else if (PipelineConstants.ClientKickoff.Equals(code))
                        {
                            _msg.OnClientKickOff();
                            WakeUp(code);
                        }
                        else if (PipelineConstants.ServerKickoff.Equals(code))
                        {
                            _msg.OnServerKickOff();
                            WakeUp(code);
                        }
                        else if (PipelineConstants.ConnectSuccess.Equals(code))
                        {
                            _msg.OnConnectMsg(matches[0].Groups["msg"].Value);
                        }
                        else
                        {
                            _msg.OnOtherMsg(matches[0].Groups["msg"].Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Message is invalid:" + msg + e.Message);
                    _msg.OnException(e);
                    return null;
                }
            }
            return null;
        }

        private void WakeUp(string code)
        {
            try
            {
                Monitor.Enter(_objLock);
                _cometManager.SetServerRespCode(code);
                Monitor.PulseAll(_objLock);
            }
            catch (Exception e)
            {
                //ignore
            }
            finally
            {
                Monitor.Exit(_objLock);
            }
        }

        public override ICometMessageProcesser MessageProcesser
        {
            get { return _msg; }
        }

        public override void OnException(Exception ex)
        {
            _logger.Error(ex.Message);
        }

        public override void Close()
        {
            StreamAlive = false;
        }
    }
}
