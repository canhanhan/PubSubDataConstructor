using System;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class AvgReducer : SingleCandidateReducer
    {
        protected override object CalculateValue(Type valueType, DataCandidate[] candidates)
        {
            return candidates.Average(x => (int)x.Value);
        }
    }
}
