using System;
using System.Linq;

namespace PubSubDataConstructor.Reducers
{
    public class MinReducer : IReducer
    {
        public DataCandidate[] Reduce(params DataCandidate[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            return values.OrderBy(x => x.Value).ToArray();
        }
    }
}
