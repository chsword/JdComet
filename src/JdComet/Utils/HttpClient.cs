using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using JdComet.Util;

namespace JdComet
{
     class HttpClient
    {
        private readonly ILogger _log = new DefaultLogger();
        private readonly IDictionary<string, string> _parameters;
        private readonly Configuration _config;

        public HttpClient(Configuration config, IDictionary<string, string> parameters)
        {
            if (config == null || parameters == null)
            {
                throw new Exception("conf and params is must not null");
            }
            this._config = config;
            this._parameters = parameters;
            System.Net.ServicePointManager.DefaultConnectionLimit = 128;
        }

        public HttpResponse Post()
        {

            int retriedCount;
            int retry = _config.HttpConnectRetryCount + 1;
            HttpResponse resp = null;
            for (retriedCount = 1; retriedCount <= retry; retriedCount++)
            {
                try
                {
                    HttpWebRequest con = null;
                    System.IO.Stream outStream = null;
                    try
                    {
                        con = GetHttpRequest(_config.ConnectUrl, _config.HttpConnectionTimeout, _config.HttpReadTimeout);
                        con.KeepAlive = true;

                        SetHeaders(con, _config.RequestHeader);
                        con.Method = "POST";
                        con.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

                        string postParam = WebUtils.BuildQuery(_parameters);
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postParam);

                        outStream = con.GetRequestStream();
                        outStream.Write(bytes, 0, bytes.Length);
                        outStream.Close();

                        var response = (HttpWebResponse)con.GetResponse();
                        var responseCode = response.StatusCode;

                        if (HttpStatusCode.OK == responseCode)
                        {
                            _log.Info("connect successful");

                            StringBuilder respHeader = new StringBuilder();
                            WebHeaderCollection responseHeaders = con.Headers;

                            foreach (string key in responseHeaders.AllKeys)
                            {
                                string[] values = responseHeaders.GetValues(key);

                                foreach (string value in values)
                                {
                                    if (key != null)
                                    {
                                        respHeader.Append(key).Append("=").Append(value);
                                    }
                                    else
                                    {
                                        respHeader.Append(value);
                                    }
                                    respHeader.Append(";");
                                }

                                _log.Info("Response: " + respHeader.ToString());
                            }
                            resp = new HttpResponse(con);
                            return resp;
                        }
                        else if (HttpStatusCode.BadRequest == responseCode)
                        {   //参数校验出错
                            _log.Info("Request param is invalid,errmsg is:" + con.Headers.Get(PipelineConstants.ErrMsgHeader));
                            throw new JdCometException("Server response err msg:" + con.Headers.Get(PipelineConstants.ErrMsgHeader));
                        }
                        else if (HttpStatusCode.Forbidden == responseCode)
                        {//服务端在发布，需要休眠一段时间

                            _log.Info("Server is deploying,sleep " + retriedCount * _config.HttpConnectRetryInterval + " seconds");
                            if (retriedCount == _config.HttpConnectRetryCount)
                            {
                                _log.Info("May be server occure some error,please contact top tech support");
                                throw new JdCometException("May be server occure some error,please contact top tech support");
                            }
                            try
                            {
                                Thread.Sleep(retriedCount * _config.HttpConnectRetryInterval * 1000);
                            }
                            catch (Exception e)
                            {
                                //ignore;
                            }
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message);                       
                    }
                    finally
                    {
                        try
                        {
                            if (outStream != null)
                            {
                                outStream.Close();
                            }
                        }
                        catch (Exception ignore)
                        {
                        }
                    }
                }
                catch (Exception ioe)
                {
                    // connection timeout or read timeout
                    if (retriedCount == _config.HttpConnectRetryCount)
                    {
                        throw new JdCometException(ioe.Message);
                    }
                }
                try
                {
                    _log.Info("Sleeping " + _config.HttpConnectRetryInterval + " seconds until the next retry.");
                    Thread.Sleep(retriedCount * _config.HttpConnectRetryInterval * 1000);
                }
                catch (Exception ignore)
                {
                    //nothing to do
                }
            }
            return resp;
        }

        private HttpWebRequest GetHttpRequest(string url, int connTimeout, int readTimeout)
        {
            var con = (HttpWebRequest)WebRequest.Create(url);
            if (connTimeout > 0)
            {
                con.Timeout = connTimeout * 1000;
            }

            if (readTimeout > 0)
            {
                con.ReadWriteTimeout = readTimeout * 1000;
            }

            con.AllowAutoRedirect = false;
            con.ServicePoint.Expect100Continue = false;

            return con;
        }

        /**
         * sets HTTP headers
         *
         * @param connection HttpURLConnection
         * @param reqHeader 
         */
        private void SetHeaders(HttpWebRequest connection, IDictionary<string, string> reqHeader)
        {
            if (reqHeader != null && reqHeader.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in reqHeader)
                {
                    connection.Headers.Add(pair.Key, pair.Value);
                }
            }
        }
    }

}
