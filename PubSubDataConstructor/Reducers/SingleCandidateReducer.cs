using System;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public abstract class SingleCandidateReducer : IReducer
    {
        public DataCandidate[] Reduce(params DataCandidate[] candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            if (candidates.Length == 0)
                return null;

            var firstCandidate = candidates.FirstOrDefault(x => x.Value != null);
            if (firstCandidate == null)
                return null;

            var valueType = firstCandidate.Value.GetType();
            var value = CalculateValue(valueType, candidates);

            return new [] { new DataCandidate
            {
                Freshness = candidates.Max(x => x.Freshness),
                Priority = candidates.Max(x => x.Priority),
                SourceField = "Merged",
                SourceId = "Merged",
                TargetId = firstCandidate.TargetId,
                TargetField = firstCandidate.TargetField,
                Value = value
            } };
        }

        protected abstract object CalculateValue(Type valueType, DataCandidate[] candidates);
    }
}
