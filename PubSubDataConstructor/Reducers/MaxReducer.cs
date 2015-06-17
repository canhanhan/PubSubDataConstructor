using System;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class MaxReducer : IReducer
    {
        public DataCandidate[] Reduce(params DataCandidate[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            return values.OrderByDescending(x => x.Value).ToArray();
        }
    }
}
