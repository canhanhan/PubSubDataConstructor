using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IPublisher : IChannelClient
    {
        event EventHandler<DataCandidateEventArgs> OnPublished;
        event EventHandler<DataCandidateEventArgs> OnQueued;

        bool IsSuspended { get; }

        void Suspend();
        void Resume();
        void Publish(DataCandidate candidate);
        void Publish(IEnumerable<DataCandidate> candidates);
    }
}
