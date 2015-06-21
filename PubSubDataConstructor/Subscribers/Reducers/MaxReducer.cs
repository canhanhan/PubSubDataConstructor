using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class MaxReducer : IReducer
    {
        public IEnumerable<DataCandidate> Reduce(IEnumerable<DataCandidate> values)
        {
            return values.OrderByDescending(x => x.Value);
        }
    }
}
