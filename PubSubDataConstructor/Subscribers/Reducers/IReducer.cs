using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IReducer
    {
        IEnumerable<DataCandidate> Reduce(IEnumerable<DataCandidate> candidates);
    }
}
