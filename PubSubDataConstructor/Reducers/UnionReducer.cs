using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class UnionReducer : SingleCandidateReducer
    {
        protected override object CalculateValue(Type valueType, DataCandidate[] candidates)
        {
            return TypeHelper.CombineArray(valueType.GetElementType(), candidates.SelectMany(x => (IEnumerable<object>)x.Value));
        }
    }
}
