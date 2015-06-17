using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface ISubscriber : IChannelClient
    {
        event EventHandler<DataCandidateEventArgs> OnReceived;
        event EventHandler<DataEventArgs> OnConstructed;
    }
}
