using System;

namespace XamarinSignalRExample
{
    public class ChatHub : IHubProxy
    {
        private string s;

        public ChatHub(string s)
        {
            this.s = s;
        }

        public void Invoke(string v, string username, string text)
        {
            //throw new NotImplementedException();
        }

        public void On<T1, T2>(T2 v, Action<string, string> value)
        {
            //throw new NotImplementedException();
        }
    }
}