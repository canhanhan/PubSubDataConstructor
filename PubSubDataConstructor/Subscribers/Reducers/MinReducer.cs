using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class MinReducer : IReducer
    {
        public IEnumerable<DataCandidate> Reduce(IEnumerable<DataCandidate> candidates)
        {
            return candidates.OrderBy(x => x.Value);
        }
    }
}
