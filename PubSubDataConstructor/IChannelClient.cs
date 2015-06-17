using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IChannelClient
    {
        IEnumerable<IFilter> Filters { get; }

        void AddFilter(IFilter filter);
        void RemoveFilter(IFilter filter);
    }
}
