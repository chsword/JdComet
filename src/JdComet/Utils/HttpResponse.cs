using System.IO;
using System.Net;
using System.Text;

namespace JdComet
{
     class HttpResponse
    {
        private readonly StreamReader _reader;
        private readonly HttpWebRequest _request;
        private readonly HttpWebResponse _response;
        private readonly System.IO.Stream _stream;

        public HttpResponse(HttpWebRequest request)
        {
            _request = request;
            _response = (HttpWebResponse) request.GetResponse();
            _stream = _response.GetResponseStream();
            if (_stream != null) _reader = new StreamReader(_stream, Encoding.UTF8);
        }

        public string GetResponseHeader(string name)
        {
            return _request.Headers.Get(name);
        }

        public string GetMsg()
        {
            return _reader.ReadLine();
        }

        public void Close()
        {
            // 释放资源
            if (_reader != null) _reader.Close();
            if (_stream != null) _stream.Close();
            if (_response != null) _response.Close();
        }
    }
}