using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public abstract class MergeReducer : IReducer
    {
        public IEnumerable<DataCandidate> Reduce(IEnumerable<DataCandidate> candidates)
        {
            return candidates
                .Where(x => x.Value != null)
                .GroupBy(x => x.Priority)
                .Select(priortyGroup =>
                {
                    var firstCandidate = priortyGroup.First();
                    var valueType = firstCandidate.Value.GetType();
                    var value = CalculateValue(valueType, priortyGroup);

                    return new DataCandidate
                    {
                        Freshness = priortyGroup.Max(x => x.Freshness),
                        Priority = priortyGroup.Key,
                        SourceField = "Merged",
                        SourceId = "Merged",
                        TargetId = firstCandidate.TargetId,
                        TargetField = firstCandidate.TargetField,
                        Value = value
                    };
                }).ToArray();
        }

        protected abstract object CalculateValue(Type valueType, IEnumerable<DataCandidate> candidates);
    }    
}
