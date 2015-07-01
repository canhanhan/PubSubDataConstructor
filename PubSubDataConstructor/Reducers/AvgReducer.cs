using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class AvgReducer : MergeReducer
    {
        protected override object CalculateValue(Type valueType, IEnumerable<DataCandidate> candidates)
        {
            return candidates.Average(x => (int)x.Value);
        }
    }
}
