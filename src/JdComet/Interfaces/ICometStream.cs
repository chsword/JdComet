using JdComet.Message;

namespace JdComet
{
    public interface ICometManager
    {
        ICometConnection CometConnection { set; }
        ICometMessageProcesser CometMessageProcesser { set; }
        void Start();
        void Stop();
    }
}
