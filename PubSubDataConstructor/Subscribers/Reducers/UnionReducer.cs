using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class UnionReducer : MergeReducer
    {
        protected override object CalculateValue(Type valueType, IEnumerable<DataCandidate> candidates)
        {
            return TypeHelper.CombineArray(valueType.GetElementType(), candidates.SelectMany(x => (IEnumerable<object>)x.Value).Where(x => x != null));
        }
    }
}
