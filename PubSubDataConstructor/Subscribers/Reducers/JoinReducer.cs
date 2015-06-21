using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class JoinReducer : MergeReducer
    {
        protected override object CalculateValue(Type valueType, IEnumerable<DataCandidate> candidates)
        {
            return TypeHelper.CombineArray(valueType, candidates.Select(x => x.Value));
        }
    }
}
