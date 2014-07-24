using System;

namespace JdComet
{
    public interface ICometReader
    {
        bool IsAlive();
        void NextMessage();
        string ParseLine(string msg);
        void OnException(Exception ex);
        void Close();
    }
}
