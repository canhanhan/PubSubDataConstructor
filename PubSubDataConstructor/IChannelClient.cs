using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IChannelClient
    {
        event EventHandler OnConnect;
        event EventHandler OnDisconnect;

        bool IsConnected { get; }        
        IEnumerable<IFilter> Filters { get; }

        void Connect(IChannel channel);
        void Disconnect();

        void AddFilter(IFilter filter);
        void RemoveFilter(IFilter filter);
    }
}
