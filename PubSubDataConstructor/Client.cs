using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor
{
    public class Client
    {
        protected readonly IChannel channel;              

        public Client(IChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            
            this.channel = channel;
            this.channel.OnConnect += channel_OnConnect;
            this.channel.OnDisconnect += channel_OnDisconnect;
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
