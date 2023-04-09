using System;

namespace XamarinSignalRExample
{
    public interface IHubProxy
    {
        void Invoke(string v, string username, string text);
        void On<T1, T2>(T2 v, Action<string, string> value);
    }
}