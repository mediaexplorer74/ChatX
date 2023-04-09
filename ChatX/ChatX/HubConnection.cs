using System;
using System.Threading.Tasks;

namespace XamarinSignalRExample
{
    public class HubConnection
    {
        internal ConnectionState State;
        private string url;

        public HubConnection(string url)
        {
            this.url = url;
        }

        public Action<StateChange> StateChanged { get; set; }

        public IHubProxy CreateHubProxy(string v)
        {
            //throw new NotImplementedException();
            return default;
        }

        public Task Start()
        {
            //throw new NotImplementedException();
            return default;
        }
    }
}