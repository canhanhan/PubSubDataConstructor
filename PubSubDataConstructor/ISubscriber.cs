using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface ISubscriber
    {
        IEnumerable<IFilter> Filters { get; }

        void AddFilter(IFilter filter);
        void RemoveFilter(IFilter filter);

        IEnumerable<DataCandidate> Poll(string topic);
        void Subscribe(string topic, Action<DataCandidate> callback);
        void Unsubscribe(string topic);
    }
}
