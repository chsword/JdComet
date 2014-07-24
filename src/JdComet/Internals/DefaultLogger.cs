using System;
using System.Diagnostics;

namespace JdComet
{
    /// <summary>
    /// 日志
    /// </summary>
    public class DefaultLogger : ILogger
    {
        public const string LogFileName = "comet.log";
        public const string DatetimeFormat = "yyyy-MM-dd HH:mm:ss";

        static DefaultLogger()
        {
            try
            {
                Trace.Listeners.Add(new TextWriterTraceListener(LogFileName));
            }
            catch (Exception e)
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }
            Trace.AutoFlush = true;
        }

        public void Error(string message)
        {
            Trace.WriteLine(message, DateTime.Now.ToString(DatetimeFormat) + " ERROR");
        }

        public void Warn(string message)
        {
            Trace.WriteLine(message, DateTime.Now.ToString(DatetimeFormat) + " WARN");
        }

        public void Info(string message)
        {
            Trace.WriteLine(message, DateTime.Now.ToString(DatetimeFormat) + " INFO");
        }
    }
}
