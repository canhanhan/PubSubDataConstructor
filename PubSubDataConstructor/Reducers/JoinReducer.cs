using System;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class JoinReducer : SingleCandidateReducer
    {
        protected override object CalculateValue(Type valueType, DataCandidate[] candidates)
        {
            return TypeHelper.CombineArray(valueType, candidates.Select(x => x.Value));
        }
    }
}
