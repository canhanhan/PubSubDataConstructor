using PubSubDataConstructor.Repositories;
using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IClient
    {
        event EventHandler<DataCandidateEventArgs> OnPublished;
        event EventHandler<DataCandidateEventArgs> OnQueued;

        bool IsSuspended { get; }

        IRepository Repository { get; }
        IEnumerable<DataCandidate> Poll(Topic topic);

        void Suspend();
        void Resume();

        void Subscribe(Topic topic, Action<DataCandidate> callback);
        void Unsubscribe(Topic topic);
        void Publish(DataCandidate candidate);
        void Publish(IEnumerable<DataCandidate> candidates);
    }
}
