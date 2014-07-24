namespace JdComet
{
    /// <summary>
    /// 日志打点接口。
    /// </summary>
    public interface ILogger
    {
        void Error(string message);
        void Warn(string message);
        void Info(string message);
    }
}
