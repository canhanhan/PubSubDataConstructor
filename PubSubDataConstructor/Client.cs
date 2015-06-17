using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor
{
    public class Client : IChannelClient
    {
        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        protected IChannel channel;
        protected readonly List<IFilter> filters;

        public IEnumerable<IFilter> Filters { get { return filters; } }
        public bool IsSuspended { get; private set; }
        public bool IsConnected { get { return channel != null && channel.IsConnected; } }

        public Client()
        {
            this.filters = new List<IFilter>();
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
            this.CheckConnection();

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

        public virtual void Connect(IChannel channel)
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected");

            if (channel == null)
                throw new ArgumentNullException("channel");

            this.channel = channel;
            this.channel.OnConnect += channel_OnConnect;
            this.channel.OnDisconnect += channel_OnDisconnect;
            this.channel.Connect();
        }

        public virtual void Disconnect()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Already disconnected");

            this.channel.OnConnect -= channel_OnConnect;
            this.channel.OnDisconnect -= channel_OnDisconnect;
            this.channel.Disconnect();
        }

        protected void CheckConnection()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");
        }

        protected void InvokeOnConnect()
        {
            if (OnConnect != null)
                this.OnConnect.Invoke(this, null);
        }
        
        protected void InvokeOnDisconnect()
        {
            if (OnDisconnect != null)
                this.OnDisconnect.Invoke(this, null);
        }

        protected virtual void OnChannelConnect() 
        {
            this.InvokeOnConnect();
        }

        protected virtual void OnChannelDisconnect()
        {
            this.InvokeOnDisconnect();
        }

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
