using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor
{
    public class Client : IChannelClient
    {
        protected readonly IChannel channel;
        protected readonly List<IFilter> filters;

        public IEnumerable<IFilter> Filters { get { return filters; } }
        public bool IsSuspended { get; private set; }

        public Client(IChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            this.filters = new List<IFilter>();
            this.channel = channel;
            this.channel.OnConnect += channel_OnConnect;
            this.channel.OnDisconnect += channel_OnDisconnect;
        }

        public void AddFilter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            filters.Add(filter);
        }

        public void RemoveFilter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            filters.Remove(filter);
        }

        public virtual void Suspend()
        {
            if (IsSuspended)
                return;

            IsSuspended = true;
        }

        public virtual void Resume()
        {
            this.CheckConnection();

            if (!IsSuspended)
                return;

            IsSuspended = false;
        }

        protected void CheckConnection()
        {
            if (!channel.IsConnected)
                channel.Connect();
        }

        protected virtual void OnChannelConnect() {}

        protected virtual void OnChannelDisconnect() {}

        private void channel_OnConnect(object sender, EventArgs e)
        {
            this.OnChannelConnect();
        }

        private void channel_OnDisconnect(object sender, EventArgs e)
        {
            this.OnChannelDisconnect();
        }
    }
}
