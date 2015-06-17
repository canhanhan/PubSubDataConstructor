using System;

namespace PubSubDataConstructor
{
    public interface IReducer
    {
        DataCandidate[] Reduce(params DataCandidate[] candidates);
    }
}
