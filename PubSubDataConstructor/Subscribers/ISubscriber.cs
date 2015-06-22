using PubSubDataConstructor.Subscribers.Repositories;
using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface ISubscriber : IPublisher
    {
        IEnumerable<IFilter> Filters { get; }
        IRepository Repository { get; }

        void AddFilter(IFilter filter);
        void RemoveFilter(IFilter filter);

        IEnumerable<DataCandidate> Poll(Topic topic);
        void Subscribe(Topic topic, Action<DataCandidate> callback);
        void Unsubscribe(Topic topic);
    }
}
